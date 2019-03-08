using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class GetLinkUrlRequest : ServiceRequestBase
    {
        public int CampaignId { get; set; }
        public byte? LinkIndex { get; set; }
        public IEnumerable<int> CampaignIDs { get; set; }
        public int ContactId { get; set; }
    }

    public class GetLinkUrlResponse : ServiceResponseBase
    {
        public CampaignLinkViewModel CampaignLinkViewModel { get; set; }
        public Guid ReferenceId { get; set; }
        public IEnumerable<string> MergeFields { get; set; }
    }
    public class GetLinkUrlsResponse : ServiceResponseBase
    {
        public IEnumerable<CampaignLinkViewModel> CampaignLinks { get; set; }
    }
}
