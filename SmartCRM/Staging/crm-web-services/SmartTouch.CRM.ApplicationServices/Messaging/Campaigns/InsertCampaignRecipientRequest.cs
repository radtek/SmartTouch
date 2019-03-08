using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class InsertCampaignRecipientRequest : ServiceRequestBase
    {
        public int ContactId { get; set; }
        public int CampaignId { get; set; }
        public int WorkflowId { get; set; }
    }

    public class InsertCampaignRecipientResponse : ServiceResponseBase 
    {
         
    }
}
