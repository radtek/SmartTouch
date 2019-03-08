using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class APILeadSubmissionsDb
    {
        [Key]
        public int APILeadSubmissionID { get; set; }
        public int? ContactID { get; set; }
        public int AccountID { get; set; }
        public string SubmittedData { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public byte IsProcessed { get; set; }
        public string Remarks { get; set; }
        public int? OwnerID { get; set; }
        public int FormID { get; set; }
        public string IPAddress { get; set; }

        public DateTime? FieldUpdatedOn { get; set; }
        [ForeignKey("UpdatedUser")]
        public int? FieldUpdatedBy { get; set; }
        public UsersDb UpdatedUser { get; set; }
    }
}
