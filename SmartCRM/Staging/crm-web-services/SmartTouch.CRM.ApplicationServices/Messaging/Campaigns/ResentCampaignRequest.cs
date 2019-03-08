using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class ResentCampaignRequest : ServiceRequestBase
    {
        public int ParentCampaignId { get; set; }
        public int CampaignId { get; set; }
        public CampaignResentTo CampaignResentTo { get; set; }
    }
    public class ResentCampaignResponse : ServiceResponseBase
    {
    }
}
