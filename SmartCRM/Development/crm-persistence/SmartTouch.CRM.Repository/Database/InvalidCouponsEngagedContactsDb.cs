using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SmartTouch.CRM.Repository.Database
{
    public class InvalidCouponsEngagedContactsDb
    {
        [Key]
        public int InvalidCoupanId { get; set; }
        public int FormSubmissionID { get; set; }
        public int ContactID { get; set; }
        public DateTime LastUpdatedDate { get; set; }

    }
}
