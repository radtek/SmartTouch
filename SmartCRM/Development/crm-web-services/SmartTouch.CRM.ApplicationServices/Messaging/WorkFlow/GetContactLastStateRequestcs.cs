using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow
{
    public class GetContactLastStateRequest : ServiceRequestBase
    {
        public int ContactId { get; set; }
        public int WorkflowId { get; set; }
    }

    public class GetContactLastStateResponse : ServiceResponseBase
    {
        public int WorkflowActionId { get; set; } 
    }
}
