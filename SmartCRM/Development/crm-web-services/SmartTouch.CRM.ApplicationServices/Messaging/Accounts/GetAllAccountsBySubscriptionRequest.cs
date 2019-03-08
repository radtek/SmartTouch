using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class GetAllAccountsBySubscriptionRequest: ServiceRequestBase
    {
        public byte ID { get; set; }
    }

    public class GetAllAccountsBySubscriptionResponse : ServiceResponseBase
    {
        public IEnumerable<AccountListViewModel> Accounts { get; set; }
    }
}
