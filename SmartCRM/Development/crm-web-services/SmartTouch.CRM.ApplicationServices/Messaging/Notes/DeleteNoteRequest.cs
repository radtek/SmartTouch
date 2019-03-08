using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Notes
{
    public class DeleteNoteRequest : ServiceRequestBase
    {
        public int NoteId { get; set; }
        public int ContactId { get; set; }
    }

    public class DeleteNoteResponse : ServiceResponseBase
    {
        public IEnumerable<int> ContactsRelated { get; set; }
    }
}
