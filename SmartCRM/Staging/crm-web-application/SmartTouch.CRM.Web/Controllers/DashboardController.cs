using LandmarkIT.Enterprise.Extensions;
using SmartTouch.CRM.ApplicationServices.Messaging.Action;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.Dashboard;
using SmartTouch.CRM.ApplicationServices.Messaging.MarketingMessages;
using SmartTouch.CRM.ApplicationServices.Messaging.Reports;
using SmartTouch.CRM.ApplicationServices.Messaging.User;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Web.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;

namespace SmartTouch.CRM.Web.Controllers
{
    public class DashboardController : SmartTouchController
    {
        IUserService userService;
        ITourService tourService;
        IContactService contactService;
        ICachingService cachingService;
        IReportService reportService;
        IActionService actionService;
        IMarketingMessageService marketingMessageService;
        public DashboardController(IUserService userService, ITourService tourService, IContactService contactService,
            ICachingService cachingService, IReportService reportService, IActionService actionService, IMarketingMessageService marketingMessageService)
        {
            this.userService = userService;
            this.tourService = tourService;
            this.contactService = contactService;
            this.cachingService = cachingService;
            this.reportService = reportService;
            this.actionService = actionService;
            this.marketingMessageService = marketingMessageService;
        }

        //
        // GET: /DashBoard/
        public ActionResult Index()
        {
            return View();
        }

        [Route("dashboard")]
        [SmarttouchAuthorize(AppModules.Dashboard, AppOperations.Read)]
        [MenuType(MenuCategory.Dashboard, MenuCategory.LeftMenuCRM)]
        [OutputCache(Duration = 30)]
        public ActionResult DashboardList()
        {
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            var usersPermissions = cachingService.GetUserPermissions(Thread.CurrentPrincipal.Identity.ToAccountID());
            List<byte> userModules = usersPermissions.Where(s => s.RoleId == (short)Thread.CurrentPrincipal.Identity.ToRoleID()).Select(s => s.ModuleId).ToList();
            var IsSTadmin = this.Identity.IsSTAdmin();
            if (IsSTadmin)
                ViewBag.IsSTadmin = true;
            if (userModules.Contains((byte)AppModules.Reports))
                ViewBag.ReportPermissions = true;

            bool isbdx = userService.GetAccountSubscription(this.Identity.ToAccountID());
            ViewBag.IsBDXAccount = isbdx;
            return View("Dashboard");
        }

