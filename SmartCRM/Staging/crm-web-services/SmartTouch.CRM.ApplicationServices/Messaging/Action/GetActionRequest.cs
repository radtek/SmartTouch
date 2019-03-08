using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Action
{
    public class GetActionRequest : ServiceRequestBase
    {
        public int Id { get; set; }
        public int ContactId { get; set; }
    }

    public class GetActionResponse : ServiceResponseBase
    {
        public ActionViewModel ActionViewModel { get; set; }
    }    
}
