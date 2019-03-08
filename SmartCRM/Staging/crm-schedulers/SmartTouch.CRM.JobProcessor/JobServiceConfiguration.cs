using System;
using System.Configuration;

namespace SmartTouch.CRM.JobProcessor
{
    public class JobServiceConfiguration
    {
        public string ImageHostingServiceUrl { get; set; }
        public int RetryMaxHours { get; set; }
        public string MailTesterUrl { get; set; }
        public string NeverBounceApiUrl { get; set; }
        public string NeverBounceCsvFiles { get; set; }
        public string NeverBounceCsvurl { get; set; }
        public string NeverBounceResults { get; set; }
        public string NeverBounceUpdateEmail { get; set; }
        public string EmailTemplatesPhysicalPath { get; set; }
        public string NightlyReportPhysicalPath { get; set; }
        public string NightlyReportedEmails { get; set; }
        public string SupportEmailId { get; set; }
        public string MailDeliveryProcessPath { get; set; }
        public string MailFeedbackLoopDeliverProcessPath { get; set; }
        public string MailDeliveryProcessArchivePath { get; set; }
        public string MailFeedbackLoopDeliveryProcessArchivePath { get; set; }
        public string DiagActiveDirectory { get; set; }
        public string FblActiveDirectory { get; set; }
        public string FtpArchivePath { get; set; }
        public string SmarttouchPartnerKey { get; set; }
        public short SplitVisitInterval { get; set; }
        public bool IncludeCurrentDayInDailySummary { get; set; }
        public int IndexChunkSize { get; set; }
        public string LimitusBaseUrl { get; set; }
        public string Ikey { get; set; }
        public int RescheduleMinutes { get; set; }
        public int WorkflowActionChunkSize { get; set; }

        public static JobServiceConfiguration FromAppConfig()
        {
            try
            {
                var configuration = new JobServiceConfiguration
                {
                    //add variables from appconfig
                    ImageHostingServiceUrl = ConfigurationManager.AppSettings["IMAGE_HOSTING_SERVICE_URL"] ?? "",
                    RetryMaxHours = int.Parse((ConfigurationManager.AppSettings["RetryMaxHours"]) ?? "0"),
                    MailTesterUrl = ConfigurationManager.AppSettings["MailTesterURL"] ?? "",
                    NeverBounceApiUrl = ConfigurationManager.AppSettings["NeverBounce_API_URL"] ?? "",
                    NeverBounceCsvFiles = ConfigurationManager.AppSettings["NeverBounce_CSV_Files"] ?? "",
                    NeverBounceCsvurl = ConfigurationManager.AppSettings["NeverBounce_CSV_URL"] ?? "",
                    NeverBounceResults = ConfigurationManager.AppSettings["NeverBounce_Results"] ?? "",
                    NeverBounceUpdateEmail = ConfigurationManager.AppSettings["NeverBounce_Update_Email"] ?? "",
                    EmailTemplatesPhysicalPath = ConfigurationManager.AppSettings["EMAILTEMPLATES_PHYSICAL_PATH"] ?? "",
                    NightlyReportPhysicalPath = ConfigurationManager.AppSettings["NIGHTLYREPORT_PHYSICAL_PATH"] ?? "",
                    NightlyReportedEmails = ConfigurationManager.AppSettings["NIGHTLY_REPORTED_EMAILS"] ?? "",
                    SupportEmailId = ConfigurationManager.AppSettings["SUPPORT_EMAILID"] ?? "",
                    MailDeliveryProcessPath = ConfigurationManager.AppSettings["MAIL_DELIVERY_PROCESS_PATH"] ?? "",
                    MailFeedbackLoopDeliverProcessPath = ConfigurationManager.AppSettings["MAIL_FEEDBACKLOOP_DELIVER_PROCESS_PATH"] ?? "",
                    MailDeliveryProcessArchivePath = ConfigurationManager.AppSettings["MAIL_DELIVERY_PROCESS_ARCHIVE_PATH"] ?? "",
                    MailFeedbackLoopDeliveryProcessArchivePath = ConfigurationManager.AppSettings["MAIL_FEEDBACKLOOP_DELIVER_PROCESS_ARCHIVE_PATH"] ?? "",
                    DiagActiveDirectory = ConfigurationManager.AppSettings["DIAG_ACTIVE_DIRECTORY"] ?? "",
                    FblActiveDirectory = ConfigurationManager.AppSettings["FBL_ACTIVE_DIRECTORY"] ?? "",
                    FtpArchivePath = ConfigurationManager.AppSettings["FTP_ARCHIVE_PATH"] ?? "",
                    SmarttouchPartnerKey = ConfigurationManager.AppSettings["SMARTTOUCH_PARTNER_KEY"] ?? "0",
                    IndexChunkSize = int.Parse(ConfigurationManager.AppSettings["INDEX_CHUNK_SIZE"] ?? "0"),
                    SplitVisitInterval = short.Parse(ConfigurationManager.AppSettings["SPLIT_VISIT_INTERVAL"] ?? "0"),
                    IncludeCurrentDayInDailySummary = bool.Parse(ConfigurationManager.AppSettings["INCLUDE_CURRENT_DAY_IN_DAILY_SUMMARY"] ?? "false"),
                    LimitusBaseUrl = ConfigurationManager.AppSettings["LITMUS_BASE_URL"] ?? "",
                    Ikey = ConfigurationManager.AppSettings["iKey"],
                    RescheduleMinutes = int.Parse(ConfigurationManager.AppSettings["RescheduleMinutes"] ?? "0"),
                    WorkflowActionChunkSize = int.Parse(ConfigurationManager.AppSettings["WorkflowActionChunkSize"] ?? "10"),
                };
                return configuration;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Invalid settings in app.config", ex);
            }
        }
    }
}
