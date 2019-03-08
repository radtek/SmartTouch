using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow
{
    public class UpdateNotifyUserActionRequest:ServiceRequestBase
    {
        public WorkflowNotifyUserActionViewModel NotifyUserActionViewModel { get; set; }
    }

    public class UpdateNotifyUserActionResponse:ServiceResponseBase
    {

    }
}
