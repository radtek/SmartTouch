using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.Contact;
using SmartTouch.CRM.Domain.Tag;

namespace SmartTouch.CRM.Domain.Action
{
    public class Action : EntityBase<int>, IAggregateRoot
    {
        string details;
        public string Details { get { return details; } set { details = !string.IsNullOrEmpty(value)?value.Trim():null;  } }
        public ReminderType ReminderType { get; set; }
        public DateTime RemindOn { get; set; }
        public IList<Contact.Contact> Contacts { get; set; }
        public IEnumerable<Tag.Tag> Tags { get; set; }
        public bool? IsCompleted { get; set; }

        protected override void Validate()
        {
            if (string.IsNullOrEmpty(Details))
            {
                AddBrokenRule(ActionBusinessRule.ActionMessageRequired);
            }

            if (Details != null && Details.Length > 1000)
            {
                AddBrokenRule(ActionBusinessRule.ActionMessageLengthNotExceed1000);
            }

            if (Contacts == null || Contacts.Count == 0)
            {
                AddBrokenRule(ActionBusinessRule.ContactsRequired);
            }

            if (RemindOn != null)
            {
                DateTime outResult;
                if (!DateTime.TryParse(RemindOn.ToString(), out outResult))
                {
                    AddBrokenRule(ActionBusinessRule.RemindOnInvalid);
                }
            }
            DateTime today = DateTime.Now.ToUniversalTime();
            DateTime reminderDate = RemindOn.ToUniversalTime();
            var result = DateTime.Compare(reminderDate,today);
            if (result < 0)
            {
                AddBrokenRule(ActionBusinessRule.RemindOndateInvalid);
            }
        }
    }
}
