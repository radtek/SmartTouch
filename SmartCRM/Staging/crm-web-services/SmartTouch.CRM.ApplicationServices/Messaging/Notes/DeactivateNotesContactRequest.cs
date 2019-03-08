using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Notes
{
    public class DeactivateNotesContactRequest : ServiceRequestBase
    {
        public int noteId { get; set; }
        public int ContactId { get; set; }
    }

    public class DeactivateNotesContactResponse : ServiceResponseBase
    { }
}
