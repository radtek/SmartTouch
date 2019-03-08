using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow
{
    public class GetNotifyUserActionRequest:ServiceRequestBase
    {
        public int WorkflowActionId { get; set; }   
    }

    public class GetNotifyUserActionResponse: ServiceResponseBase
    {
        public WorkflowNotifyUserActionViewModel  NotifyUserViewModel { get; set; }
    }
}
