using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ImportData
{
    public class ImportCustomData
    {
        public Int64 FieldID { get; set; }
        public int? FieldTypeID { get; set; }
        public string FieldValue { get; set; }
        public Guid? ReferenceID { get; set; }
    }
}
