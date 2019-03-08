using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum IndexType : byte
    {
        Contacts = 1,
        Campaigns = 2,
        Opportunity = 3,
        Forms = 4,
        Tags = 5,
        Workflows = 6,
        Contacts_Delete = 7
    }
}
