using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class InsertNeverBounceRequest: ServiceRequestBase
    {
        public IEnumerable<int> TagIds { get; set; }
        public IEnumerable<int> SearchdefinitionIds { get; set; }
        public int TotalCount { get; set; }
    }

    public class InsertNeverBounceResponse : ServiceResponseBase
    {

    }
}
