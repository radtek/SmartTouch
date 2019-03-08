using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class GetUserTimeZoneRequest : ServiceRequestBase
    {
    }

    public class GetUserTimeZoneResponse : ServiceResponseBase
    {
        public string TimeZone { get; set; }
    }
}
