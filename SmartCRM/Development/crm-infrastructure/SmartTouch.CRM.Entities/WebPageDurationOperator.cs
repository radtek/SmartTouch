using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum WebPageDurationOperator : byte
    {
        Undefined = 0,
        LessThan = 1,
        IsEqualTo = 2,
        GreaterThan = 3
    }
}
