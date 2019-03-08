using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class QueueCampaignRequest:ServiceRequestBase
    {
        public CampaignViewModel CampaignViewModel { get; set; }
    }

    public class QueueCampaignResponse : ServiceResponseBase
    {
        public int CampaignId { get; set; } 
        public int TotalRecipients {get;set;}
    }
}
