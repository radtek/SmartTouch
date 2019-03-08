using System;
using System.Configuration;
using System.Threading;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.WindowsServiceRunner;
using SmartTouch.CRM.JobProcessor.Jobs.Schedulers;
using LandmarkIT.Enterprise.CommunicationManager.Database;
using SmartTouch.CRM.JobProcessor.Jobs.Notify;

namespace SmartTouch.CRM.Scheduler
{
    [System.ComponentModel.DesignerCategory("Code")]
    internal sealed partial class Host : QuartzService
    {
        public const string NameOfService = "SmartTouch CRM Scheduler Service";

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
            Run(new Host());
#endif
        }

        public Host()
        {
            var currentThread = Thread.CurrentThread;
            currentThread.Name = NameOfService;
            ServiceName = NameOfService;

            InitializeComponent();
        }

        protected override void OnBeforeStart()
        {
            RegisterJob<NotifyByEmailJob>(CronJobType.LandmarkITMailProcessor);
            RegisterJob<NotifyByTextJob>(CronJobType.LandmarkITTextProcessor);
            RegisterJob<DailySummaryEmailJob>(CronJobType.DailySummaryEmailProcessor, ConfigurationManager.AppSettings["DAILY_SUMMARY_MAIL_JOB_CRON_EXPRESSION"]);
            RegisterJob<NightlyScheduledDeliverabilityReportJob>(CronJobType.NightlyScheduledDeliverabilityReportProcessor, ConfigurationManager.AppSettings["NIGHTLY_REPORT_JOB_CRON_EXPRESSION"]);
        }
    }
}
