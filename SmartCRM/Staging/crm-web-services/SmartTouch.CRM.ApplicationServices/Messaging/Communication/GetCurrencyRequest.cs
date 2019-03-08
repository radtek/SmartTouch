using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Communication
{
    public class GetCurrencyRequest : ServiceRequestBase
    {
        public GetCurrencyRequest() { }
    }

    public class GetCurrencyResponse : ServiceResponseBase
    {
        public GetCurrencyResponse() { }
        public IEnumerable<dynamic> Currencies {get; set;}
    }
}
