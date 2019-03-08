using AutoMapper;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Exceptions;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.Tags;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Opportunities;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.MessageQueues;
using SmartTouch.CRM.SearchEngine.Indexing;
using SmartTouch.CRM.SearchEngine.Search;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class TagService : ITagService
    {
        readonly ITagRepository tagRepository;
        readonly IUnitOfWork unitOfWork;
        readonly ISearchService<Tag> searchService;
        readonly IIndexingService indexingService;
        readonly IAccountService accountService;
        readonly ICachingService cachingService;
        readonly IContactService contactService;
        readonly IContactRepository contactRepository;
        readonly IMessageService messageService;
        public TagService(ITagRepository tagRepository, IUnitOfWork unitOfWork,
            IIndexingService indexingService, ISearchService<Tag> searchService, IAccountService accountService, ICachingService cachingService, IMessageService messageService,
            IContactRepository contactRepository, IContactService contactService)
        {
            if (tagRepository == null) throw new ArgumentNullException("tagRepository");
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");

            this.tagRepository = tagRepository;
            this.unitOfWork = unitOfWork;
            this.searchService = searchService;
            this.indexingService = indexingService;
            this.accountService = accountService;
            this.cachingService = cachingService;
            this.messageService = messageService;
            this.contactService = contactService;
            this.contactRepository = contactRepository;
        }

        public SearchTagsResponse SearchTagByName(SearchTagsRequest request)
        {
            Logger.Current.Verbose("Request to search by tag name.");
            SearchTagsResponse response = new SearchTagsResponse();
            IEnumerable<Tag> tagsResult = tagRepository.GetTagsByName(request.Query, request.AccountId);

            if (tagsResult == null)
                response.Exception = GetTagNotFoundException();
            else
            {
                response.Tags= Mapper.Map<IEnumerable<Tag>, IEnumerable<TagViewModel>>(tagsResult);
            }

            return response;
        }

        public SearchTagsResponse SearchTagsByTagName(SearchTagsRequest request)
        {
           Logger.Current.Verbose("Request to search by tag name.");
            SearchTagsResponse response = new SearchTagsResponse();
            IList<TagViewModel> tags = null;

            SearchParameters parameters = new SearchParameters();
            parameters.AutoCompleteFieldName = "tagNameAutoComplete";
            parameters.Types = new List<Type>() { typeof(Tag) };
            parameters.AccountId = request.AccountId;
            var results = tagRepository.SearchTagsByTagName(request.AccountId, request.Query, request.Limit);
           
          
                tags = new List<TagViewModel>();
                foreach (Tag suggestion in results)
                {
                    tags.Add(Mapper.Map<Tag,TagViewModel>(suggestion));
                }
                response.Tags = tags;
            

            return response;
        }
        public GetTagResponse GetTag(GetTagRequest request)
        {
            GetTagResponse response = new GetTagResponse();
            Tag tag = tagRepository.FindBy(request.Id);
            if (tag == null)
                response.Exception = GetTagNotFoundException();
            else
            {
                TagViewModel tagViewModel = Mapper.Map<Tag, TagViewModel>(tag);
                response.TagViewModel = tagViewModel;
            }

            return response;
        }

        public SaveTagResponse SaveTag(SaveTagRequest request)
        {
            Logger.Current.Verbose("Request to save the tag.");
            Tag tag = Mapper.Map<TagViewModel, Tag>(request.TagViewModel);
            isTagValid(tag);

            if (tag.Id == 0 && request.TagViewModel.ContactId == 0)
                IsDuplicateTag(tag);
            Tag persistedTag = tagRepository.SaveContactTag(request.TagViewModel.ContactId,
                request.TagViewModel.TagName, request.TagViewModel.TagID, request.TagViewModel.AccountID, request.TagViewModel.CreatedBy);

            unitOfWork.Commit();

            addToTopic(persistedTag.Id, request.TagViewModel.AccountID, request.TagViewModel.ContactId, request.TagViewModel.CreatedBy, true);

            if (request.TagViewModel.ContactId != 0)
                contactService.ContactIndexing(new ContactIndexingRequest() { ContactIds = new List<int>() { request.TagViewModel.ContactId }, Ids = new Dictionary<int, bool>() { { request.TagViewModel.ContactId, true } }.ToLookup(o => o.Key, o => o.Value) });
            //persistedTag.TagNameAutoComplete.Output = persistedTag.LeadScoreTag == true ? persistedTag.TagName + " *" : persistedTag.TagName;
            indexingService.IndexTag(persistedTag);

            request.TagViewModel.TagID = persistedTag.Id;

            SaveTagResponse response = new SaveTagResponse();
            response.TagViewModel = request.TagViewModel;
            return response;
        }
        public SaveTagResponse LeadScore_SaveTag(SaveTagRequest request)
        {
            Logger.Current.Verbose("Request to save the tag.");
            Tag tag = Mapper.Map<TagViewModel, Tag>(request.TagViewModel);
            isTagValid(tag);

            Tag persistedTag = tagRepository.SaveContactTag(request.TagViewModel.ContactId,
                request.TagViewModel.TagName, request.TagViewModel.TagID, request.TagViewModel.AccountID, request.TagViewModel.CreatedBy);

            unitOfWork.Commit();

            addToTopic(persistedTag.Id, request.TagViewModel.AccountID, request.TagViewModel.ContactId, request.TagViewModel.CreatedBy, true);

            //persistedTag.TagNameAutoComplete.Output = persistedTag.LeadScoreTag == true ? persistedTag.TagName + " *" : persistedTag.TagName;

            indexingService.IndexTag(persistedTag);

            request.TagViewModel.TagID = persistedTag.Id;

            SaveTagResponse response = new SaveTagResponse();
            response.TagViewModel = request.TagViewModel;
            return response;
        }
        public SaveTagResponse SaveOpportunityTag(SaveTagRequest request)
        {
            Logger.Current.Verbose("Request to save the opportunity tag.");
            Tag tag = Mapper.Map<TagViewModel, Tag>(request.TagViewModel);
            isTagValid(tag);

            Tag persistedTag = tagRepository.SaveOpportunityTag(request.TagViewModel.OpportunityID,
                request.TagViewModel.TagName, request.TagViewModel.TagID, request.TagViewModel.AccountID, request.TagViewModel.CreatedBy);
            unitOfWork.Commit();
            //persistedTag.TagNameAutoComplete.Output = persistedTag.LeadScoreTag == true ? persistedTag.TagName + " *" : persistedTag.TagName;
            indexingService.IndexTag(persistedTag);
            SaveTagResponse response = new SaveTagResponse();
            response.TagViewModel = request.TagViewModel;
            return response;
        }

        public GetTagListResponse GetTagsBasedonaccount(int accountId)
        {
            Logger.Current.Verbose("Request to fetch all the tags.");
            GetTagListResponse response = new GetTagListResponse();
            IEnumerable<Tag> tags = tagRepository.FindAll(accountId);
            if (tags == null)
            {
                response.Exception = GetTagNotFoundException();
            }
            else
            {
                IEnumerable<TagViewModel> list = Mapper.Map<IEnumerable<Tag>, IEnumerable<TagViewModel>>(tags);
                response.Tags = list;
            }

            return response;
        }
        
        public GetTagListResponse GetAllTags(GetTagListRequest request)
        {
            Logger.Current.Verbose("Request to fetch all the tags.");
            GetTagListResponse response = new GetTagListResponse();
            if (request.SortField != null)
            {
                var maps = SmartTouch.CRM.ApplicationServices.ObjectMappers.MapperConfigurationProvider.Instance.FindTypeMapFor<TagViewModel, Tag>();

                foreach (var propertyMap in maps.GetPropertyMaps())
                {
                    if (request.SortField.Equals(propertyMap.SourceMember.Name))
                    {
                        request.SortField = propertyMap.DestinationProperty.MemberInfo.Name;
                        break;
                    }
                }
            }
            IEnumerable<Tag> tags = tagRepository.FindAll(request.Limit, request.PageNumber, request.Name, request.AccountId, request.SortField, request.SortDirection).ToList();
            if (tags == null)
            {
                response.Exception = GetTagNotFoundException();
            }
            else
            {
                response.TotalHits = (tags != null && tags.Any()) ? tags.FirstOrDefault().TotalTagCount : 0;
                response.Tags = Mapper.Map<IEnumerable<Tag>, IEnumerable<TagViewModel>>(tags);
            }
            return response;
        }
        public GetTagListResponse GetAllTagsByContacts(GetTagListRequest request)
        {
            Logger.Current.Verbose("Request to fetch all the tags.");
            GetTagListResponse response = new GetTagListResponse();
            int TotalHits = default(int);
            if (request.SortField != null)
            {
                var maps = SmartTouch.CRM.ApplicationServices.ObjectMappers.MapperConfigurationProvider.Instance.FindTypeMapFor<TagViewModel, Tag>();

                foreach (var propertyMap in maps.GetPropertyMaps())
                {
                    if (request.SortField.Equals(propertyMap.SourceMember.Name))
                    {
                        request.SortField = propertyMap.DestinationProperty.MemberInfo.Name;
                        break;
                    }
                }
            }
            bool isAccountAdmin = CheckingDataSharing(request.RoleId, request.AccountId, request.IsSTadmin);
            IEnumerable<Tag> tags = tagRepository.AllTagsByContacts(request.Name, request.Limit, request.PageNumber, request.AccountId, isAccountAdmin, request.RequestedBy.Value, out TotalHits, request.AccountId, request.SortField, request.SortDirection);
            if (tags == null)
            {
                response.Exception = GetTagNotFoundException();
            }
            else
            {
                IEnumerable<TagViewModel> list = Mapper.Map<IEnumerable<Tag>, IEnumerable<TagViewModel>>(tags);
                response.Tags = list;
                response.TotalHits = TotalHits;
            }
            return response;
        }

        private bool CheckingDataSharing(short roleID, int accountId, bool isStAdmin)
        {
            var isAccountAdmin = cachingService.IsAccountAdmin(roleID, accountId);
            if (isStAdmin == true || isAccountAdmin == true)
                return isAccountAdmin = true;

            if (isAccountAdmin == false)
            {
                bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, accountId);
                if (isPrivate == false)
                    isAccountAdmin = true;
            }
            return isAccountAdmin;
        }

        public DeleteTagResponse DeleteTag(DeleteTagRequest request)
        {

            Logger.Current.Verbose("Request to delete the contact Tag.");
            Tag tag; ;
            if (request.TagId == 0)
                tag = tagRepository.FindBy(request.TagName, request.AccountId);
            else
                tag = tagRepository.FindBy(request.TagId);
            if (tag != null)
            {
                tagRepository.DeleteForContact(tag.Id, request.ContactID, request.AccountId);
                unitOfWork.Commit();

                if (request.ContactID != 0)
                    contactService.ContactIndexing(new ContactIndexingRequest() { ContactIds = new List<int>() { request.ContactID }, Ids = new Dictionary<int, bool>() { { request.ContactID, true } }.ToLookup(o => o.Key, o => o.Value) });
                addToTopic(tag.Id, request.AccountId, request.ContactID, 0, false);
                return new DeleteTagResponse();
            }
            else
            {
                return new DeleteTagResponse() { Exception = GetTagNotFoundException() };
            }
        }

        public DeleteTagResponse DeleteOpportunityTag(DeleteTagRequest request)
        {

            Logger.Current.Verbose("Request to delete the contact Tag.");
            Tag tag;
            if (request.TagId == 0)
                tag = tagRepository.FindBy(request.TagName, request.AccountId);
            else
                tag = tagRepository.FindBy(request.TagId);
            if (tag != null)
            {
                tagRepository.DeleteOpportunityTag(tag.Id, request.OpportunityID);
                unitOfWork.Commit();
                //int count = indexingService.RemoveTag(tag.Id, tag.AccountID);
                //Logger.Current.Verbose("Removed the tag " + tag.Id + " from elastic search. Response count from elastic search" + count);
                return new DeleteTagResponse();
            }
            else
            {
                return new DeleteTagResponse() { Exception = GetTagNotFoundException() };
            }
        }

        public ReIndexTagsResponse ReIndexTags(ReIndexTagsRequest request)
        {
            var response = new GetAccountsResponse();
            var totalTagsIndexed = 0;
            if (request.AccountId != 0)
            {
                var arl = new List<AccountViewModel>();
                arl.Add(new AccountViewModel() { AccountID = request.AccountId });
                response.Accounts = arl;
            }
            else
            {
                response = accountService.GetAccounts();
            }
            foreach (int accountId in response.Accounts.Select(a => a.AccountID))
            {
                Logger.Current.Verbose("Request to reindex the tags for account id:" + accountId);
                indexingService.SetupTagIndex(accountId);
                IEnumerable<Tag> documents = tagRepository.FindAll(accountId).ToList();
                int count = indexingService.ReIndexAllTags(documents);
                totalTagsIndexed += count;
            }
            return new ReIndexTagsResponse() { IndexedTags = totalTagsIndexed };
        }

        private ResourceNotFoundException GetTagNotFoundException()
        {
            return new ResourceNotFoundException("The requested tag was not found.");
        }

        void isTagValid(Tag tag)
        {
            IEnumerable<BusinessRule> brokenRules = tag.GetBrokenRules();

            if (brokenRules.Any())
            {
                StringBuilder brokenRulesBuilder = new StringBuilder();
                foreach (BusinessRule rule in brokenRules)
                {
                    brokenRulesBuilder.AppendLine(rule.RuleDescription);
                }

                throw new UnsupportedOperationException(brokenRulesBuilder.ToString());
            }
        }
        void IsDuplicateTag(Tag tag)
        {
            Tag tagRecord = tagRepository.FindBy(tag.TagName, tag.AccountID);

            if (tagRecord != null)
            {
                throw new UnsupportedOperationException("[|Tag already exists.|]");
            }
        }

        public DeleteTagResponse DeleteTags(DeleteTagIdsRequest request)
        {

            Logger.Current.Verbose("Request to delete the Tag.");
            bool isAssociatedWithWorkflow = tagRepository.isAssociatedWithWorkflows(request.TagID);
            bool isAssociatedWithLeadScoreRules = tagRepository.isAssociatedWithLeadScoreRules(request.TagID);
            if (isAssociatedWithLeadScoreRules && isAssociatedWithWorkflow)
                throw new UnsupportedOperationException("[|The selected Tag(s) is associated with Workflows and Lead Score Rules|]. [|Delete operation cancelled|].");
            else if (isAssociatedWithLeadScoreRules)
                throw new UnsupportedOperationException("[|The selected Tag(s) is associated with lead score|]. [|Delete operation cancelled|].");
            else if (isAssociatedWithWorkflow)
                throw new UnsupportedOperationException("[|The selected Tag(s) is associated with Workflow|]. [|Delete operation cancelled|].");
            IEnumerable<int> contactIds = tagRepository.DeleteTags(request.TagID, request.AccountId);
            int count = 0;
            foreach (var tagid in request.TagID)
            {
                count = count + indexingService.RemoveTag(tagid, request.AccountId);
                Logger.Current.Verbose("Removed the tag " + tagid + " from elastic search.");
            }
            if (contactIds != null && contactIds.Any())
                contactService.ContactIndexing(new ContactIndexingRequest() { ContactIds = contactIds.ToList(), Ids = contactIds.ToLookup(o => o, o => { return true; }) });

            Logger.Current.Verbose("Total deleted tags:" + request.TagID.Count()
                + ". Total deleted tags from elastic search. Response count from elastic search: " + count);
            return new DeleteTagResponse();
        }

        private Tag AssignAvailablePropertiesToDomain(ITagViewModel tagProperties)
        {
            Tag tag = new Tag();
            tag.Id = tagProperties.TagID;
            tag.TagName = tagProperties.TagName;
            tag.Description = tagProperties.Description;
            tag.Count = (int)tagProperties.Count;
            return tag;
        }

        public ContactTagSummaryResponse ContactSummaryByTag(ContactTagSummaryRequest request)
        {

            int contactsByTag = 0;
            GetModuleSharingPermissionRequest permissionRequest = new GetModuleSharingPermissionRequest();
            permissionRequest.ModuleId = (byte)AppModules.Contacts;
            permissionRequest.AccountId = request.AccountId;
            GetModuleSharingPermissionResponse permissionResponse = accountService.GetModuleSharingPermission(permissionRequest);
            bool isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);

            if (request.Tag != null)
            {
                if (permissionResponse.DataSharing == false || isAccountAdmin == true) // false implies that the data is shared by default.
                    contactsByTag = tagRepository.TotalContactsByTag(request.Tag.TagID, request.AccountId);
                else
                    contactsByTag = tagRepository.TotalContactsByTag(request.Tag.TagID, request.AccountId, request.RequestedBy);
            }

            int contactsByTags = 0;
            if (permissionResponse.DataSharing == Convert.ToBoolean(DataSharing.Shared))
            {
                contactsByTags = tagRepository
                .TotalContactsByTags(request.AllTags.Where(t => t != null)
                .Select(t => t.TagID).ToList(), request.AccountId);
            }
            else
            {
                contactsByTags = tagRepository
                    .TotalContactsByTags(request.AllTags.Where(t => t != null)
                    .Select(t => t.TagID).ToList(), request.AccountId, request.RequestedBy);
            }
            return new ContactTagSummaryResponse() { CountByTag = contactsByTag, CountsByAllTags = contactsByTags };
        }

        public PopularTagsResponse GetPopularTags(PopularTagsRequest request)
        {
            Logger.Current.Verbose("Request received to fetch popular tags for accountid: " + request.AccountId);
            PopularTagsResponse response = new PopularTagsResponse();
            IEnumerable<Tag> popularTags = tagRepository.GetPopularTags(request.Limit, request.AccountId);
            response.TagsViewModel = convertToViewModel(popularTags, request.TagsList);
            Logger.Current.Informational("Popular tags count: " + response.TagsViewModel.Count());
            return response;
        }

        public RecentTagsResponse GetRecentTags(RecentTagsRequest request)
        {
            Logger.Current.Verbose("Request received to fetch recent tags for accountid: " + request.AccountId);
            RecentTagsResponse response = new RecentTagsResponse();
            IEnumerable<Tag> recentTags = tagRepository.GetRecentTags(request.Limit, request.AccountId);
            response.TagsViewModel = convertToViewModel(recentTags, request.TagsList);
            Logger.Current.Informational("Recent tags count: " + response.TagsViewModel.Count());
            return response;
        }

        public GetRecentAndPopularTagsResponse GetRecentAndPopularTags(GetRecentAndPopularTagsRequest request)
        {
            Logger.Current.Verbose("Request receive to fetch Recent and Popular tags for account: " + request.AccountId);
            GetRecentAndPopularTagsResponse response = new GetRecentAndPopularTagsResponse();
            var sw1 = new Stopwatch();
            sw1.Start();
            IEnumerable<RecentPopularTag> recentPopularTags = tagRepository.GetRecentAndPopularTags(request.AccountId);
            sw1.Stop();
            Logger.Current.Informational("Response received in : " + sw1.ElapsedMilliseconds + " ms.");
            response.RecentTags = convertToViewModel(recentPopularTags.Where(c => c.TagType == "Recent").ToList(), request.TagsList);
            response.PopularTags = convertToViewModel(recentPopularTags.Where(c => c.TagType == "Popular").ToList(), request.TagsList);
            return response;

        }

        private IEnumerable<TagViewModel> convertToViewModel(IEnumerable<Tag> tags, int[] tagsList)
        {
            IList<TagViewModel> tagViewModels = new List<TagViewModel>();
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    var tagViewModel = Mapper.Map<Tag, TagViewModel>(tag);
                    if (tagsList != null)
                        tagViewModel.IsSelected = tagsList.Where(p => p == tagViewModel.TagID).Any();
                    tagViewModels.Add(tagViewModel);

                }
            }
            return tagViewModels;
        }
        //  public async Task<SaveContactTagsResponse> SaveContactTags(SaveContactTagsRequest request)
        public SaveContactTagsResponse SaveContactTags(SaveContactTagsRequest request)
        {
            SaveContactTagsResponse response = new SaveContactTagsResponse();

            if (request.Tags != null)
            {
                IEnumerable<int> contacts = new List<int>();
                if (request.Contacts != null)
                {
                    contacts = request.Contacts.Select(p => p.Id).ToList();
                }
                List<Opportunity> opportunities = new List<Opportunity>();
                if (request.Opportunities != null)
                {
                    foreach (OpportunitiesList opportunityList in request.Opportunities)
                    {
                        Opportunity opportunity;
                        opportunity = new Opportunity() { Id = opportunityList.OpportunityID };
                        opportunities.Add(opportunity);
                    }
                }
                IEnumerable<Tag> Tags = Mapper.Map<IEnumerable<TagViewModel>, IEnumerable<Tag>>(request.Tags);

                List<Tag> tags = tagRepository.SaveContactTags(contacts, opportunities, Tags, request.AccountId, request.UserId);
                if (tags != null)
                {
                    foreach (Tag tag in tags)
                        indexingService.IndexTag(tag);
                }
                if (request.Contacts != null && request.Contacts.Any())
                {
                    contactService.ContactIndexing(new ContactIndexingRequest() { ContactIds = request.Contacts.Select(s => s.Id).ToList(), Ids = request.Contacts.ToLookup(o => o.Id, o => { return true; }) });

                    addToTopic(tags.Select(s => s.Id).Distinct(), contacts, request.UserId, request.AccountId);
                }

                response.TagIds = tags.Select(p => p.Id).ToList();

            }

            return response;
        }

        public Tag UpdateTagBulkData(int tagId, int accounId, int userId)
        {
            List<int> tagids = new List<int>();
            tagids.Add(tagId);
            List<int> contactIds = tagRepository.GetContactsByTag(tagId, accounId);
            var tag = tagRepository.FindBy(tagId);
            //contactService.ContactIndexing(new ContactIndexingRequest() { ContactIds = contactIds });
             accountService.InsertIndexingData(new InsertIndexingDataRequest()
            {
                IndexingData = new Domain.Accounts.IndexingData() { EntityIDs = contactIds, IndexType = (int)IndexType.Contacts }
            });
            addToTopic(tagids, contactIds, userId, accounId);
            return tag;
        }

        public WorkflowAddTagResponse AddTag(WorkflowAddTagRequest request)
        {
            Logger.Current.Informational("Request received for adding a tag for a contact");
            WorkflowAddTagResponse response = new WorkflowAddTagResponse();

            if (request.TagId != 0 && request.ContactId != 0)
            {
                tagRepository.SaveContactTag(request.ContactId, request.TagId, request.CreatedBy, request.AccountId);
                accountService.InsertIndexingData(new InsertIndexingDataRequest() { IndexingData = new Domain.Accounts.IndexingData() { EntityIDs = new List<int>() { request.ContactId }, IndexType = (int)IndexType.Contacts } });
                addToTopic(request.TagId, request.AccountId, request.ContactId, request.CreatedBy, true);
            }
            return response;
        }

        public WorkflowRemoveTagResponse RemoveTag(WorkflowRemoveTagRequest request)
        {
            Logger.Current.Informational("Request received for removing a tag for a contact");
            WorkflowRemoveTagResponse response = new WorkflowRemoveTagResponse();

            if (request.TagId != 0 && request.ContactId != 0)
            {
                tagRepository.DeleteForContact(request.TagId, request.ContactId, request.AccountId);
                accountService.InsertIndexingData(new InsertIndexingDataRequest() { IndexingData = new Domain.Accounts.IndexingData() { EntityIDs = new List<int>() { request.ContactId }, IndexType = (int)IndexType.Contacts } });
                addToTopic(request.TagId, request.AccountId, request.ContactId, request.CreatedBy, false);
            }
            return response;
        }

        //For workflow Add/Remove
        void addToTopic(int tagId, int accountId, int contactId, int createdBy, bool isAddTag)
        {
            if (tagId != 0 && accountId != 0)
            {
                //new message for each contact and tag.
                byte conditionTypeId = isAddTag ? (byte)LeadScoreConditionType.ContactTagAdded : (byte)LeadScoreConditionType.ContactTagRemoved;
                var message = new TrackMessage()
                {
                    EntityId = tagId,
                    AccountId = accountId,
                    ContactId = contactId,
                    UserId = createdBy,
                    LeadScoreConditionType = conditionTypeId,
                    LinkedEntityId = tagId
                };
                messageService.SendMessages(new Messaging.Messages.SendMessagesRequest()
                    {
                        Message = message
                    });
            }
        }

        public void addToTopic(IEnumerable<int> tagIds, IEnumerable<int> contactIds, int userId, int accountId)
        {
            if (tagIds.Any() && contactIds.Any())
            {
                foreach (var tagId in tagIds)
                {
                    var messages = new List<TrackMessage>();
                    foreach (var contactId in contactIds)
                    {
                        var message = new TrackMessage()
                        {
                            EntityId = tagId,
                            AccountId = accountId,
                            ContactId = contactId,
                            UserId = userId,
                            LeadScoreConditionType = (int)LeadScoreConditionType.ContactTagAdded,
                            LinkedEntityId = tagId
                        };
                        messages.Add(message);
                    }
                    messageService.SendMessages(new Messaging.Messages.SendMessagesRequest()
                        {
                            Messages = messages
                        });
                }
            }
        }

        public void addLeadAdapterToTopic(int leadadapterid, IEnumerable<int> contactIds, int accountId)
        {
            var messages = new List<TrackMessage>();
            foreach (var contactId in contactIds)
            {
                var message = new TrackMessage()
                {
                    EntityId = leadadapterid,
                    AccountId = accountId,
                    ContactId = contactId,
                    LeadScoreConditionType = (int)LeadScoreConditionType.LeadAdapterSubmitted,
                };
                messages.Add(message);
            }
            messageService.SendMessages(new Messaging.Messages.SendMessagesRequest()
                {
                    Messages = messages
                });
        }

        #region Merge

        public SaveTagResponse MergeTag(SaveTagRequest request)
        {
            Logger.Current.Verbose("Request to merge the tags.");
            SaveTagResponse tagResponse = new SaveTagResponse();
            bool isAssociatedWithWorkflow = tagRepository.isAssociatedWithWorkflows(new int[] { request.TagViewModel.sourceTagID });
            bool isAssociatedWithLeadScoreRules = tagRepository.isAssociatedWithLeadScoreRules(new int[] { request.TagViewModel.sourceTagID });
            tagResponse.IsAssociatedWithLeadScoreRules = isAssociatedWithLeadScoreRules;
            tagResponse.IsAssociatedWithWorkflows = isAssociatedWithWorkflow;
            if (isAssociatedWithLeadScoreRules || isAssociatedWithWorkflow)
                return tagResponse;
            return Merge(request.TagViewModel);
        }

        public SaveTagResponse ContinueMergeTag(SaveTagRequest request)
        {
            Logger.Current.Informational("Request received for continuing the merge tags");
            return Merge(request.TagViewModel);
        }

        private SaveTagResponse Merge(TagViewModel tagviewmodel)
        {
            Tag tag = new Tag();
            SaveTagResponse tagResponse = new SaveTagResponse();
            Tag existingTag = tagRepository.FindBy(tagviewmodel.TagID);
            if (existingTag == null)
            {
                Logger.Current.Informational("Logging If ExistingTag is Null TagId :" + tagviewmodel.TagID);
                tag = Mapper.Map<TagViewModel, Tag>(tagviewmodel);
                isTagValid(tag);
                tagRepository.Insert(tag);

                Tag newtag = unitOfWork.Commit() as Tag;
                TagViewModel tagViewModel = Mapper.Map<Tag, TagViewModel>(newtag);
                tagResponse.TagViewModel = tagViewModel;
                tagRepository.MergeTags(tagviewmodel.sourceTagID, tagViewModel.TagID, tagviewmodel.AccountID);
                indexingService.RemoveTag(tagviewmodel.sourceTagID, tagviewmodel.AccountID);
                //    newtag.TagNameAutoComplete.Output = newtag.LeadScoreTag == true ? newtag.TagName + " *" : newtag.TagName;
                indexingService.IndexTag(newtag);
                var contacts = tagRepository.GetContactsByTag(newtag.Id, tagviewmodel.AccountID);
                contactService.ContactIndexing(new ContactIndexingRequest() { ContactIds = contacts, Ids = contacts.ToLookup(o => o, o => { return true; }) });
                accountService.ScheduleAnalyticsRefresh(tagViewModel.TagID, (byte)IndexType.Tags);
            }
            else
            {
                Logger.Current.Informational("Logging If ExistingTag is Not Null TagId :" + existingTag.Id);
                tagRepository.MergeTags(tagviewmodel.sourceTagID, existingTag.Id, tagviewmodel.AccountID);
                var contacts = tagRepository.GetContactsByTag(existingTag.Id, tagviewmodel.AccountID);
                contactService.ContactIndexing(new ContactIndexingRequest() { ContactIds = contacts, Ids = contacts.ToLookup(o => o, o => { return true; }) });
                indexingService.RemoveTag(tagviewmodel.sourceTagID, tagviewmodel.AccountID);
                accountService.ScheduleAnalyticsRefresh(existingTag.Id, (byte)IndexType.Tags);
            }
            return tagResponse;
        }
        #endregion

        #region UpdateTag

        public UpdateTagResponse ContinueUpdateTag(UpdateTagRequest request)
        {
            Logger.Current.Verbose("Request to update the tag.");
            Tag existingTag = tagRepository.FindBy(request.TagViewModel.TagID);
            if (existingTag != null)
            {
                existingTag.TagName = request.TagViewModel.TagName;
                return Update(existingTag);
            }
            else
            {
                return new UpdateTagResponse() { Exception = GetTagNotFoundException() };
            }
        }

        public UpdateTagResponse UpdateTag(UpdateTagRequest request)
        {
            Logger.Current.Verbose("Request to update the tag.");
            UpdateTagResponse tagresponse = new UpdateTagResponse();
            Tag existingTag = tagRepository.FindBy(request.TagViewModel.TagID);
            if (existingTag != null)
            {
                bool isAssociatedWithWorkflow = tagRepository.isAssociatedWithWorkflows(new int[] { existingTag.Id });
                bool isAssociatedWithLeadScoreRules = tagRepository.isAssociatedWithLeadScoreRules(new int[] { existingTag.Id });
                tagresponse.IsAssociatedWithWorkflows = isAssociatedWithWorkflow;
                tagresponse.IsAssociatedWithLeadScoreRules = isAssociatedWithLeadScoreRules;
                if (isAssociatedWithLeadScoreRules || isAssociatedWithWorkflow)
                    return tagresponse;
                existingTag.TagName = request.TagViewModel.TagName;
                return Update(existingTag);
            }
            else
            {
                return new UpdateTagResponse() { Exception = GetTagNotFoundException() };
            }
        }

        private UpdateTagResponse Update(Tag exisitingtag)
        {
            bool isDuplicate = tagRepository.IsDuplicateTag(exisitingtag.TagName, exisitingtag.AccountID, exisitingtag.Id);
            if (isDuplicate)
                throw new InvalidOperationException("[|Tag Name already exists.|]");
            tagRepository.Update(exisitingtag);
            unitOfWork.Commit();
            Tag tag = tagRepository.FindBy(exisitingtag.Id);
            //   tag.TagNameAutoComplete.Output = tag.LeadScoreTag == true ? tag.TagName + " *" : tag.TagName;
            indexingService.UpdateTag(tag);
            return new UpdateTagResponse();
        }

        #endregion

        public TagIndexingResponce TagIndexing(TagIndexingRequest request)
        {
            TagIndexingResponce responce = new TagIndexingResponce();
            foreach (var id in request.TagIds)
            {
                Tag tag = tagRepository.FindBy(id);
                indexingService.IndexTag(tag);
            }
            return responce;
        }

    }
}
