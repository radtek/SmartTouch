using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.LeadAdapters
{
    public class GetLeadAdapterTypesRequest : ServiceRequestBase
    {
        public GetLeadAdapterTypesRequest() { }
    }
    public class GetLeadAdapterTypesResponse : ServiceResponseBase
    {
        public IEnumerable<dynamic> LeadAdapterTypes { get; set; }
    }
}
