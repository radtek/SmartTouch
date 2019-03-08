using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Communication;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class AttachmentActivityAnalyzer
    {
        IEnumerable<Attachment> attachmentActivities;
        string DateFormat;
        public AttachmentActivityAnalyzer(IEnumerable<Attachment> activities, string DateFormat)
        {
            this.attachmentActivities = activities;
            this.DateFormat = DateFormat;
        }

        public IEnumerable<Attachment> GenerateAnalysis()
        {
            var activity = new AttachmentEntryActivity(attachmentActivities, DateFormat);
            return activity.Analyze().OrderByDescending(l => l.CreatedDate);
        }
    }


    public abstract class AttachmentActivity
    {
        protected IEnumerable<Attachment> Attachments { get; set; }
        public string Message { get; set; }
        public DateTime ActivityDate { get; set; }
        public abstract IEnumerable<Attachment> Analyze();
    }

    public class AttachmentEntryActivity : AttachmentActivity
    {
        List<Attachment> analysis = new List<Attachment>();
        string DateFormat;
        public AttachmentEntryActivity(IEnumerable<Attachment> attachments, string DateFormat)
        {
            this.Attachments = attachments.ToList();
            this.DateFormat = DateFormat;
        }

        public override IEnumerable<Attachment> Analyze()
        {
            analyzeByActivity(this.Attachments, DateFormat);
            return analysis;
        }

        void analyzeByActivity(IEnumerable<Attachment> attachments, string _DateFormat)
        {
            DateTime now = DateTime.Now.ToUniversalTime();

            var lastMinuteAttachments = attachments.Where(l => (now.Date - l.CreatedDate.Date).Days == 0 && (now - l.CreatedDate).Hours == 0 && (now - l.CreatedDate).Minutes < 1);
            foreach (var attachment in lastMinuteAttachments)
            {
                attachment.AttachmentTime = "Now";
                analysis.Add(attachment);
            }

            var firstHourAttachments = attachments.Where(l => (now.Date - l.CreatedDate.Date).Days == 0 && (now - l.CreatedDate).Hours == 0 && (now - l.CreatedDate).Minutes >= 1 && (now - l.CreatedDate).Minutes < 60);
            foreach (var attachment in firstHourAttachments)
            {
                string message = "";
                if ((now - attachment.CreatedDate).Minutes == 1)
                {
                    message = (now - attachment.CreatedDate).Minutes + " minute ago";
                }
                else
                {
                    message = (now - attachment.CreatedDate).Minutes + " minutes ago";
                }
                attachment.AttachmentTime = message;
                analysis.Add(attachment);
            }

            var halfdayAttachments = attachments.Where(l => (now.Date - l.CreatedDate.Date).Days == 0 && ((now - l.CreatedDate).Hours >= 1 && (now - l.CreatedDate).Hours <= 12));
            foreach (var attacment in halfdayAttachments)
            {
                string message = "";
                if ((now - attacment.CreatedDate).Hours == 1)
                {
                    message = (now - attacment.CreatedDate).Hours + " hour ago";
                }
                else
                {
                    message = (now - attacment.CreatedDate).Hours + " hours ago";
                }
                attacment.AttachmentTime = message;
                analysis.Add(attacment);
            }

            var todayAttachments = attachments.Where(l => (now.Date - l.CreatedDate.Date).Days == 0 && ((now - l.CreatedDate).Hours > 12 && (now - l.CreatedDate).Hours <= 24));
            foreach (var attahcment in todayAttachments)
            {
                attahcment.AttachmentTime = " Today";
                analysis.Add(attahcment);
            }

            var yesterdayAttachments = attachments.Where(l => (now.Date - l.CreatedDate.Date).Days == 1);
            foreach (var attachment in yesterdayAttachments)
            {
                attachment.AttachmentTime = " Yesterday";
                analysis.Add(attachment);
            }

            var weekAttachments = attachments.Where(l => (now.Date - l.CreatedDate.Date).Days >= 2 && (now.Date - l.CreatedDate.Date).Days < 7);
            foreach (var attachment in weekAttachments)
            {
                attachment.AttachmentTime = " on " + attachment.CreatedDate.DayOfWeek.ToString();
                analysis.Add(attachment);
            }

            var remainingAttachments = attachments.Where(l => (now.Date - l.CreatedDate.Date).Days >= 7);
            foreach (var attachment in remainingAttachments)
            {
                attachment.AttachmentTime = " on " + attachment.CreatedDate.ToString(_DateFormat, CultureInfo.InvariantCulture);
                analysis.Add(attachment);
            }
        }
    }
}
