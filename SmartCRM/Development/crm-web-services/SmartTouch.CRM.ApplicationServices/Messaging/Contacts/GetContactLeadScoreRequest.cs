using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetContactLeadScoreRequest:ServiceRequestBase
    {
        public int ContactId { get; set; }
    }

    public class GetContactLeadScoreResponse : ServiceResponseBase
    {
        public int LeadScore { get; set; }
    }
}
