using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Forms
{
   public class SubmittedFormFieldData
    {
        public int SubmittedFormFieldDataID { get; set; }
        public int SubmittedFormDataID { get; set; }
        public string Field { get; set; }
        public string Value { get; set; }
    }
}
