using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetContactAuditLeadScoreRequest : ServiceRequestBase
    {
        public int ContactId { get; set; }
        public DateTime Period { get; set; }
    }
    public class GetContactAuditLeadScoreResponse : ServiceResponseBase
    {
        public int? LeadScore { get; set; }
    }
}
