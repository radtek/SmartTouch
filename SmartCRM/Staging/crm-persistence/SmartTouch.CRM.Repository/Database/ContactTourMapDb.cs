using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class ContactTourMapDb
    {
        [Key]
        public int ContactTourMapID { get; set; }

        public bool IsCompleted { get; set; }

        //[ForeignKey("Tour")]
        public virtual int TourID { get; set; }
        public virtual TourDb Tour { get; set; }

        //[ForeignKey("Contact")]
        public virtual int ContactID { get; set; }
        public virtual ContactsDb Contact { get; set; }

        public int? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
    }
}
