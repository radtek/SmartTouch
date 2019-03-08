using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class APILeadSubmissionsDb
    {
        [Key]
        public int APILeadSubmissionID { get; set; }
        public virtual int ContactID { get; set; }
        public virtual ContactsDb Contact { get; set; }
        public virtual int AccountID { get; set; }
        public virtual AccountsDb Account { get; set; }
        public string SubmittedData { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public byte IsProcessed { get; set; }
        public string Remarks { get; set; }
        public virtual int OwnerID { get; set; }
        public virtual UsersDb User { get; set; }
        public int FormID { get; set; }
        public string IPAddress { get; set; }
    }
}
