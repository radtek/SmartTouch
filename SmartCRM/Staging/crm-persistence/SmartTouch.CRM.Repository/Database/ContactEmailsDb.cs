using SmartTouch.CRM.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class ContactEmailsDb
    {
        [Key]
        public int ContactEmailID { get; set; }

        [ForeignKey("Statuses")]
        public short EmailStatus { get; set; }
        public StatusesDb Statuses { get; set; }

        public string Email { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? SnoozeUntil { get; set; }
      
        [ForeignKey("Contact")]
        public virtual int ContactID { get; set; }
        public virtual ContactsDb Contact { get; set; }

        [ForeignKey("Account")]
        public virtual int AccountID { get; set; }
        public virtual AccountsDb Account { get; set; }
    }
}
