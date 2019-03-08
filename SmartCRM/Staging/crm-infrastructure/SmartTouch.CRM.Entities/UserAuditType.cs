using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum UserAuditType : byte
    {
        ProfileUpdate = 1,
        PasswordChange = 2
    }
}
