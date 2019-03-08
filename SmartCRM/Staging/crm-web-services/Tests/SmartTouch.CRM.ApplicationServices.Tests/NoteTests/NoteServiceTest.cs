using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SmartTouch.CRM.Domain.Note;
using SmartTouch.CRM.Domain.Contact;
using SmartTouch.CRM.Domain.Tag;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.ApplicationServices.Messaging.Notes;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.ApplicationServices.Exceptions;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Tests;

using Moq;
using AutoMapper;
using SmartTouch.CRM.Repository.Database;

namespace SmartTouch.CRM.ApplicationServices.Tests.NoteTests
{
    [TestClass]
    public class NoteServiceTest
    {
        public const int NOTE_ID = 1;
        public const int CREATED_BY_ID = 1;

        MockRepository mockRepository;
        INoteService noteService;
        Mock<INoteRepository> mockNoteRepository;
        Mock<ITagRepository> mockTagRepository;
        IUnitOfWork mockUnitofWork;


        [TestInitialize]
        public void Initialize()
        {
            InitializeAutoMapper.Initialize();
            mockRepository = new MockRepository(MockBehavior.Default);
            mockUnitofWork = mockRepository.Create<IUnitOfWork>().Object;
            mockNoteRepository = mockRepository.Create<INoteRepository>();
            mockTagRepository = mockRepository.Create<ITagRepository>();
            noteService = new NoteService(mockNoteRepository.Object, mockTagRepository.Object, mockUnitofWork);
        }

        [TestCleanup]
        public void CleanUp()
        {
        }

        [TestMethod]
        public void GetNotesContacts_GetNotes_Succeed()
        {
            var mockNotes = NoteMockData.GetMockNotes(mockRepository, 10).ToList();
            mockNoteRepository.Setup(c => c.FindByContact(It.IsAny<int>())).Returns(mockNotes);

            GetNotesListResponse response = noteService.GetNotesContacts(new GetNotesListRequest() { Id = NOTE_ID });
            IEnumerable<NoteViewModel> notes = response.NotesListViewModel;
            mockRepository.VerifyAll();
            Assert.AreEqual(mockNotes.Count(), notes.Count());
            Assert.AreEqual(null, response.Exception);
        }

        [TestMethod]
        public void GetNote_ValidNote_Successful()
        {
           
            var mockNotes = NoteMockData.CreateMockNote(mockRepository, 5);
            mockNoteRepository.Setup(c => c.FindBy(It.IsAny<int>(), It.IsAny<int>())).Returns(mockNotes.Object);

            GetNoteResponse response = noteService.GetNote(new GetNoteRequest() { ContactId = 4, NoteId = 5 });
            
            mockRepository.VerifyAll();
            Assert.AreEqual(mockNotes.Object.Id, response.NoteViewModel.NoteId);
        }

        [TestMethod]
        public void GetNote_RuntimeException_ExceptionDetails()
        {
            mockNoteRepository.Setup(c => c.FindBy(It.IsAny<int>(), It.IsAny<int>())).Throws(new InvalidOperationException());

            GetNoteResponse response = noteService.GetNote(new GetNoteRequest() { NoteId = NOTE_ID });
            mockRepository.VerifyAll();
            Assert.AreEqual(typeof(InvalidOperationException), response.Exception.GetType());
            Assert.AreNotEqual(null, response.Exception);
        }

