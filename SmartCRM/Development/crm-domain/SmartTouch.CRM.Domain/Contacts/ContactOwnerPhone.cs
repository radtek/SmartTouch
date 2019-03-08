using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Contacts
{
    public class ContactOwnerPhone
    {
        public int ContactID { get; set; }
        public string OwnerNumber { get; set; }
        public string ContactNumber { get; set; }
    }
}
