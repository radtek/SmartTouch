using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum NeverBounceStatus : short
    {
        LeadAdapterQueued = 900,
        Queued = 901,
        Accepted = 902,
        Rejected = 903,
        CSVGenerated = 904,
        Polling = 905,
        PollingCompleted = 906,
        Processed = 907,
        Failed = 908
    }
}
