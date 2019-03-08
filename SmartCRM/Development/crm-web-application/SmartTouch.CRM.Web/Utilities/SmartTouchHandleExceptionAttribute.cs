using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.Logging;

namespace SmartTouch.CRM.Web.Utilities
{
    /// <summary>
    /// Handles http:500 the error message.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SmartTouchHandleExceptionAttribute : HandleErrorAttribute
    {
        private static bool IsAjax(ExceptionContext filterContext)
        {
            return filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }
        /// <summary>
        /// Include method parameter values before rethrowing to Application_Error
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnException(ExceptionContext filterContext)
        {
            if (new HttpException(null, filterContext.Exception).GetHttpCode() != 500)
            {
                return;
            }

            string reason = filterContext.Exception.Message.ToString();
            if (string.IsNullOrEmpty(reason))
                reason = "[|Requested entity was deleted. Please try another|]";
            else if (!(filterContext.Exception is UnsupportedOperationException))
                reason = "[|An error has occurred, please try again later.|]";
            
            var exception = filterContext.Exception;
            var keys = filterContext.Controller.ControllerContext.HttpContext.Request.Form.AllKeys;
            keys.Each(k =>
            {
                exception.Data.Add(k, filterContext.Controller.ControllerContext.HttpContext.Request.Form.Get(k));
            });
            if(!exception.Data.Contains("URL"))
                exception.Data.Add("URL", filterContext.HttpContext.Request.Url.AbsoluteUri);
            if (!exception.Data.Contains("User"))
                exception.Data.Add("User", filterContext.HttpContext.User.Identity.ToUserID());

            if(IsAjax(filterContext))
            {
                filterContext.Result = new JsonResult()
                {
                    Data = new 
                    {
                        success = false,
                        error = reason //filterContext.Exception.Message
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                //ExceptionHandler.Current.HandleException(filterContext.Exception, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                Logger.Current.Error(reason, filterContext.Exception);
                //this will not call application_error event.
                filterContext.ExceptionHandled = true;
            }
            else
            {

                if ((filterContext.Exception is UnsupportedOperationException))
                {
                    filterContext.ExceptionHandled = true;
                    var Url = new UrlHelper(filterContext.RequestContext);
                    var url = Url.Action("AccessDenied", "Error");
                    filterContext.Result = new RedirectResult(url);
                }
            }

        }
    }
}