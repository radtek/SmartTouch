using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Subscriptions
{
    public class InsertSubscriptionPermissionsRequest : ServiceRequestBase
    {
        public byte SubscriptionId { get; set; }
        public List<ModuleViewModel> ModuleViewModel { get; set; }
    }

    public class InsertSubscriptionPermissionsResponse : ServiceResponseBase
    {

    }
}
