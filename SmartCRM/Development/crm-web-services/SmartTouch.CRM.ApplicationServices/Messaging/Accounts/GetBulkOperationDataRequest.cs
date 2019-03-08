using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class GetBulkOperationDataRequest : ServiceRequestBase
    {

    }
    public class GetBulkOperationDataResponse : ServiceResponseBase
    {
        public BulkOperations BulkOperations { get; set; }
        public int[] BulkContactIDs { get; set; }
    }
    public class UpdateBulkOperationStatusRequest : ServiceRequestBase
    {
        public int BulkOperationId { get; set; }
        public BulkOperationStatus Status { get; set; }
    }
    public class UpdateBulkOperationStatusResponse : ServiceResponseBase
    {

    }
}
