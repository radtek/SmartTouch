using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ImplicitSync
{
    public class OutlookEmailInformation
    {
        public int ContactID { get; set; }
        public string Email { get; set; }
        public int ContactEmailID { get; set; }
    }
}
