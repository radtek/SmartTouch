using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using LandmarkIT.Enterprise.Utilities.Logging;
using Newtonsoft.Json;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.Messaging.Common;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Web.Utilities;
using LandmarkIT.Enterprise.Extensions;
using System.Web.SessionState;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using System.Collections.Specialized;
using SmartTouch.CRM.ApplicationServices.Messaging.AccountUnsubscribeView;
using System.Threading;
using System.Text;
using HtmlAgilityPack;

namespace SmartTouch.CRM.Web.Controllers
{
    public class CampaignController : SmartTouchController
    {
        readonly ICampaignService campaignService;

        readonly ICommunicationService communicationService;

        readonly IContactService contactService;

        readonly IAccountService accountService;

        readonly IFormService formService;

        readonly IUserService userService;

        readonly ICachingService cacheService;

        readonly IUrlService urlService;

        readonly IAccountSettingsService accountSettingsService;

        public CampaignController(ICampaignService campaignService, ICommunicationService communicationService, IContactService contactService, IAccountService accountService, IUserService userService, IFormService formService, IUrlService urlService, ICachingService cacheService, IAccountSettingsService accountSettingsService)
        {
            this.campaignService = campaignService;
            this.communicationService = communicationService;
            this.contactService = contactService;
            this.accountService = accountService;
            this.userService = userService;
            this.urlService = urlService;
            this.formService = formService;
            this.cacheService = cacheService;
            this.accountSettingsService = accountSettingsService;
        }

        /// <summary>
        /// Campaignses this instance.
        /// </summary>
        /// <returns></returns>
        [Route("campaigns")]
        [SmarttouchAuthorize(AppModules.Campaigns, AppOperations.Read)]
        [MenuType(MenuCategory.Campaigns, MenuCategory.LeftMenuCRM)]
        [OutputCache(Duration = 30)]
        public ActionResult Campaigns()
        {
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            ViewBag.ItemsPerPage = ItemsPerPage;
            return View("CampaignList");
        }

        /// <summary>
        /// Campaignses the report list.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="StartDate">The start date.</param>
        /// <param name="EndDate">The end date.</param>
        /// <returns></returns>
        [Route("campaignsreportlist/{userIds?}/{StartDate?}/{EndDate?}")]
        [SmarttouchAuthorize(AppModules.Campaigns, AppOperations.Read)]
        [MenuType(MenuCategory.Campaigns, MenuCategory.LeftMenuCRM)]
        [OutputCache(Duration = 30)]
        public ActionResult CampaignsReportList(string userIds, string StartDate, string EndDate)
        {
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);

            ViewBag.DateFormat = this.Identity.ToDateFormat();
            ViewBag.ItemsPerPage = ItemsPerPage;
            int[] userIDs = null;
            if (!string.IsNullOrEmpty(userIds))
                userIDs = JsonConvert.DeserializeObject<int[]>(userIds);
            ViewBag.UserIds = userIDs;
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            return View("CampaignList");
        }

        /// <summary>
        /// Campaigns the search.
        /// </summary>
        /// <returns></returns>
        [Route("campaigns/search")]
        [SmarttouchAuthorize(AppModules.Campaigns, AppOperations.Read)]
        [MenuType(MenuCategory.Campaigns, MenuCategory.LeftMenuCRM)]
        public ActionResult CampaignSearch()
        {
            ViewBag.campaignId = 1;
            return View("CampaignList");
        }

