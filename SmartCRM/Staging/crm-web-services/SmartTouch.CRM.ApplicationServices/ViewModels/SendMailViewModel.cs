
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
   public class SendMailViewModel
    {
       //public List<string> To { get; set; }
       //public List<string> CC { get; set; }
       //public List<string> Bcc { get; set; }
       //public IDictionary<string, Stream> Attachments { get; set; }
       //public string From { get; set; }
       //public string Body { get; set; }
       //public string Subject { get; set; }
       //public int UserId { get; set; }

        public string To { get; set; }
        public IEnumerable<dynamic> Contacts { get; set; }
       
        public string CC { get; set; }
        public string BCC { get; set; }
        public IDictionary<string, Stream> Attachments { get; set; }
        public string From { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
        public int AccountID { get; set; }
        public DateTime ScheduleTime { get; set; }
        public int ProviderID { get; set; }
        public byte ServiceProvider { get; set; }
        public string EmailSignature { get; set; }
        public bool CampaignUnsuscribeStatus { get; set; }
        public string AccountPrivacyPolicy { get; set; }
        public string UnsuscribeLink { get; set; }
        public string SenderName { get; set; }
        public byte? CampaignTypeId { get; set; }
        //selfCampaign.contactsValidation.clearError();
    }
}
