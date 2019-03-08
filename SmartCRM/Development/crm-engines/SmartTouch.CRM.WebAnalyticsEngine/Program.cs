using System;
using System.Configuration;
using System.Threading;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.CommunicationManager.Database;
using SmartTouch.CRM.JobProcessor;
using System.Collections.Generic;
using System.ServiceProcess;

namespace SmartTouch.CRM.WebAnalyticsEngine
{
    [System.ComponentModel.DesignerCategory("Code")]
    public class Program : SmartTouchServiceBase
    {
        public const string NameOfService = "SmartCRM Web Analytics Engine";
        private static object _syncLock = new Object();
        private readonly JobService _JobService = new JobService();

        internal static void Main()
        {
            Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration.Active.InstrumentationKey = ConfigurationManager.AppSettings["iKey"];
#if (DEBUG)
            //assign instrumentation key to appinsights

            Console.WriteLine("-----------------------------------------------------------------------");
            Console.WriteLine("{0} are starting...", NameOfService);
            Console.WriteLine("-----------------------------------------------------------------------");

            using (Program host = new Program())
            {
                host.OnStart(new string[] { });

                Console.WriteLine("-----------------------------------------------------------------------");
                Console.WriteLine("{0} are listening...Press <ENTER> to terminate.", NameOfService);
                Console.WriteLine("-----------------------------------------------------------------------");

                Console.ReadLine();

                host.OnStop();
            }
#else
            ServiceBase.Run(new Program());
#endif
        }

        public Program()
        {
            var currentThread = Thread.CurrentThread;
            currentThread.Name = NameOfService;
            ServiceName = NameOfService;
        }

        protected override void OnStart(string[] args)
        {
            Initializers();

            IEnumerable<CronJobType> jobTypes = new List<CronJobType>()
            {
                CronJobType.WebAnalyticsVisitProcessor,
                CronJobType.WebAnalyticsInstantAlertProcessor,
                CronJobType.WebAnalyticsDailyEmailProcessor
            };

            foreach (var jobType in jobTypes)
            {
                var job = new SmartTouchServiceBase()
                {
                    JobType = jobType,
                };
                if (jobType == CronJobType.WebAnalyticsVisitProcessor)
                    job.Processor = new WebAnalyticsProcessor(job.CurrentCronJob, _JobService, job.CacheName);
                else if (jobType == CronJobType.WebAnalyticsInstantAlertProcessor)
                    job.Processor = new WebVisitEmailNotifier(job.CurrentCronJob, _JobService, job.CacheName);
                else if (jobType == CronJobType.WebAnalyticsDailyEmailProcessor)
                    job.Processor = new WebVisitDailySummaryProcessor(job.CurrentCronJob, _JobService, job.CacheName);
                job.Start(args);
            }
        }
    }
}
