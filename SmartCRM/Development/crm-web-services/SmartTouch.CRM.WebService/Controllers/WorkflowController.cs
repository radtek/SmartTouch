using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Formatting;
using SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.WebService.Helpers;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.Messaging.Forms;
using SmartTouch.CRM.ApplicationServices.Messaging.Search;
using SmartTouch.CRM.ApplicationServices.Messaging.Tags;
using SmartTouch.CRM.ApplicationServices.Messaging.User;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using SmartTouch.CRM.ApplicationServices.Messaging.LeadAdapters;
using System.Web.Http.Description;

namespace SmartTouch.CRM.WebService.Controllers
{
    /// <summary>
    /// Creating workflow controller for workflow module
    /// </summary>
    [ApiExplorerSettings(IgnoreApi=true)]
    public class WorkflowController : SmartTouchApiController
    {
        readonly IWorkflowService workflowService;
        readonly ICachingService cachingService;
        readonly IAdvancedSearchService advancedSearchService;
        readonly ICommunicationService communitcationService;
        ITagService tagService;

        /// <summary>
        /// Creating constructor for users controller for accessing
        /// </summary>
        /// <param name="workflowService">workflowService</param>
        /// <param name="cachingService">cachingService</param>
        /// <param name="advancedSearchService">advancedSearchService</param>
        /// <param name="communitcationService">communitcationService</param>
        /// <param name="tagService"></param>
        public WorkflowController(IWorkflowService workflowService, ICachingService cachingService,
                                  IAdvancedSearchService advancedSearchService, ICommunicationService communitcationService, ITagService tagService)
        {
            this.workflowService = workflowService;
            this.cachingService = cachingService;
            this.advancedSearchService = advancedSearchService;
            this.communitcationService = communitcationService;
            this.tagService = tagService;
        }

