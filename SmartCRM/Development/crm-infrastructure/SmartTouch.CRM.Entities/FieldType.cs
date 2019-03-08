using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum FieldType : byte
    {
        checkbox = 1,
        datetime = 2,
        email = 3,
        number = 5,
        radio = 6,
        text = 8,
        time = 9,
        url = 10,
        dropdown = 11,
        multiselectdropdown = 12,
        date = 13,
        textarea = 14
    }
}
