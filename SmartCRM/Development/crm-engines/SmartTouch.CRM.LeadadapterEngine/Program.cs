using SmartTouch.CRM.LeadAdapterEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SimpleInjector;
using SmartTouch.CRM.ApplicationServices;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using System.Configuration;
using Quartz;
using Quartz.Impl;

namespace SmartTouch.CRM.LeadAdapterEngine
{
    public class Program : ServiceBase
    {
        public const string NameOfService = "SmartTouch LeadAdapter Engine";
        private static object _syncLock = new Object();

        private Timer leadAdaptersProcessor = default(Timer);
        LeadProcessor leadProcessor = new LeadProcessor();

        private void InitializeContainer()
        {
            var time = ConfigurationManager.AppSettings["LeadAdapterEngineConfigurationTime"];
            int leadadapterconfigurationtime = int.Parse(time);
            leadAdaptersProcessor = new Timer(new TimerCallback(leadProcessor.Trigger), null, 10000, leadadapterconfigurationtime);

            //IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            //scheduler.Start();

            //scheduler.ScheduleJob(CreateJob<AdaptedContactsUpdate>(),
            //        CreateTrigger(ConfigurationManager.AppSettings["CONTACT_ELASTIC_UPDATE_EXPRESSION"].ToString(), "Contacts Indexing"));
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

        public Program()
        {
            var currentThread = Thread.CurrentThread;
            currentThread.Name = NameOfService;
            ServiceName = NameOfService;
            InitializeContainer();
        }

        internal static void Main()
        {
#if (DEBUG)
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
                Logger.Current.CreateRollingFlatFileListener(System.Diagnostics.Tracing.EventLevel.Verbose, ConfigurationManager.AppSettings["WINDOWS_SERVICE_LOG_FILE_PATH"], 2048);
                #endregion

                InitializeContainer();
                Logger.Current.Informational(string.Format("{0} are starting...", NameOfService));
            }
            catch (Exception ex)
            {
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
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

        private void InitializeComponent()
        {
            // 
            // Program
            // 
            this.ServiceName = "SmartTouch LeadAdapter Engine";

        }


        ///// <summary>
        ///// The main entry point for the application.
        ///// </summary>
        //static void Main()
        //{
        //    ServiceBase[] ServicesToRun;
        //    ServicesToRun = new ServiceBase[] 
        //    { 
        //        new Service1() 
        //    };
        //    ServiceBase.Run(ServicesToRun);
        //}
    }
}
