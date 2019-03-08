using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Contacts
{
    public class SenderRecipientInfoNightlyReport
    {
        [DisplayName("Account Name")]
        public string AccountName { get; set; }
        [DisplayName("Sender Reputation Count")]
        public int SenderReputationCount { get; set; }
        [DisplayName("Campaigns Sent")]
        public int CampaignsSent { get; set; }
        [DisplayName("Recipients")]
        public int Recipients { get; set; }
        [DisplayName("Sent")]
        public int Sent { get; set; }
        [DisplayName("Delivered")]
        public int Delivered { get; set; }
        [DisplayName("Bounced")]
        public string Bounced { get; set; }
        [DisplayName("Opened")]
        public string Opened { get; set; }
        [DisplayName("Clicked")]
        public string Clicked { get; set; }
        [DisplayName("Tags All")]
        public int TagsAll { get; set; }
        [DisplayName("Tags Active")]
        public int TagsActive { get; set; }
        [DisplayName("SS All")]
        public int SSAll { get; set; }
        [DisplayName("SS Active")]
        public int SSActive { get; set; }

    }
}
