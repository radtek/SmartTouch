using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Campaigns
{
    public class CampaignUniqueRecipient
    {
        public int EntityType { get; set; }
        public int EntityId { get; set; }
        public int Total { get; set; }
    }
}
