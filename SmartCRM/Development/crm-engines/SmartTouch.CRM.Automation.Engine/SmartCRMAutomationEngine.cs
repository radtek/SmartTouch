using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Tracing;
using System.Configuration;

using SimpleInjector;
using SmartTouch.CRM.ApplicationServices;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.Automation.Core;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.MessageQueues;
using SmartTouch.CRM.SearchEngine.Indexing;
using Quartz.Impl;
using Quartz;
using System;

namespace SmartTouch.CRM.Automation.Engine
{
    public partial class SmartCRMAutomationEngine : ServiceBase
    {
        public SmartCRMAutomationEngine()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Container container = new Container();
            IoC.Configure(container);
            InitializeAutoMapper.Initialize();

            #region Logging Configuration
            ExceptionHandler.Current.AddDefaultLogAndRethrowPolicy();
            ExceptionHandler.Current.AddDefaultLogOnlyPolicy();
            Logger.Current.CreateRollingFlatFileListener(EventLevel.Verbose, ConfigurationManager.AppSettings["AUTOMATION_ENGINE_LOG_FILE_PATH"], 2048);
            #endregion

            var cachingService = IoC.Container.GetInstance<ICachingService>();
            var indexingService = IoC.Container.GetInstance<IIndexingService>();
            var advancedSearchService = IoC.Container.GetInstance<IAdvancedSearchService>();
            var contactService = IoC.Container.GetInstance<IContactService>();
            var workflowService = IoC.Container.GetInstance<IWorkflowService>();
            var accountService = IoC.Container.GetInstance<IAccountService>();
            var tagService = IoC.Container.GetInstance<ITagService>();
            var campaignService = IoC.Container.GetInstance<ICampaignService>();
            var leadScoreService = IoC.Container.GetInstance<ILeadScoreService>();
            var pubSubService = IoC.Container.GetInstance<IPublishSubscribeService>();
            var opportunityServcie = IoC.Container.GetInstance<IOpportunitiesService>();
            var communicationService = IoC.Container.GetInstance<ICommunicationService>();

            #region Schedule Messaging Job
            //Quartz.Net Job Scheduler
            //use http://www.cronmaker.com/ to build cron expression
            Logger.Current.Informational("Scheduling Job : Message Scheduler");
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            scheduler.ScheduleJob(CreateJob<MessagingScheduler>(),
                CreateTrigger(ConfigurationManager.AppSettings["MESSAGESCHEDULE_CRON"].ToString(), "Message Scheduler"));
            #endregion

            AutomationEngine engine = new AutomationEngine(cachingService, indexingService, advancedSearchService,
                   contactService, workflowService, accountService, tagService, campaignService, leadScoreService, pubSubService, opportunityServcie, communicationService);
            engine.Start();
            Logger.Current.Informational("Engine started successfully.");
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
        protected override void OnStop()
        {
        }
    }
}
