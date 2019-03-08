using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class PrimitiveContactValue
    {
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
    }

    public class PrimitiveContactValues
    {
        public IEnumerable<PrimitiveContactValue> ContactValues { get; set; }

    }
}
