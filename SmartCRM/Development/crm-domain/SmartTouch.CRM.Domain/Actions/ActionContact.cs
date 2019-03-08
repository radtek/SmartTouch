using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Domain.Actions
{
    public class ActionContact
    {
        public int ContactId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public ContactType ContactType { get; set; }
        public string Email { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? RemindOn { get; set; }
        public string ActionDetails { get; set; }
        public int ActionId { get; set; }

        public DateTime? LastUpdatedOn { get; set; }
        public int? LastUpdatedBy { get; set; }
    }
}
