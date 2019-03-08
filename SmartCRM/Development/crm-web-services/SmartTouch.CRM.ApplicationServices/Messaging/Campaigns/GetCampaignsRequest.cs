using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class GetCampaignsRequest : ServiceRequestBase
    {
        public int Limit { get; set; }
        public string Query { get; set; }
        public int PageNumber { get; set; }
        public byte Status { get; set; }
        public bool IsWorklflowCampaign { get; set; }
    }

    public class GetCampaignsResponse : ServiceResponseBase
    {
        public long TotalHits { get; set; }
        public IEnumerable<CampaignViewModel> Campaigns { get; set; }
    }
}
