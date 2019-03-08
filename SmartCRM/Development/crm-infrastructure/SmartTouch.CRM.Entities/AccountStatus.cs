using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum AccountStatus : byte
    {
        Active = 1,
        Draft = 105,
        Terminate = 3,
        Suspend = 4,
        Maintanance = 5,
        Delete = 6
    }
}
