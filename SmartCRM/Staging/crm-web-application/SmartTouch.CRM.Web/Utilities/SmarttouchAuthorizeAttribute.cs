using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Threading;
using System.Configuration;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;

namespace SmartTouch.CRM.Web.Utilities
{
    public sealed class SmarttouchAuthorizeAttribute : AuthorizeAttribute
    {
        private AppModules _Module;
        private AppOperations _Action;
        ICachingService cachingService;
        public SmarttouchAuthorizeAttribute(AppModules module, AppOperations action)
        {
            this._Module = module;
            this._Action = action;
            this.cachingService = IoC.Container.GetInstance<ICachingService>();
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAuthenticated)
            {
                int accountId = Thread.CurrentPrincipal.Identity.ToAccountID();
                short roleId = Thread.CurrentPrincipal.Identity.ToRoleID();

                var accountOperations = cachingService.GetAccountPermissions(accountId);
                var usersPermissions = cachingService.GetUserPermissions(accountId);

                List<byte> userModules = usersPermissions.Where(s => s.RoleId == (short)roleId).Select(s => s.ModuleId).ToList();
                if (accountOperations.Contains((byte)_Module))
                {
                    if (!userModules.Contains((byte)_Module))
                    {
                        if (filterContext.HttpContext.Request.IsAjaxRequest() || string.Compare("GET", filterContext.HttpContext.Request.HttpMethod, true) != 0)
                        {
                            // Returns 403.
                            filterContext.Result = new ContentResult();
                            filterContext.HttpContext.Response.StatusCode = 403;
                            return;
                        }
                        else
                        {
                            // Returns 401.
                            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Error", action = "AccessDenied" }));
                            return;
                        }
                    }
                }
                else
                {
                    if (filterContext.HttpContext.Request.IsAjaxRequest() || string.Compare("GET", filterContext.HttpContext.Request.HttpMethod, true) != 0)
                    {
                        // Returns 403.
                        filterContext.Result = new ContentResult();
                        filterContext.HttpContext.Response.StatusCode = 403;
                        return;
                    }
                    else
                    {
                        // Returns 401.
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Error", action = "AccessDenied" }));
                        return;
                    }
                }
            }
        }
    }
}