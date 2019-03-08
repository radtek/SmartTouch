using SmartTouch.CRM.Domain.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetContactFormSubmissionsRequest : ServiceRequestBase
    {
        public int ContactId { get; set; }
        public int[] ContactIds { get; set; }
    }

    public class GetContactFormSubmissionsResponse : ServiceResponseBase
    {
        public IEnumerable<FormSubmission> FormSubmissions { get; set; }
    }
}
