using SmartTouch.CRM.Domain.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class GetAccountSubscriptionDataRequest :ServiceRequestBase
    {
        
    }
    public class GetAccountSubscriptionDataResponse : ServiceResponseBase
    {
        public AccountSubscriptionData AccountSubscriptionData { get; set; }
    }
}
