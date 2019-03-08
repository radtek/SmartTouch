using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
   public class CommunicationTrackerDb
    {
       [Key]
       public long CommunicationTrackerID { get; set; }
       public bool? Address { get; set; }
       public System.Guid TrackerGuid { get; set; }
       public System.DateTime CreatedDate { get; set; }
       public CommunicationStatus CommunicationStatusID { get; set; }
       public CommunicationType CommunicationTypeID { get; set; }
        [ForeignKey("Contacts")]
       public int? ContactID { get; set; }
        public virtual ContactsDb Contacts { get; set; }

    }
}
