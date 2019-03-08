using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using System;
using System.Configuration;
using System.Threading;
using SmartTouch.CRM.JobProcessor;
using System.Collections;
using System.Collections.Generic;
using System.ServiceProcess;

namespace SmartTouch.CRM.CampaignProcessor
{
    [System.ComponentModel.DesignerCategory("Code")]
    internal sealed partial class Host : SmartTouchServiceBase
    {
        public const string NameOfService = "Smart CRM - Campaign Processor";
        private static object _syncLock = new Object();
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

            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Initializers();

            IEnumerable<CronJobType> jobTypes = new List<CronJobType>()
            {
                CronJobType.CampaignProcessor,
                CronJobType.LitmusTestProcessor,
                CronJobType.MailTesterProcessor,
                CronJobType.AutomationCampaignProcessor,
                CronJobType.VMTAFTPLogProcessor,
                CronJobType.VMTALogReadProcessor
            };

            foreach(var jobType in jobTypes)
            {
                var job = new SmartTouchServiceBase()
                {
                    JobType = jobType
                };
                job.Start(args);
            }
        }
    }
}
