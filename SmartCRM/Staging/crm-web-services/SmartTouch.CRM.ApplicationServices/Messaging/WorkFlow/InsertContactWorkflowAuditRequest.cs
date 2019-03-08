using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow
{
    public class InsertContactWorkflowAuditRequest : ServiceRequestBase
    {
        public int WorkflowId { get; set; }
        public int WorkflowActionId { get; set; }
        public int ContactId { get; set; }
        public string MessageId { get; set; }
    }

    public class InsertContactWorkflowAuditResponse : ServiceResponseBase
    { 
    
    }
}
