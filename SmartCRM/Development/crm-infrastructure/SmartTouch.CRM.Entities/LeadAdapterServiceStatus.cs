using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum LeadAdapterServiceStatus : short
    {
        InvalidXMLFile = 301,
        InvalidCredentials = 302,
        AllContactsInserted = 303,
        PartialContactsInserted = 304,
        AllContactsFailed = 305,
        Other = 306,
        TokenExpired = 307,
        InvalidRequest = 308
    }
}
