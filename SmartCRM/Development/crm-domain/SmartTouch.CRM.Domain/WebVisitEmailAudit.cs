using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain
{
    public class WebVisitEmailAudit
    {
        public int AuditID { get; set; }
        public string Recipients { get; set; }
        public WebVisitEmailStatus EmailStatus { get; set; }
        public DateTime? SentDate { get; set; }
        public string VisitReference { get; set; }
        public DateTime CreatedOn { get; set; }
        public string JobID { get; set; }
        public string Remarks { get; set; }
    }
}
