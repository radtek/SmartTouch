using SmartTouch.CRM.Domain.SuppressedEmails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.SuppressionList
{
    public class GetSuppressionEmailsRequest : ServiceRequestBase
    {

    }

    public class GetSuppressionEmailsResponse : ServiceResponseBase
    {
        public IEnumerable<SuppressedEmail> SuppressedEmails { get; set; }
    }
}
