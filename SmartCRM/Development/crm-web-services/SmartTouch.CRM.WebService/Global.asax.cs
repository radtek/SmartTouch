using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SimpleInjector;
using SmartTouch.CRM.ApplicationServices;
using SmartTouch.CRM.WebService.DependencyResolution;
using System;
using System.Configuration;
using System.Diagnostics.Tracing;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;

namespace SmartTouch.CRM.WebService
{
    // visit http://go.microsoft.com/fwlink/?LinkId=301868
    /// <summary>
    /// Note: For instructions on enabling IIS7 classic mode, 
    /// </summary>
    public class WebApiApplication : System.Web.HttpApplication
    {
        /// <summary>
        /// for application start up
        /// </summary>
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            Container container = new Container();
            IoC.Configure(container);
            Context.SetSessionStateBehavior(System.Web.SessionState.SessionStateBehavior.ReadOnly);
            AreaRegistration.RegisterAllAreas();
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            InitializeAutoMapper.Initialize();
            GlobalConfiguration.Configuration.EnsureInitialized();
            Logger.Current.CreateRollingFlatFileListener(EventLevel.Verbose, ConfigurationManager.AppSettings["WEBSERVICE_LOG_FILE_PATH"], 2048);
            ExceptionHandler.Current.AddDefaultLogAndRethrowPolicy();
            ExceptionHandler.Current.AddDefaultLogOnlyPolicy();
            i18n.LocalizedApplication.Current.DefaultLanguage = "en";
            i18n.UrlLocalizer.UrlLocalizationScheme = i18n.UrlLocalizationScheme.Void;            
        }

        /// <summary>
        /// for application error
        /// </summary>
        protected void Application_Error()
        {
            Exception ex = Server.GetLastError();
            Logger.Current.Error("Unhandled error caught", ex, "Application_Error", "Global.asax.cs");
        }
    }
}
