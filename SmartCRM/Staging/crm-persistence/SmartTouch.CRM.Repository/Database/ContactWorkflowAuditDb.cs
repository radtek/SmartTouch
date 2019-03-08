using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class ContactWorkflowAuditDb
    {
        [Key]
        public int ContactWorkflowAuditID { get; set; }
        [ForeignKey("Contacts")]
        public int ContactID { get; set; }

        public ContactsDb Contacts { get; set; }
        [ForeignKey("Workflows")]
        public short WorkflowID { get; set; }
        public WorkflowsDb Workflows { get; set; }

        [ForeignKey("WorkflowActions")]
        public int WorkflowActionID { get; set; }
        public WorkflowActionsDb WorkflowActions { get; set; }

        public string MessageID { get; set; }

        public DateTime ActionPerformedOn { get; set; }
    }
}
