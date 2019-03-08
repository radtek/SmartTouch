using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.MarketingMessages
{
    public class DeleteMarketingMessageRequest:ServiceRequestBase
    {
        public int[] MessageIds { get; set; }
    }

    public class DeleteMarketingMessageResponse: ServiceResponseBase
    {

    }
}
