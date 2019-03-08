using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Forms
{
    public class SpamValidator
    {
        public string Validator { get; set; }
        public string Value { get; set; }
        public int AccountID { get; set; }
        public bool RunValidation { get; set; }
        public int Order { get; set; }
    }
}
