using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Action
{
    public class DeleteActionsRequest: ServiceRequestBase
    {
        public int[] ActionIds { get; set; }        
    }
    public class DeleteActionsResponse : ServiceResponseBase
    {
    }
}
