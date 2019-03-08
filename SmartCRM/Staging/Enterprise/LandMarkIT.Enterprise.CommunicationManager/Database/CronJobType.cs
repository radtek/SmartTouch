namespace LandmarkIT.Enterprise.CommunicationManager.Database
{
    public enum CronJobType : byte
    {
        Undefined = 0,
        CampaignProcessor = 1,
        LeadScoreProcessor = 2,
        IndexProcessor = 3,
        ActionProcessor = 4,
        BulkOperationProcessor = 5,
        LeadProcessor = 6,
        ImportLeadProcessor = 7,
        FormSubmissionProcessor = 8,
        WebAnalyticsVisitProcessor = 9,
        WebAnalyticsInstantAlertProcessor = 10,
        WebAnalyticsDailyEmailProcessor = 11,
        LandmarkITMailProcessor = 12,
        LandmarkITTextProcessor = 13,
        APILeadSubmissionProcessor = 14,
        SmartSearchProcessor = 15,
        LitmusTestProcessor = 16,
        MailTesterProcessor = 17,
        NeverBounceFileProcessor = 18,
        NeverBouncePollingProcessor = 19,
        NeverBounceResultsProcessor = 20,
        AutomationCampaignProcessor = 21,
        VMTAFTPLogProcessor = 22,
        VMTALogReadProcessor = 23,
        BouncedMailDataProcessor = 24,
        DailySummaryEmailProcessor = 26,
        NightlyScheduledDeliverabilityReportProcessor = 27
    }
}
