using Newtonsoft.Json;
using SmartTouch.CRM.ApplicationServices.Messaging.SuppressionList;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.SuppressedEmails;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Web.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartTouch.CRM.Web.Controllers
{
    public class SuppressionListController : SmartTouchController
    {
        ISuppressionListService suppressionListService;

        public SuppressionListController(ISuppressionListService suppressionListService)
    {
            this.suppressionListService = suppressionListService;
        }

        // GET: SuppressedEmails
        public ActionResult Index()
        {
            return View();
        }

        [Route("suppressionlist")]
        [SmarttouchAuthorize(AppModules.SuppressionList, AppOperations.Read)]
        [MenuType(MenuCategory.SuppressionList, MenuCategory.LeftMenuAccountConfiguration)]
        public ActionResult AddSuppressionList()
        {
            SuppressionListViewModel viewModel = new SuppressionListViewModel();
            return View("SuppressionList", viewModel);
        }

        public JsonResult  InsertSuppressedEmails(string suppressedEmails)
        {
            IEnumerable<SuppressedEmailViewModel> viewModel = JsonConvert.DeserializeObject<IEnumerable<SuppressedEmailViewModel>>(suppressedEmails);
            InsertSuppressedEmailResponse response = suppressionListService.InsertSuppressedEmails(new InsertSuppressedEmailRequest { EmailViewModel = viewModel });
            return Json(new { success = true, response = response }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult InsertSuppressedDomains(string suppressedDomains)
        {
            IEnumerable<SuppressedDomainViewModel> viewModel = JsonConvert.DeserializeObject<IEnumerable<SuppressedDomainViewModel>>(suppressedDomains);
            InsertSuppressedDomainResponse response = suppressionListService.InertSuppressedDomains(new InsertSuppressedDomainRequest { DomainViewModel = viewModel });
            return Json(new { success = true, response = response }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult SearchForSuppressedEmailsAndDomains(string text,byte type)
        {
            int accountId = this.Identity.ToAccountID();
            if (type == 1)
            {
                var response = new SearchSuppressionListResponse<SuppressedEmail>();

                response = suppressionListService.SearchSuppressionList<SuppressedEmail>(new SearchSuppressionListRequest()
                {
                    Text = text,
                    IndexType = 1,
                    AccountId = accountId
                });
                return Json(new { success = true, response = response.Results }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var response = new SearchSuppressionListResponse<SuppressedDomain>();

                response = suppressionListService.SearchSuppressionList<SuppressedDomain>(new SearchSuppressionListRequest()
                {
                    Text = text,
                    IndexType = 2,
                    AccountId = accountId
                });
                return Json(new { success = true, response = response.Results }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
