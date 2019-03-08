using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum SubscriptionSettingTypes : byte
    {
        Master = 1,
        Login = 2,
        ForgotPassword = 3,
        ResetPassword = 4,
        Register = 5
    }
}
