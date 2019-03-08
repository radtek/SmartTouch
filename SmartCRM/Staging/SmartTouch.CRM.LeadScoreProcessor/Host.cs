﻿using System;
using System.Configuration;
using System.Threading;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.WindowsServiceRunner;
using LandmarkIT.Enterprise.CommunicationManager.Database;
using SmartTouch.CRM.JobProcessor.Jobs;

namespace SmartTouch.CRM.LeadScoreProcessor
{
    [System.ComponentModel.DesignerCategory("Code")]
    internal sealed partial class Host : QuartzService
    {
        public const string NameOfService = "Smart CRM - Lead Score Processor";

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
            RegisterJob<LeadScoreJob>(CronJobType.LeadScoreProcessor);
        }

    }
}
