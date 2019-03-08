using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class CampaignTrackerDb
    {
        [Key]
        public int CampaignTrackerID { get; set; }
        public int AccountID { get; set; }
        
        [ForeignKey("Campaign")]
        public int CampaignID { get; set; }
        
        public CampaignsDb Campaign { get; set; }

        public CampaignContactActivity ActivityType { get; set; }
        
        [ForeignKey("CampaignLink")]
        public int? CampaignLinkID { get; set; }
        public CampaignLinksDb CampaignLink { get; set; }

        public byte? LinkIndex { get; set; }
        public DateTime ActivityDate { get; set; }

        [ForeignKey("CampaignRecipients")]
        public int CampaignRecipientId { get; set; }
        public CampaignRecipientsDb CampaignRecipients { get; set; }
    }
}
