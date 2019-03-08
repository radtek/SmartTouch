using Kendo.Mvc.UI;
using Newtonsoft.Json;
using SmartTouch.CRM.ApplicationServices.Messaging.Notes;
using SmartTouch.CRM.ApplicationServices.Messaging.Action;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.Geo;
using SmartTouch.CRM.ApplicationServices.Messaging.Tags;
using SmartTouch.CRM.ApplicationServices.Messaging.Tour;
using SmartTouch.CRM.ApplicationServices.Messaging.DropdownValues;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Web.Utilities;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.Messaging.Search;
using SmartTouch.CRM.Domain.ValueObjects;
using System.Threading;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.Opportunity;
using SmartTouch.CRM.ApplicationServices.Messaging.CustomFields;
using System.Globalization;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.SearchEngine.Search;
using SmartTouch.CRM.ApplicationServices.Messaging.User;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using SmartTouch.CRM.ApplicationServices.Messaging.Forms;
using SmartTouch.CRM.ApplicationServices.Messaging.LeadAdapters;
using SmartTouch.CRM.ApplicationServices.Messaging.Image;
using System.ComponentModel;
using LandmarkIT.Enterprise.Extensions;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using RestSharp;
using System.Configuration;
using System.Web.SessionState;
using SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using System.Net.Http;
using System.Collections.Specialized;
using SmartTouch.CRM.Domain.Opportunities;
using HtmlAgilityPack;
using System.Text;
using SmartTouch.CRM.Domain.Campaigns;
using System.Xml.Linq;
using LandmarkIT.Enterprise.Utilities.Common;
using SmartTouch.CRM.ApplicationServices.Exceptions;
//using DataTables.Mvc;
//using System.Dynamic;

