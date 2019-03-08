using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow
{
    public class NotifyUserRequest : ServiceRequestBase
    {
        public string WorkflowName { get; set; }
        public IEnumerable<int> UserIds { get; set; }
        public IEnumerable<int> NotificationFieldIds { get; set; }
        public byte NotifyType { get; set; }
        public string Message { get; set; }
        public int ContactId { get; set; }
        public LeadScoreConditionType trigger { get; set; }
        public int? LinkEntityId { get; set; }
        public int? EntityId { get; set; }
        public int WorkflowActionId { get; set; }
        public int WorkflowId { get; set; }
    }

    public class NotifyUserResponse : ServiceResponseBase
    { 
    
    }
}
