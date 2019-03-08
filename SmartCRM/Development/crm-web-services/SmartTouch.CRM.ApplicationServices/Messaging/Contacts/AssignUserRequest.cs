using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class AssignUserRequest : ServiceRequestBase
    {
        public int ContactId { get; set; }
        public int WorkflowID { get; set; }
        public int userAssignmentActionID { get; set; }
        public byte ScheduledID { get; set; }
    }

    public class AssignUserResponse : ServiceResponseBase
    { 
    
    }
}
