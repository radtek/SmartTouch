using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum AppOperations : byte
    {
        Create = 1,
        Read = 2,
        Edit = 3,
        Delete = 4,
        CopyContact = 6,
        ExportContact = 7
    }
}
