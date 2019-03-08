using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum SearchQualifier
    {
        Is = 1,
        IsNot = 2,
        IsEmpty = 3,
        IsNotEmpty = 4,
        IsGreaterThan = 5,
        IsGreaterThanEqualTo = 6,
        IsLessThan = 7,
        IsLessThanEqualTo = 8,
        Contains = 9,
        DoesNotContain = 10
    }
}
