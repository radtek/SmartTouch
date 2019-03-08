using Kendo.Mvc.UI;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.Logging;
using Newtonsoft.Json;
using SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Web.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartTouch.CRM.Web.Controllers
{
    public class WorkFlowController : SmartTouchController
    {
        ICachingService cachingService;

        readonly IWorkflowService workflowService;

        public WorkFlowController(IWorkflowService workflowService, ICachingService cachingService)
        {
            this.cachingService = cachingService;
            this.workflowService = workflowService;
        }

        #region WorkFlowGrid
        [Route("workflows")]
        [SmarttouchAuthorize(AppModules.Automation, AppOperations.Read)]
        [MenuType(MenuCategory.Automation, MenuCategory.LeftMenuCRM)]
        [OutputCache(Duration = 30)]
        public ActionResult WorkFlowList()
        {
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            return View("WorkFlowList", new WorkFlowViewModel());
        }

        [SmarttouchAuthorize(AppModules.Automation, AppOperations.Read)]
        public ActionResult WorkFlowsViewRead([DataSourceRequest] DataSourceRequest request, string name, string status)
        {
            short workFlowStatus = default(short);
            if (!String.IsNullOrEmpty(status))
                short.TryParse(status, out workFlowStatus);
            AddCookie("workflowpagesize", request.PageSize.ToString(), 1);
            AddCookie("workflowpagenumber", request.Page.ToString(), 1);
            var sortField = request.Sorts.Count > 0 ? request.Sorts.First().Member : GetPropertyName<WorkFlowViewModel, DateTime?>(t => t.ModifiedOn);
            var direction = request.Sorts.Count > 0 ? request.Sorts.First().SortDirection : System.ComponentModel.ListSortDirection.Descending;
            GetWorkflowListResponse response = workflowService.GetAllWorkFlows(new GetWorkflowListRequest()
            {
                Query = name,
                Limit = request.PageSize,
                PageNumber = request.Page,
                Status = workFlowStatus,
                AccountId = UserExtensions.ToAccountID(this.Identity),
                SortField = sortField,
                SortDirection = direction
            });
            var jsonResult = Json(new DataSourceResult
            {
                Data = response.Workflows,
                Total = (int)response.TotalHits
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        #endregion
        #region Cookies
        public void AddCookie(string cookieName, string Value, int days)
        {
            HttpCookie CartCookie = new HttpCookie(cookieName, Value);
            CartCookie.Expires = DateTime.Now.AddDays(days);
            Response.Cookies.Add(CartCookie);
        }

        #endregion
        #region Add workflow
        [Route("addworkflow")]
        [SmarttouchAuthorize(AppModules.Automation, AppOperations.Read)]
        [MenuType(MenuCategory.AddWorkflow, MenuCategory.LeftMenuCRM)]
        public ActionResult AddWorkFlow()
        {
            WorkFlowViewModel viewModel = new WorkFlowViewModel();
            viewModel.WorkflowName = "";
            viewModel.WorkflowID = 0;
            viewModel.StatusID = 402;
            viewModel.AllowParallelWorkflows = 1;
            viewModel.IsWorkflowAllowedMoreThanOnce = false;
            List<WorkflowTriggerViewModel> triggers = new List<WorkflowTriggerViewModel>();
            WorkflowTriggerViewModel startTrigger = new WorkflowTriggerViewModel()
            {
                TriggerTypeID = 0,
                WorkflowID = 0,
                WorkflowTriggerID = 0,
                IsStartTrigger = true
            };
            WorkflowTriggerViewModel stopTrigger = new WorkflowTriggerViewModel()
            {
                TriggerTypeID = 0,
                WorkflowID = 0,
                WorkflowTriggerID = 0,
                IsStartTrigger = false
            };
            triggers.Add(startTrigger);
            triggers.Add(stopTrigger);
            viewModel.Triggers = triggers;
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            ViewBag.ParentID = 0;
            ViewBag.SenderName = this.Identity.ToFirstName() + " " + this.Identity.ToLastName();
            return View("AddEditWorkflow", viewModel);
        }

        #endregion
        [HttpGet]
        public ActionResult GetDropdownValues(DropdownFieldTypes DropDownID)
        {
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            var lifecyclestages = dropdownValues.Where(k => k.DropdownID == (byte)DropDownID).Select(k => k.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            return Json(new
            {
                success = true,
                response = lifecyclestages
            }, JsonRequestBehavior.AllowGet);
        }

        #region Edit workflow
        [Route("editworkflow")]
        [SmarttouchAuthorize(AppModules.Automation, AppOperations.Read)]
        [MenuType(MenuCategory.EditWorkflow, MenuCategory.LeftMenuCRM)]
        public ActionResult EditWorkFlow(short WorkflowID)
        {
            var accountId = this.Identity.ToAccountID();
            GetWorkflowResponse response = workflowService.GetWorkFlow(new GetWorkflowRequest()
            {
                WorkflowID = WorkflowID,
                AccountId = accountId
            });
            ViewBag.ParentID = response.WorkflowViewModel.ParentWorkflowID != 0 ? response.WorkflowViewModel.ParentWorkflowID : 0;
            if (response.WorkflowViewModel != null && response.WorkflowViewModel.WorkflowActions != null)
                response.WorkflowViewModel.WorkflowActions.Each(a =>
                {
                    if (a.WorkflowActionTypeID == WorkflowActionType.SetTimer)
                    {
                        if (((WorkflowTimerActionViewModel)a.Action).RunAt != null)
                        {
                            Logger.Current.Informational("workflow  set timer action time zone(when retriving workflow in controller): " + ((WorkflowTimerActionViewModel)a.Action).RunAt.Value.ToUtc().ToUserUtcDateTimeV2());
                            ((WorkflowTimerActionViewModel)a.Action).RunAt = ((WorkflowTimerActionViewModel)a.Action).RunAt.Value.ToUtcBrowserDatetime();
                        }
                        else if (((WorkflowTimerActionViewModel)a.Action).RunAtTime != null)
                        {
                            Logger.Current.Informational("workflow set timer action time zone(when retriving workflow in controller): " + ((WorkflowTimerActionViewModel)a.Action).RunAtTime.Value.ToUtc().ToUserUtcDateTimeV2());
                            DateTime value = ((WorkflowTimerActionViewModel)a.Action).RunAtTime.Value.ToUtcBrowserDatetime();
                            ((WorkflowTimerActionViewModel)a.Action).RunAtTime = value;
                        }
                        else if (((WorkflowTimerActionViewModel)a.Action).StartDate != null && ((WorkflowTimerActionViewModel)a.Action).EndDate != null)
                        {
                            ((WorkflowTimerActionViewModel)a.Action).StartDate = ((WorkflowTimerActionViewModel)a.Action).StartDate.Value.ToUtcBrowserDatetime();
                            ((WorkflowTimerActionViewModel)a.Action).EndDate = ((WorkflowTimerActionViewModel)a.Action).EndDate.Value.ToUtcBrowserDatetime();
                        }
                        if (((WorkflowTimerActionViewModel)a.Action).RunOnDate != null)
                        {
                            ((WorkflowTimerActionViewModel)a.Action).RunOnDate = ((WorkflowTimerActionViewModel)a.Action).RunOnDate.Value.ToUserUtcDateTime();                        
                        }
                    }
                });

            var viewModel = ProcessWorkflowFromServer(response.WorkflowViewModel, response.WorkflowViewModel);
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            ViewBag.SenderName = this.Identity.ToFirstName() + " " + this.Identity.ToLastName();
            return View("AddEditWorkflow", viewModel);
        }

        #endregion
        #region Copy Workflow
        [Route("copyworkflow")]
        [SmarttouchAuthorize(AppModules.Automation, AppOperations.Read)]
        [MenuType(MenuCategory.AddWorkflow, MenuCategory.LeftMenuCRM)]
        public ActionResult CopyWorkFlow(short WorkflowID)
        {
            GetWorkflowResponse response = workflowService.CopyWorkFlow(new GetWorkflowRequest()
            {
                WorkflowID = WorkflowID,
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID()
            });
            ViewBag.ParentID = 0;
            if (response.WorkflowViewModel != null && response.WorkflowViewModel.WorkflowActions != null)
                response.WorkflowViewModel.WorkflowActions.Each(a =>
                {
                    if (a.WorkflowActionTypeID == WorkflowActionType.SetTimer)
                    {
                        if (((WorkflowTimerActionViewModel)a.Action).RunAt != null)
                        {
                            Logger.Current.Informational("workflow  set timer action time zone(when retriving workflow in controller): " + ((WorkflowTimerActionViewModel)a.Action).RunAt.Value.ToUtcBrowserDatetime());
                            ((WorkflowTimerActionViewModel)a.Action).RunAt = ((WorkflowTimerActionViewModel)a.Action).RunAt.Value.ToUtcBrowserDatetime();
                        }
                        else if (((WorkflowTimerActionViewModel)a.Action).RunAtTime != null)
                        {
                            Logger.Current.Informational("workflow set timer action time zone(when retriving workflow in controller): " + ((WorkflowTimerActionViewModel)a.Action).RunAtTime.Value.ToUtcBrowserDatetime());
                            DateTime value = ((WorkflowTimerActionViewModel)a.Action).RunAtTime.Value.ToUtcBrowserDatetime();
                            ((WorkflowTimerActionViewModel)a.Action).RunAtTime = value;
                        }
                        else if (((WorkflowTimerActionViewModel)a.Action).StartDate != null && ((WorkflowTimerActionViewModel)a.Action).EndDate != null)
                        {
                            ((WorkflowTimerActionViewModel)a.Action).StartDate = ((WorkflowTimerActionViewModel)a.Action).StartDate.Value.ToUtcBrowserDatetime();
                            ((WorkflowTimerActionViewModel)a.Action).EndDate = ((WorkflowTimerActionViewModel)a.Action).EndDate.Value.ToUtcBrowserDatetime();
                        }
                        if (((WorkflowTimerActionViewModel)a.Action).RunOnDate != null)
                        {
                            ((WorkflowTimerActionViewModel)a.Action).RunOnDate = ((WorkflowTimerActionViewModel)a.Action).RunOnDate.Value.ToUserUtcDateTime();
                        }
                    }
                });
            var viewModel = ProcessWorkflowFromServer(response.WorkflowViewModel, response.WorkflowViewModel);
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            ViewBag.SenderName = this.Identity.ToFirstName() + " " + this.Identity.ToLastName();
            return View("AddEditWorkflow", viewModel);
        }

        #endregion

        #region Full Edit Workflow
        [Route("fulleditworkflow")]
        [SmarttouchAuthorize(AppModules.Automation, AppOperations.Read)]
        [MenuType(MenuCategory.AddWorkflow, MenuCategory.LeftMenuCRM)]
        public ActionResult FullEditWorkFlow(short WorkflowID)
        {
            GetWorkflowResponse response = workflowService.CopyWorkFlow(new GetWorkflowRequest()
            {
                WorkflowID = WorkflowID,
                IsNewWorkflow = true,
                AccountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID(),
                RequestedBy = this.Identity.ToUserID()
            });

            ViewBag.ParentID = WorkflowID;

            if (response.WorkflowViewModel != null && response.WorkflowViewModel.WorkflowActions != null)
                response.WorkflowViewModel.WorkflowActions.Each(a =>
                {
                    if (a.WorkflowActionTypeID == WorkflowActionType.SetTimer)
                    {
                        if (((WorkflowTimerActionViewModel)a.Action).RunAt != null)
                        {
                            Logger.Current.Informational("workflow  set timer action time zone(when retriving workflow in controller): " + ((WorkflowTimerActionViewModel)a.Action).RunAt.Value.ToUtcBrowserDatetime());
                            ((WorkflowTimerActionViewModel)a.Action).RunAt = ((WorkflowTimerActionViewModel)a.Action).RunAt.Value.ToUtcBrowserDatetime();
                        }
                        else if (((WorkflowTimerActionViewModel)a.Action).RunAtTime != null)
                        {
                            Logger.Current.Informational("workflow set timer action time zone(when retriving workflow in controller): " + ((WorkflowTimerActionViewModel)a.Action).RunAtTime.Value.ToUtcBrowserDatetime());
                            DateTime value = ((WorkflowTimerActionViewModel)a.Action).RunAtTime.Value.ToUtcBrowserDatetime();
                            ((WorkflowTimerActionViewModel)a.Action).RunAtTime = value;
                        }
                        else if (((WorkflowTimerActionViewModel)a.Action).StartDate != null && ((WorkflowTimerActionViewModel)a.Action).EndDate != null)
                        {
                            ((WorkflowTimerActionViewModel)a.Action).StartDate = ((WorkflowTimerActionViewModel)a.Action).StartDate.Value.ToUtcBrowserDatetime();
                            ((WorkflowTimerActionViewModel)a.Action).EndDate = ((WorkflowTimerActionViewModel)a.Action).EndDate.Value.ToUtcBrowserDatetime();
                        }
                        if (((WorkflowTimerActionViewModel)a.Action).RunOnDate != null)
                        {
                            ((WorkflowTimerActionViewModel)a.Action).RunOnDate = ((WorkflowTimerActionViewModel)a.Action).RunOnDate.Value.ToUserUtcDateTime();
                        }
                    }
                });
            var viewModel = ProcessWorkflowFromServer(response.WorkflowViewModel, response.WorkflowViewModel);
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            ViewBag.SenderName = this.Identity.ToFirstName() + " " + this.Identity.ToLastName();
            return View("AddEditWorkflow", viewModel);
        }

        #endregion
        [Route("InsertWorkflow")]
        [SmarttouchAuthorize(AppModules.Automation, AppOperations.Create)]
        public JsonResult AddWorkflow(string workflowViewModel)
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new BaseWorkflowActionConverter());
            settings.NullValueHandling = NullValueHandling.Ignore;
            var viewModel = JsonConvert.DeserializeObject<WorkFlowViewModel>(workflowViewModel, settings);
            /*exclude send campaign actions */
            viewModel.WorkflowActions.Each(a => {
                if (a.WorkflowActionTypeID == WorkflowActionType.SetTimer)
                {
                    if (((WorkflowTimerActionViewModel)a.Action).RunAtDateTime != null)
                        ((WorkflowTimerActionViewModel)a.Action).RunAt = ((WorkflowTimerActionViewModel)a.Action).RunAtDateTime;
                    else if (((WorkflowTimerActionViewModel)a.Action).RunAtTimeDateTime != null)
                        ((WorkflowTimerActionViewModel)a.Action).RunAtTime = ((WorkflowTimerActionViewModel)a.Action).RunAtTimeDateTime;
                }
            });
            viewModel.WorkflowActions.Each(a => {
                if (a.WorkflowActionTypeID == WorkflowActionType.SetTimer)
                {
                    if (((WorkflowTimerActionViewModel)a.Action).RunAt != null)
                        Logger.Current.Informational("workflow  set timer action time zone(when inserting workflow in controller): " + ((WorkflowTimerActionViewModel)a.Action).RunAt);
                    else if (((WorkflowTimerActionViewModel)a.Action).RunAtTime != null)
                        Logger.Current.Informational("workflow set timer action time zone(when inserting workflow in controller): " + ((WorkflowTimerActionViewModel)a.Action).RunAtTime);
                }
            });
            viewModel = ProcessWorkflowFromClient(viewModel, viewModel);
            InsertWorkflowRequest request = new InsertWorkflowRequest()
            {
                WorkflowViewModel = viewModel,
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID()
            };
            workflowService.InsertWorkflow(request);
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        [Route("UpdateWorkflow")]
        public JsonResult UpdateWorkflow(string workflowViewModel)
        {
            var settings = new JsonSerializerSettings();
            var accountId = this.Identity.ToAccountID();
            settings.Converters.Add(new BaseWorkflowActionConverter());
            settings.NullValueHandling = NullValueHandling.Ignore;
            var viewModel = JsonConvert.DeserializeObject<WorkFlowViewModel>(workflowViewModel, settings);
            viewModel.WorkflowActions.Each(a => {
                if (a.WorkflowActionTypeID == WorkflowActionType.SetTimer)
                {
                    if (((WorkflowTimerActionViewModel)a.Action).RunAtDateTime != null)
                        ((WorkflowTimerActionViewModel)a.Action).RunAt = ((WorkflowTimerActionViewModel)a.Action).RunAtDateTime;
                    else if (((WorkflowTimerActionViewModel)a.Action).RunAtTimeDateTime != null)
                        ((WorkflowTimerActionViewModel)a.Action).RunAtTime = ((WorkflowTimerActionViewModel)a.Action).RunAtTimeDateTime;

                    if (((WorkflowTimerActionViewModel)a.Action).RunOnDate.HasValue)
                        ((WorkflowTimerActionViewModel)a.Action).RunOnDate = ((WorkflowTimerActionViewModel)a.Action).RunOnDate.Value.Date.ToUserDateTime().ToUtc();
                }
            });
            viewModel.WorkflowActions.Each(a => {
                if (a.WorkflowActionTypeID == WorkflowActionType.SetTimer)
                {
                    if (((WorkflowTimerActionViewModel)a.Action).RunAt != null)
                        Logger.Current.Informational("workflow  set timer action time zone(when Updating workflow in controller): " + ((WorkflowTimerActionViewModel)a.Action).RunAt);
                    else if (((WorkflowTimerActionViewModel)a.Action).RunAtTime != null)
                        Logger.Current.Informational("workflow set timer action time zone(when Updating workflow in controller): " + ((WorkflowTimerActionViewModel)a.Action).RunAtTime);
                }
            });
            var serverViewModel = workflowService.GetWorkFlow(new GetWorkflowRequest()
            {
                WorkflowID = viewModel.WorkflowID,
                AccountId = accountId
            }).WorkflowViewModel;
            viewModel = ProcessWorkflowFromClient(viewModel, serverViewModel);
            UpdateWorkflowRequest request = new UpdateWorkflowRequest()
            {
                WorkflowViewModel = viewModel,
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID()
            };
            workflowService.UpdateWorkflow(request);
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        [Route("workflowreport")]
        [SmarttouchAuthorize(AppModules.Automation, AppOperations.Read)]
        [MenuType(MenuCategory.ViewWorkflow, MenuCategory.LeftMenuCRM)]
        public ActionResult GetWorkflowReport(short workflowID)
        {
            var accountId = this.Identity.ToAccountID();
            ViewBag.CurrentWfId = workflowService.GetWorkflowIdByParentWfId(workflowID);
            GetWorkflowResponse response = workflowService.GetWorkFlow(new GetWorkflowRequest()
            {
                WorkflowID = workflowID,
                AccountId = accountId,
                RoleId=this.Identity.ToRoleID(),
                RequestedBy = this.Identity.ToUserID()
            });

            if (response.WorkflowViewModel != null && response.WorkflowViewModel.WorkflowActions != null)
                response.WorkflowViewModel.WorkflowActions.Each(a =>
                {
                    if (a.WorkflowActionTypeID == WorkflowActionType.SetTimer)
                    {
                        if (((WorkflowTimerActionViewModel)a.Action).RunAt != null)
                        {
                            ((WorkflowTimerActionViewModel)a.Action).RunAt = ((WorkflowTimerActionViewModel)a.Action).RunAt.Value.ToUtcBrowserDatetime();
                        }
                        else if (((WorkflowTimerActionViewModel)a.Action).RunAtTime != null)
                        {
                            DateTime value = ((WorkflowTimerActionViewModel)a.Action).RunAtTime.Value.ToUtcBrowserDatetime();
                            ((WorkflowTimerActionViewModel)a.Action).RunAtTime = value;
                        }
                        else if (((WorkflowTimerActionViewModel)a.Action).StartDate != null && ((WorkflowTimerActionViewModel)a.Action).EndDate != null)
                        {
                            ((WorkflowTimerActionViewModel)a.Action).StartDate = ((WorkflowTimerActionViewModel)a.Action).StartDate.Value.ToUtcBrowserDatetime();
                            ((WorkflowTimerActionViewModel)a.Action).EndDate = ((WorkflowTimerActionViewModel)a.Action).EndDate.Value.ToUtcBrowserDatetime();
                        }
                        if (((WorkflowTimerActionViewModel)a.Action).RunOnDate != null)
                        {
                            ((WorkflowTimerActionViewModel)a.Action).RunOnDate = ((WorkflowTimerActionViewModel)a.Action).RunOnDate.Value.ToUserUtcDateTime();
                        }
                    }
                });
            var viewModel = ProcessWorkflowFromServer(response.WorkflowViewModel, response.WorkflowViewModel);
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            ViewData["Parentwfs"] = response.ParentWorkflows.ToList();
            if (Request.IsAjaxRequest() == true)
                return Json(new
                {
                    success = true,
                    response = viewModel
                }, JsonRequestBehavior.AllowGet);
            else
                return View("WorkflowReport", viewModel);
        }

        [HttpGet]
        public JsonResult GetEmailStaistics(int campaignID, int workflowID, string from, string to)
        {
            var fromDate = DateTime.ParseExact(from, "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            var toDate = DateTime.ParseExact(to, "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            var response = workflowService.GetCampaignStatisticsByWorkflow(new ApplicationServices.Messaging.Campaigns.CampaignStatisticsByWorkflowRequest()
            {
                WorkflowID = (short)workflowID,
                CampaignID = campaignID,
                FromDate = fromDate,
                ToDate = toDate
            });
            return Json(new
            {
                success = true,
                response = response.CampaignStatistics
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateWorkflowStatus(WorkflowStatus status, int workflowID)
        {
            workflowService.UpdateWorkflowStatus(new WorkflowStatusRequest()
            {
                Status = status,
                WorkflowID = workflowID,
                RequestedBy = this.Identity.ToUserID(),
                AccountId = this.Identity.ToAccountID()
            });
            return Json(new
            {
                success = true,
                response = string.Empty
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InitiateWorkflow(int workflowID)
        {
            workflowService.UpdateWorkflowStatus(new WorkflowStatusRequest()
            {
                Status = WorkflowStatus.Active,
                WorkflowID = workflowID,
                RequestedBy = this.Identity.ToUserID(),
                AccountId = this.Identity.ToAccountID()
            });
            return Json(new
            {
                success = true,
                response = string.Empty
            }, JsonRequestBehavior.AllowGet);
        }

        private static WorkFlowViewModel ProcessWorkflowFromClient(WorkFlowViewModel source, WorkFlowViewModel destination)
        {
            IEnumerable<int?> _new = null;
            source.Triggers.Each(t => {
                if (t.FormIDs.IsAny())
                {
                    _new = t.FormIDs;
                    /*to be removed triggers*/
                    var deleted = destination.Triggers.Where(dt => dt.IsStartTrigger == t.IsStartTrigger && dt.TriggerTypeID == WorkflowTriggerType.FormSubmitted).Except(t.FormIDs, et => et.FormID, f => f).ToList();
                    var otherdeleted = destination.Triggers.Where(dt => dt.IsStartTrigger == t.IsStartTrigger && dt.TriggerTypeID != WorkflowTriggerType.FormSubmitted);
                    destination.Triggers = destination.Triggers.Except(deleted).Except(otherdeleted);
                    _new = t.FormIDs.Except(destination.Triggers, f => f, dt => dt.FormID);
                    _new.Each(f => {
                        destination.Triggers = destination.Triggers.Concat(new[] {
                            new WorkflowTriggerViewModel () {
                                WorkflowID = destination.WorkflowID,
                                FormID = f,
                                TriggerTypeID = WorkflowTriggerType.FormSubmitted,
                                IsStartTrigger = t.IsStartTrigger
                            }
                        });
                        ;
                    });
                }
                else if (t.TagIDs.IsAny())
                {
                    _new = t.TagIDs;
                    /*to be removed triggers*/
                    var deleted = destination.Triggers.Where(dt => dt.IsStartTrigger == t.IsStartTrigger && dt.TriggerTypeID == WorkflowTriggerType.TagApplied).Except(t.TagIDs, et => et.TagID, f => f).ToList();
                    var otherdeleted = destination.Triggers.Where(dt => dt.IsStartTrigger == t.IsStartTrigger && dt.TriggerTypeID != WorkflowTriggerType.TagApplied);
                    destination.Triggers = destination.Triggers.Except(deleted).Except(otherdeleted);
                    _new = t.TagIDs.Except(destination.Triggers, f => f, dt => dt.TagID);
                    _new.Each(ta => {
                        destination.Triggers = destination.Triggers.Concat(new[] {
                            new WorkflowTriggerViewModel () {
                                WorkflowID = destination.WorkflowID,
                                TagID = ta,
                                TriggerTypeID = WorkflowTriggerType.TagApplied,
                                IsStartTrigger = t.IsStartTrigger
                            }
                        });
                        ;
                    });
                }
                else if (t.SearchDefinitionIDs.IsAny())
                {
                    _new = t.SearchDefinitionIDs;
                    /*to be removed triggers*/
                    var deleted = destination.Triggers.Where(dt => dt.IsStartTrigger == t.IsStartTrigger && dt.TriggerTypeID == WorkflowTriggerType.SmartSearch).Except(t.SearchDefinitionIDs, et => et.SearchDefinitionID, f => f).ToList();
                    var otherdeleted = destination.Triggers.Where(dt => dt.IsStartTrigger == t.IsStartTrigger && dt.TriggerTypeID != WorkflowTriggerType.SmartSearch);
                    destination.Triggers = destination.Triggers.Except(deleted).Except(otherdeleted);
                    _new = t.SearchDefinitionIDs.Except(destination.Triggers, f => f, dt => dt.SearchDefinitionID);
                    _new.Each(s => {
                        destination.Triggers = destination.Triggers.Concat(new[] {
                            new WorkflowTriggerViewModel () {
                                WorkflowID = destination.WorkflowID,
                                SearchDefinitionID = s,
                                TriggerTypeID = WorkflowTriggerType.SmartSearch,
                                IsStartTrigger = t.IsStartTrigger
                            }
                        });
                        ;
                    });
                }
                else if (t.LeadAdapterIDs.IsAny())
                {
                    _new = t.LeadAdapterIDs;
                    /*to be removed triggers*/
                    var deleted = destination.Triggers.Where(dt => dt.IsStartTrigger == t.IsStartTrigger && dt.TriggerTypeID == WorkflowTriggerType.LeadAdapterSubmitted).Except(t.LeadAdapterIDs, et => et.LeadAdapterID, f => f).ToList();
                    var otherdeleted = destination.Triggers.Where(dt => dt.IsStartTrigger == t.IsStartTrigger && dt.TriggerTypeID != WorkflowTriggerType.LeadAdapterSubmitted);
                    destination.Triggers = destination.Triggers.Except(deleted).Except(otherdeleted);
                    _new = t.LeadAdapterIDs.Except(destination.Triggers, f => f, dt => dt.LeadAdapterID);
                    _new.Each(s => {
                        destination.Triggers = destination.Triggers.Concat(new[] {
                            new WorkflowTriggerViewModel () {
                                WorkflowID = destination.WorkflowID,
                                LeadAdapterID = s,
                                TriggerTypeID = WorkflowTriggerType.LeadAdapterSubmitted,
                                IsStartTrigger = t.IsStartTrigger
                            }
                        });
                        ;
                    });
                }
                else
                {
                    var trigger = destination.Triggers.Where(dt => dt.IsStartTrigger == t.IsStartTrigger).FirstOrDefault();
                    destination.Triggers = destination.Triggers.Except(new[] {
                        t
                    }, dt => dt.IsStartTrigger, st => st.IsStartTrigger);
                    trigger.TriggerTypeID = t.TriggerTypeID;
                    trigger.LifecycleDropdownValueID = t.LifecycleDropdownValueID;
                    trigger.CampaignID = t.CampaignID;
                    trigger.OpportunityStageID = t.OpportunityStageID;
                    trigger.SelectedLinks = t.SelectedLinks;
                    trigger.LeadScore = t.LeadScore;
                    trigger.WebPage = t.WebPage;
                    trigger.Duration = t.Duration;
                    trigger.Operator = t.Operator;
                    trigger.TagID = null;
                    trigger.FormID = null;
                    trigger.LeadAdapterID = null;
                    trigger.SearchDefinitionID = null;
                    trigger.IsAnyWebPage = t.IsAnyWebPage;
                    trigger.ActionType = t.ActionType;
                    trigger.TourType = t.TourType;
                    destination.Triggers = destination.Triggers.Concat(new[] {
                        trigger
                    });
                }
            });
            source.Triggers = destination.Triggers.OrderBy(t => t.IsStartTrigger).Reverse();
            return source;
        }

        private static WorkFlowViewModel ProcessWorkflowFromServer(WorkFlowViewModel source, WorkFlowViewModel destination)
        {
            source.Triggers.GroupBy(t => new {
                t.IsStartTrigger,
                t.TriggerTypeID
            }).Each(t => {
                var trigger = new WorkflowTriggerViewModel();
                trigger.IsStartTrigger = t.Key.IsStartTrigger;
                trigger.WorkflowID = destination.WorkflowID;
                trigger.FormIDs = t.Where(f => f.FormID != null).Select(f => f.FormID);
                trigger.FormNames = t.Where(f => f.FormID != null).Select(f => f.FormName);
                trigger.TagIDs = t.Where(tag => tag.TagID != null).Select(tag => tag.TagID);
                trigger.TagNames = t.Where(tag => tag.TagID != null).Select(tag => tag.TagName);
                trigger.LeadAdapterIDs = t.Where(leadadapter => leadadapter.LeadAdapterID != null).Select(leadadapter => leadadapter.LeadAdapterID);
                trigger.LeadAdapterNames = t.Where(leadadapter => leadadapter.LeadAdapterID != null).Select(leadadapter => leadadapter.LeadAdapterName);
                trigger.SearchDefinitionIDs = t.Where(s => s.SearchDefinitionID != null).Select(s => s.SearchDefinitionID);
                trigger.SearchDefinitionNames = t.Where(s => s.SearchDefinitionID != null).Select(s => s.SearchDefinitionName);
                trigger.TriggerTypeID = t.Select(ty => ty.TriggerTypeID).FirstOrDefault();
                trigger.LifecycleDropdownValueID = t.Where(l => l.LifecycleDropdownValueID != null).Select(l => l.LifecycleDropdownValueID).SingleOrDefault();
                trigger.LifecycleName = t.Where(l => l.LifecycleDropdownValueID != null).Select(x => x.LifecycleName).FirstOrDefault();
                trigger.OpportunityStageID = t.Where(l => l.OpportunityStageID != null).Select(l => l.OpportunityStageID).SingleOrDefault();
                trigger.OpportunityStageName = t.Where(l => l.OpportunityStageID != null).Select(l => l.OpportunityStageName).FirstOrDefault();
                trigger.CampaignID = t.Where(l => l.CampaignID != null).Select(l => l.CampaignID).SingleOrDefault();
                trigger.CampaignName = t.Where(l => l.CampaignID != null).Select(l => l.CampaignName).FirstOrDefault();
                trigger.SelectedLinks = t.Select(l => l.SelectedLinks).FirstOrDefault();
                trigger.SelectedURLs = t.Select(l => l.SelectedURLs).FirstOrDefault();
                trigger.LeadScore = t.Select(s => s.LeadScore).FirstOrDefault();
                trigger.IsAnyWebPage = t.Select(s => s.IsAnyWebPage).FirstOrDefault();
                trigger.WebPage = t.Select(s => s.WebPage).FirstOrDefault();
                trigger.Duration = t.Select(s => s.Duration).FirstOrDefault();
                trigger.Operator = t.Select(s => s.Operator).FirstOrDefault();
                trigger.ActionType = t.Select(s => s.ActionType).FirstOrDefault();
                trigger.ActionTypeName = t.Select(s => s.ActionTypeName).FirstOrDefault();
                trigger.TourType = t.Select(s => s.TourType).FirstOrDefault();
                trigger.TourTypeName = t.Select(s => s.TourTypeName).FirstOrDefault();
                destination.Triggers = destination.Triggers.Where(dt => dt.IsStartTrigger != t.Key.IsStartTrigger).Concat(new[] {
                    trigger
                });
            });
            destination.Triggers = destination.Triggers.OrderBy(t => t.IsStartTrigger).Reverse();
            return destination;
        }

        [Route("savesettings")]
        [SmarttouchAuthorize(AppModules.Automation, AppOperations.Create)]
        public JsonResult SaveSettings(string workflowViewModel)
        {
            WorkFlowViewModel viewModel = JsonConvert.DeserializeObject<WorkFlowViewModel>(workflowViewModel);
            UpdateWorkflowRequest request = new UpdateWorkflowRequest()
            {
                WorkflowViewModel = viewModel,
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID()
            };
            workflowService.UpdateWorkflow(request);
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateWorkflowName(string name, int workflowID)
        {
            workflowService.UpdateWorkflowName(name, workflowID,this.Identity.ToAccountID());
            return Json(new
            {
                success = true,
                response = string.Empty
            }, JsonRequestBehavior.AllowGet);
        }

        [Route("editnotifyuseraction")]
        public ActionResult EditNotifyUserAction(int actionId,int workflowId)
        {
            ViewBag.WorkflowId = workflowId;
            GetNotifyUserActionResponse response = workflowService.GetNotifyUserAction(new GetNotifyUserActionRequest() { WorkflowActionId = actionId });
            return PartialView("_WorkflowNotifyUserActionEdit", response.NotifyUserViewModel);
        }

        public JsonResult UpdateNotifyUserAction(string notifyuseraction)
        {
            var viewModel = JsonConvert.DeserializeObject<WorkflowNotifyUserActionViewModel>(notifyuseraction);
            UpdateNotifyUserActionResponse response = workflowService.UpdateNotifyUserAction(new UpdateNotifyUserActionRequest()
            {
                NotifyUserActionViewModel = viewModel,
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID()
            });
            return Json(new
            {
                success = true,
                response = string.Empty
            }, JsonRequestBehavior.AllowGet);
        }

        [Route("edituserassignmentaction")]
        public ActionResult EditUserAssignmentAction(int actionId, int workflowId)
        {
            ViewBag.WorkflowId = workflowId;
            GetUserAssigmentActionResponse response = workflowService.GetUserAssignmentAction(new GetUserAssignmentActionRequest() { WorkflowActionId = actionId });
            return PartialView("_WorkflowUserAssignmentActionEdit", response.ActionViewModel);
        }

        public JsonResult UpdateUserAssignmentAction(string userassignmentaction)
        {
            var viewModel = JsonConvert.DeserializeObject<WorkflowUserAssignmentActionViewModel>(userassignmentaction);
            UpdateUserAssignmentActionReponse response = workflowService.UpdateUserAssignmentAction(new UpdateUserAssignmentActionRequest()
            {
                ActionViewModel = viewModel,
            });
            return Json(new
            {
                success = true,
                response = string.Empty
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult IsWorkflowHasTimerActions(int workflowId)
        {
            bool hasTimerAction = workflowService.IsWorkflowHasTimerActions(workflowId);
            return Json(new
            {
                success = true,
                response = hasTimerAction
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
