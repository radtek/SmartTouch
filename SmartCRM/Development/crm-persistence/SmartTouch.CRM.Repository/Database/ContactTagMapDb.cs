using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace SmartTouch.CRM.Repository.Database
{
    public class ContactTagMapDb
    {
        [Key]
        public int ContactTagMapID { get; set; }

        [ForeignKey("Contact")]
        public virtual int ContactID { get; set; }        
        public virtual ContactsDb Contact { get; set; }

        [ForeignKey("Tag")]
        public virtual int TagID { get; set; }
        public virtual TagsDb Tag { get; set; }

        [ForeignKey("User")]
        public virtual int TaggedBy { get; set; }
        public virtual UsersDb User { get; set; }

        public DateTime TaggedOn { get; set; }

        public int AccountID { get; set; }
    }
}
