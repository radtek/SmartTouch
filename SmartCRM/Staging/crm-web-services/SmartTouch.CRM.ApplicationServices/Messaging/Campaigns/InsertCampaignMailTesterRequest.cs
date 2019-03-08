using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class InsertCampaignMailTesterRequest : ServiceRequestBase
    {
        public int CampaignID { get; set; }
    }

    public class InsertCampaignMailTesterResponse : ServiceResponseBase
    {

    }
}
