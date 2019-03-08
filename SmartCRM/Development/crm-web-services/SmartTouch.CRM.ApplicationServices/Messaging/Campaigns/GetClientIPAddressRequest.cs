using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class GetClientIPAddressRequest : ServiceRequestBase
    {
        public string Domain { get; set; }
        public int ContactId { get; set; }
        public string STITrackingID { get; set; }
    }

    public class GetClientIPAddressResponse : ServiceResponseBase
    {
        public string IPAddress { get; set; }
    }
}
