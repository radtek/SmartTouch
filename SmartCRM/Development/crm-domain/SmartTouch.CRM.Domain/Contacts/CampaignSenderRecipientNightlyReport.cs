using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Contacts
{
    public class CampaignSenderRecipientNightlyReport
    {
        [DisplayName("Day")]
        public int Day { get; set; }
        [DisplayName("Account Name")]
        public string AccountName { get; set; }
        [DisplayName("CampaignId")]
        public int CampaignId { get; set; }
        [DisplayName("Campaign Subject")]
        public string CampaignSubject { get; set; }
        [DisplayName("Vmta")]
        public string Vmta { get; set; }
        [DisplayName("Recipients")]
        public int Recipients { get; set; }
        [DisplayName("Sent")]
        public int Sent { get; set; }
        [DisplayName("Sent Date")]
        public DateTime SentDate { get; set; }
        [DisplayName("Delivered")]
        public string Delivered { get; set; }
        [DisplayName("Bounced")]
        public string Bounced { get; set; }
        [DisplayName("Opened")]
        public string Opened { get; set; }
        [DisplayName("Clicked")]
        public string Clicked { get; set; }
        [DisplayName("Complained")]
        public string Complained { get; set; }
        [DisplayName("Tags All")]
        public int TagsAll { get; set; }
        [DisplayName("Tags Active")]
        public int TagsActive { get; set; }
        [DisplayName("Saved Search All")]
        public int SavedSearchAll { get; set; }
        [DisplayName("Saved Search Active")]
        public int SavedSearchActive { get; set; }
    }
}
