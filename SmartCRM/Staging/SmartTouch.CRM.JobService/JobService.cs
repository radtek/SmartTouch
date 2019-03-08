using System;
using System.Collections.Generic;
using System.Configuration;
using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.JobProcessor.Jobs;
using SmartTouch.CRM.JobProcessor.Jobs.CampaignJobs;
using SmartTouch.CRM.JobProcessor.Jobs.FormSubmission;
using SmartTouch.CRM.JobProcessor.Jobs.Leads;
using SmartTouch.CRM.JobProcessor.Jobs.NewBounce;
using SmartTouch.CRM.JobProcessor.Jobs.Schedulers;
using SmartTouch.CRM.JobProcessor.Jobs.VMTALog;
using SmartTouch.CRM.JobProcessor.Jobs.WebAnalytics;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.WindowsServiceRunner;

namespace SmartTouch.CRM.JobServices
{
    [System.ComponentModel.DesignerCategory("Code")]
    internal sealed class JobService : QuartzService
    {
        private readonly Logger _logger = Logger.Current;

        private static readonly Dictionary<string, Dictionary<CronJobType, Type>> JobGroups = new Dictionary<string, Dictionary<CronJobType, Type>>
        {
            { "Action", new Dictionary<CronJobType, Type> { { CronJobType.ActionProcessor, typeof(WorkflowActionJob) } } },
            { "BulkOperation", new Dictionary<CronJobType, Type> { { CronJobType.BulkOperationProcessor, typeof(BulkOperationJob) } } },
            { "Campaign", new Dictionary<CronJobType, Type>
            {
                { CronJobType.CampaignProcessor, typeof(CampaignJob) },
                { CronJobType.AutomationCampaignProcessor, typeof(AutomationCampaignJob) },
                { CronJobType.LitmusTestProcessor, typeof(LitmusTestJob) },
                { CronJobType.VMTAFTPLogProcessor, typeof(FtpJob) },
                { CronJobType.VMTALogReadProcessor, typeof(FileReadJob) },
                { CronJobType.MailTesterProcessor, typeof(CampaignMailTesterJob) }
            } },
            { "FormSubmission", new Dictionary<CronJobType, Type> {
                { CronJobType.FormSubmissionProcessor, typeof(FormSubmissionJob) },
                { CronJobType.APILeadSubmissionProcessor, typeof(ApiLeadSubmissionJob) } 
            } },
            { "ImportLead", new Dictionary<CronJobType, Type>
            {
                { CronJobType.ImportLeadProcessor, typeof(ImportLeadJob) },
                { CronJobType.NeverBounceFileProcessor, typeof(NeverBounceFileJob) },
                { CronJobType.NeverBouncePollingProcessor, typeof(NeverBouncePollingJob) },
                { CronJobType.NeverBounceResultsProcessor, typeof(NeverBounceResultsJob) }
            } },
            { "Index", new Dictionary<CronJobType, Type> { { CronJobType.IndexProcessor, typeof(IndexJob) } } },
            { "Lead", new Dictionary<CronJobType, Type>
            {
                { CronJobType.LeadProcessor, typeof(LeadJob) },
                { CronJobType.LeadScoreProcessor, typeof(LeadScoreJob) }
            } },
            { "Scheduler", new Dictionary<CronJobType, Type>
            {
                { CronJobType.DailySummaryEmailProcessor, typeof(DailySummaryEmailJob) },
                { CronJobType.NightlyScheduledDeliverabilityReportProcessor, typeof(NightlyScheduledDeliverabilityReportJob) }
            } },
            { "SmartSearch", new Dictionary<CronJobType, Type> { { CronJobType.SmartSearchProcessor, typeof(SmartSearchJob) } } },
            { "WebAnalytics", new Dictionary<CronJobType, Type>
            {
                { CronJobType.WebAnalyticsVisitProcessor, typeof(WebAnalyticsKickfireJob) },
                { CronJobType.WebAnalyticsInstantAlertProcessor, typeof(WebVisitDailySummaryJob) },
                { CronJobType.WebAnalyticsDailyEmailProcessor, typeof(WebVisitDailySummaryJob) }
            } }
        };

        /// <summary>
        /// Application entry point.
        /// </summary>
        internal static int Main(string[] args)
        {
            //assign instrumentation key to appinsights
            Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration.Active.InstrumentationKey = ConfigurationManager.AppSettings["iKey"];

            return QuartzServiceRunner.Run(args, new JobServiceInstaller(), new JobService());
        }

        protected override void OnBeforeStart()
        {
            foreach (var group in JobGroups)
            {
                foreach (var job in group.Value)
                {
                    try
                    {
                        RegisterJob(job.Key, job.Value, group.Key);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(string.Format("Eror while registing job {0} {1}", job.Key, ex));
                    }
                }
            }
        }
    }
}
