using System;
using System.Configuration;
using System.Threading;
using LandmarkIT.Enterprise.CommunicationManager.Database;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.WindowsServiceRunner;
using SmartTouch.CRM.JobProcessor.Jobs.WebAnalytics;

namespace SmartTouch.CRM.WebAnalyticsEngine
{
    [System.ComponentModel.DesignerCategory("Code")]
    public class Host : QuartzService
    {
        public const string NameOfService = "SmartCRM Web Analytics Engine";

        internal static void Main()
        {
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
            Run(new Host());
#endif
        }

        public Host()
        {
            var currentThread = Thread.CurrentThread;
            currentThread.Name = NameOfService;
            ServiceName = NameOfService;
        }

        protected override void OnBeforeStart()
        {
            RegisterJob<WebAnalyticsKickfireJob>(CronJobType.WebAnalyticsVisitProcessor);
            //RegisterJob<WebVisitDailySummaryJob>(CronJobType.WebAnalyticsDailyEmailProcessor);
            //RegisterJob<WebVisitEmailNotifierJob>(CronJobType.WebAnalyticsInstantAlertProcessor);
        }
    }
}