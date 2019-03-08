using Kendo.Mvc.UI;
using Newtonsoft.Json;
using SmartTouch.CRM.ApplicationServices.Messaging.DropdownValues;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Web.Utilities;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SmartTouch.CRM.Web.Controllers
{
    public class DropdownValuesController : SmartTouchController
    {
        private readonly IDropdownValuesService dropdownValuesService;
        private readonly ICachingService cachingService;

        public DropdownValuesController(IDropdownValuesService dropdownValuesService, ICachingService cachingService)
        {
            this.dropdownValuesService = dropdownValuesService;
            this.cachingService = cachingService;
        }

        public ActionResult DropdownValuesViewRead([DataSourceRequest] DataSourceRequest request, string name)
        {
            GetDropdownListResponse response = dropdownValuesService.GetAll(new GetDropdownListRequest() { Query = name, Limit = request.PageSize, PageNumber = request.Page, AccountID = UserExtensions.ToAccountID(this.Identity) });
            return Json(new DataSourceResult
            {
                Data = response.DropdownValuesViewModel,
                Total = response.TotalHits
            }, JsonRequestBehavior.AllowGet);
        }

        [Route("dropdownfields")]
        [SmarttouchAuthorize(AppModules.DropdownFields, AppOperations.Read)]
        [MenuType(MenuCategory.DropdownFields, MenuCategory.LeftMenuAccountConfiguration)]
        [OutputCache(Duration = 30)]
        public ActionResult DropdownValuesList()
        {
            DropdownViewModel viewModel = new DropdownViewModel();
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            return View("DropdownList", viewModel);
        }

        [HttpGet]
        public JsonResult GetPhoneTypes(int DropDownID)
        {
            GetDropdownValueRequest req = new GetDropdownValueRequest() { DropdownID = DropDownID, AccountId = this.Identity.ToAccountID() };
            GetDropdownValueResponse resp = dropdownValuesService.GetDropdownValue(req);
            return Json(resp.DropdownValues.DropdownValuesList.ToArray(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCommunities(int DropDownID)
        {
            GetDropdownValueRequest req = new GetDropdownValueRequest() { DropdownID = DropDownID, AccountId = this.Identity.ToAccountID() };
            GetDropdownValueResponse resp = dropdownValuesService.GetDropdownValue(req);
            return Json(resp.DropdownValues.DropdownValuesList.ToArray(), JsonRequestBehavior.AllowGet);
        }
        [Route("editdropdown")]
        [SmarttouchAuthorize(AppModules.DropdownFields, AppOperations.Edit)]
        public ActionResult EditDropdown(byte dropdownId)
        {
            GetDropdownValueRequest request = new GetDropdownValueRequest();
            request.DropdownID = dropdownId;
            request.AccountId = this.Identity.ToAccountID();
            
            GetDropdownValueResponse response = dropdownValuesService.GetDropdownValue(request);
            ViewBag.IsModal = true;
            var view = PartialView("_EditDropdown", response.DropdownValues);
            return view;
        }

        [SmarttouchAuthorize(AppModules.DropdownFields, AppOperations.Create)]
        public ActionResult InsertDropdown(string dropdownViewModel)
        {
            DropdownViewModel viewModel = JsonConvert.DeserializeObject<DropdownViewModel>(dropdownViewModel);
            viewModel.AccountID = this.Identity.ToAccountID();
            foreach (DropdownValueViewModel dvvm in viewModel.DropdownValuesList)
            {
                dvvm.AccountID = viewModel.AccountID;
                if (dvvm.IsNewField)
                    dvvm.DropdownValueID = 0;
            }
            InsertDropdownRequest request = new InsertDropdownRequest();
            request.DropdownViewModel = viewModel;
            dropdownValuesService.InsertDropdownValue(request);
            cachingService.AddDropdownValues(this.Identity.ToAccountID());
            return Json(new { success = true, response = "" }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetOppoertunityStageGroups()
        {
            GetOpportunityStageGroupRequest request = new GetOpportunityStageGroupRequest();
            request.AccountId = this.Identity.ToAccountID();
            GetOpportunityStageGroupResponse response = dropdownValuesService.GetOppoertunityStageGroups(request);
            return Json(new { success = true, response = response.OpportunityGroups }, JsonRequestBehavior.AllowGet);
        }

        [Route("dropdownvaluesview")]
        public ActionResult DropdownvaluesView(int dropdownID)
        {
            GetDropdownValueRequest request = new GetDropdownValueRequest();
            request.DropdownID = dropdownID;
            request.AccountId = this.Identity.ToAccountID();
            GetDropdownValueResponse response = dropdownValuesService.GetDropdownValue(request);
            return PartialView("_DropdownvaluesHtml", response.DropdownValues);
        }        
    }
}