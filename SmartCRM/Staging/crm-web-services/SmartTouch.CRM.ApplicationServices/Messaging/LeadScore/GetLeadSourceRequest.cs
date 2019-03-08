using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.LeadScore
{
    public class GetLeadSourceRequest : ServiceRequestBase
    {
        public GetLeadSourceRequest() { }
    }

    public class GetLeadSourceResponse : ServiceResponseBase
    {
        public GetLeadSourceResponse() { }

        public IEnumerable<dynamic> LeadSource { get; set; }
    }
}