        [Route("GetUserCalender")]
        [HttpGet]
        public JsonResult GetUserCalender(string startDate, string endDate)
        {
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan startDateTimSpan = TimeSpan.FromMilliseconds(long.Parse(startDate));
            start = start.Add(startDateTimSpan);

            TimeSpan endDateTimSpan = TimeSpan.FromMilliseconds(long.Parse(endDate));
            DateTime end = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            end = end.Add(endDateTimSpan);

            GetUserCalenderRequest request = new GetUserCalenderRequest()
            {
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID(),
                AccountId = this.Identity.ToAccountID(),
                StartDate = start,
                EndDate = end
            };
            GetUserCalenderResponse response = userService.GetUserCalender(request);
            response.CalenderSlots.ToList().ForEach
                (t =>
                {
                    t.start = t.start.ToUtc();
                    t.end = t.start.AddHours(1);
                });
            return Json(new { success = true, response = response.CalenderSlots }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetTrafficBySourceAreaChartDetails()
        {
            GetDashboardChartDetailsRequest request = new GetDashboardChartDetailsRequest
            {
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID(),
                UserId = this.Identity.ToUserID(),
                IsSTadmin = this.Identity.IsSTAdmin(),
                FromDate = DateTime.Now.AddDays(-30),
                ToDate = DateTime.Now
            };

            request.FromDate = request.FromDate.Date;
            request.FromDate = ToUserUtcDateTime(request.FromDate.Date);
            request.ToDate = Convert.ToDateTime(request.ToDate.Date.AddHours(23).AddMinutes(59));
            request.ToDate = ToUserUtcDateTime(request.ToDate);


            GetDashboardChartDetailsResponse response = tourService.GetToursBySourceAreaChartDetails(request);
            if (response.ChartDetailsViewModel.Chart1Details != null)
            {

                response.ChartDetailsViewModel.ReportId = GetReportId(Reports.TrafficBySource);
            }
            return Json(new { success = true, response = response.ChartDetailsViewModel }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetTrafficBySourcePieChartDetails()
        {
            GetDashboardChartDetailsRequest request = new GetDashboardChartDetailsRequest
            {
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID(),
                UserId = this.Identity.ToUserID(),
                IsSTadmin = this.Identity.IsSTAdmin(),
                FromDate = DateTime.Now.ToUniversalTime().AddDays(-30),
                ToDate = DateTime.Now.ToUniversalTime()
            };
            GetDashboardChartDetailsResponse response = tourService.GetToursBySourcePieChartDetails(request);
            if (response.ChartDetailsViewModel.Chart1Details != null)
            {

                response.ChartDetailsViewModel.ReportId = GetReportId(Reports.TrafficBySource);
            }
            return Json(new { success = true, response = response.ChartDetailsViewModel }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetTrafficByTypeBarChartDetails()
        {
            GetDashboardChartDetailsRequest request = new GetDashboardChartDetailsRequest
            {
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID(),
                UserId = this.Identity.ToUserID(),
                IsSTadmin = this.Identity.IsSTAdmin(),
                FromDate = DateTime.Now.ToUniversalTime().AddDays(-30),
                ToDate = DateTime.Now.ToUniversalTime()
            };
            GetDashboardChartDetailsResponse response = tourService.GetToursByTypeBarChartDetails(request);
            if (response.ChartDetailsViewModel.Chart1Details != null)
            {
                response.ChartDetailsViewModel.ReportId = GetReportId(Reports.TrafficByType);
            }
            return Json(new { success = true, response = response.ChartDetailsViewModel }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetTrafficByTypeFunnelChartDetails()
        {
            GetDashboardChartDetailsRequest request = new GetDashboardChartDetailsRequest
            {
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID(),
                UserId = this.Identity.ToUserID(),
                IsSTadmin = this.Identity.IsSTAdmin(),
                FromDate = DateTime.Now.ToUniversalTime(),
                ToDate = DateTime.Now.ToUniversalTime().AddDays(30)
            };
            GetDashboardChartDetailsResponse response = tourService.GetToursByTypeFunnelChartDetails(request);
            if (response.ChartDetailsViewModel.Chart2Details != null)
            {
                response.ChartDetailsViewModel.ReportId = GetReportId(Reports.OpportunityPipeline);
            }

            return Json(new { success = true, response = response.ChartDetailsViewModel }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetNewLeadsChartDetails()
        {
            GetUsersResponse users = contactService.GetUsers(new GetUsersRequest() { AccountID = this.Identity.ToAccountID(), UserId = this.Identity.ToUserID(), IsSTadmin = this.Identity.IsSTAdmin() });

            ReportViewModel viewModel = new ReportViewModel()
            {
                CustomStartDate = DateTime.Now.AddDays(-30),
                CustomEndDate = DateTime.Now,
                OwnerIds = users.Owner.Select(owner => owner.OwnerId).ToArray(),
                IsCompared = true,
                CustomStartDatePrev = DateTime.Now.ToUniversalTime().AddDays(-60),
                CustomEndDatePrev = DateTime.Now.ToUniversalTime().AddDays(-30),
                IsDashboard = true

            };

            viewModel.CustomStartDate = viewModel.CustomStartDate.Date;
            viewModel.CustomStartDate = ToUserUtcDateTime(viewModel.CustomStartDate.Date);
            viewModel.CustomEndDate = Convert.ToDateTime(viewModel.CustomEndDate.Date.AddHours(23).AddMinutes(59));
            viewModel.CustomEndDate = ToUserUtcDateTime(viewModel.CustomEndDate);

            StandardReportRequest request = new StandardReportRequest
            {
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID(),
                ReportViewModel = viewModel,
                UserID = this.Identity.ToUserID(),
                IsSTadmin = this.Identity.IsSTAdmin(),
                RequestedBy = this.Identity.ToUserID()
            };
            StandardReportResponse response = new StandardReportResponse();
            response = reportService.GetNewLeadsVisualizationAsync(request);
            if (response.TopFive != null && response.TopLeads != null)
            {
                response.ReportId = GetReportId(Reports.NewLeads);
            }
            return Json(new { success = true, response = response }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [SmarttouchSessionStateBehaviour(SessionStateBehavior.Required)]
        public ActionResult GetUnTouchedLeadsChartDetails()
        {
            GetUsersResponse users = contactService.GetUsers(new GetUsersRequest() { AccountID = this.Identity.ToAccountID(), UserId = this.Identity.ToUserID(), IsSTadmin = this.Identity.IsSTAdmin() });

            ReportViewModel viewModel = new ReportViewModel()
            {
                CustomStartDate = DateTime.UtcNow.AddDays(-30),
                CustomEndDate = DateTime.UtcNow,
                OwnerIds = users.Owner.Select(owner => owner.OwnerId).ToArray(),
                IsCompared = true,
                CustomStartDatePrev = DateTime.Now.ToUniversalTime().AddDays(-60),
                CustomEndDatePrev = DateTime.Now.ToUniversalTime().AddDays(-30),
                IsDashboard = true

            };

            viewModel.CustomStartDate = viewModel.CustomStartDate.Date;
            viewModel.CustomStartDate = ToUserUtcDateTime(viewModel.CustomStartDate.Date);
            viewModel.CustomEndDate = Convert.ToDateTime(viewModel.CustomEndDate.Date.AddHours(23).AddMinutes(59));
            viewModel.CustomEndDate = ToUserUtcDateTime(viewModel.CustomEndDate);

            StandardReportRequest request = new StandardReportRequest
            {
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID(),
                ReportViewModel = viewModel,
                UserID = this.Identity.ToUserID(),
                IsSTadmin = this.Identity.IsSTAdmin(),
                RequestedBy = this.Identity.ToUserID()
            };
            StandardReportResponse response = new StandardReportResponse();
            response = reportService.GetUntouchedLeadsVisualizationAsync(request);
            Session["NewContactsSearchViewModel"] = response.AdvancedSearchViewModel;
            return Json(new { success = true, response = response }, JsonRequestBehavior.AllowGet);
        }

        public void AddCookie(string cookieName, string Value, int days)
        {
            HttpCookie CartCookie = new HttpCookie(cookieName, Value);
            CartCookie.Expires = DateTime.Now.AddDays(days);
            Response.Cookies.Add(CartCookie);
        }

        private static DateTime ToUserUtcDateTime(DateTime d)
        {
            return d.ToJsonSerailizedDate().ToUserUtcDateTime();
        }

        public JsonResult GetActiveCampaignList()
        {
            ReportViewModel viewModel = new ReportViewModel { CustomStartDate = DateTime.Now.ToUniversalTime().AddDays(-30), CustomEndDate = DateTime.Now.ToUniversalTime() };
            StandardReportResponse response = reportService.GetDashboardcampaignList(new StandardReportRequest
            {
                ReportViewModel = viewModel,
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID(),
                IsSTadmin = this.Identity.IsSTAdmin(),
                UserID = this.Identity.ToUserID()
            });
            if (response.ReportList != null)
                response.ReportId = GetReportId(Reports.CampaignList);
            return Json(new { success = true, response = response }, JsonRequestBehavior.AllowGet);

        }

        [NonAction]
        private int? GetReportId(Reports reportType)
        {
            GetReportsRequest request = new GetReportsRequest { ReportType = reportType, AccountId = this.Identity.ToAccountID() };
            GetReportsResponse response = reportService.GetReportByType(request);
            if (response.Report != null)
                return response.Report.ReportID;
            else
                return null;
        }

        public JsonResult GetHotListContacts()
        {
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());

            ReportViewModel viewModel = new ReportViewModel
            {
                CustomStartDate = DateTime.Now.ToUniversalTime().AddDays(-30),
                CustomEndDate = DateTime.Now.ToUniversalTime(),
                PageNumber = 1,
                ShowTop = 10
            };
            var lifecycleStages = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LifeCycle)
                     .Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true).Select(d => d.DropdownValueID).ToArray();
            viewModel.LifeStageIds = lifecycleStages;

            ReportDataResponse response = reportService.GetHotListData(new ReportDataRequest
            {
                ReportViewModel = viewModel,
                AccountId = this.Identity.ToAccountID(),
                UserId = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID(),
                IsSTAdmin = this.Identity.IsSTAdmin(),
                IsDasboardView = true
            });
            response.ReportId = GetReportId(Reports.HotList);


            response.HotlistGridData.Contacts = response.ContactIds = new List<int>();
            return Json(new { success = true, response = response }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUserCreatedActions(int pageNumber, int limit)
        {
            IEnumerable<ActionViewModel> actions = null;
            var direction = System.ComponentModel.ListSortDirection.Descending;
            GetActionListResponse response = actionService.GetUserCreatedActions(new GetActionListRequest()
            {
                UserIds = new int[] { this.Identity.ToUserID() },
                AccountId = this.Identity.ToAccountID(),
                IsStAdmin = this.Identity.IsSTAdmin(),
                PageNumber = pageNumber,
                Limit = limit,
                Name = null,
                Filter = null,
                FilterByActionType = null,
                StartDate = DateTime.MinValue,
                EndDate = DateTime.MinValue,
                IsDashboard = true,
                SortDirection = direction
            });
            if (response != null)
            {
                response.ActionListViewModel.Each(a => a.RemindOn = (a.RemindOn == null ? null : (DateTime?)a.RemindOn.Value.ToUtcBrowserDatetime()));
                actions = response.ActionListViewModel;
            }
            return Json(new { success = true, response = actions }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllMarketingMessages()
        {
            int accountId = this.Identity.ToAccountID();
            GetAllMarketingMessageContentResponse response = marketingMessageService.GetAllMarketingMessageContents(new GetAllMarketingMessageContentRequest() { AccountID = accountId });
            return Json(new { success = true, response = response.marketingMessagesViewModel }, JsonRequestBehavior.AllowGet);
        }
    }
}