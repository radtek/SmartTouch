using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class OpportunitiesTimeLineDb
    {        
        [Key]
        public long? TimelineID { get; set; }
        public int OpportunityID { get; set; }
        public string Module { get; set; }
        public string AuditAction { get; set; }
        public string Value { get; set; }
        public DateTime AuditDate { get; set; }
        public int ModuleId { get; set; }
        public string UserName { get; set; }
        public int? CreatedBy { get; set; }
        public string TimeLineDate { get; set; }
        public string TimeLineTime { get; set; }
        public bool AuditStatus { get; set; }
    }
}
