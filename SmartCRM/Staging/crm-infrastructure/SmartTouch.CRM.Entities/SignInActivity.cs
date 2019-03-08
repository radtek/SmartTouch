using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum SignInActivity : byte
    {
        SignIn = 1,
        SignOut = 2,
        ForgotPassword = 3,
        ResetPassword = 4
    }
}
