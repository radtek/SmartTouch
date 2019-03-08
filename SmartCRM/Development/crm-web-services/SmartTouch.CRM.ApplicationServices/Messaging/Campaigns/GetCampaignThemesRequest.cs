using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class GetCampaignThemesRequest : ServiceRequestBase
    {
    }

    public class GetCampaignThemesResponse : ServiceResponseBase
    {
        public IEnumerable<CampaignThemeViewModel> Themes { get; set; }
    }

}
