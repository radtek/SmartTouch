using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LandmarkIT.Enterprise.Utilities.Logging;
using System.IO;
using System.Globalization;

namespace SmartTouch.CRM.Web.Utilities
{
    public sealed class SmartTouchAOSPActionLogFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var action = filterContext.ActionDescriptor.ActionName;
            var controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;

            Logger.Current.Informational(string.Format("Action method started: {0} at {1}", 
                Path.Combine(action,controller) , DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)));
            base.OnActionExecuting(filterContext);
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            string controller = filterContext.RouteData.Values["controller"].ToString();
            string action = filterContext.RouteData.Values["action"].ToString();

            base.OnResultExecuted(filterContext);
            Logger.Current.Informational(string.Format("Action method ended: {0} at {1}", 
                Path.Combine(action, controller), DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)));
        }
    }
}