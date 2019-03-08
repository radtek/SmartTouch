using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.MarketingMessages
{
   public class InsertMarketingMessageRequest:ServiceRequestBase
    {
        public MarketingMessagesViewModel marketingMessageViewModel { get; set; }
    }

    public class InsertMarketingMessageResponse : ServiceResponseBase
    {

    }
}
