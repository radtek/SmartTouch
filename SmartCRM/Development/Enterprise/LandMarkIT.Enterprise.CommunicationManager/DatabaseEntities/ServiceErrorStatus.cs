using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandmarkIT.Enterprise.CommunicationManager.DatabaseEntities
{
    public enum ServiceErrorStatus:short
    {
        InvalidXMLFile = 301,
        InvalidCredentials = 302,
        AllContactsInserted = 303,
        PartialContactsInserted = 304,
        AllContactsFailed = 305,
        Other = 306
    }
}
