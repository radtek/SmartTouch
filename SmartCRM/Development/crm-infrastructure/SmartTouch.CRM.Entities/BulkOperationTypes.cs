using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum BulkOperationTypes : byte
    {
        Action = 1,
        Note = 2,
        Tag = 3,
        Tour = 4,
        SendEmail = 5,
        SendText = 6,
        Relationship = 7,
        ChangeOwner = 8,
        Delete = 9,
        Export = 10
    }
}
