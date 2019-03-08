using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class UnsubscribeContactRequest: ServiceRequestBase
    {

    }

    public class UnsubscribeContactResponse : ServiceResponseBase
    {
        public bool Success { get; set; }
        public string Acknowledgement { get; set; }
    }
}
