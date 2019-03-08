using SmartTouch.CRM.Domain.Workflows;
using System.Collections.Generic;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow
{
    public sealed class UpdateActionBatchStatusRequest : ServiceRequestBase
    {
        public IEnumerable<TrackAction> TrackActions { get; set; }
        public IEnumerable<TrackActionLog> TrackActionLogs { get; set; }
    }
}
