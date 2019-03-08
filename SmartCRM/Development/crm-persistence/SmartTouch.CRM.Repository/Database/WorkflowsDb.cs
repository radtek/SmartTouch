using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class WorkflowsDb
    {
        [Key]
        public short WorkflowID { get; set; }
        public string WorkflowName { get; set; }

        [ForeignKey("Accounts")]
        public int AccountID { get; set; }
        public virtual AccountsDb Accounts { get; set; }

        [ForeignKey("Statusses")]
        public short Status { get; set; }
        public virtual StatusesDb Statusses { get; set; }

        public DateTime? DeactivatedOn { get; set; }
        public bool? IsWorkflowAllowedMoreThanOnce { get; set; }
        public byte? AllowParallelWorkflows { get; set; }
        public string RemovedWorkflows { get; set; }

        [ForeignKey("Users")]
        public int CreatedBy { get; set; }
        public virtual UsersDb Users { get; set; }

        public DateTime CreatedOn { get; set; }

        [ForeignKey("ModifiedByUser")]
        public int? ModifiedBy { get; set; }
        public virtual UsersDb ModifiedByUser { get; set; }

        public DateTime? ModifiedOn { get; set; }
        public bool IsDeleted { get; set; }

        public int ParentWorkflowID { get; set; }

        public ICollection<WorkflowTriggersDb> Triggers { get; set; }
        public ICollection<WorkflowActionsDb> WorkflowActions { get; set; }

        [NotMapped]
        public int ContactsStarted { get; set; }
        [NotMapped]
        public int ContactsInProgress { get; set; }
        [NotMapped]
        public int ContactsFinished { get; set; }
        [NotMapped]
        public int ContactsOptedOut { get; set; }
        [NotMapped]
        public int TotalWorkflowCount { get; set; }
    }
}
