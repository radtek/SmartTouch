using SmartTouch.CRM.Domain.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class GetCampaignLitmusResponse: ServiceResponseBase
    {
        public IEnumerable<CampaignLitmusMap> CampaignLitmusMaps { get; set; }
        public string LitmusId { get; set; }
    }
    public class UpdateCampaignLitmusMap :ServiceRequestBase
    {
        public CampaignLitmusMap CampaignLitmusMap { get; set; }
    }
    public class RequestLitmusCheck:ServiceRequestBase
    {
        public int CampaignId { get; set; }
    }

    public class GetCampaignLitmusMapRequest : ServiceRequestBase
    {
        public int CampaignId { get; set; }
    }

}
