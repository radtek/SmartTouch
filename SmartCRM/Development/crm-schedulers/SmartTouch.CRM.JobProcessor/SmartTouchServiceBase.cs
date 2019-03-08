using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.Caching;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LandmarkIT.Enterprise.Extensions;

namespace SmartTouch.CRM.JobProcessor
{
    public class SmartTouchServiceBase : ServiceBase
    {
        public SmartTouchServiceBase()
        {
            LogFilePath = "SMART_CRM_JOB_PROCESSOR_LOG_FILE_PATH";
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(this))
            {
                DefaultValueAttribute myAttribute = (DefaultValueAttribute)property.Attributes[typeof(DefaultValueAttribute)];

                if (myAttribute != null)
                {
                    property.SetValue(this, myAttribute.Value);
                }
            }
        }

        private static object _syncLock = new Object();

        private Timer _jobTimer = default(Timer);
        private Timer _supportTimer = default(Timer);

        [DefaultValue(10000)]
        public int JobTimerDueTime { get; set; }
        [DefaultValue(30000)]
        public int JobTimerPeriod { get; set; }
        [DefaultValue(120000)]
        public int SupportTimerDueTime { get; set; }
        [DefaultValue(120000)]
        public int SupportTimerPeriod { get; set; }

        public CronJobType JobType { get; set; }
        public CronJobProcessor Processor { get; set; }


        private string _cacheName = string.Empty;
        public string CacheName
        {
            get
            {
                if (_cacheName == string.Empty)
                    _cacheName = JobType.GetDisplayName() + "Cache" + Instance;
                return _cacheName;
            }
            set
            {
                _cacheName = value;
            }
        }
        public static string LogFilePath { get; set; }

        [DefaultValue(1)]
        public int Instance { get; set; }

        private static MemoryCache cache = MemoryCache.Default;
        private readonly JobService _JobService = new JobService();

