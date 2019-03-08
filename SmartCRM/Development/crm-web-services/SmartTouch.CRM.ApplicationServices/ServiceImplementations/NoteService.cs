using AutoMapper;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using RestSharp.Extensions.MonoHttp;
using SmartTouch.CRM.ApplicationServices.Exceptions;
using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.Notes;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Notes;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.MessageQueues;
using SmartTouch.CRM.SearchEngine.Indexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class NoteService : INoteService
    {
        readonly INoteRepository noteRepository;
        readonly ITagRepository tagRepository;
        readonly IContactRepository contactRepository;
        readonly IUnitOfWork unitOfWork;
        readonly IMessageService messageService;
        readonly IIndexingService indexingService;
        readonly IContactService contactService;
        readonly IAccountService accountService;
        public NoteService(INoteRepository noteRepository, ITagRepository tagRepository,
            IUnitOfWork unitOfWork, IIndexingService indexingService, IMessageService messageService, IContactService contactService, IAccountService accountService,
            IContactRepository contactRepository)
        {
            if (noteRepository == null) throw new ArgumentNullException("noteRepository");
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");

            this.noteRepository = noteRepository;
            this.tagRepository = tagRepository;
            this.unitOfWork = unitOfWork;
            this.indexingService = indexingService;
            this.messageService = messageService;
            this.contactService = contactService;
            this.accountService = accountService;
            this.contactRepository = contactRepository;
        }

        private ResourceNotFoundException GetTagNotFoundException()
        {
            return new ResourceNotFoundException("The requested tag was not found.");
        }

        public SaveNoteResponse InsertNote(SaveNoteRequest request)
        {
            Logger.Current.Verbose("Request for inserting Note");
            Logger.Current.Informational("Note details:" + request.NoteViewModel.NoteDetails);
            Note note = Mapper.Map<NoteViewModel, Note>(request.NoteViewModel);

            isNoteValid(note);
            noteRepository.Insert(note);
            Note newNote = unitOfWork.Commit() as Note;

            foreach (Tag tag in note.Tags.Where(t => t.Id == 0))
            {
                if (tag.Id == 0)
                {
                    Tag savedTag = tagRepository.FindBy(tag.TagName, request.AccountId);
                    indexingService.IndexTag(savedTag);
                    accountService.ScheduleAnalyticsRefresh(savedTag.Id, (byte)IndexType.Tags);
                }
            }

            note.Id = newNote.Id;
            if (request.NoteViewModel.Contacts != null && request.NoteViewModel.Contacts.Count() > 0)
            {
                var noteContactIds = request.NoteViewModel.Contacts.Select(c => c.Id).ToList();
                List<LastTouchedDetails> details = new List<LastTouchedDetails>();
                foreach (var id in noteContactIds)
                {
                    LastTouchedDetails detail = new LastTouchedDetails();
                    detail.ContactID = id;
                    detail.LastTouchedDate = DateTime.UtcNow;
                    details.Add(detail);
                }
                updateLastTouchedInformation(details);
                contactService.ContactIndexing(new ContactIndexingRequest() { ContactIds = noteContactIds, Ids = noteContactIds.ToLookup(o => o, o => { return true; }) });
            }

            if (note.SelectAll == false)
            {
                this.addToTopic(note, request.AccountId);
            }

            return new SaveNoteResponse() { NoteViewModel = new NoteViewModel() { NoteId = newNote.Id } };
        }

        public Note UpdateNoteBulkData(int noteId, int accountId, IEnumerable<int> contactIds)
        {
            Note note = noteRepository.GetNoteById(noteId, accountId);
            Logger.Current.Verbose("Indexing contacts mapped to noteid: " + noteId);

            if (contactIds.IsAny())
            {
                List<LastTouchedDetails> details = new List<LastTouchedDetails>();
                foreach (var id in contactIds)
                {
                    LastTouchedDetails detail = new LastTouchedDetails();
                    detail.ContactID = id;
                    detail.LastTouchedDate = DateTime.UtcNow;
                    details.Add(detail);
                }
                updateLastTouchedInformation(details);
            }
            accountService.InsertIndexingData(new InsertIndexingDataRequest()
            {
                IndexingData = new Domain.Accounts.IndexingData() { EntityIDs = contactIds.ToList(), IndexType = (int)IndexType.Contacts }
            });
            
            Logger.Current.Verbose("Indexing contacts successfully noteid: " + noteId);
            this.addToTopic(note, accountId);
            return note;
        }

        public UpdateNoteResponse UpdateNote(UpdateNoteRequest request)
        {
            Logger.Current.Verbose("Request for updating note");
            Logger.Current.Informational("NoteId :" + request.NoteViewModel.NoteId);

            Note note = Mapper.Map<NoteViewModel, Note>(request.NoteViewModel);
            isNoteValid(note);
            noteRepository.Update(note);
            unitOfWork.Commit();

            foreach (Tag tag in note.Tags.Where(t => t.Id == 0))
            {
                if (tag.Id == 0)
                {
                    Tag savedTag = tagRepository.FindBy(tag.TagName, request.AccountId);
                    indexingService.IndexTag(savedTag);
                    accountService.ScheduleAnalyticsRefresh(savedTag.Id, (byte)IndexType.Tags);
                }
            }

            if (request.NoteViewModel.Contacts != null && request.NoteViewModel.Contacts.Count() > 0)
            {
                var noteContactIds = request.NoteViewModel.Contacts.Select(c => c.Id).ToList();
                List<LastTouchedDetails> details = new List<LastTouchedDetails>();
                foreach (var id in noteContactIds)
                {
                    LastTouchedDetails detail = new LastTouchedDetails();
                    detail.ContactID = id;
                    detail.LastTouchedDate = DateTime.UtcNow;
                    details.Add(detail);
                }
                updateLastTouchedInformation(details);
                contactService.ContactIndexing(new ContactIndexingRequest() { ContactIds = noteContactIds, Ids = noteContactIds.ToLookup(o => o, o => { return true; }) });
            }

            // this.addToMessageQueue(note, request.AccountId);
            this.addToTopic(note, request.AccountId);
            return new UpdateNoteResponse();
        }

        void updateLastTouchedInformation(List<LastTouchedDetails> details)
        {
            if (details.IsAny())
                contactRepository.UpdateLastTouchedInformation(details, AppModules.ContactNotes, null);
        }

        void addToTopic(Note note, int accountId)
        {
            if (note.Tags.Count > 0)
            {
                foreach (var contact in note.Contacts)
                {
                    foreach (var tag in note.Tags.Where(t => t.Id > 0))
                    {
                        //new message for each contact and tag.
                        var message = new TrackMessage()
                        {
                            EntityId = note.Id,
                            AccountId = accountId,
                            ContactId = contact.Id,
                            UserId = note.CreatedBy,
                            LeadScoreConditionType = (int)LeadScoreConditionType.ContactNoteTagAdded,
                            LinkedEntityId = tag.Id
                        };
                        messageService.SendMessages(new Messaging.Messages.SendMessagesRequest()
                        {
                            Message = message
                        });

                    }
                }
            }

            if (note.Contacts.IsAny())
            {
                foreach (var contact in note.Contacts)
                {
                    //new message for each contact with NoteCategory
                    var message = new TrackMessage()
                    {
                        EntityId = note.Id,
                        AccountId = accountId,
                        ContactId = contact.Id,
                        UserId = note.CreatedBy,
                        LeadScoreConditionType = (int)LeadScoreConditionType.ContactNoteCategoryAdded,
                        LinkedEntityId = note.NoteCategory
                    };
                    messageService.SendMessages(new Messaging.Messages.SendMessagesRequest()
                    {
                        Message = message
                    });
                }
            }
        }

        public DeleteNoteResponse DeleteNote(DeleteNoteRequest request)
        {
            Logger.Current.Verbose("Request for delete note");

            Logger.Current.Informational("NoteId :" + request.NoteId);
            var response = new DeleteNoteResponse();
            response.ContactsRelated = noteRepository.DeleteNote(request.NoteId, request.ContactId);
            unitOfWork.Commit();
            //contactService.ContactIndexing(new ContactIndexingRequest() { ContactIds = response.ContactsRelated.ToList() });
            accountService.InsertIndexingData(new InsertIndexingDataRequest()
            {
                IndexingData = new Domain.Accounts.IndexingData() { EntityIDs = response.ContactsRelated.ToList(), IndexType = (int)IndexType.Contacts }
            });
            return response;
        }

        public GetNoteResponse GetNote(GetNoteRequest request)
        {
            Logger.Current.Verbose("request for Note");
            GetNoteResponse response = new GetNoteResponse();

            Logger.Current.Informational("NoteId :" + request.NoteId);
            Logger.Current.Informational("ContactId :" + request.ContactId);
            Note note = noteRepository.FindBy(request.NoteId, request.ContactId, request.AccountId);
            if (note == null)
                response.Exception = GetContactNotFoundException();
            else
            {
                NoteViewModel noteViewModel = Mapper.Map<Note, NoteViewModel>(note);

                response.NoteViewModel = noteViewModel;
            }
            return response;
        }

        public DeactivateNotesContactResponse ContactDeleteForNote(DeactivateNotesContactRequest request)
        {

            Note note = noteRepository.FindNoteById(request.noteId, request.AccountId);
            Contact contact = note.Contacts.SingleOrDefault(c => c.Id == request.ContactId);
            note.Contacts.Remove(contact);

            noteRepository.Update(note);
            unitOfWork.Commit();
            return new DeactivateNotesContactResponse();
        }

        public GetNotesListResponse GetNotesContacts(GetNotesListRequest request)
        {
            GetNotesListResponse response = new GetNotesListResponse();
            IEnumerable<Note> notes = noteRepository.FindByContact(request.Id, request.AccountId);

            if (notes == null)
            {
                response.Exception = GetContactNotFoundException();
            }
            else
            {
                IEnumerable<NoteViewModel> noteslist = Mapper.Map<IEnumerable<Note>, IEnumerable<NoteViewModel>>(notes);
                response.NotesListViewModel = noteslist;
            }
            return response;
        }

        private ResourceNotFoundException GetContactNotFoundException()
        {
            return new ResourceNotFoundException("The requested note was not found.");
        }

        public ReIndexDocumentResponse ReIndexNotes(ReIndexDocumentRequest request)
        {
            Logger.Current.Verbose("Request for reindexing notes.");


            IEnumerable<Note> documents = noteRepository.FindAll();
            int count = indexingService.ReIndexAll<Note>(documents);

            return new ReIndexDocumentResponse() { Documents = count };
        }

        void isNoteValid(Note note)
        {
            IEnumerable<BusinessRule> brokenRules = note.GetBrokenRules();
            Logger.Current.Informational("Broken rules count : " + brokenRules.Count());
            if (brokenRules.Any())
            {
                StringBuilder brokenRulesBuilder = new StringBuilder();
                foreach (BusinessRule rule in brokenRules)
                {
                    brokenRulesBuilder.AppendLine(rule.RuleDescription);
                }
                throw new UnsupportedOperationException(brokenRulesBuilder.ToString());
                // throw new Exception(brokenRulesBuilder.ToString());
            }
        }


        public GetContactCountsResponse NoteContactsCount(GetContactCountsRequest request)
        {
            Logger.Current.Verbose("Request for count of contacts for note :");
            Logger.Current.Informational("NoteId :" + request.NoteId);
            GetContactCountsResponse response = new GetContactCountsResponse();
            response.Count = noteRepository.ContactsCount(request.NoteId);
            response.SelectAll = noteRepository.IsNoteFromSelectAll(request.NoteId, request.AccountId);
            return response;
        }

        public short GetActionDetailsNoteCategoryID(int accountId, short dropdownValueTypeId, byte dropdownFieldTypeId)
        {
           return noteRepository.GetActionDetailsNoteCategoryID(accountId, dropdownValueTypeId, dropdownFieldTypeId);
        }
    }
}
