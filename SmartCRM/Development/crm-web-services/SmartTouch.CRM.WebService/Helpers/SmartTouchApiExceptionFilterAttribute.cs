using i18n;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;

namespace SmartTouch.CRM.WebService.Helpers
{
    /// <summary>
    /// for SmartTouch Api Exception Filter Attribute
    /// </summary>
    public class SmartTouchApiExceptionFilterAttribute : ExceptionFilterAttribute 
    {
        /// <summary>
        /// On Exception
        /// </summary>
        /// <param name="context">context</param>
        public override void OnException(HttpActionExecutedContext context)
        {
            context.Exception.Data.Clear();
            context.ActionContext.ActionArguments.ToList().ForEach(a =>
                {
                    context.Exception.Data.Add(a.Key, JsonConvert.SerializeObject(a.Value));
                });
            ExceptionHandler.Current.HandleException(context.Exception, DefaultExceptionPolicies.LOG_ONLY_POLICY);
            string reason = context.Exception.Message.ToString();
           
            if (!string.IsNullOrEmpty(reason))
                Logger.Current.Informational("Exception Reason:" + reason);
            
            if (string.IsNullOrEmpty(reason))
                reason = "[|Requested entity was deleted. Please try another|]";
            else if (!(context.Exception is UnsupportedOperationException) && !reason.Contains("Contact already exists"))
                reason = "[|An error occured, please contact administrator|]";
            
            var resp = new HttpResponseMessage(context.Exception.ConvertToHttpStatusCode())
            {
                Content = new StringContent(HttpContext.Current.ParseAndTranslate(reason)),
                ReasonPhrase = "Error"
            };
            context.Response = resp;
        }
    }
}