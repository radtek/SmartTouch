using LandmarkIT.Enterprise.Extensions;
using SmartTouch.CRM.ApplicationServices.Messaging.Reports;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.WebService.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.SessionState;

namespace SmartTouch.CRM.WebService.Controllers
{
    public class ReportController : SmartTouchApiController
    {
        readonly IReportService reportService;

        readonly ICachingService cachingService;

        readonly IContactService contactService;

        readonly ITagService tagService;

        readonly ICampaignService campaignService;

        readonly IAccountService accountService;

        /// <summary>
        /// Creating constructor for Report controller for accessing
        /// </summary>
        /// <param name="cachingService"></param>
        /// <param name="reportService"></param>
        /// <param name="contactService"></param>
        /// <param name="tagService"></param>
        /// <param name="campaignService"></param>
        /// <param name="accountService"></param>
        public ReportController(ICachingService cachingService, IReportService reportService, IContactService contactService, ITagService tagService, ICampaignService campaignService, IAccountService accountService)
        {
            this.cachingService = cachingService;
            this.reportService = reportService;
            this.contactService = contactService;
            this.tagService = tagService;
            this.campaignService = campaignService;
            this.accountService = accountService;
        }

        /// <summary>
        /// For New Leads Data.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [Route("NewLeadsList")]
        [HttpPost]
        public async Task<HttpResponseMessage> NewLeadsList(ReportViewModel viewModel)
        {
            viewModel.CustomStartDate = viewModel.CustomStartDate.Date;
            viewModel.CustomStartDate = ToUserUtcDateTime(viewModel.CustomStartDate.Date);
            viewModel.CustomEndDate = Convert.ToDateTime(viewModel.CustomEndDate.Date.AddHours(23).AddMinutes(59));
            viewModel.CustomEndDate = ToUserUtcDateTime(viewModel.CustomEndDate);
            var response = await reportService.GetNewLeadsListAsync(new StandardReportRequest
            {
                RequestedBy = this.UserId,
                AccountId = this.AccountId,
                RoleId = this.RoleId,
                IsSTadmin = this.IsSTAdmin,
                ReportViewModel = viewModel
            });
            //if (viewModel != null)
            //    UpdateLastRunActivity(viewModel.ReportId, viewModel.ReportName);
            //Session["NewContactsSearchViewModel"] = response.AdvancedSearchViewModel;
            //Session["AdvancedSearchVM"] = response.AdvancedSearchViewModel;

            return Request.BuildResponse(response);
        }

