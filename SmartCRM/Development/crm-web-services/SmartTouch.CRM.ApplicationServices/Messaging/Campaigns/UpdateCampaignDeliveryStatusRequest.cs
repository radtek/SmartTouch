using SmartTouch.CRM.Domain.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class UpdateCampaignDeliveryStatusRequest : ServiceRequestBase
    {
        public List<KeyValuePair<string,string>> DataReceived { get; set; }
    }

    public class UpdateCampaignDeliveryStatusResponse : ServiceResponseBase
    {
        public Campaign Campaign { get; set; }
    }
}
