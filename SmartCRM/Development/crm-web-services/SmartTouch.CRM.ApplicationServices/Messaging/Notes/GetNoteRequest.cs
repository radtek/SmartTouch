using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Notes
{
    public class GetNoteRequest : ServiceRequestBase
    {
        public int NoteId { get; set; }
        public int ContactId { get; set; }
    }

    public class GetNoteResponse : ServiceResponseBase
    {
        public NoteViewModel NoteViewModel { get; set; }
    }
}
