using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Communication
{
    public  class SendSmsIdeaTextRequest:ServiceRequestBase
    {
        public string To { get; set; }
        public string Message { get; set; }
        public DateTime? ScheduledTime { get; set; }

    }

    public class SendSmsIdeaResponse:ServiceResponseBase
    {
        public string ServiceResponse { get; set; }
      //  public string Message { get; set; }
    }
}
