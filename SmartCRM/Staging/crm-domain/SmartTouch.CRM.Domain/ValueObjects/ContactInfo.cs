using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class ContactInfo
    {
        public string familyName { get; set; }
        public string fullName { get; set; }
        public string givenName { get; set; }
    }
}
