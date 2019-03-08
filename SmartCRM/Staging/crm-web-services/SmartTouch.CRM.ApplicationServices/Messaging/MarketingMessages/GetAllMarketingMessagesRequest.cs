using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.MarketingMessages
{
   public class GetAllMarketingMessagesRequest:ServiceRequestBase
    {
        public int Limit { get; set; }
        public int PageNumber { get; set; }
    }
    public class GetAllMarketingMessagesResponse: ServiceResponseBase
    {
        public int TotalHits { get; set; }
        public IEnumerable<MarketingMessagesViewModel> MarketingMessagesViewModel { get; set; }
    }

}
