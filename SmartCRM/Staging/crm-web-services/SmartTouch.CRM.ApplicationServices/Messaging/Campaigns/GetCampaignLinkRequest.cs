using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.ValueObjects;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class GetCampaignLinkRequest: ServiceRequestBase
    {
        public int CampaignId { get; set; }
        public int LinkIndex { get; set; }
    }

    public class GetCampaignLinkResponse : ServiceResponseBase
    {
        public CampaignLink Link { get; set; }
    }
}
