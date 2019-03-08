using AutoMapper;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Exceptions;
using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.Image;
using SmartTouch.CRM.ApplicationServices.Messaging.Opportunity;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Images;
using SmartTouch.CRM.Domain.Opportunities;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.Users;
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
using System.Linq;
using System.Text;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class OpportunityService : IOpportunitiesService
    {
        readonly IOpportunityRepository opportunityRepository;
        readonly IUnitOfWork unitOfWork;
        IIndexingService indexingService;
        ISearchService<Opportunity> searchService;
        readonly ICachingService cachingService;
        readonly IContactService contactService;
        readonly IMessageService messageService;
        readonly IImageService imageService;
        readonly IUrlService urlService;

        readonly ITagRepository tagRepository;
        readonly IUserRepository userRepository;
        readonly IContactRepository contactRepository;


        public OpportunityService(IOpportunityRepository opportunityRepository, IContactService contactService,
            IUnitOfWork unitOfWork, ICachingService cachingService,
            IIndexingService indexingService, ITagRepository tagRepository,
            ISearchService<Opportunity> searchService, IUserRepository userRepository,
            IContactRepository contactRepository, IMessageService messageService, IImageService imageService, IUrlService urlService)
        {
            this.opportunityRepository = opportunityRepository;
            this.unitOfWork = unitOfWork;
            this.indexingService = indexingService;
            this.cachingService = cachingService;
            this.contactService = contactService;
            this.searchService = searchService;
            this.messageService = messageService;
            this.tagRepository = tagRepository;
            this.userRepository = userRepository;
            this.contactRepository = contactRepository;
            this.imageService = imageService;
            this.urlService = urlService;
        }

        /// <summary>
        /// Insert Opportunity.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>HttpResponseMessage</returns>
        public InsertOpportunityResponse InsertOpportunity(InsertOpportunityRequest request)
        {
            Logger.Current.Verbose("Request for inserting an opportunity");
            if (request.opportunityViewModel.Image != null)
            {
                SaveImageResponse imageResponse = imageService.SaveImage(new SaveImageRequest()
                {
                    ImageCategory = ImageCategory.OpportunityProfile,
                    ViewModel = request.opportunityViewModel.Image,
                    AccountId = request.AccountId
                });
                request.opportunityViewModel.Image = imageResponse.ImageViewModel;
            }
            Opportunity opportunity = Mapper.Map<OpportunityViewModel, Opportunity>(request.opportunityViewModel);
            opportunity.IsDeleted = false;
            bool isOpportunityUnique = opportunityRepository.IsOpportunityUnique(opportunity);
            if (!isOpportunityUnique)
            {
                var message = "[|Opportunity with name|] \"" + opportunity.OpportunityName + "\" [|already exists.|]";
                throw new UnsupportedOperationException(message);
            }
            isOpportunityValid(opportunity);
            OpportunityTableType opportunityTableType = Mapper.Map<OpportunityViewModel, OpportunityTableType>(request.opportunityViewModel);
            OpportunityTableType newOpportunityType = opportunityRepository.InsertOpportunity(opportunityTableType);
           // opportunityRepository.Insert(opportunity);
            Opportunity newOpportunity = opportunityRepository.FindBy(newOpportunityType.OpportunityID);

            if (indexingService.Index<Opportunity>(newOpportunity) > 0)
                Logger.Current.Verbose("Indexed the opportunity successfully");

           
            return new InsertOpportunityResponse();
        }

        void isOpportunityValid(Opportunity opportunity)
        {
            IEnumerable<BusinessRule> brokenRules = opportunity.GetBrokenRules();

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

        public GetOpportunityResponse getOpportunity(GetOpportunityRequest request)
        {
            GetOpportunityResponse response = new GetOpportunityResponse();
            hasAccess(request.OpportunityID, request.RequestedBy, request.AccountId, request.RoleId);
            Opportunity opportunity = opportunityRepository.FindBy(request.OpportunityID);

            


            if (opportunity == null)
            {
                response.Exception = GetOpportunitiesNotFoundException();
            }
            else
            {
                SearchContactsRequest contactsRequest = new SearchContactsRequest();
                contactsRequest.ContactIDs = opportunity.Contacts.ToArray();
                contactsRequest.AccountId = request.AccountId;
                contactsRequest.RoleId = request.RoleId;
                contactsRequest.RequestedBy = request.RequestedBy;
                contactsRequest.Limit = opportunity.Contacts.Count();
                contactsRequest.PageNumber = 1;
                SearchContactsResponse<ContactEntry> contactsResponse = contactService.GetAllContacts<ContactEntry>(contactsRequest);
                IEnumerable<int> people = opportunity.Contacts;
                var contacts = contactsResponse.Contacts.Where(c => people.Contains(c.Id));

                IEnumerable<Tag> tags = tagRepository.FindByOpportunity(request.OpportunityID);

                OpportunityViewModel opportunityViewModel = Mapper.Map<Opportunity, OpportunityViewModel>(opportunity);
                opportunityViewModel.Contacts = contacts;
                opportunityViewModel.ContactCount = opportunity.Contacts.Count();
                response.OpportunityViewModel = opportunityViewModel;

                opportunityViewModel.TagsList = tags;

                if (opportunity.ImageID != null)
                {
                    Image image = opportunityRepository.GetOpportunityProfileImage(opportunity.ImageID.Value);
                    opportunityViewModel.Image = Mapper.Map<Image, ImageViewModel>(image);
                    opportunityViewModel.Image.ImageContent = urlService.GetUrl(opportunityViewModel.AccountID, ImageCategory.OpportunityProfile, opportunityViewModel.Image.StorageName);
                }
                    

            }
            return response;
        }

        void hasAccess(int documentId, int? userId, int accountId, short roleId)
        {
            bool isAccountAdmin = cachingService.IsAccountAdmin(roleId, accountId);
            if (!isAccountAdmin)
            {
                bool isPrivate = cachingService.IsModulePrivate(AppModules.Opportunity, accountId);
                if (isPrivate && !searchService.IsCreatedBy(documentId, userId, accountId))
                    throw new PrivateDataAccessException("Requested user is not authorized to get this opportunity.");
            }
        }

        public UpdateOpportunityResponse UpdateOpportunity(UpdateOpportunityRequest request)
        {
            Logger.Current.Verbose("Request for updating an opportunity");
            request.opportunityViewModel.IsDeleted = false;
            var opportunityViewModel = request.opportunityViewModel;
            hasAccess(request.opportunityViewModel.OpportunityID, request.RequestedBy, request.AccountId, request.RoleId);
            if (request.opportunityViewModel.Image != null)
            {
                SaveImageResponse imageResponse = imageService.SaveImage(new SaveImageRequest()
                {
                    ImageCategory = ImageCategory.OpportunityProfile,
                    ViewModel = request.opportunityViewModel.Image,
                    AccountId = request.AccountId
                });
                request.opportunityViewModel.Image = imageResponse.ImageViewModel;
            }
            Opportunity opportunity = Mapper.Map<OpportunityViewModel, Opportunity>(request.opportunityViewModel);
            bool isOpportunityUnique = opportunityRepository.IsOpportunityUnique(opportunity);
            if (!isOpportunityUnique)
            {
                var message = "[|Opportunity with name|] \"" + opportunity.OpportunityName + "\" [|already exists.|]";
                throw new InvalidOperationException(message);
            }
            isOpportunityValid(opportunity);
            //opportunityRepository.Update(opportunity);
            // Opportunity updatedOpportunity = unitOfWork.Commit() as Opportunity;
            OpportunityTableType opportunityTableType = Mapper.Map<OpportunityViewModel, OpportunityTableType>(request.opportunityViewModel);
            OpportunityTableType newOpportunityType = opportunityRepository.InsertOpportunity(opportunityTableType);
            // opportunityRepository.Insert(opportunity);
            Opportunity updatedOpportunity = opportunityRepository.FindBy(newOpportunityType.OpportunityID);
            
            if (indexingService.Update<Opportunity>(updatedOpportunity) > 0)
                Logger.Current.Verbose("Opportunity updated to elasticsearch successfully");
            if (request.opportunityViewModel.StageID != request.opportunityViewModel.PreviousStageID)
                addToTopic(opportunityViewModel.OpportunityID, request.AccountId, opportunityViewModel.Contacts.Select(s => s.Id), opportunityViewModel.StageID);

            return new UpdateOpportunityResponse();
        }

        void addToTopic(int opportunityId, int accountId, IEnumerable<int> contactIds, short stageId)
        {
            if (opportunityId != 0 && accountId != 0 && contactIds.Any())
            {
                var messages = new List<TrackMessage>();
                foreach (var contactId in contactIds)
                {
                    var message = new TrackMessage()
                    {
                        EntityId = stageId,
                        ContactId = contactId,
                        AccountId = accountId,
                        LeadScoreConditionType = (int)LeadScoreConditionType.OpportunityStatusChanged,
                        LinkedEntityId = opportunityId
                    };
                    messages.Add(message);
                }
                messageService.SendMessages(new Messaging.Messages.SendMessagesRequest()
                    {
                        Messages = messages
                    });
            }
        }

        public SearchOpportunityResponse GetAllOpportunities(SearchOpportunityRequest request)
        {
            SearchOpportunityResponse response = new SearchOpportunityResponse();

            IEnumerable<Opportunity> opportunities = opportunityRepository.GetOpportunitiesWithBuyersList(request.AccountId,request.PageNumber,request.Limit,request.Query,request.SortField,request.UserIDs,request.StartDate,request.EndDate,request.SortDirection);
            IEnumerable<OpportunityViewModel> opportunityList = MapDomainToVM(opportunities);
            response.Opportunities = opportunityList;
            response.TotalHits = opportunityList.IsAny() ? opportunityList.Select(t => t.TotalCount).FirstOrDefault() : 0;
            return response;
        }

        private IEnumerable<OpportunityViewModel> MapDomainToVM(IEnumerable<Opportunity> opportunityDomain)
        {
            List<OpportunityViewModel> opportunities = new List<OpportunityViewModel>();
            if (opportunityDomain.IsAny())
            {
                foreach (var opp in opportunityDomain)
                {
                    OpportunityViewModel opportunity = new OpportunityViewModel();
                    opportunity.OpportunityID = opp.Id;
                    opportunity.OpportunityName = opp.OpportunityName;
                    opportunity.OpportunityType = opp.OpportunityType;
                    opportunity.ProductType = opp.ProductType;
                    opportunity.Potential = opp.Potential.HasValue?opp.Potential.Value:0;
                    opportunity.ContactName = opp.ContactName;
                    opportunity.ContactID = opp.ContactID;
                    opportunity.ContactType = opp.ContactType;
                    opportunity.AccountID = opp.AccountID;
                    opportunity.TotalCount = opp.TotalCount;
                    opportunities.Add(opportunity);
                }
            }
            return opportunities;
        }

        public DeleteOpportunityResponse DeleteOpportunities(DeleteOpportunityRequest request)
        {
            Logger.Current.Verbose("Request for opportunities delete");

            opportunityRepository.DeleteOpportunities(request.OpportunityIDs, (int)request.RequestedBy);
            foreach (int opportunityId in request.OpportunityIDs)
                indexingService.Remove<Opportunity>(opportunityId);

            return new DeleteOpportunityResponse();
        }

        public ReIndexDocumentResponse ReIndexOpportunities(ReIndexDocumentRequest request)
        {
            Logger.Current.Verbose("Request for ReIndexing opportunities.");

            var opportunities = opportunityRepository.FindAll().ToList();
            foreach (var opportunity in opportunities)
            {
                opportunity.Contacts = opportunityRepository.GetRelatedContacts(opportunity.Id);
            }
            int count = indexingService.ReIndexAll<Opportunity>(opportunities);
            return new ReIndexDocumentResponse() { Documents = count };
        }

        public IEnumerable<OpportunityViewModel> GetAllOpportunitiesByOwner(int[] ownerId, DateTime startDate, DateTime endDate)
        {
            Logger.Current.Verbose("Request for get the opportunities for selected owner.");

            IEnumerable<Opportunity> opportunities = opportunityRepository.FindAllByOwner(ownerId, startDate, endDate).ToList();
            foreach (var opportunity in opportunities)
            {
                opportunity.Contacts = opportunityRepository.GetRelatedContacts(opportunity.Id);
            }
            IEnumerable<OpportunityViewModel> opportunityList = Mapper.Map<IEnumerable<Opportunity>, IEnumerable<OpportunityViewModel>>(opportunities);
            return opportunityList;
        }

        private ResourceNotFoundException GetOpportunitiesNotFoundException()
        {
            return new ResourceNotFoundException("The requested opportuniites was not found.");
        }

        SearchOpportunityResponse search(SearchOpportunityRequest request, IEnumerable<Type> types, IList<string> fields, bool matchAll, bool autoComplete)
        {
            SearchOpportunityResponse response = new SearchOpportunityResponse();
            SearchParameters parameters = new SearchParameters();
            parameters.Limit = request.Limit;
            parameters.PageNumber = request.PageNumber == 0 ? 1 : request.PageNumber;
            parameters.Types = types;
            parameters.MatchAll = matchAll;
            parameters.AccountId = request.AccountId;
            parameters.Ids = request.OpportunityIDs;
            parameters.UserID = request.UserID;
            parameters.StartDate = request.StartDate;
            parameters.EndDate = request.EndDate;

            if (request.SortField != null)
            {
                List<string> sortFields = new List<string>();
                var maps = SmartTouch.CRM.ApplicationServices.ObjectMappers.MapperConfigurationProvider.Instance.FindTypeMapFor<OpportunityViewModel, Opportunity>();

                foreach (var propertyMap in maps.GetPropertyMaps())
                {
                    if (request.SortField.Equals(propertyMap.SourceMember.Name))
                    {
                        sortFields.Add(propertyMap.DestinationProperty.MemberInfo.Name);
                        break;
                    }
                }
                parameters.SortDirection = request.SortDirection;
                parameters.SortFields = sortFields;
            }
            Logger.Current.Informational("Search string:" + request.Query);
            Logger.Current.Informational("Parameters:" + parameters.ToString());
            SearchResult<Opportunity> searchResult;

            bool isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Opportunity, request.AccountId);
            if (isPrivate && !isAccountAdmin)
            {
                int userId = (int)request.RequestedBy;
                searchResult = searchService.Search(request.Query, c => c.OwnerId == userId, parameters);
            }
            else if (request.UserID != null && request.StartDate != null && request.EndDate != null)
            {
                int userId = (int)request.UserID;
                searchResult = searchService.Search(request.Query, c => c.CreatedBy == userId, parameters);
            }
            else
            {
                searchResult = searchService.Search(request.Query, parameters);
            }

            IEnumerable<Opportunity> opportunities = searchResult.Results;

            Logger.Current.Informational("Search complete, total results:" + searchResult.Results.Count());

            if (opportunities == null)
                response.Exception = GetOpportunitiesNotFoundException();
            else
            {
                IEnumerable<OpportunityViewModel> list = Mapper.Map<IEnumerable<Opportunity>, IEnumerable<OpportunityViewModel>>(opportunities);

                IEnumerable<int> contactIds = searchResult.Results.SelectMany(o => o.Contacts).Distinct().ToList();
                SearchContactsRequest contactsRequest = new SearchContactsRequest();
                contactsRequest.ContactIDs = contactIds.ToArray();
                contactsRequest.AccountId = request.AccountId;
                contactsRequest.RoleId = request.RoleId;
                contactsRequest.RequestedBy = request.RequestedBy;
                contactsRequest.Limit = contactIds.Count();
                contactsRequest.PageNumber = 1;
                contactsRequest.Module = AppModules.Opportunity;
                SearchContactsResponse<ContactEntry> contactsResponse = contactService.GetAllContacts<ContactEntry>(contactsRequest);
                foreach (Opportunity opportunity in searchResult.Results)
                {
                    var opportunityViewModel = list.SingleOrDefault(o => o.OpportunityID == opportunity.Id);

                    IEnumerable<int> people = opportunity.Contacts;

                    //if (isPrivate && !isAccountAdmin)
                    //{
                    //    IList<int> owncontacts = contactRepository.GetContactByUserId((int)request.RequestedBy);

                    //    var peopleinvoled = opportunity.PeopleInvolved.Where(n => !owncontacts.Contains(n.ContactID));

                    //    foreach (var p in peopleinvoled)
                    //    {
                    //        p.IsPrivate = true;
                    //    }
                    //}

                    var contacts = contactsResponse.Contacts.Where(c => people.Contains(c.Id));
                    opportunityViewModel.Contacts = contacts;
                }


                response.Opportunities = list;
                response.TotalHits = searchResult.TotalHits;
            }
            return response;
        }

        public GetOpportunityListByContactResponse GetContactOpportunities(GetOpportunityListByContactRequest request)
        {
            Logger.Current.Verbose("Request to fetch Opportunities based on ContactId");
            GetOpportunityListByContactResponse response = new GetOpportunityListByContactResponse();
            IEnumerable<Opportunity> opportunities = null;

            Logger.Current.Informational("ContactId : " + request.ContactID);

            bool isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Opportunity, request.AccountId);
            if (isPrivate && !isAccountAdmin)
            {
                int userId = (int)request.RequestedBy;
                opportunities = opportunityRepository.FindByContact(request.ContactID,userId); 
            }
            else
                opportunities = opportunityRepository.FindByContact(request.ContactID, 0); 


          

            if (opportunities == null)
            {
                response.Exception = GetOpportunitiesNotFoundException();
            }
            else
            {
                IEnumerable<OpportunityViewModel> opportunitieslist = Mapper.Map<IEnumerable<Opportunity>, IEnumerable<OpportunityViewModel>>(opportunities);
                response.OpportunitiesList = opportunitieslist;
            }
            return response;
        }

        public GetOpportunityListByContactResponse GetContactOpportunitiesList(GetOpportunityListByContactRequest request)
        {
            Logger.Current.Verbose("Request to fetch Opportunities based on ContactId");
            GetOpportunityListByContactResponse response = new GetOpportunityListByContactResponse();
            IEnumerable<OpportunityBuyer> opportunities = null;

            Logger.Current.Informational("ContactId : " + request.ContactID);

            bool isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Opportunity, request.AccountId);
            if (isPrivate && !isAccountAdmin)
            {
                int userId = (int)request.RequestedBy;
                opportunities = opportunityRepository.GetAllContactOpportunities(request.ContactID);
            }
            else
                opportunities = opportunityRepository.GetAllContactOpportunities(request.ContactID);




            if (opportunities == null)
            {
                response.Exception = GetOpportunitiesNotFoundException();
            }
            else
            {
                response.Opportunities = opportunities;
            }
            return response;
        }

        public DeleteOpportunityContactResponse DeleteOpportunityContact(DeleteOpportunityContactRequest request)
        {
            Logger.Current.Verbose("Request for deleting the contact for the opportunity");

            opportunityRepository.DeleteOpportunityContact(request.OpportunityID, request.ContactID);
            return new DeleteOpportunityContactResponse();
        }

        public GetOpportunityStageContactsRsponse GetOpportunityStageContacts(GetOpportunityStageContactsRequest request)
        {
            GetOpportunityStageContactsRsponse response = new GetOpportunityStageContactsRsponse();

            short OpportunityStageId;
            bool isConverted = short.TryParse(request.StageId.ToString(), out OpportunityStageId);
            if (isConverted)
                response.ContactIds = opportunityRepository.GetOpportunityStageContacts(OpportunityStageId);
            return response;
        }

        public OpportunityIndexingResponce OpportunityIndexing(OpportunityIndexingRequest request)
        {
            OpportunityIndexingResponce responce = new OpportunityIndexingResponce();
            foreach (int id in request.OpportunityIds) 
            {
               Opportunity opportunity = opportunityRepository.FindBy(id);
               if (indexingService.Index<Opportunity>(opportunity) > 0)
                   Logger.Current.Verbose("Indexed the opportunity successfully");
            }
            return responce;
        }

        public GetOpportunityListResponse GetAllOpportunitiesByName(GetOpportunityListRequest request)
        {
            GetOpportunityListResponse response = new GetOpportunityListResponse();
            IEnumerable<Opportunity> opportunities = opportunityRepository.SearchByOpportunityName(request.AccountID, request.Query);
            IEnumerable<OpportunityViewModel> opportunitieslist = Mapper.Map<IEnumerable<Opportunity>, IEnumerable<OpportunityViewModel>>(opportunities);
            response.Opportunities = opportunitieslist;
            return response;
        }

        public InsertOpportunityBuyerResponse InsertOpportunityBuyer(InsertOpportunityBuyerRequest request)
        {
            InsertOpportunityBuyerResponse response = new InsertOpportunityBuyerResponse();
            
            List<OpportunityContactMapTableType> opportunityBuyers = new List<OpportunityContactMapTableType>();
            foreach(int contactid in request.opportunityViewModel.Contacts.Select(c => c.Id))
            {
                OpportunityContactMapTableType oppBuyerTableType = new OpportunityContactMapTableType();
                oppBuyerTableType.OpportunityContactMapID = 0;
                oppBuyerTableType.OpportunityID = request.opportunityViewModel.OpportunityID;
                oppBuyerTableType.ContactID = contactid;
                oppBuyerTableType.Potential = request.opportunityViewModel.Potential;
                oppBuyerTableType.ExpectedToClose = request.opportunityViewModel.ExpectedCloseDate;
                oppBuyerTableType.Comments = request.opportunityViewModel.Comments;
                oppBuyerTableType.Owner = request.opportunityViewModel.OwnerId;
                oppBuyerTableType.StageID = request.opportunityViewModel.StageID;
                oppBuyerTableType.CreatedOn = DateTime.Now.ToUniversalTime();
                oppBuyerTableType.CreatedBy = request.RequestedBy.Value;
                opportunityBuyers.Add(oppBuyerTableType);
                this.addToTopic(request.opportunityViewModel.OpportunityID, request.opportunityViewModel.AccountID,new List<int>() { contactid }, request.opportunityViewModel.StageID);
            }

            if (request.RequestedBy != request.opportunityViewModel.OwnerId)
            {
                Notification notificationdata = new Notification();
                notificationdata.Details = "[|An opportunity has been assigned to you.|]";
                notificationdata.EntityId = request.opportunityViewModel.OpportunityID;
                notificationdata.Subject = "[|An opportunity has been assigned to you.|]";
                notificationdata.Time = DateTime.Now.ToUniversalTime();
                notificationdata.Status = NotificationStatus.New;
                notificationdata.UserID = request.opportunityViewModel.OwnerId;
                notificationdata.ModuleID = request.ModuleID;
                userRepository.AddNotification(notificationdata);
            }

            IEnumerable<OpportunityContactMapTableType> buyers = opportunityBuyers.ToList();
            opportunityRepository.InsertAndUpdateOpportunityBuyers(buyers);
            Opportunity updatedOpportunity = opportunityRepository.FindBy(request.opportunityViewModel.OpportunityID);

            if (indexingService.Update<Opportunity>(updatedOpportunity) > 0)
                Logger.Current.Verbose("Opportunity updated to elasticsearch successfully");
            return response;
        }

        public UpdateOpportunityBuyerResponse UpdateOpportunityBuyer(UpdateOpportunityBuyerRequest request)
        {
            UpdateOpportunityBuyerResponse response = new UpdateOpportunityBuyerResponse();
            List<OpportunityContactMapTableType> opportunityBuyers = new List<OpportunityContactMapTableType>();
            foreach (int contactid in request.opportunityViewModel.Contacts.Select(c => c.Id))
            {
                OpportunityContactMapTableType oppBuyerTableType = new OpportunityContactMapTableType();
                oppBuyerTableType.OpportunityContactMapID = 0;
                oppBuyerTableType.OpportunityID = request.opportunityViewModel.OpportunityID;
                oppBuyerTableType.ContactID = contactid;
                oppBuyerTableType.Potential = request.opportunityViewModel.Potential;
                oppBuyerTableType.ExpectedToClose = request.opportunityViewModel.ExpectedCloseDate;
                oppBuyerTableType.Comments = request.opportunityViewModel.Comments;
                oppBuyerTableType.Owner = request.opportunityViewModel.OwnerId;
                oppBuyerTableType.StageID = request.opportunityViewModel.StageID;
                oppBuyerTableType.IsDeleted = false;
                oppBuyerTableType.CreatedOn = DateTime.Now.ToUniversalTime();
                oppBuyerTableType.CreatedBy = request.RequestedBy.Value;
                opportunityBuyers.Add(oppBuyerTableType);
            }
            IEnumerable<OpportunityContactMapTableType> buyers = opportunityBuyers.ToList();
            opportunityRepository.InsertAndUpdateOpportunityBuyers(buyers);
            Opportunity updatedOpportunity = opportunityRepository.FindBy(request.opportunityViewModel.OpportunityID);

            if (indexingService.Update<Opportunity>(updatedOpportunity) > 0)
                Logger.Current.Verbose("Opportunity updated to elasticsearch successfully");
            return response;
        }

        public UpdateOpportunityViewResponse UpdateOpportunityName(UpdateOpportunityViewRequest request)
        {
            UpdateOpportunityViewResponse response = new UpdateOpportunityViewResponse();
        
            Opportunity updatedOpportunity = opportunityRepository.FindBy(request.OpportunityID);
            updatedOpportunity.OpportunityName = request.OpportunityName;
            bool isOpportunityUnique = opportunityRepository.IsOpportunityUnique(updatedOpportunity);
            if (!isOpportunityUnique)
            {
                var message = "[|Opportunity with name|] \"" + updatedOpportunity.OpportunityName + "\" [|already exists.|]";
                throw new UnsupportedOperationException(message);
            }

            opportunityRepository.UpdateOpportunityName(request.OpportunityID, request.OpportunityName,request.RequestedBy.Value);
            if (indexingService.Update<Opportunity>(updatedOpportunity) > 0)
                Logger.Current.Verbose("Opportunity updated to elasticsearch successfully");
            OpportunityViewModel opportunityViewModel = Mapper.Map<Opportunity, OpportunityViewModel>(updatedOpportunity);
            response.opportunityViewModel = opportunityViewModel;
            return response;
        }

        public UpdateOpportunityViewResponse UpdateOpportunityStage(UpdateOpportunityViewRequest request)
        {
            UpdateOpportunityViewResponse response = new UpdateOpportunityViewResponse();
            opportunityRepository.UpdateOpportunityStage(request.OpportunityID, request.StageID);
            Opportunity updatedOpportunity = opportunityRepository.FindBy(request.OpportunityID);
            if (indexingService.Update<Opportunity>(updatedOpportunity) > 0)
                Logger.Current.Verbose("Opportunity updated to elasticsearch successfully");
            OpportunityViewModel opportunityViewModel = Mapper.Map<Opportunity, OpportunityViewModel>(updatedOpportunity);
            response.opportunityViewModel = opportunityViewModel;
            return response;

        }

        public UpdateOpportunityViewResponse UpdateOpportunityOwner(UpdateOpportunityViewRequest request)
        {

            UpdateOpportunityViewResponse response = new UpdateOpportunityViewResponse();
            opportunityRepository.UpdateOpportunityOwner(request.OpportunityID, request.OwnerId);
            Opportunity updatedOpportunity = opportunityRepository.FindBy(request.OpportunityID);
            if (indexingService.Update<Opportunity>(updatedOpportunity) > 0)
                Logger.Current.Verbose("Opportunity updated to elasticsearch successfully");
            OpportunityViewModel opportunityViewModel = Mapper.Map<Opportunity, OpportunityViewModel>(updatedOpportunity);
            response.opportunityViewModel = opportunityViewModel;
            return response;
        }

        public UpdateOpportunityViewResponse UpdateOpportunityDescription(UpdateOpportunityViewRequest request)
        {
            UpdateOpportunityViewResponse response = new UpdateOpportunityViewResponse();
            opportunityRepository.UpdateOpportunityDescription(request.OpportunityID, request.Description, request.RequestedBy.Value);
            Opportunity updatedOpportunity = opportunityRepository.FindBy(request.OpportunityID);
            if (indexingService.Update<Opportunity>(updatedOpportunity) > 0)
                Logger.Current.Verbose("Opportunity updated to elasticsearch successfully");
            OpportunityViewModel opportunityViewModel = Mapper.Map<Opportunity, OpportunityViewModel>(updatedOpportunity);
            response.opportunityViewModel = opportunityViewModel;
            return response;
        }

        public UpdateOpportunityViewResponse UpdateOpportunityPotential(UpdateOpportunityViewRequest request)
        {
            UpdateOpportunityViewResponse response = new UpdateOpportunityViewResponse();
            opportunityRepository.UpdateOpportunityPotential(request.OpportunityID, request.Potential, request.RequestedBy.Value);
            Opportunity updatedOpportunity = opportunityRepository.FindBy(request.OpportunityID);
            if (indexingService.Update<Opportunity>(updatedOpportunity) > 0)
                Logger.Current.Verbose("Opportunity updated to elasticsearch successfully");
            OpportunityViewModel opportunityViewModel = Mapper.Map<Opportunity, OpportunityViewModel>(updatedOpportunity);
            response.opportunityViewModel = opportunityViewModel;
            return response;
        }

        public UpdateOpportunityViewResponse UpdateOpportunityExpectedCloseDate(UpdateOpportunityViewRequest request)
        {
            UpdateOpportunityViewResponse response = new UpdateOpportunityViewResponse();
            opportunityRepository.UpdateOpportunityExpectedCloseDate(request.OpportunityID, request.ExpectedCloseDate);
            Opportunity updatedOpportunity = opportunityRepository.FindBy(request.OpportunityID);
            if (indexingService.Update<Opportunity>(updatedOpportunity) > 0)
                Logger.Current.Verbose("Opportunity updated to elasticsearch successfully");
            OpportunityViewModel opportunityViewModel = Mapper.Map<Opportunity, OpportunityViewModel>(updatedOpportunity);
            response.opportunityViewModel = opportunityViewModel;
            return response;
        }

        public UpdateOpportunityViewResponse UpdateOpportunityType(UpdateOpportunityViewRequest request)
        {
            UpdateOpportunityViewResponse response = new UpdateOpportunityViewResponse();
            opportunityRepository.UpdateOpportunityType(request.OpportunityID, request.OpportunityType, request.RequestedBy.Value);
            Opportunity updatedOpportunity = opportunityRepository.FindBy(request.OpportunityID);
            if (indexingService.Update<Opportunity>(updatedOpportunity) > 0)
                Logger.Current.Verbose("Opportunity updated to elasticsearch successfully");
            OpportunityViewModel opportunityViewModel = Mapper.Map<Opportunity, OpportunityViewModel>(updatedOpportunity);
            response.opportunityViewModel = opportunityViewModel;
            return response;
        }

        public UpdateOpportunityViewResponse UpdateOpportunityProductType(UpdateOpportunityViewRequest request)
        {
            UpdateOpportunityViewResponse response = new UpdateOpportunityViewResponse();
            opportunityRepository.UpdateOpportunityProductType(request.OpportunityID, request.ProductType, request.RequestedBy.Value);
            Opportunity updatedOpportunity = opportunityRepository.FindBy(request.OpportunityID);
            if (indexingService.Update<Opportunity>(updatedOpportunity) > 0)
                Logger.Current.Verbose("Opportunity updated to elasticsearch successfully");
            OpportunityViewModel opportunityViewModel = Mapper.Map<Opportunity, OpportunityViewModel>(updatedOpportunity);
            response.opportunityViewModel = opportunityViewModel;
            return response;
        }

        public UpdateOpportunityViewResponse UpdateOpportunityAddress(UpdateOpportunityViewRequest request)
        {
            UpdateOpportunityViewResponse response = new UpdateOpportunityViewResponse();
            opportunityRepository.UpdateOpportunityAddress(request.OpportunityID, request.Address, request.RequestedBy.Value);
            Opportunity updatedOpportunity = opportunityRepository.FindBy(request.OpportunityID);
            if (indexingService.Update<Opportunity>(updatedOpportunity) > 0)
                Logger.Current.Verbose("Opportunity updated to elasticsearch successfully");
            OpportunityViewModel opportunityViewModel = Mapper.Map<Opportunity, OpportunityViewModel>(updatedOpportunity);
            response.opportunityViewModel = opportunityViewModel;
            return response;
        }

        public UpdateOpportunityViewResponse UpdateOpportunityImage(UpdateOpportunityViewRequest request)
        {
            UpdateOpportunityViewResponse response = new UpdateOpportunityViewResponse();
            Opportunity updatedOpportunity = opportunityRepository.FindBy(request.OpportunityID);
            Image image = Mapper.Map<ImageViewModel, Image>(request.image);
            opportunityRepository.UpdateOpportunityImage(request.OpportunityID, image,request.RequestedBy.Value);
            if (indexingService.Update<Opportunity>(updatedOpportunity) > 0)
                Logger.Current.Verbose("Opportunity updated to elasticsearch successfully");
            OpportunityViewModel opportunityViewModel = Mapper.Map<Opportunity, OpportunityViewModel>(updatedOpportunity);
            response.opportunityViewModel = opportunityViewModel;
            return response;
        }

        public GetOpportunityBuyerResponse GetOpportunityBuyers(GetOpportunityBuyerRequest request)
        {
            GetOpportunityBuyerResponse response = new GetOpportunityBuyerResponse();
            IEnumerable<OpportunityBuyer> buyers = opportunityRepository.GetAllOpportunityBuyers(request.OpportunityId, request.AccountId, request.PageNumber, request.PageSize);
            string comments = string.Empty;
            var totalBuyers = new List<OpportunityBuyer>();
            if(buyers.IsAny())
            {
                foreach (OpportunityBuyer buyer in buyers.Where(r=>r.RowNumber==1))
                {
                    var sb = new StringBuilder();
                    int i = 0;
                    if (buyers.Count(c=> c.ContactID == buyer.ContactID) > 1)
                    {
                        buyers.Where(b => b.ContactID == buyer.ContactID).Each(e =>
                        {
                             if(i > 0)
                               sb.AppendLine(!string.IsNullOrEmpty(e.Comments) ? e.CreatedOn.ToShortDateString()  + ":" + e.Comments : string.Empty);
                            i++;

                        });
                        buyer.PreviousComments = sb.ToString();
                    }
                    totalBuyers.Add(buyer);
                }
            }
            response.OpportunityBuyers = totalBuyers;
            return response;
        }

        public OpportunityBuyer GetOpportunityBuyerById(int buyerId)
        {
            OpportunityBuyer buyer = opportunityRepository.GetOpportunityBuyerDetailsById(buyerId);
            buyer.Comments = null;
            return buyer;
        }

        public void DeleteOpportunityBuyer(int buyerId)
        {
            int opportunityId = opportunityRepository.DeleteOpportunityBuyer(buyerId);
            if (opportunityId != 0)
            {
                Opportunity updatedOpportunity = opportunityRepository.FindBy(opportunityId);

                if (indexingService.Update<Opportunity>(updatedOpportunity) > 0)
                    Logger.Current.Verbose("Opportunity updated to elasticsearch successfully");
            }
        }

        public string GetOpportunityNameByOPPContactMapId(int buyerId)
        {
            return opportunityRepository.GetOpportunityNameByBuyerId(buyerId);
        }

        public IEnumerable<OpportunityBuyer> GetAllOpportunityBuyersName(int opportunityId, int accountId)
        {
            IEnumerable<OpportunityBuyer> buyers = opportunityRepository.GetAllOpportunityBuyerNames(opportunityId, accountId);
            return buyers;
        }

        public GetOpportunityContactsResponse GetOpportunityContacts(GetOpportunityContactsRequest request)
        {
            GetOpportunityContactsResponse response = new GetOpportunityContactsResponse();
            response.ContactIdList = opportunityRepository.GetOpportunityContactIds(request.OpportunityId);
            return response;
        }
    }
}
