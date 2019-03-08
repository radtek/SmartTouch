using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics
{
    public class ValidateVisiStatKeyRequest : ServiceRequestBase
    {
        public string VisiStatKey { get; set; }
        public string TrackingDomain { get; set; }
    }

    public class ValidateVisiStatKeyResponse : ServiceResponseBase
    {
        public bool IsValidKey { get; set; }
        public string ResponseDescription { get; set; }
    }
}
