using Newtonsoft.Json;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.MarketingMessages;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Web.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Principal;
using Kendo.Mvc.UI;
using SmartTouch.CRM.ApplicationServices.Messaging.Subscriptions;
using LandmarkIT.Enterprise.Extensions;

namespace SmartTouch.CRM.Web.Controllers
{
    public class MarketingMessagesController : SmartTouchController
    {
        private readonly IAccountService accountService;
        private readonly IMarketingMessageService marketingService;
        private readonly ISubscriptionService subscriptionService;

        public MarketingMessagesController(IAccountService accountService, IMarketingMessageService marketingService, ISubscriptionService subscriptionService)
        {
            this.accountService = accountService;
            this.marketingService = marketingService;
            this.subscriptionService = subscriptionService;
        }

        /// <summary>
        /// Creating The Cookie
        /// </summary>
        /// <param name="cookieName"></param>
        /// <param name="Value"></param>
        /// <param name="days"></param>
        public void AddCookie(string cookieName, string Value, int days)
        {
            HttpCookie CartCookie = new HttpCookie(cookieName, Value);
            CartCookie.Expires = DateTime.Now.AddDays(days);
            Response.Cookies.Add(CartCookie);
        }
       
        /// <summary>
        /// Get All Messages
        /// </summary>
        /// <returns></returns>
        [Route("MarketingMessages")]
        [SmarttouchAuthorize(AppModules.MarketingMessageCenter, AppOperations.Read)]
        [MenuType(MenuCategory.MarketingMessageCenter, MenuCategory.LeftMenuAccountConfiguration)]
        [SmarttouchSessionStateBehaviour(System.Web.SessionState.SessionStateBehavior.Required)]
        public ActionResult GetAllMarketingMessages()
        {
            ViewBag.MarketingMessageID = 0;
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            ViewBag.ItemsPerPage = ItemsPerPage;
            return View("MarketingMessagesListView");
        }

        /// <summary>
        /// Add Message Notification
        /// </summary>
        /// <returns></returns>
        [Route("addmessage")]
        [SmarttouchAuthorize(AppModules.MarketingMessageCenter, AppOperations.Read)]
        [MenuType(MenuCategory.MarketingMessage, MenuCategory.LeftMenuAccountConfiguration)]
        [SmarttouchSessionStateBehaviour(System.Web.SessionState.SessionStateBehavior.Required)]
        public ActionResult AddMessage()
        {
            MarketingMessagesViewModel messagesViewModel = new MarketingMessagesViewModel();
            GetAllAccountSubscriptionTypesResponse response = subscriptionService.GetAllAccountsSubscriptionTypes(new GetAllAccountSubscriptionTypesRequest());
            ViewBag.AcccountSubscriptionTypes = response.subscriptionViewModel;
            return View("AddEditMarketingMessage", messagesViewModel);
        }

