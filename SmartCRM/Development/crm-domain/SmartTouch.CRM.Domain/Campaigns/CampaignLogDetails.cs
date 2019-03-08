using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Campaigns
{
    public class CampaignLogDetails
    {
        public int CampaignLogDetailsID { get; set; }
        public int? CampaignId { get; set; }
        public int? CampaignRecipientId { get; set; }
        public string Recipient { get; set; }
        public short? DeliveryStatus { get; set; }
        public short? OptOutStatus { get; set; }
        public int? BounceCategory { get; set; }
        public DateTime? TimeLogged { get; set; }
        public string Remarks { get; set; }
        public DateTime CreatedOn { get; set; }
        public byte? Status { get; set; }
        public int? FileType { get; set; }
    }
}
