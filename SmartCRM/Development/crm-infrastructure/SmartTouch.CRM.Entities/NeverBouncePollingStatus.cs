using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum NeverBouncePollingStatus : byte
    {
        Received = 0,
        Indexing = 1,
        Awaiting = 2,
        Processed = 3,
        Completed = 4,
        Failed = 5
    }
}
