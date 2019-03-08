using Kendo.Mvc.UI;
using LandmarkIT.Enterprise.Extensions;
using Newtonsoft.Json;
using SmartTouch.CRM.ApplicationServices.Messaging.LeadAdapters;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Web.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace SmartTouch.CRM.Web.Controllers
{
    public class LeadAdapterController : SmartTouchController
    {
        ILeadAdapterService leadAdapterService;
        ICachingService cachingService;
        public LeadAdapterController(ILeadAdapterService leadAdapterService, ICachingService cachingService)
        {
            this.leadAdapterService = leadAdapterService;
            this.cachingService = cachingService;
        }

        #region Add,Edit,View Leadadapter Operations

        [Route("addleadadapter")]
        [MenuType(MenuCategory.AddEditLeadAdapter, MenuCategory.LeftMenuAccountConfiguration)]
        public ActionResult AddLeadAdapter()
        {
            LeadAdapterViewModel viewModel = new LeadAdapterViewModel();
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            viewModel.LeadSourceDropdownValues = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LeadSources)
                                                               .Select(s => s.DropdownValuesList).FirstOrDefault()
                                                               .Where(d => d.IsActive == true);
            ViewBag.FacebookAPPId = leadAdapterService.GetFacebookApp(new GetFacebookAppRequest() { AccountId = this.Identity.ToAccountID() }).FacebookAppID;
            return View("AddEditLeadAdapter", viewModel);
        }

        [Route("editleadadapter")]
        [SmarttouchAuthorize(AppModules.LeadAdapter, AppOperations.Read)]
        [MenuType(MenuCategory.AddEditLeadAdapter, MenuCategory.LeftMenuAccountConfiguration)]
        public ActionResult EditLeadAdapter(int leadAdapterID)
        {
            LeadAdapterViewModel viewModel = new LeadAdapterViewModel();
            GetLeadAdapterResponse response = leadAdapterService.GetLeadAdapter(new GetLeadAdapterRequest(leadAdapterID));
            if (response != null)
            {
                viewModel = response.LeadAdapterViewModel;
            }
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            viewModel.LeadSourceDropdownValues = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LeadSources)
                                                               .Select(s => s.DropdownValuesList).FirstOrDefault()
                                                               .Where(d => d.IsActive == true);
            ViewBag.FacebookAPPId = leadAdapterService.GetFacebookApp(new GetFacebookAppRequest() { AccountId = this.Identity.ToAccountID() }).FacebookAppID;
            return View("AddEditLeadAdapter", viewModel);
        }

        [Route("viewleadadapter")]
       // [SmarttouchAuthorize(AppModules.LeadAdapter, AppOperations.Read)]
        [MenuType(MenuCategory.LeadAdapters, MenuCategory.LeftMenuAccountConfiguration)]
        public ActionResult ViewLeadAdapter(int leadAdapterID, string leadAdapterName)
        {
            var usersPermissions = cachingService.GetUserPermissions(Thread.CurrentPrincipal.Identity.ToAccountID());
            List<byte> userModules = usersPermissions.Where(s => s.RoleId == (short)Thread.CurrentPrincipal.Identity.ToRoleID()).Select(s => s.ModuleId).ToList();
            if (userModules.Contains((byte)AppModules.ImportData) || userModules.Contains((byte)AppModules.LeadAdapter))
            {
                ViewBag.Name = leadAdapterName;
                ViewBag.leadAdapterID = leadAdapterID;
                short ItemsPerPage = default(short);
                short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
                ViewBag.ItemsPerPage = ItemsPerPage;
                ViewBag.DateFormat = this.Identity.ToDateFormat();
                ViewLeadAdapterViewModel viewModel = new ViewLeadAdapterViewModel();
                return View("ViewLeadAdapter", viewModel);
            }
            else
            {
                return RedirectToAction("AccessDenied", "Error");
            }
            
        }

        [Route("leadadapters")]
        [SmarttouchAuthorize(AppModules.LeadAdapter, AppOperations.Read)]
        [MenuType(MenuCategory.LeadAdapters, MenuCategory.LeftMenuAccountConfiguration)]
        public ActionResult LeadAdapterList()
        {
            LeadAdapterViewModel viewModel = new LeadAdapterViewModel();
            viewModel.AccountID = UserExtensions.ToAccountID(this.Identity);
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            return View("LeadAdapterList", viewModel);
        }

        #endregion

        #region Lead Adapter Grid

        [SmarttouchAuthorize(AppModules.LeadAdapter, AppOperations.Read)]
        public ActionResult LeadAdaptersViewRead([DataSourceRequest] DataSourceRequest request, string name)
        {
            GetLeadAdapterListRequest leadadapterrequest = new GetLeadAdapterListRequest()
            {
                Query = name,
                Limit = request.PageSize,
                PageNumber = request.Page,
                AccountID = UserExtensions.ToAccountID(this.Identity)
            };
            GetLeadAdapterListResponse response = leadAdapterService.GetAllLeadAdapters(leadadapterrequest);
            response.LeadAdapters.Each(l =>
            {
                if (l.LastProcessed.HasValue)
                    l.LastProcessed = l.LastProcessed.Value.ToUtcBrowserDatetime();
            });
            return Json(new DataSourceResult
            {
                Data = response.LeadAdapters,
                Total = response.TotalHits
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region ViewLeadAdapter Grid

      //  [SmarttouchAuthorize(AppModules.LeadAdapter, AppOperations.Read)]
        public ActionResult ViewLeadAdaptersViewRead([DataSourceRequest] DataSourceRequest request, int leadAdapterID)
        {
            var usersPermissions = cachingService.GetUserPermissions(Thread.CurrentPrincipal.Identity.ToAccountID());
            List<byte> userModules = usersPermissions.Where(s => s.RoleId == (short)Thread.CurrentPrincipal.Identity.ToRoleID()).Select(s => s.ModuleId).ToList();
            if(userModules.Contains((byte)AppModules.ImportData) || userModules.Contains((byte)AppModules.LeadAdapter))
            {
                GetViewLeadAdapterListRequest viewleadadapterrequest = new GetViewLeadAdapterListRequest()
                {
                    Limit = request.PageSize,
                    PageNumber = request.Page,
                    LeadAdapterID = leadAdapterID
                };
                GetViewLeadAdapterListResponse response = leadAdapterService.GetAllViewLeadAdapters(viewleadadapterrequest);
                response.ViewLeadAdapters.Each(v =>
                {
                    v.ImportDate = v.ImportDate.ToUtcBrowserDatetime();
                });
                return Json(new DataSourceResult
                {
                    Data = response.ViewLeadAdapters,
                    Total = response.TotalHits
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return RedirectToAction("AccessDenied", "Error");
            }
            
        }

        #endregion

        #region Lead Adapter CRUD Operations

        [SmarttouchAuthorize(AppModules.LeadAdapter, AppOperations.Create)]
        public ActionResult InsertLeadAdapter(string leadAdapterViewModel)
        {
            LeadAdapterViewModel viewModel = JsonConvert.DeserializeObject<LeadAdapterViewModel>(leadAdapterViewModel);
            viewModel.AccountID = this.Identity.ToAccountID();
            viewModel.CreatedBy = this.Identity.ToUserID();
            viewModel.CreatedDateTime = DateTime.Now.ToUniversalTime();

            InsertLeadAdapterRequest request = new InsertLeadAdapterRequest() { LeadAdapterViewModel = viewModel, AccountId = this.Identity.ToAccountID() };
            if (viewModel.LeadAdapterType != LeadAdapterTypes.Facebook)
                leadAdapterService.InsertLeadAdapter(request);
            else
                leadAdapterService.InsertFacebookLeadAdapter(request);
            return Json(new { success = true, response = "" }, JsonRequestBehavior.AllowGet);
        }

        [SmarttouchAuthorize(AppModules.LeadAdapter, AppOperations.Edit)]
        public ActionResult UpdateLeadAdapter(string leadAdapterViewModel)
        {
            LeadAdapterViewModel viewModel = JsonConvert.DeserializeObject<LeadAdapterViewModel>(leadAdapterViewModel);
            viewModel.AccountID = this.Identity.ToAccountID();
            if (viewModel.LastProcessed.HasValue)
                viewModel.LastProcessed = viewModel.LastProcessed.Value.GetCorrectUtzDateTime();
            viewModel.ModifiedBy = this.Identity.ToUserID();
            viewModel.ModifiedDateTime = DateTime.Now.ToUniversalTime();
            UpdateLeadAdapterRequest request = new UpdateLeadAdapterRequest() { LeadAdapterViewModel = viewModel, AccountId = this.Identity.ToAccountID() };
            if (viewModel.LeadAdapterType != LeadAdapterTypes.Facebook)
                leadAdapterService.UpdateLeadAdapter(request);
            else
                leadAdapterService.UpdateFacebookLeadAdapter(request);

            return Json(new { success = true, response = "" }, JsonRequestBehavior.AllowGet);
        }

        [SmarttouchAuthorize(AppModules.LeadAdapter, AppOperations.Delete)]
        public JsonResult DeleteLeadAdapter(int leadAdapterID)
        {
            DeleteLeadAdapterRequest request = new DeleteLeadAdapterRequest(leadAdapterID);
            leadAdapterService.DeleteLeadAdapter(request);
            return Json(new { success = true, reponse = "" }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Lead Adapter Error List

        [Route("leadadaptererrorlist")]
        [SmarttouchAuthorize(AppModules.LeadAdapter, AppOperations.Read)]
        [MenuType(MenuCategory.EmptyTopMenuItem, MenuCategory.LeftMenuAccountConfiguration)]
        public ActionResult ErrorLeadAdapterList(int leadAdapterJobLogID, int leadAdapterID, string leadAdapterName)
        {
            ViewBag.LeadAdapterJobLogID = leadAdapterJobLogID;
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            ViewBag.LeadAdapterID = leadAdapterID;
            ViewBag.LeadAdapterName = leadAdapterName;
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            LeadAdapterJobLogDeailsViewModel viewModel = new LeadAdapterJobLogDeailsViewModel();
            return View("LeadAdapterErrorDetails", viewModel);
        }

        //[SmarttouchAuthorize(AppModules.LeadAdapter, AppOperations.Read)]
        public ActionResult ErrorLeadAdaptersViewRead([DataSourceRequest] DataSourceRequest request, int leadAdapterJobLogID)
        {
            var usersPermissions = cachingService.GetUserPermissions(Thread.CurrentPrincipal.Identity.ToAccountID());
            List<byte> userModules = usersPermissions.Where(s => s.RoleId == (short)Thread.CurrentPrincipal.Identity.ToRoleID()).Select(s => s.ModuleId).ToList();
            if (userModules.Contains((byte)AppModules.ImportData) || userModules.Contains((byte)AppModules.LeadAdapter))
            {
                GetLeadAdapterJobLogDetailsListRequest joblogdetailsrequest = new GetLeadAdapterJobLogDetailsListRequest()
                {
                    Limit = request.PageSize,
                    PageNumber = request.Page,
                    LeadAdapterJobLogID = leadAdapterJobLogID,
                    LeadAdapterRecordStatus = false
                };
                GetLeadAdapterJobLogDetailsListResponse response = leadAdapterService.GetAllLeadAdaptersJobLogDetails(joblogdetailsrequest);
                response.LeadAdapterErrorDeailsViewModel.Each(v =>
                {
                    v.CreatedDateTime = v.CreatedDateTime.ToUtcBrowserDatetime();
                });
                return Json(new DataSourceResult
                {
                    Data = response.LeadAdapterErrorDeailsViewModel,
                    Total = response.TotalHits
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return RedirectToAction("AccessDenied", "Error");
            }
            
        }

        #endregion
    }
}