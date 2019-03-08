using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class CampaignClicks : ValueObjectBase
    {
        public int CampaignId { get; set; }
        public string CampaignName { get; set; }
        public int CreatedBy { get; set; }
        public string OpenRate { get; set; }
        public string ClickRate { get; set; }
        public int CampaignLinkClicks { get; set; }
      
        public int CampaignDeliveryCount { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
