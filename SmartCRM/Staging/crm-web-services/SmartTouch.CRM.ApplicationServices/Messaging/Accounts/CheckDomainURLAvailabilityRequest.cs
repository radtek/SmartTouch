using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class CheckDomainURLAvailabilityRequest : ServiceRequestBase
    {
        public string DomainURL { get; set; }
    }

    public class CheckDomainURLAvailabilityResponse : ServiceResponseBase
    {
        public bool Available { get; set; }
        public string Message { get; set; }
    }
}
