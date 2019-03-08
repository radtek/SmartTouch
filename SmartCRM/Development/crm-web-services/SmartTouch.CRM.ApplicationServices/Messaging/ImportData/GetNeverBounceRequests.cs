using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.ImportData
{
    public class GetNeverBounceRequest : ServiceRequestBase
    {
        public int Limit { get; set; }
        public int PageNumber { get; set; }
    }

    public class GetNeverBounceResponse : ServiceResponseBase
    {
        public IEnumerable<NeverBounceQueue> Queue { get; set; }
    }
}
