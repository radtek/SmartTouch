using SmartTouch.CRM.Domain.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class GetAutomationCampaignRequest : ServiceRequestBase
    {
        public IEnumerable<int> CampaignIds { get; set; }
    }

    public class GetAutomationCampaignResponse : ServiceResponseBase
    {
        public IEnumerable<Campaign> Campaigns { get; set; }
    }

}
