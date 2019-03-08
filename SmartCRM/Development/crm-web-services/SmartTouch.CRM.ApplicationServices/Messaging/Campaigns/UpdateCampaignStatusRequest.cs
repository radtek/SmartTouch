using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class UpdateCampaignStatusRequest : ServiceRequestBase
    {
        public int campaignId { get; set; }
        public CampaignStatus Status { get; set; }
    }

    public class UpdateCampaignStatusResponse : ServiceResponseBase
    {

    }
}
