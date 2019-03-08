using Kendo.Mvc.UI;
using LandmarkIT.Enterprise.Extensions;
using Newtonsoft.Json;
using SmartTouch.CRM.ApplicationServices.Messaging.LeadScore;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Web.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartTouch.CRM.Web.Controllers
{
    public class LeadScoreController : SmartTouchController
    {
        ILeadScoreRuleService leadScoreService;

        ICachingService cachingService;

        public LeadScoreController(ILeadScoreRuleService leadScoreService, ICachingService cachingService)
        {
            this.leadScoreService = leadScoreService;
            this.cachingService = cachingService;
        }

        public void AddCookie(string cookieName, string Value, int days)
        {
            HttpCookie CartCookie = new HttpCookie(cookieName, Value);
            CartCookie.Expires = DateTime.Now.AddDays(days);
            Response.Cookies.Add(CartCookie);
        }

        [SmarttouchAuthorize(AppModules.LeadScore, AppOperations.Read)]
        public ActionResult RulesListViewRead([DataSourceRequest] DataSourceRequest request, string name)
        {
            AddCookie("leadscorepagesize", request.PageSize.ToString(), 1);
            AddCookie("leadscorepagenumber", request.Page.ToString(), 1);
            int AccountID = this.Identity.ToAccountID();
            int RoleID = this.Identity.ToRoleID();
            IEnumerable<UserPermission> userPermissions = cachingService.GetUserPermissions(AccountID);
            userPermissions = userPermissions.Where(i => i.RoleId == RoleID);
            IEnumerable<byte> userModules = userPermissions.Select(x => x.ModuleId);
            GetLeadScoreListResponse response = leadScoreService.GetLeadScoresList(new GetLeadScoreListRequest()
            {
                Query = name,
                Limit = request.PageSize,
                PageNumber = request.Page,
                AccountID = UserExtensions.ToAccountID(this.Identity),
                SortField = request.Sorts.Count > 0 ? request.Sorts.First().Member : GetPropertyName<LeadScoreRuleViewModel, DateTime>(t => t.ModifiedOn),
                SortDirection = request.Sorts.Count > 0 ? request.Sorts.First().SortDirection : System.ComponentModel.ListSortDirection.Descending,
                Modules = userModules
            });
            return Json(new DataSourceResult
            {
                Data = response.LeadScoreViewModel,
                Total = response.TotalHits
            }, JsonRequestBehavior.AllowGet);
        }

        [Route("leadscorerules")]
        [SmarttouchAuthorize(AppModules.LeadScore, AppOperations.Read)]
        [MenuType(MenuCategory.LeadScore, MenuCategory.LeftMenuAccountConfiguration)]
        [OutputCache(Duration = 30)]
        public ActionResult RulesList(int? accountId)
        {
            LeadScoreRuleViewModel viewModel = new LeadScoreRuleViewModel();
            ViewBag.leadscoreID = 0;
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            return View("RulesList", viewModel);
        }

        [SmarttouchAuthorize(AppModules.LeadScore, AppOperations.Create)]
        public ActionResult _AddLeadScore()
        {
            LeadScoreRuleViewModel viewModel = new LeadScoreRuleViewModel();
            viewModel.AccountID = this.Identity.ToAccountID();
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            viewModel.TourTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.TourType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            viewModel.LeadsourceTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LeadSources).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            viewModel.NoteCategories = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.NoteCategory).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            viewModel.ActionTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.ActionType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            GetLeadScoreCategoriesResponse categoriesResponse = leadScoreService.GetLeadScoreCategories(new GetLeadScoreCategoriesRequest() { });
            viewModel.Categories = categoriesResponse.Categories;
            GetLeadScoreConditionsResponse conditionsResponce = leadScoreService.GetLeadScoreConditions(new GetLeadScoreConditionsRequest() { });
            viewModel.Conditions = conditionsResponce.Conditions;
            var leadScoreConditionValues = new List<LeadScoreConditionValueViewModel>();
            var newConditionValue = new LeadScoreConditionValueViewModel()
            {
                LeadScoreConditionValueId = 0,
                LeadScoreRuleId = 0,
                Value = "",
                ValueType = LeadScoreValueType.PageDuration
            };
            leadScoreConditionValues.Add(newConditionValue);
            viewModel.LeadScoreConditionValues = leadScoreConditionValues;
            return PartialView("_AddEditRule", viewModel);
        }

        [SmarttouchAuthorize(AppModules.LeadScore, AppOperations.Create)]
        public ActionResult InsertLeadScore(LeadScoreRuleViewModel viewModel)
        {
            int UserID = this.Identity.ToUserID();
            int AccountID = this.Identity.ToAccountID();
            viewModel.CreatedBy = UserID;
            viewModel.ModifiedBy = UserID;
            viewModel.AccountID = AccountID;
            viewModel.CreatedOn = DateTime.Now.ToUniversalTime();
            viewModel.ModifiedOn = DateTime.Now.ToUniversalTime();
            InsertLeadScoreRuleRequest request = new InsertLeadScoreRuleRequest()
            {
                LeadScoreRuleViewModel = viewModel,
                RequestedBy = UserID,
                AccountId = AccountID
            };
            leadScoreService.CreateRule(request);
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        [Route("changescore")]
        [SmarttouchAuthorize(AppModules.LeadScore, AppOperations.Edit)]
        public ActionResult ChangeScore()
        {
            return PartialView("_ChangeScore");
        }

        [SmarttouchAuthorize(AppModules.LeadScore, AppOperations.Edit)]
        public JsonResult ChangeLeadScore(string leadScoreRoleViewModel)
        {
            LeadScoreRuleViewModel viewModel = JsonConvert.DeserializeObject<LeadScoreRuleViewModel>(leadScoreRoleViewModel);
            viewModel.ModifiedBy = UserExtensions.ToUserID(this.Identity);
            UpdateLeadScoreRuleRequest request = new UpdateLeadScoreRuleRequest()
            {
                LeadScoreRuleViewModel = viewModel
            };
            leadScoreService.UpdateRule(request);
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        [Route("editrule/")]
        [SmarttouchAuthorize(AppModules.LeadScore, AppOperations.Edit)]
        public ActionResult EditLeadScore(int leadScoreRuleMapId)
        {
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            GetLeadScoreResponse response = leadScoreService.GetLeadScoreRule(new GetLeadScoreRequest(leadScoreRuleMapId)
            {

            });
            response.LeadScoreViewModel.DateFormat = this.Identity.ToDateFormat();
            response.LeadScoreViewModel.CreatedOn = response.LeadScoreViewModel.CreatedOn.ToUserUtcDateTimeV2();
            response.LeadScoreViewModel.LeadsourceTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LeadSources).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            response.LeadScoreViewModel.TourTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.TourType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            response.LeadScoreViewModel.NoteCategories = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.NoteCategory).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            response.LeadScoreViewModel.ActionTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.ActionType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            GetLeadScoreCategoriesResponse categoriesResponse = leadScoreService.GetLeadScoreCategories(new GetLeadScoreCategoriesRequest() { });
            response.LeadScoreViewModel.Categories = categoriesResponse.Categories;
            GetLeadScoreConditionsResponse conditionsResponce = leadScoreService.GetLeadScoreConditions(new GetLeadScoreConditionsRequest() { });
            var leadScoreConditionValues = new List<LeadScoreConditionValueViewModel>();
            var newConditionValue = new LeadScoreConditionValueViewModel()
            {
                LeadScoreConditionValueId = 0,
                LeadScoreRuleId = 0,
                Value = "",
                ValueType = LeadScoreValueType.PageDuration
            };
            leadScoreConditionValues.Add(newConditionValue);
            if (!response.LeadScoreViewModel.LeadScoreConditionValues.IsAny())
                response.LeadScoreViewModel.LeadScoreConditionValues = leadScoreConditionValues;


            response.LeadScoreViewModel.Conditions = conditionsResponce.Conditions;
            ViewBag.IsModal = true;
            var view = PartialView("_AddEditRule", response.LeadScoreViewModel);
            return view;
        }

        [SmarttouchAuthorize(AppModules.LeadScore, AppOperations.Edit)]
        public JsonResult UpdateLeadScore(LeadScoreRuleViewModel viewModel)
        {
            int UserID = this.Identity.ToUserID();
            viewModel.ModifiedBy = UserID;
            viewModel.ModifiedOn = DateTime.Now.ToUniversalTime();
            viewModel.CreatedOn = viewModel.CreatedOn.ToUniversalTime();
            UpdateLeadScoreRuleRequest request = new UpdateLeadScoreRuleRequest()
            {
                LeadScoreRuleViewModel = viewModel,
                RequestedBy = UserID,
                AccountId = this.Identity.ToAccountID()
            };
            leadScoreService.UpdateRule(request);
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        [Route("deleterule/")]
        [SmarttouchAuthorize(AppModules.LeadScore, AppOperations.Delete)]
        public ActionResult DeleteRule(string leadScoreData)
        {
            DeleteLeadScoreRequest request = JsonConvert.DeserializeObject<DeleteLeadScoreRequest>(leadScoreData);
            request.accountID = UserExtensions.ToAccountID(this.Identity);
            leadScoreService.UpdateLeadScoreStatus(request);
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        [SmarttouchAuthorize(AppModules.LeadScore, AppOperations.Read)]
        public JsonResult GetCategories()
        {
            GetCategoriesResponse response = leadScoreService.GetCategories(new GetCategoriesRequest()
            {
                accountId = this.Identity.ToAccountID(),
                RoleId = this.Identity.ToRoleID()
            });
            return Json(new
            {
                success = true,
                response = response.Categories
            }, JsonRequestBehavior.AllowGet);
        }

        [SmarttouchAuthorize(AppModules.LeadScore, AppOperations.Read)]
        public JsonResult GetConditions(int categoryID)
        {
            GetConditionsResponse response = leadScoreService.GetConditions(new GetConditionsRequest(categoryID));
            return Json(new
            {
                success = true,
                response = response.Conditions
            }, JsonRequestBehavior.AllowGet);
        }

        [Route("getleadscore")]
        [SmarttouchAuthorize(AppModules.LeadScore, AppOperations.Read)]
        public JsonResult GetLeadScoreRule(int leadScoreId)
        {
            GetLeadScoreResponse response = leadScoreService.GetLeadScoreRule(new GetLeadScoreRequest(leadScoreId)
            {

            });
            return Json(response.LeadScoreViewModel, JsonRequestBehavior.AllowGet);
        }

        [SmarttouchAuthorize(AppModules.Campaigns, AppOperations.Read)]
        public JsonResult GetCampaigns()
        {
            int accountID = UserExtensions.ToAccountID(this.Identity);
            GetCampaignsResponse response = leadScoreService.GetCampaigns(new GetCampaignsRequest()
            {
                accountId = accountID
            });
            return Json(new
            {
                success = true,
                response = response.Campaigns
            }, JsonRequestBehavior.AllowGet);
        }

        [SmarttouchAuthorize(AppModules.Forms, AppOperations.Read)]
        public JsonResult GetForms()
        {
            int accountID = UserExtensions.ToAccountID(this.Identity);
            GetFormResponse response = leadScoreService.GetForms(new GetFormsRequest()
            {
                AccountId = accountID,
                RoleId = this.Identity.ToRoleID(),
                RequestedBy = this.Identity.ToUserID()
            });
            return Json(new
            {
                success = true,
                response = response.Forms
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
