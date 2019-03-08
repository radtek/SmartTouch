using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using SmartTouch.CRM.Web.Utilities;
using SimpleInjector;
using SmartTouch.CRM.ApplicationServices;
using LandmarkIT.Enterprise.Utilities.Logging;
using System.Diagnostics.Tracing;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using XSockets.Plugin.Framework;
using XSockets.Core.Common.Socket;
using XSockets.Core.Configuration;
using XSockets.Core.Common.Configuration;
using System.Diagnostics;
using i18n;
using i18n.Helpers;
//using SmartTouch.CRM.Web.XSocketsNET;

namespace SmartTouch.CRM.Web
{
    // Note: For instructions on enabling IIS7 classic mode, 
    // visit http://go.microsoft.com/fwlink/?LinkId=301868
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Container container = new Container();
            IoC.Configure(container);
            //RouteTable.Routes.MapHubs();
            //assign instrumentation key to appinsights
            Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration.Active.InstrumentationKey = System.Web.Configuration.WebConfigurationManager.AppSettings["iKey"];
            ControllerBuilder.Current.SetControllerFactory(typeof(SmarttouchControllerFactory)); 
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //ControllerBuilder.Current.SetControllerFactory(typeof(SmarttouchControllerFactory));
            InitializeAutoMapper.Initialize();
            //GlobalConfiguration.Configuration.EnsureInitialized(); 
            HttpContext.Current.Application["image_hostedpath"] = ConfigurationManager.AppSettings["IMAGE_HOSTING_SERVICE_URL"];
            HttpContext.Current.Application["webservice_url"] = ConfigurationManager.AppSettings["WEBSERVICE_URL"];
            Logger.Current.CreateRollingFlatFileListener(EventLevel.Verbose, ConfigurationManager.AppSettings["WEBAPP_LOG_FILE_PATH"], 2048);
            ExceptionHandler.Current.AddDefaultLogAndRethrowPolicy();
            ExceptionHandler.Current.AddDefaultLogOnlyPolicy();
            i18n.LocalizedApplication.Current.DefaultLanguage = "en";
            i18n.UrlLocalizer.UrlLocalizationScheme = i18n.UrlLocalizationScheme.Void;

            var defaultJsonFactory = ValueProviderFactories.Factories.OfType<JsonValueProviderFactory>().FirstOrDefault();
            var index = ValueProviderFactories.Factories.IndexOf(defaultJsonFactory);
            ValueProviderFactories.Factories.Remove(defaultJsonFactory);
            ValueProviderFactories.Factories.Insert(index, new SmartTouchJsonValueProviderFactory());

            bool isHttpsMode = false;
            string httpsMode = System.Configuration.ConfigurationManager.AppSettings["IsHttpsMode"];
            bool.TryParse(httpsMode, out isHttpsMode);

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine()); 

            var masterAccountDns = ConfigurationManager.AppSettings["MASTER_ACCOUNT_DNS"];
            if (masterAccountDns.Equals("localhost"))
            {
                using (var socketServerContainer = Composable.GetExport<IXSocketServerContainer>())
                {
                    socketServerContainer.StartServers();
                }
            }
            else
            {
                using (var socketServerContainer = Composable.GetExport<IXSocketServerContainer>())
                {
                    IList<IConfigurationSetting> configurationSettings = new List<IConfigurationSetting>();
                    //string uri = string.Empty;
                    IConfigurationSetting configSetting = null;
                    Uri uri = null;

                    if (isHttpsMode)
                    {
                        string sslKey = System.Configuration.ConfigurationManager.AppSettings["SSLSerialNumber"];

                        uri = new Uri("wss://" + ConfigurationManager.AppSettings["MASTER_ACCOUNT_DNS"]
                                + ":" + ConfigurationManager.AppSettings["WEBSOCKET_PORT"]);

                        X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                        store.Open(OpenFlags.ReadOnly);

                        var certificate = store.Certificates.Find(X509FindType.FindBySerialNumber, sslKey, false).OfType<X509Certificate2>().FirstOrDefault();
                        if (certificate != null)
                        {
                            Logger.Current.Informational("certificate found");
                            configSetting = new ConfigurationSetting
                            {
                                Port = uri.Port,
                                Origin = new HashSet<string>() { "*" },
                                Uri = uri,
                                Certificate = certificate

                            };
                            Logger.Current.Informational("Configured was mode for websockets.");
                        }
                        else
                        {
                            Logger.Current.Informational("certificate not found");
                            foreach (X509Certificate2 objCert in store.Certificates)
                            {
                                string serialNumber = objCert.SerialNumber.Trim().ToString().ToUpper();
                                Logger.Current.Verbose("Certificate name" + objCert.FriendlyName + " Store serial number:" + objCert.SerialNumber.Trim());
                                string orgSerialNumber = sslKey.Trim().ToString().ToUpper();
                                if (String.Equals(serialNumber, orgSerialNumber, StringComparison.InvariantCulture))
                                {
                                    certificate = objCert;
                                }
                            }
                            if (certificate != null)
                            {
                                Logger.Current.Informational("Certificate found.");
                                configSetting = new ConfigurationSetting
                                {
                                    Port = uri.Port,
                                    Origin = new HashSet<string>() { "*" },
                                    Uri = uri,
                                    Certificate = certificate

                                };
                                Logger.Current.Informational("Configured wss mode for websockets.");
                            }
                            else
                                Logger.Current.Informational("Certificate not found. Could not set wss for the application.");
                        }
                    }
                    else
                    {
                        uri = new Uri("ws://" + ConfigurationManager.AppSettings["MASTER_ACCOUNT_DNS"]
                                + ":" + ConfigurationManager.AppSettings["WEBSOCKET_PORT"]);
                        configSetting = new ConfigurationSetting(uri);
                        Logger.Current.Informational("Configured ws mode for websockets.");
                    }

                    configurationSettings.Add(configSetting);
                    socketServerContainer.StartServers(withInterceptors:true, configurationSettings: configurationSettings);
                }
            }
        }
        /// <summary>
        /// Gets the exact error message to the log.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            string reason = ex.Message.ToString();
            if (string.IsNullOrEmpty(reason))
                reason = "Requested entity was deleted. Please try another";
            else if (!(ex is UnsupportedOperationException))
                reason = "An error has occurred, please try again later.";
            var exception = new Exception(reason, ex);
            ExceptionHandler.Current.HandleException(exception, DefaultExceptionPolicies.LOG_ONLY_POLICY);
        }

        protected void Application_BeginRequest(object source, EventArgs e)
        {
            i18n.LanguageTag lt = i18n.LanguageTag.GetCachedInstance("en");
            if (lt.IsValid())
            {
                Response.Cookies.Add(new HttpCookie("i18n.langtag")
                {
                    Value = lt.ToString(),
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddYears(1)
                });
            }
            else
            {
                var cookie = Response.Cookies["i18n.langtag"];
                if (cookie != null)
                {
                    cookie.Value = null;
                    cookie.Expires = DateTime.UtcNow.AddMonths(-1);
                }
            }
            if (HttpContext.Current.Request.RawUrl.ToLower().Contains("content") || HttpContext.Current.Request.RawUrl.ToLower().Contains("style"))
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            HttpContext.Current.SetPrincipalAppLanguageForRequest(lt);
        }
    }
}
