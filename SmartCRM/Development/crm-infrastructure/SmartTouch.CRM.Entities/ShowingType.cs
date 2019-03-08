using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum ShowingType : byte
    {
        AdvancedSearch = 1,
        Report = 2,
        Action = 3,
        Opportunity = 4,
        ContactsGrid = 5,
        Forms = 6,
        NewContactsReport = 7,
        TagsReport = 8
    }
}