        /// <summary>
        /// For Getting A Perticular Report Data Based On Report Type
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        [Route("GetReportData")]
        [HttpPost]
        public HttpResponseMessage GetReportData(ReportViewModel viewmodel)
        {
            viewmodel.CustomStartDate = viewmodel.CustomStartDate != DateTime.MinValue ? ToUserUtcDateTime(viewmodel.CustomStartDate) : DateTime.MinValue;
            viewmodel.CustomEndDate = ToUserUtcDateTime(viewmodel.CustomEndDate);
            viewmodel.CustomStartDatePrev = viewmodel.CustomStartDatePrev != DateTime.MinValue ? ToUserUtcDateTime(viewmodel.CustomStartDatePrev) : DateTime.MinValue;
            viewmodel.CustomEndDatePrev = viewmodel.CustomEndDatePrev != DateTime.MinValue ? ToUserUtcDateTime(viewmodel.CustomEndDatePrev) : DateTime.MinValue;
            ReportDataRequest request = new ReportDataRequest()
            {
                ReportViewModel = viewmodel,
                AccountId = this.AccountId,
                RequestedBy = this.UserId,
                RoleId = this.RoleId
            };
            ReportDataResponse response = new ReportDataResponse();
            if (viewmodel.Type == 'T')
                response = reportService.GetTrafficByTypeData(request);
            if (viewmodel.Type == 'P')
                response = reportService.GetOpportunityPipeline(request);
            if (viewmodel.Type == 'S')
                response = reportService.GetTrafficBySourceData(request);
            if (viewmodel.Type == 'L')
                response = reportService.GetTrafficByLifeCycleData(request);
            if (viewmodel.Type == 'A')
                response = reportService.GetActivityData(request);
            if (viewmodel.Type == 'C')
                response = reportService.GetTrafficByTypeAndLifeCycleData(request);
            // UpdateLastRunActivity(viewmodel.ReportId, viewmodel.ReportName);
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// For Hot List Report Data
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        [Route("GetHotListReportData")]
        [HttpPost]
        public HttpResponseMessage RunHotList(ReportViewModel viewmodel)
        {
            if (viewmodel.PeriodId != 7)
            {
                viewmodel.CustomStartDate = ToUserUtcDateTime(viewmodel.CustomStartDate);
                viewmodel.CustomEndDate = ToUserUtcDateTime(viewmodel.CustomEndDate);
            }
            ReportDataResponse response = reportService.GetHotListData(new ReportDataRequest
            {
                ReportViewModel = viewmodel,
                AccountId = this.AccountId,
                RoleId = this.RoleId,
                RequestedBy = this.UserId,
                UserId = this.UserId,
                IsDasboardView = false
            });
            // UpdateLastRunActivity(viewmodel.ReportId, viewmodel.ReportName);
            //Guid guid = Guid.NewGuid();
            //ViewBag.Guid = guid.ToString();
            //if (response.ContactIds != null)
            //{
            //    AddCookie("ContactsGuid", guid.ToString(), 1);
            //    var contactIds = response.ContactIds;
            //    cachingService.StoreSavedSearchContactIds(guid.ToString(), contactIds);
            //}
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get Report Data By Type
        /// </summary>
        /// <param name="reportType"></param>
        /// <returns></returns>
        [Route("GetReportById")]
        [HttpGet]
        public HttpResponseMessage GetReportByType(byte reportType)
        {
            GetReportsResponse response = reportService.GetReportByType(new GetReportsRequest()
            {
                AccountId = this.AccountId,
                ReportType = (Reports)reportType 
            });

            return Request.BuildResponse(response);
        }

        [Route("GetContactIds")]
        [HttpPost]
        public HttpResponseMessage GetContacts(ReportViewModel viewmodel)
        {
            viewmodel.CustomStartDate = viewmodel.CustomStartDate != DateTime.MinValue ? ToUserUtcDateTime(viewmodel.CustomStartDate) : DateTime.MinValue;
            viewmodel.CustomEndDate = ToUserUtcDateTime(viewmodel.CustomEndDate);
            if (viewmodel.ReportType != Reports.FirstLeadSourceReport && viewmodel.ReportType != Reports.AllLeadSourceReport && viewmodel.ReportType != Reports.DatabaseLifeCycleReport)
            {
                viewmodel.CustomStartDatePrev = viewmodel.CustomStartDatePrev != DateTime.MinValue ? ToUserUtcDateTime(viewmodel.CustomStartDatePrev) : DateTime.MinValue;
                viewmodel.CustomEndDatePrev = viewmodel.CustomEndDatePrev != DateTime.MinValue ? ToUserUtcDateTime(viewmodel.CustomEndDatePrev) : DateTime.MaxValue;
            }
            ReportDataRequest request = new ReportDataRequest()
            {
                ReportViewModel = viewmodel,
                AccountId = this.AccountId,
                RequestedBy = this.UserId,
                RoleId = this.RoleId,
                IsSTAdmin = this.IsSTAdmin
            };
            ReportDataResponse response = new ReportDataResponse();
            if (viewmodel.Type == 'T')
                response = reportService.GetTrafficByTypeContacts(request);
            else if (viewmodel.Type == 'P')
                response = reportService.GetOpportunityPipelineContacts(request);
            else if (viewmodel.Type == 'S')
                response = reportService.GetTrafficBySourceContacts(request);
            else if (viewmodel.Type == 'L')
                response = reportService.GetTrafficByLifeCycleContacts(request);
            else if (viewmodel.Type == 'C')
                response = reportService.GetTrafficByTypeAndLifeCycleContacts(request);
            else if (viewmodel.Type == 'A' && (viewmodel.ActivityModule == "Contacts" || viewmodel.ActivityModule == "Tours" || viewmodel.ActivityModule == "Notes"))
            {
                request.ModuleId = viewmodel.ActivityModule == "Contacts" ? (byte)AppModules.Contacts : viewmodel.ActivityModule == "Tours" ? (byte)AppModules.ContactTours : (byte)AppModules.ContactNotes;
                response = reportService.GetActivityReportContacts(request);
            }
            else if (viewmodel.ReportType == Reports.FirstLeadSourceReport)
                response = reportService.FirstLeadSourceReportContacts(request);
            else if (viewmodel.ReportType == Reports.AllLeadSourceReport)
                response = reportService.AllLeadSourceReportContacts(request);
            else if (viewmodel.ReportType == Reports.DatabaseLifeCycleReport)
                response = reportService.AllDatabaseReportContacts(request);

            return Request.BuildResponse(response);
        }



        private static DateTime ToUserUtcDateTime(DateTime d)
        {
            return d.ToJsonSerailizedDate().ToUserUtcDateTime();
        }

    }
}
