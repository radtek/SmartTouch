using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow
{
    public class WorkflowStatusRequest : ServiceRequestBase
    {
        public WorkflowStatus Status { get; set; }
        public int WorkflowID { get; set; }
    }

    public class WorkflowStatusResponse : ServiceResponseBase
    {

    }
}