        /// <summary>
        /// Inserting Marketing Message
        /// </summary>
        /// <param name="marketingmessage"></param>
        /// <returns></returns>
        public ActionResult InsertMarketingMessage(string marketingmessage)
        {
            int userID = UserExtensions.ToUserID(this.Identity);
            MarketingMessagesViewModel marketingMessagesViewModel = JsonConvert.DeserializeObject<MarketingMessagesViewModel>(marketingmessage);
            marketingMessagesViewModel.AccountIDs = marketingMessagesViewModel.SelectedAccountIDs;
            marketingMessagesViewModel.CreatedBy = userID;
            marketingMessagesViewModel.CreatedDate = DateTime.Now.ToUniversalTime();
            marketingMessagesViewModel.LastUpdatedBy = userID;
            marketingMessagesViewModel.LastUpdatedDate = DateTime.Now.ToUniversalTime();
            if (marketingMessagesViewModel.ScheduleFrom != null && marketingMessagesViewModel.ScheduleTo != null)
            {
                marketingMessagesViewModel.ScheduleFrom = marketingMessagesViewModel.ScheduleFrom.Value.ToUserUtcDateTimeV2();
                marketingMessagesViewModel.ScheduleTo = marketingMessagesViewModel.ScheduleTo.Value.ToUserUtcDateTimeV2();

            }
            InsertMarketingMessageResponse response = marketingService.insertMarketingMessage(new InsertMarketingMessageRequest() { RequestedBy = userID, marketingMessageViewModel = marketingMessagesViewModel });
            return Json(new { success = true, response = response }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Editing Marketing Massage
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        [Route("editmessage")]
        [SmarttouchAuthorize(AppModules.MarketingMessageCenter, AppOperations.Read)]
        [MenuType(MenuCategory.MarketingMessage, MenuCategory.LeftMenuAccountConfiguration)]
        [SmarttouchSessionStateBehaviour(System.Web.SessionState.SessionStateBehavior.Required)]
        public ActionResult EditMessage(int messageId)
        {
            GetMarketingMessageResponseById response = marketingService.GetMarketingMessageById(new GetMarketingMessageRequestById() { MarketingMessageID = messageId });
            if (response.marketingMessagesViewModel.ScheduleTo.HasValue)
                response.marketingMessagesViewModel.ScheduleTo = response.marketingMessagesViewModel.ScheduleTo.Value.ToUtc();
            if(response.marketingMessagesViewModel.ScheduleFrom.HasValue)
                response.marketingMessagesViewModel.ScheduleFrom = response.marketingMessagesViewModel.ScheduleFrom.Value.ToUtc();

            response.marketingMessagesViewModel.CreatedDate = response.marketingMessagesViewModel.CreatedDate.ToUtc();

            GetAllAccountSubscriptionTypesResponse response1 = subscriptionService.GetAllAccountsSubscriptionTypes(new GetAllAccountSubscriptionTypesRequest());
            ViewBag.AcccountSubscriptionTypes = response1.subscriptionViewModel;
            return View("AddEditMarketingMessage", response.marketingMessagesViewModel);
        }

        /// <summary>
        /// Updating Marketing Message
        /// </summary>
        /// <param name="marketingmessage"></param>
        /// <returns></returns>
        public ActionResult UpdateMarketingMessage(string marketingmessage)
        {
            int userID = UserExtensions.ToUserID(this.Identity);
            MarketingMessagesViewModel marketingMessagesViewModel = JsonConvert.DeserializeObject<MarketingMessagesViewModel>(marketingmessage);
            marketingMessagesViewModel.AccountIDs = marketingMessagesViewModel.SelectedAccountIDs;
            marketingMessagesViewModel.CreatedBy = userID;
            marketingMessagesViewModel.CreatedDate = DateTime.Now.ToUniversalTime();
            marketingMessagesViewModel.LastUpdatedBy = userID;
            marketingMessagesViewModel.LastUpdatedDate = DateTime.Now.ToUniversalTime();

            if (marketingMessagesViewModel.ScheduleFrom != null && marketingMessagesViewModel.ScheduleTo != null)
            {
                marketingMessagesViewModel.ScheduleFrom = marketingMessagesViewModel.ScheduleFrom.Value.ToUserUtcDateTimeV2();
                marketingMessagesViewModel.ScheduleTo = marketingMessagesViewModel.ScheduleTo.Value.ToUserUtcDateTimeV2();

            }

            UpdateMarketingMessageResponse response = marketingService.updateMarketingMessage(new UpdateMarketingMessageRequest () { RequestedBy = userID, marketingMessagesViewModel = marketingMessagesViewModel });
            return Json(new { success = true, response = response }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Getting All Marketing Messages
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.MarketingMessageCenter,AppOperations.Read)]
        public ActionResult MarketingMessagesListViewRead([DataSourceRequest] DataSourceRequest request)
        {
            AddCookie("messagepagesize", request.PageSize.ToString(), 1);
            AddCookie("messagepagenumber", request.Page.ToString(), 1);
            int RoleId = UserExtensions.ToRoleID(this.Identity);
            GetAllMarketingMessagesResponse response = marketingService.GetAllMarketingMessages(new GetAllMarketingMessagesRequest() {
                RequestedBy = RoleId,
                Limit = request.PageSize,
                PageNumber = request.Page
            });
            foreach (var message in response.MarketingMessagesViewModel) {
                message.Title = message.MarketingMessageTitle.Length > 55 ? message.MarketingMessageTitle.Substring(0, 54) + "..." : message.Title = message.MarketingMessageTitle;
                message.CreatedDate = message.CreatedDate.ToUtc().ToUtcBrowserDatetime();
                message.ScheduleFrom = message.ScheduleFrom.HasValue ? message.ScheduleFrom.Value.ToUtc().ToUtcBrowserDatetime() :(DateTime?) null;
                message.ScheduleTo = message.ScheduleTo.HasValue ? message.ScheduleTo.Value.ToUtc().ToUtcBrowserDatetime() : (DateTime?)null;
            } 
            return Json(new DataSourceResult { Data = response.MarketingMessagesViewModel,Total = response.TotalHits}, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Deleting the Marketing Messages
        /// </summary>
        /// <param name="Ids"></param>
        /// <returns></returns>
        public ActionResult DeleteMessages(int[] Ids)
        {
            marketingService.deleteMarketingMessage(new DeleteMarketingMessageRequest() { MessageIds = Ids });
            return Json(new { success = true, response = "" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get all accounts
        /// </summary>
        /// <returns></returns>
        [Route("gaa/{Id}")]
        public JsonResult GetAllAccounts(byte Id)
        {
            GetAllAccountsBySubscriptionResponse response = accountService.GetAllAccountsBySubscription(new GetAllAccountsBySubscriptionRequest() {ID = Id });
            return Json(new { success = true, response = response.Accounts }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Get All Subscription types
        /// </summary>
        /// <returns></returns>
        public JsonResult GetAllSubscriptions()
        {
            GetAllAccountSubscriptionTypesResponse response = subscriptionService.GetAllAccountsSubscriptionTypes(new GetAllAccountSubscriptionTypesRequest());
            return Json(new { success = true, response = response.subscriptionViewModel }, JsonRequestBehavior.AllowGet);
        }
    }
}