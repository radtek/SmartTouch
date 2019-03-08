using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Contacts
{
    public class ContactCampaigSummary
    {
        public int CampaignID { get; set; }
        public string CampaigName { get; set; }
        public DateTime SentOn { get; set; }
        public bool OpenStatus { get; set; }
        public int LinksClicked { get; set; }
        public DateTime? LastActivity { get; set; }
        public int CampaignRecipientID { get; set; }

    }
}
