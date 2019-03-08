using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum RequestOrigin : byte
    {
        CRM = 1,
        Forms = 2,
        Outlook = 3,
        Imports = 4,
        LeadAdapters = 5,
        API = 6
    }
}
