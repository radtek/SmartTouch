using SmartTouch.CRM.Domain.Workflows;
using System.Collections.Generic;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow
{
    public class GetNextBatchToProcessResponse : ServiceResponseBase
    {
        public IEnumerable<TrackAction> TrackActions { get; set; }
    }
}
