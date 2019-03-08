using System;
using System.Collections.Generic;
using System.IO;
using SmartTouch.CRM.Domain.Contacts;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using System.Net.Mail;

namespace LandmarkIT.Enterprise.CommunicationManager.Requests
{
    public class SendMailRequest
    {
        public int SentMailDetailID { get; set; }
        public Guid TokenGuid { get; set; }
        public Guid RequestGuid { get; set; }
        public string From { get; set; }
        public string DisplayName { get; set; }
        public string ReplyTo { get; set; }
        public MailPriority PriorityID { get; set; }
        public IEnumerable<EmailRecipient> CampaignReceipients { get; set; }
        public List<string> To { get; set; }
        public List<string> CC { get; set; }
        public List<string> BCC { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsBodyHtml { get; set; }
        public DateTime? ScheduledTime { get; set; }
        public string SenderDomain { get; set; }
        public string AccountDomain { get; set; }
        public string ServiceProviderEmail { get; set; }
        public string JobID { get; set; }
        public Guid? AttachmentGUID { get; set; }
        public string NotificationAttachementGuid { get; set; }
        public bool GetProcessedByClassic { get; set; }
        public IEnumerable<Guid> NightlyAttachmentGUIDS { get; set; }
        public Int16 CategoryID { get; set; }
        public int AccountID { get; set; }
        public Dictionary<string,Dictionary<string,string>> MergeValues { get; set; }
    }
    
    public class SendSingleMailRequest : SendMailRequest
    {
        public new string To { get; set; }
        public new string CC { get; set; }
        public new string BCC { get; set; }
    }

    public class MailAttachment
    {
        public string FileName { get; set; }
        public Stream ContentStream { get; set; }
    }

    public enum MailPriority : byte
    {
        Normal = 0,
        Low = 1,
        High = 2,
    }
    
}
