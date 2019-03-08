using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Contacts
{
    public class ContactNoteMap: ValueObjectBase
    {
        public int NoteID { get; set; }
        public int ContactID { get; set; }
        public string NoteDetails { get; set; }
        public short NoteCategory { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }


        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
