using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices;
using SmartTouch.CRM.Plugins.Utilities.ImplicitSync;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace SmartTouch.CRM.Plugins
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ConcurrentUsers.ConcurrentUser = new List<ConcurrentUser>();
            InitializeAutoMapper.Initialize();
            #region Logging Configuration
            
            Logger.Current.CreateRollingFlatFileListener(EventLevel.Verbose, ConfigurationManager.AppSettings["IMPLICITSYNC_SERVICE_LOG_FILE_PATH"], 2048);
            ExceptionHandler.Current.AddDefaultLogAndRethrowPolicy();
            ExceptionHandler.Current.AddDefaultLogOnlyPolicy();
            #endregion
        }
    }
}
