using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class WorkflowUserAssignmentAuditDb
    {
        [Key]
        public int WorkflowUserAssignmentAuditID { get; set; }
        public int ContactID { get; set; }
        public int UserID { get; set; }
        public int WorkflowUserAssignmentActionID { get; set; }
        public byte DayOfWeek { get; set; }
    }
}
