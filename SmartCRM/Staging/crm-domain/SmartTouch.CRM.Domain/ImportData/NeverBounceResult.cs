using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ImportData
{
    public class NeverBounceResult
    {
        public int ContactID { get; set; }
        public int ContactEmailID { get; set; }
        public bool IsValid { get; set; }
        public int NeverBounceRequestID { get; set; }
    }
}
