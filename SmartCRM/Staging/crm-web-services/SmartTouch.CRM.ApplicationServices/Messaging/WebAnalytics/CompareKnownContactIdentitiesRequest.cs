using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics
{
    public class CompareKnownContactIdentitiesRequest : ServiceRequestBase
    {
        public IEnumerable<string> ReceivedIps { get; set; }
        public IEnumerable<string> ReceivedIdentities { get; set; }

    }

    public class CompareKnownContactIdentitiesResponse : ServiceResponseBase
    {
        public IEnumerable<string> KnownIps { get; set; }
        public IEnumerable<string> KnownIdentities { get; set; }

    }
}
