using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.ImplicitSync
{
    public class GetToursToSyncRequest : ServiceRequestBase
    {
        public int? MaxNumRecords { get; set; }
        public DateTime? TimeStamp { get; set; }
        public bool FirstSync { get; set; }
        public bool IsDeleted { get; set; }
        public CRUDOperationType OperationType { get; set; }
    }

    public class GetToursToSyncResponse : ServiceResponseBase
    {
        public IEnumerable<CRMOutlookSyncViewModel> CRMOutlookSyncMappings { get; set; }
        public IEnumerable<TourViewModel> ToursToSync { get; set; }
        public IEnumerable<int> DeletedTours { get; set; }
        public IEnumerable<KeyValuePair<int,int>> CompletedContacts { get; set; }
    }
}
