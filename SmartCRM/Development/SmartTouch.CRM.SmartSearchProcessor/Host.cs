using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SimpleInjector;
using SmartTouch.CRM.JobProcessor;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.Tracing;
using SmartTouch.CRM.ApplicationServices;
using System.ServiceProcess;

namespace SmartTouch.CRM.SmartSearchProcessor
{

    [System.ComponentModel.DesignerCategory("Code")]
    internal sealed partial class Host : SmartTouchServiceBase
    {
        public const string NameOfService = "Smart CRM - Smart Search Processor";
        private static object _syncLock = new Object();

        /// <summary>
        /// Application entry point.
        /// </summary>
        internal static void Main()
        {
            //assign instrumentation key to appinsights
            Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration.Active.InstrumentationKey = ConfigurationManager.AppSettings["iKey"];
#if (DEBUG)
            Console.WriteLine("-----------------------------------------------------------------------");
            Console.WriteLine("{0} are starting...", NameOfService);
            Console.WriteLine("-----------------------------------------------------------------------");

            using (Host host = new Host())
            {
                host.OnStart(new string[] { });

                Console.WriteLine("-----------------------------------------------------------------------");
                Console.WriteLine("{0} are listening...Press <ENTER> to terminate.", NameOfService);
                Console.WriteLine("-----------------------------------------------------------------------");

                Console.ReadLine();

                host.OnStop();
            }
#else
            ServiceBase.Run(new Host());
#endif
        }

        public Host()
        {
            var currentThread = Thread.CurrentThread;
            currentThread.Name = NameOfService;
            ServiceName = NameOfService;

            //InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            SmartTouchServiceBase.Initializers();
            JobType = CronJobType.SmartSearchProcessor;
            base.OnStart(args);
        }
    }
}
