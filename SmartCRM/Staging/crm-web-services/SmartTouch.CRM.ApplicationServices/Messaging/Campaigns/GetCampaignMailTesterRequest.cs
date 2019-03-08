using SmartTouch.CRM.Domain.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class GetCampaignMailTesterRequest : ServiceRequestBase
    {

    }

    public class GetCampaignMailTesterResponse : ServiceResponseBase
    {
        public IEnumerable<CampaignMailTester> CampaignMailTesterData { get; set; }
    }
}
