using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.Infrastructure;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Geo
{
    public class GetStatesRequest: ServiceRequestBase
    {
        public string CountryCode { get; private set; }
        
        public GetStatesRequest(string countryCode) {
            this.CountryCode = countryCode;
        }
    }

    public class GetStatesResponse : ServiceResponseBase
    {
        public GetStatesResponse() { }

        //Name, Code
        public IEnumerable<dynamic> States { get; set; }
    }
}
