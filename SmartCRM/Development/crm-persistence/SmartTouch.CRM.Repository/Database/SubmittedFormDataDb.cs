using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
   public class SubmittedFormDataDb
    {
        [Key]
        public int SubmittedFormDataID { get; set; }

        public int FormID { get; set; }
        public int AccountID { get; set; }
        public string IPAddress { get; set; }
        public DateTime CreatedOn { get; set; }
        public int Status { get; set; }
        public Int16 LeadSourceID { get; set; }
        public string STITrackingID { get; set; }
        public int? CreatedBy { get; set; }
        public int? OwnerID { get; set; }
        public string Remarks { get; set; }

        [ForeignKey("FormSubmissionID")]
        public FormSubmissionDb FormSubmission { get; set; }
        public int? FormSubmissionID { get; set; }

        public DateTime? FieldUpdatedOn { get; set; }
        [ForeignKey("User")]
        public int? FieldUpdatedBy { get; set; }
        public UsersDb User { get; set; }
    }
}
