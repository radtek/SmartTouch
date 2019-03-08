using Kendo.Mvc.UI;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using Newtonsoft.Json;
using SmartTouch.CRM.ApplicationServices.Messaging.Action;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.Dashboard;
using SmartTouch.CRM.ApplicationServices.Messaging.Reports;
using SmartTouch.CRM.ApplicationServices.Messaging.User;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.WebService.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SmartTouch.CRM.WebService.Controllers
{
    public class DashboardsController : SmartTouchApiController
    {
        private readonly IUserService userService;
        private readonly ITourService tourService;
        private readonly IContactService contactService;
        private readonly ICachingService cachingService;
        private readonly IReportService reportService;
        private readonly IActionService actionService;

        /// <summary>
        /// Creating constructor for Dashboards controller for accessing
        /// </summary>
        /// <param name="userService"></param>
        /// <param name="tourService"></param>
        /// <param name="contactService"></param>
        /// <param name="cachingService"></param>
        /// <param name="reportService"></param>
        /// <param name="actionService"></param>
        public DashboardsController(IUserService userService, ITourService tourService, IContactService contactService,
            ICachingService cachingService, IReportService reportService, IActionService actionService)
        {
            this.userService = userService;
            this.tourService = tourService;
            this.contactService = contactService;
            this.cachingService = cachingService;
            this.reportService = reportService;
            this.actionService = actionService;
        }

        /// <summary>
        /// Get User Dashboard Settings.
        /// </summary>
        /// <returns></returns>
        [Route("DashboardSetting")]
        [HttpGet]
        public HttpResponseMessage GetUserDashBoardSettings()
        {
            GetDashboardItemsResponse response = reportService.GetDashboardItems(this.UserId);
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Saving User Dashboard Settings
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [Route("SaveDashbordSetting")]
        [HttpPost]
        public HttpResponseMessage SaveUserDashboardSettings(DashboardViewModel viewModel)
        {
            InsertUserDashboardSettingsResponse response = new InsertUserDashboardSettingsResponse();
            IEnumerable<DashboardSettingViewModel> ids = viewModel.Settings;
            reportService.InsertUserDashboardSettings(new InsertUserDashboardSettingsRequest()
            {
                UserId = this.UserId,
                DashboardViewModel = ids
            });

            if (response.Exception != null)
            {
                var message = response.Exception.Message.Replace("[|", "").Replace("|]", "");
                response.Exception = new UnsupportedOperationException(message);
            }

            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get New Leads and Top 5 Lead Sources Chart Details
        /// </summary>
        /// <returns></returns>
        [Route("NewLeadAndTop5Sources")]
        [HttpGet]
        public HttpResponseMessage GetNewLeadsChartData()
        {
            GetUsersResponse users = contactService.GetUsers(new GetUsersRequest() { AccountID = this.AccountId, UserId = this.UserId, IsSTadmin = this.IsSTAdmin });

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
                AccountId = this.AccountId,
                RoleId = this.RoleId,
                ReportViewModel = viewModel,
                UserID = this.UserId,
                IsSTadmin = this.IsSTAdmin,
                RequestedBy = this.UserId
            };
            StandardReportResponse response = new StandardReportResponse();
            response = reportService.GetNewLeadsVisualizationAsync(request);
            if (response.TopFive != null && response.TopLeads != null)
            {
                response.ReportId = GetReportId(Reports.NewLeads);
            }

            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Getting UnTouched Leads Chart Details
        /// </summary>
        /// <returns></returns>
        [Route("UntouchedLeads")]
        [HttpGet]
        public HttpResponseMessage GetUnTouchedLeadsChartData()
        {
            GetUsersResponse users = contactService.GetUsers(new GetUsersRequest() { AccountID = this.AccountId, UserId = this.UserId, IsSTadmin = this.IsSTAdmin });

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
                AccountId = this.AccountId,
                RoleId = this.RoleId,
                ReportViewModel = viewModel,
                UserID = this.UserId,
                IsSTadmin = this.IsSTAdmin,
                RequestedBy = this.UserId
            };
            StandardReportResponse response = new StandardReportResponse();
            response = reportService.GetUntouchedLeadsVisualizationAsync(request);
            //Session["NewContactsSearchViewModel"] = response.AdvancedSearchViewModel;
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get Tour Chat Details
        /// </summary>
        /// <returns></returns>
        [Route("ToursChartData")]
        [HttpGet]
        public HttpResponseMessage GetTrafficBySourceAreaChartData()
        {
            GetDashboardChartDetailsRequest request = new GetDashboardChartDetailsRequest
            {
                AccountId = this.AccountId,
                RoleId = this.RoleId,
                UserId = this.UserId,
                IsSTadmin = this.IsSTAdmin,
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

            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get Top 5 Tour Sources Chat Details.
        /// </summary>
        /// <returns></returns>
        [Route("TourSources")]
        [HttpGet]
        public HttpResponseMessage GetTrafficBySourcePieChartData()
        {
            GetDashboardChartDetailsRequest request = new GetDashboardChartDetailsRequest
            {
                AccountId = this.AccountId,
                RoleId = this.RoleId,
                UserId = this.UserId,
                IsSTadmin = this.IsSTAdmin,
                FromDate = DateTime.Now.ToUniversalTime().AddDays(-30),
                ToDate = DateTime.Now.ToUniversalTime()
            };
            GetDashboardChartDetailsResponse response = tourService.GetToursBySourcePieChartDetails(request);
            if (response.ChartDetailsViewModel.Chart1Details != null)
            {
                response.ChartDetailsViewModel.ReportId = GetReportId(Reports.TrafficBySource);
            }

            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get Hot List Data
        /// </summary>
        /// <returns></returns>
        [Route("HotList")]
        [HttpGet]
        public HttpResponseMessage GetHotListLeads()
        {
            var dropdownValues = cachingService.GetDropdownValues(this.AccountId);

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
                AccountId = this.AccountId,
                UserId = this.UserId,
                RoleId = this.RoleId,
                IsSTAdmin = this.IsSTAdmin,
                IsDasboardView = true
            });
            response.ReportId = GetReportId(Reports.HotList);
            response.HotlistGridData.Contacts = response.ContactIds = new List<int>();
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Getting User Created Actions
        /// </summary>
        /// <returns></returns>
        [Route("MyActions")]
        [HttpGet]
        public HttpResponseMessage GetCreatedActionsList()
        {
            IEnumerable<ActionViewModel> actions = null;
            string userName = string.Empty;
            GetActionListResponse response = actionService.GetUserCreatedActions(new GetActionListRequest()
            {
                RequestedBy = this.UserId,
                UserIds = new int[] { this.UserId },
                AccountId = this.AccountId,
                IsStAdmin = this.IsSTAdmin,
                PageNumber = 1,
                Limit = 10,
                Name = null,
                Filter =  "2",
                SortDirection = System.ComponentModel.ListSortDirection.Descending,
                SortField = "ActionDate",
                IsDashboard = true,
                StartDate =  DateTime.MinValue,
                EndDate =  DateTime.MinValue,
                FilterByActionType = string.Empty
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
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get User Caleder Details
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [Route("MyCalender")]
        [HttpGet]
        public HttpResponseMessage GetUserCalenderData(string startDate, string endDate)
        {
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan startDateTimSpan = TimeSpan.FromMilliseconds(long.Parse(startDate));
            start = start.Add(startDateTimSpan);

            TimeSpan endDateTimSpan = TimeSpan.FromMilliseconds(long.Parse(endDate));
            DateTime end = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            end = end.Add(endDateTimSpan);

            GetUserCalenderRequest request = new GetUserCalenderRequest()
            {
                RequestedBy = this.UserId,
                RoleId = this.RoleId,
                AccountId = this.AccountId,
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
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get Recent Web Visists
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="lmit"></param>
        /// <param name="includeViewWebVisits"></param>
        /// <returns></returns>
        [Route("RecentWebVisits")]
        [HttpGet]
        public HttpResponseMessage GetWebVisits(byte pageNumber, int lmit, bool includeViewWebVisits)
        {
            GetRecentWebVisitsRequest request = new GetRecentWebVisitsRequest();
            request.PageNumber = (byte)pageNumber;
            request.Limit = lmit;
            request.IncludePreviouslyRead = includeViewWebVisits;
            request.RequestedBy = this.UserId;
            request.AccountId = this.AccountId;
            request.RoleId = this.RoleId;
            GetRecentWebVisitsResponse response = userService.GetRecentWebVisits(request);
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get My Communications Data
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        [Route("MyCommunications")]
        [HttpGet]
        public HttpResponseMessage GetMyCommunications(byte period)
        {
            DateTime startDate = DateTime.UtcNow;
            DateTime endDate = DateTime.UtcNow;
            if (period == 0)
                startDate = ToUserUtcDateTime(startDate.AddDays(-7).Date);
            else
                startDate = ToUserUtcDateTime(startDate.AddDays(-30).Date);

            MyCommunicationResponse response = userService.GetMyCommunicationDetails(new MyCommunicationRequest()
            {
                AccountId = this.AccountId,
                UserId = this.UserId,
                StartDate = startDate,
                EndDate = endDate
            });

            return Request.BuildResponse(response);
        }

        private static DateTime ToUserUtcDateTime(DateTime d)
        {
            return d.ToJsonSerailizedDate().ToUserUtcDateTime();
        }

        [NonAction]
        private int? GetReportId(Reports reportType)
        {
            GetReportsRequest request = new GetReportsRequest { ReportType = reportType, AccountId = this.AccountId };
            GetReportsResponse response = reportService.GetReportByType(request);
            if (response.Report != null)
                return response.Report.ReportID;
            else
                return null;
        }


    }
}
