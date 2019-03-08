using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using System;
using System.Configuration;
using System.Diagnostics.Tracing;
using System.Runtime.Caching;
using System.ServiceProcess;
using System.Threading;
using System.Linq;
using SmartTouch.CRM.JobProcessor;
using SimpleInjector;

namespace LandmarkIT.Enterprise.Scheduler
{
    [System.ComponentModel.DesignerCategory("Code")]
    internal sealed partial class Host: SmartTouchServiceBase
    {
        public const string NameOfService = "LandmarkIT Scheduler Service";
        private readonly JobService _JobService = new JobService();

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
        }

        protected override void OnStart(string[] args)
        {
            //Initialize IoC, Automapper, Logs
            SmartTouchServiceBase.Initializers();
            var instances = Convert.ToInt16((ConfigurationManager.AppSettings["MAIL_QUEUE_JOB_INSTANCES"] == null || ConfigurationManager.AppSettings["MAIL_QUEUE_JOB_INSTANCES"].Length == 0) ? "4": ConfigurationManager.AppSettings["MAIL_QUEUE_JOB_INSTANCES"]);
            for (var i = 1; i < (instances+1); i++)
            {
                var mailJob = new SmartTouchServiceBase()
                {
                    JobType = CronJobType.LandmarkITMailProcessor,
                    Instance = i
                };
                mailJob.Processor = new MailProcessor(mailJob.CurrentCronJob, _JobService, mailJob.CacheName);
                mailJob.Start(args);
            }

            var textJob = new SmartTouchServiceBase()
            {
                JobType = CronJobType.LandmarkITTextProcessor
            };
            textJob.Processor = new TextProcessor(textJob.CurrentCronJob, _JobService, textJob.CacheName);
            textJob.Start(args);
        }
    }
}