        [TestMethod]
        public void InsertNote_ValidNote_Succeed()
        {
            NoteViewModel viewModel = NoteMockData.GetNoteViewModel();
            //var mockNote = NoteMockData.CreateMockNote(mockRepository, 0);
            mockNoteRepository.Setup(mnt => mnt.Insert(It.IsAny<Note>())).Verifiable("Error ocuured calling repository method");

            SaveNoteResponse response = noteService.InsertNote(new SaveNoteRequest() { NoteViewModel = viewModel});
            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void InsertNote_RunTimeException_ExceptionDetails()
        {
            NoteViewModel noteViewModel = NoteMockData.GetNoteViewModel();
            mockNoteRepository.Setup(c => c.Insert(new Note())).Throws(new NullReferenceException());
            SaveNoteResponse response = noteService.InsertNote(new SaveNoteRequest() { NoteViewModel = noteViewModel });
            mockNoteRepository.VerifyAll();
            Assert.AreEqual(typeof(NullReferenceException), response.Exception.GetType());
            Assert.AreNotEqual(null, response.Exception);
        }

        [TestMethod]
        public void DeleteNote_ValidNote_Succeed()
        {
            mockNoteRepository.Setup(a => a.DeleteNote(It.IsAny<int>()));
            DeleteNoteResponse response = noteService.DeleteNote(new DeleteNoteRequest() { NoteId = NOTE_ID });
            mockRepository.VerifyAll();
            Assert.AreEqual(null, response.Exception);
        }

        [TestMethod]
        public void UpdateNote_ValidNote_Succeed()
        {
            NoteViewModel noteViewModel = NoteMockData.GetNoteViewModel();
            mockNoteRepository.Setup(a => a.Update(new Note()));
            UpdateNoteResponse response = noteService.UpdateNote(new UpdateNoteRequest() { NoteViewModel = noteViewModel });
            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void UpdateNote_InValidNote_Failed()
        {
            NoteViewModel noteViewModel = NoteMockData.GetNoteViewModel();
            mockNoteRepository.Setup(a => a.Update(new Note())).Throws(new NullReferenceException());
            UpdateNoteResponse response = noteService.UpdateNote(new UpdateNoteRequest() { NoteViewModel = noteViewModel });
            mockRepository.VerifyAll();
            Assert.AreEqual(typeof(NullReferenceException), response.Exception.GetType());
            Assert.AreNotEqual(null, response.Exception);
        } 
        
        [TestMethod]
        public void NoteAutoMap_ViewModelToEntity_SuccessfulMapping()
        {
            NoteViewModel viewModel = new NoteViewModel()
            {
                NoteId = NOTE_ID,
                NoteDetails = "Sample Note",
                //Contacts = NoteMockData.GetMockContacts(mockRepository, 2),
                //TagsList = NoteMockData.GetMockContacts(mockRepository, 2)
            };

            Note note = Mapper.Map<NoteViewModel, Note>(viewModel);

            Assert.AreEqual(note.Id, viewModel.NoteId);
            Assert.AreEqual(note.Details, viewModel.NoteDetails);
            Assert.AreEqual(note.Tags, viewModel.TagsList);
        }

        [TestMethod]
        public void NoteAutoMap_EntityToDatabase_SuccessfulMapping()
        {
            Note note = new Note()
            {
                Id = NOTE_ID,
                CreatedBy = CREATED_BY_ID,
                CreatedOn = DateTime.Now,
                //Contacts = (IList<Contact>)NoteMockData.GetMockNoteswithSetups(mockRepository, 2),
                //Tags = (ICollection<Tag>)NoteMockData.GetMockNoteTagsWithSetups(mockRepository, 2)
            };

            NotesDb notesDb = Mapper.Map<Note, NotesDb>(note);

            Assert.AreEqual(notesDb.NoteID, note.Id);
            Assert.AreEqual(notesDb.NoteDetails, note.Details);
        }

        [TestMethod]
        public void NoteAutoMAp_DatabaseToEntities_SuccessfulMapping()
        {
            NotesDb notesDb = new NotesDb()
            {
                NoteID = NOTE_ID,
                NoteDetails = "Sample Note",
                CreatedBy = CREATED_BY_ID,
                CreatedOn = DateTime.Now,
                //Contacts = (ICollection<ContactsDb>)NoteMockData.GetMockNoteswithSetups(mockRepository, 2),
                //Tags = (IList<TagsDb>)NoteMockData.GetMockNoteTagsWithSetups(mockRepository, 2)
            };

            Note note = Mapper.Map<NotesDb, Note>(notesDb);

            Assert.AreEqual(note.Id, notesDb.NoteID);
            Assert.AreEqual(note.Details, notesDb.NoteDetails);
        }

        [TestMethod]
        public void NoteAutoMAp_EntitiesToViewModel_SuccessfulMapping()
        {
            Note note = new Note()
            {
                Id = NOTE_ID,
                Details = "Sample Note",
                CreatedBy = CREATED_BY_ID,
                CreatedOn = DateTime.Now,
                //Contacts = (IList<Contact>)NoteMockData.GetMockNoteswithSetups(mockRepository, 2),
                //Tags = (IList<Tag>)NoteMockData.GetMockNoteTagsWithSetups(mockRepository, 2)
            };

            NoteViewModel viewModel = Mapper.Map<Note, NoteViewModel>(note);

            Assert.AreEqual(viewModel.NoteId, note.Id);
            Assert.AreEqual(viewModel.NoteDetails, note.Details);
            Assert.AreEqual(viewModel.TagsList, note.Tags);
        }
    }
}
