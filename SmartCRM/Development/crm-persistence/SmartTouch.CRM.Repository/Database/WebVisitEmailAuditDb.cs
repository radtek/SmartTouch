using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class WebVisitEmailAuditDb
    {
        [Key]
        public int AuditID { get; set; }
        public string Recipients { get; set; }
        public WebVisitEmailStatus EmailStatus { get; set; }
        public DateTime?   SentDate { get; set; }
        public string VisitReference { get; set; }
        public DateTime CreatedOn { get; set; }
        public string JobID { get; set; }
        public string Remarks { get; set; }
    }
}
