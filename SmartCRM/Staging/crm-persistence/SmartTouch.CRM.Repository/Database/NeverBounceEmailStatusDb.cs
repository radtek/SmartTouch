using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class NeverBounceEmailStatusDb
    {
        [Key]
        public int NeverBounceEmailStatusID { get; set; }
        public int ContactID { get; set; }
        public int ContactEmailID { get; set; }
        public short EmailStatus { get; set; }
        public int NeverBounceRequestID { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
