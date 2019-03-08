using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Notes
{
    public class GetNotesListRequest : ServiceRequestBase
    {
        public int Id { get; set; }
    }

    public class GetNotesListResponse : ServiceResponseBase
    {
        public IEnumerable<NoteViewModel> NotesListViewModel { get; set; }
    }
}