        /// <summary>
        /// Inserts a workflow
        /// </summary>
        /// <param name="viewModel">Properties of a new workflow</param>
        /// <returns>Workflow insertion Details Response</returns>
        [Route("Workflows")]
        [HttpPost]
        public HttpResponseMessage PostWorkflow(WorkFlowViewModel viewModel)
        {
            InsertWorkflowRequest request = new InsertWorkflowRequest()
            {
                WorkflowViewModel = viewModel,
                AccountId = this.AccountId,
                RequestedBy = this.UserId,
                RoleId = this.RoleId
            };
            InsertWorkflowResponse response = workflowService.InsertWorkflow(request);
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// updates a workflow
        /// </summary>
        /// <param name="viewModel">Properties of a updating workflow</param>
        /// <returns>Workflow Updation Details Response</returns>
        [Route("Workflows")]
        [HttpPut]
        public HttpResponseMessage PutWorkflow(WorkFlowViewModel viewModel)
        {
            UpdateWorkflowRequest request = new UpdateWorkflowRequest()
            {
                WorkflowViewModel = viewModel,
                AccountId = this.AccountId,
                RequestedBy = this.UserId,
                RoleId = this.RoleId
            };
            UpdateWorkflowResponse response = workflowService.UpdateWorkflow(request);
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Delete a workflow
        /// </summary>
        /// <param name="workflowIds">WorkflowIDs of deleting workflows</param>
        /// <returns>Workflow Deletion Details Response</returns>
        [Route("Workflows")]
        [HttpDelete]
        public HttpResponseMessage DeleteWorkflow(DeleteWorkflowRequest workflowIds)
        {
            DeleteWorkflowResponse response = workflowService.DeleteWorkflow(workflowIds);
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// fetches all the dropdownvalues based on the account and the dropdownid (phonetype, lifecyclestage etc)
        /// </summary>
        /// <param name="DropDownID">DropdonwID</param>
        /// <returns>Dropdown values</returns>
        [Route("getdropdownvalues")]
        [HttpGet]
        public HttpResponseMessage GetDropdownValues(DropdownFieldTypes DropDownID)
        {
            var dropdownValues = cachingService.GetDropdownValues(this.AccountId);
            var liefcyclestages = dropdownValues.Where(k => k.DropdownID == (byte)DropDownID)
                                        .Select(k => k.DropdownValuesList)
                                        .ToList().FirstOrDefault()
                                        .Where(d => d.IsActive == true);
            return Request.CreateResponse(HttpStatusCode.OK, liefcyclestages);
        }

        /// <summary>
        /// Fetches all the workflow campaigns and sent campaigns based on the status
        /// </summary>
        /// <param name="IsWorklfowCampaigns">for finding whether the request is for workflows or normal campaigns</param>
        /// <returns>All campaigns</returns>
        [Route("getallcampaigns")]
        [HttpGet]
        public HttpResponseMessage GetCampaigns(bool IsWorklfowCampaigns)
        {
            GetCampaignsRequest request = new GetCampaignsRequest()
            {
                AccountId = this.AccountId,
                IsWorklflowCampaign = IsWorklfowCampaigns
            };
            GetCampaignsResponse response = workflowService.GetAllCampaigns(request);
            return Request.CreateResponse(HttpStatusCode.OK, response.Campaigns);
        }

        /// <summary>
        /// To fetch single workflow
        /// </summary>
        /// <param name="WorkflowID">WorkflowID</param>
        /// <returns>All workflows</returns>
        [Route("getworkflows")]
        [HttpGet]
        public HttpResponseMessage GetWorkflows(short WorkflowID)
        {
            GetWorkflowsRequest request = new GetWorkflowsRequest()
            {
                AccountId = this.AccountId,
                WorkflowID = WorkflowID
            };
            GetWorkflowsResponse response = workflowService.GetRemainingWorkFlows(request);
            return Request.CreateResponse(HttpStatusCode.OK, response.Workflows);
        }

        /// <summary>
        /// Get all the related campaigns associated with the workflow
        /// </summary>
        /// <param name="workflowID">WorkflowIDs</param>
        /// <returns>workflow related campaigns</returns>
        [Route("getrelatedcampaigns")]
        [HttpGet]
        public HttpResponseMessage GetRelatedCampaigns(short workflowID)
        {
            GetWorkflowRelatedCampaignsRequest request = new GetWorkflowRelatedCampaignsRequest()
            {
                WorkflowID = workflowID
            };
            GetWorkflowRelatedCampaignsResponse response = workflowService.GetRelatedCampaigns(request);
            return Request.CreateResponse(HttpStatusCode.OK, response.Campaigns);
        }

        /// <summary>
        /// Fetch all the tags for the account
        /// </summary>        
        /// <returns>All Tags</returns>
        [Route("getalltags")]
        [HttpGet]
        public HttpResponseMessage GetTags()
        {
            GetTagListRequest request = new GetTagListRequest()
            {
                AccountId = this.AccountId
            };
            return Request.CreateResponse(HttpStatusCode.OK, workflowService.GetAllTags(request).Tags);
        }

        /// <summary>
        /// Fetch all the tags for the account
        /// </summary>        
        /// <returns>All Tags</returns>
        [Route("searchtags")]
        [HttpGet]
        public HttpResponseMessage SearchTags(string tagName)
        {
            SearchTagsRequest request = new SearchTagsRequest()
            {
                AccountId = this.AccountId,
                Query = tagName ?? "",
                Limit = 10
            };
            return Request.CreateResponse(HttpStatusCode.OK, tagService.SearchTagsByTagName(request).Tags);
        }

        /// <summary>
        /// Fetch all the tags for the account
        /// </summary>        
        /// <returns>All Recent Popular</returns>
        [Route("getrecentpopulartags")]
        [HttpGet]
        public HttpResponseMessage GetRecentPopularTags()
        {
            GetTagListResponse response = new GetTagListResponse();
            var tags =  tagService.GetRecentAndPopularTags(new GetRecentAndPopularTagsRequest() { AccountId = this.AccountId });
            response.PopularTags = tags.PopularTags;
            response.RecentTags = tags.RecentTags;  
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }



        /// <summary>
        /// Fetch all the forms for the account
        /// </summary>        
        /// <returns>All Forms</returns>
        [Route("getallforms")]
        [HttpGet]
        public HttpResponseMessage GetForms()
        {
            GetFormsRequest request = new GetFormsRequest()
            {
                AccountId = this.AccountId
            };
            GetFormsResponse response = workflowService.GetAllForms(request);
            return Request.CreateResponse(HttpStatusCode.OK, response.Forms);
        }

        /// <summary>
        /// Fetch all the saved searches for the account
        /// </summary>        
        /// <returns>All Saved Searches</returns>
        [Route("getallsmartsearches")]
        [HttpGet]
        public HttpResponseMessage GetSmartSearches()
        {
            GetSavedSearchesRequest request = new GetSavedSearchesRequest()
            {
                AccountId = this.AccountId,
                RequestedBy = this.UserId
            };
            GetSavedSearchesResponse response = workflowService.GetAllSmartSearches(request);
            return Request.CreateResponse(HttpStatusCode.OK, response.SearchResults);
        }

        /// <summary>
        /// Fetch all the campaign links associated with campaign
        /// </summary>   
        /// <param name="CampaignID">CampaignID</param>
        /// <returns>All Campaign Links</returns>
        [Route("getcampaignlinks")]
        [HttpGet]
        public HttpResponseMessage GetCampaignLinks(int CampaignID)
        {
            GetCampaignLinksRequest request = new GetCampaignLinksRequest()
            {
                CampaignID = CampaignID
            };
            GetCampaignLinksResponse response = workflowService.GetCampaignLinks(request);
            return Request.CreateResponse(HttpStatusCode.OK, response.CampaignsLinks);
        }

        /// <summary>
        /// To Fetch all the users associated with the accont
        /// </summary>
        /// <returns>All Users</returns>
        [Route("getallusers")]
        [HttpGet]
        public HttpResponseMessage GetUsers()
        {
            GetUserListRequest request = new GetUserListRequest()
            {
                AccountId = this.AccountId
            };
            GetUserListResponse response = workflowService.GetAllUsers(request);
            return Request.CreateResponse(HttpStatusCode.OK, response.Users);

        }

        /// <summary>
        /// To fetch all the fields in the account
        /// </summary>
        /// <returns>All Saved Search Fields</returns>
        [Route("getallfields")]
        [HttpGet]
        public HttpResponseMessage GetFields()
        {
            GetAdvanceSearchFieldsRequest fieldrequest = new GetAdvanceSearchFieldsRequest()
            {
                accountId = this.AccountId,
                RoleId = this.RoleId
            };
            GetAdvanceSearchFieldsResponse response = advancedSearchService.GetSearchFields(fieldrequest);
            IEnumerable<FieldViewModel> searchFields = response.FieldsViewModel;
            foreach (FieldViewModel field in searchFields)
                if (field.AccountID == null)
                    field.Title += " *";
            return Request.CreateResponse(HttpStatusCode.OK, searchFields);
        }

        /// <summary>
        /// Gets the updatable fields.
        /// </summary>
        /// <returns></returns>
        [Route("getupdatablefields")]
        [HttpGet]
        public HttpResponseMessage GetUpdatableFields()
        { 
            GetUpdatableFieldsRequest request = new GetUpdatableFieldsRequest()
            {
                AccountId = this.AccountId,
                RoleId = this.RoleId
            };
            GetUpdatableFieldsResponse response = advancedSearchService.GetUpdatableFields(request);
            IEnumerable<FieldViewModel> searchFields = response.FieldsViewModel;
            foreach (FieldViewModel field in searchFields)
                if (field.AccountID == null)
                    field.Title += " *";
            return Request.CreateResponse(HttpStatusCode.OK, searchFields);
        }

        /// <summary>
        /// To get all the phone numbers associated with the account
        /// </summary>
        /// <returns>All Phone numbers</returns>
        [Route("getphonenumbers")]
        [HttpGet]
        public HttpResponseMessage GetPhoneNumbers()
        {
            SendTextRequest request = new SendTextRequest()
            {
                AccountId = this.AccountId,
                UserId = this.UserId
            };
            SendTextResponse response = communitcationService.GetSendTextviewModel(request);
            return Request.CreateResponse(HttpStatusCode.OK, response.SendTextViewModel.FromPhones);
        }

        /// <summary>
        /// To get all the leadadapters associated with the account
        /// </summary>
        /// <returns>all leadadapters</returns>
        [Route("getallleadadapters")]
        [HttpGet]
        public HttpResponseMessage GetLeadAdapters()
        {
            GetLeadAdapterListRequest request = new GetLeadAdapterListRequest()
            {
                AccountId = this.AccountId
            };
            GetLeadAdapterListResponse response = workflowService.GetAllLeadAdapters(request);
            return Request.CreateResponse(HttpStatusCode.OK, response.LeadAdapters);
        }

        /// <summary>
        /// To get all the notification fields.
        /// </summary>
        /// <returns></returns>
        [Route("getnotificationfields")]
        [HttpGet]
        public HttpResponseMessage GetAllNotificationFields()
        {
            GetAdvanceSearchFieldsResponse response = advancedSearchService.GetSearchFields(new GetAdvanceSearchFieldsRequest()
            {
                accountId = this.AccountId,
                RoleId = this.RoleId
            });
            List<byte> nonNotifiableFields = this.getNonNotifiableFields();
            response.FieldsViewModel = response.FieldsViewModel.Where(w => !nonNotifiableFields.Contains((byte)w.FieldId)).ToList();
            return Request.CreateResponse(HttpStatusCode.OK, response.FieldsViewModel);
        }

        private List<byte> getNonNotifiableFields()
        {
            List<byte> contactFields = new List<byte>();
            contactFields.Add((byte)ContactFields.NoteSummary);
            contactFields.Add((byte)ContactFields.LastNoteDate);
            contactFields.Add((byte)ContactFields.LastNote);
            contactFields.Add((byte)ContactFields.TourType);
            contactFields.Add((byte)ContactFields.TourDate);
            contactFields.Add((byte)ContactFields.TourCreator);
            contactFields.Add((byte)ContactFields.Community);
            contactFields.Add((byte)ContactFields.TourAssignedUsers);
            contactFields.Add((byte)ContactFields.EmailStatus);
            contactFields.Add((byte)ContactFields.ActionCreatedDate);
            contactFields.Add((byte)ContactFields.ActionType);
            contactFields.Add((byte)ContactFields.ActionDate);
            contactFields.Add((byte)ContactFields.ActionStatus);
            contactFields.Add((byte)ContactFields.ActionAssignedTo);
            contactFields.Add((byte)ContactFields.NoteCategory);
            contactFields.Add((byte)ContactFields.LastNoteCategory);
            return contactFields;
        }

        /// <summary>
        /// Get All Campaigns By Name
        /// </summary>
        /// <param name="campaign"></param>
        /// <returns></returns>
        [Route("getcampaigns")]
        [HttpPost]
        public HttpResponseMessage GetAllCampaigns(CampaignNameViewModel campaign)
        {
            GetCampaignsRequest request = new GetCampaignsRequest()
            {
                AccountId = this.AccountId,
                IsWorklflowCampaign = campaign.IsWorklfowCampaigns,
                Query = campaign.Name
            };
            GetCampaignsResponse response = workflowService.GetCampaigns(request);
            return Request.CreateResponse(HttpStatusCode.OK, response.Campaigns);
        }

        /// <summary>
        /// Get Action Types
        /// </summary>
        /// <returns></returns>
        [Route("getactiontypes")]
        [HttpGet]
        public HttpResponseMessage GetActionTypes()
        {
            IEnumerable<DropdownViewModel> dropdownValues = cachingService.GetDropdownValues(this.AccountId);
            var values = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.ActionType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true 
                && (d.DropdownValueTypeID == 46 || d.DropdownValueTypeID == 47 || d.DropdownValueTypeID == 48));
            return Request.CreateResponse(HttpStatusCode.OK, values);
        }

        /// <summary>
        /// Get Tour Types.
        /// </summary>
        /// <returns></returns>
        [Route("gettourtypes")]
        [HttpGet]
        public HttpResponseMessage GetTourTypes()
        {
            IEnumerable<DropdownViewModel> dropdownValues = cachingService.GetDropdownValues(this.AccountId);
            var values = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.TourType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true
            &&(d.DropdownValueTypeID == (short)DropdownValueTypes.First || d.DropdownValueTypeID == (short)DropdownValueTypes.Be_Back || d.DropdownValueTypeID == (short)DropdownValueTypes.Agent));
            return Request.CreateResponse(HttpStatusCode.OK, values);
        }

    }
}
