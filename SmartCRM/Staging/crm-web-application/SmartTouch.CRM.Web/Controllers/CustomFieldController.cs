using SmartTouch.CRM.ApplicationServices.Messaging.CustomFields;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Web.Utilities;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SmartTouch.CRM.Web.Controllers
{
    public class CustomFieldController : SmartTouchController
    {
        ICustomFieldService customFieldService;

        public CustomFieldController(ICustomFieldService customFieldService)
        {
            this.customFieldService = customFieldService;
        }

        [Route("customfields")]
        [SmarttouchAuthorize(AppModules.CustomFields, AppOperations.Create)]
        [MenuType(MenuCategory.CustomFields, MenuCategory.LeftMenuAccountConfiguration)]
        public ActionResult CustomFields()
        {
            ViewBag.AccountId = this.Identity.ToAccountID();
            GetAllCustomFieldTabsRequest request = new GetAllCustomFieldTabsRequest(ViewBag.AccountId);
            GetAllCustomFieldTabsResponse response = customFieldService.GetAllCustomFieldTabs(request);
            CustomFieldValueOptionViewModel valueOption = new CustomFieldValueOptionViewModel()
            {
                CustomFieldId = 0,
                CustomFieldValueOptionId = 0,
                IsDeleted = false,
                Value = "",
                Order = 0
            };
            CustomFieldViewModel fieldViewModel = new CustomFieldViewModel()
            {
                AccountID = this.Identity.ToAccountID(),
                FieldInputTypeId = FieldType.text,
                IsMandatory = false,
                SortId = 0,
                StatusId = FieldStatus.Active,
                Title = "New Field",
                ValueOptions = new List<CustomFieldValueOptionViewModel>()
            };
            CustomFieldSectionViewModel section = new CustomFieldSectionViewModel()
            {
                CustomFields = new List<CustomFieldViewModel>(),
                Name = "New Section"
            };
            CustomFieldTabViewModel tab = new CustomFieldTabViewModel()
            {
                Sections = new List<CustomFieldSectionViewModel>(),
                Name = "New Tab",
                AccountId = this.Identity.ToAccountID()
            };
            response.CustomFieldsViewModel.TabTemplate = tab;
            response.CustomFieldsViewModel.SectionTemplate = section;
            response.CustomFieldsViewModel.CustomFieldTemplate = fieldViewModel;
            response.CustomFieldsViewModel.ValueOptionTemplate = valueOption;
            return View("AddEditCustomFields", response.CustomFieldsViewModel);
        }

        [Route("customfields/delete")]
        [SmarttouchAuthorize(AppModules.CustomFields, AppOperations.Delete)]
        public ActionResult DeleteCustomFieldTab(string customFieldTabId)
        {
            if (string.IsNullOrEmpty(customFieldTabId))
            {
                return Json(new
                {
                    success = true,
                    response = "Could not delete the tab."
                }, JsonRequestBehavior.AllowGet);
            }
            DeleteCustomFieldTabRequest request = new DeleteCustomFieldTabRequest(int.Parse(customFieldTabId));
            customFieldService.DeleteCustomFieldTab(request);
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
