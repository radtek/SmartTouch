using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Action
{
    public class BulkSendEmailRequest:ServiceRequestBase
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public int[] ActionIds { get; set; }
    }

    public class BulkSendEmailResponse:ServiceResponseBase
    {

    }
}
