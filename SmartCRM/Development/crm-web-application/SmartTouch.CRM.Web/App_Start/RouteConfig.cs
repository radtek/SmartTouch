using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmartTouch.CRM.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapMvcAttributeRoutes();
            routes.MapRoute(name: "Dashboard", url: "dashboard", defaults: new
            {
                controller = "Dashboard",
                action = "DashboardList",
                id = UrlParameter.Optional
            });
            routes.MapRoute(name: "Campaigns", url: "campaigns", defaults: new
            {
                controller = "Campaign",
                action = "Campaigns",
                id = UrlParameter.Optional
            });
            routes.MapRoute(name: "Forms", url: "forms", defaults: new
            {
                controller = "Form",
                action = "Forms",
                id = UrlParameter.Optional
            });
            routes.MapRoute(name: "contacts", url: "contacts", defaults: new
            {
                controller = "Contact",
                action = "Contacts",
                id = UrlParameter.Optional
            });
            routes.MapRoute(name: "users", url: "users", defaults: new
            {
                controller = "User",
                action = "UserList",
                id = UrlParameter.Optional
            });
            routes.MapRoute(name: "accounts", url: "accounts", defaults: new
            {
                controller = "Account",
                action = "AccountList",
                id = UrlParameter.Optional
            });
            routes.MapRoute(name: "error", url: "error", defaults: new
            {
                controller = "Error",
                action = "Index",
                id = UrlParameter.Optional
            });
            routes.MapRoute(name: "tags", url: "tags", defaults: new
            {
                controller = "Tag",
                action = "TagList",
                id = UrlParameter.Optional
            });
            routes.MapRoute(name: "roles", url: "roles", defaults: new
            {
                controller = "Role",
                action = "AddRolePermissions",
                id = UrlParameter.Optional
            });
            routes.MapRoute(name: "leadadapter", url: "leadadapter", defaults: new
            {
                controller = "LeadAdapter",
                action = "TagList",
                id = UrlParameter.Optional
            });
            routes.MapRoute(name: "dropdownfields", url: "dropdownfields", defaults: new
            {
                controller = "DropdownValues",
                action = "DropdownValuesList",
                id = UrlParameter.Optional
            });
            routes.MapRoute(name: "importdata", url: "importdata", defaults: new
            {
                controller = "ImportData",
                action = "ImportDataList",
                id = UrlParameter.Optional
            });
            routes.MapRoute(name: "apikeys", url: "apikeys", defaults: new
            {
                controller = "APIKeys",
                action = "GetAllApiKeys",
                id = UrlParameter.Optional
            });
            routes.MapRoute(name: "healthreport", url: "healthreport", defaults: new
            {
                controller = "Account",
                action = "GetHealthReport",
                id = UrlParameter.Optional
            });
            routes.MapRoute(name: "Default", url: "{controller}/{action}/{id}", defaults: new
            {
                controller = "Login",
                action = "Login",
                id = UrlParameter.Optional
            });
        }
    }
}
