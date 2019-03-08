using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class ContactNoteMapDb
    {
        [Key]
        public int ContactNoteMapID { get; set; }

        [ForeignKey("Note")]
        public virtual int NoteID { get; set; }
        public virtual NotesDb Note { get; set; }

        [ForeignKey("Contact")]
        public virtual int ContactID { get; set; }
        public virtual ContactsDb Contact { get; set; }

    }
}
