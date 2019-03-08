using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.Logging;
using Newtonsoft.Json;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.Reports;
using SmartTouch.CRM.ApplicationServices.Messaging.Tags;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Web.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;

namespace SmartTouch.CRM.Web.Controllers
{
    public class ReportsController : SmartTouchController
    {
        readonly IReportService reportService;

        readonly ICachingService cachingService;

        readonly IContactService contactService;

        readonly ITagService tagService;

        readonly ICampaignService campaignService;

        readonly IAccountService accountService;

        public ReportsController(ICachingService cachingService, IReportService reportService, IContactService contactService, ITagService tagService, ICampaignService campaignService, IAccountService accountService)
        {
            this.cachingService = cachingService;
            this.reportService = reportService;
            this.contactService = contactService;
            this.tagService = tagService;
            this.campaignService = campaignService;
            this.accountService = accountService;
        }

        [Route("editreport")]
        [SmarttouchAuthorize(AppModules.Reports, AppOperations.Edit)]
        [MenuType(MenuCategory.EditReport, MenuCategory.LeftMenuCRM)]
        public ActionResult EditReport(string reportName, byte reportType, int reportId, bool runReportResults)
        {
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            bool isAccountAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, this.Identity.ToAccountID());
            ViewBag.IsPrivateSearch = (isPrivate && !isAccountAdmin);
            ViewBag.RunReportResults = runReportResults;
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            ReportViewModel reportviewModel = new ReportViewModel();
            reportviewModel.ReportId = reportId;
            reportviewModel.ReportName = !string.IsNullOrEmpty(reportName) ? reportName : reportService.GetReportNameByType(new GetReportNameByTypeRequest()
            {
                AccountId = this.Identity.ToAccountID(),
                ReportType = reportType
            }).ReportName;
            if(reportType == (byte)Reports.DatabaseLifeCycleReport)
                reportviewModel.LifecycleStages = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LifeCycle).Select(s => s.DropdownValuesList).ToList().FirstOrDefault();
            else
                reportviewModel.LifecycleStages = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LifeCycle).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);


            reportviewModel.Communities = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.Community).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            reportviewModel.TourTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.TourType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            reportviewModel.LeadSources = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LeadSources).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            reportviewModel.OpportunityStages = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.OpportunityStage).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            ViewBag.ReportType = null;
            ViewBag.ReportId = null;
            reportviewModel.ReportType = (Reports)reportType;
            if (runReportResults == true && reportType == (byte)Reports.OpportunityPipeline)
            {
                ViewBag.IsPipeline = true;
                reportviewModel.OwnerId = this.Identity.ToUserID();
                return View("EditReport", reportviewModel);
            }
            if (reportType == (byte)Reports.NewLeads)
            {
                return View("NewLeads", reportviewModel);
            }
            if (reportType == (byte)Reports.HotList)
            {
                return View("HotList", reportviewModel);
            }
            else if (reportType == (byte)Reports.CampaignList)
            {
                return View("CampaignList", reportviewModel);
            }
            else if (reportType == (byte)Reports.OpportunityPipeline)
            {
                ViewBag.IsPipeline = true;
                return View("EditReport", reportviewModel);
            }
            else if (reportType == (byte)Reports.TrafficByType)
            {
                ViewBag.IsType = true;
                return View("EditReport", reportviewModel);
            }
            else if (reportType == (byte)Reports.TrafficByLifecycle)
            {
                ViewBag.IsLifeCycle = true;
                return View("EditReport", reportviewModel);
            }
            else if (reportType == (byte)Reports.TrafficBySource)
            {
                ViewBag.IsSource = true;
                return View("EditReport", reportviewModel);
            }
            else if (reportType == (byte)Reports.TrafficByTypeAndLifecycle)
            {
                ViewBag.IsTypeAndLifecycle = true;
                return View("EditReport", reportviewModel);
            }
            else if (reportType == (byte)Reports.Activity)
            {
                ViewBag.IsActivity = true;
                reportviewModel.OwnerId = this.Identity.ToUserID();
                return View("EditReport", reportviewModel);
            }
            else if (reportType == (byte)Reports.FirstLeadSourceReport)
            {
                return View("_FirstLeadSourceReport", reportviewModel);
            }
            else if (reportType == (byte)Reports.AllLeadSourceReport)
            {
                return View("_AllLeadSourceReport", reportviewModel);
            }
            else if (reportType == (byte)Reports.FormsCountSummary)
            {
                return View("FormsCountSummary", reportviewModel);
            }
            else if (reportType == (byte)Reports.BDXFreemiumCustomLeadReport)
            {
                return View("BDXFreemiumCustomLeadReport", reportviewModel);
            }
            else if (reportType == (byte)Reports.NightlyStatus || reportType == (byte)Reports.NightlyCampaign || reportType == (byte)Reports.BouncedEmail || reportType == (byte)Reports.LoginFrequency)
            {
                reportviewModel.AccountsList = this.GetAllAccounts(2).Accounts;    //SubscriptionId -> 2
                return View("NightlyStatusReport", reportviewModel);
            }
            else if (reportType == (byte)Reports.TagsReport)
            {
                ViewBag.tagId = 0;
                TagViewModel viewModel = new TagViewModel();
                UpdateLastRunActivity(reportviewModel.ReportId, "Tags");
                ViewBag.ReportType = (byte)Reports.TagsReport;
                ViewBag.ReportId = reportId;
                return View("TagsReport", viewModel);
            }
            else if (reportType == (byte)Reports.BDXCustomLeadReport)
            {
                return View("BDXCustomLeadReport", reportviewModel);
            }
            else
                switch (reportType)
                {
                    case (byte)Reports.WebVisits:
                        {
                            return View("WebVisitsReport", reportviewModel);
                        }
                    case (byte)Reports.CampaignReEngagementReport:
                        {
                            ViewBag.ReportId = reportId;
                            return View("ReEngagementReport", reportviewModel);
                        }
                    case (byte)Reports.DatabaseLifeCycleReport:
                        {
                            return View("DatabaseLifeCyleReport", reportviewModel);
                        }
                    case (byte)Reports.ToursByContactsReport:
                        {
                            return View("TourByContactsReport", reportviewModel);
                        }
                }

            return null;
        }

        [Route("editcustomreport")]
        [SmarttouchAuthorize(AppModules.Reports, AppOperations.Edit)]
        [MenuType(MenuCategory.EditReport, MenuCategory.LeftMenuCRM)]
        public ActionResult EditCustomReport(string ReportName, int ReportId)
        {
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            ReportViewModel viewModel = new ReportViewModel();
            viewModel.ReportId = ReportId;
            viewModel.ReportName = ReportName;
            return View("CustomReport", viewModel);
        }

        [Route("editstatusreport")]
        [SmarttouchAuthorize(AppModules.Reports, AppOperations.Edit)]
        [MenuType(MenuCategory.EditReport, MenuCategory.LeftMenuAccountConfiguration)]
        public ActionResult EditStatusReport(string ReportName, byte reportType, int ReportId)
        {
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            ViewBag.RunReportResults = true;
            ReportViewModel viewModel = new ReportViewModel();
            viewModel.ReportId = ReportId;
            viewModel.ReportType = (Reports)reportType;
            viewModel.AccountsList = this.GetAllAccounts(2).Accounts;    //SubscriptionId -> 2
            viewModel.ReportName = !string.IsNullOrEmpty(ReportName) ? ReportName : reportService.GetReportNameByType(new GetReportNameByTypeRequest()
            {
                AccountId = this.Identity.ToAccountID(),
                ReportType = reportType
            }).ReportName;
            return View("NightlyStatusReport", viewModel);
        }

        [Route("reports")]
        [SmarttouchAuthorize(AppModules.Reports, AppOperations.Read)]
        [MenuType(MenuCategory.Reports, MenuCategory.LeftMenuCRM)]
        [OutputCache(Duration = 30)]
        public ActionResult ReportList()
        {
            ViewBag.IsSTAdminReports = false;
            return View("ReportsList", this.GetViewModel());
        }

        //Account's reports
        [Route("customreports")]
        [SmarttouchAuthorize(AppModules.Reports, AppOperations.Read)]
        [MenuType(MenuCategory.Reports, MenuCategory.LeftMenuAccountConfiguration)]
        [OutputCache(Duration = 30)]
        public ActionResult CustomReportList()
        {
            ViewBag.IsSTAdminReports = true;
            return View("ReportsList", this.GetViewModel());            
        }

        private ReportListViewModel GetViewModel()
        {
            ReportListViewModel viewmodel = new ReportListViewModel();
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            ViewBag.reportId = 0;
            short ItemsPerPage = default(short);
            CustomReportDataResponse response = new CustomReportDataResponse();
            response = reportService.GetCustomReports(new CustomReportDataRequest()
            {
                AccountId = this.Identity.ToAccountID()
            });
            ViewBag.hasCustomReports = response.hasCustomReports;
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            return viewmodel;
        }

        [SmarttouchAuthorize(AppModules.Reports, AppOperations.Read)]
        public ActionResult ReportsViewRead([DataSourceRequest] DataSourceRequest request, string name, string filterdata)
        {
            AddCookie("reportssize", request.PageSize.ToString(), 1);
            AddCookie("reportsnumber", request.Page.ToString(), 1);
            var sortField = request.Sorts.Count > 0 ? request.Sorts.First().Member : GetPropertyName<ReportListEntry, String>(r => r.ReportName);
            var direction = request.Sorts.Count > 0 ? request.Sorts.First().SortDirection : System.ComponentModel.ListSortDirection.Ascending;
            GetReportsResponse response = reportService.GetReportList(new GetReportsRequest()
            {
                Name = name,
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID(),
                PageNumber = request.Page,
                pageSize = request.PageSize,
                SortDirection = direction,
                SortField = sortField,
                Filter = filterdata
            });
            if (response != null)
                response.ReportListViewModel.Reports.ToList().ForEach(a => a.LastRunOn = a.LastRunOn != null ? a.LastRunOn.Value.ToUtcBrowserDatetime() : a.LastRunOn);
            return Json(new DataSourceResult
            {
                Data = response.ReportListViewModel.Reports,
                Total = response.TotalRecordsCount
            }, JsonRequestBehavior.AllowGet);
        }

        [SmarttouchAuthorize(AppModules.Reports, AppOperations.Read)]
        public ActionResult CustomReportsViewRead([DataSourceRequest] DataSourceRequest request, string name)
        {
            AddCookie("reportssize", request.PageSize.ToString(), 1);
            AddCookie("reportsnumber", request.Page.ToString(), 1);
            var sortField = request.Sorts.Count > 0 ? request.Sorts.First().Member : GetPropertyName<ReportListEntry, String>(r => r.ReportName);
            var direction = request.Sorts.Count > 0 ? request.Sorts.First().SortDirection : System.ComponentModel.ListSortDirection.Ascending;
            GetReportsResponse response = reportService.GetCustomReportList(new GetReportsRequest()
            {
                Name = name,
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID(),
                PageNumber = request.Page,
                pageSize = request.PageSize,
                SortDirection = direction,
                SortField = sortField
            });
            return Json(new DataSourceResult
            {
                Data = response.ReportListViewModel.Reports,
                Total = response.TotalRecordsCount
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult WebVisitReport(ReportViewModel viewmodel)
        {
            viewmodel.CustomStartDate = ToUserUtcDateTime(viewmodel.CustomStartDate);
            viewmodel.CustomEndDate = ToUserUtcDateTime(viewmodel.CustomEndDate);
            ReportDataResponse response = reportService.GetWebVisitsReport(new ReportDataRequest
            {
                ReportViewModel = viewmodel,
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID(),
                RequestedBy = this.Identity.ToUserID()
            });
            UpdateLastRunActivity(viewmodel.ReportId, viewmodel.ReportName);
            return Json(new
            {
                success = true,
                response = new DataSourceResult
                {
                    Data = response.WebVisits,
                    Total = response.TotalHits
                }
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RunHotList(ReportViewModel viewmodel)
        {
            if (viewmodel.PeriodId != 7)
            {
                viewmodel.CustomStartDate = ToUserUtcDateTime(viewmodel.CustomStartDate);
                viewmodel.CustomEndDate = ToUserUtcDateTime(viewmodel.CustomEndDate);
            }
            ReportDataResponse response = reportService.GetHotListData(new ReportDataRequest
            {
                ReportViewModel = viewmodel,
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID(),
                RequestedBy = this.Identity.ToUserID(),
                IsDasboardView = false,
                UserId = this.Identity.ToUserID()
            });
            UpdateLastRunActivity(viewmodel.ReportId, viewmodel.ReportName);
            Guid guid = Guid.NewGuid();
            ViewBag.Guid = guid.ToString();
            if (response.ContactIds != null)
            {
                AddCookie("ContactsGuid", guid.ToString(), 1);
                var contactIds = response.ContactIds;
                cachingService.StoreSavedSearchContactIds(guid.ToString(), contactIds);
            }
            return Json(new
            {
                success = true,
                response = new DataSourceResult
                {
                    Data = response.HotlistGridData.HotlistData,
                    Total = response.TotalHits
                }
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetReportData(ReportViewModel viewmodel)
        {
            viewmodel.CustomStartDate = viewmodel.CustomStartDate != DateTime.MinValue ? ToUserUtcDateTime(viewmodel.CustomStartDate) : DateTime.MinValue;
            viewmodel.CustomEndDate = ToUserUtcDateTime(viewmodel.CustomEndDate);
            viewmodel.CustomStartDatePrev = viewmodel.CustomStartDatePrev != DateTime.MinValue ? ToUserUtcDateTime(viewmodel.CustomStartDatePrev) : DateTime.MinValue;
            viewmodel.CustomEndDatePrev = viewmodel.CustomEndDatePrev != DateTime.MinValue ? ToUserUtcDateTime(viewmodel.CustomEndDatePrev) : DateTime.MinValue;
            ReportDataRequest request = new ReportDataRequest()
            {
                ReportViewModel = viewmodel,
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID()
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
            UpdateLastRunActivity(viewmodel.ReportId, viewmodel.ReportName);
            return Json(new
            {
                success = true,
                response = response.ReportData
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFirstLeadSourceReport(ReportViewModel viewModel)
        {
            viewModel.CustomStartDate = ToUserUtcDateTime(viewModel.CustomStartDate);
            viewModel.CustomEndDate = ToUserUtcDateTime(viewModel.CustomEndDate);
            ReportDataRequest request = new ReportDataRequest()
            {
                ReportViewModel = viewModel,
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID()
            };
            ReportDataResponse response = new ReportDataResponse();
            if (viewModel.ReportType == Reports.FirstLeadSourceReport)
                response = reportService.RunFirstLeadSourceReport(request);
            else if (viewModel.ReportType == Reports.AllLeadSourceReport)
                response = reportService.RunAllLeadSourceReport(request);

            UpdateLastRunActivity(viewModel.ReportId, viewModel.ReportName);
            return Json(new
            {
                success = true,
                response = response.ReportData
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCustomReportData(ReportViewModel viewModel)
        {
            viewModel.CustomStartDate = viewModel.CustomStartDate.Date;
            viewModel.CustomStartDate = ToUserUtcDateTime(viewModel.CustomStartDate.Date);
            viewModel.CustomEndDate = Convert.ToDateTime(viewModel.CustomEndDate.Date.AddHours(23).AddMinutes(59));
            viewModel.CustomEndDate = ToUserUtcDateTime(viewModel.CustomEndDate);
            viewModel.CustomStartDate = viewModel.CustomStartDate.ToJSDate();
            viewModel.CustomEndDate = viewModel.CustomEndDate.ToJSDate();
            ReportDataRequest request = new ReportDataRequest()
            {
                ReportViewModel = viewModel,
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID()
            };
            ReportDataResponse response = new ReportDataResponse();
            response = reportService.GetCustomReportData(request);
            UpdateLastRunActivity(viewModel.ReportId, viewModel.ReportName);
            return Json(new
            {
                success = true,
                response = response.ReportData.CustomReportData
            }, JsonRequestBehavior.AllowGet);
        }

        [SmarttouchSessionStateBehaviour(SessionStateBehavior.Required)]
        public async Task<ActionResult> NewLeadsList(ReportViewModel viewModel)
        {
            viewModel.CustomStartDate = viewModel.CustomStartDate.Date;
            viewModel.CustomStartDate = ToUserUtcDateTime(viewModel.CustomStartDate.Date);
            viewModel.CustomEndDate = Convert.ToDateTime(viewModel.CustomEndDate.Date.AddHours(23).AddMinutes(59));
            viewModel.CustomEndDate = ToUserUtcDateTime(viewModel.CustomEndDate);
            var response = await reportService.GetNewLeadsListAsync(new StandardReportRequest
            {
                RequestedBy = this.Identity.ToUserID(),
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID(),
                IsSTadmin = this.Identity.IsSTAdmin(),
                ReportViewModel = viewModel
            });
            if (viewModel != null)
                UpdateLastRunActivity(viewModel.ReportId, viewModel.ReportName);
            Session["NewContactsSearchViewModel"] = response.AdvancedSearchViewModel;
            Session["AdvancedSearchVM"] = response.AdvancedSearchViewModel;
            return Json(new
            {
                success = true,
                response = new DataSourceResult
                {
                    Data = response.ReportList,
                    Total = (int)response.TotalHits
                }
            }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> ExportNewLeadsList(ReportViewModel viewModel)
        {
            viewModel.CustomStartDate = viewModel.CustomStartDate.Date;
            viewModel.CustomStartDate = ToUserUtcDateTime(viewModel.CustomStartDate.Date);
            viewModel.CustomEndDate = Convert.ToDateTime(viewModel.CustomEndDate.Date.AddHours(23).AddMinutes(59));
            viewModel.CustomEndDate = ToUserUtcDateTime(viewModel.CustomEndDate);
            var response = await reportService.GetNewLeadsListAsync(new StandardReportRequest
            {
                RequestedBy = this.Identity.ToUserID(),
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID(),
                IsSTadmin = this.Identity.IsSTAdmin(),
                ReportViewModel = viewModel
            });
            if (viewModel != null)
                UpdateLastRunActivity(viewModel.ReportId, viewModel.ReportName);
            Session["NewContactsSearchViewModel"] = response.AdvancedSearchViewModel;
            Session["AdvancedSearchVM"] = response.AdvancedSearchViewModel;
            return Json(response.ReportList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult NewContactsVisualization(ReportViewModel viewModel)
        {
            viewModel.CustomStartDate = viewModel.CustomStartDate.Date;
            viewModel.CustomStartDate = ToUserUtcDateTime(viewModel.CustomStartDate.Date);
            viewModel.CustomEndDate = Convert.ToDateTime(viewModel.CustomEndDate.Date.AddHours(23).AddMinutes(59));
            viewModel.CustomEndDate = ToUserUtcDateTime(viewModel.CustomEndDate);
            StandardReportResponse response = new StandardReportResponse();
            response = reportService.GetNewLeadsVisualizationAsync(new StandardReportRequest
            {
                RequestedBy = this.Identity.ToUserID(),
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID(),
                IsSTadmin = this.Identity.IsSTAdmin(),
                ReportViewModel = viewModel,
            });
            return Json(new
            {
                success = true,
                response = response
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PDF_Export_Save(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);
            return File(fileContents, contentType, fileName);
        }

        public ActionResult GetContacts(ReportViewModel viewmodel)
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
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID(),
                IsSTAdmin = this.Identity.IsSTAdmin()
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
            else if (viewmodel.Type == 'A')
            {
                if (viewmodel.ActivityModule == "Tours")
                    viewmodel.ModuleIds = new int[] { 7 };
                else if (viewmodel.ActivityModule == "Notes")
                    viewmodel.ModuleIds = new int[] { 6 };
                else if (viewmodel.ActivityModule == "Contacts")
                    viewmodel.ModuleIds = new int[] { 3 };
                else { }

                response = reportService.GetActivityReportContacts(request);
            }
            else if (viewmodel.ReportType == Reports.FirstLeadSourceReport)
                response = reportService.FirstLeadSourceReportContacts(request);
            else if (viewmodel.ReportType == Reports.AllLeadSourceReport)
                response = reportService.AllLeadSourceReportContacts(request);
            else if (viewmodel.ReportType == Reports.DatabaseLifeCycleReport)
                response = reportService.AllDatabaseReportContacts(request);

            Guid guid = Guid.NewGuid();
            if (response.ContactIds != null)
            {
                AddCookie("ContactsGuid", guid.ToString(), 1);
                var contactIds = response.ContactIds.Distinct();
                cachingService.StoreSavedSearchContactIds(guid.ToString(), contactIds);
            }

            return Json(new
            {
                success = true,
                response = guid.ToString()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetOnlineRegistertedContacts(ReportViewModel viewmodel)
        {
            if (viewmodel.PeriodId != 7)
            {
                var isMinDate = viewmodel.CustomEndDate == DateTime.MinValue;
                var minDate = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;
                minDate = minDate.AddYears(1);
                viewmodel.CustomStartDate = ToUserUtcDateTime(isMinDate ? minDate : viewmodel.CustomStartDate);
                viewmodel.CustomEndDate = ToUserUtcDateTime(isMinDate ? DateTime.UtcNow : viewmodel.CustomEndDate);
            }
            ReportDataRequest request = new ReportDataRequest()
            {
                ReportViewModel = viewmodel,
                AccountId = this.Identity.ToAccountID(),
                UserId = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID()
            };
            ReportDataResponse response = new ReportDataResponse();
            response = reportService.GetOnlineRegisteredContacts(request);
            Guid guid = Guid.NewGuid();
            if (response.ContactIds != null)
            {
                AddCookie("ContactsGuid", guid.ToString(), 1);
                var contactIds = response.ContactIds;
                cachingService.StoreSavedSearchContactIds(guid.ToString(), contactIds);
            }
            return Json(new
            {
                success = true,
                response = guid.ToString()
            }, JsonRequestBehavior.AllowGet);
        }


        [Route("getreengagementinfo")]
        [HttpGet]
        public JsonResult GetReEngagementInfo(GetReEngagementInfoRequest request)
        {
            Logger.Current.Verbose("CampaignController/GetReEngagementInfo. AccountId: " + this.Identity.ToAccountID());
            request.AccountId = this.Identity.ToAccountID();
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(this.Identity.ToTimeZone());
            if (request.IsDefaultDateRange == false)
            {
                Logger.Current.Verbose("StartDate before converting: " + request.StartDate.ToString());
                request.StartDate = request.StartDate.AddMilliseconds(-timeZoneInfo.BaseUtcOffset.TotalMilliseconds);
                Logger.Current.Verbose("StartDate after converting: " + request.StartDate.ToString());

                Logger.Current.Verbose("EndDate before converting: " + request.EndDate.ToString()); 
                request.EndDate = request.EndDate.AddMilliseconds(-timeZoneInfo.BaseUtcOffset.TotalMilliseconds);
                Logger.Current.Verbose("EndDate after converting: " + request.EndDate.ToString());
            }
            var reEngagementInfo = campaignService.GetReEngagementInfo(request);
            UpdateLastRunActivity(request.ReportId, "Campaigns Re-engagement");
            return Json(new
            {
                success = true,
                response = reEngagementInfo
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetReEngagedContacts(GetReEngagementInfoRequest request)
        {
            Logger.Current.Verbose("In ReportController/GetReEngagedContacts");
            request.AccountId = this.Identity.ToAccountID();
            request.RequestedBy = this.Identity.ToUserID();
            request.RoleId = this.Identity.ToRoleID();
            GetReEngagementInfoResponse response = new GetReEngagementInfoResponse();
            response = reportService.GetReEngagedContacts(request);
            Guid guid = Guid.NewGuid();
            if (response.ContactIds != null)
            {
                AddCookie("ContactsGuid", guid.ToString(), 1);
                var contactIds = response.ContactIds;
                cachingService.StoreSavedSearchContactIds(guid.ToString(), contactIds);
                Logger.Current.Informational(response.ContactIds.Count() + " contacts found");
            }
            return Json(new
            {
                success = true,
                response = guid.ToString()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FormsCountSummary(ReportViewModel viewModel)
        {
            StandardReportResponse response = new StandardReportResponse();
            if (viewModel.PeriodId != 7)
            {
                var isMinDate = viewModel.CustomEndDate == DateTime.MinValue;
                var minDate = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;
                minDate = minDate.AddYears(1);
                viewModel.CustomStartDate = ToUserUtcDateTime(isMinDate ? minDate : viewModel.CustomStartDate);
                viewModel.CustomEndDate = ToUserUtcDateTime(isMinDate ? DateTime.UtcNow : viewModel.CustomEndDate);
            }
            response = reportService.GetFormsCountSummaryReport(new StandardReportRequest
            {
                UserID = this.Identity.ToUserID(),
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID(),
                IsSTadmin = this.Identity.IsSTAdmin(),
                ReportViewModel = viewModel,
            });
            if (viewModel != null)
                UpdateLastRunActivity(viewModel.ReportId, viewModel.ReportName);
            return Json(new
            {
                success = true,
                response = new DataSourceResult
                {
                    Data = response.ReportList,
                    Total = (int)response.TotalHits
                }
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BDXFreemiumCustomLeadReport(ReportViewModel viewModel)
        {
            StandardReportResponse response = new StandardReportResponse();
            if (viewModel.PeriodId != 7)
            {
                viewModel.CustomStartDate = ToUserUtcDateTime(viewModel.CustomStartDate);
                viewModel.CustomEndDate = ToUserUtcDateTime(viewModel.CustomEndDate);
            }
            response = reportService.GetBDXFreemiumCustomLeadReportDetails(new StandardReportRequest
            {
                UserID = this.Identity.ToUserID(),
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID(),
                IsSTadmin = this.Identity.IsSTAdmin(),
                ReportViewModel = viewModel,
            });
            if (viewModel != null)
                UpdateLastRunActivity(viewModel.ReportId, viewModel.ReportName);
            Guid guid = Guid.NewGuid();
            ViewBag.Guid = guid.ToString();
            if (response.ContactIds != null)
            {
                AddCookie("ContactsGuid", guid.ToString(), 1);
                var contactIds = response.ContactIds;
                cachingService.StoreSavedSearchContactIds(guid.ToString(), contactIds);
            }
            return Json(new
            {
                success = true,
                response = new DataSourceResult
                {
                    Data = response.ReportContacts,
                    Total = (int)response.TotalHits
                }
            }, JsonRequestBehavior.AllowGet);
        }

        #region BDXCustomLeadReport
        public ActionResult BDXCustomLeadReport(ReportViewModel viewModel)
        {
            if (viewModel.PeriodId != 7)
            {
                viewModel.CustomStartDate = ToUserUtcDateTime(viewModel.CustomStartDate);
                viewModel.CustomEndDate = ToUserUtcDateTime(viewModel.CustomEndDate);
            }
            BDXCustomLeadReportResponse response = reportService.GetBDXCustomLeadReportDetails(new BDXCustomLeadReportRequest
            {
                UserID = this.Identity.ToUserID(),
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID(),
                IsSTadmin = this.Identity.IsSTAdmin(),
                ReportViewModel = viewModel,
            });
            foreach (BDXLeadReportEntry entry in response.Contacts)
            {
                entry.CreatedDate = entry.CreatedDate.ToUtcBrowserDatetime();
            }
            if (viewModel != null)
                UpdateLastRunActivity(viewModel.ReportId, viewModel.ReportName);
            Guid guid = Guid.NewGuid();
            ViewBag.Guid = guid.ToString();
            if (response.ContactIds != null)
            {
                AddCookie("ContactsGuid", guid.ToString(), 1);
                var contactIds = response.ContactIds;
                cachingService.StoreSavedSearchContactIds(guid.ToString(), contactIds);
            }
            return Json(new
            {
                success = true,
                response = new DataSourceResult
                {
                    Data = response.Contacts,
                    Total = response.TotalHits
                }
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BDXCustomLeadReportExport(ReportViewModel viewModel)
        {
            if (viewModel.PeriodId != 7)
            {
                viewModel.CustomStartDate = ToUserUtcDateTime(viewModel.CustomStartDate);
                viewModel.CustomEndDate = ToUserUtcDateTime(viewModel.CustomEndDate);
            }
            BDXCustomLeadReportResponse response = reportService.GetBDXCustomLeadReportDetailsExport(new BDXCustomLeadReportRequest
            {
                UserID = this.Identity.ToUserID(),
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID(),
                IsSTadmin = this.Identity.IsSTAdmin(),
                ReportViewModel = viewModel,
            });
            string fileKey = Guid.NewGuid().ToString();
            bool result = cachingService.StoreTemporaryFile(fileKey, response.byteArray);
            Logger.Current.Informational("Did temporary file stored in cache : " + result.ToString());
            return Json(new
            {
                success = true,
                fileKey = fileKey,
                fileName = response.FileName
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion
        public JsonResult RunCampaignReport(ReportViewModel viewModel)
        {
            if (viewModel.PeriodId != 7)
            {
                viewModel.CustomStartDate = ToUserUtcDateTime(viewModel.CustomStartDate);
                viewModel.CustomEndDate = ToUserUtcDateTime(viewModel.CustomEndDate);
            }
            StandardReportResponse response = reportService.RunCampaignReport(new StandardReportRequest
            {
                ReportViewModel = viewModel,
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID(),
                IsSTadmin = this.Identity.IsSTAdmin(),
                UserID = this.Identity.ToUserID(),
                isReputationReport = false
            });
            if (viewModel != null)
                UpdateLastRunActivity(viewModel.ReportId, viewModel.ReportName);
            return Json(new
            {
                success = true,
                response = new DataSourceResult
                {
                    Data = response.ReportList,
                    Total = (int)response.TotalHits
                }
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetWorkflows(int campaignId)
        {
            GetWorkflowsForCampaignReportResponse response = reportService.GetWorkflowsForCampaignReport(new GetWorkflowsForCampaignReportRequest() { CampaignID = campaignId });
            return Json(new { data = response.WorkflowNames }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RunCampaignReportExport(DateTime CustomStartDate, DateTime CustomEndDate)
        {
            ReportExportResponce response = reportService.RunCampaignReportExport(new ReportExportRequest
            {
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID(),
                IsSTadmin = this.Identity.IsSTAdmin(),
                isReputationReport = true
            });
            return File(response.fileContent, @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml", response.fileName);
        }

        public JsonResult RunCampaignReputationReport(ReportViewModel viewModel)
        {
            if (viewModel.PeriodId != 7)
            {
                viewModel.CustomStartDate = ToUserUtcDateTime(viewModel.CustomStartDate);
                viewModel.CustomEndDate = ToUserUtcDateTime(viewModel.CustomEndDate);
            }
            StandardReportResponse response = reportService.RunCampaignReport(new StandardReportRequest
            {
                ReportViewModel = viewModel,
                AccountId = viewModel.AccountId,
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID(),
                IsSTadmin = this.Identity.IsSTAdmin(),
                UserID = this.Identity.ToUserID(),
                isReputationReport = true
            });
            return Json(new
            {
                success = true,
                response = new DataSourceResult
                {
                    Data = response.ReportList,
                    Total = (int)response.TotalHits
                }
            }, JsonRequestBehavior.AllowGet);
        }

        public void AddCookie(string cookieName, string Value, int days)
        {
            HttpCookie CartCookie = new HttpCookie(cookieName, Value);
            CartCookie.Expires = DateTime.Now.AddDays(days);
            Response.Cookies.Add(CartCookie);
        }

        void UpdateLastRunActivity(int reportId, string reportName)
        {
            if (reportId != 0)
            {
                int userId = this.Identity.ToUserID();
                int accountId = this.Identity.ToAccountID();
                reportService.UpdateLastRunActivity(new SmartTouch.CRM.ApplicationServices.Messaging.Reports.InsertLastRunActivityRequest()
                {
                    RequestedBy = userId,
                    ReportName = reportName,
                    AccountId = accountId,
                    ReportId = reportId
                });
            }
        }

        private static DateTime ToUserUtcDateTime(DateTime d)
        {
            return d.ToJsonSerailizedDate().ToUserUtcDateTime();
        }

        public ActionResult Excel_Export_Save()
        {
            return null;
        }

        public ActionResult TagsViewRead([DataSourceRequest] DataSourceRequest request, string name)
        {
            var sortField = request.Sorts.Count > 0 ? request.Sorts.First().Member : GetPropertyName<TagViewModel, int>(t => t.TagID);
            var direction = request.Sorts.Count > 0 ? request.Sorts.First().SortDirection : System.ComponentModel.ListSortDirection.Descending;
            GetTagListResponse response = tagService.GetAllTagsByContacts(new GetTagListRequest()
            {
                Name = name,
                Limit = request.PageSize,
                PageNumber = request.Page,
                AccountId = UserExtensions.ToAccountID(this.Identity),
                SortDirection = direction,
                SortField = sortField,
                RequestedBy = this.Identity.ToUserID(),
                IsSTadmin = this.Identity.IsSTAdmin(),
                RoleId = this.Identity.ToRoleID()
            });
            return Json(new DataSourceResult
            {
                Data = response.Tags,
                Total = response.TotalHits
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TagsExcelExport(string name)
        {
            var sortField = GetPropertyName<TagViewModel, int>(t => t.TagID);
            var direction = System.ComponentModel.ListSortDirection.Descending;
            GetTagListResponse response = tagService.GetAllTagsByContacts(new GetTagListRequest()
            {
                Name = "",
                Limit = 0,
                PageNumber = 0,
                AccountId = UserExtensions.ToAccountID(this.Identity),
                SortDirection = direction,
                SortField = sortField,
                RequestedBy = this.Identity.ToUserID(),
                IsSTadmin = this.Identity.IsSTAdmin(),
                RoleId = this.Identity.ToRoleID()
            });
            return Json(response.Tags, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetContactIdListBy(int tagID)
        {
            GetTagContactsResponse response = contactService.GetTagRelatedContacts(new GetTagContactsRequest
            {
                TagID = tagID
            });
            return Json(new
            {
                success = true,
                response = response.ContactIdList
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertUserSettings(string viewModel)
        {
            DashboardViewModel dashViewModel = JsonConvert.DeserializeObject<DashboardViewModel>(viewModel);
            IEnumerable<DashboardSettingViewModel> ids = dashViewModel.Settings;
            reportService.InsertUserDashboardSettings(new InsertUserDashboardSettingsRequest()
            {
                UserId = this.Identity.ToUserID(),
                DashboardViewModel = ids
            });
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult GetDashboardItems()
        {
            int userId = this.Identity.ToUserID();
            GetDashboardItemsResponse response = reportService.GetDashboardItems(userId);
            return Json(response.DashboardSettingViewModel, JsonRequestBehavior.AllowGet);
        }

        [Route("getreporttype")]
        [HttpPost]
        public JsonResult GetReportIdByType(string reportType)
        {
            GetReportsRequest request = new GetReportsRequest { ReportType = (Reports)byte.Parse(reportType), AccountId = this.Identity.ToAccountID() };
            GetReportsResponse response = reportService.GetReportByType(request);

            return Json(new
            {
                success = true,
                response = response.Report.ReportID
            }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetDatabaseLifeCycleData(ReportViewModel viewModel)
        {
            viewModel.CustomStartDate = ToUserUtcDateTime(viewModel.CustomStartDate);
            viewModel.CustomEndDate = ToUserUtcDateTime(viewModel.CustomEndDate);

            ReportDataRequest request = new ReportDataRequest()
            {
                ReportViewModel = viewModel,
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID(),
                IsSTAdmin = this.Identity.IsSTAdmin()
            };
            ReportDataResponse response = new ReportDataResponse();

            response = reportService.GetDatabaseLifeCycleData(request);

            UpdateLastRunActivity(viewModel.ReportId, viewModel.ReportName);
            return Json(new
            {
                success = true,
                response = response.ReportData
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ExportToExcel(string contentType, string base64, string fileName)
        {
            int _CheckAccountId = this.Identity.ToAccountID(); // Rama Krishna HDST-8588
            var fileContents = Convert.FromBase64String("");
            if (_CheckAccountId != 339)
            {
                fileContents = Convert.FromBase64String(base64);
                return File(fileContents, contentType, fileName);
            }
            else
            {
                return null;
            }
        }

        public ActionResult GetTourByContactsReportData(ReportViewModel viewModel)
        {
            viewModel.CustomStartDate = ToUserUtcDateTime(viewModel.CustomStartDate);
            viewModel.CustomEndDate = ToUserUtcDateTime(viewModel.CustomEndDate);
            TourByContactsReponse response = new TourByContactsReponse();
            response.TourByContactsReportData = reportService.GetTourByContactsReportData(new TourByContactsRequest
            {
                AccountId = this.Identity.ToAccountID(),
                FromDate = viewModel.CustomStartDate,
                ToDate = viewModel.CustomEndDate,
                TourStatus = viewModel.TourStatusIds,
                TourType = viewModel.TourTypeIds,
                TourCommunity = viewModel.CommunityIds,
                pageNumber = viewModel.PageNumber,
                pageSize = (int)viewModel.ShowTop,
                SortField = viewModel.Sorts.IsAny() ? viewModel.Sorts.Select(f => f.field).FirstOrDefault() : "TourDate",
                SortDirection = viewModel.Sorts.IsAny() ? viewModel.Sorts.Select(f => f.dir).FirstOrDefault() : "desc"

            }).TourByContactsReportData;
            UpdateLastRunActivity(viewModel.ReportId, viewModel.ReportName);
            return Json(new
            {
                success = true,
                response = new DataSourceResult
                {
                    Data = response.TourByContactsReportData,
                    Total = response.TourByContactsReportData.IsAny() ? response.TourByContactsReportData.Select(s => s.TotalCount).FirstOrDefault() : 0
                }
            }, JsonRequestBehavior.AllowGet);
        }

        [SmarttouchAuthorize(AppModules.Reports, AppOperations.Read)]
        public ActionResult GetNightlyStatusReport(ReportViewModel model)
        {
            model.CustomStartDate = ToUserUtcDateTime(model.CustomStartDate);
            model.CustomEndDate = ToUserUtcDateTime(model.CustomEndDate);
            StandardReportResponse response = new StandardReportResponse();
            response = reportService.RunNightlyStatusReport(new ReportDataRequest() { ReportViewModel = model });
            UpdateLastRunActivity(model.ReportId, model.ReportName);

            return Json(new
            {
                success = true,
                response = new DataSourceResult 
                {
                    Data = response.ReportList,
                    Total = model.ReportType == Reports.BouncedEmail ? (response.ReportList != null ? (response.ReportList.IsAny() ? response.ReportList.Select(s => s.TotalCount).FirstOrDefault() : 0) : 0)
                    : response.ReportList != null ? (response.ReportList.IsAny() ? response.ReportList.Count() : 0) : 0
                }
            });
        }

        public ActionResult GetLoginFrequncyReport(ReportViewModel model)
        {
            model.CustomStartDate = ToUserUtcDateTime(model.CustomStartDate);
            model.CustomEndDate = ToUserUtcDateTime(model.CustomEndDate);
            model.AccountId = this.Identity.ToAccountID();
            StandardReportResponse response = new StandardReportResponse();
            response = reportService.RunLoginFrequencyReport(new ReportDataRequest() { ReportViewModel = model });
            UpdateLastRunActivity(model.ReportId, model.ReportName);

            return Json(new
            {
                success = true,
                response = new DataSourceResult
                {
                    Data = response.ReportList,
                    Total = response.ReportList != null ? (response.ReportList.IsAny() ? response.ReportList.Count() : 0) : 0
                }
            });
        }


        [SmarttouchAuthorize(AppModules.Reports, AppOperations.Read)]
        public ActionResult GetCampaignStatusReport(ReportViewModel model)
        {
            model.CustomStartDate = ToUserUtcDateTime(model.CustomStartDate);
            model.CustomEndDate = ToUserUtcDateTime(model.CustomEndDate);
            StandardReportResponse response = new StandardReportResponse();
            response = reportService.RunNightlyStatusReport(new ReportDataRequest() { ReportViewModel = model });
            UpdateLastRunActivity(model.ReportId, model.ReportName);

            return Json(new
            {
                success = true,
                response = new DataSourceResult
                {
                    Data = response.ReportList,
                    Total = response.ReportList != null ? (response.ReportList.IsAny() ? response.ReportList.Count() : 0) : 0
                }
            });
        }

        private GetAllAccountsBySubscriptionResponse GetAllAccounts(byte Id)
        {
            return accountService.GetAllAccountsBySubscription(new GetAllAccountsBySubscriptionRequest() { ID = Id });
        }
    }
}
