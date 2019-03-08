using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class GetResentCampaignCountRequest :ServiceRequestBase
    {
        public int ParentCampaignId { get; set; }
        public CampaignResentTo CampaignResentTo { get; set; }
    }
    public class GetResentCampaignCountResponse : ServiceResponseBase
    {
        public int Count { get; set; }
    }
}
