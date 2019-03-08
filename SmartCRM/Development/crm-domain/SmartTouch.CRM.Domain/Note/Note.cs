using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.Contact;
using SmartTouch.CRM.Domain.Tag;

namespace SmartTouch.CRM.Domain.Note
{
    public class Note: EntityBase<int>, IAggregateRoot
    {
        string details;
        public string Details { get { return details; } set { details = !string.IsNullOrEmpty(value)?value.Trim():null;  } }
        public virtual IList<Contact.Contact> Contacts { get; set; }
        public virtual ICollection<Tag.Tag> Tags { get; set; }
        public virtual int CreatedBy { get; set; }
        public virtual DateTime CreatedOn { get; set; }

        protected override void Validate()
        {
            if (string.IsNullOrEmpty(Details))
            {
                AddBrokenRule(NoteBusinessRule.NoteDetailsRequired);
            }

            if (Details.Length > 1000)
            {
                AddBrokenRule(NoteBusinessRule.NoteDetailsNotMoreThan1000Characters);
            }

            if (Contacts.Count == 0 || Contacts == null)
            {
                AddBrokenRule(NoteBusinessRule.ContactsRequired);
            }
        }

        
    }
}
