using SmartTouch.CRM.WebService.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;

namespace SmartTouch.CRM.WebService
{
    /// <summary>
    /// Creating WebApiConfig class
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// For Configaration Register
        /// </summary>
        /// <param name="config">config</param>
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            var cors = new EnableCorsAttribute(@"*", "*", "*");
            cors.SupportsCredentials = true;
            config.EnableCors(cors);

            // Web API routes
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new {id = RouteParameter.Optional }
            );

            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.MapHttpAttributeRoutes();
            config.Filters.Add(new SmartTouchApiExceptionFilterAttribute());
            config.Filters.Add(new SmartTouchApiAuthorizationAttribute());
            config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
        }
    }
}
