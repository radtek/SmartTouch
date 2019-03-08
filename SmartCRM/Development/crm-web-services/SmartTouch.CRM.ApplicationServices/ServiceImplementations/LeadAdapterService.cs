using AutoMapper;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.CustomFields;
using SmartTouch.CRM.ApplicationServices.Messaging.LeadAdapters;
using SmartTouch.CRM.ApplicationServices.ServiceAgents;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.CustomFields;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.SearchEngine.Indexing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using Facebook;
using SmartTouch.CRM.Domain.ValueObjects;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class LeadAdapterService : ILeadAdapterService
    {
        readonly ILeadAdaptersRepository leadAdaptersRepository;
        readonly ILeadAdaptersJobLogsRepository leadAdaptersJobLogsRepository;
        readonly ITagRepository tagRepository;
        readonly IUnitOfWork unitOfWork;
        IIndexingService indexingService;
        readonly ICustomFieldService customfieldService;
        readonly ICustomFieldRepository customfieldRepository;
        readonly IAccountRepository accountRepository;

        public LeadAdapterService(ILeadAdaptersRepository leadAdaptersRepository,
            ILeadAdaptersJobLogsRepository leadAdaptersJobLogsRepository, ITagRepository tagRepository,
            IUnitOfWork unitOfWork, ICustomFieldService customfieldService, ICustomFieldRepository customfieldRepository, IAccountRepository accountRepository)
        {
            if (leadAdaptersRepository == null) throw new ArgumentNullException("leadAdapterRepository");
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
            this.indexingService = new IndexingService();
            this.leadAdaptersRepository = leadAdaptersRepository;
            this.leadAdaptersJobLogsRepository = leadAdaptersJobLogsRepository;
            this.tagRepository = tagRepository;
            this.customfieldService = customfieldService;
            this.customfieldRepository = customfieldRepository;
            this.accountRepository = accountRepository;
            this.unitOfWork = unitOfWork;
        }

        public DeleteLeadAdapterResponse DeleteLeadAdapter(DeleteLeadAdapterRequest request)
        {
            DeleteLeadAdapterResponse response = new DeleteLeadAdapterResponse();
            Logger.Current.Verbose("Request received to delete lead adapter with leadAdapterID " + request.Id);
            bool isLinkedToWorkflows = leadAdaptersRepository.isLinkedToWorkflows(request.Id);
            if (isLinkedToWorkflows)
                throw new UnsupportedOperationException("[|The selected Lead Adapter is linked to Automation Workflow|]. [|You cannot delete the Lead Adapter|].");
            leadAdaptersRepository.DeleteLeadAdapter(request.Id);
            unitOfWork.Commit();
            return response;
        }

        public InsertLeadAdapterResponse InsertLeadAdapter(InsertLeadAdapterRequest request)
        {
            Logger.Current.Verbose("Request received to insert a lead adapter.");
            InsertLeadAdapterResponse response = new InsertLeadAdapterResponse();
            FTPAgent agent = new FTPAgent();
            
            LeadAdapterViewModel vm = request.LeadAdapterViewModel;
            int AccountID = vm.AccountID;
            bool isDuplicate = leadAdaptersRepository.IsDuplicateLeadAdapter(vm.LeadAdapterType, AccountID,
                                                                             vm.LeadAdapterAndAccountMapId);
            if (isDuplicate)
                throw new UnsupportedOperationException("[|LeadAdapter already exists.|]");

            string leadAdapterPhysicalPath = ConfigurationManager.AppSettings["LEADADAPTER_PHYSICAL_PATH"].ToString();
            vm.ArchivePath = Path.Combine(leadAdapterPhysicalPath, AccountID.ToString(), vm.LeadAdapterType.ToString(), "Archive");
            vm.LocalFilePath = Path.Combine(leadAdapterPhysicalPath, AccountID.ToString(), vm.LeadAdapterType.ToString(), "Local");
            vm.RequestGuid = agent.FTPRegistration(vm.UserName, vm.Password, vm.Url, vm.Port, vm.EnableSSL, vm.RequestGuid);

            //Create folders
            if (!Directory.Exists(vm.LocalFilePath))
            {
                Directory.CreateDirectory(vm.LocalFilePath);
            }
            if(!Directory.Exists(vm.ArchivePath))
            {
                Directory.CreateDirectory(vm.ArchivePath);
            }

            LeadAdapterAndAccountMap leadAdapter = Mapper.Map<LeadAdapterViewModel, LeadAdapterAndAccountMap>(vm);
            bool isLeadAdapterAlreadyConfiguredForAccount = leadAdaptersRepository.isLeadAdapterAlreadyConfigured(AccountID, vm.LeadAdapterType);
            leadAdaptersRepository.Insert(leadAdapter);
            LeadAdapterAndAccountMap newLeadAdapter = unitOfWork.Commit() as LeadAdapterAndAccountMap;
            if (!isLeadAdapterAlreadyConfiguredForAccount)
            {
                CustomFieldTab customfieldtab = customfieldRepository.GetLeadAdapterCustomFieldTab(AccountID);
                if (customfieldtab == null)
                {
                    InsertCustomFieldTabRequest customfieldtabrequest = new InsertCustomFieldTabRequest();
                    CustomFieldTabViewModel tab = new CustomFieldTabViewModel();
                    CustomFieldSectionViewModel section = new CustomFieldSectionViewModel();
                    GetLeadAdapterCustomFieldRequest getleadadaptercustomfieldsrequest = new GetLeadAdapterCustomFieldRequest();
                    getleadadaptercustomfieldsrequest.AccountId = AccountID;
                    getleadadaptercustomfieldsrequest.LeadAdapterType = vm.LeadAdapterType;
                    GetLeadAdapterCustomFieldResponse getleadadaptercustomfieldsresponse = customfieldService.GetLeadAdapterCustomFieldsByType(getleadadaptercustomfieldsrequest);
                    section.CustomFields = getleadadaptercustomfieldsresponse.CustomFields.ToList();
                    section.Name = vm.LeadAdapterType.ToString();
                    section.StatusId = CustomFieldSectionStatus.Active;

                    tab.AccountId = AccountID;
                    tab.Name = "Lead Adapter Custom Fields";
                    tab.IsLeadAdapterTab = true;
                    tab.Sections = new List<CustomFieldSectionViewModel>();
                    tab.Sections.Add(section);
                    tab.StatusId = CustomFieldTabStatus.Active;
                    customfieldtabrequest.CustomFieldTabViewModel = tab;
                    customfieldService.InsertCustomFieldTab(customfieldtabrequest);
                }
                else
                {
                    UpdateCustomFieldTabRequest customfieldtabrequest = new UpdateCustomFieldTabRequest();
                    CustomFieldTabViewModel tab = Mapper.Map<CustomFieldTab, CustomFieldTabViewModel>(customfieldtab);
                    CustomFieldSectionViewModel section = new CustomFieldSectionViewModel();
                    GetLeadAdapterCustomFieldRequest getleadadaptercustomfieldsrequest = new GetLeadAdapterCustomFieldRequest();
                    getleadadaptercustomfieldsrequest.AccountId = AccountID;
                    getleadadaptercustomfieldsrequest.LeadAdapterType = vm.LeadAdapterType;
                    GetLeadAdapterCustomFieldResponse getleadadaptercustomfieldsresponse = customfieldService.GetLeadAdapterCustomFieldsByType(getleadadaptercustomfieldsrequest);
                    section.CustomFields = getleadadaptercustomfieldsresponse.CustomFields.ToList();
                    section.Name = vm.LeadAdapterType.ToString();
                    section.StatusId = CustomFieldSectionStatus.Active;                   
                    tab.Sections.Add(section);
                    tab.StatusId = CustomFieldTabStatus.Active;
                    customfieldtabrequest.CustomFieldTabViewModel = tab;
                    customfieldService.UpdateCustomFieldTab(customfieldtabrequest);
                }
            }
            foreach (Tag tag in leadAdapter.Tags.Where(t => t.Id == 0))
            {
                Tag savedTag = tagRepository.FindBy(tag.TagName, leadAdapter.AccountID);
                indexingService.IndexTag(savedTag);
                accountRepository.ScheduleAnalyticsRefresh(savedTag.Id,(byte)IndexType.Tags);
            }
            response.LeadAdapterViewModel = Mapper.Map<LeadAdapterAndAccountMap, LeadAdapterViewModel>(newLeadAdapter);
            Logger.Current.Informational("Leadadapter inserted successfully.");
            return new InsertLeadAdapterResponse();
        }

        public UpdateLeadAdapterResponse UpdateLeadAdapter(UpdateLeadAdapterRequest request)
        {
            Logger.Current.Verbose("Request received to update a lead adapter.");
            FTPAgent agent = new FTPAgent();

            LeadAdapterAndAccountMap leadAdapter = Mapper.Map<LeadAdapterViewModel, LeadAdapterAndAccountMap>(request.LeadAdapterViewModel);
            bool isDuplicate = leadAdaptersRepository.IsDuplicateLeadAdapter(leadAdapter.LeadAdapterTypeID, leadAdapter.AccountID, leadAdapter.Id);

            if (isDuplicate)
            {
                throw new UnsupportedOperationException("[|LeadAdapter already exists.|]");
            }
            string leadAdapterPhysicalPath = ConfigurationManager.AppSettings["LEADADAPTER_PHYSICAL_PATH"].ToString();
            request.LeadAdapterViewModel.ArchivePath = Path.Combine(leadAdapterPhysicalPath, request.LeadAdapterViewModel.AccountID.ToString(),
                                                                    request.LeadAdapterViewModel.LeadAdapterType.ToString(), "Archive");
            request.LeadAdapterViewModel.LocalFilePath = Path.Combine(leadAdapterPhysicalPath, request.LeadAdapterViewModel.AccountID.ToString(),
                                                                      request.LeadAdapterViewModel.LeadAdapterType.ToString(), "Local");
            if (agent.UpdateFtpRegistration(request.LeadAdapterViewModel.UserName, request.LeadAdapterViewModel.Password, request.LeadAdapterViewModel.Url,
                                            request.LeadAdapterViewModel.Port, request.LeadAdapterViewModel.EnableSSL, request.LeadAdapterViewModel.RequestGuid) && leadAdapter != null)
            {               
                    leadAdaptersRepository.Update(leadAdapter);
                    unitOfWork.Commit();
                    Logger.Current.Informational("Leadadapter updated successfully.");               
            }
            return new UpdateLeadAdapterResponse();
        }

        public UpdateLeadAdapterResponse UpdateFacebookLeadAdapter(UpdateLeadAdapterRequest request)
        {
            Logger.Current.Verbose("Request received to update Facebook lead adapter.");

            try
            {
                LeadAdapterAndAccountMap leadAdapter = Mapper.Map<LeadAdapterViewModel, LeadAdapterAndAccountMap>(request.LeadAdapterViewModel);

                bool isDuplicate = leadAdaptersRepository.IsDuplicateFacebookAdapter(request.AccountId, leadAdapter.Id, leadAdapter.FacebookLeadAdapterName);
                if (isDuplicate)
                    throw new UnsupportedOperationException("[|LeadAdapter with same name already exists.|]");

                leadAdaptersRepository.Update(leadAdapter);
                unitOfWork.Commit();
                GetFacebookAppResponse fbResponse = this.GetFacebookApp(new GetFacebookAppRequest() { AccountId = request.AccountId });
                string extendedToken = this.GetExtendedPageAccessToken(leadAdapter.FacebookLeadAdapter.UserAccessToken, leadAdapter.FacebookLeadAdapter.PageID, fbResponse.FacebookAppID, fbResponse.FacebookAppSecret);
                leadAdapter.FacebookLeadAdapter.PageAccessToken = !string.IsNullOrEmpty(extendedToken) ? extendedToken : leadAdapter.FacebookLeadAdapter.PageAccessToken;


                leadAdaptersRepository.UpdateFacebookLeadAdapter(leadAdapter.FacebookLeadAdapter);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while updating facebook lead adapter", ex);
            }
            Logger.Current.Informational("Leadadapter updated successfully.");
            return new UpdateLeadAdapterResponse();
        }

        public InsertLeadAdapterResponse InsertFacebookLeadAdapter(InsertLeadAdapterRequest request)
        {
            Logger.Current.Verbose("Request received to insert Facebook lead adapter.");
            InsertLeadAdapterResponse response = new InsertLeadAdapterResponse();
            int newLeadAdapterID = 0;
            try
            {
                LeadAdapterViewModel vm = request.LeadAdapterViewModel;
                int AccountID = vm.AccountID;

                bool isDuplicate = leadAdaptersRepository.IsDuplicateFacebookAdapter(AccountID, vm.LeadAdapterAndAccountMapId, vm.FacebookLeadAdapterName);
                if (isDuplicate)
                    throw new UnsupportedOperationException("[|LeadAdapter with same name already exists.|]");

                
                LeadAdapterAndAccountMap leadAdapter = Mapper.Map<LeadAdapterViewModel, LeadAdapterAndAccountMap>(vm);
                leadAdaptersRepository.Insert(leadAdapter);
                LeadAdapterAndAccountMap newLeadAdapter = unitOfWork.Commit() as LeadAdapterAndAccountMap;
                newLeadAdapterID = newLeadAdapter.Id;
                leadAdapter.FacebookLeadAdapter.LeadAdapterAndAccountMapID = newLeadAdapter.Id;

                GetFacebookAppResponse fbResponse = this.GetFacebookApp(new GetFacebookAppRequest() { AccountId = AccountID });
                string extendedToken = this.GetExtendedPageAccessToken(leadAdapter.FacebookLeadAdapter.UserAccessToken, leadAdapter.FacebookLeadAdapter.PageID, fbResponse.FacebookAppID, fbResponse.FacebookAppSecret);
                leadAdapter.FacebookLeadAdapter.PageAccessToken = !string.IsNullOrEmpty(extendedToken) ? extendedToken : leadAdapter.FacebookLeadAdapter.PageAccessToken;

                leadAdaptersRepository.InsertFacebookLeadAdapter(leadAdapter.FacebookLeadAdapter);

                var hasFacebookFields = leadAdaptersRepository.HasFacebookFields(AccountID);
                if (!hasFacebookFields)
                {
                    CustomFieldTab customfieldtab = customfieldRepository.GetLeadAdapterCustomFieldTab(AccountID);
                    if (customfieldtab == null)
                    {
                        InsertCustomFieldTabRequest customfieldtabrequest = new InsertCustomFieldTabRequest();
                        CustomFieldTabViewModel tab = new CustomFieldTabViewModel();
                        CustomFieldSectionViewModel section = new CustomFieldSectionViewModel();
                        GetLeadAdapterCustomFieldRequest getleadadaptercustomfieldsrequest = new GetLeadAdapterCustomFieldRequest();
                        getleadadaptercustomfieldsrequest.AccountId = AccountID;
                        getleadadaptercustomfieldsrequest.LeadAdapterType = vm.LeadAdapterType;
                        GetLeadAdapterCustomFieldResponse getleadadaptercustomfieldsresponse = customfieldService.GetLeadAdapterCustomFieldsByType(getleadadaptercustomfieldsrequest);
                        section.CustomFields = getleadadaptercustomfieldsresponse.CustomFields.ToList();
                        section.Name = vm.LeadAdapterType.ToString();
                        section.StatusId = CustomFieldSectionStatus.Active;

                        tab.AccountId = AccountID;
                        tab.Name = "Lead Adapter Custom Fields";
                        tab.IsLeadAdapterTab = true;
                        tab.Sections = new List<CustomFieldSectionViewModel>();
                        tab.Sections.Add(section);
                        tab.StatusId = CustomFieldTabStatus.Active;
                        customfieldtabrequest.CustomFieldTabViewModel = tab;
                        customfieldService.InsertCustomFieldTab(customfieldtabrequest);
                    }
                    else
                    {
                        UpdateCustomFieldTabRequest customfieldtabrequest = new UpdateCustomFieldTabRequest();
                        CustomFieldTabViewModel tab = Mapper.Map<CustomFieldTab, CustomFieldTabViewModel>(customfieldtab);
                        CustomFieldSectionViewModel section = new CustomFieldSectionViewModel();
                        GetLeadAdapterCustomFieldRequest getleadadaptercustomfieldsrequest = new GetLeadAdapterCustomFieldRequest();
                        getleadadaptercustomfieldsrequest.AccountId = AccountID;
                        getleadadaptercustomfieldsrequest.LeadAdapterType = vm.LeadAdapterType;
                        GetLeadAdapterCustomFieldResponse getleadadaptercustomfieldsresponse = customfieldService.GetLeadAdapterCustomFieldsByType(getleadadaptercustomfieldsrequest);
                        section.CustomFields = getleadadaptercustomfieldsresponse.CustomFields.ToList();
                        section.Name = vm.LeadAdapterType.ToString();
                        section.StatusId = CustomFieldSectionStatus.Active;
                        tab.Sections.Add(section);
                        tab.StatusId = CustomFieldTabStatus.Active;
                        customfieldtabrequest.CustomFieldTabViewModel = tab;
                        customfieldService.UpdateCustomFieldTab(customfieldtabrequest);
                    }
                }

                foreach (Tag tag in leadAdapter.Tags.Where(t => t.Id == 0))
                {
                    Tag savedTag = tagRepository.FindBy(tag.TagName, leadAdapter.AccountID);
                    indexingService.IndexTag(savedTag);
                    accountRepository.ScheduleAnalyticsRefresh(savedTag.Id, (byte)IndexType.Tags);
                }
                response.LeadAdapterViewModel = Mapper.Map<LeadAdapterAndAccountMap, LeadAdapterViewModel>(newLeadAdapter);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while creating Facebook LeadAdapter", ex);
                if (newLeadAdapterID != 0)
                    this.DeleteLeadAdapter(new DeleteLeadAdapterRequest(newLeadAdapterID));
                throw;
            }
            
            Logger.Current.Informational("Facebook Leadadapter inserted successfully.");
            return new InsertLeadAdapterResponse();
        }

        public GetFacebookLeadGensResponse GetFacebookLeadGens(GetFacebookLeadGensRequest request)
        {
            GetFacebookLeadGensResponse response = new GetFacebookLeadGensResponse();
            if (request.AccountMapID != 0)
                response.FacebookLeadGens = leadAdaptersRepository.GetFacebookLeadGens(request.AccountMapID);
            return response;
        }

        public GetLeadAdapterResponse GetLeadAdapter(GetLeadAdapterRequest request)
        {
            GetLeadAdapterResponse response = new GetLeadAdapterResponse();
            Logger.Current.Verbose("Request to fetch LeadAdapter based on leadAdapterId" + request.Id);
            LeadAdapterAndAccountMap leadAdapter = leadAdaptersRepository.FindBy(request.Id);
            if (leadAdapter == null)
            {
                response.Exception = GetLeadAdapterNotFoundException();
            }
            else
            {
                if (leadAdapter.LeadAdapterTypeID != LeadAdapterTypes.Facebook)
                {
                    FTPAgent agent = new FTPAgent();
                    var ftpDetails = agent.GetFtpRegistration(leadAdapter.RequestGuid);
                    if (ftpDetails != null)
                    {
                        leadAdapter.Url = ftpDetails.Host;
                        leadAdapter.UserName = ftpDetails.UserName;
                        leadAdapter.Password = ftpDetails.Password;
                        leadAdapter.EnableSsl = ftpDetails.EnableSsl;
                        leadAdapter.Port = ftpDetails.Port;
                    }
                }
                LeadAdapterViewModel leadAdapterViewModel = Mapper.Map<LeadAdapterAndAccountMap, LeadAdapterViewModel>(leadAdapter);
                response.LeadAdapterViewModel = leadAdapterViewModel;
            }
            return response;
        }

        void isValidLeadAdapter(LeadAdapterAndAccountMap leadAdapter)
        {
            IEnumerable<BusinessRule> brokenRules = leadAdapter.GetBrokenRules();
            if (brokenRules.Any())
            {
                StringBuilder brokenRulesBuilder = new StringBuilder();
                foreach (BusinessRule rule in brokenRules.Distinct())
                {
                    brokenRulesBuilder.AppendLine(rule.RuleDescription);
                }
                throw new Exception(brokenRulesBuilder.ToString());
            }
        }

        public GetLeadAdapterListResponse GetAllLeadAdapters(GetLeadAdapterListRequest request)
        {
            GetLeadAdapterListResponse response = new GetLeadAdapterListResponse();
            IEnumerable<LeadAdapterAndAccountMap> leadAdapters = leadAdaptersRepository.FindAll(request.Query, request.Limit, request.PageNumber, request.AccountID);
            if (leadAdapters == null)
            {
                response.Exception = GetLeadAdapterNotFoundException();
            }
            else
            {
                IEnumerable<LeadAdapterViewModel> list = Mapper.Map<IEnumerable<LeadAdapterAndAccountMap>, IEnumerable<LeadAdapterViewModel>>(leadAdapters);
                response.LeadAdapters = list;
                response.TotalHits = list == null ? 0 : list.Select(s => s.TotalCount).FirstOrDefault();
            }
            return response;
        }

        public GetViewLeadAdapterListResponse GetAllViewLeadAdapters(GetViewLeadAdapterListRequest request)
        {
            GetViewLeadAdapterListResponse response = new GetViewLeadAdapterListResponse();
            IEnumerable<LeadAdapterJobLogs> leadAdapterJobLogs = leadAdaptersRepository.FindLeadAdapterJobLogAll(request.Limit, request.PageNumber, request.LeadAdapterID);
            if (leadAdapterJobLogs == null)
            {
                response.Exception = GetLeadAdapterNotFoundException();
            }
            else
            {
                IEnumerable<ViewLeadAdapterViewModel> list = Mapper.Map<IEnumerable<LeadAdapterJobLogs>, IEnumerable<ViewLeadAdapterViewModel>>(leadAdapterJobLogs);
                response.ViewLeadAdapters = list;
                response.TotalHits = leadAdaptersRepository.FindLeadAdapterJobLogAll(request.LeadAdapterID).Count();
            }
            return response;
        }

        public GetLeadAdapterJobLogDetailsListResponse GetAllLeadAdaptersJobLogDetails(GetLeadAdapterJobLogDetailsListRequest request)
        {
            GetLeadAdapterJobLogDetailsListResponse response = new GetLeadAdapterJobLogDetailsListResponse();
            IEnumerable<LeadAdapterJobLogDetails> leadAdapterJobLogs = leadAdaptersJobLogsRepository.FindLeadAdapterJobLogDetailsAll(request.Limit, request.PageNumber,
                                                                                                                                     request.LeadAdapterJobLogID, request.LeadAdapterRecordStatus);
            if (leadAdapterJobLogs == null)
            {
                response.Exception = GetLeadAdapterNotFoundException();
            }
            else
            {
                IEnumerable<LeadAdapterJobLogDeailsViewModel> list = Mapper.Map<IEnumerable<LeadAdapterJobLogDetails>, IEnumerable<LeadAdapterJobLogDeailsViewModel>>(leadAdapterJobLogs);
                response.LeadAdapterErrorDeailsViewModel = list;
                response.TotalHits = leadAdaptersJobLogsRepository.FindLeadAdapterJobLogDetailsCount(request.LeadAdapterJobLogID, request.LeadAdapterRecordStatus);
            }
            return response;
        }

        private UnsupportedOperationException GetLeadAdapterNotFoundException()
        {
            return new UnsupportedOperationException("The requested lead adapter was not found.");
        }

        public GetLeadAdapterSubmissionResponse GetLeadAdapterSubmission(GetLeadAdapterSubmissionRequest request)
        {
            GetLeadAdapterSubmissionResponse response = new GetLeadAdapterSubmissionResponse();
            string submittedData = leadAdaptersRepository.GetLeadAdapterSubmittedDataByID(request.JobLogDetailID);
            LeadAdapterSubmittedDataViewModel submittedviewmodel = new LeadAdapterSubmittedDataViewModel()
            {
                JobLogID = request.JobLogDetailID,
                SubmittedData = submittedData
            };
            response.LeadAdapterSubmission = submittedviewmodel;
            return response;
        }

        public LeadAdapterData GetLeadAdapterData(int? leadadaptermapId)
        {
            var leadAdapterSubmissionData = leadAdaptersJobLogsRepository.GetLeadAdapterSubmittedData(leadadaptermapId);
            return leadAdapterSubmissionData;
        }

        /// <summary>
        /// Getting LeadAdpter Name
        /// </summary>
        /// <param name="leadadaptermapId"></param>
        /// <returns></returns>
        public string GetLeadAdapterName(int? leadadaptermapId)
        {
            string leadAdapterName = leadAdaptersJobLogsRepository.GetLeadAdapterName(leadadaptermapId);
            return leadAdapterName;
        }

        public InsertFacebookLeadGenResponse InsertFacebookLeadGen(InsertFacebookLeadGenRequest request)
        {
            InsertFacebookLeadGenResponse response = new InsertFacebookLeadGenResponse();
            try
            {
                leadAdaptersRepository.InsertFacebookLeadGen(request.FacebookLeadGen);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while inserting Facebook LeadGen", ex);
            }
            return response;
        }

        public GetFacebookAppResponse GetFacebookApp(GetFacebookAppRequest request)
        {
            GetFacebookAppResponse response = new GetFacebookAppResponse();
            if (request.AccountId != 0)
            {
                Account account = leadAdaptersRepository.GetFacebookApp(request.AccountId);
                if (account != null)
                {
                    response.FacebookAppID = account.FacebookAPPID;
                    response.FacebookAppSecret = account.FacebookAPPSecret;
                }
            }
            return response;
        }

        private string GetExtendedPageAccessToken(string userAccessToken, long pageId, string appID, string appSecret)
        {
            string extendedToken = string.Empty;
            if (!String.IsNullOrEmpty(userAccessToken) && pageId != 0)
            {
                string extendedUserToken = GetExtendedToken(userAccessToken, userAccessToken, appID, appSecret);
                FacebookClient client = new FacebookClient(extendedUserToken);
                FacebookPageData pageData = client.Get<FacebookPageData>("/me/accounts");
                IEnumerable<FacebookPage> FBPages = (IEnumerable<FacebookPage>)pageData.data;
                string pageAccessToken = FBPages.Where(w => w.id == pageId).Select(s => s.access_token).FirstOrDefault();
                extendedToken = GetExtendedToken(pageAccessToken, extendedUserToken, appID, appSecret);
            }
            return extendedToken;
        }

        private string GetExtendedToken(string token, string requestToken, string appID, string appSecret)
        {
            string extendedToken = token;
            if (!String.IsNullOrEmpty(token))
            {
                try
                {
                    FacebookClient client = new FacebookClient(requestToken);
                    dynamic result = client.Get("/oauth/access_token", new
                    {
                        grant_type = "fb_exchange_token",
                        client_id = appID,
                        client_secret = appSecret,
                        fb_exchange_token = token
                    });
                    Logger.Current.Informational("Result from fb_exchange_token" + result);
                    extendedToken = result.access_token;
                }
                catch (Exception Ex)
                {
                    Logger.Current.Error("An error occured while exchanging page access token for long lived token", Ex);
                }
            }
            return extendedToken;
        }

        public string GetFaceBookHostNameByContactId(int contactId)
        {
            return leadAdaptersJobLogsRepository.GetFaceBookHostName(contactId);
        }

        public IEnumerable<LeadAdapterType> GetAllLeadadapterTypes()
        {
            return leadAdaptersRepository.GetAllLeadadapterTypes();
        }
    }
}
