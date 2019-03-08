using Quartz;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler;
using System;
using LandmarkIT.Enterprise.Utilities.Excel;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.Domain.Contacts;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Reflection;
using SmartTouch.CRM.JobProcessor.Utilities;

namespace SmartTouch.CRM.JobProcessor.Jobs.Schedulers
{
    public class NightlyScheduledDeliverabilityReportJob : BaseJob
    {
        private readonly IContactRepository _contactRepository;
        private readonly JobServiceConfiguration _jobConfig;

        public NightlyScheduledDeliverabilityReportJob (
            IContactRepository contactRepository, 
            JobServiceConfiguration jobConfig )
        {
            _contactRepository = contactRepository;
            _jobConfig = jobConfig;
        }

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            try
            {
                Logger.Current.Informational("Nightly Scheduled Deliverability Report Trigger Started.");
                NightlyScheduledDeliverabilityReport nightlyReport = _contactRepository.GetSenderRecipientInfoNightlyReport();
                IEnumerable<CampaignSenderRecipientNightlyReport> campaignReport = _contactRepository.GetCampaignSenderRecipientInfoNightlyReport();
                DataTable dayReportData = GetDataTable<SenderRecipientInfoNightlyReport>(nightlyReport.DayReport);
                DataTable weekDayReportData = GetDataTable<SenderRecipientInfoNightlyReport>(nightlyReport.SevenDaysReport);
                DataTable campaignDayReportData = GetDataTable<CampaignSenderRecipientNightlyReport>(campaignReport.Where(d => d.Day == 1).Select(s => s).ToList());
                DataTable campaignWeekDayReportData = GetDataTable<CampaignSenderRecipientNightlyReport>(campaignReport.Where(d => d.Day == 2).Select(s => s).ToList());
                ReadExcel exl = new ReadExcel();
                byte[] daydata = exl.ConvertDataSetToExcel(dayReportData, string.Empty);
                byte[] weekdata = exl.ConvertDataSetToExcel(weekDayReportData, string.Empty);
                byte[] campainDaydata = exl.ConvertDataSetToExcel(campaignDayReportData, string.Empty);
                byte[] campaignWeekdata = exl.ConvertDataSetToExcel(campaignWeekDayReportData, string.Empty);
                Guid dayGuid = Guid.NewGuid();
                Guid weekGuid = Guid.NewGuid();
                Guid campaignDayGuid = Guid.NewGuid();
                Guid campaignWeekGuid = Guid.NewGuid();
                string attachmentPath = _jobConfig.NightlyReportPhysicalPath;
                string toEmail = _jobConfig.NightlyReportedEmails;
                string fromEmail = _jobConfig.SupportEmailId;
                List<string> toEmails = toEmail.Split(',').Select(s => s).ToList();
                File.WriteAllBytes(Path.Combine(attachmentPath, dayGuid + ".xlsx"), daydata);
                File.WriteAllBytes(Path.Combine(attachmentPath, weekGuid + ".xlsx"), weekdata);
                File.WriteAllBytes(Path.Combine(attachmentPath, campaignDayGuid + ".xlsx"), campainDaydata);
                File.WriteAllBytes(Path.Combine(attachmentPath, campaignWeekGuid + ".xlsx"), campaignWeekdata);
                // Guid loginToken = new Guid();
                string attachmentName = string.Empty;
                var dayReportTable = LandmarkIT.Enterprise.Extensions.EnumerableExtentions.GetTable<SenderRecipientInfoNightlyReport>(nightlyReport.DayReport,
                    p => p.AccountName,
                    p => p.SenderReputationCount,
                    p => p.CampaignsSent,
                    p => p.Recipients,
                    p => p.Sent,
                    p => p.Delivered,
                    p => p.Bounced,
                    p => p.Opened,
                    p => p.Clicked,
                    p => p.TagsAll,
                    p => p.TagsActive,
                    p => p.SSAll,
                    p => p.SSActive);

                var weekReportTable = LandmarkIT.Enterprise.Extensions.EnumerableExtentions.GetTable<SenderRecipientInfoNightlyReport>(nightlyReport.SevenDaysReport,
                    p => p.AccountName,
                    p => p.SenderReputationCount,
                    p => p.CampaignsSent,
                    p => p.Recipients,
                    p => p.Sent,
                    p => p.Delivered,
                    p => p.Bounced,
                    p => p.Opened,
                    p => p.Clicked,
                    p => p.TagsAll,
                    p => p.TagsActive,
                    p => p.SSAll,
                    p => p.SSActive);

                var campaignDayReportTable = LandmarkIT.Enterprise.Extensions.EnumerableExtentions.GetTable<CampaignSenderRecipientNightlyReport>(campaignReport.Where(d => d.Day == 1).Select(s => s).ToList(),
                   p => p.AccountName,
                   p => p.CampaignId,
                   p => p.CampaignSubject,
                   p => p.Vmta,
                   p => p.Recipients,
                   p => p.Sent,
                   p => p.SentDate,
                   p => p.Delivered,
                   p => p.Bounced,
                   p => p.Opened,
                   p => p.Clicked,
                   p => p.Complained,
                   p => p.TagsAll,
                   p => p.TagsActive,
                   p => p.SavedSearchAll,
                   p => p.SavedSearchActive);

                var campaignWeekReportTable = LandmarkIT.Enterprise.Extensions.EnumerableExtentions.GetTable<CampaignSenderRecipientNightlyReport>(campaignReport.Where(d => d.Day == 2).Select(s => s).ToList(),
                   p => p.AccountName,
                   p => p.CampaignId,
                   p => p.CampaignSubject,
                   p => p.Vmta,
                   p => p.Recipients,
                   p => p.Sent,
                   p => p.SentDate,
                   p => p.Delivered,
                   p => p.Bounced,
                   p => p.Opened,
                   p => p.Clicked,
                   p => p.Complained,
                   p => p.TagsAll,
                   p => p.TagsActive,
                   p => p.SavedSearchAll,
                   p => p.SavedSearchActive);

                var smtpSection = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
                string username = smtpSection.Network.UserName;
                string host = smtpSection.Network.Host;
                int port = smtpSection.Network.Port;
                string password = smtpSection.Network.Password;
                bool ssl = smtpSection.Network.EnableSsl;
                var credentials = new NetworkCredential(username, password);

                SmtpClient smtpClient = default(SmtpClient);
                smtpClient = new SmtpClient(host);
                smtpClient.Credentials = new NetworkCredential(username, password);
                smtpClient.Port = port;
                smtpClient.EnableSsl = ssl;


                if (nightlyReport != null)
                {
                    var message = new MailMessage();
                    message.From = new MailAddress(fromEmail, "Support");
                    message.Subject = "Nightly Status Report – ST";
                    message.IsBodyHtml = true;
                    message.Body = string.Format("Hi, please find campaign sender info nightly report for 1 Day <br /><br />{0}<br />For last 7 Days <br /><br />{1}", dayReportTable, weekReportTable);
                    var rcpts = toEmail.Split(',');
                    rcpts.ToList().ForEach(r =>
                    {
                        message.To.Add(r);
                    });

                    for (int i = 0; i < 2; i++)
                    {
                        System.Net.Mail.Attachment nightlyAttachment = default(System.Net.Mail.Attachment);
                        string savedExcelFileName = Path.Combine(attachmentPath, i == 0 ? dayGuid + ".xlsx" : weekGuid + ".xlsx");
                        nightlyAttachment = new System.Net.Mail.Attachment(savedExcelFileName);
                        nightlyAttachment.Name = i == 0 ? "1 Day Report.xlsx" : "7 Days Report.xlsx";
                        message.Attachments.Add(nightlyAttachment);
                    }

                    smtpClient.Send(message);
                }
                if (campaignReport != null)
                {
                    var message = new MailMessage();
                    message.From = new MailAddress(fromEmail, "Support");
                    message.Subject = "Daily Campaign Report – ST";
                    message.IsBodyHtml = true;
                    message.Body = string.Format("Hi, please find campaign sender info nightly report for 1 Day  <br /><br />{0}<br />For last 7 Days <br /><br />{1}", campaignDayReportTable, campaignWeekReportTable);
                    var rcpts = toEmail.Split(',');
                    rcpts.ToList().ForEach(r =>
                    {
                        message.To.Add(r);
                    });

                    for (int i = 0; i < 2; i++)
                    {
                        System.Net.Mail.Attachment nightlyAttachment = default(System.Net.Mail.Attachment);
                        string savedExcelFileName1 = Path.Combine(attachmentPath, i == 0 ? campaignDayGuid + ".xlsx" : campaignWeekGuid + ".xlsx");
                        nightlyAttachment = new System.Net.Mail.Attachment(savedExcelFileName1);
                        nightlyAttachment.Name = i == 0 ? "1 Day Report.xlsx" : "7 Days Report.xlsx";
                        message.Attachments.Add(nightlyAttachment);
                    }



                    smtpClient.Send(message);
                }

                //EmailAgent agent = new EmailAgent();
                //IEnumerable<ServiceProvider> serviceProviders = serviceProviderRepository.GetAccountCommunicationProviders(1, CommunicationType.Mail, MailType.TransactionalEmail);
                //if (serviceProviders != null && serviceProviders.FirstOrDefault() != null)
                //    loginToken = serviceProviders.FirstOrDefault().LoginToken;

                //if (loginToken != new Guid() && nightlyReport != null)
                //{
                //    Logger.Current.Informational("Nightly Scheduled Deliverability Report Sending Email");
                //    SendMailRequest mailRequest = new SendMailRequest();
                //    mailRequest.Body = string.Format("Hi, please find campaign sender info nightly report For 1 Day <br /><br />{0}<br />For last 7 Days <br /><br />{1}", dayReportTable, weekReportTable);
                //    mailRequest.From = fromEmail;
                //    mailRequest.IsBodyHtml = true;
                //    mailRequest.Subject = "Nightly Status Report – ST";
                //    mailRequest.DisplayName = "Support";
                //    mailRequest.To = toEmails;
                //    mailRequest.TokenGuid = loginToken;
                //    mailRequest.RequestGuid = Guid.NewGuid();
                //    mailRequest.NightlyAttachmentGUIDS = new List<Guid>() { dayGuid, weekGuid };
                //    agent.SendEmail(mailRequest);
                //}
                //if (loginToken != new Guid() && campaignReport != null)
                //{
                //    Logger.Current.Informational("Campaign Sender Deliverability Report Sending Email");
                //    SendMailRequest mailRequest = new SendMailRequest();
                //    mailRequest.Body = string.Format("Hi, please find campaign sender info nightly report For 1 Day  <br /><br />{0}<br />For last 7 Days <br /><br />{1}", campaignDayReportTable, campaignWeekReportTable);
                //    mailRequest.From = fromEmail;
                //    mailRequest.IsBodyHtml = true;
                //    mailRequest.Subject = "Daily Campaign Report – ST";
                //    mailRequest.DisplayName = "Support";
                //    mailRequest.To = toEmails;
                //    mailRequest.TokenGuid = loginToken;
                //    mailRequest.RequestGuid = Guid.NewGuid();
                //    mailRequest.NightlyAttachmentGUIDS = new List<Guid>() { campaignDayGuid, campaignWeekGuid };
                //    agent.SendEmail(mailRequest);
                //}

            }
            catch (Exception ex)
            {
                Logger.Current.Error("Nightly Scheduled Deliverability Report Exception Logging : ", ex);
            }
        }

        public static DataTable GetDataTable<T>(IEnumerable<T> reportData)
        {
            DataTable table = new DataTable();
            var properties = typeof(T).GetProperties();
            var values = new object[properties.Count()];
            if (properties.Where(c => c.Name == "Day").Any())
                values = new object[properties.Count() - 1];
            else
                values = new object[properties.Count()];

            foreach (var item in properties)
            {
                if (item.Name != "Day")
                    table.Columns.Add(item.GetCustomAttribute<DisplayNameAttribute>().DisplayName.ToString());
            }

            foreach (T data in reportData)
            {
                int count = 0;
                if (properties.Where(c => c.Name == "Day").Any())
                    count = properties.Count() - 1;
                else
                    count = properties.Count();

                for (int i = 0; i < count; i++)
                {
                    if (properties.Where(c => c.Name == "Day").Any())
                        values[i] = data.GetType().GetProperty(properties[i + 1].Name).GetValue(data, null);
                    else
                        values[i] = data.GetType().GetProperty(properties[i].Name).GetValue(data, null);
                }

                table.Rows.Add(values);
            }
            return table;
        }
    }
}
