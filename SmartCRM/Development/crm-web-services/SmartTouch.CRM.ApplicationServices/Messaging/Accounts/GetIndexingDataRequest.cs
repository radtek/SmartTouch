using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class GetIndexingDataRequest : ServiceRequestBase
    {
        public int ChunkSize { get; set; }
    }
    public class GetIndexingDataResponce : ServiceResponseBase
    {
        public IEnumerable<IndexingData> IndexingData { get; set; }
    }
    public class UpdateIndexingStatusRequest : ServiceRequestBase
    {
        public IEnumerable<Guid> ReferenceIds { get; set; }
        public int Status { get; set; }
    }
    public class UpdateIndexingStatusResponse:ServiceResponseBase
    {

    }
}
