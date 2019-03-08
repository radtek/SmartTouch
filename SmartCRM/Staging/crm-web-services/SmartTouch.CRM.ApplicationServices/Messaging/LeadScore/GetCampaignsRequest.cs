using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.LeadScore
{
    public class GetCampaignsRequest : ServiceRequestBase
    {
        public int accountId { get; set; }
    }

    public class GetCampaignsResponse : ServiceResponseBase
    {
        public GetCampaignsResponse() { }

        public IEnumerable<CampaignEntryViewModel> Campaigns { get; set; }
    }
}
