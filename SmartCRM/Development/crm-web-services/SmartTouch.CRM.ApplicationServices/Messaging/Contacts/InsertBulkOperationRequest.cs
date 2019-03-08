using SmartTouch.CRM.Domain.Contacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class InsertBulkOperationRequest : ServiceRequestBase
    {
        public BulkOperations OperationData { get; set; }
        public DateTime CreatedOn { get; set; }
        public int[] DrillDownContactIds { get; set; }
    }
    public class InsertBulkOperationResponse : ServiceResponseBase
    {

    }
}
