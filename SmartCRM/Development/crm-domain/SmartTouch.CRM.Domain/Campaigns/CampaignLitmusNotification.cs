using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Campaigns
{
    public class CampaignLitmusNotification
    {
        public int CampaignId { get; set; }
        public DateTime LastModifiedOn { get; set; }
        public string Name { get; set; }
        public int LastUpdatedBy { get; set; }
    }
}
