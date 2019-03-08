using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Enterprices
{
   public class SendEmailRequest: ServiceRequestBase
    {
        public IEnumerable<int> Contacts { get; set; }
        public IEnumerable<string> FormSubmissionIds { get; set; }
        public string EmailBody { get; set; }
    }

    public class SendEmailResponse: ServiceResponseBase
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
