using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Tracing;
using System.Linq;
using System.ServiceProcess;
using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using Quartz;
using Quartz.Impl;
using SmartTouch.CRM.ApplicationServices;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler;

namespace SmartTouch.CRM.JobProcessor.QuartzScheduler.WindowsServiceRunner
{
    public class QuartzService : ServiceBase
    {
        private readonly Logger _logger = Logger.Current;
        private const string LogFilePath = "SMART_CRM_JOB_PROCESSOR_LOG_FILE_PATH";

        private IScheduler _scheduler;
        private JobService _jobService;

        public void Start(string[] args)
        {
            OnStart(args);
        }

        protected override void OnStart(string[] args)
        {
            _logger.Informational($"Starting service {ServiceName}...");

            //Initialize IoC and etc
            Initializers();

            _jobService = new JobService();
            //Init Quartz
            ISchedulerFactory schedFact = new StdSchedulerFactory();
            _scheduler = schedFact.GetScheduler();
            _scheduler.JobFactory = new DependencyInjectionJobFactory();

            //Run child initialization
            OnBeforeStart();

            _scheduler.Start();

            _logger.Informational($"Service {ServiceName} started.");
        }

        protected override void OnStop()
        {
            _logger.Informational($"Ending {ServiceName} service...");

            //Shutdown in reverse order
            _scheduler.Shutdown(true);

            OnBeforeStop();

            _logger.Informational($"Service {ServiceName} stopped.");
        }

        public void RegisterJob(CronJobType jobType, Type job, string jobGroup)
        {
            var dbJob = _jobService.GetCronJobByType(jobType);
            if (dbJob == null)
                throw new InvalidOperationException($"Job with {jobType} not found in the database");

            var group = "group" + jobGroup;
            var jobName = "job" + jobType;

            var trigger = BuildTrigger(jobType, dbJob.Expression);
            var jobDetail = JobBuilder.Create(job)
                .WithIdentity(jobName, group)
                .UsingJobData(BaseJob.DbJobTypeKey, (int)jobType)
                .Build();

            _scheduler.ScheduleJob(jobDetail, trigger);
        }

        public void RegisterJob<T>(CronJobType jobType) where T : IJob
        {
            var dbJob = _jobService.GetCronJobByType(jobType);
            if (dbJob == null)
                throw new InvalidOperationException($"Job with {jobType} not found in the database");

            RegisterJob<T>(jobType, dbJob.Expression);
        }

        public void RegisterJob<T>(CronJobType jobType, string cronExpressinon) where T : IJob
        {
            var group = "group" + jobType;
            var jobName = "job" + jobType;

            var trigger = BuildTrigger(jobType, cronExpressinon);
            var jobDetail = JobBuilder.Create<T>()
                .WithIdentity(jobName, group)
                .UsingJobData(BaseJob.DbJobTypeKey, (int)jobType)
                .Build();

            _scheduler.ScheduleJob(jobDetail, trigger);
        }

        private ITrigger BuildTrigger(CronJobType jobType, string cronExpressinon)
        {
            if (String.IsNullOrEmpty(cronExpressinon))
                throw new ArgumentException("cronExpressinon is not defined");

            var trigger = TriggerBuilder.Create()
                .WithIdentity("trigger", jobType.ToString())
                .WithCronSchedule(cronExpressinon)
                .Build();

            return trigger;
        }

        /// <summary>
        /// This method shouldn't be called multiple times, when multiple instances are being used.
        /// </summary>
        private void Initializers()
        {
            InitializeIoc();
            InitializeAutomapper();
            InitializeLogs();
        }
        /// <summary>
        /// Initialize IoC
        /// </summary>
        private static void InitializeIoc()
        {
            IoC.Configure(new SimpleInjector.Container());
        }
        /// <summary>
        /// Initialize Automapper
        /// </summary>
        private static void InitializeAutomapper()
        {
            InitializeAutoMapper.Initialize();
        }
        /// <summary>
        /// Initialize logging file
        /// </summary>
        private static void InitializeLogs()
        {
            #region Logging Configuration
            ExceptionHandler.Current.AddDefaultLogAndRethrowPolicy();
            ExceptionHandler.Current.AddDefaultLogOnlyPolicy();
            Logger.Current.CreateRollingFlatFileListener(EventLevel.Verbose, ConfigurationManager.AppSettings[LogFilePath], 2048);
            #endregion
        }

        protected virtual void OnBeforeStart() { }
        protected virtual void OnBeforeStop() { }
    }
}
