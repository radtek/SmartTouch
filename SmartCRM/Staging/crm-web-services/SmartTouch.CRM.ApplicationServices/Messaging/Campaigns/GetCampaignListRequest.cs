using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class GetCampaignListRequest : ServiceRequestBase
    {
        public int Id { get; set; }
    }

    public class GetCampaignListResponse : ServiceResponseBase
    {
        public IEnumerable<CampaignViewModel> CampaignsListViewModel { get; set; }
    }
}
