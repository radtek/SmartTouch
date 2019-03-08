using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class UpdateSyncedEntitiesRequest : ServiceRequestBase
    {
        public Dictionary<int, string> SyncedEntities { get; set; }
    }

    public class UpdateSyncedEntitiesResponse : ServiceResponseBase
    {
 
    }
}
