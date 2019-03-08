using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class NotesDb
    {
        [Key]
        public int NoteID { get; set; }
        public string NoteDetails { get; set; }

        [ForeignKey("Accounts")]
        public virtual int? AccountID { get; set; }
        public virtual AccountsDb Accounts { get; set; }

        public ICollection<ContactsDb> Contacts { get; set; }
        public ICollection<TagsDb> Tags { get; set; }

        //public ICollection<ContactNotesDb> Contacts { get; set; }
        //public ICollection<NoteTagsDb> Tags { get; set; }

        [ForeignKey("User")]
        public virtual int CreatedBy { get; set; }
        public virtual UsersDb User { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool SelectAll { get; set; }
        public bool AddToContactSummary { get; set; }

        public short NoteCategory { get; set; }
    }
}
