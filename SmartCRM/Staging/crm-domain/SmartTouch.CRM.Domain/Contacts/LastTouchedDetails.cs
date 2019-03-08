using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.Contacts
{
    public class LastTouchedDetails 
    {
        public int ContactID { get; set; }
        public DateTime LastTouchedDate { get; set; }
        public int ActionID { get; set; }
    }
}
