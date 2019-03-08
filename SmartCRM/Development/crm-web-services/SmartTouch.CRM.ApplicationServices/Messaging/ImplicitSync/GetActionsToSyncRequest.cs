using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.ImplicitSync
{
    public class GetActionsToSyncRequest : ServiceRequestBase
    {
        public int? MaxNumRecords { get; set; }
        public DateTime? TimeStamp { get; set; }
        public bool FirstSync { get; set; }
        public bool IsDeleted { get; set; }
    }


    public class GetActionsToSyncResponse : ServiceResponseBase
    {
        //public IEnumerable<ContactOutlookSyncViewModel> ContactsToSync { get; set; }
        public IEnumerable<int> DeletedActions { get; set; }
    }
}
