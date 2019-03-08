using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class OutlookEmailSentRequest : ServiceRequestBase
    {
        public DateTime SentDate { get; set; }
    }

    public class OutllokEmailSentResposne : ServiceResponseBase
    {
        public bool isEmailAlreadySent { get; set; }
    }
}
