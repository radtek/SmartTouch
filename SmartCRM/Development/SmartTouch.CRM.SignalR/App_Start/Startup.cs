using LandmarkIT.Enterprise.Utilities.Logging;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Web;
using System.Diagnostics;

[assembly: OwinStartupAttribute(typeof(SmartTouch.CRM.SignalR.App_Start.Startup))]
namespace SmartTouch.CRM.SignalR.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //assign instrumentation key to appinsights
            Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration.Active.InstrumentationKey = ConfigurationManager.AppSettings["iKey"];
            var hubsConfiguration = new HubConfiguration();
            hubsConfiguration.EnableDetailedErrors = true;
            app.Map("/signalr", map =>
                {
                    map.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
                    map.RunSignalR(hubsConfiguration);
                });
            Container container = new Container();
            IoC.Configure(container);
            Logger.Current.CreateRollingFlatFileListener(System.Diagnostics.Tracing.EventLevel.Verbose, ConfigurationManager.AppSettings["SIGNALR_LOG_FILE_PATH"], 2048);
            //if (masterAccountDns.Equals("localhost"))
            //{
            //    using (var socketServerContainer = Composable.GetExport<IXSocketServerContainer>())
            //    {
            //        socketServerContainer.Start();
            //    }
            //}
        }
    }
}