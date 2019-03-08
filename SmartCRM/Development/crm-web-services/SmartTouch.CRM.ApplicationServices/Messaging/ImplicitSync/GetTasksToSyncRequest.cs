using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.ImplicitSync
{
    public class GetTasksToSyncRequest : ServiceRequestBase
    {
        public int? MaxNumRecords { get; set; }
        public DateTime? TimeStamp { get; set; }
        public bool FirstSync { get; set; }
        public bool IsDeleted { get; set; }
        public CRUDOperationType OperationType { get; set; }
    }

    public class GetTasksToSyncResponse : ServiceResponseBase
    {
        public IEnumerable<CRMOutlookSyncViewModel> CRMOutlookSyncMappings { get; set; }
        public IEnumerable<ActionViewModel> ActionsToSync { get; set; }
        public IEnumerable<int> DeletedActions { get; set; }
    }
}
