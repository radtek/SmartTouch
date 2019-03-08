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
    public class ActionController : SmartTouchController
    {

        ICachingService cachingService;

        public ActionController(ICachingService cachingService)
        {
            this.cachingService = cachingService;
        }

        /// <summary>
        /// Actionses this instance.
        /// </summary>
        /// <returns></returns>

        [SmarttouchAuthorize(AppModules.ContactActions, AppOperations.Read)]
        [MenuType(MenuCategory.ActionList, MenuCategory.LeftMenuCRM)]
        public ActionResult Actions()
        {
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            ViewBag.IsMyActions = true;
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            var actionTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.ActionType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            List<SelectListItem> selectedListItems = new List<SelectListItem>();
            SelectListItem selectListItem1 = new SelectListItem();
            selectListItem1.Text = "Select";
            selectListItem1.Value = "0";
            selectedListItems.Add(selectListItem1);
            foreach (DropdownValueViewModel item in actionTypes)
            {
                SelectListItem selectListItem = new SelectListItem();
                selectListItem.Text = item.DropdownValue;
                selectListItem.Value = item.DropdownValueID.ToString();
                selectedListItems.Add(selectListItem);
            }

            ViewBag.ActionTypes = selectedListItems;

            return View("~/Views/Contact/ActionList.cshtml");        
        }

       

        //
        // GET: /Action/
        public ActionResult Index()
        {
            return View();
        }
	}
}