using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Communication
{
    public class GetDateFormatRequest: ServiceRequestBase
    {
        public GetDateFormatRequest() { }
    }

    public class GetDateFormatResponse : ServiceResponseBase
    {
        public GetDateFormatResponse() { }
        public IEnumerable<dynamic> DateFormats { get; set; }
    }
}
