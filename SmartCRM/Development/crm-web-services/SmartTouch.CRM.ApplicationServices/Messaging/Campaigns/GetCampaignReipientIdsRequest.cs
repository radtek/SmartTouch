using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class GetCampaignRecipientIdsRequest : ServiceRequestBase
    {
        public int CampaignId { get; set; }
    }
    public class GetCampaignRecipientIdsResponse : ServiceResponseBase
    {
        public List<int> ContactIds { get; set; }
    }
}
