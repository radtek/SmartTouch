using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Contacts
{
    public class SmartSearchContact
    {
        public int SearchDefinitionID { get; set; }
        public int AccountID { get; set; }
        public int ContactID { get; set; }
        public bool IsActive { get; set; }
    }
}
