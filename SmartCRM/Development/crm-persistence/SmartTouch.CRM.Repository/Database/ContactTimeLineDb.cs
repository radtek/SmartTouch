using System;
using System.ComponentModel.DataAnnotations;

namespace SmartTouch.CRM.Repository.Database
{
    public class ContactTimeLineDb
    {
        [Key]
        public long? TimelineID { get; set; }
        public int ContactID { get; set; }
        public string Module { get; set; }
        public string AuditAction { get; set; }
        public string Value { get; set; }
        public DateTime? AuditDate { get; set; }        
        public int ModuleId { get; set; }
        public string UserName { get; set; }
        public int? CreatedBy { get; set; }
        public bool AuditStatus { get; set; }
        public bool IsAPIForm { get; set; }
    }
}
