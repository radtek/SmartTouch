using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.Infrastructure;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Geo
{
    public class GetCountriesRequest : ServiceRequestBase
    {
        public GetCountriesRequest() { }
    }

    public class GetCountriesResponse:ServiceResponseBase
    {
        public GetCountriesResponse() { }

        public IEnumerable<dynamic> Countries { get; set; }
    }
}
