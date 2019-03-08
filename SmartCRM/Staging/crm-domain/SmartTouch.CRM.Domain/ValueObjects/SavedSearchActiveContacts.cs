using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class SavedSearchActiveContacts
    {
        public int SearchDefinitionId { get; set; }
        public long TotalCount { get; set; }
        public long ActiveContactsCount { get; set; }
        public long NonActiveContactsCount { get; set; }

        public int ContactID { get; set; }
        public bool IsActive { get; set; }
    }
}
