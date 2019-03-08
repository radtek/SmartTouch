using AutoMapper;
using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.CommunicationManager.Processors;
using LM = LandmarkIT.Enterprise.CommunicationManager.Requests;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Exceptions;
using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using SmartTouch.CRM.ApplicationServices.Messaging.Image;
using SmartTouch.CRM.ApplicationServices.Messaging.Search;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Search;
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
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using SD = SmartTouch.CRM.Domain.Images;
using SDI = System.Drawing.Imaging;
using SmartTouch.CRM.ApplicationServices.ServiceAgents;
using System.Diagnostics;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class CampaignService : ICampaignService
    {

        readonly IAdvancedSearchRepository advancedSearchRepository;
        readonly ICampaignRepository campaignRepository;
        readonly IContactRepository contactRepository;
        readonly IUnitOfWork unitOfWork;
        readonly IUrlService urlService;
        readonly ITagRepository tagRepository;
        readonly ICachingService cachingService;
        IIndexingService indexingService;
        ISearchService<Campaign> searchService;
        ISearchService<Contact> contactSearchService;
        readonly IAdvancedSearchService advancedSearchService;
        readonly IUserService userService;
        readonly IUserRepository userRepository;
        readonly IMessageService messageService;
        readonly ICommunicationService communicationService;
        readonly IAccountService accountService;
        readonly IServiceProviderRepository serviceProviderRepository;

        public CampaignService(ICampaignRepository campaignRepository, IContactRepository contactRepository,
            IUnitOfWork unitOfWork, IUrlService urlService, ITagRepository tagRepository,
            ICachingService cachingService, IIndexingService indexingService, ISearchService<Campaign> searchService,
            IAdvancedSearchService advancedSearchService, IUserService userService, IUserRepository userRepository,
            IMessageService messageService, ICommunicationService communicationService, IAccountService accountService,
            IServiceProviderRepository serviceProviderRepository, ISearchService<Contact> contactSearchService)
        {
            this.campaignRepository = campaignRepository;
            this.contactRepository = contactRepository;
            this.unitOfWork = unitOfWork;
            this.urlService = urlService;
            this.tagRepository = tagRepository;
            this.cachingService = cachingService;
            this.indexingService = indexingService;
            this.messageService = messageService;
            this.searchService = searchService;
            this.contactSearchService = contactSearchService;
            this.advancedSearchService = advancedSearchService;
            this.userService = userService;
            this.userRepository = userRepository;
            this.communicationService = communicationService;
            this.accountService = accountService;
            this.serviceProviderRepository = serviceProviderRepository;
            this.advancedSearchRepository = advancedSearchRepository;
        }

        public GetCampaignTemplatesResponse GetCampaignTemplates(GetCampaignTemplatesRequest request)
        {
            GetCampaignTemplatesResponse response = new GetCampaignTemplatesResponse();
            Logger.Current.Verbose("Request received to fetch campaign templates");
            IList<CampaignTemplateViewModel> campaignTemplateListViewModel = new List<CampaignTemplateViewModel>();
            response.Templates = campaignTemplateListViewModel;
            var campaignTemplates = (campaignRepository.GetTemplates(request.AccountId));
            response.Templates = Mapper.Map<IEnumerable<CampaignTemplate>, IEnumerable<CampaignTemplateViewModel>>(campaignTemplates);
            foreach (CampaignTemplateViewModel campaignTemplateVM in response.Templates)
            {
                CampaignTemplate campaignTemplate = campaignTemplates.Where(c => c.Id == campaignTemplateVM.TemplateId).FirstOrDefault();
                campaignTemplateVM.ThumbnailImageUrl = urlService.GetUrl(campaignTemplate.ThumbnailImage.ImageCategoryID, campaignTemplate.ThumbnailImage.StorageName);
            }
            return response;
        }

        public GetCampaignTemplateResponse GetCampaignTemplate(GetCampaignTemplateRequest request)
        {
            GetCampaignTemplateResponse response = new GetCampaignTemplateResponse();
            Logger.Current.Verbose("Request received to fetch campaign template " + request.CampaignTemplateID);

            CampaignTemplateViewModel campaignTemplateViewModel = new CampaignTemplateViewModel();
            response.CampaignTemplateViewModel = campaignTemplateViewModel;
            var campaignTemplate = campaignRepository.GetTemplate(request.CampaignTemplateID);
            response.CampaignTemplateViewModel = Mapper.Map<CampaignTemplate, CampaignTemplateViewModel>(campaignTemplate);

            //Revisit the below two lines of code. They are not needed.
            var url = urlService.GetUrl(campaignTemplate.ThumbnailImage.ImageCategoryID, campaignTemplate.ThumbnailImage.StorageName);
            response.CampaignTemplateViewModel.ThumbnailImageUrl = url;
            response.CampaignTemplateViewModel.HTMLContent = response.CampaignTemplateViewModel.HTMLContent.Replace("[IMAGES_URL]", urlService.GetImageHostingUrl());

            return response;
        }

        public GetCampaignTemplateNamesResponse GetTemplateNamesRequest(GetCampaignTemplateNamesRequest request)
        {
            GetCampaignTemplateNamesResponse response = new GetCampaignTemplateNamesResponse();
            response.TemplateNames = new List<CampaignTemplate>();

            if (request.AccountId != 0)
            {
                response.TemplateNames = campaignRepository.GetTemplateNames(request.AccountId);
            }
            return response;
        }

        public async Task<GetCampaignRecipientIdsResponse> GetCampaignRecipientsByID(GetCampaignRecipientIdsRequest request)
        {
            GetCampaignRecipientIdsResponse response = new GetCampaignRecipientIdsResponse();
            List<int> contactIds = new List<int>();
            List<int> searchDefinitionIds = campaignRepository.GetSearchDefinitionIds(request.CampaignId);
            foreach (int id in searchDefinitionIds)
            {
                var contactIDs = await advancedSearchService.GetSavedSearchContactIds(new GetSavedSearchContactIdsRequest()
                {
                    AccountId = request.AccountId,
                    RequestedBy = request.RequestedBy,
                    RoleId = request.RoleId,
                    SearchDefinitionId = id
                });

                contactIds.AddRange(contactIDs);
            }
            response.ContactIds = contactIds;
            return response;

        }

        public GetCampaignResponse GetCampaign(GetCampaignRequest request)
        {
            GetCampaignResponse response = new GetCampaignResponse();
            Logger.Current.Verbose("Request received to fetch the Campaign with CampaignID: " + request.Id);

            hasAccess(request.Id, request.RequestedBy, request.AccountId, request.RoleId);
            Campaign campaign = campaignRepository.GetCampaignById(request.Id);
            CampaignViewModel campaignViewModel = Mapper.Map<Campaign, CampaignViewModel>(campaign);
            campaignViewModel.IsLitmusTestPerformed = campaignRepository.CampaignHasLitmusResults(request.Id);
            IEnumerable<CampaignLitmusMap> results = campaignRepository.GetLitmusIdByCampaignId(request.Id, request.AccountId, 0);
            if (results.IsAny())
                campaignViewModel.LitmusGuid = results.OrderByDescending(m => m.LastModifiedOn).FirstOrDefault().LitmusId;

            Guid mailTesterGuid = campaignRepository.GetMailTesterGuid(request.Id);
            if (mailTesterGuid != null && mailTesterGuid != new Guid())
                campaignViewModel.MailTesterGuid = mailTesterGuid.ToString();

            IEnumerable<Tag> tags = tagRepository.FindByCampaign(request.Id);
            IEnumerable<Tag> contactTags = tagRepository.FindContactTagsByCampaign(request.Id);
            IEnumerable<SearchDefinition> searchDefinitions = campaignRepository.GetCampaignSearchDefinitionsMap(request.Id);
            campaignViewModel.TagsList = Mapper.Map<IEnumerable<Tag>, IEnumerable<TagViewModel>>(tags);
            campaignViewModel.ContactTags = Mapper.Map<IEnumerable<Tag>, IList<TagViewModel>>(contactTags);
            campaignViewModel.SearchDefinitions = Mapper.Map<IEnumerable<SearchDefinition>, IEnumerable<AdvancedSearchViewModel>>(searchDefinitions).ToList();
            campaignViewModel.Links = Mapper.Map<IEnumerable<CampaignLink>, IEnumerable<CampaignLinkViewModel>>(campaignRepository.GetCampaignLinks(request.Id));
            response.CampaignViewModel = campaignViewModel;          
         
            return response;
        }

        public GetCampaignMailTesterGuidResponse GetMailTesterGuid(GetCampaignMailTesterGuid request)
        {
            string guid = string.Empty;
            Guid mailTesterGuid = campaignRepository.GetMailTesterGuid(request.CampaignID);
            if (mailTesterGuid != null && mailTesterGuid != new Guid())
                guid = mailTesterGuid.ToString();

            return new GetCampaignMailTesterGuidResponse() { Guid = guid };
        }

        void hasAccess(int documentId, int? userId, int accountId, short roleId)
        {
            Logger.Current.Verbose("Request received to vadidate user permission for document: " + documentId + ", for user: " + userId);
            bool isAccountAdmin = cachingService.IsAccountAdmin(roleId, accountId);
            if (!isAccountAdmin)
            {
                bool isPrivate = cachingService.IsModulePrivate(AppModules.Campaigns, accountId);
                if (isPrivate && !searchService.IsCreatedBy(documentId, userId, accountId))
                    throw new UnsupportedOperationException("[|Requested user is not authorized to get this campaign.|]");
            }
        }

        public SearchCampaignsResponse GetAllCampaigns(SearchCampaignsRequest request)
        {
            Logger.Current.Verbose("Request received to get campaigns through search by user: " + request.RequestedBy);
            SearchCampaignsResponse response = new SearchCampaignsResponse();
            //IEnumerable<Type> types = new List<Type>() { typeof(Campaign) };
            //try
            //{
            //    response = search(request, types, false, false);
            //}
            //catch (Exception ex)
            //{
            //    Logger.Current.Error("An error occured while fetching all campaigns : ", ex);
            //    throw ex;
            //}
            var campaigns = campaignRepository.FindAll(request.AccountId, request.ShowingFieldType, request.PageNumber, request.Limit, request.UserIds, request.StartDate, request.EndDate, request.Query, request.SortField, request.SortDirection);
            response.Campaigns = Mapper.Map<IEnumerable<Campaign>, IEnumerable<CampaignViewModel>>(campaigns);
            response.TotalHits = campaigns.Any() ? campaigns.FirstOrDefault().TotalCampaignCount : 0;
            return response;
        }

        private ResourceNotFoundException GetCampaignNotFoundException()
        {
            return new ResourceNotFoundException("[|The requested campaign was not found.|]");
        }

        public GetCampaignImagesResponse GetPublicCampaignImages(GetCampaignImagesRequest request)
        {
            Logger.Current.Verbose("Request received to get campaign public images " + (request.AccountID ?? 0).ToString());

            GetCampaignImagesResponse response = new GetCampaignImagesResponse();
            IEnumerable<string> imageUrls = new List<string>();
            if (!request.AccountID.HasValue)
                imageUrls = campaignRepository.GetCampaignPublicImages().Select(i => urlService.GetUrl(Entities.ImageCategory.Campaigns, i.StorageName));
            response.ImageUrls = imageUrls;
            return response;
        }

        public GetAccountCampaignImagesResponse GetCampaignImages(GetCampaignImagesRequest request)
        {
            Logger.Current.Verbose("Request received to get campaign images " + (request.AccountID ?? 0).ToString());
            GetAccountCampaignImagesResponse response = new GetAccountCampaignImagesResponse();
            List<SD.Image> images = new List<SD.Image>();
            if (request.AccountID.HasValue)
            {
                images = campaignRepository.GetCampaignImages(request.AccountID);
                foreach (var image in images)
                {
                    if (image.AccountID != null)
                        image.StorageName = urlService.GetUrl(Convert.ToInt16(image.AccountID), Entities.ImageCategory.Campaigns, image.StorageName);
                    else
                        image.StorageName = urlService.GetUrl(Entities.ImageCategory.Campaigns, image.StorageName);
                }
            }

            response.Images = Mapper.Map<List<SD.Image>, List<ImageViewModel>>(images);
            return response;
        }

        public async Task<GetNextCampaignToTriggerResponse> GetNextCampaignToTriggerAsync()
        {
            var campaign = campaignRepository.GetNextCampaignToTrigger();
            if (campaign == null)
                return new GetNextCampaignToTriggerResponse() { Campaign = null };
            else if (campaign.CampaignStatus == CampaignStatus.Failure)
            {
                UpdateCampaignTriggerStatus(new UpdateCampaignTriggerStatusRequest(campaign.Id)
                {
                    Status = CampaignStatus.Failure,
                    SentDateTime = DateTime.Now.ToUniversalTime(),
                    Remarks = "Campaign Failed. Please contact administrator for more details"
                });
            }
            try
            {
                if (campaignRepository.GetCampaignRecipientCount(campaign.Id) == 0)
                {
                    campaign.CampaignStatus = CampaignStatus.Failure;
                    campaign.Remarks = "[|No recipients found with the provided search criteria.|]";
                    Campaign failedCampaign = campaignRepository.UpdateCampaignFailedStatus(campaign.Id, campaign.CampaignStatus, campaign.Remarks);
                    //failedCampaign.HTMLContent = null;
                    //if (indexingService.Index<Campaign>(failedCampaign) > 0)
                    //    Logger.Current.Informational("Campaign indexed in to elasticsearch successfully.");
                    var responseAwait = await GetNextCampaignToTriggerAsync();
                    return new GetNextCampaignToTriggerResponse { Campaign = responseAwait.Campaign };
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An Exception occured in GetNextCampaignToTriggerAsync method: ", ex);
                if (campaign != null)
                    UpdateCampaignTriggerStatus(new UpdateCampaignTriggerStatusRequest(campaign.Id) { Status = CampaignStatus.Failure });
            }
            return new GetNextCampaignToTriggerResponse { Campaign = campaign };
        }

        /// <summary>
        /// Returns false when Contacts module is Shared
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        private bool GetContactsSharingPermission(int accountId)
        {
            GetModuleSharingPermissionRequest permissionRequest = new GetModuleSharingPermissionRequest();
            permissionRequest.ModuleId = (byte)AppModules.Contacts;
            permissionRequest.AccountId = accountId;
            GetModuleSharingPermissionResponse permissionResponse = accountService.GetModuleSharingPermission(permissionRequest);
            return permissionResponse.DataSharing;
        }

        public GetCampaignRecipientsResponse GetCampaignRecipients(GetCampaignRecipientsRequest request)
        {
            var campaignRecipients = campaignRepository.GetCampaignRecipients(request.CampaignId, request.AccountId).ToList();
            Logger.Current.Verbose("Getting receipietns3");

            IEnumerable<Contact> contacts = contactRepository.FindAll(campaignRecipients.Select(p => p.ContactID).ToList());

            foreach (CampaignRecipient rcp in campaignRecipients)
            {
                rcp.Contact = contacts.Where(p => p.Id == rcp.ContactID).FirstOrDefault();
            }

            return new GetCampaignRecipientsResponse() { Recipients = campaignRecipients };
        }

        public GetCampaignRecipientsResponse GetCampaignRecipientsInfo(GetCampaignRecipientsRequest request)
        {

            var campaignRecipients = campaignRepository.GetCampaignRecipientsInfo(request.CampaignId, request.IsLinkedToWorkflow);
            var contacts = new Dictionary<int, IDictionary<string, string>>();
            var excludels = new List<string>(){
                "CONTACTID"
            };
            foreach (IDictionary<string, object> contact in campaignRecipients)
            {
                var crid = Convert.ToInt32(CheckIfNull(contact["CRID"]));
                var dkv = new Dictionary<string, string>();
                foreach (var key in contact.Keys)
                {
                    if (!dkv.ContainsKey(key))
                        dkv.Add(key, CheckIfNull(contact[key]));
                }
                if (!contacts.ContainsKey(crid))
                {
                    if (dkv.Where(k => k.Key == "EMAILID" & k.Value != string.Empty).IsAny())
                        contacts.Add(crid, dkv);
                }


            }

            return new GetCampaignRecipientsResponse()
            {
                RecipientsInfo = contacts,

            };
        }
        /// <summary>
        /// Checks given object is null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        private string CheckIfNull(object input)
        {
            if (input == null)
                return string.Empty;
            return input.ToString();
        }

        public GetCampaignUniqueRecipientsCountResponse GetCampaignTotalUniqueRecipientsCount(GetCampaignUniqueRecipientsCountRequest request)
        {
            var tags = Mapper.Map<IEnumerable<TagViewModel>, IEnumerable<Tag>>(request.Tags);
            GetCampaignUniqueRecipientsCountResponse response = new GetCampaignUniqueRecipientsCountResponse();
            response.Recipients = campaignRepository
                    .CampaignUniqueRecipientsCount(tags, request.AccountId, request.ContactIdsFromSearch, request.ToTagStatus);
            return response;
        }

        public InsertCampaignResponse InsertCampaign(InsertCampaignRequest request)
        {
            Logger.Current.Verbose("Request received to insert a new campaign.");
            var response = insertCampaign(request.CampaignViewModel);
            return new InsertCampaignResponse() { CampaignViewModel = response };
        }

        CampaignViewModel insertCampaign(CampaignViewModel viewModel)
        {
            Logger.Current.Verbose("Request received to insert a new campaign. 'insertCampaign' method called.");

            if (viewModel.ParentCampaignId == 0 && viewModel.Name.Length > 75)
            {
                throw new UnsupportedOperationException("[|Campaign Name Is Maximum 75 characters.|]");
            }

            viewModel.HTMLContent = viewModel.HTMLContent != null ?
                               viewModel.HTMLContent.Replace("ui-droppable", "") : null;

            Campaign campaign = Mapper.Map<CampaignViewModel, Campaign>(viewModel);

            foreach (Tag tag in campaign.Tags)
                tag.AccountID = viewModel.AccountID;

            bool isCampaignNameUnique = campaignRepository.IsCampaignNameUnique(campaign);
            if (!isCampaignNameUnique)
            {

                var message = "[|Campaign with name|] \"" + campaign.Name + "\" [|already exists.|]";
                Logger.Current.Informational(message);

                throw new UnsupportedOperationException(message);
            }
            campaign.IsRecipientsProcessed = false;
            isCampaignValid(campaign);
            //var campaignstring = campaign.HTMLContent;
            //var campaignbodyindex = campaignstring.IndexOf("</div></body></html>");
            //if (campaign.CampaignUnsuscribeStatus==true)
            //    campaign.HTMLContent = campaignstring.Insert(campaignbodyindex, "<div style='display:inline-block;padding:15px 0;'><a href=" + campaign.unsubscribeLink + " style='color: #63676b;'>Unsubscribe</a></div>");

            if (campaign.CampaignStatus == CampaignStatus.Scheduled || campaign.CampaignStatus == CampaignStatus.Queued)
            {
                isCampaignHasCommunicationProviders(campaign.AccountID);
            }
            campaignRepository.Insert(campaign);
            Campaign newCampaign = unitOfWork.Commit() as Campaign;

            if (viewModel.TagsList != null)
            {

                foreach (var tag in viewModel.TagsList)
                {
                    if (tag != null && tag.TagID == 0)
                    {
                        Tag savedTag = tagRepository.FindBy(tag.TagName, viewModel.AccountID);
                        indexingService.IndexTag(savedTag);
                        accountService.ScheduleAnalyticsRefresh(savedTag.Id, (byte)IndexType.Tags);
                    }
                }
            }

            newCampaign.DeliveryRate = "0% | 0";
            newCampaign.OpenRate = "0% | 0";
            newCampaign.ClickRate = "0% | 0";
            newCampaign.HTMLContent = null;
            if (indexingService.IndexCampaign(newCampaign) > 0)
                Logger.Current.Informational("Campaign indexed in to elasticsearch successfully.");

            Logger.Current.Informational("Campaign inserted successfully.");
            //for Litmustest
            if (viewModel.PerformLitmusTest)
            {
                RequestLitmusCheck(new RequestLitmusCheck()
                {
                    CampaignId = newCampaign.Id
                });
            }

            if (viewModel.PerformMailTester)
            {
                InsertMailTesterRequest(new InsertCampaignMailTesterRequest()
                {
                    CampaignID = newCampaign.Id,
                    AccountId = newCampaign.AccountID,
                    RequestedBy = newCampaign.CreatedBy
                });
            }

            return Mapper.Map<Campaign, CampaignViewModel>(newCampaign);
        }

        public InsertCampaignImageResponse InsertCampaignImage(InsertCampaignImageRequest request)
        {
            InsertCampaignImageResponse response = new InsertCampaignImageResponse();
            response.ImageViewModel = new ImageViewModel();
            SD.Image image = Mapper.Map<ImageViewModel, SD.Image>(request.ImageViewModel);
            bool isImageFriendlyNameUnique = campaignRepository.IsImageFriendlyNameUnique(image);
            if (isImageFriendlyNameUnique)
            {
                var message = "[|Image with friendly name|] \"" + image.FriendlyName + "\" [|already exists. Please choose a different name.|]";
                throw new InvalidOperationException(message);
            }
            response.ImageViewModel.ImageID = campaignRepository.InsertCampaignImage(image);

            Logger.Current.Informational("Image inserted successfully.");
            return response;
        }

        CampaignViewModel updateCampaign(CampaignViewModel viewModel)
        {
            if (viewModel.ParentCampaignId == 0 && viewModel.Name.Length > 75)
            {
                throw new UnsupportedOperationException("[|Campaign Name Is Maximum 75 characters.|]");
            }
            if ((CampaignType)viewModel.CampaignTypeId != CampaignType.PlainText)
                viewModel.HTMLContent = viewModel.HTMLContent != null ? viewModel.HTMLContent.Replace("ui-droppable", "").Replace("\n", "") : null;



            Campaign campaign = Mapper.Map<CampaignViewModel, Campaign>(viewModel);
            bool isCampaignNameUnique = campaignRepository.IsCampaignNameUnique(campaign);
            if (!isCampaignNameUnique)
            {
                var message = "[|Campaign with name|] \"" + campaign.Name + "\" [|already exists. Please choose a different name.|]";
                throw new UnsupportedOperationException(message);
            }
            isCampaignValid(campaign);

            if (campaign.CampaignStatus == CampaignStatus.Queued || campaign.CampaignStatus == CampaignStatus.Scheduled)
            {
                isCampaignHasCommunicationProviders(campaign.AccountID);
            }
            campaignRepository.Update(campaign);
            Campaign updatedCampaign = unitOfWork.Commit() as Campaign;


            if (viewModel.TagsList != null)
            {

                foreach (var tag in viewModel.TagsList)
                {
                    if (tag != null && tag.TagID == 0)
                    {
                        Tag savedTag = tagRepository.FindBy(tag.TagName, viewModel.AccountID);
                        indexingService.IndexTag(savedTag);
                        accountService.ScheduleAnalyticsRefresh(savedTag.Id, (byte)IndexType.Tags);
                    }
                }
            }
            if (updatedCampaign.CampaignStatus != CampaignStatus.Sent)
            {
                updatedCampaign.DeliveryRate = "0% | 0";
                updatedCampaign.OpenRate = "0% | 0";
                updatedCampaign.ClickRate = "0% | 0";
            }
            updatedCampaign.Tags = null;
            updatedCampaign.HTMLContent = null;
            if (indexingService.IndexCampaign(updatedCampaign) > 0)
                Logger.Current.Informational("Campaign updated to elasticsearch successfully.");


            // Logic for saved search by ramakrisha NEXG-3005------
            var _SearchDefinitionsid = 0;
            foreach (SearchDefinition ff in campaign.SearchDefinitions)
            {
                _SearchDefinitionsid = ff.Id;
            }
            GetSearchRequest _nn = new GetSearchRequest();
            _nn.SearchDefinitionID = _SearchDefinitionsid;
            _nn.IncludeSearchResults = false;
            _nn.Limit = 20000;
            _nn.AccountId = campaign.AccountID;
            _nn.RoleId = 00;
            _nn.RequestedBy = null;
            _nn.IsRunSearchRequest = true;
            _nn.IsSTAdmin = true;

            List<CampaignRecipient> _Mylist = null;
            if (_SearchDefinitionsid > 0)
            {
                _Mylist = testing(_nn, campaign);

                // insert into Campaign Recipient table woth campId
                campaign.CampaignRecipients = _Mylist;
            }
            if (_Mylist != null)
            {
                foreach (CampaignRecipient _obj in _Mylist)
                {
                    if (_obj.To != null)
                    {
                        if (_obj.ScheduleTime != null)
                            campaignRepository.InsertCampaignRecipients(campaign, _obj);
                    }
                }
            }
            // NEXG-3005-------- END--------

            Logger.Current.Informational("Campaign updated successfully.");
            return Mapper.Map<Campaign, CampaignViewModel>(updatedCampaign);
        }

        public UpdateCampaignResponse UpdateCampaign(UpdateCampaignRequest request)
        {
            Logger.Current.Verbose("Request received to update campaign with CampaignID " + request.CampaignViewModel.CampaignID);
            //var htmlContent = request.CampaignViewModel.HTMLContent;
            updateCampaign(request.CampaignViewModel);
            return new UpdateCampaignResponse();
        }

        public void UpdateCampaignTriggerStatus(UpdateCampaignTriggerStatusRequest request)
        {
            Logger.Current.Verbose("In CampaignService > UpdateCampaignTriggerStatus. Request: " + request.Id + " AccountId: " + request.AccountId);

            List<LastTouchedDetails> lastTouched = new List<LastTouchedDetails>();
            Campaign updatedCampaign = null;
            if (request.RecipientIds.IsAny() && request.IsRelatedToWorkFlow)
            {
                Logger.Current.Verbose("Workflow Campaign: RecipientIds Found. Count: " + request.RecipientIds.Count());
                updatedCampaign = campaignRepository.UpdateCampaignTriggerStatusForWorkflow
                (request.Id, request.Status, request.SentDateTime, request.ServiceProviderCampaignId
                , request.RecipientIds, request.Remarks, request.ServiceProviderID, out lastTouched);
            }
            else
            {
                var campaignRecipients = request.Recipients.IsAny() ? request.Recipients.ToList() :
                                            ((request.RecipientIds.IsAny()) ? request.RecipientIds.Select(r => r.ToString()).ToList() :
                                                        new List<string>());
                Logger.Current.Verbose("Updating Campaign Status to : " + request.Status);
                updatedCampaign = campaignRepository.UpdateCampaignTriggerStatus
                (request.Id, request.Status, request.SentDateTime, request.ServiceProviderCampaignId
                , campaignRecipients, request.Remarks, request.ServiceProviderID, request.IsDelayedCampaign, out lastTouched, request.RecipientIds);
            }

            Logger.Current.Verbose("Campaign Status updated successfully. CampaignId: " + request.Id + ". Status: " + request.Status);

            IList<int> ContactIDs = new List<int>();
            if (lastTouched != null)
            {
                ContactIDs = lastTouched.Select(x => x.ContactID).ToList();
                contactRepository.UpdateLastTouchedInformation(lastTouched, AppModules.Campaigns, null);
                var indexingData = new IndexingData()
                {
                    EntityIDs = ContactIDs,
                    IndexType = (int)IndexType.Contacts
                };
                accountService.InsertIndexingData(new InsertIndexingDataRequest() { IndexingData = indexingData });
            }
            updatedCampaign.Contacts = null;
            updatedCampaign.CampaignRecipients = null;
            updatedCampaign.ContactTags = null;
            updatedCampaign.Contacts = null;
            updatedCampaign.SearchDefinitions = null;
            //CampaignStatistics campaignStatistics = campaignRepository.GetCampaignStatistics(request.Id);
            //updatedCampaign = calculateCampaignAnalytics(updatedCampaign, campaignStatistics);
            if (updatedCampaign.CampaignStatus == CampaignStatus.Sent || updatedCampaign.CampaignStatus == CampaignStatus.Failure)
            {
                Notification notificationData = new Notification();
                notificationData.Details = "Campaign '" + updatedCampaign.Name
                    + (updatedCampaign.CampaignStatus == CampaignStatus.Sent ? "' sent successfully" : "' failed");
                notificationData.EntityId = updatedCampaign.Id;
                notificationData.Subject = "Campaign '" + updatedCampaign.Name
                    + (updatedCampaign.CampaignStatus == CampaignStatus.Sent ? "' sent successfully" : "' failed");
                notificationData.Time = DateTime.Now.ToUniversalTime();
                notificationData.Status = NotificationStatus.New;
                notificationData.UserID = updatedCampaign.CreatedBy;
                notificationData.ModuleID = (byte)AppModules.Campaigns;
                userRepository.AddNotification(notificationData);
            }
            if (!updatedCampaign.IsLinkedToWorkflows)
            {
                bool IsLinkedToWorkFlow = campaignRepository.IsworkFlowAttachedCampaign(request.Id);
                if (IsLinkedToWorkFlow && lastTouched != null && updatedCampaign.CampaignStatus == CampaignStatus.Sent)
                {
                    var messages = new List<TrackMessage>();
                    foreach (int id in ContactIDs)
                    {
                        var message = new TrackMessage()
                        {
                            EntityId = request.Id,
                            AccountId = updatedCampaign.AccountID,
                            ContactId = id,
                            LeadScoreConditionType = (int)LeadScoreConditionType.CampaignSent
                        };
                        messages.Add(message);
                    }
                    messageService.SendMessages(new Messaging.Messages.SendMessagesRequest()
                    {
                        Messages = messages
                    });
                }
            }
            updatedCampaign.HTMLContent = null;
            //if (indexingService.Update<Campaign>(updatedCampaign) > 0)
            //    Logger.Current.Informational("Campaign updated to elasticsearch successfully. CampaignId: " + updatedCampaign.Id);
        }


        public QueueCampaignResponse QueueCampaign(QueueCampaignRequest request)
        {
            Logger.Current.Verbose("Request received to send campaign with CampaignID " + request.CampaignViewModel.CampaignID);
            //int campaignId = 0;
            CampaignViewModel campaignViewModel;
            CampaignViewModel newCampaign = new CampaignViewModel();

            if (request.CampaignViewModel.ParentCampaignId == 0 && request.CampaignViewModel.Name.Length > 75)
            {
                throw new UnsupportedOperationException("[|Campaign Name Is Maximum 75 characters.|]");
            }

            if (request.CampaignViewModel.CampaignID == 0)
            {
                campaignViewModel = request.CampaignViewModel;
                newCampaign = insertCampaign(request.CampaignViewModel);
                campaignViewModel.CampaignID = newCampaign.CampaignID;
                campaignViewModel.Links = null;//revisit
            }
            else
            {
                newCampaign = updateCampaign(request.CampaignViewModel);
                campaignViewModel = request.CampaignViewModel;
                campaignViewModel.Links = newCampaign.Links;
            }

            Logger.Current.Informational("Set the campaign status to QUEUED. CampaignId: " + campaignViewModel.CampaignID);
            //}
            //}
            //Logger.Current.Informational("Campaign " + campaignViewModel.CampaignID + "queued successfully. Total recipients: " + totalRecipients);
            return new QueueCampaignResponse() { CampaignId = campaignViewModel.CampaignID };
        }

        public DeleteCampaignImageResponse DeleteCampaignImage(DeleteCampaignImageRequest request)
        {
            Logger.Current.Verbose("Request received to delete Image with ImageID " + request.Id);
            DeleteCampaignImageResponse response = new DeleteCampaignImageResponse();
            campaignRepository.DeleteCampaignImage(request.Id, request.AccountId);
            unitOfWork.Commit();
            Logger.Current.Informational("Image deleted successfully. Id: " + request.Id);
            return response;
        }

        public DeleteCampaignResponse Deactivate(DeleteCampaignRequest request)
        {
            DeleteCampaignResponse response = new DeleteCampaignResponse();
            // hasAccess(request.Id, request.RequestedBy, request.AccountId);
            var message = campaignRepository.DeleteCampaigns(request.CampaignID, (int)request.RequestedBy, request.AccountId);

            if (string.IsNullOrEmpty(message))
            {
                foreach (int campaignId in request.CampaignID)
                    indexingService.RemoveCampaign(campaignId, request.AccountId);
            }
            else
            {
                throw new UnsupportedOperationException(message);
            }
            return response;
        }

        public DeleteCampaignResponse Archive(DeleteCampaignRequest request)
        {
            DeleteCampaignResponse response = new DeleteCampaignResponse();
            // hasAccess(request.Id, request.RequestedBy, request.AccountId);
            var campaigns = campaignRepository.ArchiveCampaigns(request.CampaignID, (int)request.RequestedBy);
            Campaign campaign = new Campaign();
            if (campaigns != null)
            {
                foreach (Campaign campaignValue in campaigns)
                {
                    campaign = campaignValue;
                    campaign.HTMLContent = null;
                    //CampaignStatistics campaignStatistics = campaignRepository.GetCampaignStatistics(campaign.AccountID, campaign.Id);
                    //campaign = calculateCampaignAnalytics(campaign, campaignStatistics);

                    if (indexingService.IndexCampaign(campaign) > 0)
                        Logger.Current.Informational("Campaign indexed in to elasticsearch successfully.");
                }

            }

            return response;
        }

        public ResentCampaignResponse SaveResendCampaign(ResentCampaignRequest request)
        {
            campaignRepository.SaveResendCampaign(request.ParentCampaignId, request.CampaignId, request.CampaignResentTo);
            return new ResentCampaignResponse();
        }

        public GetResentCampaignCountResponse GetResentCampaignCount(GetResentCampaignCountRequest request)
        {
            GetResentCampaignCountResponse response = new GetResentCampaignCountResponse();
            response.Count = campaignRepository.GetResentCampaignCount(request.ParentCampaignId, request.CampaignResentTo);
            return response;
        }

        public CancelCampaignResponse CancelCampaign(CancelCampaignRequest request)
        {
            Logger.Current.Verbose("Request received to cancel campaign with id: " + request.Id);
            CancelCampaignResponse response = new CancelCampaignResponse();
            hasAccess(request.Id, request.RequestedBy, request.AccountId, request.RoleId);
            response.CanceledCampaign = campaignRepository.CancelCampaign(request.Id);
            //if (response.CanceledCampaign != null && indexingService.Index<Campaign>(response.CanceledCampaign) > 0)
            //    Logger.Current.Informational("Campaign id " + response.CanceledCampaign.Name + " indexed in to elasticsearch successfully.");
            return response;
        }

        public GetAccountCampaignImagesResponse FindAllImages(GetCampaignImagesRequest request)
        {
            GetAccountCampaignImagesResponse response = new GetAccountCampaignImagesResponse();
            IEnumerable<SD.Image> images = campaignRepository.FindAllImages(request.name, request.Limit, request.PageNumber, request.AccountID);
            IEnumerable<ImageViewModel> list = Mapper.Map<IEnumerable<SD.Image>, IEnumerable<ImageViewModel>>(images);
            if (request.AccountID.HasValue)
            {
                foreach (var image in list)
                {
                    if (image.AccountID != 0)
                        image.StorageName = urlService.GetUrl(Convert.ToInt16(image.AccountID), Entities.ImageCategory.Campaigns, image.StorageName);
                    else
                        image.StorageName = urlService.GetUrl(Entities.ImageCategory.Campaigns, image.StorageName);
                }

                response.Images = list.OrderBy(c => c.ImageID).Reverse().ToList();
                response.TotalHits = campaignRepository.FindAllImages(request.name, request.AccountID).Count();
            }
            return response;
        }

        public ReIndexDocumentResponse ReIndexCampaigns(ReIndexDocumentRequest request)
        {
            Logger.Current.Verbose("Request for reindexing campaigns.");
            int count = 0;
            GetAccountsResponse accounts = new GetAccountsResponse();
            if (request.AccountId != 0)
            {
                var arl = new List<AccountViewModel>();
                arl.Add(new AccountViewModel() { AccountID = request.AccountId });
                accounts.Accounts = arl;
            }
            else
            {
                accounts = accountService.GetAccounts();
            }
            var totaldocuments = new List<Campaign>();
            accounts.Accounts.ToList().ForEach(a =>
            {
                indexingService.SetupCampaignIndex(a.AccountID);
                Logger.Current.Verbose("Indexing campaigns account :" + a.AccountID + " .." + DateTime.Now.ToString());

                List<Campaign> documents = campaignRepository.FindAll(a.AccountID).ToList();

                documents.ForEach(c =>
                {
                    c.HTMLContent = null;
                    //CampaignStatistics campaignStatistics = campaignRepository.GetCampaignStatistics(c.AccountID, c.Id);
                    //c = calculateCampaignAnalytics(c, campaignStatistics);
                });
                totaldocuments.AddRange(documents);
                var campaignsCount = documents.Count();
                Logger.Current.Verbose("Campaigns available :" + campaignsCount);
            });
            Func<IEnumerable<Campaign>, int, IEnumerable<IEnumerable<Campaign>>> Chunk = (cts, chunkSize) =>
            {
                return cts
                    .Select((v, i) => new { v, groupIndex = i / chunkSize })
                    .GroupBy(x => x.groupIndex)
                    .Select(g => g.Select(x => x.v));
            };
            var chunkDocs = Chunk(totaldocuments, 100);

            chunkDocs.ToList().ForEach(tds =>
            {
                count = count + indexingService.ReIndexAll(tds);
            });

            return new ReIndexDocumentResponse() { Documents = count };
        }

        void isCampaignValid(Campaign campaign)
        {
            Logger.Current.Verbose("Request received to validate campaign with CampaignID " + campaign.Id);
            IEnumerable<BusinessRule> brokenRules = campaign.GetBrokenRules();

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

        void isCampaignHasCommunicationProviders(int AccountID)
        {
            Logger.Current.Verbose("Request received to find campaign has service providers or not for account id:  " + AccountID);
            GetDefaultCampaignEmailProviderRequest request = new GetDefaultCampaignEmailProviderRequest();
            request.AccountId = AccountID;
            GetDefaultCampaignEmailProviderResponse respone = communicationService.GetDefaultCampaignEmailProvider(request);
            if (respone.Exception != null)
                throw new UnsupportedOperationException("[|Service providers are not set for this Account|]");
            else if (respone.CampaignEmailProvider.LoginToken == new Guid())
            {
                throw new UnsupportedOperationException("[|Service providers are not set for this Account|]");
            }
        }

        SearchCampaignsResponse search(SearchCampaignsRequest request, IEnumerable<Type> types, bool matchAll, bool autoComplete)
        {
            SearchCampaignsResponse response = new SearchCampaignsResponse();
            SearchParameters parameters = new SearchParameters();
            parameters.Limit = request.Limit;
            parameters.PageNumber = request.PageNumber == 0 ? 1 : request.PageNumber;
            parameters.Types = types;
            parameters.MatchAll = matchAll;
            parameters.AccountId = request.AccountId;
            parameters.UserID = request.UserID;
            parameters.StartDate = request.StartDate;
            parameters.EndDate = request.EndDate;

            if (request.SortField != null)
            {
                List<string> sortFields = new List<string>();
                var maps = SmartTouch.CRM.ApplicationServices.ObjectMappers.MapperConfigurationProvider.Instance.FindTypeMapFor<CampaignViewModel, Campaign>();
                foreach (var propertyMap in maps.GetPropertyMaps())
                {
                    if (propertyMap.SourceMember != null && request.SortField.Equals(propertyMap.SourceMember.Name))
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
            SearchResult<Campaign> searchResult;

            bool isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Campaigns, request.AccountId);
            if (isPrivate && !isAccountAdmin)
            {
                int userId = (int)request.RequestedBy;
                if (request.ShowingFieldType == 0)
                {
                    searchResult = searchService.Search(request.Query, c => c.CreatedBy == userId, parameters);
                }
                else
                {
                    CampaignStatus status = (CampaignStatus)request.ShowingFieldType;
                    searchResult = searchService.Search(request.Query, c => c.CampaignStatus == status && c.CreatedBy == userId, parameters);
                }
            }
            else if (request.UserID != null && request.StartDate != null && request.EndDate != null)
            {
                int userId = (int)request.UserID;
                searchResult = searchService.Search(request.Query, c => c.CreatedBy == userId, parameters);
            }
            else
            {
                if (request.ShowingFieldType == 0)
                {
                    searchResult = searchService.Search(request.Query, c => c.CampaignStatus != CampaignStatus.Archive, parameters);
                }
                else
                {
                    CampaignStatus status = (CampaignStatus)request.ShowingFieldType;
                    searchResult = searchService.Search(request.Query, c => c.CampaignStatus == status, parameters);
                }
            }
            IEnumerable<Campaign> campaigns = searchResult.Results;

            Logger.Current.Informational("Search complete, total results:" + searchResult.Results.Count());

            if (campaigns == null)
                response.Exception = GetCampaignNotFoundException();
            else
            {
                IEnumerable<CampaignViewModel> list = Mapper.Map<IEnumerable<Campaign>, IEnumerable<CampaignViewModel>>(campaigns);
                Logger.Current.Informational("Automapper conversion is successful.");

                response.Campaigns = list;
                response.TotalHits = searchResult.TotalHits;
            }

            return response;
        }

        /// <summary>
        /// Insert an entry whenever a campaign is opened or whenever a link in a campaign is clicked for which action is applied
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public InsertCampaignOpenOrClickEntryResponse InsertCampaignOpenEntry(InsertCampaignOpenOrClickEntryRequest request)
        {
            InsertCampaignOpenOrClickEntryResponse response = new InsertCampaignOpenOrClickEntryResponse();

            CampaignRecipient campaignRecipient = campaignRepository.InsertCampaignOpenEntry(request.CampaignId, request.LinkId, request.CampaignRecipientID);
            response.Recipient = campaignRecipient;

            if (campaignRecipient != null)
            {
                if (!string.IsNullOrEmpty(request.IpAddress))
                    contactRepository.TrackContactIPAddress(campaignRecipient.ContactID, request.IpAddress, null);

                if (!request.LinkId.HasValue)
                    addToTopic(campaignRecipient.CampaignID, campaignRecipient.AccountID, campaignRecipient.ContactID, request.CampaignRecipientID);
                else
                {
                    var campaignLink = campaignRepository.GetLinkUrl(campaignRecipient.CampaignID, request.LinkId);
                    if (campaignLink != null)
                        addToTopic(campaignRecipient.CampaignID, campaignRecipient.AccountID, campaignRecipient.ContactID, campaignLink.CampaignLinkId, request.CampaignRecipientID);
                }

                accountService.ScheduleAnalyticsRefresh(campaignRecipient.CampaignID, (byte)IndexType.Campaigns);
            }
            else
            {
                Logger.Current.Informational("No Campaign recipient found");
                response = null;
            }


            return response;
        }

        Campaign calculateCampaignAnalytics(Campaign campaign, CampaignStatistics campaignStatistics)
        {
            float deliveryRate = 0;
            float openRate = 0;
            float clickRate = 0;
            float compliantRate = 0;
            int uniqueOpens = 0;
            int uniqueClicks = 0;
            int compliants = 0;
            if (campaignStatistics != null)
            {
                var campaignLinksCount = campaignStatistics.Campaign.Links.Count();
                deliveryRate = campaignStatistics.Delivered != 0 ? (float)campaignStatistics.Delivered / (float)campaignStatistics.Sent * 100 : 0; //to be changed once webhooks are integrated for campaign status for each contact.

                uniqueOpens = campaignStatistics.Opens.Select(x => new { ContactID = x.RecipientId }).Distinct().ToList().Count();
                openRate = campaignStatistics.Delivered != 0 ? (float)uniqueOpens / (float)campaignStatistics.Delivered * 100 : 0;

                uniqueClicks = campaignStatistics.Clicks.Select(x => new { ContactID = x.RecipientId }).Distinct().ToList().Count();
                clickRate = campaignStatistics.Delivered != 0 && campaignLinksCount > 0 ? (((float)uniqueClicks / (float)campaignStatistics.Delivered) * 100) : 0;

                compliants = campaignStatistics.Complained;
                compliantRate = campaignStatistics.Sent != 0 ? (float)compliants / (float)campaignStatistics.Sent * 100 : 0;
            }

            campaign.RecipientCount = campaignStatistics.RecipientCount;
            campaign.SentCount = campaignStatistics.Sent;
            campaign.DeliveredCount = campaignStatistics.Delivered;
            campaign.DeliveryRate = Math.Round(deliveryRate, 2) + "%" + " | " + campaignStatistics.Delivered;
            campaign.OpenRate = Math.Round(openRate, 2) + "%" + " | " + uniqueOpens;
            campaign.ClickRate = Math.Round(clickRate, 2) + "%" + " | " + uniqueClicks;
            campaign.UniqueClicks = uniqueClicks;
            campaign.CompliantRate = Math.Round(compliantRate, 2) + "%" + " | " + compliants;
            return campaign;
        }

        public GetCampaignStatisticsResponse GetCampaignStatistics(GetCampaignStatisticsRequest request)
        {
            Logger.Current.Verbose("Request received to fetch the campaign statistics with CampaignId: " + request.CampaignId);
            GetCampaignStatisticsResponse response = new GetCampaignStatisticsResponse();
            MailService mailService = new MailService();

            response.CampaignStatisticsViewModel = new CampaignStatisticsViewModel();
            CampaignStatistics campaignStatistics = campaignRepository.GetCampaignStatistics(request.AccountId, request.CampaignId);
            if (campaignStatistics.ServiceProviderGuid != Guid.Empty && campaignStatistics.ServiceProviderGuid != null)
            {
                var mailRegistration = mailService.GetMailRegistrationDetails(campaignStatistics.ServiceProviderGuid);
                if (!string.IsNullOrEmpty(mailRegistration.VMTA))
                    campaignStatistics.MailProvider = campaignStatistics.MailProvider + " - " + mailRegistration.VMTA;

                Logger.Current.Verbose("Campaign Sending Service Provider Name:" + campaignStatistics.MailProvider);
            }

            response.CampaignStatisticsViewModel = Mapper.Map<CampaignStatistics, CampaignStatisticsViewModel>(campaignStatistics);
            if (response.CampaignStatisticsViewModel != null)
            {
                IEnumerable<Tag> ContactTags = tagRepository.FindContactTagsByCampaign(request.CampaignId);
                IEnumerable<SearchDefinition> SavedSearches = campaignRepository.GetCampaignSearchDefinitionsMap(request.CampaignId);

                response.CampaignStatisticsViewModel.ContactTags = Mapper.Map<IEnumerable<Tag>, IList<TagViewModel>>(ContactTags);
                response.CampaignStatisticsViewModel.SearchDefinitions = Mapper.Map<IEnumerable<SearchDefinition>, IEnumerable<AdvancedSearchViewModel>>(SavedSearches).ToList();
                response.IsParentCampaign = campaignRepository.GetResentCampaignsData(request.CampaignId);
                response.CampaignStatisticsViewModel.IsLinkedToWorkflows = response.CampaignStatisticsViewModel.CampaignViewModel.IsLinkedToWorkflows;
                return response;
            }
            else
                throw new UnsupportedOperationException("The campaign you are looking for do not exist");
        }

        void addToTopic(int campaignId, int accountId, int contactId, int crid)
        {
            var message = new TrackMessage()
            {
                EntityId = campaignId,
                AccountId = accountId,
                ContactId = contactId,
                LeadScoreConditionType = (int)LeadScoreConditionType.ContactOpensEmail,
                ConditionValue = crid.ToString()
            };

            messageService.SendMessages(new Messaging.Messages.SendMessagesRequest()
            {
                Message = message
            });
        }

        void addToTopic(int campaignId, int accountId, int contactId, int linkId, int crid)
        {
            //sending two messages to make sure lick is clicked trigger performed
            var clickMessage = new TrackMessage()
            {
                EntityId = campaignId,
                AccountId = accountId,
                ContactId = contactId,
                LeadScoreConditionType = (int)LeadScoreConditionType.ContactClicksLink,
                ConditionValue = campaignId.ToString()
            };
            var message = new TrackMessage()
            {
                EntityId = campaignId,
                AccountId = accountId,
                ContactId = contactId,
                LeadScoreConditionType = (int)LeadScoreConditionType.ContactClicksLink,
                LinkedEntityId = linkId,
                ConditionValue = crid.ToString()
            };
            var messages = new List<TrackMessage>();
            messages.Add(clickMessage);
            messages.Add(message);
            messageService.SendMessages(new Messaging.Messages.SendMessagesRequest()
            {
                Messages = messages
            });
        }

        public GetLinkUrlResponse GetLinkURl(GetLinkUrlRequest request)
        {
            GetLinkUrlResponse response = new GetLinkUrlResponse();

            response.CampaignLinkViewModel = new CampaignLinkViewModel();
            CampaignLink campaignLink = campaignRepository.GetLinkUrl(request.CampaignId, request.LinkIndex);
            response.ReferenceId = (Guid)contactRepository.GetContactReferenceId(request.ContactId);
            response.CampaignLinkViewModel = Mapper.Map<CampaignLink, CampaignLinkViewModel>(campaignLink);
            if (campaignLink != null && campaignLink.URL != null && !string.IsNullOrEmpty(campaignLink.URL.URL))
            {
                response.MergeFields = ExtractMergeFieldsFromURL(response.CampaignLinkViewModel.URL.URL);
                if (response.MergeFields.Any())
                {
                    var contactPrimitiveValues = campaignRepository.GetContactMergeFields(response.MergeFields, request.ContactId);
                    foreach (string field in response.MergeFields)
                    {
                        var mergeFieldValue = contactPrimitiveValues.Where(c => c.FieldName.ToLower() == field.ToLower()).Select(c => c.FieldValue).FirstOrDefault() ?? "";
                        response.CampaignLinkViewModel.URL.URL = response.CampaignLinkViewModel.URL.URL.Replace("*|" + field + "|*", mergeFieldValue);
                    }
                    Logger.Current.Informational("Merge field URL: " + response.CampaignLinkViewModel.URL.URL);

                }
            }
            return response;
        }

        private IEnumerable<string> ExtractMergeFieldsFromURL(string url)
        {
            Logger.Current.Verbose("Fetching contact merge fields if any.");
            var subStrings = url.Replace("*|", "{[").Replace("|*", "}");
            var splitFields = subStrings.Split('{', '}');
            var filteredFields = new List<string>();
            foreach (string field in splitFields)
                if (field.IndexOf('[') == 0)
                    filteredFields.Add(field.Replace("[", ""));
            Logger.Current.Informational(filteredFields.Count() + " merge fields found");
            return filteredFields;
        }
        public GetCampaignStatusResponse GetCampaignStatus(GetCampaignStatusRequest request)
        {
            GetCampaignStatusResponse response = new GetCampaignStatusResponse();
            Logger.Current.Verbose("Request received for fetching campaign status");
            if (request != null)
            {
                CampaignStatus? campaignStatus = campaignRepository.GetCampaignStatus(request.campaignId);
                response.campaignStatus = campaignStatus;
                Logger.Current.Informational("Campaign status received is :" + campaignStatus);
            }
            return response;
        }

        public GetLinkUrlsResponse GetCampaignLinks(GetLinkUrlRequest request)
        {
            GetLinkUrlsResponse response = new GetLinkUrlsResponse();
            IEnumerable<CampaignLink> campaignLinks = campaignRepository.GetCampaignLinkUrls(request.CampaignIDs);
            IEnumerable<CampaignLinkViewModel> linksList = Mapper.Map<IEnumerable<CampaignLink>, IEnumerable<CampaignLinkViewModel>>(campaignLinks);
            response.CampaignLinks = linksList;
            return response;
        }

        public void UpdateCampaignRecipientOptOutStatus(int contactId, int campaignId, int workflowId)
        {
            campaignRepository.UpdateCampaignRecipientoptOutStatus(campaignId, contactId, workflowId);
        }

        public MailChimpWebhookResponse MailChimpWebhookUpdate(MailChimpWebhookRequest request)
        {
            Logger.Current.Verbose("Request received to update MailChimp webhook. " + request.CampaignResponse.ResponseItems.ToString());
            MailChimpWebhookResponse response = new MailChimpWebhookResponse();

            var updateType = request.CampaignResponse.ResponseItems.Where(c => c.Key == "type").FirstOrDefault();
            var dataReason = request.CampaignResponse.ResponseItems.Where(c => c.Key == "data[reason]").FirstOrDefault();
            var firedAt = request.CampaignResponse.ResponseItems.Where(c => c.Key == "fired_at").FirstOrDefault();
            var campaignRecipientId = request.CampaignResponse.ResponseItems.Where(c => c.Key == "data[merges][CRID]").FirstOrDefault();
            var email = request.CampaignResponse.ResponseItems.Where(c => c.Key == "data[email]").FirstOrDefault();
            var mailChimpCampaignId = request.CampaignResponse.ResponseItems.Where(c => c.Key == "data[campaign_id]").FirstOrDefault();
            var campaignRecipient = new CampaignRecipient();
            if (!string.IsNullOrEmpty(campaignRecipientId.Value))
                campaignRecipient = campaignRepository.GetCampaignRecipient(int.Parse(campaignRecipientId.Value), request.AccountId);
            else
                campaignRecipient = campaignRepository.GetCampaignRecipient(mailChimpCampaignId.Value, email.Value);

            var timeLogged = DateTime.Parse(firedAt.Value);
            if (updateType.Value == "unsubscribe" && dataReason.Value != "abuse")
            {
                Email contactEmail = contactRepository.UpdateContactEmail(campaignRecipient.ContactID, campaignRecipient.To, EmailStatus.UnSubscribed, campaignRecipient.AccountID, null);
                campaignRepository.UpdateCampaignRecipientStatus(
                            campaignRecipient.CampaignRecipientID
                            , (CampaignDeliveryStatus)campaignRecipient.DeliveryStatus
                            , (DateTime)campaignRecipient.DeliveredOn, "Unsubscribed", null, timeLogged, campaignRecipient.AccountID, (short?)CampaignOptOutStatus.Unsubscribed);
                if (contactEmail != null)
                {
                    Logger.Current.Informational("Contact email status updated to 'unsubscribe' successfully. Email:" + contactEmail.EmailId);
                }
            }
            else if (updateType.Value == "subscribe")
            {
                Email contactEmail = contactRepository.UpdateContactEmail(campaignRecipient.ContactID, campaignRecipient.To, EmailStatus.Verified, campaignRecipient.AccountID, null);
                Logger.Current.Informational("Contact email status updated to 'subscribe' successfully. Email:" + contactEmail.EmailId);
            }
            else if (updateType.Value == "cleaned" || dataReason.Value == "abuse")
            {
                var emailStatus = dataReason.Value == "hard" ? EmailStatus.HardBounce : EmailStatus.Complained;

                if (campaignRecipient != null && campaignRecipient.ContactID > 0)
                {
                    Email contactEmail = contactRepository.UpdateContactEmail(campaignRecipient.ContactID, campaignRecipient.To, emailStatus, campaignRecipient.AccountID, null);
                    Logger.Current.Informational("Contact email status updated to 'cleaned' successfully. Email:" + contactEmail.EmailId + " and Recipient: " + campaignRecipient.To);

                    Campaign campaign = campaignRepository.FindBy(campaignRecipient.CampaignID);


                    if (dataReason.Value == "hard")
                    {
                        Logger.Current.Informational("Updating recipient status. Email:"
                            + contactEmail.EmailId + " and Recipient: " + campaignRecipient.To);
                        campaignRepository.UpdateCampaignRecipientDeliveryStatus(campaignRecipient.CampaignRecipientID, CampaignDeliveryStatus.HardBounce, campaignRecipient.AccountID);

                        Logger.Current.Informational("Recipient status updated as hard bounce successfully. Email:"
                            + contactEmail.EmailId + " and Recipient: " + campaignRecipient.To);


                    }
                    else if (dataReason.Value == "abuse")
                    {
                        campaignRepository.UpdateCampaignRecipientStatus(
                           campaignRecipient.CampaignRecipientID
                            , (CampaignDeliveryStatus)campaignRecipient.DeliveryStatus
                            , (DateTime)campaignRecipient.DeliveredOn, "Marked as spam", null, timeLogged, campaignRecipient.AccountID, (short?)CampaignOptOutStatus.Abuse);
                    }

                    //CampaignStatistics campaignStatistics = campaignRepository.GetCampaignStatistics(request.AccountId, campaignRecipient.CampaignID);
                    //campaign = calculateCampaignAnalytics(campaign, campaignStatistics);

                    Logger.Current.Informational("Campaign analytics updated successfully. CampaignID:" + campaignRecipient.CampaignID);

                    //if (indexingService.Index<Campaign>(campaign) > 0)
                    //    Logger.Current.Informational("Campaign indexed in to elasticsearch successfully. CampaignId: " + campaignRecipient.CampaignID);
                }
            }
            return response;
        }

        public UpdateCampaignDeliveryStatusResponse UpdateCampaignDeliveryStatus(UpdateCampaignDeliveryStatusRequest request)
        {
            UpdateCampaignDeliveryStatusResponse response = new UpdateCampaignDeliveryStatusResponse();

            var id = request.DataReceived.Where(d => d.Key == "data[id]").Select(d => d.Value).FirstOrDefault();
            var status = request.DataReceived.Where(d => d.Key == "data[status]").Select(d => d.Value).FirstOrDefault();
            var deliveryDate = request.DataReceived.Where(d => d.Key == "fired_at").Select(d => d.Value).FirstOrDefault();
            var reason = request.DataReceived.Where(d => d.Key == "data[reason]").Select(d => d.Value).FirstOrDefault();

            CampaignDeliveryStatus campaignDeliveryStatus = status == "sent" ? CampaignDeliveryStatus.Delivered : CampaignDeliveryStatus.SoftBounce;
            DateTime campaignDeliveryDate = DateTime.Parse(deliveryDate);

            Campaign updatedCampaign = campaignRepository.UpdateCampaignDeliveryStatus(id, campaignDeliveryStatus, campaignDeliveryDate, reason);
            //CampaignStatistics campaignStatistics = campaignRepository.GetCampaignStatistics(updatedCampaign.AccountID, updatedCampaign.Id);
            //updatedCampaign = calculateCampaignAnalytics(updatedCampaign, campaignStatistics);
            updatedCampaign.CampaignRecipients = null; //To avoid self reference error while indexing.
            updatedCampaign.HTMLContent = null;
            //if (indexingService.Index<Campaign>(updatedCampaign) > 0)
            //    Logger.Current.Informational("Campaign delivery status updated and indexed in to elasticsearch successfully. CampaignId: " + updatedCampaign.Id);


            return response;
        }

        public GetClientIPAddressResponse GetClientIPAddress(GetClientIPAddressRequest request)
        {
            GetClientIPAddressResponse response = new GetClientIPAddressResponse();
            HttpRequest currentRequest = HttpContext.Current.Request;
            String clientIP = currentRequest.ServerVariables["REMOTE_ADDR"];
            response.IPAddress = clientIP;
            return response;
        }

        public GetCampaignRecipientResponse GetCampaignRecipient(GetCampaignRecipientRequest request)
        {
            Logger.Current.Verbose(string.Format("In CampaignService/GetCampaignRecipient accountid: {0}, crid: {1}", request.AccountId, request.CampaignRecipientId));
            GetCampaignRecipientResponse response = new GetCampaignRecipientResponse();
            if (request.AccountId > 0)
                response.CampaignRecipient = campaignRepository.GetCampaignRecipient(request.CampaignRecipientId, request.AccountId);
            else
                response.CampaignRecipient = campaignRepository.GetCampaignRecipient(request.CampaignRecipientId, request.AccountId);

            return response;
        }

        public InsertCampaignRecipientResponse InsertCampaignRecipients(InsertCampaignRecipientRequest request)
        {
            InsertCampaignRecipientResponse response = new InsertCampaignRecipientResponse();

            if (request.ContactId != 0 && request.CampaignId != 0 && request.WorkflowId != 0 && request.AccountId != 0)
                campaignRepository.UpdateAutomationCampaignRecipients(request.ContactId, request.CampaignId, request.WorkflowId, request.AccountId);

            return response;
        }

        public GetNextAutomationCampaignRecipientsResponse GetNextAutomationCampaignRecipients(GetNextAutomationCampaignRecipientsRequest request)
        {
            GetNextAutomationCampaignRecipientsResponse response = new GetNextAutomationCampaignRecipientsResponse();

            IEnumerable<CampaignRecipient> recipients = new List<CampaignRecipient>();
            recipients = campaignRepository.GetNextAutomationCampaignRecipients();

            IEnumerable<Contact> contacts = contactRepository.FindAll(recipients.Select(p => p.ContactID).ToList(), false);
            Logger.Current.Verbose(string.Format("Found {0} contacts", contacts.Count()));
            foreach (CampaignRecipient rcp in recipients)
            {
                Logger.Current.Verbose(string.Format("ContactID:{0}", rcp.ContactID));
                rcp.Contact = contacts.Where(p => p.Id == rcp.ContactID).FirstOrDefault();
            }
            Logger.Current.Verbose(string.Format("Returning recipients: {0}", recipients.Count()));
            response.AutomationCampaignRecipients = recipients;

            return response;
        }

        public GetAutomationCampaignResponse GetAutomationCampaigns(GetAutomationCampaignRequest request)
        {
            GetAutomationCampaignResponse response = new GetAutomationCampaignResponse();

            response.Campaigns = campaignRepository.GetAutomationCampaigns(request.CampaignIds);

            return response;
        }

        public GetCampaignsSeekingAnalysisResponse GetCampaignsSeekingAnalysis(GetCampaignsSeekingAnalysisRequest request)
        {
            GetCampaignsSeekingAnalysisResponse response = new GetCampaignsSeekingAnalysisResponse();
            response.Campaigns = campaignRepository.GetCampaignIdsSeekingAnalysis();
            return response;
        }

        public GetServiceProviderSenderEmailResponse GetDefaultBulkEmailProvider(GetServiceProviderSenderEmailRequest request)
        {
            GetServiceProviderSenderEmailResponse response = new GetServiceProviderSenderEmailResponse();

            GetDefaultCampaignEmailProviderResponse emailProvider =
                communicationService.GetDefaultCampaignEmailProvider(new GetDefaultCampaignEmailProviderRequest() { AccountId = request.AccountId });

            response.SenderEmail = accountService.GetDefaultBulkEmailProvider(new GetServiceProviderSenderEmailRequest()
            {
                ServiceProviderID = emailProvider.CampaignEmailProvider.CommunicationLogID,
                AccountId = request.AccountId
            }).SenderEmail;

            var serviceProvider = emailProvider.CampaignEmailProvider;
            MailService mailService = new MailService();
            MailRegistrationDb mailRegistration = mailService.GetMailRegistrationDetails(serviceProvider.LoginToken);
            response.SenderName = mailRegistration.Name;
            if (response.SenderEmail != null)
            {
                response.SenderEmail.MailProviderID = (byte)mailRegistration.MailProviderID;
            }
            return response;
        }

        public GetServiceProviderSenderEmailResponse GetEmailProviderById(GetServiceProviderSenderEmailRequest request)
        {
            GetServiceProviderSenderEmailResponse response = new GetServiceProviderSenderEmailResponse();

            GetServiceProviderByIdResponse emailProvider =
                communicationService.GetEmailProviderById(new GetServiceProviderByIdRequest() { AccountId = request.AccountId, ServiceProviderId = request.ServiceProviderID });

            response.SenderEmail = accountService.GetDefaultBulkEmailProvider(new GetServiceProviderSenderEmailRequest()
            {
                ServiceProviderID = emailProvider.CampaignEmailProvider.CommunicationLogID,
                AccountId = request.AccountId
            }).SenderEmail;

            var serviceProvider = communicationService
                .GetDefaultCampaignEmailProvider(new GetDefaultCampaignEmailProviderRequest() { AccountId = request.AccountId }).CampaignEmailProvider;
            MailService mailService = new MailService();
            MailRegistrationDb mailRegistration = mailService.GetMailRegistrationDetails(serviceProvider.LoginToken);
            response.SenderName = mailRegistration.Name;
            if (response.SenderEmail != null)
            {
                response.SenderEmail.MailProviderID = (byte)mailRegistration.MailProviderID;
            }
            return response;
        }

        public GetCampaignThemesResponse GetCampaignThemes(GetCampaignThemesRequest request)
        {
            GetCampaignThemesResponse response = new GetCampaignThemesResponse();

            IEnumerable<CampaignTheme> campaignThemes = campaignRepository.GetCampaignThemes((int)request.RequestedBy, request.AccountId);
            response.Themes = Mapper.Map<IEnumerable<CampaignTheme>, IEnumerable<CampaignThemeViewModel>>(campaignThemes);

            return response;
        }

        public SendTestEmailResponse SendTestEmail(SendTestEmailRequest request)
        {
            Logger.Current.Verbose("Request received by SendTestEmail method to send email for account id: " + request.AccountId);
            SendTestEmailResponse sendTestEmailResponse = new SendTestEmailResponse();
            MailService mailService = new MailService();
            request.MailViewModel.Subject = string.IsNullOrEmpty(request.MailViewModel.Subject) ? "No subject" : request.MailViewModel.Subject;

            var emailProviderResponse = new GetDefaultCampaignEmailProviderResponse();
            GetServiceProviderSenderEmailResponse providerSenderEmailResponse = new GetServiceProviderSenderEmailResponse();

            if (request.ServiceProviderID == null)
            {
                Logger.Current.Verbose("Sending test email from default service provider set to account: " + request.AccountId);
                emailProviderResponse.CampaignEmailProvider
                    = communicationService.GetDefaultCampaignEmailProvider(new GetDefaultCampaignEmailProviderRequest()
                    {
                        AccountId = request.AccountId
                    }).CampaignEmailProvider;
                Logger.Current.Informational("Service provider: " + emailProviderResponse.CampaignEmailProvider.LoginToken);
                providerSenderEmailResponse = GetDefaultBulkEmailProvider(new GetServiceProviderSenderEmailRequest()
                {
                    AccountId = request.AccountId,
                    RequestedBy = request.RequestedBy
                });
            }
            else
            {
                Logger.Current.Verbose("Sending test email from default service provider set to account: " + request.AccountId);
                emailProviderResponse.CampaignEmailProvider
                    = communicationService.GetEmailProviderById(new GetServiceProviderByIdRequest()
                    {
                        AccountId = request.AccountId,
                        ServiceProviderId = (int)request.ServiceProviderID
                    }).CampaignEmailProvider;
                Logger.Current.Informational("Service provider: " + emailProviderResponse.CampaignEmailProvider.LoginToken);
                providerSenderEmailResponse = GetEmailProviderById(new GetServiceProviderSenderEmailRequest()
                {
                    AccountId = request.AccountId,
                    RequestedBy = request.RequestedBy,
                    ServiceProviderID = (int)request.ServiceProviderID
                });
            }
            var serviceProvider = emailProviderResponse.CampaignEmailProvider;
            var accountDomain = serviceProvider.Account.DomainURL;
            var accountAddress = "";
            if (serviceProvider.Account.Addresses != null && serviceProvider.Account.Addresses.Any())
            {
                var primaryAddress = serviceProvider.Account.Addresses.Where(a => a.IsDefault == true).FirstOrDefault();
                if (primaryAddress != null)
                    accountAddress = primaryAddress.ToString();
            }
            MailRegistrationDb mailRegistration = mailService.GetMailRegistrationDetails(serviceProvider.LoginToken);
            var emailRecipients = new List<EmailRecipient>();
            if (request.MailViewModel.To != null && request.MailTesterGuid == new Guid())
            {
                var toEmails = request.MailViewModel.To;
                string[] toEmailList = toEmails.Split(',');
                foreach (var emailId in toEmailList)
                {
                    if (!string.IsNullOrEmpty(emailId))
                        emailRecipients.Add(new EmailRecipient() { EmailId = emailId });
                }
            }
            else if (request.MailViewModel.BCC != null && request.MailTesterGuid == new Guid())
            {
                var Emails = request.MailViewModel.BCC;
                string[] EmailList = Emails.Split(',');
                foreach (var Email in EmailList)
                {
                    emailRecipients.Add(new EmailRecipient() { EmailId = Email });
                }
            }
            else if (request.MailViewModel.To != null && request.MailTesterGuid != new Guid())
            {
                emailRecipients.Add(new EmailRecipient() { EmailId = request.MailViewModel.To });
                request.MailViewModel.SenderName = emailProviderResponse.CampaignEmailProvider.ProviderName;
            }
            if (mailRegistration != null)
            {
                if (mailRegistration.MailProviderID == LM.MailProvider.MailChimp)
                {
                    Logger.Current.Verbose("Test email is being sent by MailChimp. RegistrationID: " + mailRegistration.MailRegistrationID);
                    var mc = new MailChimpCampaign(mailRegistration.APIKey);
                    IEnumerable<Company> companies = new List<Company>() { };
                    string mailChimpCampaignId = mc.SendCampaign(0, "Test Email", emailRecipients, companies, request.MailViewModel.Subject, request.MailViewModel.Subject,
                        request.MailViewModel.Body, request.MailViewModel.From, request.MailViewModel.SenderName, null, request.AccountId, request.MailViewModel.CampaignTypeId, null);
                    Logger.Current.Informational("MailChimp campaignId : " + mailChimpCampaignId);
                }
                else if (mailRegistration.MailProviderID == LM.MailProvider.SmartTouch)
                {
                    Logger.Current.Verbose("Test email is being sent by VMTA. RegistrationID: " + mailRegistration.MailRegistrationID);
                    var content = string.Empty;
                    var imageHostingUrl = ConfigurationManager.AppSettings["IMAGE_HOSTING_SERVICE_URL"].ToString();
                    var imageDomain = serviceProvider.ImageDomain;

                    var vmta = new VMTACampaign(mailRegistration.VMTA, mailRegistration.UserName, mailRegistration.Password, mailRegistration.Host, mailRegistration.Port);

                    if (imageHostingUrl != null && imageDomain != null && imageDomain.Status != false && !string.IsNullOrEmpty(imageDomain.Domain))
                    {
                        mailRegistration.ImageDomain = serviceProvider.ImageDomain.Domain;
                        content = request.MailViewModel.Body.Replace(imageHostingUrl, serviceProvider.ImageDomain.Domain);

                        var index = serviceProvider.ImageDomain.Domain.IndexOf("//");
                        var imageDomainProtocol = serviceProvider.ImageDomain.Domain.Substring(0, serviceProvider.ImageDomain.Domain.IndexOf("://") + 3);

                        var dotCount = serviceProvider.ImageDomain.Domain.Count(d => d == '.');
                        var linkDomain = serviceProvider.ImageDomain.Domain;
                        if (index >= 0 && dotCount == 1)
                        {
                            linkDomain = serviceProvider.ImageDomain.Domain.Replace(imageDomainProtocol, imageDomainProtocol + serviceProvider.AccountCode + ".");
                            content = content.Replace("http://" + accountDomain, linkDomain).Replace("https://" + accountDomain, linkDomain);
                        }

                    }
                    else
                        content = request.MailViewModel.Body;
                    IEnumerable<FieldValueOption> customFieldsValueOptions = new List<FieldValueOption>();
                    IEnumerable<Company> companies = new List<Company>() { };

                    var vmtaEmail = providerSenderEmailResponse.SenderEmail != null ? providerSenderEmailResponse.SenderEmail.EmailId : "";
                    Logger.Current.Verbose("Vmta email:  " + vmtaEmail);

                    var successfulRecipients = vmta.SendCampaignFromScheduler(0, "Test Campaign", null, null
                        , emailRecipients, companies, customFieldsValueOptions, request.MailViewModel.Subject, request.MailViewModel.Subject, content, request.MailViewModel.From,
                        request.MailViewModel.SenderName, emailProviderResponse.CampaignEmailProvider.AccountCode, mailRegistration.SenderDomain, accountDomain, vmtaEmail, accountAddress, request.AccountId, request.MailViewModel.CampaignTypeId, mailRegistration, request.HasDisCliamer);

                    if (successfulRecipients != null && successfulRecipients.Any())
                        sendTestEmailResponse.Exception = null;
                    else
                        sendTestEmailResponse.Exception = new Exception("Unable to send test email.");
                }
                else if (mailRegistration.MailProviderID == LM.MailProvider.Smtp)
                {
                    Logger.Current.Verbose("Test email is being sent by SMTP. RegistrationID: " + mailRegistration.MailRegistrationID);
                    var smtp = new SmtpMailService(mailRegistration);
                    smtp.Send(new LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest()
                    {
                        CC = emailRecipients.Select(r => r.EmailId).ToList(),
                        From = request.MailViewModel.From,
                        DisplayName = request.MailViewModel.SenderName,
                        Subject = request.MailViewModel.Subject,
                        IsBodyHtml = true,
                        Body = request.MailViewModel.Body,
                        CampaignReceipients = emailRecipients
                    });
                }
            }
            return sendTestEmailResponse;
        }

        public InsertCampaignTemplateResponse InsertCampaignTemplate(InsertCampaignTemplateRequest request)
        {
            string htmlContent = "<div id='htmlcontent_R' style='max-width:700px; margin:0 auto;font-family:Calibri;background-color:#fff;padding:10px;' class='layouts'>" + request.CampaignTemplateViewModel.HTMLContent + "</div>";

            Boolean IsDuplicate = campaignRepository.IsDuplicateCampaignLayout(request.CampaignTemplateViewModel.Name, request.AccountId);
            if (IsDuplicate)
                throw new UnsupportedOperationException("[|Campaign Template with this name already exists.|]");


            var imagePhysicalPath = ConfigurationManager.AppSettings["CAMPAIGN_LAYOUT_TEMPLATES_PHYSICAL_PATH"].ToString();
            string newGuid = Guid.NewGuid().ToString();
            var path = imagePhysicalPath + newGuid + ".png";

            ImageViewModel imageViewModel = new ImageViewModel();
            imageViewModel.AccountID = request.AccountId;
            imageViewModel.ImageContent = request.CampaignTemplateViewModel.ImageContent;
            imageViewModel.ImageType = request.CampaignTemplateViewModel.ImageType;
            imageViewModel.OriginalName = request.CampaignTemplateViewModel.OriginalName;
            imageViewModel.StorageName = newGuid;
            Image Image;
            if (!System.IO.Directory.Exists(imagePhysicalPath))
            {
                System.IO.Directory.CreateDirectory(imagePhysicalPath);
            }
            if (System.IO.Directory.Exists(imagePhysicalPath))
            {
                string imageContent = imageViewModel.ImageContent.Split(',')[1];
                byte[] imageData = Convert.FromBase64String(imageContent);
                using (MemoryStream ms = new MemoryStream(imageData))
                {
                    Image = Image.FromStream(ms);
                    Image.Save(path);
                }

            }
            SD.Image thumbnailimage = new SD.Image()
            {
                Id = 0,
                ImageCategoryID = ImageCategory.CampaignTemplateThumbnails,
                AccountID = request.AccountId,
                FriendlyName = newGuid,
                OriginalName = imageViewModel.OriginalName,
                StorageName = newGuid + ".png",
            };
            int ThumnailImageId = campaignRepository.InsertCampaignImage(thumbnailimage);

            CampaignTemplate template = new CampaignTemplate()
            {
                AccountId = request.AccountId,
                CreatedBy = request.RequestedBy.Value,
                Name = request.CampaignTemplateViewModel.Name,
                HTMLContent = htmlContent,
                ThumbnailImageId = ThumnailImageId,
                Type = CampaignTemplateType.Custom,
                Description = ""
            };
            campaignRepository.InsertCampaignTemplates(template);
            InsertCampaignTemplateResponse response = new InsertCampaignTemplateResponse();
            return response;
        }



        public void ConvertHtmlToImage(string htmlContent)
        {
            Bitmap m_Bitmap = new Bitmap(400, 600);
            //PointF point = new PointF(0, 0);
            //SizeF maxSize = new System.Drawing.SizeF(500, 500);

            //    HtmlRender.Render(Graphics.FromImage(m_Bitmap), htmlContent, point, maxSize);

            //HtmlRender.HtmlRender.Render(Graphics.FromImage(m_Bitmap),
            //                                        "<html><body><p>This is a shitty html code</p>"
            //                                        + "<p>This is another html line</p></body>",
            //                                         point, maxSize);

            m_Bitmap.Save(@"C:\Test.png", SDI.ImageFormat.Png);
        }

        public void UpdateCampaignStatistics(GetCampaignRequest request)
        {

            Campaign campaign = campaignRepository.FindBy(request.Id);
            if (campaign != null)
            {
                Logger.Current.Verbose("Request received to update campaign statistics for campaign : " + request.Id);
                //CampaignStatistics campaignStatistics = campaignRepository.GetCampaignStatistics(request.Id);
                //campaign = calculateCampaignAnalytics(campaign, campaignStatistics);
                //if (indexingService.Index<Campaign>(campaign) > 0)
                //    Logger.Current.Informational("Campaign statistics is updated and indexed into elastic search successfully. CampaignId: " + campaign.Id);
            }
        }


        public GetCampaignTemplatesResponse GetCampaignTemplatesForEmails(GetCampaignTemplatesRequest request)
        {
            GetCampaignTemplatesResponse response = new GetCampaignTemplatesResponse();
            Logger.Current.Verbose("Request received to fetch campaign templates based on request");
            var campaignTemplates = campaignRepository.GetTemplatesForContactEmails(request.AccountId);
            response.Templates = Mapper.Map<IEnumerable<CampaignTemplate>, IEnumerable<CampaignTemplateViewModel>>(campaignTemplates);
            foreach (CampaignTemplateViewModel campaignTemplateVM in response.Templates.Where(i => i.Type != CampaignTemplateType.SentCampaigns))
            {
                CampaignTemplate campaignTemplate = campaignTemplates.Where(c => c.Id == campaignTemplateVM.TemplateId).FirstOrDefault();
                campaignTemplateVM.ThumbnailImageUrl = urlService.GetUrl(campaignTemplate.ThumbnailImage.ImageCategoryID, campaignTemplate.ThumbnailImage.StorageName);
            }
            return response;
        }

        public GetCampaignTemplateResponse GetCampaignTemplateHTML(GetCampaignTemplateRequest request)
        {
            GetCampaignTemplateResponse response = new GetCampaignTemplateResponse();
            Logger.Current.Verbose("Request received to fetch campaign template " + request.CampaignTemplateID);
            var campaignTemplate = campaignRepository.GetTemplateHTML(request.CampaignTemplateID, request.TemplateType);
            response.HTMLContent = campaignTemplate.Replace("[IMAGES_URL]", urlService.GetImageHostingUrl()).Replace("*|CRID|*", "0").Replace("*|CAMPID|*", request.CampaignTemplateID.ToString());
            return response;
        }

        public void UpdateCampaignRecipientsStatus(UpdateCampaignRecipientsStatusRequest request)
        {
            campaignRepository.UpdateCampaignRecipientsStatus(request.CampaignRecipientIDs, request.Remarks, request.DeliveredOn, request.SentOn, request.DeliveryStatus);
        }

        public GetCampaignSocialMediaPostResponse GetCampaignPosts(GetCampaignSocialMediaPostRequest request)
        {
            return new GetCampaignSocialMediaPostResponse()
            {
                Posts = campaignRepository.GetPosts(request.CampaignId, request.UserId, request.CommunicationType)
            };
        }

        public CampaignIndexingResponce CampaignIndexing(CampaignIndexingRequest request)
        {
            CampaignIndexingResponce responce = new CampaignIndexingResponce();
            foreach (var id in request.CampaignIds)
            {
                this.UpdateCampaignStatistics(new GetCampaignRequest(id));
                //Campaign campaign = campaignRepository.FindBy(id);
                //if (indexingService.Index<Campaign>(campaign) > 0)
                //    Logger.Current.Informational("Campaign indexed in to elasticsearch successfully.");
            }
            return responce;
        }

        public GetUniqueRecipientsCountResponse GetUniqueRecipients(GetUniqueRecipientsCountRequest request)
        {
            var sdContacts = new List<SearchDefinitionContact>();
            List<SavedSearchActiveContacts> ssContacts = new List<SavedSearchActiveContacts>();
            List<TagActiveContacts> tagContacts = new List<TagActiveContacts>();

            var groupId = Guid.NewGuid();
            var timer = new Stopwatch();
            timer.Start();
            bool isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);
            if (request.SDefinitions.IsAny())
            {
                foreach (var sd in request.SDefinitions)
                {
                    var sdRequest = new GetSavedSearchContactIdsRequest()
                    {
                        AccountId = request.AccountId,
                        RequestedBy = request.RequestedBy.Value,
                        RoleId = (short)request.RoleId,
                        SearchDefinitionId = sd,
                        IsActiveContactsSearch = true
                    };
                    var task = Task.Run(() => advancedSearchService.GetActiveContactIds(sdRequest));
                    var contacts = task.Result;
                    var ca = contacts.ToArray();
                    var ssdContacts = ca.Select(c => new SavedSearchActiveContacts()
                    {
                        SearchDefinitionId = sd,
                        ContactID = c.Id,
                        IsActive = c.IsActive
                    });
                    ssContacts.AddRange(ssdContacts);
                }
            }
            if (request.Tags.IsAny())
            {
                foreach (var tag in request.Tags)
                {
                    SearchDefinition sd = new SearchDefinition();
                    sd.AccountID = request.AccountId;
                    sd.IsAggregationNeeded = false;
                    sd.PredicateType = SearchPredicateType.And;
                    sd.Filters = new List<SearchFilter>() { new SearchFilter() { FieldOptionTypeId = 11, Field = ContactFields.ContactTag, Qualifier = SearchQualifier.Is, SearchText = tag.ToString() } };
                    SearchParameters parameters = new SearchParameters();
                    parameters.PageNumber = 1;
                    parameters.Limit = 500000;
                    parameters.Fields = new List<ContactFields>() { ContactFields.ContactId, ContactFields.IsActive };
                    bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, request.AccountId);
                    Logger.Current.Informational("Is Account Admin : " + isAccountAdmin.ToString());
                    Logger.Current.Informational("Is Contacts in private mode: " + isPrivate.ToString());
                    Logger.Current.Informational("RoleId: " + request.RoleId);
                    Logger.Current.Informational("AccountID: " + request.AccountId);

                    if (isPrivate && !isAccountAdmin)
                    {
                        parameters.IsPrivateSearch = true;
                        parameters.DocumentOwnerId = request.RequestedBy;
                    }
                    else
                        parameters.IsPrivateSearch = false;
                    parameters.IsActiveContactsSearch = true;
                    var task = Task.Run(() => contactSearchService.AdvancedSearchAsync(null, sd, parameters));
                    var contacts = task.Result;
                    var tagContact = contacts.Results.Select(s => new TagActiveContacts()
                    {
                        ContactID = s.Id,
                        IsActive = ((Person)s).IsActive,
                        TagID = tag
                    });
                    tagContacts.AddRange(tagContact);
                }
            }

            IEnumerable<CampaignUniqueRecipient> recipientsCount = new List<CampaignUniqueRecipient>();
            Logger.Current.Informational("Time taken to get recipients count From SP: " + timer.Elapsed);
            GetUniqueRecipientsCountResponse response = new GetUniqueRecipientsCountResponse();
            response.TagCounts = new Dictionary<int, int>();
            response.SDefinitionCounts = new Dictionary<int, int>();
            IEnumerable<int> tagAllContactIds = new List<int>();
            IEnumerable<int> tagActiveContactIds = new List<int>();
            IEnumerable<int> ssAllContactIds = new List<int>();
            IEnumerable<int> ssActiveContactIds = new List<int>();

            if (tagContacts.IsAny())
            {
                var tagsGroup = tagContacts.GroupBy(g => g.TagID);
                if (tagsGroup.IsAny())
                {
                    tagsGroup.Each(e =>
                    {
                        response.TagCounts.Add(e.Key, e.Count());
                    });
                }
                var tagAllContactsGroup = tagContacts.GroupBy(g => g.ContactID);
                response.TagAllCount = tagAllContactsGroup.Count();
                tagAllContactIds = tagAllContactsGroup.Select(s => s.Key);

                var tagActiveContactsGroup = tagContacts.Where(w => w.IsActive).GroupBy(g => g.ContactID);
                response.TagActiveCount = tagActiveContactsGroup.Count();
                tagActiveContactIds = tagActiveContactsGroup.Select(s => s.Key);
            }
            if (ssContacts.IsAny())
            {
                var ssGroup = ssContacts.GroupBy(g => g.SearchDefinitionId);
                if (ssGroup.IsAny())
                    ssGroup.Each(e =>
                    {
                        response.SDefinitionCounts.Add(e.Key, e.Count());
                    });
                var sdAllContactsGroup = ssContacts.GroupBy(g => g.ContactID);
                response.SDefinitionAllCount = sdAllContactsGroup.Count();
                ssAllContactIds = sdAllContactsGroup.Select(s => s.Key);

                var sdActiveContactsGroup = ssContacts.Where(w => w.IsActive).GroupBy(g => g.ContactID);
                response.SDefinitionActiveCount = sdActiveContactsGroup.Count();
                ssActiveContactIds = sdActiveContactsGroup.Select(s => s.Key);
            }
            response.TagsAllSDsActiveCount = tagAllContactIds.Concat(ssActiveContactIds).GroupBy(g => g).Count();
            response.TagsActiveSdsAllCount = tagActiveContactIds.Concat(ssAllContactIds).GroupBy(g => g).Count();
            response.TotalActiveUniqueCount = tagActiveContactIds.Concat(ssActiveContactIds).GroupBy(g => g).Count();
            response.TotalAllUniqueCount = tagAllContactIds.Concat(ssAllContactIds).GroupBy(g => g).Count();

            return response;
        }

        public GetUniqueRecipientsCountResponse GetEmailValidatorContactsCount(GetUniqueRecipientsCountRequest request)
        {
            GetUniqueRecipientsCountResponse response = new GetUniqueRecipientsCountResponse();
            var sdContacts = new List<SearchDefinitionContact>();
            var groupId = Guid.NewGuid();
            if (request.SDefinitions.IsAny())
            {
                request.SDefinitions.Each(sd =>
                {
                    var sdRequest = new GetSavedSearchContactIdsRequest()
                    {
                        AccountId = request.AccountId,
                        RequestedBy = request.RequestedBy.Value,
                        RoleId = (short)request.RoleId,
                        SearchDefinitionId = sd
                    };

                    var task = Task.Run(() => advancedSearchService.GetSavedSearchContactIds(sdRequest));
                    var contacts = task.Result;
                    contacts.ForEach(c =>
                    {
                        var sdc = new SearchDefinitionContact
                        {
                            GroupId = groupId,
                            SearchDefinitionId = sd,
                            ContactId = c
                        };
                        sdContacts.Add(sdc);
                    });
                });
                response.SDefinitionActiveCount = sdContacts.Where(w => w.SearchDefinitionId == request.SelectedSearhDefinitionID).Count();
                response.SDefinitionAllCount = campaignRepository.GetUniqueContactscount(request.Tags, sdContacts, request.AccountId);
            }

            if (request.Tags.IsAny())
            {
                response.TagActiveCount = tagRepository.GetContactsCountByTagId(request.SelectedTagID);
                response.TagAllCount = campaignRepository.GetUniqueContactscount(request.Tags, sdContacts, request.AccountId);
            }
            return response;
        }

        public InsertCampaignLogDetailsResponce InsertCampaignLogDetails(InsertCampaignLogDetailsRequest request)
        {
            InsertCampaignLogDetailsResponce responce = new InsertCampaignLogDetailsResponce();
            campaignRepository.InsertCampaignLogDetails(request.CampaignLogDetails);
            return responce;
        }

        public async Task<InsertBulkRecipientsResponse> InsertCampaignRecipients(InsertBulkRecipientsRequest request)
        {
            InsertBulkRecipientsResponse response = new InsertBulkRecipientsResponse();
            try
            {
                Logger.Current.Verbose("Attempting to insert recipients for campaign " + request.CampaignId);
                var campaign = GetCampaignBasicInfo(new GetCampaignRequest(request.CampaignId)).Campaign;
                var roleResponse = userService.GetUserRole(new Messaging.User.GetUserRoleRequest() { UserId = campaign.CreatedBy });

                var contactIds = await GetCampaignRecipientsByID(new GetCampaignRecipientIdsRequest()
                {
                    CampaignId = request.CampaignId,
                    AccountId = campaign.AccountID,
                    RoleId = roleResponse.Role.RoleId,
                    RequestedBy = campaign.CreatedBy
                });
                var temporaryRecipients = contactIds.ContactIds.Select(c => new TemporaryRecipient() { CampaignID = request.CampaignId, ContactID = c });
                campaignRepository.InsertBulkRecipients(temporaryRecipients);
                Logger.Current.Informational("Successfully inserted recipients for campaign " + request.CampaignId);
                response.Result = true;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occured while inserting recipients for campaign " + request.CampaignId, ex);
                response.Result = false;
            }
            return response;
        }

        public GetCampaignResponse GetCampaignBasicInfo(GetCampaignRequest request)
        {
            GetCampaignResponse response = new GetCampaignResponse();
            response.Campaign = campaignRepository.GetCampaignBasicInfoById(request.Id);
            return response;
        }

        /// <summary>
        /// Get campaign links associated with workflow link actions
        /// </summary>
        /// <param name="request"></param>
        /// <returns>List of Campaign links</returns>
        public GetWorkflowLinkActionsResponse GetWorkflowCampaignActionLinks(GetWorkflowLinkActionsRequest request)
        {
            Logger.Current.Verbose("In GetWorkflowCampaignActionLinks. AccountId: " + request.AccountId);
            GetWorkflowLinkActionsResponse response = new GetWorkflowLinkActionsResponse();
            response.CampaignLinks = campaignRepository.GetWorkflowCampaignActionLinks(request.AccountId).ToList();
            Logger.Current.Informational(response.CampaignLinks.Count() + " link actions found.");
            return response;
        }

        /// <summary>
        /// Get campaign re-engaged contacts summary
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public GetReEngagementInfoResponse GetReEngagementInfo(GetReEngagementInfoRequest request)
        {
            Logger.Current.Verbose("In CampaignService/GetReEngagementInfo. AccountId: " + request.AccountId);
            GetReEngagementInfoResponse response = new GetReEngagementInfoResponse();
            response.CampaignStats = campaignRepository.GetReEngagementSummary(request.StartDate, request.EndDate, request.IsDefaultDateRange, request.AccountId, request.LinkIds);
            Logger.Current.Informational(response.CampaignStats.Count() + " campaigns found");
            return response;
        }

        /// <summary>
        /// Get Campaign UTM Information.
        /// </summary>
        /// <param name="campignId"></param>
        /// <returns></returns>
        public Campaign GetCampaignUTMInformation(int campignId)
        {
            Campaign campaign = campaignRepository.GetCampaignUTMInformation(campignId);
            return campaign;
        }

        public GetCampaignLitmusResponse GetPendingLitmusRequests()
        {
            return new GetCampaignLitmusResponse()
            {
                CampaignLitmusMaps = campaignRepository.GetPendingLitmusRequests()
            };
        }

        public void UpdateLitmusId(UpdateCampaignLitmusMap request)
        {
            campaignRepository.UpdateLitmusId(request.CampaignLitmusMap);
        }
        public void RequestLitmusCheck(RequestLitmusCheck request)
        {
            campaignRepository.RequestLitmusCheck(request.CampaignId);
        }
        public GetCampaignLitmusResponse GetCampaignLitmusMap(GetCampaignLitmusMapRequest request)
        {
            return new GetCampaignLitmusResponse()
            {
                CampaignLitmusMaps = campaignRepository.GetLitmusIdByCampaignId(request.CampaignId, request.AccountId, request.RequestedBy.Value),
            };
        }
        public void NotifyLitmusCheck()
        {
            campaignRepository.NotifyLitmusCheck();
        }

        public UpdateCampaignStatusResponse UpdateCampaignStatus(UpdateCampaignStatusRequest request)
        {
            UpdateCampaignStatusResponse response = new UpdateCampaignStatusResponse();
            campaignRepository.UpdateCampaignStatus(request.campaignId, request.Status);
            return response;
        }

        public InsertCampaignMailTesterResponse InsertMailTesterRequest(InsertCampaignMailTesterRequest request)
        {
            Guid guid = Guid.NewGuid();
            Email senderEmail = new Email();
            InsertCampaignMailTesterResponse response = new InsertCampaignMailTesterResponse();
            Account account = accountService.GetAccountMinDetails(request.AccountId);
            IEnumerable<ServiceProvider> serviceProviders = serviceProviderRepository.GetAccountCommunicationProviders(request.AccountId, CommunicationType.Mail, MailType.TransactionalEmail);
            if (serviceProviders != null && serviceProviders.FirstOrDefault() != null)
            {
                senderEmail = serviceProviderRepository.GetServiceProviderEmail(serviceProviders.FirstOrDefault().Id);
            }

            Campaign campaign = campaignRepository.GetCampaignBasicInfoById(request.CampaignID);
            string toEmail = ConfigurationManager.AppSettings["MailTesterEmail"];
            SendMailViewModel viewModel = new SendMailViewModel();
            viewModel.Body = campaign.HTMLContent;
            viewModel.Subject = campaign.Subject;
            viewModel.CampaignTypeId = campaign.CampaignTypeId;
            viewModel.To = toEmail.Replace("XYZ", guid.ToString());
            viewModel.From = (senderEmail != null && !string.IsNullOrEmpty(senderEmail.EmailId)) ? senderEmail.EmailId : account.Email.EmailId;

            SendTestEmail(new SendTestEmailRequest()
            {
                MailViewModel = viewModel,
                ServiceProviderID = null,
                AccountId = request.AccountId,
                HasDisCliamer = false,
                MailTesterGuid = guid
            });
            // SendMailTesterEmail(request.CampaignID, guid, request.AccountId);
            campaignRepository.InsertMailTesterRequest(request.CampaignID, guid, request.RequestedBy.Value);
            return response;
        }

        public void SendMailTesterEmail(int campaignId, Guid guid, int accountId)
        {
            if (campaignId != 0)
            {
                Guid loginToken = new Guid();
                string accountPrimaryEmail = string.Empty;
                Email senderEmail = new Email();
                Account account = accountService.GetAccountMinDetails(accountId);
                string toEmail = ConfigurationManager.AppSettings["MailTesterEmail"];
                if (account != null && !string.IsNullOrEmpty(toEmail))
                {
                    if (account.Email != null)
                        accountPrimaryEmail = account.Email.EmailId;

                    toEmail = toEmail.Replace("XYZ", guid.ToString());
                    IEnumerable<ServiceProvider> serviceProviders = serviceProviderRepository.GetAccountCommunicationProviders(accountId, CommunicationType.Mail, MailType.TransactionalEmail);
                    if (serviceProviders != null && serviceProviders.FirstOrDefault() != null)
                    {
                        loginToken = serviceProviders.FirstOrDefault().LoginToken;
                        senderEmail = serviceProviderRepository.GetServiceProviderEmail(serviceProviders.FirstOrDefault().Id);
                    }

                    if (loginToken != new Guid() && accountPrimaryEmail != null)
                    {
                        string fromEmail = (senderEmail != null && !string.IsNullOrEmpty(senderEmail.EmailId)) ? senderEmail.EmailId : accountPrimaryEmail;

                        Campaign campaign = campaignRepository.GetCampaignBasicInfoById(campaignId);

                        EmailAgent agent = new EmailAgent();
                        LM.SendMailRequest mailRequest = new LM.SendMailRequest();
                        mailRequest.Body = campaign.HTMLContent;
                        mailRequest.From = fromEmail;
                        mailRequest.IsBodyHtml = true;
                        mailRequest.ScheduledTime = DateTime.UtcNow;
                        mailRequest.Subject = campaign.Subject;
                        mailRequest.To = new List<string>() { toEmail };
                        mailRequest.TokenGuid = loginToken;
                        mailRequest.RequestGuid = Guid.NewGuid();
                        mailRequest.AccountDomain = account.DomainURL;
                        mailRequest.CategoryID = (byte)EmailNotificationsCategory.MailTesterEmail;
                        mailRequest.AccountID = accountId;
                        agent.SendEmail(mailRequest);
                    }
                }
            }
        }


        public GetCampaignMailTesterResponse GetMailTestData(GetCampaignMailTesterRequest request)
        {
            GetCampaignMailTesterResponse response = new GetCampaignMailTesterResponse();
            response.CampaignMailTesterData = campaignRepository.GetMailTesterRequests();
            return response;
        }

        public UpdateMailTesterResponse UpdateMailTester(UpdateMailTesterRequest request)
        {
            UpdateMailTesterResponse response = new UpdateMailTesterResponse();
            if (request.MailTester.IsAny())
            {
                campaignRepository.UpdateCampaignMailTester(request.MailTester);
                List<Notification> notifications = new List<Notification>();
                request.MailTester.ToList().ForEach(f =>
                {
                    Notification notification = new Notification();
                    notification.EntityId = f.CampaignID;
                    notification.Subject = string.Format("{0} - MailTester Results", f.Name);
                    notification.Details = string.Format("{0} - MailTester Results", f.Name);
                    notification.Time = DateTime.UtcNow;
                    notification.Status = NotificationStatus.New;
                    notification.UserID = f.CreatedBy;
                    notification.ModuleID = (byte)AppModules.MailTester;
                    notification.DownloadFile = f.UniqueID.ToString();
                    notifications.Add(notification);
                });
                if (notifications.Count > 0)
                    userRepository.AddNotification(notifications);
            }
            return response;
        }

        public Dictionary<byte, string> GetCampaignURLSById(int campaignId)
        {
            Dictionary<byte, string> urls = campaignRepository.GetCampaignLinkURLsByCampaignId(campaignId);
            return urls;
        }

        // Added by Ram on 9th May 2018  for Ticket NEXG-3004
        public void UpdateCampaignSentFlagStatus(Int32 _campaignId, bool _mailsentflag)
        {
            campaignRepository.UpdateCampaignSentFlagStatus(_campaignId, _mailsentflag);
        }


        // Added by Ram on 9th May 2018  for Ticket NEXG-3005
        public List<CampaignRecipient> testing(GetSearchRequest request, Campaign _campaign)
        {

            var sdRequest = new GetSearchRequest()
            {
                SearchDefinitionID = request.SearchDefinitionID,
                IncludeSearchResults = false,
                Limit = 20000,//ItemsPerPage,
                AccountId = request.AccountId,
                RoleId = request.RoleId,
                RequestedBy = null,
                IsRunSearchRequest = true,
                IsSTAdmin = true
            };

            var task123 = Task.Run(() => advancedSearchService.GetSavedSearchAsync(sdRequest));
            var SS = task123.Result;
            AdvancedSearchViewModel viewModel = new AdvancedSearchViewModel();
            if (SS != null)
                viewModel = SS.SearchViewModel;
            SearchDefinition searchDefinition = new SearchDefinition();
            searchDefinition = Mapper.Map<AdvancedSearchViewModel, SearchDefinition>(viewModel);


            SearchParameters parameters = new SearchParameters();
            parameters.PageNumber = 1;
            parameters.Limit = 500000;
            parameters.Fields = new List<ContactFields>() { ContactFields.FirstNameField, ContactFields.LastNameField, ContactFields.ContactId, ContactFields.IsActive, ContactFields.CompanyNameField, ContactFields.PrimaryEmail, ContactFields.DonotEmail, ContactFields.PrimaryEmailStatus, ContactFields.IsPrimaryEmail, ContactFields.CompanyId };
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, request.AccountId);

            if (isPrivate && !true)
            {
                parameters.IsPrivateSearch = true;
                parameters.DocumentOwnerId = request.RequestedBy;
            }
            else
                parameters.IsPrivateSearch = false;
            parameters.IsActiveContactsSearch = true;

            var task = Task.Run(() => contactSearchService.AdvancedSearchAsync(null, searchDefinition, parameters));
            var contacts = task.Result;


            List<CampaignRecipient> tagContacts = new List<CampaignRecipient>();
            var SSContact = contacts.Results.Select(s => new CampaignRecipient()
            {
                ContactID = s.Id,
                To = s.Emails.FirstOrDefault().EmailId,
                CampaignID = _campaign.Id,
                CreatedDate = _campaign.CreatedDate,
                ScheduleTime = _campaign.ScheduleTime,
                SentOn = null,
                DeliveryStatus = Convert.ToInt16(_campaign.DeliveredCount),

            });


            tagContacts.AddRange(SSContact);
            return tagContacts;

        }




    }
}
