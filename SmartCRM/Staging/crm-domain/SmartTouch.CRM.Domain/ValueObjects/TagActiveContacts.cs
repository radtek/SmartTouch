using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class TagActiveContacts
    {
        public int TagID { get; set; }
        public int ContactID { get; set; }
        public bool IsActive { get; set; }
    }
}
