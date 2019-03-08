using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum EmailStatus: byte
    {
        NotVerified = 50,
        Verified = 51,
        SoftBounce = 52,
        HardBounce = 53,
        UnSubscribed = 54,
        Subscribed = 55,
        Complained = 56,
        Suppressed = 57
    }

    public enum EmailContactActivity : byte
    {
        Open = 1,
        Click = 2
    }
}
