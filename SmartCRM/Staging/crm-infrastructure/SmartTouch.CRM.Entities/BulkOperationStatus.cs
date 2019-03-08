using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum BulkOperationStatus : byte
    {
        Created = 1,
        Failed = 2,
        Completed = 3
    }
}
