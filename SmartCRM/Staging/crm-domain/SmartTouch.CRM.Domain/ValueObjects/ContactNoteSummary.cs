using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class ContactNoteSummary 
    {
        public int ContactId { get; set; }
        public string NoteDetails { get; set; }
        public DateTime? LastNoteDate { get; set; }
        public string LastNote { get; set; }
        public short LastNoteCategory { get; set; }
    }

    public class ContactLastNote
    {
        public int ContactId { get; set; }
        public string LastNote { get; set; }
    }
}
