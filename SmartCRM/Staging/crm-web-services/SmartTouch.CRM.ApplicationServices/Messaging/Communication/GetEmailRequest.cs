using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Communication
{
    public class GetEmailRequest : ServiceRequestBase
    {
        public GetEmailRequest() { }
        public int userId { get; set; }
        public int accountId { get; set; }
        public int roleId { get; set; }
        public EmailStatus EmailStatus { get; set; }
    }

    public class GetEmailResponse : ServiceResponseBase
    {
        public GetEmailResponse() { }
        public IList<Email> Emails { get; set; }
    }
}
