using SmartTouch.CRM.Domain.ImportData;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.ImportData
{
    public class GetNeverBounceAcceptedRequests : ServiceRequestBase
    {
        public NeverBounceStatus Status { get; set; }
    }

    public class GetNeverBounceAcceptedResponse : ServiceResponseBase
    {
        public IEnumerable<NeverBounceRequest> Requests { get; set; }
    }
}
