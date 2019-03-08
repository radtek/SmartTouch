using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum UserActivityType:byte
    {
        Create = 1,
        Read = 2,
        Update = 3,
        Delete = 4,
        ChangeOwner = 5,
        LastRunOn = 6   //Reports
    }
}
