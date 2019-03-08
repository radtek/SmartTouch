using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class GetCampaignMailTesterGuid : ServiceRequestBase
    {
        public int CampaignID { get; set; }
    }

    public class GetCampaignMailTesterGuidResponse : ServiceResponseBase
    {
        public string Guid { get; set; }
    }
}
