using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class GetCampaignLinksRequest : ServiceRequestBase
    {
        public int CampaignID { get; set; }
    }

    public class GetCampaignLinksResponse : ServiceResponseBase
    {
        public IEnumerable<CampaignLinkViewModel> CampaignsLinks { get; set; }
    }
}
