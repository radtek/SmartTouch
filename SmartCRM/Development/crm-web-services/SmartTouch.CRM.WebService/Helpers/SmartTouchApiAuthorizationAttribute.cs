using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Http.Filters;

namespace SmartTouch.CRM.WebService.Helpers
{
    /// <summary>
    /// for SmartTouch Api Authorization Attributes
    /// </summary>
    public class SmartTouchApiAuthorizationAttribute : AuthorizationFilterAttribute
    {
        /// <summary>
        /// on Action excuting
        /// </summary>
        /// <param name="actionContext">actionContext</param>
        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            HttpResponseMessage response = null;
            if(SkipAuthorization(actionContext))
            {
                //skip authorization for allow anonymous attributes
                return;
            }
            if(actionContext.Request.Headers.Authorization == null)
            {
                response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("access_token missing in header."),
                    ReasonPhrase = "Authorization failed"
                };
            }
            else if(!actionContext.RequestContext.Principal.Identity.IsAuthenticated)
            {
                response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent("Your session has expired due to inactivity or you have exceeded the maximum allowed session time. Please refresh your browser and log in again to start a new session."),
                        ReasonPhrase = "Error occured while authenticating"
                    };
            }

            if(response == null)
            {
                base.OnAuthorization(actionContext);
            }
            else
            {
                actionContext.Response = response;
            }
        }

        /// <summary>
        /// for Skiping Authorization
        /// </summary>
        /// <param name="actionContext">actionContext</param>
        /// <returns></returns>
        private static bool SkipAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            Contract.Assert(actionContext != null);
            return actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any() || actionContext.ActionDescriptor.GetCustomAttributes<System.Web.Http.AllowAnonymousAttribute>().Any()
                 || actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any();
        }
    }
}