using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Subscriptions
{
   public class GetAllAccountSubscriptionTypesRequest:ServiceRequestBase
    {

    }

    public class GetAllAccountSubscriptionTypesResponse : ServiceResponseBase
    {
        public IEnumerable<SubscriptionViewModel> subscriptionViewModel { get; set; }
    }
}
