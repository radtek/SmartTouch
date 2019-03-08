using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.MarketingMessages
{
   public class GetAllMarketingMessageContentRequest : ServiceRequestBase
    {
        public int AccountID { get; set; }
    }

    public class GetAllMarketingMessageContentResponse : ServiceResponseBase
    {
        public IEnumerable<MarketingMessageContentMapViewModel> marketingMessagesViewModel { get; set; }
    }
}
