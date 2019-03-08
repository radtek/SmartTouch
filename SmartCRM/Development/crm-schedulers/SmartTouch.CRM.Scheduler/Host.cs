using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SimpleInjector;
using System;
using System.Configuration;
using System.Diagnostics.Tracing;
using System.ServiceProcess;
using System.Threading;
using SmartTouch.CRM.ApplicationServices;
using Quartz;
using Quartz.Impl;

namespace SmartTouch.CRM.Scheduler
{
    [System.ComponentModel.DesignerCategory("Code")]
    internal sealed partial class Host : ServiceBase
    {
        public const string NameOfService = "SmartTouch CRM Scheduler Service";

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

            InitializeComponent();
        }

        private void InitializeContainer()
        {
            //Deffered it to later
            //FTPPathWatchTimer = new Timer(new TimerCallback(FTPFileProcessor.Trigger), null, 10000, 120000);

            //Quartz.Net Job Scheduler
            //use http://www.cronmaker.com/ to build cron expression
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();          

            scheduler.ScheduleJob(CreateJob<DailySummaryEmailProcessor>(),
                    CreateTrigger(ConfigurationManager.AppSettings["DAILY_SUMMARY_MAIL_JOB_CRON_EXPRESSION"].ToString(), "DailySummary Mail"));

            scheduler.ScheduleJob(CreateJob<NightlyScheduledDeliverabilityReportProcessor>(),
                    CreateTrigger(ConfigurationManager.AppSettings["NIGHTLY_REPORT_JOB_CRON_EXPRESSION"].ToString(), "Nightly Sheduled Deliverability Report Processor"));

            scheduler.ScheduleJob(CreateJob<FailedFormsNotificationProcessor>(),
                    CreateTrigger(ConfigurationManager.AppSettings["FAILED_FORM_JOB_CRON_EXPRESSION"].ToString(), "Failed Forms Report Processor"));
        }

        private IJobDetail CreateJob<T>() where T : IJob
        {
            return JobBuilder.Create<T>().Build();
        }

        private ITrigger CreateTrigger(string cronExpression, string jobName)
        {
            return TriggerBuilder.Create()
                            .WithIdentity(jobName)
                            .StartNow()
                            .WithCronSchedule(cronExpression)
                            .Build();
        }
        protected override void OnStart(string[] args)
        {
            try
            {
                Container container = new Container();
                IoC.Configure(container);
                InitializeAutoMapper.Initialize();
                #region Logging Configuration
                ExceptionHandler.Current.AddDefaultLogAndRethrowPolicy();
                ExceptionHandler.Current.AddDefaultLogOnlyPolicy();
                Logger.Current.CreateRollingFlatFileListener(EventLevel.Verbose, ConfigurationManager.AppSettings["WINDOWS_SERVICE_LOG_FILE_PATH"], 2048);
                #endregion

                InitializeContainer();
                Logger.Current.Informational(string.Format("{0} are starting...", NameOfService));
            }
            catch (Exception ex)
            {
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                throw;
            }
            finally
            {
                base.OnStart(args);
            }
        }

        public void InitializeTimers(object obj)
        {
            InitializeContainer();
        }

        protected override void OnStop()
        {
            try
            {
                Logger.Current.Informational(string.Format("{0} are shut down...", NameOfService));
            }
            finally
            {
                base.OnStop();
            }
        }
    }
}
