using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class GetCampaignStatisticsRequest : ServiceRequestBase
    {
        public int CampaignId { get; set; }
    }

    public class GetCampaignStatisticsResponse : ServiceResponseBase
    {
        public CampaignStatisticsViewModel CampaignStatisticsViewModel { get; set; }
        public bool IsParentCampaign { get; set; }
 
    }
}