        public CronJobDb CurrentCronJob
        {
            get
            {
                lock (_syncLock)
                {
                    var _cronJob = cache.Get(CacheName) as CronJobDb;

                    if (_cronJob == null)
                    {
                        //TODO: make this as dynamic
                        _cronJob = _JobService.GetAllCronJobs().Where(cj => cj.CronJobID == JobType).Single();
                        _cronJob.IsRunning = false;
                        cache.Add(CacheName, _cronJob, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.MaxValue });
                    }
                    return _cronJob;
                }

            }
        }

        public void InitializeContainer()
        {
            _jobTimer = new Timer(new TimerCallback(RunJobs), null, JobTimerDueTime, JobTimerPeriod);
            _supportTimer = new Timer(new TimerCallback(InitializeTimers), null, SupportTimerDueTime, SupportTimerPeriod);
        }

        public void InitializeTimers(object obj)
        {
            _jobTimer = new Timer(new TimerCallback(RunJobs), null, JobTimerDueTime, JobTimerPeriod);
        }

        public void RunJobs(object obj)
        {
            lock (_syncLock)
            {
                _supportTimer = new Timer(new TimerCallback(InitializeTimers), null, SupportTimerDueTime, SupportTimerPeriod);

                if (!CurrentCronJob.IsRunning && DateTime.UtcNow >= ParseCron(CurrentCronJob.Expression, CurrentCronJob.LastRunOn) && CurrentCronJob.IsActive)
                {
                    //TODO: change this
                    var jobProcessor = GetJobProcessor(CurrentCronJob);

                    CurrentCronJob.LastRunOn = DateTime.UtcNow;
                    CurrentCronJob.LastNotifyDateTime = CurrentCronJob.LastRunOn;
                    CurrentCronJob.IsRunning = true;

                    jobProcessor.Start();
                }
            }
        }

        private CronJobProcessor GetJobProcessor(CronJobDb cronJob)
        {
            /*
             * We need to create instance for all job processors based on their type
             **/
            CronJobProcessor processor = null;
            switch ((CronJobType)cronJob.CronJobID)
            {
                case CronJobType.LeadProcessor:
                    processor = new LeadProcessor(cronJob, _JobService, CacheName);
                    break;
                case CronJobType.ImportLeadProcessor:
                    processor = new ImportLeadProcessor(cronJob, _JobService, CacheName);
                    break;
                case CronJobType.IndexProcessor:
                    processor = new IndexProcessor(cronJob, _JobService, CacheName);
                    break;
                case CronJobType.LeadScoreProcessor:
                    processor = new LeadScoreProcessor(cronJob, _JobService, CacheName);
                    break;
                case CronJobType.FormSubmissionProcessor:
                    processor = new FormSubmissionProcessor(cronJob, _JobService, CacheName);
                    break;
                case CronJobType.CampaignProcessor:
                    processor = new CampaignProcessor(cronJob, _JobService, CacheName);
                    break;
                case CronJobType.BulkOperationProcessor:
                    processor = new BulkOperationProcessor(cronJob, _JobService, CacheName);
                    break;
                case CronJobType.ActionProcessor:
                    processor = new ActionProcessor(cronJob, _JobService, CacheName);
                    break;
                case CronJobType.APILeadSubmissionProcessor:
                    processor = new APILeadSubmissionProcessor(cronJob, _JobService, CacheName);
                    break;
                case CronJobType.SmartSearchProcessor:
                    processor = new SmartSearchProcessor(cronJob, _JobService, CacheName);
                    break;
                case CronJobType.LitmusTestProcessor:
                    processor = new LitmusTestProcessor(cronJob, _JobService, CacheName);
                    break;
                case CronJobType.MailTesterProcessor:
                    processor = new CampaignMailTesterProcessor(cronJob, _JobService, CacheName);
                    break;
                case CronJobType.NeverBounceFileProcessor:
                    processor = new NeverBounceFileProcessor(cronJob, _JobService, CacheName);
                    break;
                case CronJobType.NeverBouncePollingProcessor:
                    processor = new NeverBouncePollingProcessor(cronJob, _JobService, CacheName);
                    break;
                case CronJobType.NeverBounceResultsProcessor:
                    processor = new NeverBounceResultsProcessor(cronJob, _JobService, CacheName);
                    break;
                case CronJobType.AutomationCampaignProcessor:
                    processor = new AutomationCampaignProcessor(cronJob, _JobService, CacheName);
                    break;
                case CronJobType.VMTAFTPLogProcessor:
                    processor = new VMTALog.FTPProcessor(cronJob, _JobService, CacheName);
                    break;
                case CronJobType.VMTALogReadProcessor:
                    processor = new VMTALog.FileReadProcessor(cronJob, _JobService, CacheName);
                    break;
                case CronJobType.NotificationProcessor:
                    processor = new NotificationProcessor(cronJob, _JobService, CacheName);
                    break;
                case CronJobType.BulkOperationReadyProcessor:
                    processor = new BulkOperationReadyProcessor(cronJob, _JobService, CacheName);
                    break;
                case CronJobType.LandmarkITMailProcessor:
                case CronJobType.LandmarkITTextProcessor:
                case CronJobType.WebAnalyticsVisitProcessor:
                case CronJobType.WebAnalyticsDailyEmailProcessor:
                case CronJobType.WebAnalyticsInstantAlertProcessor:
                default:
                    processor = Processor;
                    break;
            }
            return processor;
        }

        /// <summary>
        /// Parses given expression & returns next run time of the job
        /// Prepare Corn Expressions from http://www.cronmaker.com/
        /// </summary>
        /// <param name="cronExpression">corn expression value of job schedule</param>
        /// <param name="lastRunTime">job last run time</param>
        /// <returns>next schedule time of the job</returns>
        private DateTime ParseCron(string cronExpression, DateTime? lastRunTime = default(DateTime?))
        {
            var nextTime = (new Quartz.CronExpression(cronExpression).GetNextValidTimeAfter(DateTime.SpecifyKind(lastRunTime ?? DateTime.Now.AddMinutes(-60).ToUniversalTime(), DateTimeKind.Utc)) ?? DateTime.UtcNow).DateTime;
            return nextTime;
        }

        /// <summary>
        /// On start method for services
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            InitializeContainer();
            Logger.Current.Informational(string.Format("{0} are starting...", ServiceName));
            base.OnStart(args);
        }
        /// <summary>
        /// For testing in console
        /// </summary>
        public void Start(string[] args)
        {
            this.OnStart(args);
        }
        /// <summary>
        /// This method shouldn't be called multiple times, when multiple instances are being used.
        /// </summary>
        public static void Initializers()
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
    }
}