namespace SmartTouch.CRM.Web.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class ContactController : SmartTouchController
    {
        IContactService contactService;

        IGeoService geoService;

        ITagService tagService;

        INoteService noteService;

        ITourService tourService;

        IActionService actionService;

        IAccountService accountService;

        IUserService userService;

        ICustomFieldService customFieldService;

        IFormService formService;

        IDropdownValuesService dropdownValuesService;

        ICachingService cachingService;

        IOpportunitiesService opportunityService;

        IContactRelationshipService contactRelationshipService;

        ICommunicationService communicationService;

        ILeadAdapterService leadAdapterService;

        ICampaignService campaignService;

        readonly IImageService imageService;

        readonly IUrlService urlService;

        readonly IAdvancedSearchService advancedSearchService;

        public ContactController(IContactService contactService, ITagService tagService, IGeoService geoService, IActionService actionService, INoteService noteService, ITourService tourService, IAccountService accountService, IUserService userService, ICustomFieldService customFieldService, IDropdownValuesService dropdownValuesService, ICachingService cachingService, IOpportunitiesService opportunityService, IContactRelationshipService contactRelationshipService, IFormService formService, ICommunicationService communicationService, ILeadAdapterService leadAdapterService, IImageService imageService, IAdvancedSearchService advancedSearchService, ICampaignService campaignService, IUrlService urlService)
        {
            this.contactService = contactService;
            this.geoService = geoService;
            this.tagService = tagService;
            this.noteService = noteService;
            this.actionService = actionService;
            this.tourService = tourService;
            this.accountService = accountService;
            this.userService = userService;
            this.customFieldService = customFieldService;
            this.dropdownValuesService = dropdownValuesService;
            this.cachingService = cachingService;
            this.opportunityService = opportunityService;
            this.contactRelationshipService = contactRelationshipService;
            this.formService = formService;
            this.leadAdapterService = leadAdapterService;
            this.communicationService = communicationService;
            this.imageService = imageService;
            this.advancedSearchService = advancedSearchService;
            this.campaignService = campaignService;
            this.urlService = urlService;
        }

        #region Main Contacts Grid
        /// <summary>
        /// Contactses this instance.
        /// </summary>
        /// <returns></returns>
        [Route("contacts")]
        [SmarttouchAuthorize(AppModules.Contacts, AppOperations.Read)]
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public ActionResult Contacts()
        {
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            ViewBag.Title = "Contacts";
            RemoveCookie("contactid");
            var IsPeople = cachingService.GetOpportunityCustomers(this.Identity.ToUserID(), this.Identity.ToAccountID(), this.Identity.ToRoleID());
            AddCookie("IsPeople", IsPeople.ToString(), 1);
            var usersPermissions = cachingService.GetUserPermissions(Thread.CurrentPrincipal.Identity.ToAccountID());
            List<byte> userModules = usersPermissions.Where(s => s.RoleId == (short)Thread.CurrentPrincipal.Identity.ToRoleID()).Select(s => s.ModuleId).ToList();
            if (userModules.Contains((byte)AppModules.SendMail))
                ViewBag.EmailPermission = true;
            bool isAccountAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
            ViewBag.IsAccountAdmin = isAccountAdmin;
            return View("ContactList");
        }

        [Route("leadgen")]
        [AllowAnonymous]
        //[HttpPost]
        [HttpGet]
        public HttpResponseMessage LeadGen()
        {
            NameValueCollection nvcData = Request.QueryString;
            string ch = nvcData["hub.challenge"];
            Dictionary<string, string> queryStrings = new Dictionary<string, string>();

            HttpResponseMessage msg = new HttpResponseMessage();
            if (!string.IsNullOrEmpty(ch))
                msg.Content = new StringContent(ch);
            return msg;
        }

        /// <summary>
        /// Actionses this instance.
        /// </summary>
        /// <returns></returns>
        [Route("actions")]
        [SmarttouchAuthorize(AppModules.ContactActions, AppOperations.Read)]
        [MenuType(MenuCategory.ActionList, MenuCategory.LeftMenuCRM)]
        public ActionResult Actions()
        {
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            var actionTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.ActionType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            List<SelectListItem> selectedListItems = new List<SelectListItem>();
            SelectListItem selectListItem1 = new SelectListItem();
            selectListItem1.Text = "Select";
            selectListItem1.Value = "0";
            selectedListItems.Add(selectListItem1);
            foreach (DropdownValueViewModel item in actionTypes)
            {
                SelectListItem selectListItem = new SelectListItem();
                selectListItem.Text = item.DropdownValue;
                selectListItem.Value = item.DropdownValueID.ToString();
                selectedListItems.Add(selectListItem);
            }

            ViewBag.ActionTypes = selectedListItems;
            return View("ActionList");
        }

        /// <summary>
        /// Reports the actions.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="StartDate">The start date.</param>
        /// <param name="EndDate">The end date.</param>
        /// <returns></returns>
        [Route("reportactions/{userIds?}/{StartDate?}/{EndDate?}")]
        [SmarttouchAuthorize(AppModules.ContactActions, AppOperations.Read)]
        [MenuType(MenuCategory.ActionList, MenuCategory.LeftMenuCRM)]
        public ActionResult ReportActions(string userIds, string StartDate, string EndDate)
        {
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            int[] userIDs = null;
            if (!string.IsNullOrEmpty(userIds))
                userIDs = JsonConvert.DeserializeObject<int[]>(userIds);

            ViewBag.ItemsPerPage = ItemsPerPage;
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            ViewBag.UserIds = userIDs;
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            var actionTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.ActionType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            List<SelectListItem> selectedListItems = new List<SelectListItem>();
            SelectListItem selectListItem1 = new SelectListItem();
            selectListItem1.Text = "Select";
            selectListItem1.Value = "0";
            selectedListItems.Add(selectListItem1);
            foreach (DropdownValueViewModel item in actionTypes)
            {
                SelectListItem selectListItem = new SelectListItem();
                selectListItem.Text = item.DropdownValue;
                selectListItem.Value = item.DropdownValueID.ToString();
                selectedListItems.Add(selectListItem);
            }

            ViewBag.ActionTypes = selectedListItems;
            return View("ActionList");
        }

        #region ContactsDrilldown
        /// <summary>
        /// Importeds the contacts.
        /// </summary>
        /// <param name="ImportedJobID">The imported job identifier.</param>
        /// <param name="recordStatus">The record status.</param>
        /// <returns></returns>
        [Route("importedcontacts")]
        [SmarttouchAuthorize(AppModules.Contacts, AppOperations.Read)]
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public ActionResult ImportedContacts(int ImportedJobID, string recordStatus)
        {
            ViewBag.JobID = ImportedJobID;
            ViewBag.recordStatus = recordStatus;
            this.ValidateContactPermission();
            return View("ContactList");
        }

        [Route("mycommunicationcontacts")]
        [SmarttouchAuthorize(AppModules.Contacts, AppOperations.Read)]
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public ActionResult MyCommunicationContacts(string activity, string period, string entity)
        {
            ViewBag.CommunicationType = activity;
            ViewBag.Period = period;
            ViewBag.ActivityType = entity;
            this.ValidateContactPermission();
            return View("ContactList");
        }


        /// <summary>
        /// Getting Never Bounce Bad Email Contacts 
        /// </summary>
        /// <param name="nbrid"></param>
        /// <returns></returns>
        [Route("nvbbemcts")]
        [SmarttouchAuthorize(AppModules.Contacts, AppOperations.Read)]
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public ActionResult NeverBounceBadEmailContacts(int nbrid, byte emailStatus)
        {
            ViewBag.NeverBounceID = nbrid;
            ViewBag.NBES = emailStatus;
            this.ValidateContactPermission();
            return View("ContactList");
        }

        /// <summary>
        /// Leads the adapter contacts.
        /// </summary>
        /// <param name="LeadAdapterJobID">The lead adapter job identifier.</param>
        /// <returns></returns>
        [Route("leadadaptercontacts")]
        [SmarttouchAuthorize(AppModules.Contacts, AppOperations.Read)]
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public ActionResult LeadAdapterContacts(int LeadAdapterJobID)
        {
            ViewBag.JobID = LeadAdapterJobID;
            ViewBag.recordStatus = "";
            this.ValidateContactPermission();
            return View("ContactList");
        }

        /// <summary>
        /// Views the submissions.
        /// </summary>
        /// <param name="formId">The form identifier.</param>
        /// <returns></returns>
        [Route("viewsubmissions")]
        [SmarttouchAuthorize(AppModules.Contacts, AppOperations.Read)]
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public ActionResult ViewSubmissions(int formId)
        {
            ViewBag.FormID = formId;
            ViewBag.recordStatus = "";
            this.ValidateContactPermission();
            return View("ContactList");
        }

        /// <summary>
        /// Integrateds the persons.
        /// </summary>
        /// <param name="persons">The persons.</param>
        /// <returns></returns>
        [Route("integratedpersons")]
        [SmarttouchAuthorize(AppModules.Contacts, AppOperations.Read)]
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public ActionResult IntegratedPersons(string persons)
        {
            ViewBag.Persons = persons;
            ViewBag.recordStatus = "";
            this.ValidateContactPermission();
            return View("ContactList");
        }

        /// <summary>
        /// Workflows the contacts.
        /// </summary>
        /// <param name="WorkflowID">The workflow identifier.</param>
        /// <param name="WorkflowContactState">State of the workflow contact.</param>
        /// <returns></returns>
        [Route("workflowcontacts")]
        [SmarttouchAuthorize(AppModules.Contacts, AppOperations.Read)]
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public ActionResult WorkflowContacts(int WorkflowID, WorkflowContactsState WorkflowContactState)
        {
            ViewBag.WorkflowID = WorkflowID;
            ViewBag.WorkflowContactState = WorkflowContactState;
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            var usersPermissions = cachingService.GetUserPermissions(Thread.CurrentPrincipal.Identity.ToAccountID());
            List<byte> userModules = usersPermissions.Where(s => s.RoleId == (short)Thread.CurrentPrincipal.Identity.ToRoleID()).Select(s => s.ModuleId).ToList();
            if (userModules.Contains((byte)AppModules.SendMail))
                ViewBag.EmailPermission = true;
            bool isAccountAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
            ViewBag.IsAccountAdmin = isAccountAdmin;
            RemoveCookie("contactid");
            return View("ContactList");
        }

        /// <summary>
        /// Campaigns the contacts.
        /// </summary>
        /// <param name="CampaignID">The campaign identifier.</param>
        /// <param name="CampaignDrillDownActivity">The campaign drill down activity.</param>
        /// <param name="CampaignLinkID">The campaign link identifier.</param>
        /// <returns></returns>
        [Route("campaigncontacts")]
        [SmarttouchAuthorize(AppModules.Contacts, AppOperations.Read)]
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public ActionResult CampaignContacts(int CampaignID, CampaignDrillDownActivity CampaignDrillDownActivity, int? CampaignLinkID)
        {
            ViewBag.CampaignID = CampaignID;
            ViewBag.CampaignDrillDownActivity = CampaignDrillDownActivity;
            ViewBag.CampaignLinkID = CampaignLinkID;
            this.ValidateContactPermission();
            return View("ContactList");
        }

        /// <summary>
        /// Workflows the contact list.
        /// </summary>
        /// <param name="WorkflowID">The workflow identifier.</param>
        /// <param name="CampaignID">The campaign identifier.</param>
        /// <param name="CampaignDrillDownActivity">The campaign drill down activity.</param>
        /// <returns></returns>
        [Route("workflowcontactlist")]
        [SmarttouchAuthorize(AppModules.Contacts, AppOperations.Read)]
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public ActionResult WorkflowContactList(short WorkflowID, int CampaignID, byte CampaignDrillDownActivity, DateTime? FromDate, DateTime? ToDate)
        {
            ViewBag.WorkflowID = WorkflowID;
            ViewBag.CampaignID = CampaignID;
            ViewBag.CampaignDrillDownActivity = CampaignDrillDownActivity;
            ViewBag.WfCmpFromDate = FromDate;
            ViewBag.WfCmpToDate = ToDate;
            this.ValidateContactPermission();
            return View("ContactList");
        }

        #endregion
        /// <summary>
        /// Gets the form contact ids.
        /// </summary>
        /// <param name="FormID">The form identifier.</param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetFormContactIds(int FormID)
        {
            GetFormContactsRequest request = new GetFormContactsRequest();
            request.FormID = FormID;
            GetFormContactsResponse response = formService.GetFormViewSubmissions(request);
            ExportPersonViewModel viewModel = new ExportPersonViewModel();
            viewModel.ContactID = response.ContactIdList.ToArray();
            return Json(new
            {
                success = true,
                response = response.ContactIdList
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Tags the contacts.
        /// </summary>
        /// <param name="TagID">The tag identifier.</param>
        /// <param name="Type">The type.</param>
        /// <returns></returns>
        [Route("taggedcontacts")]
        [SmarttouchAuthorize(AppModules.Contacts, AppOperations.Read)]
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public ActionResult TagContacts(int TagID, int Type)
        {
            ViewBag.TagID = TagID;
            ViewBag.TagType = Type;
            ViewBag.recordStatus = "";
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            var usersPermissions = cachingService.GetUserPermissions(Thread.CurrentPrincipal.Identity.ToAccountID());
            List<byte> userModules = usersPermissions.Where(s => s.RoleId == (short)Thread.CurrentPrincipal.Identity.ToRoleID()).Select(s => s.ModuleId).ToList();
            if (userModules.Contains((byte)AppModules.SendMail))
                ViewBag.EmailPermission = true;
            ViewBag.ItemsPerPage = ItemsPerPage;
            bool isAccountAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
            ViewBag.IsAccountAdmin = isAccountAdmin;
            RemoveCookie("contactid");
            return View("ContactList");
        }

        /// <summary>
        /// Searches the definition contacts.
        /// </summary>
        /// <param name="DefinitiationId">The definitiation identifier.</param>
        /// <param name="Type">The type.</param>
        /// <returns></returns>
        [Route("searchDefinitioncontacts")]
        [SmarttouchAuthorize(AppModules.Contacts, AppOperations.Read)]
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public ActionResult SearchDefinitionContacts(int DefinitiationId, int Type)
        {
            ViewBag.SSDefinitiationId = DefinitiationId;
            ViewBag.ContactType = Type;
            ViewBag.recordStatus = "";
            this.ValidateContactPermission();
            return View("ContactList");
        }

        /// <summary>
        /// Tageds the contacts.
        /// </summary>
        /// <param name="TagID">The tag identifier.</param>
        /// <param name="reportType">Type of the report.</param>
        /// <param name="reportId">The report identifier.</param>
        /// <returns></returns>
        [Route("tagreportcontacts")]
        [SmarttouchAuthorize(AppModules.Contacts, AppOperations.Read)]
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public ActionResult TagedContacts(int TagID, byte? reportType, int? reportId)
        {

            var accountId = this.Identity.ToAccountID();
            var roleId = this.Identity.ToRoleID();
            short ItemsPerPage = default(short);
            short.TryParse(Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.IsAdvancedGrid = true;

            AdvancedViewViewModel advancedViewModel = new AdvancedViewViewModel();
            advancedViewModel.AccountID = accountId;
            advancedViewModel.ItemsPerPage = ItemsPerPage;
            advancedViewModel.HasEmailPermission = cachingService.CheckSendMailPermissions(accountId, roleId);
            advancedViewModel.IsAccountAdmin = cachingService.IsAccountAdmin(roleId, accountId);
            GetAdvanceSearchFieldsResponse response = advancedSearchService.GetSearchFields(new GetAdvanceSearchFieldsRequest()
            {
                accountId = accountId,
                RoleId = roleId
            });
            advancedViewModel.SearchFields = ExcludeNonViewableFields(response.FieldsViewModel);
            RemoveCookie("contactid");

            advancedViewModel.TagID = TagID;
            advancedViewModel.IsSavedSearch = false;
            advancedViewModel.IsDynamicGrid = true;
            advancedViewModel.EntityType = (byte)ShowingType.TagsReport;
            advancedViewModel.EntityId = reportId.HasValue ? reportId.Value : 0;
            int[] ContactIDsList = null;
            GetTagContactsResponse tagsResponse = GetTagRelatedContacts((int)TagID, 0);
            if (tagsResponse.ContactIdList == null || tagsResponse.ContactIdList.Count() < 0)
            {
                ContactIDsList = new List<int>().ToArray();
            }
            else
            {
                ContactIDsList = tagsResponse.ContactIdList.ToArray();
            }
            Guid guid = Guid.NewGuid();
            AddCookie("ContactsGuid", guid.ToString(), 1);
            cachingService.StoreSavedSearchContactIds(guid.ToString(), ContactIDsList);
            advancedViewModel.Guid = guid;
            advancedViewModel.LeadAdapterTypes = leadAdapterService.GetAllLeadadapterTypes();
            if (reportId.HasValue)
                advancedViewModel.SelectedFields = GetColumnPreferences(null, reportId.Value, 8);
            return View("ContactResultsView2", advancedViewModel);
        }

        /// <summary>
        /// For Action Related Contacts.
        /// </summary>
        /// <param name="actionId"></param>
        /// <param name="showingType"></param>
        /// <returns></returns>
        [Route("actiontagedcontacts")]
        [SmarttouchAuthorize(AppModules.Contacts, AppOperations.Read)]
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public ActionResult ActionContacts(int actionId, byte showingType)
        {
            var accountId = this.Identity.ToAccountID();
            var roleId = this.Identity.ToRoleID();
            short ItemsPerPage = default(short);
            short.TryParse(Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.IsAdvancedGrid = true;

            AdvancedViewViewModel advancedViewModel = new AdvancedViewViewModel();
            advancedViewModel.AccountID = accountId;
            advancedViewModel.ItemsPerPage = ItemsPerPage;
            advancedViewModel.HasEmailPermission = cachingService.CheckSendMailPermissions(accountId, roleId);
            advancedViewModel.IsAccountAdmin = cachingService.IsAccountAdmin(roleId, accountId);
            GetAdvanceSearchFieldsResponse response = advancedSearchService.GetSearchFields(new GetAdvanceSearchFieldsRequest()
            {
                accountId = accountId,
                RoleId = roleId
            });
            advancedViewModel.SearchFields = ExcludeNonViewableFields(response.FieldsViewModel);
            advancedViewModel.LeadAdapterTypes = leadAdapterService.GetAllLeadadapterTypes();
            RemoveCookie("contactid");

            advancedViewModel.ActionID = actionId;
            advancedViewModel.EntityId = actionId;
            advancedViewModel.IsSavedSearch = false;
            advancedViewModel.IsDynamicGrid = true;
            int[] ContactIDsList = null;
            GetActionRelatedContactsResponce actionResponse = GetActionRelatedContacts(actionId);
            if (actionResponse.ContactIdList == null || actionResponse.ContactIdList.Count() < 0)
            {
                ContactIDsList = new List<int>().ToArray();
            }
            else
            {
                ContactIDsList = actionResponse.ContactIdList.ToArray();
            }
            Guid guid = Guid.NewGuid();
            AddCookie("ContactsGuid", guid.ToString(), 1);
            cachingService.StoreSavedSearchContactIds(guid.ToString(), ContactIDsList);
            advancedViewModel.Guid = guid;
            advancedViewModel.EntityType = (byte)ShowingType.Action;
            advancedViewModel.SelectedFields = GetColumnPreferences(null, actionId, (byte)ShowingType.Action);
            return View("ContactResultsView2", advancedViewModel);
        }

        [Route("opportunitycontacts")]
        [SmarttouchAuthorize(AppModules.Contacts, AppOperations.Read)]
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public ActionResult OpportunityContacts(int opportunityId, byte showingType, byte contactType)
        {
            var accountId = this.Identity.ToAccountID();
            var roleId = this.Identity.ToRoleID();
            short ItemsPerPage = default(short);
            short.TryParse(Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.IsAdvancedGrid = true;

            AdvancedViewViewModel advancedViewModel = new AdvancedViewViewModel();
            advancedViewModel.AccountID = accountId;
            advancedViewModel.ItemsPerPage = ItemsPerPage;
            advancedViewModel.HasEmailPermission = cachingService.CheckSendMailPermissions(accountId, roleId);
            advancedViewModel.IsAccountAdmin = cachingService.IsAccountAdmin(roleId, accountId);
            GetAdvanceSearchFieldsResponse response = advancedSearchService.GetSearchFields(new GetAdvanceSearchFieldsRequest()
            {
                accountId = accountId,
                RoleId = roleId
            });
            advancedViewModel.SearchFields = ExcludeNonViewableFields(response.FieldsViewModel);
            advancedViewModel.LeadAdapterTypes = leadAdapterService.GetAllLeadadapterTypes();
            RemoveCookie("contactid");

            advancedViewModel.IsDynamicGrid = true;
            int[] ContactIDsList = null;
            GetOpportunityContactsResponse actionResponse = GetOpportunityContacts(opportunityId);
            if (actionResponse.ContactIdList == null || actionResponse.ContactIdList.Count() < 0)
                ContactIDsList = new List<int>().ToArray();
            else
                ContactIDsList = actionResponse.ContactIdList.ToArray();
            Guid guid = Guid.NewGuid();
            AddCookie("ContactsGuid", guid.ToString(), 1);
            cachingService.StoreSavedSearchContactIds(guid.ToString(), ContactIDsList);
            advancedViewModel.Guid = guid;
            advancedViewModel.OpportunityID = opportunityId;
            advancedViewModel.EntityId = opportunityId;
            advancedViewModel.EntityType = (byte)ShowingType.Opportunity;
            advancedViewModel.SelectedFields = GetColumnPreferences(null, opportunityId, (byte)ShowingType.Opportunity);
            advancedViewModel.ShowingType = contactType == 1 ? ContactShowingFieldType.People : ContactShowingFieldType.Companies;
            return View("ContactResultsView2", advancedViewModel);
        }

        /// <summary>
        /// Actions the contacts.
        /// </summary>
        /// <param name="ActionID">The action identifier.</param>
        /// <returns></returns>
        [Route("actioncontacts")]
        [SmarttouchAuthorize(AppModules.Contacts, AppOperations.Read)]
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public ActionResult ActionContacts(int ActionID)
        {
            ViewBag.ActionID = ActionID;
            ViewBag.recordStatus = "";
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            var usersPermissions = cachingService.GetUserPermissions(Thread.CurrentPrincipal.Identity.ToAccountID());
            List<byte> userModules = usersPermissions.Where(s => s.RoleId == (short)Thread.CurrentPrincipal.Identity.ToRoleID()).Select(s => s.ModuleId).ToList();
            if (userModules.Contains((byte)AppModules.SendMail))
                ViewBag.EmailPermission = true;
            ViewBag.ItemsPerPage = ItemsPerPage;
            bool isAccountAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
            ViewBag.IsAccountAdmin = isAccountAdmin;
            RemoveCookie("contactid");
            return View("ContactList");
        }

        /// <summary>
        /// Contacts the search.
        /// </summary>
        /// <returns></returns>
        [Route("contacts/search")]
        [SmarttouchAuthorize(AppModules.Contacts, AppOperations.Read)]
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public ActionResult ContactSearch()
        {
            ViewBag.ContactDetail = 1;
            ViewBag.JobID = ReadCookie("ImportJobID");
            this.ValidateContactPermission();
            return View("ContactList");
        }

        /// <summary>
        /// Saveds the search contacts.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="reportType">Type of the report.</param>
        /// <param name="reportId">The report identifier.</param>
        /// <returns></returns>
        [Route("contactresults/{guid?}/{reportType?}/{reportId?}")]
        [SmarttouchSessionStateBehaviour(SessionStateBehavior.Required)]
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public ActionResult SavedSearchContacts(Guid? guid, byte? reportType, int? reportId, bool? fromssgrid, string reportName, int? formId)
        {
            var identity = this.Identity;
            AdvancedViewViewModel advancedViewModel = new AdvancedViewViewModel();
            var accountId = identity.ToAccountID();
            var roleId = identity.ToRoleID();
            ViewBag.IsSavedSearch = guid.HasValue ? false : true;
            ViewBag.AccountId = accountId;
            ViewBag.Guid = guid;        //Forms, Reports, Dashboard - Untouched, Contacts, Forms
            ViewBag.IsNewContactsSearch = false;
            ViewBag.SelectedFields = null;
            ViewBag.SearchName = null;
            ViewBag.SearchId = null;
            ViewBag.Preconfigured = null;
            ViewBag.Favorite = null;
            ViewBag.EntityType = null;
            ViewBag.ReportName = reportName;
            ViewBag.ReportId = null;
            if (!guid.HasValue && reportType == null)
            {
                var viewModel = (AdvancedSearchViewModel)Session["AdvancedSearchVM"];
                ViewBag.SearchName = !string.IsNullOrEmpty(viewModel.SearchDefinitionName) ? viewModel.SearchDefinitionName : ReadCookie("savedsearchname");
                short searchDefinitionId = 0;
                bool success = short.TryParse(ReadCookie("savedsearchid"), out searchDefinitionId);
                if (!success)
                    searchDefinitionId = viewModel.SearchDefinitionID;
                ViewBag.SearchId = searchDefinitionId;
                ViewBag.Preconfigured = viewModel.IsPreConfiguredSearch;
                ViewBag.Favorite = viewModel.IsFavoriteSearch;
                if (searchDefinitionId != default(int))
                    ViewBag.SearchDescription = advancedSearchService.GetSearchDefinitionDescription(new GetSearchDefinitionDescriptionRequest()
                    {
                        SearchDefinitionId = searchDefinitionId
                    }).Title;
                else
                    ViewBag.SearchDescription = string.Empty;
                ViewBag.SelectedFields = GetColumnPreferences(viewModel.SearchFilters, searchDefinitionId, 1);
                if (fromssgrid.HasValue)
                    ViewBag.FromSSGrid = fromssgrid;
            }
            else if (guid.HasValue && reportType == null)
            {
                ViewBag.ContactDetail = 1;
                ContactGridViewModel viewModel = cachingService.GetGridViewModel(guid.Value.ToString());
                AddCookie("pagenumber", viewModel.PageNo.ToString(), 1);
                AddCookie("pagesize", viewModel.PageCount.ToString(), 1);
                ViewBag.Name = viewModel.SearchString;
                ViewBag.Type = viewModel.ShowingField;
                ViewBag.ShowingType = viewModel.ShowingField;
                ViewBag.Sort = viewModel.SortingField;
                ViewBag.SelectedFields = new List<int>() { 1, 2, 3, 7, 22, 41 };
                ViewBag.IsDynamicGrid = true;
            }
            else if (reportType == 3)
            {
                ViewBag.IsNewContactsSearch = true;
                ViewBag.IsSavedSearch = false;
                ViewBag.ShowingType = "0";
                ViewBag.Type = "0";
                ViewBag.EntityId = reportId.HasValue ? reportId.Value : reportId;
                ViewBag.EntityType = (byte)ShowingType.Report;
                var viewModel = (AdvancedSearchViewModel)Session["NewContactsSearchViewModel"];
                if (reportId.HasValue)
                    ViewBag.SelectedFields = GetColumnPreferences(viewModel.SearchFilters, reportId.Value, 2);
                else
                    ViewBag.IsFromDashboard = true;
            }
            else
            {
                ViewBag.IsSavedSearch = false;
                ViewBag.EntityId = reportId.HasValue ? reportId.Value : reportId;
                ViewBag.EntityType = (byte)ShowingType.Report;
                if (reportId.HasValue)
                {
                    if (formId.HasValue && formId > 0)
                    {
                        Logger.Current.Verbose("Fetching columns for form : " + formId);

                        var formFields = formService.GetFormFieldIDs(new GetFormFieldIDsRequest() { FormID = (int)formId }).FieldIDs;
                        var newFormFields = formFields.ToList();
                        newFormFields.Add(51); // 51 = ContactPrimaryLeadSource
                        ViewBag.SelectedFields = newFormFields;
                    }
                    else
                        ViewBag.SelectedFields = GetColumnPreferences(null, reportId.Value, 2);
                }

            }
            short ItemsPerPage = default(short);
            var context = System.Web.HttpContext.Current.Request.UrlReferrer != null ? System.Web.HttpContext.Current.Request.UrlReferrer.AbsoluteUri : "";

            short.TryParse(identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            ViewBag.EmailPermission = cachingService.CheckSendMailPermissions(accountId, roleId);
            bool isAccountAdmin = cachingService.IsAccountAdmin(roleId, accountId);
            ViewBag.IsAccountAdmin = isAccountAdmin;
            GetAdvanceSearchFieldsResponse response = advancedSearchService.GetSearchFields(new GetAdvanceSearchFieldsRequest()
            {
                accountId = accountId,
                RoleId = roleId
            });
            ViewBag.SearchFields = ExcludeNonViewableFields(response.FieldsViewModel);
            ViewBag.IsAdvancedGrid = true;
            ViewBag.LeadAdapterTypes = leadAdapterService.GetAllLeadadapterTypes();
            RemoveCookie("contactid");
            return View("ContactResultsView");
        }

        private IEnumerable<FieldViewModel> ExcludeNonViewableFields(IEnumerable<FieldViewModel> fields)
        {
            IEnumerable<FieldViewModel> fieldModel = new List<FieldViewModel>();
            if (fields.IsAny())
                fieldModel = fields.Where(w => w.FieldId != 42 && w.FieldId != 45 && w.FieldId != 46 && w.FieldId != 47 && w.FieldId != 48 && w.FieldId != 49 && w.FieldId != 56 && w.FieldId != 57 && w.FieldId != 58 && w.FieldId != 60
                    && w.FieldId != 63 && w.FieldId != 64 && w.FieldId != 65 && w.FieldId != 66 && w.FieldId != 67 && w.FieldId != 71 && w.FieldId != 72);
            return fieldModel;
        }

        /// <summary>
        /// Gets the contact sort types.
        /// </summary>
        /// <returns></returns>
        public JsonResult GetContactSortTypes()
        {
            var ContactSortTypes = EnumToListConverter.convertEnumToList<ContactSortFieldType>();
            return Json(ContactSortTypes, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the contact URL.
        /// </summary>
        /// <returns></returns>
        public ActionResult GetContactURL()
        {
            string name = ReadCookie("searchtext");
            string strValue = ReadCookie("searchcontent");
            string pagesize = ReadCookie("pagesize");
            string pagenumber = ReadCookie("pagenumber");
            if (string.IsNullOrEmpty(strValue))
                contactService.GetAllContacts<ContactGridEntry>(new SearchContactsRequest()
                {
                    Query = name,
                    Limit = Convert.ToInt16(pagesize),
                    PageNumber = Convert.ToInt16(pagenumber),
                    AccountId = this.Identity.ToAccountID(),
                    RequestedBy = this.Identity.ToUserID(),
                    RoleId = this.Identity.ToRoleID()
                });
            else if (strValue == "1")
                contactService.GetPersons<ContactGridEntry>(new SearchContactsRequest()
                {
                    Query = name,
                    Limit = Convert.ToInt16(pagesize),
                    PageNumber = Convert.ToInt16(pagenumber),
                    AccountId = this.Identity.ToAccountID(),
                    RequestedBy = this.Identity.ToUserID(),
                    RoleId = this.Identity.ToRoleID()
                });
            else if (strValue == "2")
                contactService.GetCompanies<ContactGridEntry>(new SearchContactsRequest()
                {
                    Query = name,
                    Limit = Convert.ToInt16(pagesize),
                    PageNumber = Convert.ToInt16(pagenumber),
                    AccountId = this.Identity.ToAccountID(),
                    RequestedBy = this.Identity.ToUserID(),
                    RoleId = this.Identity.ToRoleID()
                });
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            ViewBag.CurrencyFormat = this.Identity.ToCurrency();
            return View("PersonDetails");
        }

        /// <summary>
        /// Googles the drive API.
        /// </summary>
        /// <param name="pageName">Name of the page.</param>
        /// <returns></returns>
        public ActionResult GoogleDriveApi(string pageName, string clientId, string apiKey)
        {
            int accountID = this.Identity.ToAccountID();

            ViewBag.ClientID = clientId;
            ViewBag.APIKey = apiKey;
            ViewBag.pageName = pageName;
            return PartialView("_GooglePicker");
        }

        public JsonResult GetGoogleDriveAPIKey()
        {
            GetGoogleDriveAPIKeyResponse response = accountService.GetGoogleDriveAPIKey(new GetGoogleDriveAPIKeyRequest() { AccountId = this.Identity.ToAccountID() });
            return Json(new { success = true, response = response }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveGridData(ContactGridViewModel model)
        {
            Guid guid = Guid.NewGuid();
            cachingService.SaveContactGridData(model, guid);
            return Json(guid, JsonRequestBehavior.AllowGet);
        }

        [SmarttouchSessionStateBehaviour(SessionStateBehavior.Required)]
        public async Task<ActionResult> ContactsViewRead([DataSourceRequest] DataSourceRequest request, string name, string type, string sorter, int? JobID, int? TagID, int? TagType, int? SSType, int? SSDefinitiationId,
            int? FormID, string recordStatus, string IntegratedPersons, short? WorkflowID, WorkflowContactsState? WorkflowContactState, int? CampaignID, int? CampaignLinkID, CampaignDrillDownActivity? CampaignDrillDownActivity,
            int? ActionID, bool isSavedSearch, bool? viewDynamicGrid, Guid? guid, bool? IsNewContactsSearch, DateTime? WorflowCampFromDate, DateTime? WorkflowCampToDate, int? NeverBounceRequestID, byte? NeverBounceEmailStatus, int? OpportunityID
            , string MyCommunicationType, string MyCommunicationPeriod, string MyCommunicationActivityType)
        {
            RemoveCookie("opportunityid");
            RemoveCookie("ImportJobID");
            RemoveCookie("ImportJobRecordStatus");
            RemoveCookie("TagID");
            RemoveCookie("TagType");
            RemoveCookie("ViewSubmissionFormID");
            RemoveCookie("IntegratedPersonIds");
            RemoveCookie("WorkflowID");
            RemoveCookie("WorkflowContactState");
            RemoveCookie("FormID");
            RemoveCookie("CampaignID");
            RemoveCookie("CampaignLinkID");
            RemoveCookie("CampaignDrillDownActivity");
            RemoveCookie("WfCampFromDate");
            RemoveCookie("WfCampToDate");
            RemoveCookie("ActionID");
            RemoveCookie("SSType");
            RemoveCookie("SSDefinitiationId");
            RemoveCookie("NeverBounceRequestID");
            RemoveCookie("NeverBounceEmailStatus");
            RemoveCookie("OpptyID");
            RemoveCookie("MyCommunicationType");
            RemoveCookie("MyCommunicationPeriod");
            RemoveCookie("MyCommunicationActivityType");


            var sortField = request.Sorts.Count > 0 ? request.Sorts.First().Member : null;
            var direction = request.Sorts.Count > 0 ? request.Sorts.First().SortDirection : System.ComponentModel.ListSortDirection.Descending;
            var drilldownType = String.Empty;
            if (JobID != null)
            {
                drilldownType = "5";
                AddCookie("ImportJobID", JobID.ToString(), 1);
                AddCookie("ImportJobRecordStatus", recordStatus, 1);
            }
            if (TagID != null)
            {
                drilldownType = "6";
                AddCookie("TagID", TagID.ToString(), 1);
                AddCookie("TagType", TagType.ToString(), 1);
            }
            if (FormID != null)
            {
                drilldownType = "7";
                AddCookie("ViewSubmissionFormID", FormID.ToString(), 1);
            }
            if (IntegratedPersons != "")
            {
                drilldownType = "8";
                AddCookie("IntegratedPersonIds", IntegratedPersons.ToString(), 1);
            }
            if (WorkflowID != null)
            {
                drilldownType = "9";
                AddCookie("WorkflowID", WorkflowID.Value.ToString(), 1);
                if (WorkflowContactState != null)
                    AddCookie("WorkflowContactState", WorkflowContactState.Value.ToString(), 1);
            }
            if (CampaignID != null)
            {
                drilldownType = "10";
                AddCookie("CampaignID", CampaignID.Value.ToString(), 1);
                AddCookie("CampaignDrillDownActivity", CampaignDrillDownActivity.Value.ToString(), 1);
                if (CampaignLinkID != null)
                    AddCookie("CampaignLinkID", CampaignLinkID.Value.ToString(), 1);
            }
            if (CampaignID != null && WorkflowID != null)
            {
                drilldownType = "9";
                AddCookie("WorkflowID", WorkflowID.Value.ToString(), 1);
                AddCookie("CampaignID", CampaignID.Value.ToString(), 1);
                AddCookie("CampaignDrillDownActivity", CampaignDrillDownActivity.Value.ToString(), 1);
                AddCookie("WfCampFromDate", WorflowCampFromDate.ToString(), 1);
                AddCookie("WfCampToDate", WorkflowCampToDate.ToString(), 1);
            }
            if (ActionID != null)
            {
                drilldownType = "11";
                AddCookie("ActionID", ActionID.ToString(), 1);
            }
            if (SSDefinitiationId != null)
            {
                drilldownType = "12";
                AddCookie("SSDefinitiationId", SSDefinitiationId.Value.ToString(), 1);
                AddCookie("SSType", SSType.Value.ToString(), 1);
            }
            if (isSavedSearch)
            {
            }

            if (NeverBounceRequestID != null)
            {
                drilldownType = "13";
                AddCookie("NeverBounceRequestID", NeverBounceRequestID.ToString(), 1);
                AddCookie("NeverBounceEmailStatus", NeverBounceEmailStatus.ToString(), 1);
            }
            if (OpportunityID != null)
            {
                drilldownType = "14";
                AddCookie("OpptyID", OpportunityID.ToString(), 1);
            }
            if (!string.IsNullOrEmpty(MyCommunicationType))
            {
                drilldownType = "15";
                AddCookie("MyCommunicationType", MyCommunicationType, 1);
                AddCookie("MyCommunicationPeriod", MyCommunicationPeriod, 1);
                AddCookie("MyCommunicationActivityType", MyCommunicationActivityType, 1);
            }

            dynamic result = default(dynamic);
            if (isSavedSearch || guid.HasValue || (IsNewContactsSearch.HasValue && IsNewContactsSearch.Value))
                result = await getContacts<ContactListEntry>(request.Page, request.PageSize, name, type, sorter, TagID, TagType, SSType, SSDefinitiationId, JobID, FormID, recordStatus, IsNewContactsSearch.Value,
                    false, IntegratedPersons, WorkflowID, WorkflowContactState, CampaignID, CampaignLinkID, CampaignDrillDownActivity, ActionID, isSavedSearch, guid, drilldownType, WorflowCampFromDate,
                    WorkflowCampToDate, NeverBounceRequestID, NeverBounceEmailStatus, OpportunityID, MyCommunicationType, MyCommunicationPeriod, MyCommunicationActivityType, sortField, direction);
            else
                result = await getContacts<ContactGridEntry>(request.Page, request.PageSize, name, type, sorter, TagID, TagType, SSType, SSDefinitiationId, JobID, FormID, recordStatus, false, false,
                    IntegratedPersons, WorkflowID, WorkflowContactState, CampaignID, CampaignLinkID, CampaignDrillDownActivity, ActionID, false, guid, drilldownType, WorflowCampFromDate, WorkflowCampToDate,
                    NeverBounceRequestID, NeverBounceEmailStatus, OpportunityID, MyCommunicationType, MyCommunicationPeriod, MyCommunicationActivityType, sortField, direction);
            return Json(new DataSourceResult
            {
                Data = (dynamic)result.Contacts,
                Total = (int)result.TotalHits
            }, JsonRequestBehavior.AllowGet);
        }

        async Task<dynamic> getContacts<T>(int page, int pageSize, string name, string type, string sort, int? TagID, int? TagType, int? SSType, int? SSDefinitionId, int? JobID, int? FormID, string recordStatus,
            bool isNewContactsSearch, bool navigationEnabled, string integratedpersons, short? WorkflowID, WorkflowContactsState? WorkflowContactState, int? CampaignID, int? CampaignLinkID,
            CampaignDrillDownActivity? CampaignDrillDownActivity, int? ActionID, bool isAdvancedSearch, Guid? guid, string drilldownType, DateTime? workflowCampFromDate, DateTime? worflowCampToDate,
            int? neverBounceRequestID, byte? neverBounceEmailStatus, int? OpportunityID, string myCommunicationTye, string myCommunicationPeriod, string myCommunicationActivityType, string sortField = null, ListSortDirection sortDirection = ListSortDirection.Descending) where T : IShallowContact
        {
            int[] ContactIDsList = null;
            string sortingValue = null;
            if (type == "4" && sort == "1")
                sortingValue = "1";
            else if (type == "4" && sort == "2")
                sortingValue = "2";
            else if (type == "4" && sort == "3")
                sortingValue = "3";
            int ShowingFieldValue = 0;
            if ((string.IsNullOrEmpty(sort) || sort.Equals("0")) && string.IsNullOrEmpty(name) && (!type.Equals("4")))
                sort = "1";
            else if ((string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(name)) || type.Equals("4"))
                sort = "0";
            if (type == "3" && drilldownType == String.Empty)
            {
                ShowingFieldValue = 4;
                GetRecentViwedContactsResponse response = GetUserCreatedContacts(null);
                if (!response.ContactIdList.IsAny())
                {
                    ContactIDsList = new List<int>().ToArray();
                }
                else
                {
                    ContactIDsList = response.ContactIdList.ToArray();
                }
            }
            else if (type == "4" && drilldownType == String.Empty)
            {
                var sortedValue = "";
                if (sortingValue == "1")
                    sortedValue = sortingValue == "1" ? "1" : null;
                else if (sortingValue == "2")
                    sortedValue = "2";
                else if (sortingValue == "3")
                    sortedValue = "3";
                Logger.Current.Informational(" Type filter value " + type + " and sort value value  " + sortedValue);
                ShowingFieldValue = 5;
                GetRecentViwedContactsResponse response = GetRecentViewedContacts(null, sortedValue);
                if (response.ContactIdList == null || response.ContactIdList.Count() < 0)
                {
                    ContactIDsList = new List<int>().ToArray();
                }
                else
                {
                    ContactIDsList = response.ContactIdList.ToArray();
                    Logger.Current.Informational(" recentily viewed Contacts count :" + ContactIDsList.Count());
                }
            }
            else if (drilldownType == "5")
            {
                ShowingFieldValue = 4;
                GetImportedContactResponse response = GetImportedContacts((int)JobID, recordStatus);
                if (response.ContactIdList == null || response.ContactIdList.Count() < 0)
                {
                    ContactIDsList = new List<int>().ToArray();
                }
                else
                {
                    ContactIDsList = response.ContactIdList.ToArray();
                }
            }
            else if (drilldownType == "6")
            {
                ShowingFieldValue = 4;
                GetTagContactsResponse response = GetTagRelatedContacts((int)TagID, TagType);
                if (response.ContactIdList == null || response.ContactIdList.Count() < 0)
                {
                    ContactIDsList = new List<int>().ToArray();
                }
                else
                {
                    ContactIDsList = response.ContactIdList.ToArray();
                }
            }
            else if (drilldownType == "7")
            {
                ShowingFieldValue = 4;
                GetFormContactsResponse response = GetFormRelatedContacts((int)FormID);
                if (response.ContactIdList == null || response.ContactIdList.Count() < 0)
                {
                    ContactIDsList = new List<int>().ToArray();
                }
                else
                {
                    ContactIDsList = response.ContactIdList.ToArray();
                }
            }
            else if (drilldownType == "8")
            {
                ShowingFieldValue = 4;
                ContactIDsList = integratedpersons.Split(',').Select(int.Parse).ToArray();
            }
            else if (drilldownType == "9")
            {
                ShowingFieldValue = 4;
                if (WorkflowContactState != null)
                {
                    GetWorkflowContactsResponse response = GetWorklfowRelatedContacts(WorkflowID.Value, WorkflowContactState.Value);
                    if (response.ContactIdList == null || response.ContactIdList.Count() < 0)
                    {
                        ContactIDsList = new List<int>().ToArray();
                    }
                    else
                    {
                        ContactIDsList = response.ContactIdList.ToArray();
                    }
                }
                else
                {
                    GetWorkflowContactsResponse response = GetWorkflowCampaignRelatedContacts(WorkflowID.Value, CampaignID.Value, CampaignDrillDownActivity.Value, workflowCampFromDate, worflowCampToDate);
                    if (response.ContactIdList == null || response.ContactIdList.Count() < 0)
                    {
                        ContactIDsList = new List<int>().ToArray();
                    }
                    else
                    {
                        ContactIDsList = response.ContactIdList.ToArray();
                    }
                }
            }
            else if (drilldownType == "10")
            {
                ShowingFieldValue = 4;
                GetCampaignContactsResponse response = GetCampaignRelatedContacts(CampaignID.Value, CampaignDrillDownActivity.Value, CampaignLinkID);
                if (response.ContactIdList == null || response.ContactIdList.Count() < 0)
                {
                    ContactIDsList = new List<int>().ToArray();
                }
                else
                {
                    ContactIDsList = response.ContactIdList.ToArray();
                }
            }
            else if (drilldownType == "11")
            {
                ShowingFieldValue = 4;
                GetActionRelatedContactsResponce response = GetActionRelatedContacts((int)ActionID);
                if (response.ContactIdList == null || response.ContactIdList.Count() < 0)
                {
                    ContactIDsList = new List<int>().ToArray();
                }
                else
                {
                    ContactIDsList = response.ContactIdList.ToArray();
                }
            }
            else if (drilldownType == "12")
            {
                ShowingFieldValue = 4;
                GetSearchDefinitionContactsResponce response = await GetSearchDefinitionContacts((int)SSDefinitionId, (byte)SSType);
                if (response.ContactIdList == null || response.ContactIdList.Count() < 0)
                {
                    ContactIDsList = new List<int>().ToArray();
                }
                else
                {
                    ContactIDsList = response.ContactIdList.ToArray();
                }
            }
            else if (isAdvancedSearch)
            {
            }
            else if (guid.HasValue && guid.Value != new Guid())
            {
                var key = "SavedSearchContactIds" + guid.Value;
                var contactIDs = cachingService.GetSavedSearchContactIds(key);
                if (contactIDs != null)
                    ContactIDsList = contactIDs.ToArray();
                else
                    ContactIDsList = new List<int>().ToArray();
            }
            else if (drilldownType == "13")
            {
                ShowingFieldValue = 4;
                GetNeverBounceBadEmailContactResponse response = GetNeverBounceBadEmailContactIds((int)neverBounceRequestID, neverBounceEmailStatus.Value);
                if (response.ContactIdList == null || response.ContactIdList.Count() < 0)
                {
                    ContactIDsList = new List<int>().ToArray();
                }
                else
                {
                    ContactIDsList = response.ContactIdList.ToArray();
                }
            }
            else if (drilldownType == "14")
            {
                ShowingFieldValue = 4;
                GetOpportunityContactsResponse response = GetOpportunityContacts(OpportunityID.Value);
                if (response.ContactIdList == null || response.ContactIdList.Count() < 0)
                {
                    ContactIDsList = new List<int>().ToArray();
                }
                else
                {
                    ContactIDsList = response.ContactIdList.ToArray();
                }
            }
            //else if (drilldownType == "15")
            //{
            //    ShowingFieldValue = 4;
            //    GetMyCommunicationContactsResponse response = GetMyCommunicationContacts(myCommunicationTye, myCommunicationActivityType, myCommunicationPeriod);
            //    if (response.ContactIdList == null || response.ContactIdList.Count() < 0)
            //    {
            //        ContactIDsList = new List<int>().ToArray();
            //    }
            //    else
            //    {
            //        ContactIDsList = response.ContactIdList.ToArray();
            //    }
            //}
            if (!navigationEnabled)
            {
                AddCookie("pagesize", pageSize.ToString(), 1);
                AddCookie("pagenumber", page.ToString(), 1);
                AddCookie("filtertype", type, 1);
                AddCookie("searchtext", name, 1);
                AddCookie("sorttype", sort, 1);
            }
            SearchContactsResponse<T> searchresponse = new SearchContactsResponse<T>();
            if ((string.Equals(type, "2") || string.Equals(type, "3") || string.Equals(type, "4")) && !isAdvancedSearch && !isNewContactsSearch && drilldownType == String.Empty)
            {
                var p = page;
                var contacts = ContactIDsList;
                if (string.Equals(type, "4"))
                {
                    p = 1;
                    if (string.IsNullOrEmpty(name))
                        contacts = ContactIDsList.Skip((page - 1) * pageSize).Take(pageSize).ToArray();
                    else
                        contacts = ContactIDsList.ToArray();
                    Logger.Current.Informational("Giving contactIds to elastic search: " + contacts.Count());
                }
                searchresponse = contactService.GetAllContacts<T>(new SearchContactsRequest()
                {
                    Query = name,
                    Limit = pageSize,
                    PageNumber = p,
                    SortFieldType = (ContactSortFieldType)Convert.ToInt16(sort),
                    AccountId = this.Identity.ToAccountID(),
                    ContactIDs = contacts,
                    ShowingFieldType = (ContactShowingFieldType)ShowingFieldValue,
                    RequestedBy = this.Identity.ToUserID(),
                    SortField = sortField,
                    IsResultsGrid = true,
                    RoleId = this.Identity.ToRoleID()
                });
            }
            else if (type == "0" && !isAdvancedSearch && !isNewContactsSearch && drilldownType == String.Empty)
                searchresponse = contactService.GetPersons<T>(new SearchContactsRequest()
                {
                    Query = name,
                    Limit = pageSize,
                    PageNumber = page,
                    SortFieldType = (ContactSortFieldType)Convert.ToInt16(sort),
                    AccountId = this.Identity.ToAccountID(),
                    RequestedBy = this.Identity.ToUserID(),
                    RoleId = this.Identity.ToRoleID(),
                    ContactIDs = ContactIDsList,
                    SortField = sortField,
                    SortDirection = sortDirection,
                    IsResultsGrid = true
                });
            else if (type == "1" && !isAdvancedSearch && !isNewContactsSearch && drilldownType == String.Empty)
                searchresponse = contactService.GetCompanies<T>(new SearchContactsRequest()
                {
                    Query = name,
                    Limit = pageSize,
                    PageNumber = page,
                    SortFieldType = (ContactSortFieldType)Convert.ToInt16(sort),
                    AccountId = this.Identity.ToAccountID(),
                    RequestedBy = this.Identity.ToUserID(),
                    RoleId = this.Identity.ToRoleID(),
                    ContactIDs = ContactIDsList,
                    SortField = sortField,
                    SortDirection = sortDirection,
                });
            else if (drilldownType == "5" || drilldownType == "6" || drilldownType == "7" || drilldownType == "8" || drilldownType == "9" || drilldownType == "10" || drilldownType == "11" || drilldownType == "12" || drilldownType == "13" || drilldownType == "14" || drilldownType == "15" || isAdvancedSearch || isNewContactsSearch)
            {
                if (ContactIDsList != null && !ContactIDsList.IsAny())
                {
                    searchresponse = new SearchContactsResponse<T>()
                    {
                        Contacts = new List<T>()
                    };
                }
                else if (isAdvancedSearch)
                {
                    short ItemsPerPage = default(short);
                    short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
                    IEnumerable<Type> contactType = type == "0" ? new List<Type>() {
                        typeof(Person)
                    } : type == "1" ? new List<Type>() {
                        typeof(Company)
                    } : new List<Type>() {
                        typeof(Person),
                        typeof(Company)
                    };
                    var viewModel = (AdvancedSearchViewModel)Session["AdvancedSearchVM"];
                    AdvancedSearchResponse<ContactListEntry> asresponse = await advancedSearchService.RunSearchAsync(new AdvancedSearchRequest<ContactListEntry>()
                    {
                        SearchViewModel = viewModel,
                        AccountId = this.Identity.ToAccountID(),
                        RoleId = this.Identity.ToRoleID(),
                        RequestedBy = this.Identity.ToUserID(),
                        IsAdvancedSearch = true,
                        Limit = pageSize,
                        PageNumber = page,
                        ViewContacts = true,
                        Query = name,
                        SortFieldType = (ContactSortFieldType)Convert.ToInt16(sort),
                        ShowingFieldType = (ContactShowingFieldType)ShowingFieldValue,
                        SortField = sortField,
                        SortDirection = sortDirection,
                        IsResultsGrid = true,
                        ContactTypes = contactType,
                        ShowByCreated = (type == "3") ? true : false
                    });
                    searchresponse.TotalHits = asresponse.SearchResult.TotalHits;
                    searchresponse.Contacts = (IEnumerable<T>)asresponse.SearchResult.Results;
                }
                else if (isNewContactsSearch == true)
                {
                    short ItemsPerPage = default(short);
                    short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
                    IEnumerable<Type> contactType = type == "0" ? new List<Type>() {
                        typeof(Person)
                    } : type == "1" ? new List<Type>() {
                        typeof(Campaign)        //We shouldn't show company records for NewContactsSearch NEXG-2691
                    } : new List<Type>() {
                        typeof(Person)
                        //typeof(Company)
                    };
                    var session = Session;
                    var viewModel = (AdvancedSearchViewModel)Session["NewContactsSearchViewModel"];
                    AdvancedSearchResponse<ContactListEntry> asresponse = await advancedSearchService.RunSearchAsync(new AdvancedSearchRequest<ContactListEntry>()
                    {
                        SearchViewModel = viewModel,
                        AccountId = this.Identity.ToAccountID(),
                        RoleId = this.Identity.ToRoleID(),
                        RequestedBy = this.Identity.ToUserID(),
                        IsAdvancedSearch = true,
                        Limit = pageSize,
                        PageNumber = page,
                        ViewContacts = true,
                        Query = name,
                        SortFieldType = (ContactSortFieldType)Convert.ToInt16(sort),
                        ShowingFieldType = (ContactShowingFieldType)ShowingFieldValue,
                        SortField = sortField,
                        SortDirection = sortDirection,
                        IsResultsGrid = true,
                        ContactTypes = contactType,
                        ShowByCreated = (type == "3") ? true : false
                    });
                    searchresponse.TotalHits = asresponse.SearchResult.TotalHits;
                    searchresponse.Contacts = (IEnumerable<T>)asresponse.SearchResult.Results;
                }
                else
                {
                    if (ContactIDsList == null)
                        searchresponse = new SearchContactsResponse<T>()
                        {
                            Contacts = new List<T>()
                        };
                    else
                    {
                        if ((string.Equals(type, "2") || string.Equals(type, "3") || string.Equals(type, "4")))
                        {
                            if (type == "3")
                            {
                                ShowingFieldValue = 4;
                                GetRecentViwedContactsResponse response = GetUserCreatedContacts(ContactIDsList);
                                if (!response.ContactIdList.IsAny())
                                {
                                    ContactIDsList = new List<int>().ToArray();
                                }
                                else
                                {
                                    ContactIDsList = response.ContactIdList.ToArray();
                                }
                            }
                            else if (type == "4")
                            {
                                ShowingFieldValue = 5;
                                GetRecentViwedContactsResponse response = GetRecentViewedContacts(ContactIDsList, null);
                                if (response.ContactIdList == null || response.ContactIdList.Count() < 0)
                                {
                                    ContactIDsList = new List<int>().ToArray();
                                }
                                else
                                {
                                    ContactIDsList = response.ContactIdList.ToArray();
                                }
                            }
                            if (ContactIDsList.Length == 0)
                                searchresponse = new SearchContactsResponse<T>()
                                {
                                    Contacts = new List<T>()
                                };
                            else
                                searchresponse = contactService.GetAllContacts<T>(new SearchContactsRequest()
                                {
                                    Query = name,
                                    Limit = pageSize,
                                    PageNumber = page,
                                    SortFieldType = (ContactSortFieldType)Convert.ToInt16(sort),
                                    AccountId = this.Identity.ToAccountID(),
                                    ContactIDs = ContactIDsList,
                                    ShowingFieldType = (ContactShowingFieldType)ShowingFieldValue,
                                    RequestedBy = this.Identity.ToUserID(),
                                    RoleId = this.Identity.ToRoleID()
                                });
                        }
                        else if (type == "0" && !isAdvancedSearch && !isNewContactsSearch)
                            searchresponse = contactService.GetPersons<T>(new SearchContactsRequest()
                            {
                                Query = name,
                                Limit = pageSize,
                                PageNumber = page,
                                SortFieldType = (ContactSortFieldType)Convert.ToInt16(sort),
                                AccountId = this.Identity.ToAccountID(),
                                RequestedBy = this.Identity.ToUserID(),
                                RoleId = this.Identity.ToRoleID(),
                                ContactIDs = ContactIDsList,
                                SortField = sortField,
                                SortDirection = sortDirection,
                            });
                        else if (type == "1" && !isAdvancedSearch && !isNewContactsSearch)
                            searchresponse = contactService.GetCompanies<T>(new SearchContactsRequest()
                            {
                                Query = name,
                                Limit = pageSize,
                                PageNumber = page,
                                SortFieldType = (ContactSortFieldType)Convert.ToInt16(sort),
                                AccountId = this.Identity.ToAccountID(),
                                RequestedBy = this.Identity.ToUserID(),
                                RoleId = this.Identity.ToRoleID(),
                                ContactIDs = ContactIDsList,
                                SortField = sortField,
                                SortDirection = sortDirection,
                            });
                    }
                }
            }
            Session["TotalHits"] = searchresponse.TotalHits.ToString();
            Session["ContactListIDs"] = ContactIDsList;
            var dropdowns = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            if (searchresponse.Contacts != null)
            {
                if (string.Equals(type, "4") && string.IsNullOrEmpty(name))
                {
                    int accountId = this.Identity.ToAccountID();
                    short roleId = this.Identity.ToRoleID();
                    var orderedlist = ContactIDsList.Join(searchresponse.Contacts, cl => cl, c => (int)((object)c).GetType().GetProperty("ContactID").GetValue(c, null), (cl, c) => c);
                    searchresponse.Contacts = orderedlist;
                    bool isadmin = cachingService.IsAccountAdmin(roleId, accountId);
                    bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, accountId);
                    if (isPrivate && !isadmin)
                    {
                        if (orderedlist.Count() <= 10)
                            searchresponse.TotalHits = orderedlist.Count();
                        else
                            searchresponse.TotalHits = ContactIDsList.Count();
                    }
                    else if (string.IsNullOrEmpty(name))
                        searchresponse.TotalHits = ContactIDsList.Count();
                    Logger.Current.Informational("Final result contacts count from elastic: " + orderedlist.Count());
                }
                var lifecycleStages = dropdowns.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LifeCycle).Select(s => s.DropdownValuesList).ToList().FirstOrDefault();
                foreach (var contact in searchresponse.Contacts)
                {
                    object obj = (object)contact;
                    var lifeCycleStageProp = obj.GetType().GetProperty("LifecycleStage");
                    var lifeCycleNameProp = obj.GetType().GetProperty("LifecycleName");
                    var lastTouchedProp = obj.GetType().GetProperty("LastTouched");
                    var lastContactedDateProp = obj.GetType().GetProperty("LastContactedDate");
                    var lastTouchedThroughProp = obj.GetType().GetProperty("LastTouchedThrough");
                    lifeCycleNameProp.SetValue(obj, lifecycleStages.Where(e => e.DropdownValueID == (short)lifeCycleStageProp.GetValue(obj, null)).Select(s => s.DropdownValue).FirstOrDefault());
                    if (lastContactedDateProp.GetValue(obj, null) != null && typeof(T).Equals(typeof(ContactGridEntry)))
                    {
                        lastTouchedProp.SetValue(obj, ((DateTime)lastContactedDateProp.GetValue(obj, null)).ToJSDate().ToString(this.Identity.ToDateFormat(), CultureInfo.InvariantCulture) + " (" + lastTouchedThroughProp.GetValue(obj, null) + ")");
                    }
                }
            }
            List<int?> Companyids = new List<int?>();
            if (typeof(T).Equals(typeof(ContactGridEntry)))
            {
                var contacts = (IEnumerable<ContactGridEntry>)searchresponse.Contacts;
                Companyids = contacts.Where(p => p.ContactType == 1 && p.CompanyID != null).Select(p => p.CompanyID).ToList();
                IEnumerable<Contact> companies = contactService.GetAllContactsByCompanyIds(Companyids, this.Identity.ToAccountID());
                foreach (var person in contacts.Where(p => p.ContactType == 1 && p.CompanyID != null))
                {
                    foreach (var company in companies)
                    {
                        if (person.CompanyID == company.Id && company.IsDeleted == false)
                            person.CompanyName = company.CompanyName;
                    }
                }
                foreach (var person in contacts.Where(p => p.ContactType == 1))
                {
                    string firstname = "";
                    string lastname = "";
                    if (!string.IsNullOrEmpty(person.FirstName))
                    {
                        if (person.FirstName.Length > 35)
                        {
                            firstname = person.FirstName.Substring(0, 34);
                            firstname = firstname + "...";
                        }
                        else
                        {
                            firstname = person.FirstName;
                        }
                    }
                    if (!string.IsNullOrEmpty(person.LastName))
                    {
                        if (person.LastName.Length > 35)
                        {
                            lastname = person.LastName.Substring(0, 34);
                            lastname = lastname + "...";
                        }
                        else
                        {
                            lastname = person.LastName;
                        }
                    }
                    if (!string.IsNullOrEmpty(person.FirstName) && !string.IsNullOrEmpty(person.LastName) && (person.FirstName.Length > 35 || person.LastName.Length > 35))
                    {
                        person.Name = firstname + " " + lastname;
                    }
                    person.FullName = person.FirstName + " " + person.LastName;
                }
            }
            return new
            {
                Contacts = (dynamic)searchresponse.Contacts,
                TotalHits = (int)searchresponse.TotalHits
            };
        }

        [Route("advancedview/{searchdefinitionId?}")]       //Request from AdvancedSearchList
        //[Route("contactresults")]                         //Request from Edit/View SavedSearch
        [SmarttouchSessionStateBehaviour(SessionStateBehavior.Required)]
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public async Task<ActionResult> SavedSearchAdvancedView(int? searchdefinitionId)
        {
            var accountId = this.Identity.ToAccountID();
            var roleId = this.Identity.ToRoleID();
            var userId = this.Identity.ToUserID();
            short ItemsPerPage = default(short);
            short.TryParse(Identity.ToItemsPerPage(), out ItemsPerPage);
            bool IsAccountAdmin = cachingService.IsAccountAdmin(roleId, accountId);
            AdvancedViewViewModel advancedViewModel = new AdvancedViewViewModel();
            advancedViewModel.AccountID = accountId;
            advancedViewModel.ItemsPerPage = ItemsPerPage;
            advancedViewModel.HasEmailPermission = cachingService.CheckSendMailPermissions(accountId, roleId);
            advancedViewModel.IsAccountAdmin = IsAccountAdmin;
            GetAdvanceSearchFieldsResponse response = advancedSearchService.GetSearchFields(new GetAdvanceSearchFieldsRequest()
            {
                accountId = accountId,
                RoleId = roleId
            });
            advancedViewModel.SearchFields = ExcludeNonViewableFields(response.FieldsViewModel);
            advancedViewModel.LeadAdapterTypes = leadAdapterService.GetAllLeadadapterTypes();
            RemoveCookie("contactid");
            IList<FilterViewModel> SearchFilters = null;
            int searchDefinitionID = 0;
            string searchName = string.Empty;
            ViewBag.IsAdvancedGrid = true;
            #region AdvancedSearch
            if (searchdefinitionId == null)     //Request from Edit/View SavedSearch
            {
                var viewModel = (AdvancedSearchViewModel)Session["AdvancedSearchVM"];
                searchName = !string.IsNullOrEmpty(viewModel.SearchDefinitionName) ? viewModel.SearchDefinitionName : ReadCookie("savedsearchname");
                short searchDefinitionId = 0;
                bool success = short.TryParse(ReadCookie("savedsearchid"), out searchDefinitionId);
                if (!success)
                    searchDefinitionId = viewModel.SearchDefinitionID;

                advancedViewModel.IsPreconfigured = viewModel.IsPreConfiguredSearch;
                advancedViewModel.IsFavorite = viewModel.IsFavoriteSearch;
                if (searchDefinitionId != default(int))
                    advancedViewModel.SearchDescription = advancedSearchService.GetSearchDefinitionDescription(new GetSearchDefinitionDescriptionRequest()
                    {
                        SearchDefinitionId = searchDefinitionId
                    }).Title;
                else
                    advancedViewModel.SearchDescription = string.Empty;
                SearchFilters = viewModel.SearchFilters;
                searchDefinitionID = viewModel.SearchDefinitionID;
            }
            else if (searchdefinitionId != null && searchdefinitionId.HasValue)     //Request from AdvancedSearchList
            {
                GetSearchResponse savesSearchResponse = await advancedSearchService.GetSavedSearchAsync(new GetSearchRequest()
                {
                    SearchDefinitionID = searchdefinitionId.Value,
                    IncludeSearchResults = false,
                    Limit = ItemsPerPage,
                    AccountId = accountId,
                    RoleId = roleId,
                    RequestedBy = userId,
                    IsSTAdmin = IsAccountAdmin
                });
                advancedSearchService.InsertLastRun(new InsertLastRunActivityRequest()
                {
                    AccountId = accountId,
                    RequestedBy = userId,
                    SearchDefinitionId = searchdefinitionId.Value,
                    SearchName = savesSearchResponse.SearchViewModel.SearchDefinitionName
                });
                Session["AdvancedSearchVM"] = savesSearchResponse.SearchViewModel;
                advancedViewModel.SearchDescription = advancedSearchService.GetSearchDefinitionDescription(new GetSearchDefinitionDescriptionRequest()
                {
                    SearchDefinitionId = searchdefinitionId.Value
                }).Title;
                SearchFilters = savesSearchResponse.SearchViewModel.SearchFilters;
                searchDefinitionID = savesSearchResponse.SearchViewModel.SearchDefinitionID;
                searchName = savesSearchResponse.SearchViewModel.SearchDefinitionName;
            }
            advancedViewModel.SelectedFields = GetColumnPreferences(SearchFilters, searchDefinitionID, 1);
            advancedViewModel.EntityType = (short)ShowingType.AdvancedSearch;
            advancedViewModel.EntityId = searchDefinitionID;
            advancedViewModel.SearchName = searchName;
            advancedViewModel.IsSavedSearch = true;
            #endregion

            GetCustomFieldsValueOptionsRequest request = new GetCustomFieldsValueOptionsRequest() { AccountId = accountId };
            GetCustomFieldsValueOptionsResponse customFieldsResponse = customFieldService.GetCustomFieldValueOptions(request);
            advancedViewModel.CustomFieldValueOptions = customFieldsResponse.CustomFieldValueOptions;
            return View("ContactResultsView2", advancedViewModel);
        }

        [Route("gridredirection/{guid}")]
        [SmarttouchSessionStateBehaviour(SessionStateBehavior.Required)]
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public ActionResult SavedSearchContacts(Guid guid)
        {
            var identity = this.Identity;
            AdvancedViewViewModel advancedViewModel = new AdvancedViewViewModel();
            var accountId = identity.ToAccountID();
            var roleId = identity.ToRoleID();
            short ItemsPerPage = default(short);
            ViewBag.IsAdvancedGrid = true;

            short.TryParse(identity.ToItemsPerPage(), out ItemsPerPage);
            advancedViewModel.ItemsPerPage = ItemsPerPage;
            bool isAccountAdmin = cachingService.IsAccountAdmin(roleId, accountId);

            advancedViewModel.IsSavedSearch = false;
            advancedViewModel.AccountID = accountId;
            advancedViewModel.Guid = guid;        //Forms, Reports, Dashboard - Untouched, Contacts, Forms
            advancedViewModel.SelectedFields = new List<int>() { 1, 2, 3, 7, 22, 41 };
            advancedViewModel.IsDynamicGrid = true;
            advancedViewModel.HasEmailPermission = cachingService.CheckSendMailPermissions(accountId, roleId);
            advancedViewModel.IsAccountAdmin = isAccountAdmin;

            GetAdvanceSearchFieldsResponse response = advancedSearchService.GetSearchFields(new GetAdvanceSearchFieldsRequest()
            {
                accountId = accountId,
                RoleId = roleId
            });
            advancedViewModel.SearchFields = ExcludeNonViewableFields(response.FieldsViewModel);

            ContactGridViewModel viewModel = cachingService.GetGridViewModel(guid.ToString());
            advancedViewModel.PageNumber = viewModel.PageNo;
            advancedViewModel.ItemsPerPage = viewModel.PageCount;
            advancedViewModel.SearchText = viewModel.SearchString;
            advancedViewModel.ShowingType = viewModel.ShowingField == "0" ? ContactShowingFieldType.People : (viewModel.ShowingField == "1") ? ContactShowingFieldType.Companies : (viewModel.ShowingField == "2") ?
                ContactShowingFieldType.PeopleAndComapnies : (viewModel.ShowingField == "3") ? ContactShowingFieldType.MyContacts : (viewModel.ShowingField == "4") ? ContactShowingFieldType.RecentlyViewed :
                ContactShowingFieldType.PeopleAndComapnies;
            advancedViewModel.SortField = viewModel.SortingField;
            advancedViewModel.EntityType = (short)ShowingType.ContactsGrid;

            GetCustomFieldsValueOptionsRequest request = new GetCustomFieldsValueOptionsRequest() { AccountId = accountId };
            GetCustomFieldsValueOptionsResponse customFieldsResponse = customFieldService.GetCustomFieldValueOptions(request);
            advancedViewModel.CustomFieldValueOptions = customFieldsResponse.CustomFieldValueOptions;
            advancedViewModel.LeadAdapterTypes = leadAdapterService.GetAllLeadadapterTypes();
            RemoveCookie("contactid");
            return View("ContactResultsView2", advancedViewModel);
        }

        [Route("formcontacts/{guid?}/{reportType?}/{reportId?}")]
        [SmarttouchSessionStateBehaviour(SessionStateBehavior.Required)]
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public ActionResult FromSubmissions(Guid? guid, byte? reportType, int? reportId, int? formId)
        {
            var identity = this.Identity;
            AdvancedViewViewModel advancedViewModel = new AdvancedViewViewModel();
            var accountId = identity.ToAccountID();
            var roleId = identity.ToRoleID();
            short ItemsPerPage = default(short);
            ViewBag.IsAdvancedGrid = true;

            short.TryParse(identity.ToItemsPerPage(), out ItemsPerPage);
            advancedViewModel.ItemsPerPage = ItemsPerPage;
            bool isAccountAdmin = cachingService.IsAccountAdmin(roleId, accountId);

            advancedViewModel.IsSavedSearch = false;
            advancedViewModel.AccountID = accountId;
            advancedViewModel.Guid = guid.Value;        //Forms, Reports, Dashboard - Untouched, Contacts, Forms
            advancedViewModel.SelectedFields = new List<int>() { 1, 2, 3, 7, 22, 41 };
            advancedViewModel.IsDynamicGrid = true;
            advancedViewModel.HasEmailPermission = cachingService.CheckSendMailPermissions(accountId, roleId);
            advancedViewModel.IsAccountAdmin = isAccountAdmin;

            GetAdvanceSearchFieldsResponse response = advancedSearchService.GetSearchFields(new GetAdvanceSearchFieldsRequest()
            {
                accountId = accountId,
                RoleId = roleId
            });
            advancedViewModel.SearchFields = ExcludeNonViewableFields(response.FieldsViewModel);

            advancedViewModel.EntityId = reportId.HasValue ? reportId.Value : 10;
            advancedViewModel.EntityType = (byte)ShowingType.Forms;
            if (reportId.HasValue)
            {
                if (formId.HasValue && formId > 0)
                {
                    Logger.Current.Verbose("Fetching columns for form : " + formId);

                    var formFields = formService.GetFormFieldIDs(new GetFormFieldIDsRequest() { FormID = (int)formId }).FieldIDs;
                    var newFormFields = formFields.ToList();
                    newFormFields.Add(51); // 51 = ContactPrimaryLeadSource
                    advancedViewModel.SelectedFields = newFormFields;
                }
                else
                    advancedViewModel.SelectedFields = GetColumnPreferences(null, reportId.Value, 6);
            }
            GetCustomFieldsValueOptionsRequest request = new GetCustomFieldsValueOptionsRequest() { AccountId = accountId };
            GetCustomFieldsValueOptionsResponse customFieldsResponse = customFieldService.GetCustomFieldValueOptions(request);
            advancedViewModel.CustomFieldValueOptions = customFieldsResponse.CustomFieldValueOptions;
            advancedViewModel.LeadAdapterTypes = leadAdapterService.GetAllLeadadapterTypes();
            return View("ContactResultsView2", advancedViewModel);
        }

        [Route("ncrcontacts/{reportType?}/{reportId?}")]
        [SmarttouchSessionStateBehaviour(SessionStateBehavior.Required)]
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public ActionResult NewContactsReportContacts(byte? reportType, int? reportId, string reportName)
        {
            var identity = this.Identity;
            AdvancedViewViewModel advancedViewModel = new AdvancedViewViewModel();
            var accountId = identity.ToAccountID();
            var roleId = identity.ToRoleID();
            short ItemsPerPage = default(short);
            ViewBag.IsAdvancedGrid = true;

            short.TryParse(identity.ToItemsPerPage(), out ItemsPerPage);
            advancedViewModel.ItemsPerPage = ItemsPerPage;
            bool isAccountAdmin = cachingService.IsAccountAdmin(roleId, accountId);

            advancedViewModel.IsSavedSearch = false;
            advancedViewModel.AccountID = accountId;
            advancedViewModel.SelectedFields = new List<int>() { 1, 2, 3, 7, 22, 41 };
            advancedViewModel.IsDynamicGrid = true;
            advancedViewModel.HasEmailPermission = cachingService.CheckSendMailPermissions(accountId, roleId);
            advancedViewModel.IsAccountAdmin = isAccountAdmin;
            advancedViewModel.ReportName = reportName;
            GetAdvanceSearchFieldsResponse response = advancedSearchService.GetSearchFields(new GetAdvanceSearchFieldsRequest()
            {
                accountId = accountId,
                RoleId = roleId
            });
            advancedViewModel.SearchFields = ExcludeNonViewableFields(response.FieldsViewModel);

            advancedViewModel.ShowingType = ContactShowingFieldType.People;
            advancedViewModel.EntityId = reportId.HasValue ? reportId.Value : 0;
            advancedViewModel.EntityType = (byte)ShowingType.NewContactsReport;
            var viewModel = (AdvancedSearchViewModel)Session["NewContactsSearchViewModel"];
            if (reportId.HasValue)
                advancedViewModel.SelectedFields = GetColumnPreferences(viewModel.SearchFilters, reportId.Value, 7);

            GetCustomFieldsValueOptionsRequest request = new GetCustomFieldsValueOptionsRequest() { AccountId = accountId };
            GetCustomFieldsValueOptionsResponse customFieldsResponse = customFieldService.GetCustomFieldValueOptions(request);
            advancedViewModel.CustomFieldValueOptions = customFieldsResponse.CustomFieldValueOptions;
            advancedViewModel.LeadAdapterTypes = leadAdapterService.GetAllLeadadapterTypes();
            return View("ContactResultsView2", advancedViewModel);
        }

        [Route("reportcontacts/{guid?}/{reportType?}/{reportId?}/{reportName?}")]
        [SmarttouchSessionStateBehaviour(SessionStateBehavior.Required)]
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public ActionResult ReportContacts(Guid? guid, byte? reportType, int? reportId, string reportName)
        {
            var identity = this.Identity;
            AdvancedViewViewModel advancedViewModel = new AdvancedViewViewModel();
            var accountId = identity.ToAccountID();
            var roleId = identity.ToRoleID();
            short ItemsPerPage = default(short);
            ViewBag.IsAdvancedGrid = true;

            short.TryParse(identity.ToItemsPerPage(), out ItemsPerPage);
            advancedViewModel.ItemsPerPage = ItemsPerPage;
            bool isAccountAdmin = cachingService.IsAccountAdmin(roleId, accountId);

            advancedViewModel.IsSavedSearch = false;
            advancedViewModel.AccountID = accountId;
            advancedViewModel.Guid = guid.Value;        //Forms, Reports, Dashboard - Untouched, Contacts, Forms
            advancedViewModel.SelectedFields = new List<int>() { 1, 2, 3, 7, 22, 41 };
            advancedViewModel.IsDynamicGrid = true;
            advancedViewModel.HasEmailPermission = cachingService.CheckSendMailPermissions(accountId, roleId);
            advancedViewModel.IsAccountAdmin = isAccountAdmin;
            advancedViewModel.LeadAdapterTypes = leadAdapterService.GetAllLeadadapterTypes();
            GetAdvanceSearchFieldsResponse response = advancedSearchService.GetSearchFields(new GetAdvanceSearchFieldsRequest()
            {
                accountId = accountId,
                RoleId = roleId
            });
            advancedViewModel.SearchFields = ExcludeNonViewableFields(response.FieldsViewModel);

            advancedViewModel.EntityId = reportId.HasValue ? reportId.Value : 0;
            advancedViewModel.EntityType = (byte)ShowingType.Report;
            advancedViewModel.SelectedFields = GetColumnPreferences(null, reportId.Value, 2);
            GetCustomFieldsValueOptionsRequest request = new GetCustomFieldsValueOptionsRequest() { AccountId = accountId };
            GetCustomFieldsValueOptionsResponse customFieldsResponse = customFieldService.GetCustomFieldValueOptions(request);
            advancedViewModel.CustomFieldValueOptions = customFieldsResponse.CustomFieldValueOptions;
            return View("ContactResultsView2", advancedViewModel);
        }

        [SmarttouchSessionStateBehaviour(SessionStateBehavior.Required)]
        public async Task<ActionResult> ContactsAdvancedView1(string advancedSearchModel)
        {
            dynamic result = default(dynamic);
            AdvancedViewViewModel model = JsonConvert.DeserializeObject<AdvancedViewViewModel>(advancedSearchModel);
            if (model.EntityType == (short)ShowingType.AdvancedSearch)
                result = await getAdvancedSearchContacts<ContactListEntry>(model);

            else if (model.EntityType == (short)ShowingType.Report || model.EntityType == (short)ShowingType.TagsReport || model.EntityType == (short)ShowingType.Action || model.EntityType == (short)ShowingType.Opportunity
                || model.EntityType == (short)ShowingType.Forms)
                result = getAdvancedViewContacts<ContactListEntry>(model);

            else if (model.EntityType == (short)ShowingType.ContactsGrid)
                result = getContactsGridContacts<ContactListEntry>(model);

            else if (model.EntityType == (short)ShowingType.NewContactsReport)
                result = await getNewContactsReportContacts<ContactListEntry>(model);
            return Json(new
            {
                Data = (IEnumerable<ContactListEntry>)result.Data,
                Total = (int)result.Total
            }, JsonRequestBehavior.AllowGet);
        }

        private async Task<dynamic> getAdvancedSearchContacts<T>(AdvancedViewViewModel advancedSearchModel) where T : IShallowContact
        {
            int[] ContactIDsList = null;
            SearchContactsResponse<T> searchresponse = new SearchContactsResponse<T>();

            IEnumerable<Type> contactType = advancedSearchModel.ShowingType == ContactShowingFieldType.People ? new List<Type>() {
                        typeof(Person)
                    } : advancedSearchModel.ShowingType == ContactShowingFieldType.Companies ? new List<Type>() {
                        typeof(Company)
                    } : new List<Type>() {
                        typeof(Person),
                        typeof(Company)
                    };

            var viewModel = (AdvancedSearchViewModel)Session["AdvancedSearchVM"];
            AdvancedSearchResponse<ContactListEntry> asresponse = await advancedSearchService.RunSearchAsync(new AdvancedSearchRequest<ContactListEntry>()
            {
                SearchViewModel = viewModel,
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID(),
                RequestedBy = this.Identity.ToUserID(),
                IsAdvancedSearch = true,
                Limit = advancedSearchModel.ItemsPerPage,
                PageNumber = advancedSearchModel.PageNumber,
                ViewContacts = true,
                Query = advancedSearchModel.SearchText,
                SortFieldType = (ContactSortFieldType)Convert.ToInt16(advancedSearchModel.SortBy),
                ShowingFieldType = advancedSearchModel.ShowingType,
                SortField = advancedSearchModel.SortField == "" ? null : advancedSearchModel.SortField,
                SortDirection = advancedSearchModel.SortDirection,
                IsResultsGrid = true,
                ContactTypes = contactType,
                ShowByCreated = ContactShowingFieldType.MyContacts == advancedSearchModel.ShowingType ? true : false
            });
            searchresponse.TotalHits = asresponse.SearchResult.TotalHits;
            searchresponse.Contacts = (IEnumerable<T>)asresponse.SearchResult.Results;

            Session["TotalHits"] = searchresponse.TotalHits.ToString();
            Session["ContactListIDs"] = ContactIDsList;
            return new
            {
                Data = (dynamic)searchresponse.Contacts,
                Total = (int)searchresponse.TotalHits
            };
        }

        private Object getContactsGridContacts<T>(AdvancedViewViewModel advancedSearchModel) where T : IShallowContact
        {
            SearchContactsResponse<T> contactsSearchResponse = new SearchContactsResponse<T>();
            SearchContactsResponse<T> searchresponse = new SearchContactsResponse<T>();
            int[] ContactIDsList = null;
            var key = "SavedSearchContactIds" + advancedSearchModel.Guid;
            var contactIDs = cachingService.GetSavedSearchContactIds(key);
            if (contactIDs != null)
                ContactIDsList = contactIDs.ToArray();
            else
                ContactIDsList = new List<int>().ToArray();

            if (advancedSearchModel.ShowingType == ContactShowingFieldType.MyContacts)
            {
                GetRecentViwedContactsResponse response = GetUserCreatedContacts(null);
                if (!response.ContactIdList.IsAny())
                    ContactIDsList = new List<int>().ToArray();
                else
                    ContactIDsList = response.ContactIdList.ToArray();
            }
            else if (advancedSearchModel.ShowingType == ContactShowingFieldType.RecentlyViewed)
            {
                GetRecentViwedContactsResponse response = GetRecentViewedContacts(null, ((int)advancedSearchModel.SortBy).ToString());
                if (response.ContactIdList == null || response.ContactIdList.Count() < 0)
                    ContactIDsList = new List<int>().ToArray();
                else
                {
                    ContactIDsList = response.ContactIdList.ToArray();
                    Logger.Current.Informational(" recentily viewed Contacts count :" + ContactIDsList.Count());
                }
            }

            if (advancedSearchModel.ShowingType == ContactShowingFieldType.MyContacts || advancedSearchModel.ShowingType == ContactShowingFieldType.PeopleAndComapnies ||
                    advancedSearchModel.ShowingType == ContactShowingFieldType.RecentlyViewed)
            {
                var p = advancedSearchModel.PageNumber;
                var contacts = ContactIDsList;
                if (advancedSearchModel.ShowingType == ContactShowingFieldType.MyContacts)
                {
                    p = 1;
                    if (string.IsNullOrEmpty(advancedSearchModel.SearchText))
                        contacts = ContactIDsList.Skip((advancedSearchModel.PageNumber - 1) * advancedSearchModel.ItemsPerPage).Take(advancedSearchModel.ItemsPerPage).ToArray();
                    else
                        contacts = ContactIDsList.ToArray();
                    Logger.Current.Informational("Giving contactIds to elastic search: " + contacts.Count());
                }
                searchresponse = contactService.GetAllContacts<T>(new SearchContactsRequest()
                {
                    Query = advancedSearchModel.SearchText,
                    Limit = advancedSearchModel.ItemsPerPage,
                    PageNumber = p,
                    SortFieldType = (ContactSortFieldType)Convert.ToInt16(advancedSearchModel.SortBy),
                    AccountId = this.Identity.ToAccountID(),
                    ContactIDs = contacts,
                    ShowingFieldType = advancedSearchModel.ShowingType,
                    RequestedBy = this.Identity.ToUserID(),
                    SortField = advancedSearchModel.SortField == "" ? null : advancedSearchModel.SortField,
                    IsResultsGrid = true,
                    RoleId = this.Identity.ToRoleID()
                });
            }
            else if (advancedSearchModel.ShowingType == ContactShowingFieldType.People)
                searchresponse = contactService.GetPersons<T>(new SearchContactsRequest()
                {
                    Query = advancedSearchModel.SearchText,
                    Limit = advancedSearchModel.ItemsPerPage,
                    PageNumber = advancedSearchModel.PageNumber,
                    SortFieldType = (ContactSortFieldType)Convert.ToInt16(advancedSearchModel.SortBy),
                    AccountId = this.Identity.ToAccountID(),
                    RequestedBy = this.Identity.ToUserID(),
                    RoleId = this.Identity.ToRoleID(),
                    ContactIDs = ContactIDsList,
                    SortField = advancedSearchModel.SortField == "" ? null : advancedSearchModel.SortField,
                    SortDirection = advancedSearchModel.SortDirection,
                    IsResultsGrid = true
                });
            else if (advancedSearchModel.ShowingType == ContactShowingFieldType.Companies)
                searchresponse = contactService.GetCompanies<T>(new SearchContactsRequest()
                {
                    Query = advancedSearchModel.SearchText,
                    Limit = advancedSearchModel.ItemsPerPage,
                    PageNumber = advancedSearchModel.PageNumber,
                    SortFieldType = (ContactSortFieldType)Convert.ToInt16(advancedSearchModel.SortBy),
                    AccountId = this.Identity.ToAccountID(),
                    RequestedBy = this.Identity.ToUserID(),
                    RoleId = this.Identity.ToRoleID(),
                    ContactIDs = ContactIDsList,
                    SortField = advancedSearchModel.SortField == "" ? null : advancedSearchModel.SortField,
                    SortDirection = advancedSearchModel.SortDirection,
                });

            contactsSearchResponse.TotalHits = searchresponse.TotalHits;
            contactsSearchResponse.Contacts = (IEnumerable<T>)searchresponse.Contacts;

            Session["TotalHits"] = contactsSearchResponse.TotalHits.ToString();
            Session["ContactListIDs"] = ContactIDsList;
            return new
            {
                Data = (dynamic)contactsSearchResponse.Contacts,
                Total = (int)contactsSearchResponse.TotalHits
            };
        }

        private object getAdvancedViewContacts<T>(AdvancedViewViewModel advancedSearchModel) where T : IShallowContact
        {
            int[] ContactIDsList = null;
            if (advancedSearchModel.EntityType == (short)ShowingType.TagsReport)
                ContactIDsList = GetTagRelatedContacts(advancedSearchModel.TagID, null).ContactIdList.ToArray();
            else if (advancedSearchModel.EntityType == (short)ShowingType.Opportunity)
                ContactIDsList = GetOpportunityContacts(advancedSearchModel.OpportunityID).ContactIdList.ToArray();
            else if (advancedSearchModel.EntityType == (short)ShowingType.Action)
                ContactIDsList = GetActionRelatedContacts(advancedSearchModel.ActionID).ContactIdList.ToArray();
            else
            {
                var key = "SavedSearchContactIds" + advancedSearchModel.Guid;
                ContactIDsList = cachingService.GetSavedSearchContactIds(key).ToArray();
            }

            SearchContactsResponse<T> searchresponse = new SearchContactsResponse<T>();
            if (ContactIDsList.IsAny())
            {
                if (advancedSearchModel.ShowingType == ContactShowingFieldType.MyContacts || advancedSearchModel.ShowingType == ContactShowingFieldType.PeopleAndComapnies ||
                    advancedSearchModel.ShowingType == ContactShowingFieldType.RecentlyViewed)
                {
                    if (advancedSearchModel.ShowingType == ContactShowingFieldType.MyContacts)
                    {
                        GetRecentViwedContactsResponse usersResponse = GetUserCreatedContacts(ContactIDsList.ToArray());
                        if (!usersResponse.ContactIdList.IsAny())
                        {
                            ContactIDsList = new List<int>().ToArray();
                        }
                        else
                        {
                            ContactIDsList = usersResponse.ContactIdList.ToArray();
                        }
                    }
                    else if (advancedSearchModel.ShowingType == ContactShowingFieldType.RecentlyViewed)
                    {
                        GetRecentViwedContactsResponse recentResponse = GetRecentViewedContacts(ContactIDsList.ToArray(), null);
                        if (recentResponse.ContactIdList == null || recentResponse.ContactIdList.Count() < 0)
                            ContactIDsList = new List<int>().ToArray();
                        else
                            ContactIDsList = recentResponse.ContactIdList.ToArray();
                    }
                    if (ContactIDsList.Count() == 0)
                        searchresponse = new SearchContactsResponse<T>()
                        {
                            Contacts = new List<T>()
                        };
                    else
                        searchresponse = contactService.GetAllContacts<T>(new SearchContactsRequest()
                        {
                            Query = advancedSearchModel.SearchText,
                            Limit = advancedSearchModel.ItemsPerPage,
                            PageNumber = advancedSearchModel.PageNumber,
                            SortFieldType = advancedSearchModel.SortBy,
                            AccountId = this.Identity.ToAccountID(),
                            ContactIDs = ContactIDsList.ToArray(),
                            ShowingFieldType = advancedSearchModel.ShowingType,
                            SortField = advancedSearchModel.SortField == "" ? null : advancedSearchModel.SortField,
                            SortDirection = advancedSearchModel.SortDirection,
                            RequestedBy = this.Identity.ToUserID(),
                            RoleId = this.Identity.ToRoleID(),
                            IsResultsGrid = true
                        });
                }
                else if (advancedSearchModel.ShowingType == ContactShowingFieldType.People)
                    searchresponse = contactService.GetPersons<T>(new SearchContactsRequest()
                    {
                        Query = advancedSearchModel.SearchText,
                        Limit = advancedSearchModel.ItemsPerPage,
                        PageNumber = advancedSearchModel.PageNumber,
                        SortFieldType = advancedSearchModel.SortBy,
                        AccountId = this.Identity.ToAccountID(),
                        RequestedBy = this.Identity.ToUserID(),
                        RoleId = this.Identity.ToRoleID(),
                        ContactIDs = ContactIDsList.ToArray(),
                        SortField = advancedSearchModel.SortField == "" ? null : advancedSearchModel.SortField,
                        SortDirection = advancedSearchModel.SortDirection,
                        IsResultsGrid = true
                    });
                else if (advancedSearchModel.ShowingType == ContactShowingFieldType.Companies)
                    searchresponse = contactService.GetCompanies<T>(new SearchContactsRequest()
                    {
                        Query = advancedSearchModel.SearchText,
                        Limit = advancedSearchModel.ItemsPerPage,
                        PageNumber = advancedSearchModel.PageNumber,
                        SortFieldType = advancedSearchModel.SortBy,
                        AccountId = this.Identity.ToAccountID(),
                        RequestedBy = this.Identity.ToUserID(),
                        RoleId = this.Identity.ToRoleID(),
                        ContactIDs = ContactIDsList.ToArray(),
                        SortField = advancedSearchModel.SortField == "" ? null : advancedSearchModel.SortField,
                        IsResultsGrid = true,
                        SortDirection = advancedSearchModel.SortDirection
                    });
            }
            else
            {
                searchresponse = new SearchContactsResponse<T>()
                {
                    Contacts = new List<T>()
                };
            }
            searchresponse.TotalHits = searchresponse.TotalHits;
            searchresponse.Contacts = (IEnumerable<T>)searchresponse.Contacts;

            Session["TotalHits"] = searchresponse.TotalHits.ToString();
            Session["ContactListIDs"] = ContactIDsList;
            return new
            {
                Data = (dynamic)searchresponse.Contacts,
                Total = (int)searchresponse.TotalHits
            };
        }

        private async Task<dynamic> getNewContactsReportContacts<T>(AdvancedViewViewModel advancedSearchModel) where T : IShallowContact
        {
            int[] ContactIDsList = null;
            SearchContactsResponse<T> searchresponse = new SearchContactsResponse<T>();

            if (ContactIDsList != null && !ContactIDsList.IsAny())
            {
                searchresponse = new SearchContactsResponse<T>()
                {
                    Contacts = new List<T>()
                };
            }
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            IEnumerable<Type> contactType = advancedSearchModel.ShowingType == ContactShowingFieldType.People ? new List<Type>() {
                        typeof(Person)
                    } : advancedSearchModel.ShowingType == ContactShowingFieldType.Companies ? new List<Type>() {
                        typeof(Campaign)        //We shouldn't show company records for NewContactsSearch NEXG-2691
                    } : new List<Type>() {
                        typeof(Person)
                    };
            var session = Session;
            var viewModel = (AdvancedSearchViewModel)Session["NewContactsSearchViewModel"];
            AdvancedSearchResponse<ContactListEntry> asresponse = await advancedSearchService.RunSearchAsync(new AdvancedSearchRequest<ContactListEntry>()
            {
                SearchViewModel = viewModel,
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID(),
                RequestedBy = this.Identity.ToUserID(),
                IsAdvancedSearch = true,
                Limit = advancedSearchModel.ItemsPerPage,
                PageNumber = advancedSearchModel.PageNumber,
                ViewContacts = true,
                Query = advancedSearchModel.SearchText,
                SortFieldType = advancedSearchModel.SortBy,
                ShowingFieldType = advancedSearchModel.ShowingType,
                SortField = advancedSearchModel.SortField == "" ? null : advancedSearchModel.SortField,
                SortDirection = advancedSearchModel.SortDirection,
                IsResultsGrid = true,
                ContactTypes = contactType,
                ShowByCreated = advancedSearchModel.ShowingType == ContactShowingFieldType.MyContacts ? true : false
            });
            searchresponse.TotalHits = asresponse.SearchResult.TotalHits;
            searchresponse.Contacts = (IEnumerable<T>)asresponse.SearchResult.Results;
            Session["TotalHits"] = searchresponse.TotalHits.ToString();
            Session["ContactListIDs"] = ContactIDsList;
            return new
            {
                Data = (dynamic)searchresponse.Contacts,
                Total = (int)searchresponse.TotalHits
            };
        }

        /// <summary>
        /// Previouses the contact.
        /// </summary>
        /// <returns></returns>
        [SmarttouchSessionStateBehaviour(SessionStateBehavior.Required)]
        public async Task<ActionResult> PreviousContact()
        {
            var index = Session["contactindex"].ToString();
            var searchText = ReadCookie("searchtext");
            var sortType = ReadCookie("sorttype");
            var filterType = ReadCookie("filtertype");
            int? JobID = TryParseNullable(ReadCookie("ImportJobID"));
            string jobstatus = ReadCookie("ImportJobRecordStatus");
            int? TagID = TryParseNullable(ReadCookie("TagID"));
            int? ActionID = TryParseNullable(ReadCookie("ActionID"));
            int? FormID = TryParseNullable(ReadCookie("ViewSubmissionFormID"));
            string IntegratedPersons = ReadCookie("IntegratedPersonIds");
            int? CampaignID = TryParseNullable(ReadCookie("CampaignID"));
            int? CampaignLinkID = TryParseNullable(ReadCookie("CampaignLinkID"));
            int? NeverBounceRequestID = TryParseNullable(ReadCookie("NeverBounceRequestID"));
            byte? NeverBounceEmailStatus = null;
            int? OpportunityID = TryParseNullable(ReadCookie("OpptyID"));
            string myCommType = ReadCookie("MyCommunicationType");
            string myCommPeriod = ReadCookie("MyCommunicationPeriod");
            string myCommActType = ReadCookie("MyCommunicationActivityType");

            DateTime? wfCampFromDate = new DateTime();
            DateTime? wfCampTodate = new DateTime();
            CampaignDrillDownActivity? campaigndrilldownactivity = default(CampaignDrillDownActivity?);
            CampaignDrillDownActivity tempcampaigndrilldownactivity = default(CampaignDrillDownActivity);
            if (CampaignDrillDownActivity.TryParse(ReadCookie("CampaignDrillDownActivity"), out tempcampaigndrilldownactivity))
                campaigndrilldownactivity = tempcampaigndrilldownactivity;
            else
                campaigndrilldownactivity = null;
            short? WorkflowID = default(short?);
            short tempWorkflowID;
            if (short.TryParse(ReadCookie("WorkflowID"), out tempWorkflowID))
                WorkflowID = tempWorkflowID;
            else
                WorkflowID = null;
            WorkflowContactsState? workflowcontactstate = default(WorkflowContactsState?);
            WorkflowContactsState tempworkflowcontactstate = default(WorkflowContactsState);
            if (WorkflowContactsState.TryParse(ReadCookie("WorkflowContactState"), out tempworkflowcontactstate))
                workflowcontactstate = tempworkflowcontactstate;
            else
                workflowcontactstate = null;
            int currentIndex = 0;
            int.TryParse(index, out currentIndex);
            currentIndex = currentIndex - 1;
            var result = await getContacts<ContactGridEntry>(currentIndex, 1, searchText, filterType, sortType, TagID, 1, 1, 1, JobID, FormID, jobstatus, false, true, IntegratedPersons, WorkflowID, workflowcontactstate,
                CampaignID, CampaignLinkID, campaigndrilldownactivity, ActionID, false, null, String.Empty, wfCampFromDate, wfCampTodate, NeverBounceRequestID, NeverBounceEmailStatus, OpportunityID,
                myCommType, myCommPeriod, myCommActType);
            IEnumerable<ContactGridEntry> contacts = (dynamic)result.Contacts;
            long totalHits = (int)result.TotalHits;
            ContactGridEntry entry = new ContactGridEntry();
            if (contacts.IsAny())
            {
                entry = contacts.First();
                Session["contactindex"] = currentIndex;
                Session["TotalHits"] = totalHits;
            }
            if (entry.ContactType == (int)ContactType.Person)
                return RedirectToAction("ContactDetails", new
                {
                    contactId = entry.ContactID,
                    index = currentIndex
                });
            else
                return RedirectToAction("CompanyDetails", new
                {
                    contactId = entry.ContactID,
                    index = currentIndex
                });
        }

        /// <summary>
        /// Nexts the contact.
        /// </summary>
        /// <returns></returns>
        [SmarttouchSessionStateBehaviour(SessionStateBehavior.Required)]
        public async Task<ActionResult> NextContact()
        {
            var index = Session["contactindex"].ToString();
            var searchText = ReadCookie("searchtext");
            var sortType = ReadCookie("sorttype");
            var filterType = ReadCookie("filtertype");
            int? JobID = TryParseNullable(ReadCookie("ImportJobID"));
            string jobstatus = ReadCookie("ImportJobRecordStatus");
            int? TagID = TryParseNullable(ReadCookie("TagID"));
            int? ActionID = TryParseNullable(ReadCookie("ActionID"));
            int? FormID = TryParseNullable(ReadCookie("ViewSubmissionFormID"));
            string IntegratedPersons = ReadCookie("IntegratedPersonIds");
            int? CampaignID = TryParseNullable(ReadCookie("CampaignID"));
            int? CampaignLinkID = TryParseNullable(ReadCookie("CampaignLinkID"));
            int? NeverBounceRequestID = TryParseNullable(ReadCookie("NeverBounceRequestID"));
            byte? NeverBounceEmailStatus = null;
            int? OpportunityID = TryParseNullable(ReadCookie("OpptyID"));
            string myCommType = ReadCookie("MyCommunicationType");
            string myCommPeriod = ReadCookie("MyCommunicationPeriod");
            string myCommActType = ReadCookie("MyCommunicationActivityType");

            DateTime? wfCampFromDate = new DateTime();
            DateTime? wfCampTodate = new DateTime();
            CampaignDrillDownActivity? campaigndrilldownactivity = default(CampaignDrillDownActivity?);
            CampaignDrillDownActivity tempcampaigndrilldownactivity = default(CampaignDrillDownActivity);
            if (CampaignDrillDownActivity.TryParse(ReadCookie("CampaignDrillDownActivity"), out tempcampaigndrilldownactivity))
                campaigndrilldownactivity = tempcampaigndrilldownactivity;
            else
                campaigndrilldownactivity = null;
            short? WorkflowID = default(short?);
            short tempWorkflowID;
            if (short.TryParse(ReadCookie("WorkflowID"), out tempWorkflowID))
                WorkflowID = tempWorkflowID;
            else
                WorkflowID = null;
            WorkflowContactsState? workflowcontactstate = default(WorkflowContactsState?);
            WorkflowContactsState tempworkflowcontactstate = default(WorkflowContactsState);
            if (WorkflowContactsState.TryParse(ReadCookie("WorkflowContactState"), out tempworkflowcontactstate))
                workflowcontactstate = tempworkflowcontactstate;
            else
                workflowcontactstate = null;
            int currentIndex = 0;
            int.TryParse(index, out currentIndex);
            currentIndex = currentIndex + 1;
            var result = await getContacts<ContactGridEntry>(currentIndex, 1, searchText, filterType, sortType, TagID, 1, 1, 1, JobID, FormID, jobstatus, false, true, IntegratedPersons, WorkflowID, workflowcontactstate,
                CampaignID, CampaignLinkID, campaigndrilldownactivity, ActionID, false, null, String.Empty, wfCampFromDate, wfCampTodate, NeverBounceRequestID, NeverBounceEmailStatus, OpportunityID
                , myCommType, myCommPeriod, myCommActType);
            IEnumerable<ContactGridEntry> contacts = (dynamic)result.Contacts;
            long totalHits = (int)result.TotalHits;
            ContactGridEntry entry = new ContactGridEntry();
            if (contacts.IsAny())
            {
                entry = contacts.First();
                Session["contactindex"] = currentIndex;
                Session["TotalHits"] = totalHits;
            }
            if (entry.ContactType == (int)ContactType.Person)
                return RedirectToAction("ContactDetails", new
                {
                    contactId = entry.ContactID,
                    index = currentIndex
                });
            else
                return RedirectToAction("CompanyDetails", new
                {
                    contactId = entry.ContactID,
                    index = currentIndex
                });
        }

        /// <summary>
        /// Tries the parse nullable.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public static int? TryParseNullable(string val)
        {
            int outValue;
            return int.TryParse(val, out outValue) ? (int?)outValue : null;
        }

        /// <summary>
        /// Removes the cookie.
        /// </summary>
        /// <param name="cookieName">Name of the cookie.</param>
        public void RemoveCookie(string cookieName)
        {
            if (Request.Cookies[cookieName] != null)
            {
                System.Web.HttpCookie myCookie = new System.Web.HttpCookie(cookieName);
                myCookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(myCookie);
            }
        }

        /// <summary>
        /// Adds the cookie.
        /// </summary>
        /// <param name="cookieName">Name of the cookie.</param>
        /// <param name="Value">The value.</param>
        /// <param name="days">The days.</param>
        public void AddCookie(string cookieName, string Value, int days)
        {
            System.Web.HttpCookie CartCookie = new System.Web.HttpCookie(cookieName, Value);
            CartCookie.Expires = DateTime.Now.AddDays(days);
            var cookie = Response.Cookies.Get(cookieName);
            if (cookie == null)
                Response.Cookies.Add(CartCookie);
            else
                Response.Cookies.Set(CartCookie);
        }

        /// <summary>
        /// Reads the cookie.
        /// </summary>
        /// <param name="strValue">The string value.</param>
        /// <returns></returns>
        public string ReadCookie(string strValue)
        {
            string strValues = string.Empty;
            if (Request.Cookies[strValue] != null)
            {
                strValues = Request.Cookies[strValue].Value;
            }
            return strValues;
        }

        /// <summary>
        /// Gets the imported contacts.
        /// </summary>
        /// <param name="JobID">The job identifier.</param>
        /// <param name="recordStatus">The record status.</param>
        /// <returns></returns>
        public GetImportedContactResponse GetImportedContacts(int JobID, string recordStatus)
        {
            GetImportedContactRequest request = new GetImportedContactRequest();
            request.recordStatus = recordStatus;
            request.LeadAdapterJobID = JobID;
            GetImportedContactResponse response = contactService.GetImportedContacts(request);
            return response;
        }

        /// <summary>
        /// Gets the worklfow related contacts.
        /// </summary>
        /// <param name="WorkflowID">The workflow identifier.</param>
        /// <param name="WorkflowContactsState">State of the workflow contacts.</param>
        /// <returns></returns>
        public GetWorkflowContactsResponse GetWorklfowRelatedContacts(short WorkflowID, WorkflowContactsState WorkflowContactsState)
        {
            GetWorkflowContactsRequest request = new GetWorkflowContactsRequest();
            request.WorkflowID = WorkflowID;
            request.WorkflowContactState = WorkflowContactsState;
            GetWorkflowContactsResponse response = contactService.GetWorkflowContacts(request);
            return response;
        }

        /// <summary>
        /// Gets the workflow campaign related contacts.
        /// </summary>
        /// <param name="WorkflowID">The workflow identifier.</param>
        /// <param name="CampaignID">The campaign identifier.</param>
        /// <param name="campaignDrilldown">The campaign drilldown.</param>
        /// <returns></returns>
        public GetWorkflowContactsResponse GetWorkflowCampaignRelatedContacts(short WorkflowID, int CampaignID, CampaignDrillDownActivity campaignDrilldown, DateTime? wfCampFromDate, DateTime? wfCampToDate)
        {
            GetWorkflowContactsRequest request = new GetWorkflowContactsRequest();
            request.WorkflowID = WorkflowID;
            request.CampaignID = CampaignID;
            request.CampaignDrillDownsActivity = campaignDrilldown;
            request.FromDate = wfCampFromDate;
            request.ToDate = wfCampToDate;
            GetWorkflowContactsResponse response = contactService.GetWorkflowRelatedContacts(request);
            return response;
        }

        /// <summary>
        /// Gets the campaign related contacts.
        /// </summary>
        /// <param name="CampaignID">The campaign identifier.</param>
        /// <param name="CampaignDrillDownActivity">The campaign drill down activity.</param>
        /// <param name="CampaignLinkID">The campaign link identifier.</param>
        /// <returns></returns>
        public GetCampaignContactsResponse GetCampaignRelatedContacts(int CampaignID, CampaignDrillDownActivity CampaignDrillDownActivity, int? CampaignLinkID)
        {
            GetCampaignContactsRequest request = new GetCampaignContactsRequest();
            request.CampaignID = CampaignID;
            request.CampaignDrillDownActivity = CampaignDrillDownActivity;
            request.CampaignLinkID = CampaignLinkID;
            request.AccountId = this.Identity.ToAccountID();
            GetCampaignContactsResponse response = contactService.GetCampaignContacts(request);
            return response;
        }

        /// <summary>
        /// Gets the tag related contacts.
        /// </summary>
        /// <param name="TagID">The tag identifier.</param>
        /// <param name="TagType">Type of the tag.</param>
        /// <returns></returns>
        public GetTagContactsResponse GetTagRelatedContacts(int TagID, int? TagType)
        {
            GetTagContactsRequest request = new GetTagContactsRequest();
            request.TagID = TagID;
            request.TagType = TagType == null ? 0 : (int)TagType;
            request.AccountId = this.Identity.ToAccountID();
            GetTagContactsResponse response = contactService.GetTagRelatedContacts(request);
            return response;
        }

        /// <summary>
        /// Gets the action related contacts.
        /// </summary>
        /// <param name="ActionID">The action identifier.</param>
        /// <returns></returns>
        public GetActionRelatedContactsResponce GetActionRelatedContacts(int ActionID)
        {
            GetActionRelatedContactsRequest request = new GetActionRelatedContactsRequest();
            request.ActionID = ActionID;
            GetActionRelatedContactsResponce response = contactService.GetActionRelatedContacts(request);
            return response;
        }

        /// <summary>
        /// Gets Opportunity Contacts.
        /// </summary>
        /// <param name="opportunityId"></param>
        /// <returns></returns>
        public GetOpportunityContactsResponse GetOpportunityContacts(int opportunityId)
        {
            GetOpportunityContactsResponse response = opportunityService.GetOpportunityContacts(new GetOpportunityContactsRequest() { OpportunityId = opportunityId });
            return response;
        }

        //public GetMyCommunicationContactsResponse GetMyCommunicationContacts(string activity, string activityType, string period)
        //{
        //    DateTime startDate = DateTime.UtcNow;
        //    DateTime endDate = DateTime.UtcNow;
        //    if (period == "0")
        //        startDate = ToUserUtcDateTime(startDate.AddDays(-7).Date);
        //    else
        //        startDate = ToUserUtcDateTime(startDate.AddDays(-30).Date);
        //    return userService.GetMyCommunicationContacts(new GetMyCommunicationContactsRequest()
        //    {
        //        UserId = this.Identity.ToUserID(),
        //        AccountId = this.Identity.ToAccountID(),
        //        StartDate = startDate,
        //        EndDate = endDate,
        //        Activity = activity,
        //        ActivityType = activityType
        //    });

        //}

        /// <summary>
        /// Gets the search definition contacts.
        /// </summary>
        /// <param name="SSDefinitionId">The ss definition identifier.</param>
        /// <param name="SSType">Type of the ss.</param>
        /// <returns></returns>
        public async Task<GetSearchDefinitionContactsResponce> GetSearchDefinitionContacts(int SSDefinitionId, byte SSType)
        {
            GetSearchDefinitionContactsRequest request = new GetSearchDefinitionContactsRequest();
            request.SearchDefinitionId = SSDefinitionId;
            request.ContactsType = SSType;
            Logger.Current.Informational("For Getting SavedSearch Contacts By SearchdefinitionID In Contacts Drilldown");
            var contactids = await advancedSearchService.GetSavedSearchContactIds(new GetSavedSearchContactIdsRequest()
            {
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID(),
                SearchDefinitionId = SSDefinitionId
            });
            request.ContactIds = contactids;

            request.AccountId = this.Identity.ToAccountID();
            GetSearchDefinitionContactsResponce response = new GetSearchDefinitionContactsResponce();
            if (request.ContactsType == (byte)RecipientType.Active)
                response = contactService.GetSearchDefinitionContacts(request);
            else
                response.ContactIdList = contactids;
            return response;
        }

        /// <summary>
        /// Gets the form related contacts.
        /// </summary>
        /// <param name="FormID">The form identifier.</param>
        /// <returns></returns>
        public GetFormContactsResponse GetFormRelatedContacts(int FormID)
        {
            GetFormContactsRequest request = new GetFormContactsRequest();
            request.FormID = FormID;
            GetFormContactsResponse response = formService.GetFormViewSubmissions(request);
            return response;
        }

        /// <summary>
        /// Gets the recent viewed contacts.
        /// </summary>
        /// <param name="contactIDsList">The contact i ds list.</param>
        /// <param name="sortValue">The sort value.</param>
        /// <returns></returns>
        public GetRecentViwedContactsResponse GetRecentViewedContacts(int[] contactIDsList, string sortValue)
        {
            int userId = this.Identity.ToUserID();
            int accountId = this.Identity.ToAccountID();
            GetRecentViwedContactsRequest request = new GetRecentViwedContactsRequest();
            request.ActivityName = UserActivityType.Read;
            request.ModuleName = AppModules.Contacts;
            request.UserId = userId;
            request.ContactIDs = contactIDsList;
            request.sort = sortValue;
            request.AccountId = accountId;
            GetRecentViwedContactsResponse response = userService.GetRecentlyViewedContacts(request);
            return response;
        }

        /// <summary>
        /// Gets the user created contacts.
        /// </summary>
        /// <param name="contactIDsList">The contact i ds list.</param>
        /// <returns></returns>
        public GetRecentViwedContactsResponse GetUserCreatedContacts(int[] contactIDsList)
        {
            int userId = this.Identity.ToUserID();
            GetRecentViwedContactsRequest request = new GetRecentViwedContactsRequest();
            request.ActivityName = UserActivityType.Create;
            request.ModuleName = AppModules.Contacts;
            request.UserId = userId;
            request.ContactIDs = contactIDsList;
            request.AccountId = this.Identity.ToAccountID();
            GetRecentViwedContactsResponse response = contactService.GetContactByUserId(request);
            return response;
        }

        /// <summary>
        /// Deletes the contacts.
        /// </summary>
        /// <param name="contactIds">The contact ids.</param>
        /// <returns></returns>
        public JsonResult DeleteContacts(string contactIds)
        {
            var viewModel1 = (AdvancedSearchViewModel)Session["AdvancedSearchVM"];
            var strngdata = Newtonsoft.Json.JsonConvert.SerializeObject(viewModel1);
            DeactivateContactsResponse response = new DeactivateContactsResponse();
            var drillDownContactIds = (int[])Session["ContactListIDs"];
            if (!String.IsNullOrEmpty(Server.UrlDecode(ReadCookie("selectallsearchstring"))))
            {
                BulkOperations operationData = new BulkOperations()
                {
                    OperationID = 0,
                    OperationType = (int)BulkOperationTypes.Delete,
                    SearchCriteria = HttpUtility.HtmlEncode(Server.UrlDecode(ReadCookie("selectallsearchstring"))),
                    AdvancedSearchCriteria = strngdata,
                    SearchDefinitionID = null,
                    AccountID = Thread.CurrentPrincipal.Identity.ToAccountID(),
                    UserID = (int)Thread.CurrentPrincipal.Identity.ToUserID(),
                    RoleID = Thread.CurrentPrincipal.Identity.ToRoleID()
                };
                InsertBulkOperationRequest bulkOperationRequest = new InsertBulkOperationRequest()
                {
                    OperationData = operationData,
                    AccountId = Thread.CurrentPrincipal.Identity.ToAccountID(),
                    RequestedBy = Thread.CurrentPrincipal.Identity.ToUserID(),
                    CreatedOn = DateTime.Now.ToUniversalTime().AddMinutes(1),
                    RoleId = Thread.CurrentPrincipal.Identity.ToRoleID(),
                    DrillDownContactIds = drillDownContactIds
                };
                accountService.InsertBulkOperation(bulkOperationRequest);
            }
            else
            {
                DeactivateContactsRequest contactsRequest = new DeactivateContactsRequest();
                if (contactIds.Split(',').Length > 0)
                {
                    contactsRequest.ContactIds = contactIds.Split(',').Select(int.Parse).ToArray();
                    contactsRequest.AccountId = this.Identity.ToAccountID();
                    contactsRequest.RequestedBy = this.Identity.ToUserID();
                    response = contactService.DeactivateContacts(contactsRequest);
                }
            }
            return Json(new
            {
                success = true,
                response = response.ContactsDeleted
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region Person
        /// <summary>
        /// Adds the person.
        /// </summary>
        /// <returns></returns>
        [Route("addperson/")]
        [SmarttouchAuthorize(AppModules.Contacts, AppOperations.Create)]
        [MenuType(MenuCategory.ContactAction, MenuCategory.LeftMenuCRM)]
        public ActionResult AddPerson()
        {
            ViewBag.IsAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
            ViewBag.IsSTAdmin = this.Identity.IsSTAdmin();
            PersonViewModel viewModel = new PersonViewModel();
            viewModel.AccountID = this.Identity.ToAccountID();
            viewModel.Addresses = new List<AddressViewModel>();
            viewModel.Image = new ImageViewModel();
            IList<Phone> phones = new List<Phone>();
            IList<Email> emails = new List<Email>();
            IList<Url> socialMediaUrls = new List<Url>();
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            GetUsersResponse response = contactService.GetUsers(new GetUsersRequest()
            {
                AccountID = this.Identity.ToAccountID(),
                UserId = this.Identity.ToUserID(),
                IsSTadmin = this.Identity.IsSTAdmin()
            });
            viewModel.OwnerId = response.Owner.ToList()[0].OwnerId;
            viewModel.OwnerName = response.Owner.ToList()[0].OwnerName;
            viewModel.LeadSources = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LeadSources).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            viewModel.AddressTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.AddressType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            var defaultAddressType = viewModel.AddressTypes.Where(s => s.IsDefault).FirstOrDefault();
            viewModel.Addresses.Add(new AddressViewModel()
            {
                AddressID = 0,
                AddressTypeID = defaultAddressType != null ? defaultAddressType.DropdownValueID : (short)0,
                Country = new Country
                {
                    Code = ""
                },
                State = new State
                {
                    Code = ""
                },
                IsDefault = true
            });
            viewModel.PhoneTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.PhoneNumberType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            var defaultPhoneType = viewModel.PhoneTypes.Where(s => s.IsDefault).FirstOrDefault();
            phones.Add(new Phone()
            {
                ContactPhoneNumberID = 0,
                PhoneType = defaultPhoneType != null ? defaultPhoneType.DropdownValueID : (short)0,
                PhoneTypeName = defaultPhoneType != null ? defaultPhoneType.DropdownValue : "",
                Number = "",
                IsPrimary = true
            });
            viewModel.LifecycleStages = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LifeCycle).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            var defaultLifeCycleType = viewModel.LifecycleStages.Where(s => s.IsDefault).FirstOrDefault();
            viewModel.LifecycleStage = defaultLifeCycleType != null ? (short)defaultLifeCycleType.DropdownValueID : (short)0;
            viewModel.PartnerTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.PartnerType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            var defaultPartnerType = viewModel.PartnerTypes.Where(s => s.IsDefault).FirstOrDefault();
            viewModel.PartnerType = defaultPartnerType != null ? defaultPartnerType.DropdownValueID : (short)0;
            emails.Add(new Email()
            {
                EmailId = "",
                EmailStatusValue = EmailStatus.NotVerified,
                IsPrimary = true,
                EmailID = 0
            });
            GetAllCustomFieldTabsRequest customFieldTabs = new GetAllCustomFieldTabsRequest(viewModel.AccountID);
            viewModel.CustomFieldTabs = customFieldService.GetAllCustomFieldTabs(customFieldTabs).CustomFieldsViewModel.CustomFieldTabs;
            socialMediaUrls.Add(new Url
            {
                MediaType = "Website",
                URL = ""
            });
            viewModel.Phones = phones;
            viewModel.Emails = emails;
            viewModel.SocialMediaUrls = socialMediaUrls;
            ViewBag.NewAddress = new AddressViewModel()
            {
                AddressID = 0,
                AddressTypeID = defaultAddressType != null ? defaultAddressType.DropdownValueID : (short)0,
                Country = new Country
                {
                    Code = ""
                },
                State = new State
                {
                    Code = ""
                },
                IsDefault = true
            };
            ViewBag.mode = "AddPerson";
            return View("AddEditPerson", viewModel);
        }

        /// <summary>
        /// Edits the contact.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="contactType">Type of the contact.</param>
        /// <returns></returns>
        [Route("editperson/")]
        [SmarttouchAuthorize(AppModules.Contacts, AppOperations.Edit)]
        [MenuType(MenuCategory.ContactAction, MenuCategory.LeftMenuCRM)]
        public ActionResult EditContact(int contactId, int contactType)
        {
            ViewBag.IsAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
            ViewBag.IsSTAdmin = this.Identity.IsSTAdmin();
            GetPersonResponse response = contactService.GetPerson(new GetPersonRequest(contactId)
            {
                RequestedBy = this.Identity.ToUserID(),
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID(),
                IncludeCustomFieldTabs = true
            });
            if (response.PersonViewModel != null)
            {
                GetUsersResponse userresponse = contactService.GetUsers(new GetUsersRequest()
                {
                    AccountID = this.Identity.ToAccountID(),
                    UserId = response.PersonViewModel.OwnerId == 0 || response.PersonViewModel.OwnerId == null ? this.Identity.ToUserID() : (int)response.PersonViewModel.OwnerId,
                    IsSTadmin = this.Identity.IsSTAdmin()
                });
                response.PersonViewModel.OwnerId = userresponse.Owner.Count() != 0 ? userresponse.Owner.ToList()[0].OwnerId : 0;
                response.PersonViewModel.OwnerName = userresponse.Owner.Count() != 0 ? userresponse.Owner.ToList()[0].OwnerName : "";
            }
            string DateFormat = this.Identity.ToDateFormat();
            response.PersonViewModel.DateFormat = DateFormat;
            AddCookie("dateformat", this.Identity.ToDateFormat(), 1);
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID()).ToList();
            response.PersonViewModel.AddressTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.AddressType).Select(s => s.DropdownValuesList).FirstOrDefault().Where(d => d.IsActive == true);
            response.PersonViewModel.LifecycleStages = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LifeCycle).Select(s => s.DropdownValuesList).FirstOrDefault().Where(d => d.IsActive == true);
            response.PersonViewModel.LeadSources = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LeadSources).Select(s => s.DropdownValuesList).FirstOrDefault().Where(d => d.IsActive == true);
            var defaultLifeCycleType = response.PersonViewModel.LifecycleStages.Where(p => p.IsDefault).FirstOrDefault();
            if (response.PersonViewModel.LifecycleStage == 0)
                response.PersonViewModel.LifecycleStage = defaultLifeCycleType != null ? defaultLifeCycleType.DropdownValueID : (short)0;
            response.PersonViewModel.PartnerTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.PartnerType).Select(s => s.DropdownValuesList).FirstOrDefault().Where(d => d.IsActive == true);
            if (response.PersonViewModel.Addresses.Count == 0)
            {
                response.PersonViewModel.Addresses = new List<AddressViewModel>() {
                    new AddressViewModel () {
                        AddressID = 0,
                        AddressTypeID = response.PersonViewModel.AddressTypes.Where (p => p.IsDefault).FirstOrDefault () != null ? response.PersonViewModel.AddressTypes.Where (p => p.IsDefault).FirstOrDefault ().DropdownValueID : (short)0,
                        Country = new Country {
                            Code = ""
                        },
                        State = new State {
                            Code = ""
                        },
                        IsDefault = true
                    }
                };
            }
            if (response.PersonViewModel.SocialMediaUrls.Count == 0)
            {
                IList<Url> socialMediaUrls = new List<Url>();
                socialMediaUrls.Add(new Url
                {
                    MediaType = "Website",
                    URL = ""
                });
                response.PersonViewModel.SocialMediaUrls = socialMediaUrls;
            }
            response.PersonViewModel.PhoneTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.PhoneNumberType).Select(s => s.DropdownValuesList).FirstOrDefault().Where(d => d.IsActive == true);
            var defaultPhoneType = response.PersonViewModel.PhoneTypes.Where(p => p.IsDefault).FirstOrDefault();
            if (response.PersonViewModel.Phones.Count == 0)
            {
                response.PersonViewModel.Phones.Add(new Phone()
                {
                    ContactPhoneNumberID = 0,
                    PhoneType = defaultPhoneType != null ? defaultPhoneType.DropdownValueID : (short)0,
                    PhoneTypeName = defaultPhoneType != null ? defaultPhoneType.DropdownValue : "",
                    Number = "",
                    IsPrimary = true
                });
            }
            if (response.PersonViewModel.Emails.Count == 0)
            {
                IList<Email> emails = new List<Email>();
                emails.Add(new Email()
                {
                    EmailId = "",
                    EmailStatusValue = EmailStatus.NotVerified,
                    IsPrimary = true,
                    EmailID = 0,
                    ContactID = contactId
                });
                response.PersonViewModel.Emails = emails;
            }
            ViewBag.mode = "EditPerson";
            ViewBag.NewAddress = new AddressViewModel()
            {
                AddressID = 0,
                AddressTypeID = response.PersonViewModel.AddressTypes.Where(p => p.IsDefault).FirstOrDefault() != null ? response.PersonViewModel.AddressTypes.Where(p => p.IsDefault).FirstOrDefault().DropdownValueID : (short)0,
                Country = new Country
                {
                    Code = ""
                },
                State = new State
                {
                    Code = ""
                },
                IsDefault = false
            };
            return View("AddEditPerson", response.PersonViewModel);
        }

        /// <summary>
        /// Inserts the person.
        /// </summary>
        /// <param name="personViewModel">The person view model.</param>
        /// <returns></returns>
        public ActionResult InsertPerson(string personViewModel)
        {
            PersonViewModel viewModel = JsonConvert.DeserializeObject<PersonViewModel>(personViewModel);
            int accountId = this.Identity.ToAccountID();
            int userId = this.Identity.ToUserID();
            string dateFormat = this.Identity.ToDateFormat();
            DateTime utcTime = DateTime.Now.ToUniversalTime();
            bool isStAdmin = this.Identity.IsSTAdmin();
            viewModel.AccountID = accountId;
            viewModel.LastUpdatedBy = userId;
            viewModel.FirstContactSource = ContactSource.Manual;
            viewModel.FirstSourceType = 0;
            viewModel.ReferenceId = Guid.NewGuid();
            viewModel.LastUpdatedOn = utcTime;
            viewModel.ContactSource = null;
            viewModel.SourceType = null;
            viewModel.CreatedBy = userId;
            viewModel.CreatedOn = utcTime;
            viewModel.DateFormat = dateFormat;
            var usersPermissions = cachingService.GetUserPermissions(Thread.CurrentPrincipal.Identity.ToAccountID());
            var accountPermissions = cachingService.GetAccountPermissions(Thread.CurrentPrincipal.Identity.ToAccountID());
            var userModules = usersPermissions.Where(s => s.RoleId == (short)Thread.CurrentPrincipal.Identity.ToRoleID() && accountPermissions.Contains(s.ModuleId)).Select(r => r.ModuleId).ToList();
            if (userModules.Contains((byte)AppModules.FullContact))
            {
                viewModel = ManagePersonSocialProfiles(viewModel).Values.FirstOrDefault();
            }
            InsertPersonRequest request = new InsertPersonRequest()
            {
                PersonViewModel = viewModel,
                RequestedBy = userId,
                AccountId = accountId,
                RoleId = this.Identity.ToRoleID(),
                ModuleId = (byte)AppModules.Contacts,
                isStAdmin = isStAdmin
            };
            InsertPersonResponse response = contactService.InsertPerson(request);
            var getContactDetails = contactService.GetPerson(new GetPersonRequest(response.PersonViewModel.ContactID)
            {
                AccountId = accountId
            });
            return Json(new
            {
                success = true,
                response = getContactDetails.PersonViewModel.ContactID
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Updates the person.
        /// </summary>
        /// <param name="personViewModel">The person view model.</param>
        /// <returns></returns>
        [MenuType(MenuCategory.ContactAction, MenuCategory.LeftMenuCRM)]
        public JsonResult UpdatePerson(string personViewModel)
        {
            PersonViewModel viewModel = JsonConvert.DeserializeObject<PersonViewModel>(personViewModel);
            viewModel.AccountID = this.Identity.ToAccountID();
            viewModel.LastUpdatedBy = this.Identity.ToUserID();
            viewModel.ContactSource = null;
            viewModel.SourceType = null;
            viewModel.LastUpdatedOn = DateTime.Now.ToUniversalTime();
            string dateFormat = this.Identity.ToDateFormat();
            viewModel.DateFormat = dateFormat;
            bool isStAdmin = this.Identity.IsSTAdmin();
            UpdatePersonRequest request = new UpdatePersonRequest()
            {
                PersonViewModel = viewModel,
                RequestedBy = this.Identity.ToUserID(),
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID(),
                ModuleId = (byte)AppModules.Contacts,
                isStAdmin = isStAdmin
            };
            UpdatePersonResponse response = contactService.UpdatePerson(request);
            if (response.Exception != null)
            {
                string responseJson = response.Exception.Message.Replace("\r\n", "</br>").Replace("\n", "</br>");
                return Json(new
                {
                    success = false,
                    response = responseJson
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (viewModel.OwnerId != viewModel.PreviousOwnerId)
                {
                    if (viewModel.OwnerId != null)
                        InsertChangeOwnerActivity(viewModel.ContactID, viewModel.OwnerId.Value);
                    else
                        InsertChangeOwnerActivity(viewModel.ContactID, null);
                }
            }

            return Json(new
            {
                success = true,
                response = viewModel.ContactID
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCustomFieldTabs()
        {
            var accountId = this.Identity.ToAccountID();
            GetAllCustomFieldTabsResponse response = new GetAllCustomFieldTabsResponse();
            response = contactService.GetCustomFieldTabs(new GetAllCustomFieldTabsRequest(accountId));
            return Json(new
            {
                success = true,
                response = response.CustomFieldsViewModel
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Contacts the details.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        [SmarttouchSessionStateBehaviour(SessionStateBehavior.Required)]
        [Route("person/{contactId}/{index?}")]
        [SmarttouchAuthorize(AppModules.Contacts, AppOperations.Read)]
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public ActionResult ContactDetails(int contactId, int index = 0)
        {
            try
            {
                var urlReferenceString = string.Empty;
                if ((HttpContext.Request).UrlReferrer != null)
                    urlReferenceString = (HttpContext.Request).UrlReferrer.OriginalString;
                if (urlReferenceString != null)
                {
                    var value = urlReferenceString.Contains("reportType=15");
                    if (value)
                        ViewBag.WebVisits = "webvisits";
                }
                ViewBag.IsSTAdmin = this.Identity.IsSTAdmin();
                RemoveCookie("contactid");
                RemoveCookie("period");
                RemoveCookie("module");
                RemoveCookie("contactindex");
                var log = ReadCookie("log");
                List<Phone> phones = new List<Phone>();
                List<Email> emails = new List<Email>();
                GetPersonResponse response = new GetPersonResponse();
                response.PersonViewModel = contactService.GetPerson(new GetPersonRequest(contactId)
                {
                    RequestedBy = this.Identity.ToUserID(),
                    AccountId = this.Identity.ToAccountID(),
                    RoleId = this.Identity.ToRoleID(),
                    IncludeLastTouched = true
                }).PersonViewModel;
                if (response.PersonViewModel == null)
                {
                    ErrorViewModel errorViewModel = new ErrorViewModel();
                    errorViewModel.Message = "The Contact you have requested was not found!";
                    return RedirectToAction("EntityNotFound", "Error", errorViewModel);
                }
                AddCookie("contactid", contactId.ToString(), 1);
                GetUsersResponse userresponse = contactService.GetUsers(new GetUsersRequest()
                {
                    AccountID = this.Identity.ToAccountID(),
                    UserId = response.PersonViewModel.OwnerId == 0 || response.PersonViewModel.OwnerId == null ? 0 : (int)response.PersonViewModel.OwnerId,
                    IsSTadmin = this.Identity.IsSTAdmin()
                });
                response.PersonViewModel.OwnerName = userresponse.Owner.Count() == 1 ? userresponse.Owner.ToList()[0].OwnerName : "";
                RemoveCookie("contactname");
                string firstName = "";
                string lastName = "";
                if (!string.IsNullOrEmpty(response.PersonViewModel.FirstName))
                {
                    if (response.PersonViewModel.FirstName.Length > 35)
                    {
                        firstName = response.PersonViewModel.FirstName.Substring(0, 34);
                        firstName = firstName + "...";
                    }
                    else
                    {
                        firstName = response.PersonViewModel.FirstName;
                    }
                }
                if (!string.IsNullOrEmpty(response.PersonViewModel.LastName))
                {
                    if (response.PersonViewModel.LastName.Length > 35)
                    {
                        lastName = response.PersonViewModel.LastName.Substring(0, 34);
                        lastName = lastName + "...";
                    }
                    else
                    {
                        lastName = response.PersonViewModel.LastName;
                    }
                }
                if (!string.IsNullOrEmpty(response.PersonViewModel.FirstName) && !string.IsNullOrEmpty(response.PersonViewModel.LastName) && (response.PersonViewModel.FirstName.Length > 35 || response.PersonViewModel.LastName.Length > 35))
                {
                    response.PersonViewModel.Name = firstName + " " + lastName;
                }
                var Name = response.PersonViewModel.FirstName + " " + response.PersonViewModel.LastName;
                response.PersonViewModel.FullName = Name;
                AddCookie("contactname", Name, 1);
                if (response.PersonViewModel != null && log != null && log.Equals("true"))
                    UpdateUserActivityLog(contactId, response.PersonViewModel.FirstName + " " + response.PersonViewModel.LastName);
                if (log != null && log.Equals("false"))
                    AddCookie("log", "true", 1);
                var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
                var lifecycleStages = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LifeCycle).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(s => s.IsActive == true);
                var defaultLifeCycleType = lifecycleStages.Where(s => s.IsDefault).FirstOrDefault();
                if (response.PersonViewModel.LifecycleStage == 0)
                    response.PersonViewModel.LifecycleStage = defaultLifeCycleType != null ? (short)defaultLifeCycleType.DropdownValueID : (short)0;
                short getLifeCycleStageTypeId = lifecycleStages.Where(l => l.DropdownValueID == response.PersonViewModel.LifecycleStage).Select(l => l.DropdownValueTypeID).FirstOrDefault();
                response.PersonViewModel.LifecycleStages = lifecycleStages;
                response.PersonViewModel.DropdownValueTypeId = getLifeCycleStageTypeId;
                response.PersonViewModel.PhoneTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.PhoneNumberType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(s => s.IsActive == true);
                var defaultPhoneType = response.PersonViewModel.PhoneTypes.Where(s => s.IsDefault).FirstOrDefault();
                phones.Add(new Phone()
                {
                    ContactPhoneNumberID = 0,
                    PhoneType = defaultPhoneType.DropdownValueID,
                    PhoneTypeName = defaultPhoneType.DropdownValue,
                    Number = "",
                    IsPrimary = false
                });
                response.PersonViewModel.PhoneList = response.PersonViewModel.Phones;
                if (response.PersonViewModel.Phones.Where(p => p.IsPrimary == true).Any())
                    response.PersonViewModel.Phones = new List<Phone>() { response.PersonViewModel.Phones.Where(p => p.IsPrimary == true).FirstOrDefault() };

                if (response.PersonViewModel.Phones.Count > 0 && !response.PersonViewModel.Phones.Where(p => p.IsPrimary == true).Any())
                    response.PersonViewModel.Phones = phones;

                if (response.PersonViewModel.Phones.Count == 0)
                {
                    response.PersonViewModel.Phones = phones;
                }
                response.PersonViewModel.LeadSources = dropdownValues.Where(s => s.DropdownID == (short)DropdownFieldTypes.LeadSources).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
                response.PersonViewModel.AddressTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.AddressType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
                var defaultAddressType = response.PersonViewModel.AddressTypes.Where(s => s.IsDefault).FirstOrDefault();
                if (response.PersonViewModel.Addresses.Count == 0)
                {
                    response.PersonViewModel.Addresses.Add(new AddressViewModel()
                    {
                        AddressID = 0,
                        AddressTypeID = defaultAddressType.DropdownValueID,
                        Country = new Country
                        {
                            Code = ""
                        },
                        State = new State
                        {
                            Code = ""
                        },
                        IsDefault = false
                    });
                }
                emails.Add(new Email()
                {
                    EmailId = "",
                    EmailStatusValue = EmailStatus.NotVerified,
                    IsPrimary = false,
                    EmailID = 0
                });
                response.PersonViewModel.EmailList = response.PersonViewModel.Emails;
                if (response.PersonViewModel.Emails.Where(p => p.IsPrimary == true).Any())
                    response.PersonViewModel.Emails = new List<Email>() { response.PersonViewModel.Emails.Where(p => p.IsPrimary == true).FirstOrDefault() };

                if (response.PersonViewModel.Emails.Count == 0)
                {
                    response.PersonViewModel.Emails = emails;
                }
                string totalHits = "0";
                if (Session["TotalHits"] != null)
                    totalHits = Session["TotalHits"].ToString();
                Session["TotalHits"] = totalHits;
                Session["contactindex"] = index;
                AddCookie("contactindex", index.ToString(), 1);
                GetAccountDropboxKeyResponse accountResponse = accountService.GetAccountDropboxKey(new GetAccountIdRequest()
                {
                    accountId = this.Identity.ToAccountID()
                });
                if (!string.IsNullOrEmpty(accountResponse.DropboxKey))
                    Session["dropboxkey"] = accountResponse.DropboxKey;
                else
                    Session["dropboxkey"] = null;

                ViewBag.DateFormat = this.Identity.ToDateFormat();
                ViewBag.CurrencyFormat = this.Identity.ToCurrency();
                ViewBag.IsPeople = ReadCookie("IsPeople");
                ViewBag.TagPopup = true;
                short ItemsPerPage = default(short);
                short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
                ViewBag.ItemsPerPage = ItemsPerPage;
                response.PersonViewModel.DateFormat = this.Identity.ToDateFormat();
                if (response.PersonViewModel.LastTouchedDate.HasValue)
                    response.PersonViewModel.LastContactedString = response.PersonViewModel.LastTouchedDate.Value.ToJSDate().ToString(this.Identity.ToDateFormat(), CultureInfo.InvariantCulture) + " (" + response.PersonViewModel.LastTouchedType + ")";
                return View("PersonDetails", response.PersonViewModel);
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(PrivateDataAccessException))
                    return RedirectToAction("AccessDenied", "Error");
                else
                    throw ex;
            }
        }

        /// <summary>
        /// Gets the relationships.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public JsonResult GetRelationships(int contactId)
        {
            RelationshipViewModel relations = contactRelationshipService.GetContactRelationships(contactId).RelationshipViewModel;
            return Json(new
            {
                success = true,
                response = relations
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the opportunities.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public JsonResult GetOpportunities(int contactId, int pageNumber = 1, int pageSize = 10)
        {
            IEnumerable<OpportunityBuyer> opportunities = opportunityService.GetContactOpportunitiesList(new GetOpportunityListByContactRequest()
            {
                ContactID = contactId,
                RequestedBy = this.Identity.ToUserID(),
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID(),
                PageNumber = pageNumber,
                PageSize = pageSize
            }).Opportunities;
            return Json(new
            {
                success = true,
                response = opportunities
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the actions.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public JsonResult GetActions(int contactId, int pageNumber = 1, int pageSize = 10)
        {
            IEnumerable<ActionViewModel> actions = null;
            GetActionListResponse response = actionService.GetContactActions(new GetActionListRequest()
            {
                Id = contactId,
                PageNumber = pageNumber,
                Limit = pageSize
            });
            if (response != null)
            {
                var data = response.ActionListViewModel.ToList();
                data.ForEach(a => a.RemindOn = (a.RemindOn == null ? null : (DateTime?)a.RemindOn.Value.ToUtcBrowserDatetime()));
                //data.ForEach(a => a.ActionDate = (a.ActionDate == null ? null : (DateTime?)a.ActionDate.Value.ToUtcBrowserDatetime()));
                data.Each(a =>
                {
                    DateTime date = a.ActionDate == null ? DateTime.MinValue : (DateTime)a.ActionDate.Value;
                    DateTime startTime = a.ActionStartTime == null ? DateTime.MinValue : (DateTime)a.ActionStartTime.Value;
                    DateTime Datevalue = new DateTime(date.Year, date.Month, date.Day, startTime.Hour, startTime.Minute, startTime.Second);
                    a.ActionDate = Datevalue.ToUtc();
                });
                actions = response.ActionListViewModel.Reverse();
            }
            return Json(new
            {
                success = true,
                response = actions
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Deletes the contact.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.Contacts, AppOperations.Delete)]
        public ActionResult DeleteContact(int? id)
        {
            if (!id.HasValue)
            {
                return Json(new
                {
                    success = true,
                    response = "Could not delete the contact."
                }, JsonRequestBehavior.AllowGet);
            }
            DeactivateContactRequest request = new DeactivateContactRequest(id.Value);
            request.AccountId = Thread.CurrentPrincipal.Identity.ToAccountID();
            request.RequestedBy = this.Identity.ToUserID();
            request.RoleId = this.Identity.ToRoleID();
            contactService.Deactivate(request);
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Adds this instance.
        /// </summary>
        /// <returns></returns>
        [MenuType(MenuCategory.ContactAction, MenuCategory.LeftMenuCRM)]
        public ActionResult Add()
        {
            return View("AddEditContact");
        }

        /// <summary>
        /// Copies the person.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="contactType">Type of the contact.</param>
        /// <returns></returns>
        [Route("copyperson")]
        [SmarttouchAuthorize(AppModules.Contacts, AppOperations.Create)]
        [MenuType(MenuCategory.ContactAction, MenuCategory.LeftMenuCRM)]
        public ActionResult CopyPerson(int contactId, int contactType)
        {
            ViewBag.IsAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
            ViewBag.IsSTAdmin = this.Identity.IsSTAdmin();
            CopyContactRequest request = new CopyContactRequest()
            {
                contactID = contactId,
                contactType = contactType
            };
            request.AccountID = this.Identity.ToAccountID();
            if (contactType == 1)
            {
                GetPersonResponse response = contactService.GetPerson(new GetPersonRequest(contactId)
                {
                    RequestedBy = this.Identity.ToUserID(),
                    AccountId = this.Identity.ToAccountID(),
                    RoleId = this.Identity.ToRoleID()
                });
                if (response.Exception == null)
                {
                    IList<Email> emails = new List<Email>();
                    IList<Phone> phones = new List<Phone>();
                    IList<Url> socialMediaUrls = new List<Url>();
                    emails.Add(new Email()
                    {
                        EmailId = "",
                        EmailStatusValue = EmailStatus.NotVerified,
                        IsPrimary = true,
                        EmailID = 0
                    });
                    response.PersonViewModel.Emails = emails;
                    var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
                    GetUserResponse userresponse = userService.GetUser(new GetUserRequest(this.Identity.ToUserID()));
                    response.PersonViewModel.OwnerId = userresponse.User.UserID;
                    response.PersonViewModel.OwnerName = userresponse.User.Name;
                    response.PersonViewModel.LeadSources = dropdownValues.Where(s => s.DropdownID == (short)DropdownFieldTypes.LeadSources).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
                    response.PersonViewModel.AddressTypes = dropdownValues.Where(s => s.DropdownID == (short)DropdownFieldTypes.AddressType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
                    response.PersonViewModel.LifecycleStages = dropdownValues.Where(s => s.DropdownID == (short)DropdownFieldTypes.LifeCycle).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
                    response.PersonViewModel.PartnerTypes = dropdownValues.Where(s => s.DropdownID == (short)DropdownFieldTypes.PartnerType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
                    response.PersonViewModel.PhoneTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.PhoneNumberType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
                    var defaultPhoneType = response.PersonViewModel.PhoneTypes.Where(s => s.IsDefault).FirstOrDefault();
                    if (response.PersonViewModel.Phones.Count == 0)
                    {
                        phones.Add(new Phone()
                        {
                            ContactPhoneNumberID = 0,
                            PhoneType = defaultPhoneType.DropdownValueID,
                            PhoneTypeName = defaultPhoneType.DropdownValue,
                            Number = "",
                            IsPrimary = true
                        });
                        response.PersonViewModel.Phones = phones;
                    }
                    response.PersonViewModel.AddressTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.AddressType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
                    var defaultAddressType = response.PersonViewModel.AddressTypes.Where(s => s.IsDefault).FirstOrDefault();
                    response.PersonViewModel.Image = new ImageViewModel();
                    if (response.PersonViewModel.Addresses.Count == 0)
                    {
                        response.PersonViewModel.Addresses.Add(new AddressViewModel()
                        {
                            AddressID = 0,
                            AddressTypeID = defaultAddressType.DropdownValueID,
                            Country = new Country
                            {
                                Code = ""
                            },
                            State = new State
                            {
                                Code = ""
                            },
                            IsDefault = true
                        });
                    }
                    ViewBag.NewAddress = new AddressViewModel()
                    {
                        AddressID = 0,
                        AddressTypeID = defaultAddressType.DropdownValueID,
                        Country = new Country
                        {
                            Code = ""
                        },
                        State = new State
                        {
                            Code = ""
                        },
                        IsDefault = false
                    };
                    if (response.PersonViewModel.SocialMediaUrls.Count == 0)
                    {
                        socialMediaUrls.Add(new Url
                        {
                            MediaType = "Website",
                            URL = ""
                        });
                        response.PersonViewModel.SocialMediaUrls = socialMediaUrls;
                    }
                    GetAllCustomFieldTabsRequest customFieldTabs = new GetAllCustomFieldTabsRequest(response.PersonViewModel.AccountID);
                    response.PersonViewModel.CustomFieldTabs = customFieldService.GetAllCustomFieldTabs(customFieldTabs).CustomFieldsViewModel.CustomFieldTabs;
                    PersonViewModel viewmodel = contactService.CopyPerson(response.PersonViewModel);
                    if (viewmodel != null)
                        return View("AddEditPerson", viewmodel);
                }
            }
            return View("Error/Index");
        }

        /// <summary>
        /// Gets the lead sources.
        /// </summary>
        /// <param name="DropDownID">The drop down identifier.</param>
        /// <returns></returns>
        public JsonResult GetLeadSources(int DropDownID)
        {
            GetLeadSourcesResponse response = dropdownValuesService.GetLeadSources(new GetLeadSourcesRequest()
            {
                DropdownId = DropDownID,
                AccountId = this.Identity.ToAccountID()
            });
            return Json(new
            {
                success = true,
                response = response.LeadSources
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region Company
        /// <summary>
        /// Adds the company.
        /// </summary>
        /// <returns></returns>
        [Route("addcompany")]
        [SmarttouchAuthorize(AppModules.Contacts, AppOperations.Create)]
        [MenuType(MenuCategory.CompanyAction, MenuCategory.LeftMenuCRM)]
        public ActionResult AddCompany()
        {
            CompanyViewModel viewModel = new CompanyViewModel();
            ViewBag.IsAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
            ViewBag.IsSTAdmin = this.Identity.IsSTAdmin();
            GetUsersResponse response = contactService.GetUsers(new GetUsersRequest()
            {
                AccountID = this.Identity.ToAccountID(),
                UserId = this.Identity.ToUserID(),
                IsSTadmin = this.Identity.IsSTAdmin()
            });
            viewModel.OwnerId = response.Owner.ToList()[0].OwnerId;
            viewModel.OwnerName = response.Owner.ToList()[0].OwnerName;
            viewModel.AccountID = this.Identity.ToAccountID();
            viewModel.Addresses = new List<AddressViewModel>();
            IList<Phone> phones = new List<Phone>();
            IList<Email> emails = new List<Email>();
            IList<Url> socialMediaUrls = new List<Url>();
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            viewModel.AddressTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.AddressType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            var defaultAddressType = viewModel.AddressTypes.Where(s => s.IsDefault).FirstOrDefault();
            viewModel.Addresses.Add(new AddressViewModel()
            {
                AddressID = 0,
                AddressTypeID = defaultAddressType.DropdownValueID,
                Country = new Country
                {
                    Code = ""
                },
                State = new State
                {
                    Code = ""
                },
                IsDefault = true
            });
            viewModel.Image = new ImageViewModel();
            viewModel.PhoneTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.PhoneNumberType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            var defaultPhoneType = viewModel.PhoneTypes.Where(s => s.IsDefault).FirstOrDefault();
            phones.Add(new Phone()
            {
                ContactPhoneNumberID = 0,
                PhoneType = defaultPhoneType.DropdownValueID,
                PhoneTypeName = defaultPhoneType.DropdownValue,
                Number = "",
                IsPrimary = true
            });
            emails.Add(new Email()
            {
                EmailId = "",
                EmailStatusValue = EmailStatus.NotVerified,
                IsPrimary = true,
                EmailID = 0
            });
            socialMediaUrls.Add(new Url
            {
                MediaType = "Website",
                URL = ""
            });
            viewModel.Phones = phones;
            viewModel.Emails = emails;
            viewModel.SocialMediaUrls = socialMediaUrls;
            GetAllCustomFieldTabsRequest customFieldTabs = new GetAllCustomFieldTabsRequest(viewModel.AccountID);
            viewModel.CustomFieldTabs = customFieldService.GetAllCustomFieldTabs(customFieldTabs).CustomFieldsViewModel.CustomFieldTabs;
            ViewBag.NewAddress = new AddressViewModel()
            {
                AddressID = 0,
                AddressTypeID = defaultAddressType.DropdownValueID,
                Country = new Country
                {
                    Code = ""
                },
                State = new State
                {
                    Code = ""
                },
                IsDefault = false
            };
            ViewBag.mode = "AddCompany";
            return View("AddEditCompany", viewModel);
        }

        /// <summary>
        /// Edits the company.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="contactType">Type of the contact.</param>
        /// <returns></returns>
        [Route("editcompany/")]
        [SmarttouchAuthorize(AppModules.Contacts, AppOperations.Edit)]
        [MenuType(MenuCategory.CompanyAction, MenuCategory.LeftMenuCRM)]
        public ActionResult EditCompany(int contactId, int contactType)
        {
            ViewBag.IsAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
            ViewBag.IsSTAdmin = this.Identity.IsSTAdmin();
            GetCompanyResponse response = contactService.GetCompany(new GetCompanyRequest(contactId)
            {
                RequestedBy = this.Identity.ToUserID(),
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID()
            });
            GetUsersResponse userresponse = contactService.GetUsers(new GetUsersRequest()
            {
                AccountID = this.Identity.ToAccountID(),
                UserId = response.CompanyViewModel.OwnerId == 0 || response.CompanyViewModel.OwnerId == null ? this.Identity.ToUserID() : (int)response.CompanyViewModel.OwnerId,
                IsSTadmin = this.Identity.IsSTAdmin()
            });
            response.CompanyViewModel.OwnerId = userresponse.Owner.Count() != 0 ? userresponse.Owner.ToList()[0].OwnerId : 0;
            response.CompanyViewModel.OwnerName = userresponse.Owner.Count() != 0 ? userresponse.Owner.ToList()[0].OwnerName : "";
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            GetAllCustomFieldTabsRequest customFieldTabs = new GetAllCustomFieldTabsRequest(response.CompanyViewModel.AccountID);
            response.CompanyViewModel.CustomFieldTabs = customFieldService.GetAllCustomFieldTabs(customFieldTabs).CustomFieldsViewModel.CustomFieldTabs;
            response.CompanyViewModel.AddressTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.AddressType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            response.CompanyViewModel.PhoneTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.PhoneNumberType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            if (response.CompanyViewModel.Addresses.Count == 0)
            {
                response.CompanyViewModel.Addresses = new List<AddressViewModel>() {
                    new AddressViewModel () {
                        AddressID = 0,
                        AddressTypeID = response.CompanyViewModel.AddressTypes.SingleOrDefault (a => a.IsDefault).DropdownValueID,
                        State = new State {
                            Code = ""
                        },
                        Country = new Country {
                            Code = ""
                        },
                        IsDefault = true
                    }
                };
            }
            if (response.CompanyViewModel.Phones.Count == 0)
            {
                IList<Phone> phones = new List<Phone>();
                phones.Add(new Phone()
                {
                    PhoneType = response.CompanyViewModel.PhoneTypes.SingleOrDefault(a => a.IsDefault).DropdownValueID,
                    PhoneTypeName = response.CompanyViewModel.PhoneTypes.SingleOrDefault(a => a.IsDefault).DropdownValue,
                    Number = "",
                    IsPrimary = true,
                    ContactID = contactId,
                    ContactPhoneNumberID = 0
                });
                response.CompanyViewModel.Phones = phones;
            }
            if (response.CompanyViewModel.Emails.Count == 0)
            {
                IList<Email> emails = new List<Email>();
                emails.Add(new Email()
                {
                    EmailId = "",
                    EmailStatusValue = EmailStatus.NotVerified,
                    IsPrimary = true,
                    EmailID = 0,
                    ContactID = contactId
                });
                response.CompanyViewModel.Emails = emails;
            }
            if (response.CompanyViewModel.SocialMediaUrls.Count == 0)
            {
                IList<Url> socialMediaUrls = new List<Url>();
                socialMediaUrls.Add(new Url
                {
                    MediaType = "Website",
                    URL = ""
                });
                response.CompanyViewModel.SocialMediaUrls = socialMediaUrls;
            }
            ViewBag.mode = "EditCompany";
            ViewBag.NewAddress = new AddressViewModel()
            {
                AddressID = 0,
                AddressTypeID = response.CompanyViewModel.AddressTypes.SingleOrDefault(a => a.IsDefault).DropdownValueID,
                Country = new Country
                {
                    Code = ""
                },
                State = new State
                {
                    Code = ""
                },
                IsDefault = false
            };
            return View("AddEditCompany", response.CompanyViewModel);
        }

        /// <summary>
        /// Inserts the company.
        /// </summary>
        /// <param name="companyViewModel">The company view model.</param>
        /// <returns></returns>
        public JsonResult InsertCompany(string companyViewModel)
        {
            CompanyViewModel viewModel = JsonConvert.DeserializeObject<CompanyViewModel>(companyViewModel);
            bool isStAdmin = this.Identity.IsSTAdmin();
            int accountId = this.Identity.ToAccountID();
            int userId = this.Identity.ToUserID();
            string dateFormat = this.Identity.ToDateFormat();
            DateTime utcTime = DateTime.Now.ToUniversalTime();
            viewModel.AccountID = accountId;
            viewModel.LastUpdatedBy = userId;
            viewModel.LastUpdatedOn = utcTime;
            viewModel.CreatedBy = userId;
            viewModel.CreatedOn = utcTime;
            viewModel.DateFormat = dateFormat;
            viewModel.ReferenceId = Guid.NewGuid();
            viewModel = ManageCompanySocialProfiles(viewModel).Values.FirstOrDefault();
            InsertCompanyRequest request = new InsertCompanyRequest()
            {
                CompanyViewModel = viewModel,
                RequestedBy = userId,
                AccountId = accountId,
                RoleId = this.Identity.ToRoleID(),
                ModuleId = (byte)AppModules.Contacts,
                isStAdmin = isStAdmin
            };
            InsertCompanyResponse response = contactService.InsertCompany(request);
            var getContactDetails = contactService.GetCompany(new GetCompanyRequest(response.CompanyViewModel.ContactID)
            {
                AccountId = accountId
            });
            return Json(new
            {
                success = true,
                response = getContactDetails.CompanyViewModel.ContactID
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Updates the company.
        /// </summary>
        /// <param name="companyViewModel">The company view model.</param>
        /// <returns></returns>
        [HttpPost]
        [MenuType(MenuCategory.ContactAction, MenuCategory.LeftMenuCRM)]
        public JsonResult UpdateCompany(string companyViewModel)
        {
            CompanyViewModel viewModel = JsonConvert.DeserializeObject<CompanyViewModel>(companyViewModel);
            bool isStAdmin = this.Identity.IsSTAdmin();
            viewModel.AccountID = this.Identity.ToAccountID();
            viewModel.LastUpdatedBy = this.Identity.ToUserID();
            string dateFormat = this.Identity.ToDateFormat();
            viewModel.LastUpdatedOn = DateTime.Now.ToUniversalTime();
            viewModel.DateFormat = dateFormat;
            UpdateCompanyRequest request = new UpdateCompanyRequest()
            {
                CompanyViewModel = viewModel,
                RequestedBy = this.Identity.ToUserID(),
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID(),
                ModuleId = (byte)AppModules.Contacts,
                isStAdmin = isStAdmin
            };
            contactService.UpdateCompany(request);
            if (viewModel.OwnerId != viewModel.PreviousOwnerId)
            {
                InsertChangeOwnerActivity(viewModel.ContactID, viewModel.OwnerId.Value);
            }
            var contactDetails = contactService.GetCompany(new GetCompanyRequest(viewModel.ContactID)
            {
                AccountId = this.Identity.ToAccountID()
            });
            return Json(new
            {
                success = true,
                response = contactDetails.CompanyViewModel.ContactID
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Companies the details.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        [SmarttouchSessionStateBehaviour(SessionStateBehavior.Required)]
        [Route("company/{contactId}/{index?}")]
        [SmarttouchAuthorize(AppModules.Contacts, AppOperations.Read)]
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public ActionResult CompanyDetails(int contactId, int index = 0)
        {
            ViewBag.IsSTAdmin = this.Identity.IsSTAdmin();
            RemoveCookie("contactid");
            RemoveCookie("period");
            RemoveCookie("module");
            RemoveCookie("contactindex");
            RemoveCookie("widgetperiod");
            var log = ReadCookie("log");
            GetCompanyResponse response = new GetCompanyResponse();
            List<Phone> phones = new List<Phone>();
            List<Email> emails = new List<Email>();
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            response = contactService.GetCompany(new GetCompanyRequest(contactId)
            {
                RequestedBy = this.Identity.ToUserID(),
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID()
            });
            if (response.CompanyViewModel == null)
                return RedirectToAction("NotFound", "Error");
            AddCookie("contactid", contactId.ToString(), 1);
            GetUsersResponse userresponse = contactService.GetUsers(new GetUsersRequest()
            {
                AccountID = this.Identity.ToAccountID(),
                UserId = response.CompanyViewModel.OwnerId == 0 || response.CompanyViewModel.OwnerId == null ? 0 : (int)response.CompanyViewModel.OwnerId,
                IsSTadmin = this.Identity.IsSTAdmin()
            });
            response.CompanyViewModel.OwnerName = userresponse.Owner.Count() == 1 ? userresponse.Owner.ToList()[0].OwnerName : "";
            response.CompanyViewModel.PhoneTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.PhoneNumberType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            var defaultPhoneType = response.CompanyViewModel.PhoneTypes.Where(s => s.IsDefault).FirstOrDefault();
            phones.Add(new Phone()
            {
                ContactPhoneNumberID = 0,
                PhoneType = defaultPhoneType.DropdownValueID,
                PhoneTypeName = defaultPhoneType.DropdownValue,
                Number = "",
                IsPrimary = false
            });
            response.CompanyViewModel.PhoneList = response.CompanyViewModel.Phones;
            response.CompanyViewModel.Phones = response.CompanyViewModel.Phones.Where(p => p.IsPrimary == true).ToList();

            if (response.CompanyViewModel.Phones.Count > 0 && !response.CompanyViewModel.Phones.Where(p => p.IsPrimary == true).Any())
                response.CompanyViewModel.Phones = phones;

            if (response.CompanyViewModel.Phones.Count == 0)
                response.CompanyViewModel.Phones = phones;
            response.CompanyViewModel.AddressTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.AddressType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            var defaultAddressType = response.CompanyViewModel.AddressTypes.Where(s => s.IsDefault).FirstOrDefault();
            if (response.CompanyViewModel.Addresses.Count == 0)
            {
                response.CompanyViewModel.Addresses.Add(new AddressViewModel()
                {
                    AddressID = 0,
                    AddressTypeID = defaultAddressType.DropdownValueID,
                    Country = new Country
                    {
                        Code = ""
                    },
                    State = new State
                    {
                        Code = ""
                    },
                    IsDefault = false
                });
            }
            emails.Add(new Email()
            {
                EmailId = "",
                EmailStatusValue = EmailStatus.NotVerified,
                IsPrimary = false,
                EmailID = 0
            });
            response.CompanyViewModel.EmailList = response.CompanyViewModel.Emails;
            response.CompanyViewModel.Emails = response.CompanyViewModel.Emails.Where(p => p.IsPrimary == true).ToList();
            if (response.CompanyViewModel.Emails.Count == 0)
                response.CompanyViewModel.Emails = emails;
            if (response.CompanyViewModel != null && log != null && log.Equals("true"))
                UpdateUserActivityLog(contactId, response.CompanyViewModel.CompanyName);
            if (log != null && log.Equals("false"))
                AddCookie("log", "true", 1);
            GetAccountResponse accountResponse = accountService.GetAccount(new GetAccountRequest(this.Identity.ToAccountID())
            {
                RequestedBy = this.Identity.ToUserID()
            });
            if (accountResponse.AccountViewModel != null)
                Session["dropboxkey"] = accountResponse.AccountViewModel.DropboxAppKey;
            else
                Session["dropboxkey"] = null;
            string totalHits = "0";
            if (Session["TotalHits"] != null)
                totalHits = Session["TotalHits"].ToString();
            Session["TotalHits"] = totalHits;
            Session["contactindex"] = index;
            AddCookie("contactindex", index.ToString(), 1);
            AddCookie("dateformat", this.Identity.ToDateFormat(), 1);
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            ViewBag.CurrencyFormat = this.Identity.ToCurrency();
            ViewBag.IsPeople = ReadCookie("IsPeople");
            ViewBag.TagPopup = true;
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            response.CompanyViewModel.ContactType = "2";
            return View("CompanyDetails", response.CompanyViewModel);
        }

        /// <summary>
        /// Copies the company.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="contactType">Type of the contact.</param>
        /// <returns></returns>
        [Route("copycompany")]
        [SmarttouchAuthorize(AppModules.Contacts, AppOperations.Create)]
        [MenuType(MenuCategory.CompanyAction, MenuCategory.LeftMenuCRM)]
        public ActionResult CopyCompany(int contactId, int contactType)
        {
            ViewBag.IsAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
            ViewBag.IsSTAdmin = this.Identity.IsSTAdmin();
            CopyContactRequest request = new CopyContactRequest()
            {
                contactID = contactId,
                contactType = contactType
            };
            request.AccountID = this.Identity.ToAccountID();
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            if (contactType == 2)
            {
                GetCompanyResponse response = contactService.GetCompany(new GetCompanyRequest(contactId)
                {
                    RequestedBy = this.Identity.ToUserID(),
                    AccountId = this.Identity.ToAccountID(),
                    RoleId = this.Identity.ToRoleID()
                });
                if (response.Exception == null)
                {
                    IList<Email> emails = new List<Email>();
                    IList<Phone> phones = new List<Phone>();
                    IList<Url> socialMediaUrls = new List<Url>();
                    emails.Add(new Email()
                    {
                        EmailId = "",
                        EmailStatusValue = EmailStatus.NotVerified,
                        IsPrimary = true,
                        EmailID = 0
                    });
                    response.CompanyViewModel.Emails = emails;
                    GetUserResponse userresponse = userService.GetUser(new GetUserRequest(this.Identity.ToUserID()));
                    response.CompanyViewModel.OwnerId = userresponse.User.UserID;
                    response.CompanyViewModel.OwnerName = userresponse.User.Name;
                    response.CompanyViewModel.AddressTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.AddressType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
                    response.CompanyViewModel.PhoneTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.PhoneNumberType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
                    var defaultPhoneType = response.CompanyViewModel.PhoneTypes.Where(s => s.IsDefault).FirstOrDefault();
                    if (response.CompanyViewModel.Phones.Count == 0)
                    {
                        phones.Add(new Phone()
                        {
                            ContactPhoneNumberID = 0,
                            PhoneType = defaultPhoneType.DropdownValueID,
                            PhoneTypeName = defaultPhoneType.DropdownValue,
                            Number = "",
                            IsPrimary = true
                        });
                        response.CompanyViewModel.Phones = phones;
                    }
                    response.CompanyViewModel.AddressTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.AddressType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
                    var defaultAddressType = response.CompanyViewModel.AddressTypes.Where(s => s.IsDefault).FirstOrDefault();
                    response.CompanyViewModel.Image = new ImageViewModel();
                    if (response.CompanyViewModel.Addresses.Count == 0)
                    {
                        response.CompanyViewModel.Addresses.Add(new AddressViewModel()
                        {
                            AddressID = 0,
                            AddressTypeID = defaultAddressType.DropdownValueID,
                            Country = new Country
                            {
                                Code = ""
                            },
                            State = new State
                            {
                                Code = ""
                            },
                            IsDefault = true
                        });
                    }
                    ViewBag.NewAddress = new AddressViewModel()
                    {
                        AddressID = 0,
                        AddressTypeID = defaultAddressType.DropdownValueID,
                        Country = new Country
                        {
                            Code = ""
                        },
                        State = new State
                        {
                            Code = ""
                        },
                        IsDefault = false
                    };
                    if (response.CompanyViewModel.SocialMediaUrls.Count == 0)
                    {
                        socialMediaUrls.Add(new Url
                        {
                            MediaType = "Website",
                            URL = ""
                        });
                        response.CompanyViewModel.SocialMediaUrls = socialMediaUrls;
                    }
                    GetAllCustomFieldTabsRequest customFieldTabs = new GetAllCustomFieldTabsRequest(response.CompanyViewModel.AccountID);
                    response.CompanyViewModel.CustomFieldTabs = customFieldService.GetAllCustomFieldTabs(customFieldTabs).CustomFieldsViewModel.CustomFieldTabs;
                    CompanyViewModel viewmodel = contactService.CopyCompany(response.CompanyViewModel);
                    if (viewmodel != null)
                        return View("AddEditCompany", viewmodel);
                }
            }
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Changes the owner.
        /// </summary>
        /// <param name="changeOwnerViewModel">The change owner view model.</param>
        /// <returns></returns>
        [SmarttouchAuthorizeAttribute(AppModules.ChangeOwner, AppOperations.Read)]
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public JsonResult ChangeOwner(string changeOwnerViewModel)
        {
            ChangeOwnerViewModel viewModel = JsonConvert.DeserializeObject<ChangeOwnerViewModel>(changeOwnerViewModel);
            contactService.ChangeOwner(new ChangeOwnerRequest()
            {
                ChangeOwnerViewModel = viewModel,
                RequestedBy = this.Identity.ToUserID(),
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID(),
                ModuleId = (byte)AppModules.Contacts
            });
            var viewModel1 = (AdvancedSearchViewModel)Session["AdvancedSearchVM"];
            var strngdata = Newtonsoft.Json.JsonConvert.SerializeObject(viewModel1);
            var drillDownContactIds = (int[])Session["ContactListIDs"];
            if (viewModel.SelectAll == true)
            {
                BulkOperations operationData = new BulkOperations()
                {
                    OperationID = (int)viewModel.OwnerId,
                    OperationType = (int)BulkOperationTypes.ChangeOwner,
                    SearchCriteria = HttpUtility.HtmlEncode(Server.UrlDecode(ReadCookie("selectallsearchstring"))),
                    AdvancedSearchCriteria = strngdata,
                    SearchDefinitionID = null,
                    AccountID = Thread.CurrentPrincipal.Identity.ToAccountID(),
                    UserID = (int)Thread.CurrentPrincipal.Identity.ToUserID(),
                    RoleID = Thread.CurrentPrincipal.Identity.ToRoleID()
                };
                InsertBulkOperationRequest bulkOperationRequest = new InsertBulkOperationRequest()
                {
                    OperationData = operationData,
                    AccountId = Thread.CurrentPrincipal.Identity.ToAccountID(),
                    RequestedBy = Thread.CurrentPrincipal.Identity.ToUserID(),
                    CreatedOn = DateTime.Now.ToUniversalTime().AddMinutes(1),
                    RoleId = Thread.CurrentPrincipal.Identity.ToRoleID(),
                    DrillDownContactIds = drillDownContactIds
                };
                accountService.InsertBulkOperation(bulkOperationRequest);
            }
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Updates the name of the contact.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="firstName">The first name.</param>
        /// <param name="lastName">The last name.</param>
        /// <returns></returns>
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public JsonResult UpdateContactName(int contactId, string firstName, string lastName)
        {
            GetPersonResponse response = new GetPersonResponse();
            response.PersonViewModel = contactService.GetPerson(new GetPersonRequest(contactId)
            {
                RequestedBy = this.Identity.ToUserID(),
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID(),
                IncludeLastTouched = true
            }).PersonViewModel;
            response.PersonViewModel.AccountID = this.Identity.ToAccountID();
            response.PersonViewModel.LastUpdatedBy = this.Identity.ToUserID();
            response.PersonViewModel.ContactSource = null;
            response.PersonViewModel.SourceType = null;
            response.PersonViewModel.LastUpdatedOn = DateTime.Now.ToUniversalTime();
            response.PersonViewModel.FirstName = firstName;
            response.PersonViewModel.LastName = lastName;
            UpdateContactViewResponse updateresponse = contactService.UpdateContactName(new UpdateContactViewRequest()
            {
                ContactId = contactId,
                FirstName = firstName,
                LastName = lastName,
                AccountId = this.Identity.ToAccountID(),
                PersonViewModel = response.PersonViewModel,
                LastUpdatedBy = this.Identity.ToUserID(),
                LastUpdatedOn = DateTime.Now.ToUniversalTime()
            });
            if (updateresponse.Exception != null)
            {
                string responseJson = updateresponse.Exception.Message.Replace("\r\n", "</br>").Replace("\n", "</br>");
                return Json(new
                {
                    success = false,
                    response = responseJson
                }, JsonRequestBehavior.AllowGet);
            }
            string firstname = "";
            string lastname = "";
            if (updateresponse.person.FirstName != null && updateresponse.person.FirstName.Length > 35)
            {
                firstname = updateresponse.person.FirstName.Substring(0, 34);
                firstname = firstname + "...";
            }
            else
            {
                firstname = updateresponse.person.FirstName;
            }
            if (updateresponse.person.LastName != null && updateresponse.person.LastName.Length > 35)
            {
                lastname = updateresponse.person.LastName.Substring(0, 34);
                lastname = lastname + "...";
            }
            else
            {
                lastname = updateresponse.person.LastName;
            }
            if ((updateresponse.person.FirstName != null && updateresponse.person.FirstName.Length > 35) || (updateresponse.person.LastName != null && updateresponse.person.LastName.Length > 35))
            {
                updateresponse.person.Name = firstname + " " + lastname;
            }
            response.PersonViewModel.FullName = updateresponse.person.FirstName + " " + updateresponse.person.LastName;
            return Json(new
            {
                success = true,
                response = updateresponse
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Updates the contact title.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="title">The title.</param>
        /// <returns></returns>
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public JsonResult UpdateContactTitle(int contactId, string title)
        {
            GetPersonResponse response = new GetPersonResponse();
            response.PersonViewModel = contactService.GetPerson(new GetPersonRequest(contactId)
            {
                RequestedBy = this.Identity.ToUserID(),
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID(),
                IncludeLastTouched = true
            }).PersonViewModel;
            response.PersonViewModel.AccountID = this.Identity.ToAccountID();
            response.PersonViewModel.LastUpdatedBy = this.Identity.ToUserID();
            response.PersonViewModel.ContactSource = null;
            response.PersonViewModel.SourceType = null;
            response.PersonViewModel.LastUpdatedOn = DateTime.Now.ToUniversalTime();
            response.PersonViewModel.Title = title;
            UpdateContactViewResponse updateresponse = contactService.UpdateContactTitle(new UpdateContactViewRequest()
            {
                ContactId = contactId,
                Title = title,
                AccountId = this.Identity.ToAccountID(),
                PersonViewModel = response.PersonViewModel,
                LastUpdatedBy = this.Identity.ToUserID(),
                LastUpdatedOn = DateTime.Now.ToUniversalTime()
            });
            if (updateresponse.Exception != null)
            {
                string responseJson = updateresponse.Exception.Message.Replace("\r\n", "</br>").Replace("\n", "</br>");
                return Json(new
                {
                    success = false,
                    response = responseJson
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                success = true,
                response = updateresponse
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Updates the name of the company.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="companyName">Name of the company.</param>
        /// <returns></returns>
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public JsonResult UpdateCompanyName(int contactId, string companyName)
        {
            RemoveCookie("widgetperiod");
            GetCompanyResponse response = new GetCompanyResponse();
            response = contactService.GetCompany(new GetCompanyRequest(contactId)
            {
                RequestedBy = this.Identity.ToUserID(),
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID()
            });
            response.CompanyViewModel.AccountID = this.Identity.ToAccountID();
            response.CompanyViewModel.LastUpdatedBy = this.Identity.ToUserID();
            response.CompanyViewModel.LastUpdatedOn = DateTime.Now.ToUniversalTime();
            response.CompanyViewModel.CompanyName = companyName;
            UpdateContactViewResponse updateresponse = contactService.UpdateCompanyName(new UpdateContactViewRequest()
            {
                ContactId = contactId,
                CompanyName = companyName,
                AccountId = this.Identity.ToAccountID(),
                CompanyViewModel = response.CompanyViewModel,
                LastUpdatedBy = this.Identity.ToUserID(),
                LastUpdatedOn = DateTime.Now.ToUniversalTime()
            });
            if (updateresponse.Exception != null)
            {
                string responseJson = updateresponse.Exception.Message.Replace("\r\n", "</br>").Replace("\n", "</br>");
                return Json(new
                {
                    success = false,
                    response = responseJson
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                success = true,
                response = updateresponse
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Updates the contact phone number.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="phoneType">Type of the phone.</param>
        /// <param name="phoneNumber">The phone number.</param>
        /// <returns></returns>
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public JsonResult UpdateContactPhoneNumber(int contactId, short phoneType, string phoneNumber, string countryCode, string extension)
        {
            List<Phone> phones = new List<Phone>();
            Phone phone = new Phone()
            {
                ContactPhoneNumberID = 0,
                PhoneType = phoneType,
                PhoneTypeName = "",
                Number = phoneNumber,
                IsPrimary = true,
                CountryCode = countryCode,
                Extension = extension
            };
            phones.Add(phone);
            GetPersonResponse response = new GetPersonResponse();
            response.PersonViewModel = contactService.GetPerson(new GetPersonRequest(contactId)
            {
                RequestedBy = this.Identity.ToUserID(),
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID(),
                IncludeLastTouched = true
            }).PersonViewModel;
            response.PersonViewModel.AccountID = this.Identity.ToAccountID();
            response.PersonViewModel.LastUpdatedBy = this.Identity.ToUserID();
            response.PersonViewModel.ContactSource = null;
            response.PersonViewModel.SourceType = null;
            response.PersonViewModel.LastUpdatedOn = DateTime.Now.ToUniversalTime();
            response.PersonViewModel.Phones = phones;
            UpdateContactViewResponse updateresponce = contactService.UpdateContactPhone(new UpdateContactViewRequest()
            {
                ContactId = contactId,
                Phone = phone,
                AccountId = this.Identity.ToAccountID(),
                PersonViewModel = response.PersonViewModel,
                LastUpdatedBy = this.Identity.ToUserID(),
                LastUpdatedOn = DateTime.Now.ToUniversalTime()
            });
            if (updateresponce.Exception != null)
            {
                string responseJson = updateresponce.Exception.Message.Replace("\r\n", "</br>").Replace("\n", "</br>");
                return Json(new
                {
                    success = false,
                    response = responseJson
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                success = true,
                response = updateresponce
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Updates the company phone number.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="phoneType">Type of the phone.</param>
        /// <param name="phoneNumber">The phone number.</param>
        /// <returns></returns>
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public JsonResult UpdateCompanyPhoneNumber(int contactId, short phoneType, string phoneNumber, string countryCode, string extension)
        {
            List<Phone> phones = new List<Phone>();
            Phone phone = new Phone()
            {
                ContactPhoneNumberID = 0,
                PhoneType = phoneType,
                PhoneTypeName = "",
                Number = phoneNumber,
                IsPrimary = true,
                CountryCode = countryCode,
                Extension = extension
            };
            phones.Add(phone);
            GetCompanyResponse response = new GetCompanyResponse();
            response = contactService.GetCompany(new GetCompanyRequest(contactId)
            {
                RequestedBy = this.Identity.ToUserID(),
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID()
            });
            response.CompanyViewModel.AccountID = this.Identity.ToAccountID();
            response.CompanyViewModel.LastUpdatedBy = this.Identity.ToUserID();
            response.CompanyViewModel.LastUpdatedOn = DateTime.Now.ToUniversalTime();
            response.CompanyViewModel.Phones = phones;
            UpdateContactViewResponse updateresponce = contactService.UpdateContactPhone(new UpdateContactViewRequest()
            {
                ContactId = contactId,
                Phone = phone,
                AccountId = this.Identity.ToAccountID(),
                CompanyViewModel = response.CompanyViewModel,
                LastUpdatedBy = this.Identity.ToUserID(),
                LastUpdatedOn = DateTime.Now.ToUniversalTime()
            });
            if (updateresponce.Exception != null)
            {
                string responseJson = updateresponce.Exception.Message.Replace("\r\n", "</br>").Replace("\n", "</br>");
                return Json(new
                {
                    success = false,
                    response = responseJson
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                success = true,
                response = updateresponce
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Updates the contact lifecycle stage.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="lifecycleStage">The lifecycle stage.</param>
        /// <param name="previousLifecyclestage">The previous lifecyclestage.</param>
        /// <returns></returns>
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public JsonResult UpdateContactLifecycleStage(int contactId, short lifecycleStage, short previousLifecyclestage)
        {
            UpdateContactViewResponse response = contactService.UpdateContactLifecycleStage(new UpdateContactViewRequest()
            {
                ContactId = contactId,
                LifecycleStage = lifecycleStage,
                PreviousLifecycleStage = previousLifecyclestage,
                AccountId = this.Identity.ToAccountID(),
                LastUpdatedBy = this.Identity.ToUserID(),
                LastUpdatedOn = DateTime.Now.ToUniversalTime()
            });
            if (response.Exception != null)
            {
                string responseJson = response.Exception.Message.Replace("\r\n", "</br>").Replace("\n", "</br>");
                return Json(new
                {
                    success = false,
                    response = responseJson
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Updates the contact lead source.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="contactleadSource">The contactlead source.</param>
        /// <returns></returns>
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public JsonResult UpdateContactLeadSource(int contactId, short contactleadSource)
        {
            UpdateContactViewResponse response = contactService.UpdateContactLeadSource(new UpdateContactViewRequest()
            {
                ContactId = contactId,
                ContactLeadSourceId = contactleadSource,
                AccountId = this.Identity.ToAccountID(),
                LastUpdatedBy = this.Identity.ToUserID(),
                LastUpdatedOn = DateTime.Now.ToUniversalTime()
            });
            if (response.Exception != null)
            {
                string responseJson = response.Exception.Message.Replace("\r\n", "</br>").Replace("\n", "</br>");
                return Json(new
                {
                    success = false,
                    response = responseJson
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Updates the contact email.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="emailId">The email identifier.</param>
        /// <param name="emailStatusValue">The email status value.</param>
        /// <returns></returns>
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public JsonResult UpdateContactEmail(int contactId, string emailId, EmailStatus? emailStatusValue)
        {
            string message = "Request received for updating an email : ContactId " + contactId + " EmailId: " + emailId + " emailsStatus: " + (emailStatusValue.HasValue ? emailStatusValue.Value.ToString() : "");
            Logger.Current.Informational(message);
            bool isStAdmin = this.Identity.IsSTAdmin();
            List<Email> emails = new List<Email>();
            Email email = new Email()
            {
                EmailId = emailId,
                EmailStatusValue = (EmailStatus)emailStatusValue,
                IsPrimary = true
            };
            emails.Add(email);
            GetPersonResponse response = new GetPersonResponse();
            response.PersonViewModel = contactService.GetPerson(new GetPersonRequest(contactId)
            {
                RequestedBy = this.Identity.ToUserID(),
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID(),
                IncludeLastTouched = true
            }).PersonViewModel;
            response.PersonViewModel.AccountID = this.Identity.ToAccountID();
            response.PersonViewModel.LastUpdatedBy = this.Identity.ToUserID();
            response.PersonViewModel.ContactSource = ContactSource.Manual;
            response.PersonViewModel.SourceType = null;
            response.PersonViewModel.LastUpdatedOn = DateTime.Now.ToUniversalTime();
            response.PersonViewModel.Emails = emails;
            UpdateContactViewResponse updateresponse = contactService.UpdateContactEmail(new UpdateContactViewRequest()
            {
                ContactId = contactId,
                Email = email,
                AccountId = this.Identity.ToAccountID(),
                PersonViewModel = response.PersonViewModel,
                LastUpdatedBy = this.Identity.ToUserID(),
                LastUpdatedOn = DateTime.Now.ToUniversalTime(),
                isStAdmin = isStAdmin
            });
            if (updateresponse.Exception != null)
            {
                string responseJson = updateresponse.Exception.Message.Replace("\r\n", "</br>").Replace("\n", "</br>");
                return Json(new
                {
                    success = false,
                    response = responseJson
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                success = true,
                response = updateresponse
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Updates the company email.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="emailId">The email identifier.</param>
        /// <param name="emailStatusValue">The email status value.</param>
        /// <returns></returns>
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public JsonResult UpdateCompanyEmail(int contactId, string emailId, EmailStatus emailStatusValue)
        {
            List<Email> emails = new List<Email>();
            bool isStAdmin = this.Identity.IsSTAdmin();
            Email email = new Email()
            {
                EmailId = emailId,
                EmailStatusValue = emailStatusValue,
                IsPrimary = true
            };
            emails.Add(email);
            GetCompanyResponse response = new GetCompanyResponse();
            response = contactService.GetCompany(new GetCompanyRequest(contactId)
            {
                RequestedBy = this.Identity.ToUserID(),
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID()
            });
            response.CompanyViewModel.AccountID = this.Identity.ToAccountID();
            response.CompanyViewModel.LastUpdatedBy = this.Identity.ToUserID();
            response.CompanyViewModel.LastUpdatedOn = DateTime.Now.ToUniversalTime();
            response.CompanyViewModel.Emails = emails;
            UpdateContactViewResponse updateresponse = contactService.UpdateContactEmail(new UpdateContactViewRequest()
            {
                ContactId = contactId,
                Email = email,
                AccountId = this.Identity.ToAccountID(),
                CompanyViewModel = response.CompanyViewModel,
                LastUpdatedBy = this.Identity.ToUserID(),
                LastUpdatedOn = DateTime.Now.ToUniversalTime(),
                isStAdmin = isStAdmin
            });
            if (updateresponse.Exception != null)
            {
                string responseJson = updateresponse.Exception.Message.Replace("\r\n", "</br>").Replace("\n", "</br>");
                return Json(new
                {
                    success = false,
                    response = responseJson
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                success = true,
                response = updateresponse
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Updates the contact addresses.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public JsonResult UpdateContactAddresses(int contactId, string address)
        {
            AddressViewModel addressViewModel = JsonConvert.DeserializeObject<AddressViewModel>(address);
            addressViewModel.IsDefault = true;
            UpdateContactViewResponse response = contactService.UpdateContactAddresses(new UpdateContactViewRequest()
            {
                ContactId = contactId,
                Address = addressViewModel,
                AccountId = this.Identity.ToAccountID(),
                LastUpdatedBy = this.Identity.ToUserID(),
                LastUpdatedOn = DateTime.Now.ToUniversalTime()
            });
            if (response.Exception != null)
            {
                string responseJson = response.Exception.Message.Replace("\r\n", "</br>").Replace("\n", "</br>");
                return Json(new
                {
                    success = false,
                    response = responseJson
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                success = true,
                response = response
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Updates the contact image.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="image">The image.</param>
        /// <param name="contactImageUrl">The contact image URL.</param>
        /// <returns></returns>
        [MenuType(MenuCategory.Contacts, MenuCategory.LeftMenuCRM)]
        public JsonResult UpdateContactImage(int contactId, ImageViewModel image, string contactImageUrl)
        {
            ImageViewModel imageviewmodel = new ImageViewModel();
            if (string.IsNullOrEmpty(contactImageUrl))
            {
                SaveImageResponse imageresponse = imageService.SaveImage(new SaveImageRequest()
                {
                    ImageCategory = ImageCategory.ContactProfile,
                    ViewModel = image,
                    AccountId = this.Identity.ToAccountID()
                });
                imageviewmodel = imageresponse.ImageViewModel;
            }
            else
            {
                DownloadImageResponse imageResponse = imageService.DownloadImage(new DownloadImageRequest()
                {
                    ImgCategory = ImageCategory.ContactProfile,
                    ImageInputUrl = contactImageUrl,
                    AccountId = this.Identity.ToAccountID()
                });
                imageviewmodel = imageResponse.ImageViewModel;
            }
            UpdateContactViewResponse response = contactService.UpdateContactImage(new UpdateContactViewRequest()
            {
                ContactId = contactId,
                Image = imageviewmodel,
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID(),
                LastUpdatedBy = this.Identity.ToUserID(),
                LastUpdatedOn = DateTime.Now.ToUniversalTime()
            });
            if (response.Exception != null)
            {
                string responseJson = response.Exception.Message.Replace("\r\n", "</br>").Replace("\n", "</br>");
                return Json(new
                {
                    success = false,
                    response = responseJson
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region Action
        /// <summary>
        /// Inserts the action.
        /// </summary>
        /// <param name="actionViewModel">The action view model.</param>
        /// <returns></returns>
        public JsonResult InsertAction(string actionViewModel)
        {
            ActionViewModel viewModel = JsonConvert.DeserializeObject<ActionViewModel>(actionViewModel);
            Logger.Current.Informational("Action Reminder Date: " + viewModel.RemindOn);
            Logger.Current.Informational("Action selected Reminder types: " + viewModel.SelectedReminderTypes.FirstOrDefault());
            Logger.Current.Informational("Action Reminder methods: " + viewModel.ReminderMethods.FirstOrDefault());
            Logger.Current.Informational("Action created on: " + viewModel.CreatedOn);
            Logger.Current.Informational("Action date format: " + viewModel.DateFormat);
            var accountId = Thread.CurrentPrincipal.Identity.ToAccountID();
            viewModel.CreatedBy = this.Identity.ToUserID();
            viewModel.CreatedOn = DateTime.Now.ToUniversalTime();
            viewModel.AccountId = accountId;
            viewModel.LastUpdatedBy = this.Identity.ToUserID();
            viewModel.LastUpdatedOn = DateTime.Now.ToUniversalTime();
            if (viewModel != null && viewModel.SelectedReminderTypes.IsAny())
            {
                viewModel.RemindOn = viewModel.RemindOn.Value.ToUserUtcDateTimeV2();
            }
            else
                viewModel.RemindOn = null;
            Func<DateTime?, DateTime?> GetUtzUtcDate = source =>
            {
                if (source.HasValue)
                    return source.Value.ToUserUtcDateTimeV2();
                else
                    return null;
            };
            viewModel.ActionDate = GetUtzUtcDate(viewModel.ActionDate);
            viewModel.ActionStartTime = GetUtzUtcDate(viewModel.ActionStartTime);
            viewModel.ActionEndTime = GetUtzUtcDate(viewModel.ActionEndTime);
            viewModel.ToEmail = this.Identity.ToUserEmail();
            string accountPhoneNumber = string.Empty;
            string accountAddress = string.Empty;
            string location = string.Empty;
            if (viewModel.SelectedReminderTypes != null && viewModel.SelectedReminderTypes.Contains(ReminderType.Email))
            {
                accountAddress = accountService.GetPrimaryAddress(new GetAddressRequest()
                {
                    AccountId = accountId
                }).Address;
                location = accountService.GetPrimaryAddress(new GetAddressRequest()
                {
                    AccountId = accountId
                }).Location;
                accountPhoneNumber = accountService.GetPrimaryPhone(new GetPrimaryPhoneRequest()
                {
                    AccountId = accountId
                }).PrimaryPhone;
            }
            var viewModel1 = (AdvancedSearchViewModel)Session["AdvancedSearchVM"];
            var strngdata = Newtonsoft.Json.JsonConvert.SerializeObject(viewModel1);
            var drillDownContactIds = (int[])Session["ContactListIDs"];
            actionService.InsertAction(new InsertActionRequest()
            {
                ActionViewModel = viewModel,
                AccountId = Thread.CurrentPrincipal.Identity.ToAccountID(),
                AccountPrimaryEmail = this.Identity.ToAccountPrimaryEmail(),
                Location = location,
                AccountAddress = accountAddress,
                AccountPhoneNumber = accountPhoneNumber,
                RequestedBy = this.Identity.ToUserID(),
                AccountDomain = Request.Url.Host,
                SelectAllSearchCriteria = Server.UrlDecode(ReadCookie("selectallsearchstring")),
                AdvancedSearchCritieria = strngdata,
                RoleId = this.Identity.ToRoleID(),
                RequestedFrom = RequestOrigin.CRM,
                DrillDownContactIds = drillDownContactIds
            });
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Updates the action.
        /// </summary>
        /// <param name="actionViewModel">The action view model.</param>
        /// <returns></returns>
        public JsonResult UpdateAction(string actionViewModel)
        {
            ActionViewModel viewModel = JsonConvert.DeserializeObject<ActionViewModel>(actionViewModel);
            var accountId = Thread.CurrentPrincipal.Identity.ToAccountID();
            viewModel.CreatedOn = viewModel.CreatedOn.ToUniversalTime();
            viewModel.AccountId = accountId;
            viewModel.LastUpdatedBy = this.Identity.ToUserID();
            viewModel.LastUpdatedOn = DateTime.Now.ToUniversalTime();
            string accountPhoneNumber = string.Empty;
            string accountAddress = string.Empty;
            if (viewModel != null && viewModel.SelectedReminderTypes.IsAny())
            {
                viewModel.RemindOn = viewModel.RemindOn.Value.ToUserUtcDateTimeV2();
            }
            else
                viewModel.RemindOn = null;
            Func<DateTime?, DateTime?> GetUtzUtcDate = source =>
            {
                if (source.HasValue)
                    return source.Value.ToUserUtcDateTimeV2();
                else
                    return null;
            };
            viewModel.ActionDate = GetUtzUtcDate(viewModel.ActionDate);
            viewModel.ActionStartTime = GetUtzUtcDate(viewModel.ActionStartTime);
            viewModel.ActionEndTime = GetUtzUtcDate(viewModel.ActionEndTime);
            if ((viewModel.SelectedReminderTypes != null && viewModel.SelectedReminderTypes.Contains(ReminderType.Email) && !viewModel.EmailRequestGuid.HasValue) || viewModel.SelectedReminderTypes.Contains(ReminderType.Email) && viewModel.EmailRequestGuid.Value != null)
            {
                accountAddress = accountService.GetPrimaryAddress(new GetAddressRequest()
                {
                    AccountId = accountId
                }).Address;
                accountPhoneNumber = accountService.GetPrimaryPhone(new GetPrimaryPhoneRequest()
                {
                    AccountId = accountId
                }).PrimaryPhone;
            }
            actionService.UpdateAction(new UpdateActionRequest()
            {
                ActionViewModel = viewModel,
                AccountId = Thread.CurrentPrincipal.Identity.ToAccountID(),
                AccountPrimaryEmail = this.Identity.ToAccountPrimaryEmail(),
                AccountAddress = accountAddress,
                AccountPhoneNumber = accountPhoneNumber,
                RequestedBy = this.Identity.ToUserID(),
                AccountDomain = Request.Url.Host,
                RequestedFrom = RequestOrigin.CRM
            });
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// _s the add action.
        /// </summary>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.ContactActions, AppOperations.Create)]
        public ActionResult _AddAction()
        {
            ActionViewModel viewModel = new ActionViewModel();
            viewModel.CreatedBy = this.Identity.ToUserID();
            viewModel.OwnerIds = new List<int>();
            viewModel.RemindOn = DateTime.Now.ToUniversalTime().AddDays(1).ToUtc();
            var dateFormat = this.Identity.ToDateFormat();
            List<ReminderType> types = new List<ReminderType>();
            viewModel.SelectedReminderTypes = types;
            viewModel.DateFormat = dateFormat + " hh:mm tt";
            ViewBag.TagPopup = false;
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            viewModel.ActionTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.ActionType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            viewModel.ActionType = viewModel.ActionTypes.Where(s => s.IsDefault).Select(p => p.DropdownValueID).FirstOrDefault();
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, this.Identity.ToAccountID());
            bool isSTAdmin = this.Identity.IsSTAdmin();
            bool isACTAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
            ViewBag.IsACTAdmin = (isACTAdmin || isSTAdmin) ? true : false;
            ViewBag.IsPrivate = isPrivate;
            ViewBag.IsIncludeSignature = userService.IsIncludeSignatureByDefaultOrNot(this.Identity.ToUserID());
            return PartialView("_AddEditAction", viewModel);
        }

        /// <summary>
        /// Bulk Send Email
        /// </summary>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.ContactActions, AppOperations.Create)]
        public ActionResult _BulkSendEmail()
        {
            BulkMailOperationViewModel viewModel = new BulkMailOperationViewModel();
            ViewBag.IsIncludeSignature = userService.IsIncludeSignatureByDefaultOrNot(this.Identity.ToUserID());
            return PartialView("~/Views/Contact/_BulkSendEmail.cshtml", viewModel);
        }

        /// <summary>
        /// _s the change owner.
        /// </summary>
        /// <returns></returns>
        public ActionResult _ChangeOwner()
        {
            return PartialView("_ChangeOwner");
        }

        /// <summary>
        /// _s the add action modal.
        /// </summary>
        /// <returns></returns>
        [Route("addactionmodal")]
        [SmarttouchAuthorize(AppModules.ContactActions, AppOperations.Create)]
        public ActionResult _AddActionModal()
        {
            ActionViewModel viewModel = new ActionViewModel();
            viewModel.CreatedBy = this.Identity.ToUserID();
            ViewBag.IsModal = true;
            viewModel.RemindOn = DateTime.Now.ToUniversalTime().AddDays(1);
            var dateFormat = this.Identity.ToDateFormat();
            viewModel.OwnerIds = new List<int>();
            List<ReminderType> types = new List<ReminderType>();
            viewModel.SelectedReminderTypes = types;
            viewModel.DateFormat = dateFormat + " hh:mm tt";
            ViewBag.TagPopup = false;
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            viewModel.ActionTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.ActionType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            viewModel.ActionType = viewModel.ActionTypes.Where(s => s.IsDefault).Select(p => p.DropdownValueID).FirstOrDefault();
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, this.Identity.ToAccountID());
            bool isSTAdmin = this.Identity.IsSTAdmin();
            bool isACTAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
            ViewBag.IsACTAdmin = (isACTAdmin || isSTAdmin) ? true : false;
            ViewBag.IsPrivate = isPrivate;
            ViewBag.IsIncludeSignature = userService.IsIncludeSignatureByDefaultOrNot(this.Identity.ToUserID());
            return PartialView("_AddEditAction", viewModel);
        }

        /// <summary>
        /// Edits the action.
        /// </summary>
        /// <param name="actionId">The action identifier.</param>
        /// <returns></returns>
        [Route("editaction")]
        [SmarttouchAuthorize(AppModules.ContactActions, AppOperations.Edit)]
        public ActionResult EditAction(int actionId)
        {
            bool hasPermission = false;
            bool isSTAdmin = this.Identity.IsSTAdmin();
            bool isACTAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());

            GetActionResponse response = actionService.GetAction(new GetActionRequest()
            {
                Id = actionId
            });

            if (!isSTAdmin && !isACTAdmin && response.ActionViewModel.OwnerIds.IsAny())
                hasPermission = !response.ActionViewModel.OwnerIds.Contains(this.Identity.ToUserID());

            if (hasPermission)
            {
                ViewBag.Message = " Access denied. You do not have permission to edit this Action.";
                ViewBag.CustomMessage = "Action Update Alert!";
                var partialView = PartialView("_PermissionView");
                return partialView;
            }

            Logger.Current.Informational("while editing Action  ");
            Logger.Current.Informational("Action Reminder Date: " + response.ActionViewModel.RemindOn);
            if (response.ActionViewModel.RemindOn.HasValue)
            {
                response.ActionViewModel.RemindOn = response.ActionViewModel.RemindOn.Value.ToUtc();
            }
            else
                response.ActionViewModel.RemindOn = DateTime.Now.ToUniversalTime();
            if (response.ActionViewModel.ActionDate.HasValue)
            {
                response.ActionViewModel.ActionDate = response.ActionViewModel.ActionDate.Value.ToUtc();
            }
            else
                response.ActionViewModel.ActionDate = DateTime.Now.ToUniversalTime();
            if (response.ActionViewModel.ActionStartTime.HasValue)
            {
                response.ActionViewModel.ActionStartTime = response.ActionViewModel.ActionStartTime.Value.ToUtc();
            }
            else
                response.ActionViewModel.ActionStartTime = DateTime.Now.ToUniversalTime();
            if (response.ActionViewModel.ActionEndTime.HasValue)
            {
                response.ActionViewModel.ActionEndTime = response.ActionViewModel.ActionEndTime.Value.ToUtc();
            }
            else
                response.ActionViewModel.ActionEndTime = DateTime.Now.ToUniversalTime();
            response.ActionViewModel.CreatedOn = response.ActionViewModel.CreatedOn.ToUserUtcDateTimeV2();
            var dateFormat = this.Identity.ToDateFormat();
            response.ActionViewModel.DateFormat = dateFormat + " hh:mm tt";
            response.ActionViewModel.PreviousActionType = response.ActionViewModel.ActionType;

            if (!response.ActionViewModel.OwnerIds.IsAny())
                response.ActionViewModel.OwnerIds = new List<int>();

            ViewBag.IsModal = true;
            ViewBag.EditView = true;
            ViewBag.TagPopup = false;
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, this.Identity.ToAccountID());
            ViewBag.IsACTAdmin = (isACTAdmin || isSTAdmin) ? true : false;
            ViewBag.IsPrivate = isPrivate;
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            response.ActionViewModel.ActionTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.ActionType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            ViewBag.IsIncludeSignature = userService.IsIncludeSignatureByDefaultOrNot(this.Identity.ToUserID());
            return PartialView("_AddEditAction", response.ActionViewModel);
        }

        /// <summary>
        /// Action Completed Status
        /// </summary>
        /// <param name="actionId"></param>
        /// <param name="isActionCompleted"></param>
        /// <param name="contactId"></param>
        /// <param name="completedForAll"></param>
        /// <param name="opportunityId"></param>
        /// <param name="isSchedule"></param>
        /// <param name="mailBulkId"></param>
        /// <returns></returns>
        public ActionResult ActionCompleted(int actionId, bool isActionCompleted, int? contactId, bool completedForAll, int opportunityId, bool isSchedule, int? mailBulkId, bool AddToNoteSummary)
        {
            var response = actionService.ActionStatus(new CompletedActionRequest
            {
                actionId = actionId,
                isCompleted = isActionCompleted,
                contactId = contactId,
                CompletedForAll = completedForAll,
                opportunityId = opportunityId,
                RequestedBy = this.Identity.ToUserID(),
                UpdatedOn = DateTime.Now.ToUniversalTime(),
                AccountId = this.Identity.ToAccountID(),
                IsSchedule = isSchedule,
                MailBulkId = mailBulkId,
                AddToNoteSummary = AddToNoteSummary
            });
            if (response.Exception != null)
            {
                Logger.Current.Warning(response.Exception.ToString());
            }
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Actionses the marked complete.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isShedule"></param>
        /// <returns></returns>
        public ActionResult ActionsMarkedComplete(int[] id, bool isShedule, bool AddToNoteSummary)
        {
            actionService.ActionsMarkedComplete(new CompletedActionsRequest()
            {
                ActionIds = id,
                IsSheduled = isShedule,
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID(),
                UpdatedOn = DateTime.Now.ToUniversalTime(),
                AddToNoteSummary = AddToNoteSummary
            });
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Actionses the marked in complete.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isShedule"></param>
        /// <returns></returns>
        public ActionResult ActionsMarkedInComplete(int[] id, bool isShedule)
        {
            actionService.ActionsMarkedInComplete(new CompletedActionsRequest()
            {
                ActionIds = id,
                IsSheduled = isShedule,
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID(),
                UpdatedOn = DateTime.Now.ToUniversalTime()
            });
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Deletes the action.
        /// </summary>
        /// <param name="actionId">The action identifier.</param>
        /// <param name="deleteForAll">if set to <c>true</c> [delete for all].</param>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.ContactActions, AppOperations.Delete)]
        public ActionResult DeleteAction(int actionId, bool deleteForAll, int? contactId)
        {
            actionService.DeleteAction(new DeleteActionRequest()
            {
                ActionId = actionId,
                DeleteForAll = deleteForAll,
                ContactId = contactId,
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID()
            });
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Actionses the delete.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.ContactActions, AppOperations.Delete)]
        public ActionResult ActionsDelete(int[] id)
        {
            actionService.ActionsDelete(new DeleteActionsRequest()
            {
                ActionIds = id,
                RequestedBy = this.Identity.ToUserID()
            });
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Bulk Send Email to Actions Contacts.
        /// </summary>
        /// <param name="actionIds"></param>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.ContactActions, AppOperations.Delete)]
        public ActionResult BulkSendEmail(int[] actionIds, string subject, string body)
        {
            if (!string.IsNullOrEmpty(body))
            {
                body = ReplacingSpecialCharacterWithTheirCode(body);

            }
            actionService.BulkMailSendForActionContacts(new BulkSendEmailRequest()
            {
                Subject = subject,
                Body = body.Replace("&amp;", "&"),
                ActionIds = actionIds,
                RequestedBy = this.Identity.ToUserID()
            });

            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Deletes the contact opportunity map.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public ActionResult DeleteContactOpportunityMap(int[] id)
        {
            int ContactID = default(int);
            int.TryParse(ReadCookie("contactid"), out ContactID);
            DeleteOpportunityContactRequest request = new DeleteOpportunityContactRequest()
            {
                OpportunityID = id[0],
                ContactID = ContactID
            };
            opportunityService.DeleteOpportunityContact(request);
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the action contacts count.
        /// </summary>
        /// <param name="actionId">The action identifier.</param>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public ActionResult GetActionContactsCount(int actionId, int contactId)
        {
            GetContactsCountResponse response = actionService.ActionContactsCount(new GetContactsCountRequest()
            {
                Id = actionId
            });

            IList<int> OwnerIds = actionService.GetAllAssignedUserIds(actionId);

            bool hasPermission = false;
            bool isSTAdmin = this.Identity.IsSTAdmin();
            bool isACTAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
            if (!isSTAdmin && !isACTAdmin && OwnerIds.IsAny())
                hasPermission = !OwnerIds.Contains(this.Identity.ToUserID());

            if (hasPermission)
            {
                ViewBag.Message = " Access denied. You do not have permission to delete this Action.";
                ViewBag.CustomMessage = "Action Delete Alert!";
                var partialView = PartialView("_PermissionView");
                return partialView;
            }
            ViewBag.Message = string.Empty;
            ViewBag.OkText = string.Empty;
            ViewBag.MultipleContacts = false;
            ViewBag.UserCreatedActions = false;
            ViewBag.ContactId = contactId;
            if (response.Exception != null)
            {
                string responseJson = JsonConvert.SerializeObject(response.Exception.Message.Replace("\r\n", "</br>").Replace("\n", "</br>"));
                return Json(new
                {
                    success = false,
                    response = responseJson
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (response.Count == 1 || response.SelectAll == true)
                {
                    ViewBag.Message = "You’re about to delete this Action. Are you sure you want to delete?";
                    ViewBag.OkText = "Delete Action";
                    ViewBag.ActionId = actionId;
                }
                else
                {
                    ViewBag.Message = "More than one Contact is included for this Action, How do you want to delete this Action ?";
                    ViewBag.MultipleContacts = true;
                    ViewBag.OkText = "Delete Action";
                    ViewBag.ActionId = actionId;
                }
            }
            return PartialView("_ActionConfirmation", new PersonViewModel());
        }

        /// <summary>
        /// Gets the user action contacts count.
        /// </summary>
        /// <param name="actionId">The action identifier.</param>
        /// <returns></returns>
        public ActionResult GetUserActionContactsCount(int actionId)
        {
            GetContactsCountResponse response = actionService.ActionContactsCount(new GetContactsCountRequest()
            {
                Id = actionId
            });
            ViewBag.Message = string.Empty;
            ViewBag.OkText = string.Empty;
            ViewBag.MultipleContacts = true;
            ViewBag.UserCreatedActions = false;
            ViewBag.ContactId = 0;
            if (response.Exception != null)
            {
                string responseJson = JsonConvert.SerializeObject(response.Exception.Message.Replace("\r\n", "</br>").Replace("\n", "</br>"));
                return Json(new
                {
                    success = false,
                    response = responseJson
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                ViewBag.Message = "[|You are about to delete this Action. Are you sure you want to delete|]?";
                ViewBag.OkText = "OK";
                ViewBag.ActionId = actionId;
            }
            return PartialView("_ActionConfirmation", new PersonViewModel());
        }

        /// <summary>
        /// Actions the completed operation.
        /// </summary>
        /// <param name="actionID">The action identifier.</param>
        /// <returns></returns>
        public ActionResult ActionCompletedOperation(int actionID)
        {
            bool hasPermission = true;
            bool isSTAdmin = this.Identity.IsSTAdmin();
            bool isACTAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
            GetContactsCountResponse response = new GetContactsCountResponse();
            GetActionResponse actionResponse = actionService.GetAction(new GetActionRequest()
            {
                Id = actionID
            });

            if (!isSTAdmin && !isACTAdmin && actionResponse.ActionViewModel.OwnerIds.IsAny())
                hasPermission = actionResponse.ActionViewModel.OwnerIds.Contains(this.Identity.ToUserID());

            if (hasPermission)
                response = actionService.ActionContactsCount(new GetContactsCountRequest()
                {
                    Id = actionID
                });

            return Json(new
            {
                success = hasPermission,
                response = response
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the user created actions.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="limit">The limit.</param>
        /// <returns></returns>
        public JsonResult GetUserCreatedActions(int pageNumber, int limit)
        {
            IEnumerable<ActionViewModel> actions = null;
            var direction = System.ComponentModel.ListSortDirection.Descending;
            GetActionListResponse response = actionService.GetUserCreatedActions(new GetActionListRequest()
            {
                RequestedBy = this.Identity.ToUserID(),
                AccountId = this.Identity.ToAccountID(),
                IsStAdmin = this.Identity.IsSTAdmin(),
                PageNumber = pageNumber,
                Limit = limit,
                StartDate = DateTime.MinValue,
                EndDate = DateTime.MinValue,
                Name = null,
                Filter = null,
                FilterByActionType = null,
                SortDirection = direction
            });
            if (response != null)
                actions = response.ActionListViewModel;
            return Json(new
            {
                success = true,
                response = actions
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the created actions.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="name">The name.</param>
        /// <param name="filterdata">The filterdata.</param>
        /// <param name="isDashboard">if set to <c>true</c> [is dashboard].</param>
        /// <param name="userid">The userid.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public ActionResult GetCreatedActions([DataSourceRequest] DataSourceRequest request, string name, string filterdata, bool isDashboard, string userIds, string startDate, string endDate, string type)
        {
            IEnumerable<ActionViewModel> actions = null;
            int[] userIDs = null;
            if (!string.IsNullOrEmpty(userIds))
                userIDs = JsonConvert.DeserializeObject<int[]>(userIds);

            var sortField = request.Sorts.Count > 0 ? request.Sorts.First().Member : GetPropertyName<ActionViewModel, DateTime?>(r => r.ActionDate);
            var direction = request.Sorts.Count > 0 ? request.Sorts.First().SortDirection : System.ComponentModel.ListSortDirection.Descending;
            string userName = string.Empty;
            GetActionListResponse response = actionService.GetUserCreatedActions(new GetActionListRequest()
            {
                UserIds = userIDs.IsAny() ? userIDs : new int[] { this.Identity.ToUserID() },
                AccountId = this.Identity.ToAccountID(),
                IsStAdmin = this.Identity.IsSTAdmin(),
                PageNumber = request.Page,
                Limit = request.PageSize == 0 ? 10 : request.PageSize,
                Name = name,
                Filter = string.IsNullOrEmpty(filterdata) ? "2" : filterdata,
                SortDirection = direction,
                SortField = sortField,
                IsDashboard = isDashboard,
                StartDate = !string.IsNullOrEmpty(startDate) ? Convert.ToDateTime(startDate) : DateTime.MinValue,
                EndDate = !string.IsNullOrEmpty(endDate) ? Convert.ToDateTime(endDate) : DateTime.MinValue,
                FilterByActionType = type
            });
            if (response != null)
            {
                response.ActionListViewModel.ToList().ForEach(a => a.ActionDateTime = a.ActionDateTime.ToUtc());
                response.ActionListViewModel.Each(a =>
                {
                    if (a.UserName != null && a.UserName.Length > 75)
                    {
                        userName = a.UserName.Substring(0, 74);
                        a.UserName = userName + "...";
                    }
                });
            }

            actions = response.ActionListViewModel;
            var jsonResult = Json(new DataSourceResult
            {
                Data = actions,
                Total = (int)response.TotalHits
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        #endregion
        #region Notes
        /// <summary>
        /// Inserts the note.
        /// </summary>
        /// <param name="noteViewModel">The note view model.</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult InsertNote(string noteViewModel)
        {
            NoteViewModel viewModel = JsonConvert.DeserializeObject<NoteViewModel>(noteViewModel);
            viewModel.CreatedBy = this.Identity.ToUserID();
            viewModel.CreatedOn = DateTime.Now.ToUniversalTime();
            viewModel.AccountId = Thread.CurrentPrincipal.Identity.ToAccountID();
            var viewModel1 = (AdvancedSearchViewModel)Session["AdvancedSearchVM"];
            var strngdata = Newtonsoft.Json.JsonConvert.SerializeObject(viewModel1);
            SaveNoteResponse response = new SaveNoteResponse();
            response = noteService.InsertNote(new SaveNoteRequest()
            {
                NoteViewModel = viewModel,
                AccountId = Thread.CurrentPrincipal.Identity.ToAccountID()
            });
            var drillDownContactIds = (int[])Session["ContactListIDs"];
            if (viewModel.SelectAll == true)
            {
                BulkOperations operationData = new BulkOperations()
                {
                    OperationID = response.NoteViewModel.NoteId,
                    OperationType = (int)BulkOperationTypes.Note,
                    SearchCriteria = HttpUtility.HtmlEncode(Server.UrlDecode(ReadCookie("selectallsearchstring"))),
                    AdvancedSearchCriteria = strngdata,
                    SearchDefinitionID = null,
                    AccountID = Thread.CurrentPrincipal.Identity.ToAccountID(),
                    UserID = (int)Thread.CurrentPrincipal.Identity.ToUserID(),
                    RoleID = Thread.CurrentPrincipal.Identity.ToRoleID()
                };
                InsertBulkOperationRequest bulkOperationRequest = new InsertBulkOperationRequest()
                {
                    OperationData = operationData,
                    AccountId = Thread.CurrentPrincipal.Identity.ToAccountID(),
                    RequestedBy = Thread.CurrentPrincipal.Identity.ToUserID(),
                    CreatedOn = DateTime.Now.ToUniversalTime().AddMinutes(1),
                    RoleId = Thread.CurrentPrincipal.Identity.ToRoleID(),
                    DrillDownContactIds = drillDownContactIds
                };
                accountService.InsertBulkOperation(bulkOperationRequest);
            }
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Updates the note.
        /// </summary>
        /// <param name="noteViewModel">The note view model.</param>
        /// <returns></returns>
        public JsonResult UpdateNote(string noteViewModel)
        {
            NoteViewModel viewModel = JsonConvert.DeserializeObject<NoteViewModel>(noteViewModel);
            viewModel.CreatedBy = this.Identity.ToUserID();
            viewModel.CreatedOn = DateTime.Now.ToUniversalTime();
            viewModel.AccountId = Thread.CurrentPrincipal.Identity.ToAccountID();
            noteService.UpdateNote(new UpdateNoteRequest()
            {
                NoteViewModel = viewModel,
                AccountId = Thread.CurrentPrincipal.Identity.ToAccountID()
            });
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// _s the add note.
        /// </summary>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.ContactNotes, AppOperations.Create)]
        public ActionResult _AddNote()
        {
            NoteViewModel nviewmodel = new NoteViewModel();
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            nviewmodel.NoteCategories = GetFromDropdownValues(dropdownValues, (byte)DropdownFieldTypes.NoteCategory);
            nviewmodel.NoteCategory = nviewmodel.NoteCategories.Where(s => s.IsDefault).Select(p => p.DropdownValueID).FirstOrDefault();
            nviewmodel.CreatedBy = this.Identity.ToUserID();
            ViewBag.TagPopup = false;
            ViewBag.EditView = false;
            return PartialView("_AddEditNote", nviewmodel);
        }

        /// <summary>
        /// _s the add note modal.
        /// </summary>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.ContactNotes, AppOperations.Create)]
        public ActionResult _AddNoteModal()
        {
            NoteViewModel viewModel = new NoteViewModel();
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            viewModel.NoteCategories = GetFromDropdownValues(dropdownValues, (byte)DropdownFieldTypes.NoteCategory);
            viewModel.NoteCategory = viewModel.NoteCategories.Where(s => s.IsDefault).Select(p => p.DropdownValueID).FirstOrDefault();
            viewModel.CreatedBy = this.Identity.ToUserID();
            ViewBag.IsModal = true;
            ViewBag.TagPopup = false;
            ViewBag.EditView = false;
            return PartialView("_AddEditNote", viewModel);
        }

        /// <summary>
        /// Edits the note.
        /// </summary>
        /// <param name="noteId">The note identifier.</param>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.ContactNotes, AppOperations.Edit)]
        public ActionResult EditNote(int noteId)
        {
            GetNoteResponse response = noteService.GetNote(new GetNoteRequest()
            {
                NoteId = noteId,
                AccountId = this.Identity.ToAccountID()
            });
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            response.NoteViewModel.NoteCategories = GetFromDropdownValues(dropdownValues, (byte)DropdownFieldTypes.NoteCategory);
            ViewBag.IsModal = true;
            ViewBag.TagPopup = false;
            ViewBag.EditView = true;
            var view = PartialView("_AddEditNote", response.NoteViewModel);
            return view;
        }

        /// <summary>
        /// Deletes the note.
        /// </summary>
        /// <param name="noteId">The note identifier.</param>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.ContactNotes, AppOperations.Delete)]
        public JsonResult DeleteNote(int noteId, int contactId)
        {
            noteService.DeleteNote(new DeleteNoteRequest()
            {
                NoteId = noteId,
                AccountId = this.Identity.ToAccountID(),
                ContactId = contactId
            });
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the note contacts count.
        /// </summary>
        /// <param name="noteId">The note identifier.</param>
        /// <returns></returns>
        public ActionResult GetNoteContactsCount(int noteId)
        {
            GetContactCountsResponse response = noteService.NoteContactsCount(new GetContactCountsRequest()
            {
                NoteId = noteId,
                AccountId = this.Identity.ToAccountID()
            });
            return Json(new
            {
                success = true,
                response = response
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region Tour
        /// <summary>
        /// add tour.
        /// </summary>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.ContactTours, AppOperations.Create)]
        public ActionResult _AddTour()
        {
            TourViewModel tourViewModel = new TourViewModel();
            tourViewModel.CreatedBy = this.Identity.ToUserID();
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            ViewBag.EditView = false;
            tourViewModel.Communities = GetFromDropdownValues(dropdownValues, (byte)DropdownFieldTypes.Community);
            tourViewModel.TourTypes = GetFromDropdownValues(dropdownValues, (byte)DropdownFieldTypes.TourType);
            tourViewModel.currentTime = DateTime.Now.ToUtc();
            tourViewModel.TourDate = DateTime.Now.ToUniversalTime().RoundUp().ToUtc();
            var newDate = tourViewModel.currentTime;
            double minutesToAdd = 15 - (newDate.Minute % 15);
            newDate = newDate.AddMinutes(minutesToAdd);
            tourViewModel.TourDetails = "";
            tourViewModel.TourType = 0;
            tourViewModel.ReminderTypes = new List<dynamic>();
            tourViewModel.ReminderDate = newDate;
            tourViewModel.OwnerIds = new List<int>();
            string DateFormat = this.Identity.ToDateFormat();
            tourViewModel.DateFormat = DateFormat + " hh:mm tt";
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, this.Identity.ToAccountID());
            bool isSTAdmin = this.Identity.IsSTAdmin();
            bool isACTAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
            ViewBag.IsACTAdmin = isACTAdmin;
            ViewBag.ContactMode = isPrivate;
            ViewBag.IsSTAdmin = isSTAdmin;
            return PartialView("_AddEditTour", tourViewModel);
        }

        /// <summary>
        /// add tour modal.
        /// </summary>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.ContactTours, AppOperations.Create)]
        public ActionResult _AddTourModal()
        {
            ViewBag.IsModal = true;
            TourViewModel tourViewModel = new TourViewModel();
            tourViewModel.CreatedBy = this.Identity.ToUserID();
            ViewBag.EditView = false;
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            tourViewModel.Communities = GetFromDropdownValues(dropdownValues, (byte)DropdownFieldTypes.Community);
            tourViewModel.TourTypes = GetFromDropdownValues(dropdownValues, (byte)DropdownFieldTypes.TourType);
            tourViewModel.currentTime = DateTime.Now.ToUniversalTime();
            tourViewModel.TourDate = DateTime.Now.ToUniversalTime().RoundUp().ToUtc();
            tourViewModel.CommunityID = 0;
            tourViewModel.TourDetails = "";
            tourViewModel.TourType = 0;
            tourViewModel.ReminderTypes = new List<dynamic>();
            tourViewModel.ReminderDate = null;
            tourViewModel.DateFormat = this.Identity.ToDateFormat() + " hh:mm tt";
            tourViewModel.OwnerIds = new List<int>();
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, this.Identity.ToAccountID());
            bool isSTAdmin = this.Identity.IsSTAdmin();
            bool isACTAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
            ViewBag.IsACTAdmin = isACTAdmin;
            ViewBag.ContactMode = isPrivate;
            ViewBag.IsSTAdmin = isSTAdmin;
            return PartialView("_AddEditTour", tourViewModel);
        }

        /// <summary>
        /// Edits the tour.
        /// </summary>
        /// <param name="tourId">The tour identifier.</param>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.ContactTours, AppOperations.Edit)]
        public ActionResult EditTour(int tourId)
        {
            bool hasPermission = false;
            bool isSTAdmin = this.Identity.IsSTAdmin();
            bool isACTAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
            ViewBag.IsACTAdmin = isACTAdmin;
            TourViewModel tourViewModel = tourService.GetTour(tourId).TourViewModel;

            if (!isSTAdmin && !isACTAdmin && tourViewModel.OwnerIds.IsAny())
                hasPermission = !tourViewModel.OwnerIds.Contains(this.Identity.ToUserID());

            if (hasPermission)
            {
                ViewBag.CustomMessage = "Tour Update Alert!";
                ViewBag.Message = " Access denied. You do not have permission to edit this Tour.";
                var partialView = PartialView("_PermissionView");
                return partialView;
            }

            ViewBag.IsModal = true;
            ViewBag.EditView = true;
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, this.Identity.ToAccountID());
            ViewBag.ContactMode = isPrivate;
            ViewBag.IsSTAdmin = isSTAdmin;
            Logger.Current.Informational("while Editing Tour ");
            Logger.Current.Informational("Tour Reminder Date: " + tourViewModel.ReminderDate);
            Logger.Current.Informational("Tour date : " + tourViewModel.TourDate);
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            tourViewModel.Communities = GetFromDropdownValues(dropdownValues, (byte)DropdownFieldTypes.Community);
            tourViewModel.TourTypes = GetFromDropdownValues(dropdownValues, (byte)DropdownFieldTypes.TourType);
            tourViewModel.CreatedOn = tourViewModel.CreatedOn.ToUserUtcDateTimeV2();
            tourViewModel.CreatedBy = tourViewModel.CreatedBy;
            if (!tourViewModel.OwnerIds.IsAny())
                tourViewModel.OwnerIds = new List<int>();

            tourViewModel.DateFormat = this.Identity.ToDateFormat() + " hh:mm tt";
            if (tourViewModel.ReminderDate.HasValue)
            {
                if (tourViewModel.ReminderDate.Value == DateTime.MinValue)
                    tourViewModel.ReminderDate = null;
                else
                    tourViewModel.ReminderDate = tourViewModel.ReminderDate.Value.ToUtc();
            }
            tourViewModel.TourDate = tourViewModel.TourDate.ToUtc();
            tourViewModel.currentTime = DateTime.Now.ToUniversalTime();
            ;
            var view = PartialView("_AddEditTour", tourViewModel);
            return view;
        }

        /// <summary>
        /// Inserts the tour.
        /// </summary>
        /// <param name="tourViewModel">The tour view model.</param>
        /// <returns></returns>
        public ActionResult InsertTour(string tourViewModel)
        {
            TourViewModel viewModel = JsonConvert.DeserializeObject<TourViewModel>(tourViewModel);
            Logger.Current.Informational("Tour date : viewModel.ToUserUtcDateTime" + viewModel.TourDate.ToUserUtcDateTimeV2());
            Logger.Current.Informational("Tour date :viewModel.TourDate.ToLocalTime()" + viewModel.TourDate.ToLocalTime().ToUtc());
            var accountId = Thread.CurrentPrincipal.Identity.ToAccountID();
            viewModel.CreatedBy = this.Identity.ToUserID();
            viewModel.CreatedOn = DateTime.Now.ToUniversalTime();
            viewModel.LastUpdatedBy = this.Identity.ToUserID();
            viewModel.LastUpdatedOn = DateTime.Now.ToUniversalTime();
            viewModel.AccountId = accountId;
            if (viewModel.ReminderDate != null)
            {
                viewModel.ReminderDate = viewModel.ReminderDate.Value.ToUserUtcDateTimeV2();
            }
            viewModel.TourDate = viewModel.TourDate.ToUserUtcDateTimeV2();
            viewModel.ToEmail = this.Identity.ToUserEmail();
            string accountPhoneNumber = string.Empty;
            string accountAddress = string.Empty;
            string location = string.Empty;
            if (viewModel.SelectedReminderTypes != null && viewModel.SelectedReminderTypes.Contains(ReminderType.Email))
            {
                accountAddress = accountService.GetPrimaryAddress(new GetAddressRequest()
                {
                    AccountId = accountId
                }).Address;
                location = accountService.GetPrimaryAddress(new GetAddressRequest()
                {
                    AccountId = accountId
                }).Location;
                accountPhoneNumber = accountService.GetPrimaryPhone(new GetPrimaryPhoneRequest()
                {
                    AccountId = accountId
                }).PrimaryPhone;
            }
            var viewModel1 = (AdvancedSearchViewModel)Session["AdvancedSearchVM"];
            var strngdata = Newtonsoft.Json.JsonConvert.SerializeObject(viewModel1);
            InsertTourRequest request = new InsertTourRequest()
            {
                TourViewModel = viewModel,
                AccountId = this.Identity.ToAccountID(),
                AccountPrimaryEmail = this.Identity.ToAccountPrimaryEmail(),
                AccountAddress = accountAddress,
                Location = location,
                AccountPhoneNumber = accountPhoneNumber,
                AccountDomain = Request.Url.Host,
                RequestedBy = this.Identity.ToUserID(),
                RequestedFrom = RequestOrigin.CRM,
                RoleId = this.Identity.ToRoleID()
            };
            InsertTourResponse response = tourService.InsertTour(request);
            var drillDownContactIds = (int[])Session["ContactListIDs"];
            if (viewModel.SelectAll == true)
            {
                BulkOperations operationData = new BulkOperations()
                {
                    OperationID = response.TourViewModel.TourID,
                    OperationType = (int)BulkOperationTypes.Tour,
                    SearchCriteria = HttpUtility.HtmlEncode(Server.UrlDecode(ReadCookie("selectallsearchstring"))),
                    AdvancedSearchCriteria = strngdata,
                    SearchDefinitionID = null,
                    AccountID = request.AccountId,
                    UserID = (int)request.RequestedBy,
                    RoleID = request.RoleId,
                    AccountPrimaryEmail = request.AccountPrimaryEmail,
                    AccountDomain = request.AccountDomain
                };
                InsertBulkOperationRequest bulkOperationRequest = new InsertBulkOperationRequest()
                {
                    OperationData = operationData,
                    AccountId = request.AccountId,
                    RequestedBy = request.RequestedBy,
                    CreatedOn = DateTime.Now.ToUniversalTime().AddMinutes(1),
                    RoleId = request.RoleId,
                    DrillDownContactIds = drillDownContactIds
                };
                accountService.InsertBulkOperation(bulkOperationRequest);
            }

            if (viewModel.IsCompleted == true && viewModel.AddToContactSummary == true)
                InsertTourDetailsToContactSummary(viewModel, strngdata, drillDownContactIds);

            return Json(new
            {
                success = true,
                response = response.TourViewModel
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Updates the tour.
        /// </summary>
        /// <param name="tourViewModel">The tour view model.</param>
        /// <returns></returns>
        public JsonResult UpdateTour(string tourViewModel)
        {
            TourViewModel viewModel = JsonConvert.DeserializeObject<TourViewModel>(tourViewModel);
            var accountId = Thread.CurrentPrincipal.Identity.ToAccountID();
            viewModel.LastUpdatedBy = this.Identity.ToUserID();
            viewModel.LastUpdatedOn = DateTime.Now.ToUniversalTime();
            viewModel.CreatedOn = viewModel.CreatedOn.ToUniversalTime();
            viewModel.TourDate = viewModel.TourDate.ToUserUtcDateTimeV2();
            viewModel.AccountId = accountId;
            string accountPhoneNumber = string.Empty;
            string accountAddress = string.Empty;
            if (viewModel.ReminderDate != null)
            {
                viewModel.ReminderDate = viewModel.ReminderDate.Value.ToUserUtcDateTimeV2();
            }
            if (viewModel.SelectedReminderTypes != null && viewModel.SelectedReminderTypes.Contains(ReminderType.Email))
            {
                accountAddress = accountService.GetPrimaryAddress(new GetAddressRequest()
                {
                    AccountId = accountId
                }).Address;
                accountPhoneNumber = accountService.GetPrimaryPhone(new GetPrimaryPhoneRequest()
                {
                    AccountId = accountId
                }).PrimaryPhone;
            }
            UpdateTourRequest request = new UpdateTourRequest()
            {
                TourViewModel = viewModel,
                AccountId = accountId,
                AccountPrimaryEmail = this.Identity.ToAccountPrimaryEmail(),
                AccountAddress = accountAddress,
                AccountPhoneNumber = accountPhoneNumber,
                AccountDomain = Request.Url.Host,
                RequestedBy = this.Identity.ToUserID()
            };
            UpdateTourResponse response = tourService.UpdateTour(request);
            return Json(new
            {
                success = true,
                response = response.TourViewModel
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Deletes the tour.
        /// </summary>
        /// <param name="tourId">The tour identifier.</param>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.ContactTours, AppOperations.Delete)]
        public JsonResult DeleteTour(int tourId, int contactId)
        {
            tourService.DeleteTour(tourId, this.Identity.ToUserID(), contactId);
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the tour contacts count.
        /// </summary>
        /// <param name="tourId">The tour identifier.</param>
        /// <returns></returns>
        public ActionResult GetTourContactsCount(int tourId)
        {
            bool hasPermission = true;
            bool isSTAdmin = this.Identity.IsSTAdmin();
            bool isACTAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
            GetTourContactsCountResponse response = new GetTourContactsCountResponse();
            TourViewModel tourViewModel = tourService.GetTour(tourId).TourViewModel;

            if (!isSTAdmin && !isACTAdmin && tourViewModel.OwnerIds.IsAny())
                hasPermission = tourViewModel.OwnerIds.Contains(this.Identity.ToUserID());

            if (hasPermission)
                response = tourService.TourContactsCount(tourId);

            return Json(new
            {
                success = hasPermission,
                response = response
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets from dropdown values.
        /// </summary>
        /// <param name="dropdownValues">The dropdown values.</param>
        /// <param name="dropdownId">The dropdown identifier.</param>
        /// <returns></returns>
        private static IEnumerable<dynamic> GetFromDropdownValues(IEnumerable<DropdownViewModel> dropdownValues, byte dropdownId)
        {
            return dropdownValues.Where(s => s.DropdownID == dropdownId).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
        }

        /// <summary>
        /// Gets the contact community exist.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public JsonResult GetContactCommunityExist(int contactId)
        {
            var tours = tourService.IsTourCreate(contactId);
            return Json(new
            {
                success = true,
                response = tours
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the tours.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public JsonResult GetTours(int contactId, int pageNumber = 1, int pageSize = 10)
        {
            IEnumerable<TourViewModel> tours = null;
            GetTourListResponse response = tourService.GetTourList(new GetTourListRequest()
            {
                Id = contactId,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
            var dropdowns = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            var communities = dropdowns.Where(s => s.DropdownID == (byte)DropdownFieldTypes.Community).Select(s => s.DropdownValuesList).ToList().FirstOrDefault();
            if (response != null)
            {
                tours = response.ToursListViewModel.Reverse();
                foreach (var tour in tours)
                {
                    tour.Community = communities.Where(e => e.DropdownValueID == tour.CommunityID).Select(s => s.DropdownValue).FirstOrDefault();
                    tour.TourDate = tour.TourDate.ToJSDate();
                }
            }
            return Json(new
            {
                success = true,
                response = tours
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Tours the completed operation.
        /// </summary>
        /// <param name="tourID">The tour identifier.</param>
        /// <returns></returns>
        public ActionResult TourCompletedOperation(int tourID)
        {
            bool hasPermission = true;
            bool isSTAdmin = this.Identity.IsSTAdmin();
            bool isACTAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
            GetTourContactsCountResponse response = new GetTourContactsCountResponse();
            TourViewModel tourViewModel = tourService.GetTour(tourID).TourViewModel;

            if (!isSTAdmin && !isACTAdmin && tourViewModel.OwnerIds.IsAny())
                hasPermission = tourViewModel.OwnerIds.Contains(this.Identity.ToUserID());

            if (hasPermission)
                response = tourService.TourContactsCount(tourID);

            return Json(new
            {
                success = hasPermission,
                response = response
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Tours the completed.
        /// </summary>
        /// <param name="tourID">The tour identifier.</param>
        /// <param name="isTourCompleted">if set to <c>true</c> [is tour completed].</param>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="completedForAll">if set to <c>true</c> [completed for all].</param>
        /// <returns></returns>
        public ActionResult TourCompleted(int tourID, bool isTourCompleted, int? contactId, bool completedForAll, bool addToContactSummary)
        {
            tourService.TourStatus(new CompletedTourRequest()
            {
                tourId = tourID,
                isCompleted = isTourCompleted,
                contactId = contactId,
                CompletedForAll = completedForAll,
                RequestedBy = this.Identity.ToUserID(),
                UpdatedOn = DateTime.Now.ToUniversalTime(),
                AddToContactSummary = addToContactSummary,
                AccountId = this.Identity.ToAccountID()
            });
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To the user UTC date time.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <returns></returns>
        private static DateTime ToUserUtcDateTime(DateTime d)
        {
            return d.ToJsonSerailizedDate().ToUserUtcDateTime();
        }

        #endregion
        #region Relationship
        /// <summary>
        /// Adds the edit relation.
        /// </summary>
        /// <returns></returns>
        public ActionResult AddEditRelation()
        {
            RelationshipViewModel relationview = new RelationshipViewModel();
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            relationview.RelationshipTypes = dropdownValues.Where(s => s.DropdownID == (short)DropdownFieldTypes.RelationshipType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive);
            relationview.Id = 0;
            ViewBag.Mode = "AddView";
            return PartialView(relationview);
        }

        /// <summary>
        /// Adds the edit relation modal.
        /// </summary>
        /// <returns></returns>
        public ActionResult AddEditRelationModal()
        {
            RelationshipViewModel relationview = new RelationshipViewModel();
            relationview.RelationshipTypes = dropdownValuesService.GetDropdownValue(new GetDropdownValueRequest()
            {
                AccountId = this.Identity.ToAccountID(),
                DropdownID = 9
            }).DropdownValues.DropdownValuesList;
            ViewBag.IsModal = true;
            return PartialView("AddEditRelation", relationview);
        }

        /// <summary>
        /// Deletes the relation.
        /// </summary>
        /// <param name="relationId">The relation identifier.</param>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.ContactRelationships, AppOperations.Delete)]
        public ActionResult DeleteRelation(int relationId)
        {
            contactService.DeleteRelationship(new DeleteRelationRequest()
            {
                ContactRelationshipMapID = relationId
            });
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Edits the relation.
        /// </summary>
        /// <param name="contactRelationshipMapID">The contact relationship map identifier.</param>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.ContactRelationships, AppOperations.Edit)]
        public ActionResult EditRelation(int contactRelationshipMapID)
        {
            GetRelationshipResponse response = new GetRelationshipResponse();
            response = contactRelationshipService.GetContactRelationship(contactRelationshipMapID, this.Identity.ToAccountID());
            ViewBag.IsModal = true;
            ViewBag.Mode = "EditView";
            response.RelationshipViewModel.Id = response.RelationshipViewModel.Relationshipentry[0].ContactId;
            response.RelationshipViewModel.Name = response.RelationshipViewModel.Relationshipentry[0].ContactName;
            response.RelationshipViewModel.RelationshipType = response.RelationshipViewModel.Relationshipentry[0].RelationshipType;
            response.RelationshipViewModel.RelationshipTypeName = response.RelationshipViewModel.Relationshipentry[0].RelationshipTypeName;
            response.RelationshipViewModel.RelatedContact = response.RelationshipViewModel.Relationshipentry[0].RelatedContact;
            response.RelationshipViewModel.RelatedContactID = response.RelationshipViewModel.Relationshipentry[0].RelatedContactID;
            return PartialView("AddEditRelation", response.RelationshipViewModel);
        }

        /// <summary>
        /// Saves the relation.
        /// </summary>
        /// <param name="relationViewModel">The relation view model.</param>
        /// <returns></returns>
        [HttpPost]
        [MenuType(MenuCategory.ContactAction, MenuCategory.LeftMenuCRM)]
        public JsonResult SaveRelation(string relationViewModel)
        {
            RelationshipViewModel viewModel = JsonConvert.DeserializeObject<RelationshipViewModel>(relationViewModel);
            SaveRelationshipResponse response = new SaveRelationshipResponse();
            var selectedAll = Server.UrlDecode(ReadCookie("selectallsearchstring"));
            var viewModel1 = (AdvancedSearchViewModel)Session["AdvancedSearchVM"];
            var stringifiedModel = Newtonsoft.Json.JsonConvert.SerializeObject(viewModel1);
            var drillDownContactIds = (int[])Session["ContactListIDs"];
            SaveRelationshipRequest request = new SaveRelationshipRequest()
            {
                RelationshipViewModel = viewModel,
                UserId = this.Identity.ToUserID(),
                SelectAllSearchCriteria = selectedAll,
                AdvancedSearchCritieria = stringifiedModel,
                RequestedBy = (int)Thread.CurrentPrincipal.Identity.ToUserID(),
                RoleId = Thread.CurrentPrincipal.Identity.ToRoleID(),
                AccountId = Thread.CurrentPrincipal.Identity.ToAccountID(),
                DrillDownContactIds = drillDownContactIds
            };
            if (viewModel != null && viewModel.Relationshipentry.IsAny())
                response = contactRelationshipService.SaveRelationshipMap(request);
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the contacts.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <returns></returns>
        public JsonResult GetContacts(string name, string typeId)
        {
            AutoCompleteResponse response = contactService.SearchContactFullNameforRelation(new AutoCompleteSearchRequest()
            {
                Query = name,
                AccountId = Thread.CurrentPrincipal.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID()
            });
            return Json(new
            {
                success = true,
                response = response
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the users.
        /// </summary>
        /// <returns></returns>
        public JsonResult GetUsers()
        {
            int accountID = UserExtensions.ToAccountID(this.Identity);
            GetRequestUserResponse response = userService.GetUsersList(new GetUserListRequest()
            {
                AccountID = accountID,
                IsSTAdmin = this.Identity.IsSTAdmin()
            });
            return Json(new
            {
                success = true,
                response = response.Users
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region Tags
        /// <summary>
        /// Adds the tag.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="tagId">The tag identifier.</param>
        /// <returns></returns>
        public JsonResult AddTag(string tagName, int contactId, int tagId)
        {
            TagViewModel tagViewModel = new TagViewModel()
            {
                TagName = tagName,
                ContactId = contactId,
                TagID = tagId
            };
            tagViewModel.AccountID = Thread.CurrentPrincipal.Identity.ToAccountID();
            tagViewModel.CreatedBy = this.Identity.ToUserID();
            SaveTagResponse response = tagService.SaveTag(new SaveTagRequest()
            {
                TagViewModel = tagViewModel
            });
            return Json(new
            {
                success = true,
                response = response.TagViewModel
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Removes the tag.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="tagId">The tag identifier.</param>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public JsonResult RemoveTag(string tagName, int tagId, int contactId)
        {
            ITagViewModel tagViewModel = new TagViewModel()
            {
                TagName = tagName,
                ContactId = contactId
            };
            tagViewModel.AccountID = Thread.CurrentPrincipal.Identity.ToAccountID();
            tagService.DeleteTag(new DeleteTagRequest()
            {
                TagName = tagName,
                TagId = tagId,
                ContactID = contactId,
                AccountId = Thread.CurrentPrincipal.Identity.ToAccountID()
            });
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// _s the add tag.
        /// </summary>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.Tags, AppOperations.Create)]
        public ActionResult _AddTag()
        {
            ViewBag.IsContactTag = true;
            AddTagViewModel addTagViewModel = new AddTagViewModel();
            ViewBag.TagPopup = false;
            return PartialView("_AddTag", addTagViewModel);
        }

        /// <summary>
        /// Saves the contact tags.
        /// </summary>
        /// <param name="addTagViewModel">The add tag view model.</param>
        /// <returns></returns>
        public JsonResult SaveContactTags(string addTagViewModel)
        {
            AddTagViewModel viewModel = JsonConvert.DeserializeObject<AddTagViewModel>(addTagViewModel);
            SaveContactTagsResponse response = new SaveContactTagsResponse();
            IList<ContactEntry> contacts = new List<ContactEntry>();
            if (viewModel.SelectAll == true)
            {
                viewModel.Contacts = contacts.ToArray();
            }
            response = tagService.SaveContactTags(new SaveContactTagsRequest()
            {
                Contacts = viewModel.Contacts,
                Tags = viewModel.TagsList,
                Opportunities = viewModel.Opportunities,
                UserId = this.Identity.ToUserID(),
                AccountId = this.Identity.ToAccountID()
            });
            var viewModel1 = (AdvancedSearchViewModel)Session["AdvancedSearchVM"];
            var strngdata = Newtonsoft.Json.JsonConvert.SerializeObject(viewModel1);
            var drillDownContactIds = (int[])Session["ContactListIDs"];
            foreach (int tagid in response.TagIds)
            {
                if (viewModel.SelectAll == true && !viewModel.Opportunities.IsAny())
                {
                    BulkOperations operationData = new BulkOperations()
                    {
                        OperationID = tagid,
                        OperationType = (int)BulkOperationTypes.Tag,
                        SearchCriteria = HttpUtility.HtmlEncode(Server.UrlDecode(ReadCookie("selectallsearchstring"))),
                        AdvancedSearchCriteria = strngdata,
                        SearchDefinitionID = null,
                        AccountID = this.Identity.ToAccountID(),
                        UserID = (int)this.Identity.ToUserID(),
                        RoleID = this.Identity.ToRoleID()
                    };
                    InsertBulkOperationRequest bulkOperationRequest = new InsertBulkOperationRequest()
                    {
                        OperationData = operationData,
                        AccountId = this.Identity.ToAccountID(),
                        RequestedBy = (int)this.Identity.ToUserID(),
                        CreatedOn = DateTime.Now.ToUniversalTime(),
                        RoleId = this.Identity.ToRoleID(),
                        DrillDownContactIds = drillDownContactIds
                    };
                    accountService.InsertBulkOperation(bulkOperationRequest);
                }
            }
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region Geo
        /// <summary>
        /// Gets the countries.
        /// </summary>
        /// <returns></returns>
        public JsonResult GetCountries()
        {
            GetCountriesResponse response = geoService.GetCountries(new GetCountriesRequest()
            {

            });
            return Json(new
            {
                success = true,
                response = response.Countries
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the users list.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        /// <returns></returns>
        [Route("getusers")]
        [Route("GetUsersList")]
        public ActionResult GetUsersList(int Id)
        {
            GetUsersResponse response = contactService.GetUsers(new GetUsersRequest()
            {
                AccountID = this.Identity.ToAccountID(),
                UserId = Id,
                IsSTadmin = this.Identity.IsSTAdmin()
            });
            return Json(new
            {
                success = true,
                response = response.Owner
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the active not deleted users list.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        /// <returns></returns>
        [Route("getactiveusers")]
        public ActionResult GetActiveNotDeletedUsersList(int Id)
        {
            GetUsersResponse response = contactService.GetUsers(new GetUsersRequest()
            {
                AccountID = this.Identity.ToAccountID(),
                UserId = Id,
                IsSTadmin = this.Identity.IsSTAdmin()
            });
            return Json(new
            {
                success = true,
                response = response.Owner
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the states.
        /// </summary>
        /// <param name="countryCode">The country code.</param>
        /// <returns></returns>
        public JsonResult GetStates(string countryCode)
        {
            GetStatesResponse response = geoService.GetStates(new GetStatesRequest(countryCode));
            return Json(new
            {
                success = true,
                response = response.States
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region Attachments
        /// <summary>
        /// Submits the specified files.
        /// </summary>
        /// <param name="files">The files.</param>
        /// <returns></returns>
        [SmarttouchSessionStateBehaviour(System.Web.SessionState.SessionStateBehavior.Required)]
        public ActionResult Submit(IEnumerable<HttpPostedFileBase> files)
        {
            if (files != null)
            {
                TempData["UploadedFiles"] = GetFileInfo(files);
            }
            return RedirectToAction("PersonDetails", new
            {
                contactid = Convert.ToInt32(ReadCookie("contactid"))
            });
        }

        /// <summary>
        /// Gets the file information.
        /// </summary>
        /// <param name="files">The files.</param>
        /// <returns></returns>
        private static IEnumerable<string> GetFileInfo(IEnumerable<HttpPostedFileBase> files)
        {
            return from a in files
                   where a != null
                   select string.Format("{0} ({1} bytes)", Path.GetFileName(a.FileName), a.ContentLength);
        }

        #endregion
        #region Timeline
        /// <summary>
        /// Times the line data.
        /// </summary>
        /// <param name="timelineViewModel">The timeline view model.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> TimeLineData(TimeLineViewModel timelineViewModel)
        {
            GetTimeLineResponse response = new GetTimeLineResponse();
            var timezone = this.Identity.ToTimeZone();
            response = await contactService.GetTimeLinesDataAsync(new GetTimeLineRequest()
            {
                AccountId = this.Identity.ToAccountID(),
                ContactID = timelineViewModel.ContactID,
                OpportunityID = timelineViewModel.OpportunityID,
                TimeZone = timezone,
                Limit = 20,
                PageNumber = timelineViewModel.PageNumber,
                Module = ReadCookie("module"),
                Period = ReadCookie("period"),
                PageName = timelineViewModel.PageName,
                Activities = timelineViewModel.Activities,
                DateFormat = this.Identity.ToDateFormat(),
                FromDate = timelineViewModel.CustomStartDate,
                ToDate = timelineViewModel.CustomEndDate
            });
            return Json(new
            {
                success = true,
                response = response
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region Mails & Text
        /// <summary>
        /// send mail.
        /// </summary>
        /// <returns></returns>
        [SmarttouchSessionStateBehaviour(SessionStateBehavior.Required)]
        [SmarttouchAuthorize(AppModules.SendMail, AppOperations.Create)]
        public ActionResult _SendMail()
        {
            SendMailViewModel viewmodel = new SendMailViewModel();
            GetAccountDropboxKeyResponse accountResponse = accountService.GetAccountDropboxKey(new GetAccountIdRequest()
            {
                accountId = this.Identity.ToAccountID()
            });
            if (!string.IsNullOrEmpty(accountResponse.DropboxKey))
                Session["dropboxkey"] = accountResponse.DropboxKey;
            else
                Session["dropboxkey"] = null;
            viewmodel.ServiceProvider = communicationService.GetServiceProviders(this.Identity.ToAccountID()).Value;
            viewmodel.SenderName = this.Identity.ToFirstName() + " " + this.Identity.ToLastName();
            ViewBag.ServiceProvider = viewmodel.ServiceProvider;
            ViewBag.UserFullName = this.Identity.ToFirstName() + " " + this.Identity.ToLastName();
            ViewBag.IsModel = false;
            ViewBag.IsIncludeSignature = userService.IsIncludeSignatureByDefaultOrNot(this.Identity.ToUserID());
            ViewBag.page = "SendEmail";
            return PartialView("_SendMail", viewmodel);
        }

        [SmarttouchSessionStateBehaviour(SessionStateBehavior.Required)]
        [SmarttouchAuthorize(AppModules.SendMail, AppOperations.Create)]
        [ValidateInput(false)]
        public ActionResult _SendMailModel(string contactName, string email)
        {
            SendMailViewModel viewmodel = new SendMailViewModel();
            ViewBag.page = "SendEmail";
            GetAccountDropboxKeyResponse accountResponse = accountService.GetAccountDropboxKey(new GetAccountIdRequest()
            {
                accountId = this.Identity.ToAccountID()
            });
            if (!string.IsNullOrEmpty(accountResponse.DropboxKey))
                Session["dropboxkey"] = accountResponse.DropboxKey;
            else
                Session["dropboxkey"] = null;
            ViewBag.IsModel = true;
            if (email != null)
            {
                ViewBag.IsModal = true;
                IList<dynamic> contactlist = new List<dynamic>();
                Suggestion suggestion = new Suggestion();
                suggestion.Text = contactName;
                suggestion.DocumentId = Int32.Parse(email);
                contactlist.Add(suggestion);
                viewmodel.Contacts = contactlist;
            }
            ViewBag.UserFullName = this.Identity.ToFirstName() + " " + this.Identity.ToLastName();
            ViewBag.IsIncludeSignature = userService.IsIncludeSignatureByDefaultOrNot(this.Identity.ToUserID());

            return PartialView("_SendMail", viewmodel);
        }

        [SmarttouchAuthorize(AppModules.SendMail, AppOperations.Create)]
        [ValidateInput(false)]
        public ActionResult _ContactSendMailModel(string contactName, string emailID, int contactID)
        {
            SendMailViewModel viewmodel = new SendMailViewModel();
            ViewBag.page = "SendEmail";
            ViewBag.IsModel = true;
            if (emailID != null)
            {
                ViewBag.IsModal = true;
                GetContactEmailIdRequest request = new GetContactEmailIdRequest();
                request.emailID = emailID;
                request.contactID = contactID;
                GetContactEmailIdResponse response = contactService.GetEmailID(request);
                IList<dynamic> contactlist = new List<dynamic>();
                Suggestion suggestion = new Suggestion();
                suggestion.Text = contactName + " " + "<" + emailID + ">" + " *";
                suggestion.DocumentId = response.ContactEmailID;
                contactlist.Add(suggestion);
                viewmodel.Contacts = contactlist;
            }

            ViewBag.UserFullName = this.Identity.ToFirstName() + " " + this.Identity.ToLastName();
            ViewBag.IsIncludeSignature = userService.IsIncludeSignatureByDefaultOrNot(this.Identity.ToUserID());

            return PartialView("_SendMail", viewmodel);
        }

        /// <summary>
        /// send text.
        /// </summary>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.SendText, AppOperations.Create)]
        public ActionResult _SendText()
        {
            SendTextViewModel viewmodel = new SendTextViewModel();
            SendTextRequest request = new SendTextRequest
            {
                UserId = this.Identity.ToUserID(),
                AccountId = this.Identity.ToAccountID()
            };
            viewmodel = communicationService.GetSendTextviewModel(request).SendTextViewModel;
            return PartialView("_SendText", viewmodel);
        }

        /// <summary>
        /// send campaign.
        /// </summary>
        /// <returns></returns>
        public ActionResult _SendCampaign()
        {
            int accountId = this.Identity.ToAccountID();
            GetCampaignTemplatesRequest request = new GetCampaignTemplatesRequest()
            {
                AccountId = accountId
            };
            GetCampaignTemplatesResponse response = campaignService.GetCampaignTemplates(request);
            SendCampaignViewModel campaignViewModel = new SendCampaignViewModel();
            campaignViewModel.CampaignTemplates = response.Templates;
            return PartialView("_SendCampaign", campaignViewModel);
        }

        #endregion
        #region ExportPerson
        /// <summary>
        /// export person.
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult _ExportPerson()
        {
            ExportPersonViewModel viewModel = new ExportPersonViewModel();
            IEnumerable<FieldViewModel> searchFields = null;

            GetAdvanceSearchFieldsResponse response = advancedSearchService.GetSearchFields(new GetAdvanceSearchFieldsRequest()
            {
                accountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID()
            });
            if (response.FieldsViewModel != null)
            {
                searchFields = ExcludeNonViewableFields(response.FieldsViewModel);
            }
            viewModel.SearchFields = searchFields;
            ViewBag.module = "Contacts";
            return PartialView("_ExportPerson", viewModel);
        }

        /// <summary>
        /// Downloads the submissions.
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult DownloadSubmissions()
        {
            ExportPersonViewModel viewModel = new ExportPersonViewModel();
            IEnumerable<FieldViewModel> searchFields = null;

            GetAdvanceSearchFieldsResponse response = advancedSearchService.GetSearchFields(new GetAdvanceSearchFieldsRequest()
            {
                accountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID()
            });
            if (response.FieldsViewModel != null)
            {
                searchFields = ExcludeNonViewableFields(response.FieldsViewModel);
            }
            viewModel.SearchFields = searchFields;
            ViewBag.module = "Forms";
            return PartialView("_ExportPerson", viewModel);
        }

        /// <summary>
        /// Gets the export field types.
        /// </summary>
        /// <returns></returns>
        public JsonResult GetExportFieldTypes()
        {
            var data = EnumToListConverter.convertEnumToList<ExportFieldTypes>();
            return Json(new
            {
                success = true,
                response = data
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the order by types.
        /// </summary>
        /// <returns></returns>
        public JsonResult GetOrderByTypes()
        {
            var data = Enum.GetNames(typeof(ExportFieldOrder)).ToList();
            return Json(new
            {
                success = true,
                response = data
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets down load format.
        /// </summary>
        /// <returns></returns>
        public JsonResult GetDownLoadFormat()
        {
            var data = Enum.GetNames(typeof(DownloadType)).ToList();
            return Json(new
            {
                success = true,
                response = data
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Exprorts as file.
        /// </summary>
        /// <param name="exportPersonViewModel">The export person view model.</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> ExprortAsFile(ExportPersonViewModel exportPersonViewModel)
        {
            var viewModel1 = (AdvancedSearchViewModel)Session["AdvancedSearchVM"];
            Logger.Current.Informational("AdvancedSearchVM Logging" + viewModel1);
            var strngdata = Newtonsoft.Json.JsonConvert.SerializeObject(viewModel1);
            Logger.Current.Informational("Search Definition String Data" + strngdata);
            var drillDownContactIds = (int[])Session["ContactListIDs"];
            Logger.Current.Informational("DrilDown Contact Ids" + drillDownContactIds);

            int accountID = this.Identity.ToAccountID();
            string accountsids = System.Configuration.ConfigurationManager.AppSettings["Excluded_Accounts"].ToString();
            bool accountFound = accountsids.Contains(accountID.ToString());
            if (accountFound)
                throw new UnsupportedOperationException("[|You do not have permission to perform this operation.|]");
            if (exportPersonViewModel.SelectAll == true)
            {
                BulkOperations operationData = new BulkOperations()
                {
                    OperationID = 0,
                    OperationType = (int)BulkOperationTypes.Export,
                    SearchCriteria = Server.UrlDecode(ReadCookie("selectallsearchstring")),
                    AdvancedSearchCriteria = strngdata,
                    SearchDefinitionID = null,
                    AccountID = accountID,
                    UserID = (int)this.Identity.ToUserID(),
                    RoleID = this.Identity.ToRoleID(),
                    ExportSelectedFields = string.Join(",", exportPersonViewModel.selectedFields),
                    ExportType = exportPersonViewModel.DownLoadAs == "CSV" ? (int)DownloadType.CSV : (exportPersonViewModel.DownLoadAs == "Excel" ? (int)DownloadType.Excel : (exportPersonViewModel.DownLoadAs == "PDF" ? (int)DownloadType.PDF : 0)),
                    DateFormat = this.Identity.ToDateFormat(),
                    TimeZone = this.Identity.ToTimeZone(),
                    AccountPrimaryEmail = this.Identity.ToAccountPrimaryEmail(),
                    AccountDomain = Request.Url.Host,
                    UserEmailID = this.Identity.ToUserEmail()
                };
                InsertBulkOperationRequest bulkOperationRequest = new InsertBulkOperationRequest()
                {
                    OperationData = operationData,
                    AccountId = this.Identity.ToAccountID(),
                    RequestedBy = this.Identity.ToUserID(),
                    CreatedOn = DateTime.Now.ToUniversalTime().AddMinutes(1),
                    RoleId = this.Identity.ToRoleID(),
                    DrillDownContactIds = drillDownContactIds
                };
                accountService.InsertBulkOperation(bulkOperationRequest);
            }
            else
            {
                GetContactsDataResponce response = new GetContactsDataResponce();
                response = contactService.GetContactsData(new GetContactsDataRequest()
                {
                    Limit = exportPersonViewModel.PageSize,
                    PageNumber = 1,
                    AccountId = this.Identity.ToAccountID(),
                    ContactIDs = exportPersonViewModel.ContactID,
                    RequestedBy = this.Identity.ToUserID(),
                    RoleId = this.Identity.ToRoleID()
                });
                List<int?> Companyids = new List<int?>();
                Companyids = response.Contacts.Where(p => p.CompanyID != null).Select(p => p.CompanyID).ToList();
                IEnumerable<Contact> companies = contactService.GetAllContactsByCompanyIds(Companyids, this.Identity.ToAccountID());
                foreach (var person in response.Contacts.Where(p => p.CompanyID != null))
                {
                    foreach (var company in companies)
                    {
                        if (person.CompanyID == company.Id && company.IsDeleted == false)
                            person.CompanyName = company.CompanyName;
                    }
                }
                var contactsList = response.Contacts.ToList();
                exportPersonViewModel.Contacts = contactsList;
                ExportPersonsResponse exportResponse = await contactService.GetAllContactsByIds(new ExportPersonsRequest()
                {
                    DownLoadAs = exportPersonViewModel.DownLoadAs,
                    DateFormat = this.Identity.ToDateFormat(),
                    ExportViewModel = exportPersonViewModel,
                    AccountId = this.Identity.ToAccountID(),
                    TimeZone = this.Identity.ToTimeZone()
                });
                string fileKey = Guid.NewGuid().ToString();
                bool result = cachingService.StoreTemporaryFile(fileKey, exportResponse.byteArray);
                Logger.Current.Informational("Temporary file stored : " + result);
                return Json(new
                {
                    success = true,
                    fileKey = fileKey,
                    fileName = exportResponse.FileName
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                success = true
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the submitted form information.
        /// </summary>
        /// <param name="FormSubmissionID">The form submission identifier.</param>
        /// <returns></returns>
        [Route("formsubmissioninfo")]
        public ActionResult GetSubmittedFormInfo(int FormSubmissionID)
        {
            GetFormSubmissionRequest request = new GetFormSubmissionRequest()
            {
                FormSubmissionID = FormSubmissionID
            };
            GetFormSubmissionResponse response = formService.GetFormSubmission(request);
            return PartialView("_SubmittedFormDetails", response.FormSubmission);
        }

        /// <summary>
        /// Gets the submitted lead adapter information.
        /// </summary>
        /// <param name="JobLogDetailID">The job log detail identifier.</param>
        /// <returns></returns>
        [Route("leadadaptersubmission")]
        public ActionResult GetSubmittedLeadAdapterInfo(int JobLogDetailID)
        {
            GetLeadAdapterSubmissionRequest request = new GetLeadAdapterSubmissionRequest()
            {
                JobLogDetailID = JobLogDetailID
            };
            GetLeadAdapterSubmissionResponse response = leadAdapterService.GetLeadAdapterSubmission(request);
            Dictionary<object, object> leadAdapterResultsDict = JsonConvert.DeserializeObject<Dictionary<object, object>>(response.LeadAdapterSubmission.SubmittedData);
            var result = new List<LeadadapterSubmittedDetails>();
            foreach (var item in leadAdapterResultsDict)
            {
                var model = new LeadadapterSubmittedDetails();
                model.Title = item.Key.ToString();
                var dataItem = JsonConvert.DeserializeObject<LeadAdapterSubmittedDataItem>(item.Value.ToString());
                model.OldValue = dataItem.OldValue.Replace("{BREAK}", "<br>");
                model.NewValue = dataItem.NewValue.Replace("{BREAK}", "<br>");
                result.Add(model);
            }
            response.LeadAdapterSubmission.submittedDetails = result;
            return PartialView("_SubmittedLeadAdapterDetails", response.LeadAdapterSubmission);
        }

        /// <summary>
        /// Gets the email body.
        /// </summary>
        /// <param name="SendMailDetailID">The send mail detail identifier.</param>
        /// <returns></returns>
        [Route("getemailbody")]
        public ActionResult GetEmailBody(int SendMailDetailID)
        {
            GetEmailBodyRequest request = new GetEmailBodyRequest()
            {
                SendMailID = SendMailDetailID
            };
            GetEmailBodyResponse response = communicationService.GetEmailBody(request);
            ViewBag.EmailBody = response.EmailBody;
            return PartialView("_EmailBody");
        }

        /// <summary>
        /// Gets the email body.
        /// </summary>
        /// <param name="ReceivedMailInfoID">The send mail detail identifier.</param>
        /// <returns></returns>
        [Route("getreceivedemailbody")]
        public ActionResult GetReceivedEmailBody(int ReceivedMailInfoID)
        {
            GetEmailBodyRequest request = new GetEmailBodyRequest()
            {
                ReceivedMailInfoID = ReceivedMailInfoID
            };
            GetEmailBodyResponse response = communicationService.GetEmailBody(request);
            ViewBag.EmailBody = response.EmailBody;
            return PartialView("_EmailBody");
        }

        /// <summary>
        /// Campaigns the view.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns></returns>
        [Route("campaigndesignview")]
        public ActionResult CampaignView(int campaignId)
        {
            int accountId = this.Identity.ToAccountID();
            GetCampaignResponse response = campaignService.GetCampaign(new GetCampaignRequest(campaignId)
            {
                AccountId = accountId,
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID()
            });
            return PartialView("_SentCampaignHtml", response.CampaignViewModel);
        }

        /// <summary>
        /// Downloads the file.
        /// </summary>
        /// <param name="fileKey">The file key.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public ActionResult DownloadFile(string fileKey, string fileName)
        {
            ////byte[] bytes = System.Text.Encoding.UTF8.GetBytes("FullName,Email,Company,Phonenumber,PhoneType,Address,Lifecycle,CreatedDate\nArjun Prasad ,arjun@gmail.com,,,,No address details,Partner,2011-09-09 10:05 AM\n");
            byte[] file = cachingService.GetTemporaryFile(fileKey);
            return File(file, @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml", fileName);
        }

        public ActionResult DownloadFileFromPath(string fileKey, string fileName, string accountId)
        {
            ////byte[] bytes = System.Text.Encoding.UTF8.GetBytes("FullName,Email,Company,Phonenumber,PhoneType,Address,Lifecycle,CreatedDate\nArjun Prasad ,arjun@gmail.com,,,,No address details,Partner,2011-09-09 10:05 AM\n");
            var destinationPath = Path.Combine(ConfigurationManager.AppSettings["ATTACHMENT_PHYSICAL_PATH"].ToString(), fileKey);
            byte[] file = System.IO.File.ReadAllBytes(destinationPath);
            string e = Path.GetExtension(destinationPath);
            return File(file, @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml", "Export" + e);
        }

        #endregion
        /// <summary>
        /// Gets the emails count.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        public async Task<JsonResult> GetEmailsCount(int contactId, DateTime period)
        {
            GetContactCampaignStatisticsResponse response = await contactService.GetContactCampaignSummary(new GetContactCampaignStatisticsRequest()
            {
                ContactId = contactId,
                Period = period.ToUserUtcDateTime(),
                AccountId = this.Identity.ToAccountID()
            });
            return Json(new
            {
                success = true,
                response = response
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the opportunity summary.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public JsonResult GetOpportunitySummary(int contactId)
        {
            GetOpportunitySummaryResponse response = contactService.GetOpportunitySummary(new GetOpportunitySummaryRequest()
            {
                ContactId = contactId,
                Period = ReadCookie("widgetperiod")
            });
            return Json(new
            {
                success = true,
                response = response
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the persons count.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public JsonResult GetPersonsCount(int contactId)
        {
            GetPersonsCountResponse response = contactService.GetPersonsCount(new GetPersonsCountRequest()
            {
                ContactId = contactId,
                AccountId = this.Identity.ToAccountID()
            });
            return Json(new
            {
                success = true,
                response = response
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the contact lead score list.
        /// </summary>
        /// <returns></returns>
        public JsonResult GetContactLeadScoreList()
        {
            var dropdowns = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            SearchContactsResponse<ContactLeadScoreListViewModel> searchresponse = new SearchContactsResponse<ContactLeadScoreListViewModel>();
            searchresponse = contactService.GetPersons<ContactLeadScoreListViewModel>(new SearchContactsRequest()
            {
                Query = "",
                Limit = 10,
                PageNumber = 1,
                SortFieldType = (ContactSortFieldType)Convert.ToInt16("6"),
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID()
            });
            if (searchresponse.Contacts != null)
            {
                var lifecycleStages = dropdowns.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LifeCycle).Select(s => s.DropdownValuesList).ToList().FirstOrDefault();
                foreach (var contact in searchresponse.Contacts)
                {
                    contact.LifecycleName = lifecycleStages.Where(e => e.DropdownValueID == contact.LifecycleStage).Select(s => s.DropdownValue).FirstOrDefault();
                }
            }
            return Json(searchresponse, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Updates the user activity log.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="entityName">Name of the entity.</param>
        void UpdateUserActivityLog(int entityId, string entityName)
        {
            int userId = this.Identity.ToUserID();
            UserReadActivityRequest request = new UserReadActivityRequest();
            request.ActivityName = UserActivityType.Read;
            request.EntityId = entityId;
            request.ModuleName = AppModules.Contacts;
            request.UserId = userId;
            request.AccountId = this.Identity.ToAccountID();
            request.EntityName = entityName;
            userService.InsertReadActivity(request);
        }

        /// <summary>
        /// Inserts the change owner activity.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="userId">The user identifier.</param>
        void InsertChangeOwnerActivity(int entityId, int? userId)
        {
            userService.InsertChangeOwnerActivity(new ChangeOwnerLogRequest()
            {
                ActivityName = UserActivityType.ChangeOwner,
                EntityId = entityId,
                ModuleName = AppModules.Contacts,
                UserId = userId
            });
        }

        /// <summary>
        /// Gets the web visits count.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        public async Task<JsonResult> GetWebVisitsCount(int contactId, DateTime period)
        {
            GetContactWebVisitsCountResponse response = null;
            response = await contactService.GetWebVisitsCount(new GetContactWebVisitsCountRequest()
            {
                ContactId = contactId,
                Period = period.ToUserUtcDateTime()
            });
            return Json(new
            {
                success = true,
                response = response
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the contact web visits.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        [Route("webvisitdetails")]
        public ActionResult GetContactWebVisits(int contactId)
        {
            GetContactWebVisitReportResponse response = new GetContactWebVisitReportResponse();
            response.WebVisits = contactService.GetContactWebVisits(new GetContactWebVisitReportRequest()
            {
                ContactId = contactId,
                Period = ReadCookie("widgetperiod")
            }).WebVisits;
            response.WebVisits.ToList().ForEach(w => w.VisitedOn = w.VisitedOn.ToUtcBrowserDatetime());
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            return PartialView("_WebVisitDetails", response.WebVisits);
        }

        /// <summary>
        /// Webs the visti details.
        /// </summary>
        /// <param name="visitId">The visit identifier.</param>
        /// <returns></returns>
        [Route("getwebvisit")]
        public ActionResult WebVistiDetails(int visitId)
        {
            GetWebVisitByVisitIDResponse response = new GetWebVisitByVisitIDResponse();
            response.WebVisits = contactService.GetWebVisitByVisitID(new GetWebVisitByVisitIDRequest()
            {
                ContactWebVisitID = visitId
            }).WebVisits;
            response.WebVisits.ToList().ForEach(w => w.VisitedOn = w.VisitedOn.ToUtcBrowserDatetime());
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            ViewBag.CurrentPage = "ContactDetails";
            return PartialView("~/Views/Shared/_WebVisitDetails.cshtml", response.WebVisits);
        }

        public async Task<JsonResult> GetContactLeadScore(int contactId, DateTime period)
        {
            GetContactAuditLeadScoreResponse response = null;
            response = await contactService.GetContactLeadScore(new GetContactAuditLeadScoreRequest()
            {
                ContactId = contactId,
                Period = period.ToUserUtcDateTime(),
                AccountId = this.Identity.ToAccountID()
            });
            return Json(new
            {
                success = true,
                response = response
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the engagement details.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        public async Task<JsonResult> GetEngagementDetails(int contactId, DateTime period)
        {
            GetEngagementDetailsResponse response = await contactService.GetEngagementInformation(new GetEngagementDetailsRequest()
            {
                ContactID = contactId,
                Period = period.ToUserUtcDateTime(),
                AccountId = this.Identity.ToAccountID()
            });
            return Json(new
            {
                success = true,
                response = response
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetContactEmailEngagementDetails(int contactId)
        {
            ContactEmailEngagementDetails details = contactService.GetContactEmailEngagementDetails(contactId, this.Identity.ToAccountID());

            return Json(new
            {
                success = true,
                response = details
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEmailEngagementDetails(int contactId)
        {
            GetEngagementDetailsResponse response = contactService.GetEmailStastics(new GetEngagementDetailsRequest()
            {
                ContactID = contactId,
                AccountId = this.Identity.ToAccountID()
            });

            return Json(new
            {
                success = true,
                response = response.EmailInfo
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult AddressLookup(string zillowUrl)
        {
            string zillowKey = ConfigurationManager.AppSettings["ZillowAddressLookUpKey"];
            string zillowURL = ConfigurationManager.AppSettings["ZillowAddressLookupURL"];

            var zillowClient = new RestClient(zillowURL);
            var zillowRequest = new RestRequest(zillowUrl.Replace("ZKEY", zillowKey), Method.GET);
            zillowRequest.AddHeader("Content-Type", "application/xml;charset=UTF-8");

            try
            {
                var zillowData = zillowClient.Execute(zillowRequest);
                List<ZillowAddressLookup> lookups = ReadXML(zillowData.Content);
                return Json(new { success = true, response = lookups }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while requesting zillow for address lookup", ex);
                return Json(new { success = false, response = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult WhitePages(string street_line_1, string city, string postal_code, string api_key)
        {
            string whitePagesUrl = "street_line_1=" + street_line_1.Replace(" ", "+") + "&city=" + city.Replace(" ", "+") + "&postal_code=" + postal_code + "&api_key=" + api_key;
            string whitePagesKey = ConfigurationManager.AppSettings["WhitePagesKey"];
            string whitePagesURL = ConfigurationManager.AppSettings["WhitePagesURL"];

            var whitePagesClient = new RestClient(whitePagesURL);
            var whitePagesRequest = new RestRequest(whitePagesUrl.Replace("WKEY", whitePagesKey), Method.GET);
            whitePagesRequest.AddHeader("Content-Type", "application/json;charset=UTF-8");
            SmartTouch.CRM.Domain.ValueObjects.WhitePages whitePagesData = new WhitePages();
            try
            {
                //var data = whitePagesClient.Execute(whitePagesRequest);
                var data = whitePagesClient.Execute<WhitePages>(whitePagesRequest).Content;
                whitePagesData = JsonConvert.DeserializeObject<WhitePages>(data);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while requesting whitepages for address lookup", ex);
            }
            return View("WhitePages", whitePagesData);
        }

        private List<ZillowAddressLookup> ReadXML(string xml)
        {
            XElement rootElement = XElement.Parse(xml);
            Node node = new Node(rootElement);
            IEnumerable<Node> nodes = node["response"];
            List<ZillowAddressLookup> lookups = new List<ZillowAddressLookup>();
            if (nodes.IsAny())
            {
                foreach (var n in nodes)
                {
                    NodeAttributes attributes = n.GetAllAttributesAndElements();
                    ZillowAddressLookup lookup = new ZillowAddressLookup();
                    lookup.Comparables = attributes["comparables"].Value;
                    lookup.MapThisHome = attributes["mapthishome"].Value;
                    lookup.GraphsAndData = attributes["graphsanddata"].Value;
                    lookup.HomeDetails = attributes["homedetails"].Value;
                    lookup.Street = attributes["street"].Value;
                    lookup.City = attributes["city"].Value;
                    lookup.State = attributes["state"].Value;
                    lookup.ZipCode = attributes["zipcode"].Value;
                    lookups.Add(lookup);
                }
            }
            return lookups;
        }

        #region redactor
        /// <summary>
        /// Gets the email template names.
        /// </summary>
        /// <returns></returns>
        public JsonResult GetEmailTemplateNames()
        {
            GetCampaignTemplateNamesResponse response = campaignService.GetTemplateNamesRequest(new GetCampaignTemplateNamesRequest()
            {
                AccountId = this.Identity.ToAccountID()
            });
            return Json(new
            {
                success = true,
                response = response.TemplateNames
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the email template.
        /// </summary>
        /// <param name="templateId">The template identifier.</param>
        /// <param name="Type">The type.</param>
        /// <returns></returns>
        public JsonResult GetEmailTemplate(int templateId, CampaignTemplateType Type)
        {
            if (templateId != 0)
            {
                GetCampaignTemplateResponse response = campaignService.GetCampaignTemplateHTML(new GetCampaignTemplateRequest()
                {
                    CampaignTemplateID = templateId,
                    TemplateType = Type
                });

                if (Type == CampaignTemplateType.SentCampaigns)
                {
                    Dictionary<byte, string> urls = campaignService.GetCampaignURLSById(templateId);
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(response.HTMLContent);
                    if (urls.IsAny())
                    {
                        urls.Each(u =>
                        {
                            string linkfound = "linkid=" + u.Key.ToString();
                            doc.DocumentNode.SelectNodes("//a").Each(attr =>
                            {
                                if (attr.GetAttributeValue("href", "not found").Contains(linkfound))
                                    attr.SetAttributeValue("href", u.Value);
                            });

                        });

                    }

                    response.HTMLContent = doc.DocumentNode.InnerHtml;
                }

                return Json(new
                {
                    success = true,
                    response = response.HTMLContent
                }, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(new
                {
                    success = false,
                    response = ""
                }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Uploads the image.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        /// <exception cref="LandmarkIT.Enterprise.Utilities.ExceptionHandling.UnsupportedOperationException">upload image size should not exceed  + (imageSize / 1024f) + KB</exception>
        public ActionResult UploadImage(HttpPostedFileBase file)
        {
            string imagePhysicalPath = string.Empty;
            string storageName = string.Empty;
            int imageSize = 0;
            var imageSizeRes = ConfigurationManager.AppSettings["IMAGESIZERESTRICTION"];
            int.TryParse(imageSizeRes, out imageSize);
            var accountId = this.Identity.ToAccountID();
            try
            {
                if (file != null && file.ContentLength < imageSize)
                {
                    InsertCampaignImageRequest request = null;
                    InsertCampaignImageResponse response = new InsertCampaignImageResponse();
                    ImageViewModel imageViewModel = new ImageViewModel();
                    string RootFolder = string.Empty;
                    ViewBag.Image = null;
                    request = new InsertCampaignImageRequest();
                    storageName = Guid.NewGuid().ToString() + "." + file.FileName.Split('.')[1];
                    byte[] fileData = null;
                    System.Drawing.Image Image;
                    RootFolder = CreateFolder(ImageCategory.Campaigns);
                    using (var binaryReader = new BinaryReader(file.InputStream))
                    {
                        fileData = binaryReader.ReadBytes(file.ContentLength);
                    }
                    imagePhysicalPath = Path.Combine(RootFolder, storageName);
                    using (MemoryStream ms = new MemoryStream(fileData))
                    {
                        Image = System.Drawing.Image.FromStream(ms);
                        Image.Save(imagePhysicalPath);
                    }
                    if (!System.IO.File.Exists(imagePhysicalPath))
                    {
                        Image.Save(imagePhysicalPath);
                    }
                    imageViewModel.AccountID = Convert.ToInt16(accountId);
                    imageViewModel.ImageContent = string.Empty;
                    imageViewModel.OriginalName = file.FileName;
                    imageViewModel.FriendlyName = file.FileName;
                    imageViewModel.ImageType = file.ContentType;
                    imageViewModel.StorageName = storageName;
                    imageViewModel.ImageCategoryID = ImageCategory.Campaigns;
                    request.ImageViewModel = imageViewModel;
                    response = campaignService.InsertCampaignImage(request);
                }
                else
                {
                    throw new UnsupportedOperationException("upload image size should not exceed " + (imageSize / 1024f) + "KB");
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
            var imagePath = urlService.GetUrl(accountId, Entities.ImageCategory.Campaigns, storageName);
            return Json(new
            {
                filelink = imagePath
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Creates the folder.
        /// </summary>
        /// <param name="ImageCategory">The image category.</param>
        /// <returns></returns>
        private string CreateFolder(ImageCategory ImageCategory)
        {
            var accountId = this.Identity.ToAccountID();
            var imagePhysicalPath = ConfigurationManager.AppSettings["IMAGE_PHYSICAL_PATH"].ToString();
            imagePhysicalPath = Path.Combine(imagePhysicalPath, accountId.ToString());
            if (!System.IO.Directory.Exists(imagePhysicalPath))
            {
                System.IO.Directory.CreateDirectory(imagePhysicalPath);
            }
            switch (ImageCategory)
            {
                case ImageCategory.ContactProfile:
                    imagePhysicalPath = Path.Combine(imagePhysicalPath, "pi");
                    break;
                case ImageCategory.Campaigns:
                    imagePhysicalPath = Path.Combine(imagePhysicalPath, "ci");
                    break;
                case ImageCategory.AccountLogo:
                    imagePhysicalPath = Path.Combine(imagePhysicalPath, "ai");
                    break;
                default:
                    imagePhysicalPath = string.Empty;
                    break;
            }
            if (!System.IO.Directory.Exists(imagePhysicalPath))
            {
                System.IO.Directory.CreateDirectory(imagePhysicalPath);
            }
            return imagePhysicalPath;
        }

        /// <summary>
        /// Gets the campaign images.
        /// </summary>
        /// <returns></returns>
        public JsonResult GetCampaignImages(int PageNumber)
        {
            GetAccountCampaignImagesResponse response = campaignService.FindAllImages(new GetCampaignImagesRequest()
            {
                name = "",
                PageNumber = PageNumber,
                Limit = 24,
                AccountID = UserExtensions.ToAccountID(this.Identity)
            });
            return Json(new
            {
                data = response.Images
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region FullContact
        /// <summary>
        /// Gets the contact data.
        /// </summary>
        /// <param name="emailId">The email identifier.</param>
        /// <param name="contactType">Type of the contact.</param>
        /// <returns></returns>
        private static FullContact GetContactData(string emailId, ContactType contactType)
        {
            var client = new RestClient("https://api.fullcontact.com");
            var apiKey = System.Configuration.ConfigurationManager.AppSettings["FullContactKey"];
            var request = new RestRequest("v2/person.json?", Method.GET);
            request.RequestFormat = DataFormat.Json;
            request.AddParameter("email", emailId, ParameterType.GetOrPost);
            request.AddParameter("apiKey", apiKey, ParameterType.GetOrPost);
            request.AddHeader("Content-Type", "application/json;charset=UTF-8");
            FullContact fullContact = new FullContact();
            try
            {
                fullContact = client.Execute<FullContact>(request).Data;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while fetching social profiles from Full Contact Api :", ex);
            }
            return fullContact;
        }

        /// <summary>
        /// Manages the person social profiles.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <returns></returns>
        Dictionary<bool, PersonViewModel> ManagePersonSocialProfiles(PersonViewModel viewModel)
        {
            bool isChanged = true;
            Dictionary<bool, PersonViewModel> personVM = new Dictionary<bool, PersonViewModel>();
            PersonViewModel updatedModel = viewModel;
            FullContact fullContact = GetContactData(viewModel.Emails.Where(w => w.IsPrimary == true).Select(s => s.EmailId).FirstOrDefault(), ContactType.Person);
            if (fullContact != null && fullContact.socialProfiles != null && fullContact.socialProfiles.Count > 0)
            {
                var facebookUrl = fullContact.socialProfiles.SingleOrDefault(p => p.typeId == "facebook");
                var twitterUrl = fullContact.socialProfiles.SingleOrDefault(p => p.typeId == "twitter");
                var linkedinUrl = fullContact.socialProfiles.SingleOrDefault(p => p.typeId == "linkedin");
                var googleUrl = fullContact.socialProfiles.SingleOrDefault(p => p.typeId == "googleplus");
                if (facebookUrl != null && !string.IsNullOrEmpty(facebookUrl.url))
                    if (updatedModel.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Facebook") != null)
                        updatedModel.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Facebook").URL = facebookUrl.url;
                    else
                        updatedModel.SocialMediaUrls.Add(new Url
                        {
                            MediaType = "Facebook",
                            URL = facebookUrl.url
                        });
                if (twitterUrl != null && !string.IsNullOrEmpty(twitterUrl.url))
                    if (updatedModel.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Twitter") != null)
                        updatedModel.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Twitter").URL = twitterUrl.url;
                    else
                        updatedModel.SocialMediaUrls.Add(new Url
                        {
                            MediaType = "Twitter",
                            URL = twitterUrl.url
                        });
                if (linkedinUrl != null && !string.IsNullOrEmpty(linkedinUrl.url))
                    if (updatedModel.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "LinkedIn") != null)
                        updatedModel.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "LinkedIn").URL = linkedinUrl.url;
                    else
                        updatedModel.SocialMediaUrls.Add(new Url
                        {
                            MediaType = "LinkedIn",
                            URL = linkedinUrl.url
                        });
                if (googleUrl != null && !string.IsNullOrEmpty(googleUrl.url))
                    if (updatedModel.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Google+") != null)
                        updatedModel.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Google+").URL = googleUrl.url;
                    else
                        updatedModel.SocialMediaUrls.Add(new Url
                        {
                            MediaType = "Google+",
                            URL = googleUrl.url
                        });
            }
            if (fullContact != null && fullContact.photos != null && fullContact.photos.Count > 0)
                updatedModel.ContactImageUrl = fullContact.photos.FirstOrDefault().url;
            if (fullContact != null && fullContact.socialProfiles == null && fullContact.photos == null)
                isChanged = false;
            personVM.Add(isChanged, updatedModel);
            return personVM;
        }

        /// <summary>
        /// Manages the company social profiles.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <returns></returns>
        Dictionary<bool, CompanyViewModel> ManageCompanySocialProfiles(CompanyViewModel viewModel)
        {
            bool isChanged = true;
            Dictionary<bool, CompanyViewModel> companyVM = new Dictionary<bool, CompanyViewModel>();
            CompanyViewModel updatedModel = new CompanyViewModel();
            updatedModel = viewModel;
            FullContact fullContact = GetContactData(viewModel.Emails.Where(w => w.IsPrimary == true).Select(s => s.EmailId).FirstOrDefault(), ContactType.Company);
            if (fullContact != null && fullContact.socialProfiles != null && fullContact.socialProfiles.Count > 0)
            {
                var facebookUrl = fullContact.socialProfiles.SingleOrDefault(p => p.typeId == "facebook");
                var twitterUrl = fullContact.socialProfiles.SingleOrDefault(p => p.typeId == "twitter");
                var linkedinUrl = fullContact.socialProfiles.SingleOrDefault(p => p.typeId == "linkedin");
                var googleUrl = fullContact.socialProfiles.SingleOrDefault(p => p.typeId == "googleplus");
                if (facebookUrl != null && !string.IsNullOrEmpty(facebookUrl.url))
                    if (updatedModel.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Facebook") != null)
                        updatedModel.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Facebook").URL = facebookUrl.url;
                    else
                        updatedModel.SocialMediaUrls.Add(new Url
                        {
                            MediaType = "Facebook",
                            URL = facebookUrl.url
                        });
                if (twitterUrl != null && !string.IsNullOrEmpty(twitterUrl.url))
                    if (updatedModel.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Twitter") != null)
                        updatedModel.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Twitter").URL = twitterUrl.url;
                    else
                        updatedModel.SocialMediaUrls.Add(new Url
                        {
                            MediaType = "Twitter",
                            URL = twitterUrl.url
                        });
                if (linkedinUrl != null && !string.IsNullOrEmpty(linkedinUrl.url))
                    if (updatedModel.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "LinkedIn") != null)
                        updatedModel.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "LinkedIn").URL = linkedinUrl.url;
                    else
                        updatedModel.SocialMediaUrls.Add(new Url
                        {
                            MediaType = "LinkedIn",
                            URL = linkedinUrl.url
                        });
                if (googleUrl != null && !string.IsNullOrEmpty(googleUrl.url))
                    if (updatedModel.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Google+") != null)
                        updatedModel.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Google+").URL = googleUrl.url;
                    else
                        updatedModel.SocialMediaUrls.Add(new Url
                        {
                            MediaType = "Google+",
                            URL = googleUrl.url
                        });
            }
            if (fullContact != null && fullContact.photos != null && fullContact.photos.Count > 0)
                updatedModel.ContactImageUrl = fullContact.photos.FirstOrDefault().url;
            if (fullContact != null && fullContact.socialProfiles == null && fullContact.photos == null)
                isChanged = false;
            companyVM.Add(isChanged, updatedModel);
            return companyVM;
        }

        /// <summary>
        /// Updates the social profiles.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="contactType">Type of the contact.</param>
        /// <returns></returns>
        public JsonResult UpdateSocialProfiles(int contactId, ContactType contactType)
        {
            PersonViewModel updatedPersonVM = new PersonViewModel();
            CompanyViewModel updatedCompanyVM = new CompanyViewModel();
            if (contactType == ContactType.Person)
            {
                updatedPersonVM = contactService.GetPerson(new GetPersonRequest(contactId)
                {
                    RequestedBy = this.Identity.ToUserID(),
                    AccountId = this.Identity.ToAccountID(),
                    RoleId = this.Identity.ToRoleID(),
                    IncludeLastTouched = true
                }).PersonViewModel;
                Dictionary<bool, PersonViewModel> viewModel = ManagePersonSocialProfiles(updatedPersonVM);
                updatedPersonVM = viewModel.Values.FirstOrDefault();
                bool isChanged = viewModel.Keys.FirstOrDefault();
                if (isChanged)
                {
                    UpdatePersonRequest request = new UpdatePersonRequest()
                    {
                        PersonViewModel = updatedPersonVM,
                        RequestedBy = this.Identity.ToUserID(),
                        AccountId = this.Identity.ToAccountID(),
                        RoleId = this.Identity.ToRoleID(),
                        ModuleId = (byte)AppModules.Contacts
                    };
                    contactService.UpdatePerson(request);
                }
            }
            else if (contactType == ContactType.Company)
            {
                updatedCompanyVM = contactService.GetCompany(new GetCompanyRequest(contactId)
                {
                    RequestedBy = this.Identity.ToUserID(),
                    AccountId = this.Identity.ToAccountID(),
                    RoleId = this.Identity.ToRoleID()
                }).CompanyViewModel;
                Dictionary<bool, CompanyViewModel> viewModel = ManageCompanySocialProfiles(updatedCompanyVM);
                updatedCompanyVM = viewModel.Values.FirstOrDefault();
                bool isChanged = viewModel.Keys.FirstOrDefault();
                if (isChanged)
                {
                    UpdateCompanyRequest request = new UpdateCompanyRequest()
                    {
                        CompanyViewModel = updatedCompanyVM,
                        RequestedBy = this.Identity.ToUserID(),
                        AccountId = this.Identity.ToAccountID(),
                        RoleId = this.Identity.ToRoleID(),
                        ModuleId = (byte)AppModules.Contacts
                    };
                    contactService.UpdateCompany(request);
                }
            }
            return Json(new
            {
                success = true
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion
        /// <summary>
        /// Gets the contact web visits.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public JsonResult GetContactWebVisits(GetContactWebVisitsSummaryRequest request)
        {
            GetContactWebVisitsSummaryResponse response = new GetContactWebVisitsSummaryResponse();
            response = contactService.GetContactWebVisitsSummary(request);
            if (response.WebVisits.IsAny())
            {
                response.WebVisits.Each(wv =>
                {
                    wv.VisitedOn = wv.VisitedOn.ToUtc().ToUtcBrowserDatetime();
                });
            }

            return Json(new
            {
                success = true,
                response = new DataSourceResult
                {
                    Data = response.WebVisits,
                    Total = (int)response.TotalHits
                }
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Validates the contact permission.
        /// </summary>
        private void ValidateContactPermission()
        {
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            var usersPermissions = cachingService.GetUserPermissions(Thread.CurrentPrincipal.Identity.ToAccountID());
            List<byte> userModules = usersPermissions.Where(s => s.RoleId == (short)Thread.CurrentPrincipal.Identity.ToRoleID()).Select(s => s.ModuleId).ToList();
            if (userModules.Contains((byte)AppModules.SendMail))
                ViewBag.EmailPermission = true;
            bool isAccountAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
            ViewBag.IsAccountAdmin = isAccountAdmin;
            RemoveCookie("contactid");
        }

        public JsonResult SaveColumnPreferences(int entityId, byte entityType, IEnumerable<int> fields, byte showingType)
        {
            if (entityId != default(int) && entityType != default(byte))
            {
                AVColumnPreferenceViewModel viewModel = new AVColumnPreferenceViewModel();
                viewModel.EntityID = entityId;
                viewModel.EntityType = entityType;
                viewModel.Fields = fields;
                viewModel.ShowingType = showingType;
                advancedSearchService.SaveColumns(new SaveAdvancedViewColumnsRequest()
                {
                    model = viewModel
                });
            }
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<int> GetColumnPreferences(IList<FilterViewModel> searchFilters, int entityId, byte type)
        {
            IEnumerable<int> fields = new List<int>();
            GetAdvancedViewColumnsResponse columnPreferenceResponse = advancedSearchService.GetColumns(new GetAdvancedViewColumnsRequest()
            {
                EntityId = entityId,
                EntityType = type
            });
            if (columnPreferenceResponse != null && columnPreferenceResponse.ColumnPreferenceViewModel.IsAny())
            {
                fields = columnPreferenceResponse.ColumnPreferenceViewModel.Select(s => s.FieldID).ToList();
                ViewBag.ShowingType = columnPreferenceResponse.ColumnPreferenceViewModel.Select(s => s.ShowingType).FirstOrDefault();
            }
            else if (searchFilters != null)
            {
                var SelectedFields = searchFilters.Select(s => s.FieldId).ToList();
                if (entityId == default(int))
                    SelectedFields.AddRange(new List<int>() { 1, 2, 3, 7 });
                fields = SelectedFields == null ? null : (IEnumerable<int>)SelectedFields.Distinct();
            }
            return fields;
        }

        public JsonResult GetContactActivity(int contactId, int pageNumber, int pageSize)
        {
            var person = new PersonViewModel();
            Func<JsonResult, object> GetValue = j =>
            {
                var type = j.Data.GetType();
                var property = type.GetProperty("response");
                var value = property.GetValue(j.Data, null);
                return value;
            };
            var opportunities = GetOpportunities(contactId, pageNumber, pageSize);
            person.Opportunities = (IEnumerable<OpportunityBuyer>)GetValue(opportunities);
            var actions = GetActions(contactId, pageNumber, pageSize);
            person.Actions = (IEnumerable<ActionViewModel>)GetValue(actions);
            var tours = GetTours(contactId, pageNumber, pageSize);
            person.Tours = (IEnumerable<TourViewModel>)GetValue(tours);
            var relationships = GetRelationships(contactId);
            person.RelationshipViewModel = (RelationshipViewModel)GetValue(relationships);
            return Json(new
            {
                success = true,
                response = person
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetContactIdByName(bool isprimary, string email, byte contactType, string name, int contactId)
        {
            CustomContactViewModel customViewModel = new CustomContactViewModel();
            int accountId = this.Identity.ToAccountID();
            if (contactType == 1)
            {
                PersonViewModel personViewModel = new PersonViewModel();
                personViewModel.ContactID = contactId;
                personViewModel.Emails = new List<Email>();
                personViewModel.OwnerId = this.Identity.ToUserID();
                Email emailId = new Email();
                emailId.EmailId = email;
                emailId.IsPrimary = isprimary;
                personViewModel.Emails.Add(emailId);
                personViewModel.AccountID = accountId;
                customViewModel = contactService.PersonDuplicateCheck(new InsertPersonRequest()
                {
                    PersonViewModel = personViewModel,
                    AccountId = this.Identity.ToAccountID(),
                    RequestedBy = this.Identity.ToUserID(),
                    RoleId = this.Identity.ToRoleID()
                });
            }
            else
            {
                CompanyViewModel viewModel = new CompanyViewModel();
                viewModel.ContactID = contactId;
                viewModel.CompanyName = name;
                viewModel.OwnerId = this.Identity.ToUserID();
                viewModel.Emails = new List<Email>();
                Email emailId = new Email();
                emailId.EmailId = email;
                emailId.IsPrimary = isprimary;
                viewModel.Emails.Add(emailId);
                viewModel.AccountID = accountId;
                customViewModel = contactService.CompanyDuplicateCheck(new InsertCompanyRequest()
                {
                    CompanyViewModel = viewModel,
                    AccountId = this.Identity.ToAccountID(),
                    RequestedBy = this.Identity.ToUserID(),
                    RoleId = this.Identity.ToRoleID()
                });
            }

            return Json(new
            {
                success = true,
                response = customViewModel
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ContactDuplicateCheck(string person, byte contactType)
        {
            CustomContactViewModel customViewModel = new CustomContactViewModel();

            int accountId = this.Identity.ToAccountID();
            if (contactType == 1)
            {
                PersonViewModel PersonViewModel = JsonConvert.DeserializeObject<PersonViewModel>(person);
                PersonViewModel.AccountID = accountId;
                PersonViewModel.OwnerId = this.Identity.ToUserID();
                customViewModel = contactService.PersonDuplicateCheck(new InsertPersonRequest()
                {
                    PersonViewModel = PersonViewModel,
                    AccountId = this.Identity.ToAccountID(),
                    RequestedBy = this.Identity.ToUserID(),
                    RoleId = this.Identity.ToRoleID()
                });
            }
            else
            {
                CompanyViewModel companyViewModel = JsonConvert.DeserializeObject<CompanyViewModel>(person);
                companyViewModel.AccountID = accountId;
                companyViewModel.OwnerId = this.Identity.ToUserID();
                customViewModel = contactService.CompanyDuplicateCheck(new InsertCompanyRequest()
                {
                    CompanyViewModel = companyViewModel,
                    AccountId = this.Identity.ToAccountID(),
                    RequestedBy = this.Identity.ToUserID(),
                    RoleId = this.Identity.ToRoleID()
                });
            }

            return Json(new
            {
                success = true,
                response = customViewModel
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Updates a contact custom field
        /// </summary>
        /// <param name="contactId"></param>
        /// <param name="fieldId"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public JsonResult UpdateCustomFieldValue(int contactId, int fieldId, string newValue, short inputType)
        {
            Logger.Current.Verbose(string.Format("Updating contact {0} field {1} with value {2}", contactId, fieldId, newValue));
            int result = contactService.UpdateContactCustomField(new UpdateContactCustomFieldRequest()
            {
                ContactId = contactId,
                FieldId = fieldId,
                Value = newValue,
                AccountId = this.Identity.ToAccountID(),
                inputType = inputType,
                DateFormat = this.Identity.ToDateFormat()
            }).Result;
            return Json(new
            {
                success = result
            }, JsonRequestBehavior.AllowGet);
        }

        [Route("contactworkflowsummary")]
        [MenuType(MenuCategory.ContactEngagementSummary, MenuCategory.LeftMenuCRM)]
        public ActionResult ContactWorkflowSummary(int contactId)
        {
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            ViewBag.ContactId = contactId;
            return View("ContactWorkflowSummary");
        }

        [Route("contactcampaigsummary")]
        [MenuType(MenuCategory.ContactEngagementSummary, MenuCategory.LeftMenuCRM)]
        public ActionResult ContactCampaigSummary(int contactId)
        {
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            ViewBag.AccountId = this.Identity.ToAccountID();
            ViewBag.ContactId = contactId;
            ViewBag.CategoryType = 0;
            return View("ContactCampaigSummary");
        }

        [Route("cs")]
        [MenuType(MenuCategory.ContactEngagementSummary, MenuCategory.LeftMenuCRM)]
        public ActionResult ContactCampaigSummary(int contactId, int t)
        {
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            ViewBag.AccountId = this.Identity.ToAccountID();
            ViewBag.CategoryType = t;
            ViewBag.ContactId = contactId;
            return View("ContactCampaigSummary");
        }

        [Route("es")]
        [MenuType(MenuCategory.ContactEngagementSummary, MenuCategory.LeftMenuCRM)]
        public ActionResult ContactEmailSummary(int c, int t)
        {
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            ViewBag.AccountId = this.Identity.ToAccountID();
            ViewBag.CategoryType = t;
            ViewBag.ContactId = c;
            return View("ContactEmailSummary");
        }

        [Route("contactemailsummary")]
        [MenuType(MenuCategory.ContactEngagementSummary, MenuCategory.LeftMenuCRM)]
        public ActionResult ContactEmailSummary(int contactId)
        {
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            ViewBag.AccountId = this.Identity.ToAccountID();
            ViewBag.ContactId = contactId;
            ViewBag.CategoryType = 0;
            return View("ContactEmailSummary");
        }

        public ActionResult GetContactWorkflowSummaryReadView([DataSourceRequest] DataSourceRequest request, int contactId)
        {
            GetContactEngagementSummaryResponse response = contactService.GetContactWorkflowSummaryDetails(new GetContactEngagementSummaryRequest()
            {
                AccountId = this.Identity.ToAccountID(),
                Limit = request.PageSize,
                PageNumber = request.Page,
                ContactId = contactId
            });
            return Json(new DataSourceResult { Data = response.ContactWorkflowDetails, Total = response.TotalHits }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetContactEmailSummaryReadView([DataSourceRequest] DataSourceRequest request, int contactId, int type)
        {
            GetContactEngagementSummaryResponse response = contactService.GetContactEmailSummaryDetails(new GetContactEngagementSummaryRequest()
            {
                AccountId = this.Identity.ToAccountID(),
                Limit = request.PageSize,
                PageNumber = request.Page,
                ContactId = contactId,
                Type = type
            });
            return Json(new DataSourceResult { Data = response.ContactEmailDetails, Total = response.TotalHits }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetContactCampaignSummaryReadView([DataSourceRequest] DataSourceRequest request, int contactId, int type)
        {
            GetContactEngagementSummaryResponse response = contactService.GetContactCampaignSummaryDetails(new GetContactEngagementSummaryRequest()
            {
                AccountId = this.Identity.ToAccountID(),
                Limit = request.PageSize,
                PageNumber = request.Page,
                ContactId = contactId,
                Type = type
            });
            return Json(new DataSourceResult { Data = response.ContactCampaignDetails, Total = response.TotalHits }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Send Email Opens and Clicks Tracking.
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="lid"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("emailtrack")]
        public ActionResult EmailTracker(int c, int l = 0, int a = 0, int s = 0)
        {
            string content = @"R0lGODlhAQABAIABAP///wAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==";
            var imageBytes = System.Convert.FromBase64String(content);
            if ((c != 0 && l != 0) || s != 0)
            {
                GetEmailLinkURLResponse response = contactService.GetSendMailDetailIdByLinkId(new GetEmailLinkURLRequest() { LinkId = l });
                contactService.InsertEmailClickEntry(new InsertEmailOpenOrClickEntryRequest()
                {
                    SentMailDetailID = s != 0 ? s : response.EmailLinkViewModel.SendMailDetailId,
                    ContactID = c,
                    EmailLinkID = l != 0 ? (int?)l : null,
                    ActivityType = l != 0 ? (byte)EmailContactActivity.Click : (byte)EmailContactActivity.Open,
                    ActivityDate = DateTime.Now.ToUniversalTime(),
                });

                if (l != 0)
                {
                    var url = new Uri(response.EmailLinkViewModel.URL.URL);
                    var urlBuilder = new UriBuilder(url);
                    string redirectedToBowser = ToDecodeString(urlBuilder.Uri.ToString());
                    Logger.Current.Informational("Redirecting to : " + redirectedToBowser);
                    return Redirect(redirectedToBowser);
                }
            }
            var imageStream = new MemoryStream(imageBytes);
            var fileResult = new FileStreamResult(imageStream, "image/gif");
            imageStream.Flush();
            return fileResult;
        }

        public GetNeverBounceBadEmailContactResponse GetNeverBounceBadEmailContactIds(int nerverBounceRequestId, byte emailStatus)
        {
            GetNeverBounceBadEmailContactResponse response = contactService.GetNeverBounceBadEmailContacts(new GetNeverBounceBadEmailContactRequest() { NeverbounceRequestID = nerverBounceRequestId, EmailStatus = emailStatus });
            return response;
        }

        public string ReplacingSpecialCharacterWithTheirCode(string content)
        {
            StringBuilder result = new StringBuilder(content.Length + (int)(content.Length * 0.1));
            foreach (char c in content)
            {
                int value = Convert.ToInt32(c);
                if (value > 127)
                    result.AppendFormat("&#{0};", value);
                else
                    result.Append(c);

            }

            return result.ToString();
        }

        public JsonResult GetEmailClickedLinks(int smid, int cid)
        {

            List<LinkClickedDetails> urls = contactService.GetEmailClickedLinkURLs(smid, cid);
            return Json(new
            {
                success = true,
                response = urls
            }, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetCampaignClickedLinks(int cid, int crid)
        {

            List<LinkClickedDetails> urls = contactService.GetCampaignClickedLinkURLs(cid, crid);
            return Json(new
            {
                success = true,
                response = urls
            }, JsonRequestBehavior.AllowGet);
        }

        private string ToDecodeString(string x)
        {
            if (x.Contains("&amp;"))
            {
                x = x.Replace("&amp;", "&");
            }
            return x;

        }

        private void InsertTourDetailsToContactSummary(TourViewModel viewModel, string searchCriteria, int[] drildownIds)
        {
            string tourDetails = string.Format("Tour Date: {0}, Tour Community: {1}, Tour Type: {2}", viewModel.TourDate.ToUserUtcDateTimeV2(), viewModel.Communities.Where(c => c.DropdownValueID == viewModel.CommunityID).Select(s => s.DropdownValue).FirstOrDefault(), viewModel.TourTypes.Where(t => t.DropdownValueID == viewModel.TourType).Select(s => s.DropdownValue).FirstOrDefault());
            NoteViewModel noteViewModel = new NoteViewModel();
            noteViewModel.AccountId = viewModel.AccountId;
            noteViewModel.NoteDetails = !string.IsNullOrEmpty(viewModel.TourDetails) ? viewModel.TourDetails : tourDetails;
            noteViewModel.NoteCategory = noteService.GetActionDetailsNoteCategoryID(viewModel.AccountId.Value, (short)DropdownValueTypes.TourDetails, (byte)DropdownFieldTypes.NoteCategory);
            noteViewModel.AddToContactSummary = true;
            noteViewModel.Contacts = viewModel.Contacts.ToList();
            noteViewModel.CreatedBy = viewModel.CreatedBy;
            noteViewModel.CreatedOn = DateTime.Now.ToUniversalTime();
            noteViewModel.SelectAll = viewModel.SelectAll;
            noteViewModel.TagsList = new List<TagViewModel>() { };

            SaveNoteResponse response = noteService.InsertNote(new SaveNoteRequest()
            {
                NoteViewModel = noteViewModel
            });

            if (viewModel.SelectAll)
            {
                BulkOperations operationData = new BulkOperations()
                {
                    OperationID = response.NoteViewModel.NoteId,
                    OperationType = (int)BulkOperationTypes.Note,
                    SearchCriteria = HttpUtility.HtmlEncode(Server.UrlDecode(ReadCookie("selectallsearchstring"))),
                    AdvancedSearchCriteria = searchCriteria,
                    SearchDefinitionID = null,
                    AccountID = viewModel.AccountId.Value,
                    UserID = viewModel.CreatedBy,
                    RoleID = this.Identity.ToRoleID(),
                    AccountPrimaryEmail = this.Identity.ToAccountPrimaryEmail(),
                    AccountDomain = Request.Url.Host,
                    // ActionCompleted = request.ActionViewModel.IsCompleted
                };
                InsertBulkOperationRequest bulkOperationRequest = new InsertBulkOperationRequest()
                {
                    OperationData = operationData,
                    AccountId = viewModel.AccountId.Value,
                    RequestedBy = this.Identity.ToUserID(),
                    CreatedOn = DateTime.Now.ToUniversalTime().AddMinutes(1),
                    RoleId = this.Identity.ToRoleID(),
                    DrillDownContactIds = drildownIds
                };

                accountService.InsertBulkOperation(bulkOperationRequest);
            }

        }
    }
    public class Entity
    {
        public int SalesOrderID { get; set; }
        public int SalesOrderDetailID { get; set; }
        public string CarrierTrackingNumber { get; set; }
        public int OrderQty { get; set; }
        public int ProductID { get; set; }
        public double UnitPrice { get; set; }
    }
}
