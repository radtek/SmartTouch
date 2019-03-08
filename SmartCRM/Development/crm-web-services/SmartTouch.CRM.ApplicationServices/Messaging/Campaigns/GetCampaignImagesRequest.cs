using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class GetCampaignImagesRequest: ServiceRequestBase
    {
        public int? AccountID { get;set; }
        public int Limit { get; set; }
        public int PageNumber { get; set; }
        public string name { get; set; }
    }

    public class GetCampaignImagesResponse : ServiceResponseBase    
    {
        public IEnumerable<string> ImageUrls { get; set; }
    }
    public class GetAccountCampaignImagesResponse : ServiceResponseBase
    {
        public int TotalHits { get; set; }
        public List<ImageViewModel> Images { get; set; }
    }
}