        /// <summary>
        /// Campaignses the ListView.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="name">The name.</param>
        /// <param name="status">The status.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="StartDate">The start date.</param>
        /// <param name="EndDate">The end date.</param>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.Campaigns, AppOperations.Read)]
        public ActionResult CampaignsListView([DataSourceRequest] DataSourceRequest request, string name, string status, string userIds, string StartDate, string EndDate)
        {
            byte accountstatus = 0;
            if (!String.IsNullOrEmpty(status))
                accountstatus = Convert.ToByte(status);
            int[] userIDs = null;
            if (!string.IsNullOrEmpty(userIds))
                userIDs = JsonConvert.DeserializeObject<int[]>(userIds);

            AddCookie("campaignpagesize", request.PageSize.ToString(), 1);
            AddCookie("campaignpagenumber", request.Page.ToString(), 1);
            SearchCampaignsResponse response = campaignService.GetAllCampaigns(new SearchCampaignsRequest()
            {
                Query = name,
                Limit = request.PageSize,
                PageNumber = request.Page,
                ShowingFieldType = accountstatus,
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID(),
                UserID = this.Identity.ToUserID(),
                UserIds = userIDs,
                StartDate = !string.IsNullOrEmpty(StartDate) ? Convert.ToDateTime(StartDate) : (DateTime?)null,
                EndDate = !string.IsNullOrEmpty(EndDate) ? Convert.ToDateTime(EndDate) : (DateTime?)null,
                SortField = request.Sorts.Count > 0 ? request.Sorts.First().Member : null,
                SortDirection = request.Sorts.Count > 0 ? request.Sorts.First().SortDirection : System.ComponentModel.ListSortDirection.Descending
            });
            Logger.Current.Informational("Request received successfully for getting all campaigns.");
            response.Campaigns.Where(x => x.ProcessedDate.HasValue).Each(x => x.ProcessedDate = x.ProcessedDate.Value.ToUtc().ToUtcBrowserDatetime());
            response.Campaigns.Where(x => x.ScheduleTime.HasValue).Each(x => x.ScheduleTime = x.ScheduleTime.Value.ToUtc().ToUtcBrowserDatetime());
            response.Campaigns.Where(x => x.LastUpdatedOn.HasValue).Each(x => x.LastUpdatedOn = x.LastUpdatedOn.Value.ToUtc().ToUtcBrowserDatetime());
            var jsonResult = Json(new DataSourceResult
            {
                Data = response.Campaigns,
                Total = (int)response.TotalHits
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        /// <summary>
        /// Adds the cookie.
        /// </summary>
        /// <param name="cookieName">Name of the cookie.</param>
        /// <param name="Value">The value.</param>
        /// <param name="days">The days.</param>
        public void AddCookie(string cookieName, string Value, int days)
        {
            HttpCookie CartCookie = new HttpCookie(cookieName, Value);
            CartCookie.Expires = DateTime.Now.AddDays(days);
            Response.Cookies.Add(CartCookie);
        }

        /// <summary>
        /// _s the resend campaign.
        /// </summary>
        /// <returns></returns>
        public ActionResult _ResendCampaign()
        {
            ResendCampaignViewModel viewModel = new ResendCampaignViewModel();
            viewModel.ParentCampaignId = (int)Session["CampaignID"];
            viewModel.SenderName = this.User.Identity.ToFirstName() + " " + this.User.Identity.ToLastName();
            viewModel.From = string.Empty;
            ViewBag.CampaignId = Session["CampaignID"];
            return PartialView("_ResendCampaign", viewModel);
        }

        /// <summary>
        /// Resends the new campaign.
        /// </summary>
        /// <param name="viewmodel">The viewmodel.</param>
        /// <returns></returns>
        public ActionResult ResendNewCampaign(string viewmodel)
        {
            ResendCampaignViewModel ResendviewModel = JsonConvert.DeserializeObject<ResendCampaignViewModel>(viewmodel);
            int accountId = this.Identity.ToAccountID();
            GetCampaignResponse response = campaignService.GetCampaign(new GetCampaignRequest(ResendviewModel.ParentCampaignId)
            {
                AccountId = accountId,
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID()
            });
            CampaignViewModel viewModel = response.CampaignViewModel;
            viewModel.ProcessedDate = null;
            viewModel.CampaignStatus = CampaignStatus.Queued;
            viewModel.ClickRate = null;
            viewModel.DeliveryRate = null;
            viewModel.Remarks = null;
            viewModel.SentCount = 0;
            viewModel.Subject = ResendviewModel.Subject;
            viewModel.SenderName = ResendviewModel.SenderName;
            viewModel.From = ResendviewModel.From;
            viewModel.ServiceProviderID = null;
            viewModel.ScheduleTime = null;
            viewModel.LastUpdatedBy = this.Identity.ToUserID();
            viewModel.LastUpdatedOn = DateTime.Now.ToUniversalTime();
            viewModel.Posts = viewModel.GetPosts();
            viewModel.CreatedBy = this.Identity.ToUserID();
            viewModel.CreatedDate = DateTime.Now.ToUniversalTime();
            viewModel.ParentCampaignId = ResendviewModel.ParentCampaignId;
            viewModel.IsRecipientsProcessed = false;
            GetResentCampaignCountRequest request = new GetResentCampaignCountRequest();
            request.ParentCampaignId = ResendviewModel.ParentCampaignId;
            request.CampaignResentTo = ResendviewModel.CampaignResentTo;
            GetResentCampaignCountResponse resentcampaignresponse = campaignService.GetResentCampaignCount(request);
            viewModel.Name = viewModel.Name + (ResendviewModel.CampaignResentTo == CampaignResentTo.NewContacts ? " ( Resend New Contacts -" : " ( Resend to Not Viewed Contacts -") + (resentcampaignresponse.Count + 1).ToString() + " )";
            viewModel.CampaignID = 0;
            foreach (var post in viewModel.Posts)
            {
                post.UserID = viewModel.CreatedBy;
                post.CampaignID = 0;
            }

            if (!string.IsNullOrEmpty(viewModel.HTMLContent))
            {
                viewModel.HTMLContent = ReplacingSpecialCharacterWithTheirCode(viewModel.HTMLContent);
            }

            QueueCampaignResponse queueResponse = campaignService.QueueCampaign(new QueueCampaignRequest()
            {
                CampaignViewModel = viewModel,
                AccountId = this.Identity.ToAccountID()
            });
            campaignService.SaveResendCampaign(new ResentCampaignRequest()
            {
                ParentCampaignId = ResendviewModel.ParentCampaignId,
                CampaignId = queueResponse.CampaignId,
                CampaignResentTo = ResendviewModel.CampaignResentTo
            });
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _AddCampaign()
        {
            return PartialView("_AddCampaign");
        }

        public ActionResult AddNewCampaign(string campaignVM)
        {
            var campaignViewModel = JsonConvert.DeserializeObject<CampaignViewModel>(campaignVM);
            campaignViewModel.CampaignStatus = CampaignStatus.Draft;
            campaignViewModel.ToTagStatus = 2;
            campaignViewModel.SSContactsStatus = 2;
            campaignViewModel.CreatedBy = this.Identity.ToUserID();
            campaignViewModel.AccountID = this.Identity.ToAccountID();
            campaignViewModel.SenderName = this.User.Identity.ToFirstName() + " " + this.User.Identity.ToLastName();
            campaignViewModel.DateFormat = this.Identity.ToDateFormat();
            campaignViewModel.CreatedDate = DateTime.UtcNow;
            campaignViewModel.From = string.Empty;
            campaignViewModel.IsLinkedToWorkflows = false;
            campaignViewModel.CampaignTemplate = campaignService.GetCampaignTemplate(new GetCampaignTemplateRequest()
            {
                CampaignTemplateID = 1
            }).CampaignTemplateViewModel;
            campaignViewModel.Links = new List<CampaignLinkViewModel>();
            ViewBag.SaveAs = false;
            ViewBag.AccountID = UserExtensions.ToAccountID(this.Identity);
            ViewBag.IsSTAdmin = this.Identity.IsSTAdmin();
            var userResponse = userService.GetUser(new ApplicationServices.Messaging.User.GetUserRequest(this.Identity.ToUserID()));
            campaignViewModel.EnableFacebook = (string.IsNullOrEmpty(userResponse.User.FacebookAccessToken)) ? false : true;
            campaignViewModel.EnableTwitter = (string.IsNullOrEmpty(userResponse.User.TwitterOAuthTokenSecret)) ? false : true;
            GetAllFieldsResponse response = formService.GetAllFields(new GetAllFieldsRequest()
            {
                AccountId = ViewBag.AccountID
            });
            IEnumerable<DropdownViewModel> DropdownValues = cacheService.GetDropdownValues(ViewBag.AccountID);
            IEnumerable<DropdownValueViewModel> PhoneFields = DropdownValues.Where(i => i.DropdownID == (byte)DropdownFieldTypes.PhoneNumberType).Select(x => x.DropdownValuesList).FirstOrDefault();
            IList<FieldViewModel> DropdownPhoneFields = new List<FieldViewModel>();
            foreach (DropdownValueViewModel phone in PhoneFields)
            {
                DropdownPhoneFields.Add(new FieldViewModel()
                {
                    AccountID = ViewBag.AccountID,
                    DisplayName = phone.DropdownValue,
                    DropdownId = (byte)DropdownFieldTypes.PhoneNumberType,
                    FieldCode = phone.DropdownValueID.ToString() + "DF",
                    FieldId = phone.DropdownValueID,
                    FieldInputTypeId = FieldType.text,
                    IsCustomField = false,
                    IsDropdownField = true,
                    IsMandatory = false,
                    StatusId = FieldStatus.Active,
                    Title = phone.DropdownValue,
                    ValidationMessage = phone.DropdownValue + " Requried",
                    Value = string.Empty
                });
            }
            ViewBag.TagPopup = true;
            IEnumerable<FieldViewModel> CustomFields = response.Fields.Where(i => i.AccountID != null);
            foreach (FieldViewModel fieldvm in CustomFields)
                fieldvm.FieldCode = fieldvm.FieldId.ToString() + "CF";
            campaignViewModel.ContactFields = response.Fields.Where(i => i.AccountID == null).Union(DropdownPhoneFields);
            campaignViewModel.CustomFields = CustomFields;
            ViewBag.ReplicatedCM = false;
            return View("_CampaignLayout", campaignViewModel);
        }

        /// <summary>
        /// Adds the campaign.
        /// </summary>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.Campaigns, AppOperations.Create)]
        [MenuType(MenuCategory.AddCampaign, MenuCategory.LeftMenuCRM)]
        public ActionResult AddCampaign()
        {
            ViewBag.AccountId = this.Identity.ToAccountID();
            CampaignViewModel campaignViewModel = new CampaignViewModel();
            campaignViewModel.CampaignStatus = Entities.CampaignStatus.Draft;
            campaignViewModel.ToTagStatus = 2;
            campaignViewModel.SSContactsStatus = 2;
            campaignViewModel.CampaignTemplates = campaignService.GetCampaignTemplates(new GetCampaignTemplatesRequest()
            {
                AccountId = this.Identity.ToAccountID()
            }).Templates;
            campaignViewModel.CreatedBy = this.Identity.ToUserID();
            campaignViewModel.AccountID = this.Identity.ToAccountID();
            campaignViewModel.Name = string.Empty;
            campaignViewModel.SenderName = this.User.Identity.ToFirstName() + " " + this.User.Identity.ToLastName();
            campaignViewModel.DateFormat = this.Identity.ToDateFormat();
            campaignViewModel.CreatedDate = DateTime.UtcNow;
            campaignViewModel.From = string.Empty;
            campaignViewModel.IsLinkedToWorkflows = false;
            campaignViewModel.CampaignTemplate = campaignService.GetCampaignTemplate(new GetCampaignTemplateRequest()
            {
                CampaignTemplateID = 1
            }).CampaignTemplateViewModel;
            campaignViewModel.Links = new List<CampaignLinkViewModel>();
            ViewBag.SaveAs = false;
            ViewBag.AccountID = UserExtensions.ToAccountID(this.Identity);
            ViewBag.IsSTAdmin = this.Identity.IsSTAdmin();
            var userResponse = userService.GetUser(new ApplicationServices.Messaging.User.GetUserRequest(this.Identity.ToUserID()));
            campaignViewModel.EnableFacebook = (string.IsNullOrEmpty(userResponse.User.FacebookAccessToken)) ? false : true;
            campaignViewModel.EnableTwitter = (string.IsNullOrEmpty(userResponse.User.TwitterOAuthTokenSecret)) ? false : true;
            int AccountID = this.Identity.ToAccountID();
            GetAllFieldsResponse response = formService.GetAllFields(new GetAllFieldsRequest()
            {
                AccountId = AccountID
            });
            IEnumerable<DropdownViewModel> DropdownValues = cacheService.GetDropdownValues(AccountID);
            IEnumerable<DropdownValueViewModel> PhoneFields = DropdownValues.Where(i => i.DropdownID == (byte)DropdownFieldTypes.PhoneNumberType).Select(x => x.DropdownValuesList).FirstOrDefault();
            IList<FieldViewModel> DropdownPhoneFields = new List<FieldViewModel>();
            foreach (DropdownValueViewModel phone in PhoneFields)
            {
                DropdownPhoneFields.Add(new FieldViewModel()
                {
                    AccountID = AccountID,
                    DisplayName = phone.DropdownValue,
                    DropdownId = (byte)DropdownFieldTypes.PhoneNumberType,
                    FieldCode = phone.DropdownValueID.ToString() + "DF",
                    FieldId = phone.DropdownValueID,
                    FieldInputTypeId = FieldType.text,
                    IsCustomField = false,
                    IsDropdownField = true,
                    IsMandatory = false,
                    StatusId = FieldStatus.Active,
                    Title = phone.DropdownValue,
                    ValidationMessage = phone.DropdownValue + " Requried",
                    Value = string.Empty
                });
            }
            ViewBag.TagPopup = true;
            IEnumerable<FieldViewModel> CustomFields = response.Fields.Where(i => i.AccountID != null);
            foreach (FieldViewModel fieldvm in CustomFields)
                fieldvm.FieldCode = fieldvm.FieldId.ToString() + "CF";
            campaignViewModel.ContactFields = response.Fields.Where(i => i.AccountID == null).Union(DropdownPhoneFields);
            campaignViewModel.CustomFields = CustomFields;
            ViewBag.HasDisclaimer = accountService.AccountHasDisclaimer(AccountID);
            ViewBag.ReplicatedCM = false;
            return View("_CampaignLayout", campaignViewModel);
        }

        /// <summary>
        /// Edits the campaign.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns></returns>
        [Route("editcampaign")]
        [SmarttouchAuthorize(AppModules.Campaigns, AppOperations.Edit)]
        [MenuType(MenuCategory.EditCampaign, MenuCategory.LeftMenuCRM)]
        public ActionResult EditCampaign(int campaignId)
        {
            int accountId = this.Identity.ToAccountID();
            GetCampaignResponse response = campaignService.GetCampaign(new GetCampaignRequest(campaignId)
            {
                AccountId = accountId,
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID()
            });
            ViewBag.DomainName = DomainName;
            ViewBag.SaveAs = false;
            ViewBag.IsSTAdmin = this.Identity.IsSTAdmin();
            ViewBag.AccountID = UserExtensions.ToAccountID(this.Identity);
            var absoluteUri = Request.UrlReferrer != null ? Request.UrlReferrer.AbsoluteUri : "";
            ViewBag.RedirectTo = absoluteUri;
            ViewBag.IsWorkflowId = absoluteUri.Contains("workflowid");
            response.CampaignViewModel.CreatedDate = response.CampaignViewModel.CreatedDate.ToUserUtcDateTimeV2();
            if (response.CampaignViewModel.ScheduleTime.HasValue)
                response.CampaignViewModel.ScheduleTime = response.CampaignViewModel.ScheduleTime.Value.ToUtc();
            var dateFormat = this.Identity.ToDateFormat();
            response.CampaignViewModel.SetPosts();
            AddCookie("dateformat", dateFormat, 1);
            response.CampaignViewModel.DateFormat = dateFormat;
            var userResponse = userService.GetUser(new ApplicationServices.Messaging.User.GetUserRequest(this.Identity.ToUserID()));
            response.CampaignViewModel.EnableFacebook = (string.IsNullOrEmpty(userResponse.User.FacebookAccessToken)) ? false : true;
            response.CampaignViewModel.EnableTwitter = (string.IsNullOrEmpty(userResponse.User.TwitterOAuthTokenSecret)) ? false : true;
            int AccountID = this.Identity.ToAccountID();
            GetAllFieldsResponse fieldresponse = formService.GetAllFields(new GetAllFieldsRequest()
            {
                AccountId = AccountID
            });
            IEnumerable<DropdownViewModel> DropdownValues = cacheService.GetDropdownValues(AccountID);
            IEnumerable<DropdownValueViewModel> PhoneFields = DropdownValues.Where(i => i.DropdownID == (byte)DropdownFieldTypes.PhoneNumberType).Select(x => x.DropdownValuesList).FirstOrDefault();
            IList<FieldViewModel> DropdownPhoneFields = new List<FieldViewModel>();
            foreach (DropdownValueViewModel phone in PhoneFields)
            {
                DropdownPhoneFields.Add(new FieldViewModel()
                {
                    AccountID = AccountID,
                    DisplayName = phone.DropdownValue,
                    DropdownId = (byte)DropdownFieldTypes.PhoneNumberType,
                    FieldCode = phone.DropdownValueID.ToString() + "DF",
                    FieldId = phone.DropdownValueID,
                    FieldInputTypeId = FieldType.text,
                    IsCustomField = false,
                    IsDropdownField = true,
                    IsMandatory = false,
                    StatusId = FieldStatus.Active,
                    Title = phone.DropdownValue,
                    ValidationMessage = phone.DropdownValue + " Requried",
                    Value = string.Empty
                });
            }
            ViewBag.TagPopup = true;
            ViewBag.LitmusTestPermission = false;
            ViewBag.MailTesterPermission = false;

            IEnumerable<FieldViewModel> CustomFields = fieldresponse.Fields.Where(i => i.AccountID != null);
            foreach (FieldViewModel fieldvm in CustomFields)
                fieldvm.FieldCode = fieldvm.FieldId.ToString() + "CF";
            response.CampaignViewModel.ContactFields = fieldresponse.Fields.Where(i => i.AccountID == null).Union(DropdownPhoneFields);
            response.CampaignViewModel.CustomFields = CustomFields;
            response.CampaignViewModel.CampaignStatus = response.CampaignViewModel.CampaignStatus;
            response.CampaignViewModel.CreatedDate = response.CampaignViewModel.CreatedDate.ToUtc();
            var usersPermissions = cacheService.GetUserPermissions(Thread.CurrentPrincipal.Identity.ToAccountID());
            List<byte> userModules = usersPermissions.Where(s => s.RoleId == (short)Thread.CurrentPrincipal.Identity.ToRoleID()).Select(s => s.ModuleId).ToList();
            if (userModules.Contains((byte)AppModules.LitmusTest))
                ViewBag.LitmusTestPermission = true;
            if (userModules.Contains((byte)AppModules.MailTester))
                ViewBag.MailTesterPermission = true;

            ViewBag.mode = "edit";
            ViewBag.EmailGuid = response.CampaignViewModel.LitmusGuid;
            ViewBag.HasDisclaimer = accountService.AccountHasDisclaimer(AccountID);
            ViewBag.HasLitmusAPIKey = accountService.GettingLitmusTestAPIKey(AccountID);
            ViewBag.ReplicatedCM = false;
            return View("_CampaignLayout", response.CampaignViewModel);
        }

        /// <summary>
        /// Campaigns the view.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns></returns>
        [Route("campaignview")]
        public ActionResult CampaignView(int campaignId)
        {
            int accountId = this.Identity.ToAccountID();
            GetCampaignResponse response = campaignService.GetCampaign(new GetCampaignRequest(campaignId)
            {
                AccountId = accountId,
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID()
            });
            if (response != null && response.CampaignViewModel != null && !string.IsNullOrEmpty(response.CampaignViewModel.HTMLContent))
            {
                response.CampaignViewModel.HTMLContent = response.CampaignViewModel.HTMLContent.Replace("imageeditor", "").Replace("k-widget", "").Replace("k-editor", "").Replace("k-editor-inline", "");
            }
            return PartialView("_CampaignHtml", response.CampaignViewModel);
        }

        /// <summary>
        /// Campaigns the statistics.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns></returns>
        [SmarttouchSessionStateBehaviour(SessionStateBehavior.Required)]
        [Route("CampaignStatistics")]
        [SmarttouchAuthorize(AppModules.Campaigns, AppOperations.Edit)]
        [MenuType(MenuCategory.CampaignStatistics, MenuCategory.LeftMenuCRM)]
        public ActionResult CampaignStatistics(int campaignId)
        {
            GetCampaignStatisticsResponse response = campaignService.GetCampaignStatistics(new GetCampaignStatisticsRequest()
            {
                AccountId = this.Identity.ToAccountID(),
                CampaignId = campaignId,
                RoleId = this.Identity.ToRoleID(),
                RequestedBy = this.Identity.ToUserID()
            });
            ViewBag.IsParentCampaign = response.IsParentCampaign;
            ViewBag.CampaignID = campaignId;
            var maps = campaignService.GetCampaignLitmusMap(new GetCampaignLitmusMapRequest()
            {
                AccountId = this.Identity.ToAccountID(),
                CampaignId = campaignId,
                RequestedBy = 0
            });
            ViewBag.CampaignID = campaignId;
            if (maps.CampaignLitmusMaps.Any())
                ViewBag.EmailGuid = maps.CampaignLitmusMaps.OrderByDescending(m => m.LastModifiedOn).FirstOrDefault().LitmusId;

            ViewBag.MailTesterGuid = campaignService.GetMailTesterGuid(new GetCampaignMailTesterGuid() { CampaignID = campaignId }).Guid;
            Session["CampaignID"] = campaignId;
            AddCookie("dateformat", this.Identity.ToDateFormat(), 1);
            if (response.CampaignStatisticsViewModel.SentOn.HasValue)
                response.CampaignStatisticsViewModel.SentOn = response.CampaignStatisticsViewModel.SentOn.Value.ToUtcBrowserDatetime();
            response.CampaignStatisticsViewModel.Clicks = new List<CampaignLinkContent>();
            response.CampaignStatisticsViewModel.Opens = response.CampaignStatisticsViewModel.Clicks;
            response.CampaignStatisticsViewModel.CampaignName = response.CampaignStatisticsViewModel.CampaignViewModel.Name;
            return View("CampaignDashboard", response.CampaignStatisticsViewModel);
        }

        /// <summary>
        /// _s the add campaign template.
        /// </summary>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.Campaigns, AppOperations.Create)]
        public ActionResult _AddCampaignTemplate()
        {
            CampaignViewModel viewmodel = new CampaignViewModel();
            CampaignTemplateViewModel template = new CampaignTemplateViewModel();
            template.TemplateId = 0;
            viewmodel.CampaignTemplate = template;
            return PartialView("_AddCampaignTemplate", viewmodel);
        }

        /// <summary>
        /// Inserts the campaign template.
        /// </summary>
        /// <param name="campaignTemplteViewModel">The campaign templte view model.</param>
        /// <returns></returns>
        public JsonResult InsertCampaignTemplate(string campaignTemplteViewModel)
        {
            CampaignTemplateViewModel viewModel = JsonConvert.DeserializeObject<CampaignTemplateViewModel>(campaignTemplteViewModel);
            InsertCampaignTemplateRequest request = new InsertCampaignTemplateRequest();
            request.CampaignTemplateViewModel = viewModel;
            request.AccountId = this.Identity.ToAccountID();
            request.RequestedBy = this.Identity.ToUserID();
            InsertCampaignTemplateResponse response = campaignService.InsertCampaignTemplate(request);
            return Json(new
            {
                success = true,
                response = response
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Saves the campaign as.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <param name="viewModel">The view model.</param>
        /// <returns></returns>
        [Route("savecampaignas")]
        [SmarttouchAuthorize(AppModules.Campaigns, AppOperations.Create)]
        [MenuType(MenuCategory.AddCampaign, MenuCategory.LeftMenuCRM)]
        public ActionResult SaveCampaignAs(int campaignId, string viewModel)
        {
            GetCampaignResponse response = new GetCampaignResponse();
            if (string.IsNullOrEmpty(viewModel))
            {
                response = campaignService.GetCampaign(new GetCampaignRequest(campaignId)
                {
                    RequestedBy = this.Identity.ToUserID(),
                    AccountId = this.Identity.ToAccountID(),
                    RoleId = this.Identity.ToRoleID()
                });
            }
            else
                response.CampaignViewModel = JsonConvert.DeserializeObject<CampaignViewModel>(viewModel);
            ;
            CampaignViewModel campaignViewModel = response.CampaignViewModel;
            campaignViewModel.CampaignID = 0;
            campaignViewModel.Name = "Copy of " + response.CampaignViewModel.Name;
            campaignViewModel.HTMLContent = response.CampaignViewModel.HTMLContent;
            campaignViewModel.ScheduleTime = null;
            campaignViewModel.CreatedBy = this.Identity.ToUserID();
            campaignViewModel.CreatedDate = DateTime.UtcNow;
            campaignViewModel.ServiceProviderID = null;
            campaignViewModel.ProcessedDate = null;
            campaignViewModel.ServiceProviderCampaignID = null;
            campaignViewModel.SentCount = 0;
            campaignViewModel.Remarks = null;
            campaignViewModel.LastViewedState = "D";
            campaignViewModel.LastUpdatedBy = null;
            campaignViewModel.LastUpdatedOn = null;
            campaignViewModel.Links.ToList().ForEach(c =>
            {
                c.CampaignId = 0;
                c.CampaignLinkId = 0;
            });
            if (response.CampaignViewModel.CampaignStatus == CampaignStatus.Active || response.CampaignViewModel.CampaignStatus == CampaignStatus.Queued || response.CampaignViewModel.CampaignStatus == CampaignStatus.Sent || response.CampaignViewModel.CampaignStatus == CampaignStatus.Sending || response.CampaignViewModel.CampaignStatus == CampaignStatus.Analyzing)
            {
                campaignViewModel.SenderName = this.User.Identity.ToFirstName() + " " + this.User.Identity.ToLastName();
                campaignViewModel.From = string.Empty;
                campaignViewModel.ContactTags = new List<TagViewModel>();
                campaignViewModel.TagsList = new List<TagViewModel>();
                campaignViewModel.SearchDefinitions = new List<AdvancedSearchViewModel>();
                campaignViewModel.Subject = "";
                campaignViewModel.TotalUniqueContacts = 0;
            }
            campaignViewModel.CampaignStatus = Entities.CampaignStatus.Draft;
            var dateFormat = this.Identity.ToDateFormat();
            campaignViewModel.DateFormat = dateFormat;
            var userResponse = userService.GetUser(new ApplicationServices.Messaging.User.GetUserRequest(this.Identity.ToUserID()));
            campaignViewModel.EnableFacebook = (string.IsNullOrEmpty(userResponse.User.FacebookAccessToken)) ? false : true;
            campaignViewModel.EnableTwitter = (string.IsNullOrEmpty(userResponse.User.TwitterOAuthTokenSecret)) ? false : true;
            campaignViewModel.IsRecipientsProcessed = false;
            campaignViewModel.LitmusGuid = null;
            campaignViewModel.MailTesterGuid = null;
            campaignViewModel.IsLitmusTestPerformed = false;
            ViewBag.SaveAs = true;
            int AccountID = this.Identity.ToAccountID();
            ViewBag.AccountID = AccountID;
            ViewBag.RedirectTo = string.Empty;
            ViewBag.IsWorkflowId = false;
            GetAllFieldsResponse fieldresponse = formService.GetAllFields(new GetAllFieldsRequest()
            {
                AccountId = AccountID
            });
            IEnumerable<DropdownViewModel> DropdownValues = cacheService.GetDropdownValues(AccountID);
            IEnumerable<DropdownValueViewModel> PhoneFields = DropdownValues.Where(i => i.DropdownID == (byte)DropdownFieldTypes.PhoneNumberType).Select(x => x.DropdownValuesList).FirstOrDefault();
            IList<FieldViewModel> DropdownPhoneFields = new List<FieldViewModel>();
            foreach (DropdownValueViewModel phone in PhoneFields)
            {
                DropdownPhoneFields.Add(new FieldViewModel()
                {
                    AccountID = AccountID,
                    DisplayName = phone.DropdownValue,
                    DropdownId = (byte)DropdownFieldTypes.PhoneNumberType,
                    FieldCode = phone.DropdownValueID.ToString() + "DF",
                    FieldId = phone.DropdownValueID,
                    FieldInputTypeId = FieldType.text,
                    IsCustomField = false,
                    IsDropdownField = true,
                    IsMandatory = false,
                    StatusId = FieldStatus.Active,
                    Title = phone.DropdownValue,
                    ValidationMessage = phone.DropdownValue + " Requried",
                    Value = string.Empty
                });
            }
            ViewBag.TagPopup = true;
            ViewBag.LitmusTestPermission = false;
            ViewBag.MailTesterPermission = false;

            var usersPermissions = cacheService.GetUserPermissions(Thread.CurrentPrincipal.Identity.ToAccountID());
            List<byte> userModules = usersPermissions.Where(s => s.RoleId == (short)Thread.CurrentPrincipal.Identity.ToRoleID()).Select(s => s.ModuleId).ToList();

            if (userModules.Contains((byte)AppModules.LitmusTest))
                ViewBag.LitmusTestPermission = true;
            if (userModules.Contains((byte)AppModules.MailTester))
                ViewBag.MailTesterPermission = true;

            IEnumerable<FieldViewModel> CustomFields = fieldresponse.Fields.Where(i => i.AccountID != null);
            foreach (FieldViewModel fieldvm in CustomFields)
                fieldvm.FieldCode = fieldvm.FieldId.ToString() + "CF";
            campaignViewModel.ContactFields = fieldresponse.Fields.Where(i => i.AccountID == null).Union(DropdownPhoneFields);
            campaignViewModel.CustomFields = CustomFields;
            ViewBag.HasDisclaimer = accountService.AccountHasDisclaimer(AccountID);
            ViewBag.HasLitmusAPIKey = accountService.GettingLitmusTestAPIKey(AccountID);
            ViewBag.ReplicatedCM = true;
            return View("_CampaignLayout", campaignViewModel);
        }

        /// <summary>
        /// Customizes this instance.
        /// </summary>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.Campaigns, AppOperations.Create)]
        [MenuType(MenuCategory.AddCampaign, MenuCategory.LeftMenuCRM)]
        public ActionResult Customize()
        {
            return PartialView("_CampaignDesigner");
        }

        /// <summary>
        /// Templateses this instance.
        /// </summary>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.Campaigns, AppOperations.Create)]
        [MenuType(MenuCategory.AddCampaign, MenuCategory.LeftMenuCRM)]
        public ActionResult Templates()
        {
            return PartialView("_CampaignTemplates");
        }

        /// <summary>
        /// Reviews this instance.
        /// </summary>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.Campaigns, AppOperations.Create)]
        [MenuType(MenuCategory.AddCampaign, MenuCategory.LeftMenuCRM)]
        public ActionResult Review()
        {
            return PartialView("_CampaignReviewAndSend");
        }

        /// <summary>
        /// Inserts the campaign.
        /// </summary>
        /// <param name="campaignViewModel">The campaign view model.</param>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.Campaigns, AppOperations.Create)]
        public ActionResult InsertCampaign(string campaignViewModel)
        {
            CampaignViewModel viewModel = JsonConvert.DeserializeObject<CampaignViewModel>(campaignViewModel);
            if (viewModel.ScheduleTime != null)
            {
                viewModel.ScheduleTime = viewModel.ScheduleTime.Value.ToUniversalTime();
            }
            InsertCampaignRequest request = new InsertCampaignRequest()
            {
                CampaignViewModel = viewModel
            };
            campaignService.InsertCampaign(request);
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Deletes the campaign.
        /// </summary>
        /// <param name="campaignIDs">The campaign i ds.</param>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.Campaigns, AppOperations.Delete)]
        public ActionResult DeleteCampaign(string campaignIDs)
        {
            if (!campaignIDs.IsAny())
            {
                return Json(new
                {
                    success = true,
                    response = "[|Could not delete the campaign.|]"
                }, JsonRequestBehavior.AllowGet);
            }
            DeleteCampaignRequest request = JsonConvert.DeserializeObject<DeleteCampaignRequest>(campaignIDs);
            request.RequestedBy = this.Identity.ToUserID();
            request.AccountId = this.Identity.ToAccountID();
            DeleteCampaignResponse response = new DeleteCampaignResponse();
            response = campaignService.Deactivate(request);
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
                return Json(new
                {
                    success = true,
                    response = "[|Successfully deleted campaign(s)|]"
                }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Archives the campaign.
        /// </summary>
        /// <param name="campaignIDs">The campaign i ds.</param>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.Campaigns, AppOperations.Delete)]
        public ActionResult ArchiveCampaign(string campaignIDs)
        {
            if (!campaignIDs.IsAny())
            {
                return Json(new
                {
                    success = true,
                    response = "[|Could not Archive the campaign.|]"
                }, JsonRequestBehavior.AllowGet);
            }
            DeleteCampaignRequest request = JsonConvert.DeserializeObject<DeleteCampaignRequest>(campaignIDs);
            request.RequestedBy = this.Identity.ToUserID();
            request.AccountId = this.Identity.ToAccountID();
            DeleteCampaignResponse response = new DeleteCampaignResponse();
            response = campaignService.Archive(request);
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
                return Json(new
                {
                    success = true,
                    response = "[|Successfully Archive campaign(s)|]"
                }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Imageses the ListView.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.Campaigns, AppOperations.Create)]
        public ActionResult ImagesListView([DataSourceRequest] DataSourceRequest request, string name)
        {
            ViewBag.imageID = 0;
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            AddCookie("imagepagesize", request.PageSize.ToString(), 1);
            AddCookie("imagepagenumber", request.Page.ToString(), 1);
            GetAccountCampaignImagesResponse response = campaignService.FindAllImages(new GetCampaignImagesRequest()
            {
                name = name,
                PageNumber = request.Page,
                Limit = request.PageSize == 0 ? -1 : request.PageSize,
                AccountID = UserExtensions.ToAccountID(this.Identity)
            });
            return Json(new DataSourceResult
            {
                Data = response.Images,
                Total = response.TotalHits
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Deletes the campaign image.
        /// </summary>
        /// <param name="imageID">The image identifier.</param>
        /// <returns></returns>
        public JsonResult DeleteCampaignImage(int imageID)
        {
            DeleteCampaignImageRequest request = new DeleteCampaignImageRequest(imageID)
            {
                AccountId = this.Identity.ToAccountID()
            };
            campaignService.DeleteCampaignImage(request);
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Campaigns the tracker image.
        /// </summary>
        /// <param name="cid">The cid.</param>
        /// <param name="cmpid">The cmpid.</param>
        /// <param name="linkId">The link identifier.</param>
        /// <param name="crid">The crid.</param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("campaignimage")]
        public ActionResult CampaignTrackerImage(string cid, int? cmpid, byte? linkId, int? crid)
        {
            Logger.Current.Informational("Request received to track campaign activity: cid= " + cid + ", cmpid =" + cmpid + ", linkId =" + linkId + ", crid =" + crid);
            string content = @"R0lGODlhAQABAIABAP///wAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==";
            var imageBytes = System.Convert.FromBase64String(content);
            int contactId = 0;
            int campaignId = cmpid ?? 0;
            try
            {
                bool validContactId = int.TryParse(cid, out contactId);
                if (validContactId || crid.HasValue)
                {
                    Logger.Current.Informational("Valid Contact" + cid);
                    HttpRequest currentRequest = System.Web.HttpContext.Current.Request;
                    String clientIP = currentRequest.ServerVariables["REMOTE_ADDR"];

                    var response = campaignService.InsertCampaignOpenEntry(new InsertCampaignOpenOrClickEntryRequest()
                    {
                        LinkId = linkId,
                        IpAddress = clientIP,
                        CampaignRecipientID = crid.Value
                    });
                    if (linkId != null && response != null)
                    {
                        contactId = response.Recipient.ContactID;
                        campaignId = response.Recipient.CampaignID;
                        var linkUrl = campaignService.GetLinkURl(new GetLinkUrlRequest()
                        {
                            CampaignId = response.Recipient.CampaignID,
                            LinkIndex = linkId,
                            ContactId = response.Recipient.ContactID
                        });
                        if (linkUrl != null && linkUrl.CampaignLinkViewModel != null && linkUrl.CampaignLinkViewModel.URL != null && linkUrl.CampaignLinkViewModel.URL.URL != null)
                        {
                            Logger.Current.Informational("Link url found");
                            var url = new Uri(linkUrl.CampaignLinkViewModel.URL.URL);
                            NameValueCollection nvcData = HttpUtility.ParseQueryString(url.Query);
                            Dictionary<string, string> queryStrings = new Dictionary<string, string>();
                            Dictionary<string, string> utm_medium_sources = new Dictionary<string, string>();
                            List<string> utm_strings = new List<string>();
                            utm_medium_sources.Add("1", "campaign");
                            utm_medium_sources.Add("2", "webinar");
                            utm_medium_sources.Add("3", "series");
                            utm_medium_sources.Add("4", "prospecting");
                            utm_medium_sources.Add("5", "reengagement");
                            utm_medium_sources.Add("6", "inventory");
                            Action<string, string> AddKey = (key, value) =>
                            {
                                if (!string.IsNullOrEmpty(key) && !queryStrings.ContainsKey(key))
                                    queryStrings.Add(key, value);
                            };

                            nvcData.AllKeys.Where(c => !string.IsNullOrEmpty(c)).Each(k => AddKey(k, nvcData.Get(k)));

                            Campaign campaign = campaignService.GetCampaignUTMInformation(campaignId);
                            Logger.Current.Informational("Fetched campaign UTM information : " + campaign.Name);
                            string utm_campaign = campaign.Name.ToLower() + "_" + campaignId;
                            string name = string.Format("{0}{1}", campaign.Name.ToLower(), campaign.Subject.ToLower());
                            utm_medium_sources.Each(s =>
                            {
                                if (name.Contains(s.Value))
                                    utm_strings.Add(s.Value);
                            });
                            string utm_medium = string.Empty;
                            if (utm_strings.IsAny())
                                utm_medium = string.Join("-", utm_strings);
                            else if (!utm_strings.IsAny() && campaign.CampaignStatus == CampaignStatus.Active)
                                utm_medium = "series";
                            else
                                utm_medium = "campaign";

                            AddKey("MyID", linkUrl.ReferenceId.ToString());
                            AddKey("utm_medium", utm_medium);
                            AddKey("utm_source", "email");
                            AddKey("utm_campaign", utm_campaign);

                            var query = HttpUtility.ParseQueryString(string.Empty);
                            var urlBuilder = new UriBuilder(url);
                            queryStrings.Each(k =>
                            {
                                query[k.Key] = HttpUtility.UrlEncode(k.Value);
                            });
                            urlBuilder.Query = query.ToString();
                            string redirectedToBowser = urlBuilder.Uri.ToString();
                            Logger.Current.Informational("Redirecting to : " + redirectedToBowser);
                            return Redirect(redirectedToBowser.Replace("%2540", "@"));
                        }
                        else
                        {
                            Logger.Current.Informational("Link url not found");
                            return Redirect("http://smarttouchinteractive.com" + "?MyID=" + linkUrl.ReferenceId);
                        }
                    }
                }
                else
                {
                    Logger.Current.Informational("Invalid Contact" + cid);
                }
                
            }
            catch(Exception ex)
            {
                ex.Data.Clear();
                ex.Data.Add("Keys", "cid= " + cid + ", cmpid =" + cmpid + ", linkId =" + linkId + ", crid =" + crid);
                Logger.Current.Error("Error while calculating campaign open/click", ex);
            }
            var imageStream = new MemoryStream(imageBytes);
            var fileResult = new FileStreamResult(imageStream, "image/gif");
            imageStream.Flush();
            return fileResult;
        }

        [AllowAnonymous]
        [Route("campaignEmailUpdate")]
        public JsonResult CampaignUpdateEmailstatus(string contactID, string emailID, int campaignId, int? snoozeperiod, int workflowId = 0)
        {
            try
            {
                int contactId = 0;
                bool validContactId = int.TryParse(contactID, out contactId);
                int snoozedays = snoozeperiod ?? 0;
                if (validContactId)
                    Logger.Current.Informational("Valid Contact" + contactID);
                else
                    Logger.Current.Informational("Invalid Contact" + contactID);
                if (validContactId)
                {
                    var datevalue = DateTime.UtcNow;
                    CampaignUnsubscribeResponse response = communicationService.UpdateContactEmailStatus(new CampaignUnsubscribeRequest()
                    {
                        ContactId = contactId,
                        SnoozeUntil = snoozeperiod != null ? datevalue.AddDays(snoozedays) : default(DateTime),
                        Email = emailID,
                        CampaignId = campaignId
                    });
                    var contactIds = new List<int>();
                    contactIds.Add((int)response.contactId);
                    contactService.ContactIndexing(new ContactIndexingRequest()
                    {
                        ContactIds = contactIds,
                        Ids = new Dictionary<int, bool>() { { response.contactId.Value, true } }.ToLookup(o => o.Key, o => o.Value)
                    });
                    campaignService.UpdateCampaignRecipientOptOutStatus(contactId, campaignId, workflowId);
                    accountService.ScheduleAnalyticsRefresh(campaignId, (byte)IndexType.Campaigns);
                    if(workflowId > 0)
                    {
                        accountService.ScheduleAnalyticsRefresh(workflowId, (byte)IndexType.Workflows);
                    }
                    if (response.Exception == null)
                        return Json(new
                        {
                            success = true,
                            response = "[|Your email address |] " + emailID + " [| has now been unsubscribed from future mailings.|]"
                        }, JsonRequestBehavior.AllowGet);
                    else
                        return Json(new
                        {
                            success = false,
                            response = "[|Unsubscribe failure|]"
                        }, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(new
                    {
                        success = false,
                        response = "[|Invalid contact|]"
                    }, JsonRequestBehavior.AllowGet);

            }
            catch(Exception ex)
            {
                Logger.Current.Error("Error while unsubscribing contact", ex);
                ex.Data.Clear();
                ex.Data.Add("ContactId", contactID);
                return Json(new
                {
                    success = false,
                    response = "[|Unsubscribe failure|]"
                }, JsonRequestBehavior.AllowGet);

            }
            
        }

        /// <summary>
        /// Campaigns the unsubscribe.
        /// </summary>
        /// <param name="acct">The acct.</param>
        /// <param name="cid">The cid.</param>
        /// <param name="cmpid">The cmpid.</param>
        /// <param name="email">The email.</param>
        /// <param name="crid">The crid.</param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("campaignUnsubscribe")]
        [SmarttouchActionFilter]
        public ActionResult CampaignUnsubscribe(int? acct, string cid, int? cmpid, string email, int? crid)
        {
            Logger.Current.Verbose(string.Format("Request received to unsubscribe recipient id: {0} and accountid : {1}", (crid.HasValue ? crid.Value.ToString() : "null"), (acct.HasValue ? acct.Value.ToString() : "null")));
            string accountLogo = "";
            string accountName = "";
            string accountAddress = "";
            string privacyPolicy = "";
            string WebsiteURL = "";
            string viewName = "CampaignUnsubscribe";

            if (crid.HasValue && crid > 0 && acct.HasValue && acct.Value > 0)
            {
                GetAccountUnsubscribeViewByAccountIdResponse accountViewResponse = accountSettingsService.GetAccountUnsubscribeView(new GetAccountUnsubscribeViewByAccountIdRequest() { AccountID = acct.Value });
                if (accountViewResponse.accountUnsubscribeViewMap != null && accountViewResponse.accountUnsubscribeViewMap.AccountID == acct.Value)
                {
                    viewName = "~/Views/UnSubscribeViews/" + accountViewResponse.accountUnsubscribeViewMap.ViewName + ".cshtml";
                }

                ApplicationServices.Messaging.Accounts.GetAccountImageStorageNameResponse response = accountService.GetStorageName(new ApplicationServices.Messaging.Accounts.GetAccountImageStorageNameRequest()
                {
                    AccountId = acct.Value
                });
                if (response != null && response.AccountLogoInfo != null)
                {
                    if (!String.IsNullOrEmpty(response.AccountLogoInfo.StorageName))
                        accountLogo = urlService.GetUrl(acct.Value, ImageCategory.AccountLogo, response.AccountLogoInfo.StorageName);
                    if (!String.IsNullOrEmpty(response.AccountLogoInfo.AccountName))
                        accountName = response.AccountLogoInfo.AccountName;
                    privacyPolicy = response.AccountLogoInfo.PrivacyPolicy ?? privacyPolicy;
                    WebsiteURL = response.AccountLogoInfo.WebsiteURL ?? WebsiteURL;
                }
                
                accountAddress = accountService.GetPrimaryAddress(new ApplicationServices.Messaging.Accounts.GetAddressRequest()
                {
                    AccountId = acct.Value
                }).Address;

                ViewBag.accountId = acct.Value;
                ViewBag.accountName = accountName;
                ViewBag.accountLogo = accountLogo;
                ViewBag.accountAddress = accountAddress;
                ViewBag.privacyPolicy = privacyPolicy;
                ViewBag.WebsiteURL = WebsiteURL;

                CampaignRecipient recipient = campaignService.GetCampaignRecipient(new
                    GetCampaignRecipientRequest()
                {
                    CampaignRecipientId = crid.Value,
                    AccountId = acct.Value
                }).CampaignRecipient;

                if (recipient != null)
                {
                    Logger.Current.Informational("Valid Contact" + crid);
                    ViewBag.contactId = recipient.ContactID;
                    ViewBag.emailId = recipient.To;
                    ViewBag.campaignId = recipient.CampaignID;
                    ViewBag.workflowId = recipient.WorkflowId;
                    ViewBag.ValidContact = true;
                }
                else
                {
                    ViewBag.ValidContact = false;
                    Logger.Current.Informational("Invalid Contact" + crid);
                }
            }
            else if (crid == 0 && acct != null && acct != 0)
            {
                ApplicationServices.Messaging.Accounts.GetAccountImageStorageNameResponse response = accountService.GetStorageName(new ApplicationServices.Messaging.Accounts.GetAccountImageStorageNameRequest()
                {
                    AccountId = acct.Value
                });
                if (!String.IsNullOrEmpty(response.AccountLogoInfo.StorageName))
                    accountLogo = urlService.GetUrl(acct.Value, ImageCategory.AccountLogo, response.AccountLogoInfo.StorageName);
                else
                    accountLogo = "";
                accountName = response.AccountLogoInfo.AccountName;
                privacyPolicy = response.AccountLogoInfo.PrivacyPolicy;
                accountAddress = accountService.GetPrimaryAddress(new ApplicationServices.Messaging.Accounts.GetAddressRequest()
                {
                    AccountId = acct.Value
                }).Address;
            }
            else
            {
                ViewBag.ValidContact = false;
            }
            return View(viewName);
        }

        /// <summary>
        /// Gets the campaign links.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.Campaigns, AppOperations.Read)]
        public JsonResult GetCampaignLinks(IEnumerable<int> campaignId)
        {
            int accountID = UserExtensions.ToAccountID(this.Identity);
            GetLinkUrlsResponse response = campaignService.GetCampaignLinks(new GetLinkUrlRequest()
            {
                AccountId = accountID,
                CampaignIDs = campaignId
            });
            return Json(new
            {
                success = true,
                response.CampaignLinks
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the campaign themes.
        /// </summary>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.Campaigns, AppOperations.Read)]
        public JsonResult GetCampaignThemes()
        {
            int accountID = UserExtensions.ToAccountID(this.Identity);
            int userID = UserExtensions.ToUserID(this.Identity);
            GetCampaignThemesResponse response = campaignService.GetCampaignThemes(new GetCampaignThemesRequest()
            {
                AccountId = accountID,
                RequestedBy = userID
            });
            return Json(response.Themes, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Quicks the search campaign redirection.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns></returns>
        [Route("quicksearchcampaignredirection")]
        public ActionResult QuickSearchCampaignRedirection(int campaignId)
        {
            GetCampaignStatusRequest request = new GetCampaignStatusRequest()
            {
                campaignId = campaignId
            };
            GetCampaignStatusResponse response = campaignService.GetCampaignStatus(request);
            if (response.campaignStatus != null)
            {
                if (response.campaignStatus == CampaignStatus.Sent || response.campaignStatus == CampaignStatus.Queued || response.campaignStatus == CampaignStatus.Failure)
                {
                    return RedirectToAction("CampaignStatistics", new
                    {
                        campaignId = campaignId
                    });
                }
                else
                {
                    return RedirectToAction("EditCampaign", new
                    {
                        campaignId = campaignId
                    });
                }
            }
            else
                return RedirectToAction("NotFound", "error");
        }

        /// <summary>
        /// Gets the default campaign service provider.
        /// </summary>
        /// <returns></returns>
        public JsonResult GetDefaultCampaignServiceProvider()
        {
            GetServiceProviderSenderEmailResponse response = new GetServiceProviderSenderEmailResponse();
            var providerDetails = campaignService.GetDefaultBulkEmailProvider(new GetServiceProviderSenderEmailRequest()
            {
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID()
            });
            response.SenderEmail = providerDetails.SenderEmail;
            response.SenderName = providerDetails.SenderName;
            return Json(new
            {
                success = true,
                response = response
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the communication providers.
        /// </summary>
        /// <returns></returns>
        [Route("getcommunicationproviders")]
        public JsonResult GetCommunicationProviders()
        {
            Logger.Current.Verbose("Request received to fetch campaign service providers for accountid: " + this.Identity.ToAccountID());
            GetCommunicatioProvidersRequest request = new GetCommunicatioProvidersRequest
            {
                AccountId = this.Identity.ToAccountID(),
                FromCache = true
            };
            GetCommunicatioProvidersResponse response = communicationService.GetCommunicationProviders(request);
            Logger.Current.Informational("Response received for accountid: " + this.Identity.ToAccountID() + ". Found " + response.RegistrationListViewModel.Count() + " Service providers");
            return Json(new
            {
                success = true,
                response = response
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the active campaign list.
        /// </summary>
        /// <returns></returns>
        public JsonResult GetActiveCampaignList()
        {
            byte accountstatus = 0;
            if (!String.IsNullOrEmpty(""))
                accountstatus = Convert.ToByte("");
            SearchCampaignsResponse response = campaignService.GetAllCampaigns(new SearchCampaignsRequest()
            {
                Query = "",
                Limit = 10,
                PageNumber = 1,
                ShowingFieldType = accountstatus,
                SortFieldType = ContactSortFieldType.CampaignClickrate,
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID()
            });
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Sends the test email.
        /// </summary>
        /// <param name="sendMailViewModel">The send mail view model.</param>
        /// <param name="serviceProviderId">The service provider identifier.</param>
        /// <returns></returns>
        public JsonResult SendTestEmail(string sendMailViewModel, int? serviceProviderId, bool hasDisCliamer)
        {
            Logger.Current.Verbose("Request received to send a test email");
            SendMailViewModel viewModel = JsonConvert.DeserializeObject<SendMailViewModel>(sendMailViewModel);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(viewModel.Body);
            var nodes = doc.DocumentNode.SelectNodes("//div[@class='st-layout']");
            if (nodes.IsAny())
            {
                foreach (HtmlNode node in nodes)
                {
                    node.Remove();
                }

                viewModel.Body = doc.DocumentNode.InnerHtml;
            }

            if (!string.IsNullOrEmpty(viewModel.Body))
            {
                viewModel.Body = ReplacingSpecialCharacterWithTheirCode(viewModel.Body);
            }

            SendTestEmailResponse response = campaignService.SendTestEmail(new SendTestEmailRequest()
            {
                MailViewModel = viewModel,
                ServiceProviderID = serviceProviderId,
                AccountId = this.Identity.ToAccountID(),
                HasDisCliamer = hasDisCliamer
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
        /// Gets the templates for emails.
        /// </summary>
        /// <returns></returns>
        public JsonResult GetTemplatesForEmails()
        {
            GetCampaignTemplatesRequest request = new GetCampaignTemplatesRequest();
            request.AccountId = this.Identity.ToAccountID();
            GetCampaignTemplatesResponse resposne = campaignService.GetCampaignTemplatesForEmails(request);
            return Json(new
            {
                success = true,
                response = resposne.Templates
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Queue campaign
        /// </summary>
        /// <param name="viewModel">Properties of a campaign</param>
        /// <returns>queuedly stated campaign </returns>

        [Route("Campaign/Queue")]
        [HttpPut]
        public JsonResult QueueCampaign(string campaignViewModel)
        {
            CampaignViewModel viewModel = JsonConvert.DeserializeObject<CampaignViewModel>(campaignViewModel);
            if (!string.IsNullOrEmpty(viewModel.HTMLContent))
            {
                viewModel.HTMLContent = ReplacingSpecialCharacterWithTheirCode(viewModel.HTMLContent);
            }

            viewModel.ScheduleTime = viewModel.ScheduleTimeUTC;
            if (viewModel.ScheduleTime != null)
            {
                viewModel.ScheduleTime = viewModel.ScheduleTime.Value.ToUserUtcDateTimeV2();
            }
            
            viewModel.LastUpdatedBy = this.Identity.ToUserID();
            viewModel.LastUpdatedOn = DateTime.Now.ToUniversalTime();
            viewModel.Posts = viewModel.GetPosts();
            foreach (var post in viewModel.Posts)
            {
                post.UserID = viewModel.CreatedBy;
                post.CampaignID = 0;
            }
            QueueCampaignResponse queueResponse =
                campaignService.QueueCampaign(new QueueCampaignRequest() { CampaignViewModel = viewModel, AccountId = this.Identity.ToAccountID() });
            return Json(new
            {
                success = queueResponse.Exception != null ? false : true,
                response = queueResponse
            }, JsonRequestBehavior.AllowGet);
        }

        [Route("getactionlinks")]
        [HttpGet]
        public JsonResult GetWorkflowCampaignActionLinks()
        {
            Logger.Current.Verbose("CampaignController/GetWorkflowCampaignActionLinks. AccountId: " + this.Identity.ToAccountID());
            var campaignLinks = campaignService.GetWorkflowCampaignActionLinks(new GetWorkflowLinkActionsRequest() { AccountId = this.Identity.ToAccountID() }).CampaignLinks;
            return Json(new
            {
                success = true,
                response = campaignLinks
            }, JsonRequestBehavior.AllowGet);
        }
        [Route("campaigns/litmusresults/{campaignId}")]
        //[SmarttouchAuthorize(AppModules.Campaigns, AppOperations.Read)]
        //[MenuType(MenuCategory.Undefined, MenuCategory.LeftMenuCRM)]
        //[OutputCache(Duration = 30)]
        [HttpGet]
        public ActionResult LitmusCheckResults(int campaignId)
        {
            var maps = campaignService.GetCampaignLitmusMap(new GetCampaignLitmusMapRequest()
                {
                    AccountId = this.Identity.ToAccountID(),
                    CampaignId = campaignId,
                    RequestedBy = 0
                });
            ViewBag.CampaignID = campaignId;
            if (maps.CampaignLitmusMaps.Any())
                ViewBag.EmailGuid = maps.CampaignLitmusMaps.OrderByDescending(m => m.LastModifiedOn).FirstOrDefault().LitmusId;
            else
            {
                return RedirectToAction("NotFound", "Error", new ErrorViewModel()
                    {
                        Message="Requested campaign has no litmus test results found."
                    });
            }
                
            
            return View("LitmusResults", campaignId);
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

    }


    public class ClickResponse : ApplicationServices.Messaging.ServiceResponseBase
    {
        public bool Success { get; set; }
        public string Acknowledgement { get; set; }
        public CampaignContactActivity ActivityType { get; set; }
    }
}
