using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.ValueObjects;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Geo
{
    public class GetAllStatesRequest: ServiceRequestBase
    {}

    public class GetAllStatesResponse : ServiceResponseBase
    {
        public IEnumerable<State> States { get; set; }
    }
}
