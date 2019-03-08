using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum DuplicateLogic : byte
    {
        OnlyEmailAddress = 1,
        EmailAddressAndFullNameAndCompany = 2
    }
}
