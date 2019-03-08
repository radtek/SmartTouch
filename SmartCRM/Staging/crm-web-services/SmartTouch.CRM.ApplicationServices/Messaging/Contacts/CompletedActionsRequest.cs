using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class CompletedActionsRequest: ServiceRequestBase    
    {
        public int[] ActionIds { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsSheduled { get; set; }
        public bool AddToNoteSummary { get; set; }

    }
    public class CompletedActionsResponse : ServiceResponseBase
    {

    }
}
