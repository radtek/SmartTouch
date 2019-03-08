using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Campaigns
{
    public class CampaignLitmusMap
    {
        public int CampaignLitmusMapId { get; set; }
        public int CampaignId { get; set; }
        public string LitmusId { get; set; }
        public int ProcessingStatus { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastModifiedOn { get; set; }
        public string Remarks { get; set; }
    }
}
