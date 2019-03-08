using Kendo.Mvc.UI;
using LandmarkIT.Enterprise.Extensions;
using SmartTouch.CRM.ApplicationServices.Messaging.ImportData;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
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
    public class ScrubQueController : SmartTouchController
    {
        readonly IAccountService accountService;

        public ScrubQueController(IAccountService accountService)
        {
            this.accountService = accountService;
        }

        [Route("scrubqueue")]
        [SmarttouchAuthorize(AppModules.NeverBounce, AppOperations.Read)]
        [MenuType(MenuCategory.NeverBounce, MenuCategory.LeftMenuAccountConfiguration)]
        [SmarttouchSessionStateBehaviour(System.Web.SessionState.SessionStateBehavior.Required)]
        public ActionResult ScrubQueue()
        {
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            return View("NeverBounceRequests");
        }

        [SmarttouchAuthorize(AppModules.NeverBounce, AppOperations.Read)]
        public ActionResult ScrubQueList([DataSourceRequest] DataSourceRequest request)
        {
            bool iSTAdmin = this.Identity.IsSTAdmin();
            int accountId = this.Identity.ToAccountID();
            IEnumerable<NeverBounceQueue> queue = accountService.GetNeverBounceRequests(new GetNeverBounceRequest()
            {
                AccountId = accountId,
                Limit = request.PageSize,
                PageNumber = request.Page
            }).Queue;

            if (queue.IsAny())
            {
                queue.Each(q =>
                {
                    q.IsAdmin = iSTAdmin;
                    q.AccountID = accountId;
                });
            }


            return Json(new DataSourceResult
            {
                Data = queue,
                Total = queue.IsAny()? queue.Select(s => s.TotalScrubQueCount).FirstOrDefault():0
            }, JsonRequestBehavior.AllowGet);
        }
        
        [Route("ur/{Id}/{status}")]
        public JsonResult UpdateRequests(int Id, short status)
        {
            int userId = this.Identity.ToUserID();
            UpdateNeverBounceResponse response = accountService.UpdateScrubQueueRequests(new UpdateNeverBounceRequest()
            {
                RequestedBy = userId,
                Request = new Domain.ImportData.NeverBounceRequest() { NeverBounceRequestID = Id, ServiceStatus = (NeverBounceStatus)status }
            });
            return Json(new { success = true, response = "" }, JsonRequestBehavior.AllowGet);
        }
    }
}