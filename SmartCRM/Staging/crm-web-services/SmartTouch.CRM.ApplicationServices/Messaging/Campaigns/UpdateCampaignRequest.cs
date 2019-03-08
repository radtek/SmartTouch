using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class UpdateCampaignRequest : ServiceRequestBase
    {
        public CampaignViewModel CampaignViewModel { get; set; }
    }

    public class UpdateCampaignResponse : ServiceResponseBase
    {
        public CampaignViewModel CampaignViewModel { get; set; }
    }
}
