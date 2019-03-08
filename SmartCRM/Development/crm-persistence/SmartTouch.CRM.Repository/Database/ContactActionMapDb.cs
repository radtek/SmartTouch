using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Repository.Database
{
    public class ContactActionMapDb
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ContactActionMapID { get; set; }

        public bool? IsCompleted { get; set; }
        public Guid? GroupID { get; set; }

        [ForeignKey("Action")]
        public int ActionID { get; set; }
        public virtual ActionsDb Action { get; set; }

        [ForeignKey("Contact")]
        public int ContactID { get; set; }
        public virtual ContactsDb Contact { get; set; }

        public int LastUpdatedBy { get; set; }
        public DateTime LastUpdatedOn { get; set; }
    }
}
