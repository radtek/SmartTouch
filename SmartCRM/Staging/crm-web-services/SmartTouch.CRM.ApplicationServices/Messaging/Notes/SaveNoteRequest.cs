using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Notes
{
    public class SaveNoteRequest : ServiceRequestBase
    {
        public NoteViewModel NoteViewModel { get; set; }     
    }

    public class SaveNoteResponse : ServiceResponseBase
    {
        public virtual NoteViewModel NoteViewModel { get; set; }
    }
}
