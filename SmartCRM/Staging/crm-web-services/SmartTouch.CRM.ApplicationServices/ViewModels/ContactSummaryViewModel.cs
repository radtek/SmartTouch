using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class ContactSummaryViewModel
    {
        public string NoteDetails { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string NoteCategory { get; set; }
        public int NoteID { get; set; }
        public string SummaryDetails { get; set; }
    }
}
