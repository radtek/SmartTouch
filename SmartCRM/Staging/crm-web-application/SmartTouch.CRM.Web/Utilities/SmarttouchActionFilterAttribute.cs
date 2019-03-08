using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Web.Mvc;
using System.Web.Routing;
using System.Threading;
using System.Configuration;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.ApplicationServices.Messaging.AccountUnsubscribeView;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;

namespace SmartTouch.CRM.Web.Utilities
{
    public sealed class SmarttouchActionFilterAttribute : ActionFilterAttribute
    {
        IAccountSettingsService accountUnsubscribeService;
        ICampaignService campaignService;
        IAccountService accountService;
        IUrlService urlService;
        public SmarttouchActionFilterAttribute()
        {


            // this._Action = action;
            this.accountUnsubscribeService = IoC.Container.GetInstance<IAccountSettingsService>();
            this.campaignService = IoC.Container.GetInstance<ICampaignService>();
            this.accountService = IoC.Container.GetInstance<IAccountService>();
            this.urlService = IoC.Container.GetInstance<IUrlService>();
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
           
           

            

        
            if (filterContext.Result != null)
                base.OnActionExecuting(filterContext);
        }

        public class RequestedViewResult : ViewResult
        {
            public RequestedViewResult(string viewName, int? contactId, int? campaignId, string emailId, string accountLogo, string accountName, string privacyPolicy, string accountAddress)
            {
                ViewBag.contactID = contactId;
                ViewBag.campaignID = campaignId;
                ViewBag.emailId = emailId;
                ViewBag.accountLogo = accountLogo;
                ViewBag.accountName = accountName;
                ViewBag.privacyPolicy = privacyPolicy;
                ViewBag.accountAddress = accountAddress;
                ViewName = "~/Views/UnSubscribeViews/" + viewName + ".cshtml";
            }
        }

    }
}