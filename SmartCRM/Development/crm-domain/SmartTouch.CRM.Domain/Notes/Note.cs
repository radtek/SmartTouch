using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.Users;

namespace SmartTouch.CRM.Domain.Notes
{
    public class Note: EntityBase<int>, IAggregateRoot
    {
        string details;
        public string Details { get { return details; } set { details = !string.IsNullOrEmpty(value)?value.Trim():null;  } }
        public int? AccountId { get; set; }
        public virtual IList<Contacts.Contact> Contacts { get; set; }
        public virtual ICollection<Tags.Tag> Tags { get; set; }
        public virtual int OppurtunityId { get; set; }
        public virtual int CreatedBy { get; set; }
        public virtual short NoteCategory { get; set; }
        public virtual DateTime CreatedOn { get; set; }
        public virtual User User { get; set; }
        public bool SelectAll { get; set; }
        public bool AddToContactSummary { get; set; }

        protected override void Validate()
        {
            if (string.IsNullOrEmpty(Details))
            {
                AddBrokenRule(NoteBusinessRule.NoteDetailsRequired);
            }

            if (Details != null && Details.Length > 4000)
            {
                AddBrokenRule(NoteBusinessRule.NoteDetailsNotMoreThan1000Characters);
            }

            if ( Contacts == null || Contacts.Count == 0)
            {
                AddBrokenRule(NoteBusinessRule.ContactsRequired);
            }
        }

        
    }
}
