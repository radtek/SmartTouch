using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class DailySummaryEmailAuditDb
    {
        [Key]
        public int DailySummaryEmailAuditID { get; set; }
        public int UserID { get; set; }
        public DateTime AuditedOn { get; set; }
        public byte Status { get; set; }
    }
}
