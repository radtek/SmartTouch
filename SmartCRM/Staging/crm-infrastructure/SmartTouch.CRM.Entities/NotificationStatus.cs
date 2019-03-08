using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum NotificationStatus : byte
    {
        New = 1,
        Viewed = 2,
        Removed = 3
    }

    public enum NotificationSource : byte
    {
        Action = 1,
        Tour = 2,
        WebVisit = 3,
        System = 10,
        LitmusResults =77
    }
}
