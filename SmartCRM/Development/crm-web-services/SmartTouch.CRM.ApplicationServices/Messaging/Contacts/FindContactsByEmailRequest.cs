using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    /// <summary>
    /// Request to find contacts by primary or secondary email
    /// </summary>
    public class FindContactsByEmailRequest : ServiceRequestBase
    {
        public string Email { get; set; }
    }

    /// <summary>
    /// Returns contacts by primary or secondary email
    /// </summary>
    public class FindContactsByEmailResponse : ServiceResponseBase
    {
        public IEnumerable<int> ContactIDs { get; set; }
    }
}
