using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Web.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartTouch.CRM.Web.Controllers
{
    public class EmailValidatorController : SmartTouchController
    {
        IAccountService accountService;

        public EmailValidatorController(IAccountService accountService)
        {
            this.accountService = accountService;
        }

        [Route("emailvalidator")]
        [SmarttouchAuthorize(AppModules.EmailValidator, AppOperations.Read)]
        [MenuType(MenuCategory.EmailValidator, MenuCategory.LeftMenuAccountConfiguration)]
        [SmarttouchSessionStateBehaviour(System.Web.SessionState.SessionStateBehavior.Required)]
        public ActionResult EmailValidator()
        {
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            return View("EmailValidator");
        }

        [SmarttouchAuthorize(AppModules.EmailValidator, AppOperations.Read)]
        public ActionResult EmailsValidation(int[] tagIds, int[] searchDefinitionIds, int totalCount)
        {
            if (tagIds != null || searchDefinitionIds != null)
                accountService.InsertNeverBounceRequest(new InsertNeverBounceRequest() { AccountId = this.Identity.ToAccountID(), RequestedBy = this.Identity.ToUserID(), SearchdefinitionIds = searchDefinitionIds, 
                    TagIds = tagIds, TotalCount = totalCount });
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }
    }
}