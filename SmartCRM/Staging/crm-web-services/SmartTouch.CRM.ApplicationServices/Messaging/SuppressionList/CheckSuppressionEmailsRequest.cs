using System.Collections.Generic;

namespace SmartTouch.CRM.ApplicationServices.Messaging.SuppressionList
{
    public class CheckSuppressionEmailsRequest : ServiceRequestBase
    {
        public IEnumerable<string> Emails { get; set; }
    }

    public class CheckSuppressionEmailsResponse : ServiceResponseBase
    {
        public IEnumerable<string> SuppressedEmails { get; set; }
    }
}
