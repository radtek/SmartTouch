using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum PushNotificationStatus:byte
    {
        Created=1,
        Completed=2,
        Failed=3,
        Ignored= 4
    }
}
