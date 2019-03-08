using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Communication
{
    public class GetEmailBodyRequest : ServiceRequestBase
    {
        public int SendMailID { get; set; }
        public int ReceivedMailInfoID { get; set; }
    }

    public class GetEmailBodyResponse : ServiceResponseBase
    {
        public string EmailBody { get; set; }
    }
}
