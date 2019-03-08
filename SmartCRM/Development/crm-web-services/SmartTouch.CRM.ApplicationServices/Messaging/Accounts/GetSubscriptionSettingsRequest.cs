using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class GetSubscriptionSettingsRequest : ServiceRequestBase
    {
        public int SubscriptionId { get; set; }
        public string DomainUrl { get; set; }
      
    }
    public class GetSubscriptionSettingsResponse : ServiceResponseBase
    {
        public IEnumerable<SubscriptionSettings> SubscriptionSettings { get; set; }

    }
}
