using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Workflows
{
    public class TrackAction : EntityBase<long>, IAggregateRoot
    {
        public long TrackActionID { get; set; }
        public long TrackMessageID { get; set; }
        public int WorkflowID { get; set; }
        public int ActionID { get; set; }
        public byte WorkflowActionTypeID { get; set; }
        public DateTime ScheduledOn { get; set; }
        public DateTime? ExecutedOn { get; set; }

        public DateTime CreatedOn { get; set; }

        public TrackActionProcessStatus ActionProcessStatusID { get; set; }
        public Workflow Workflow { get; set; }
        public WorkflowAction WorkflowAction { get; set; }
        public TrackMessage TrackMessage { get; set; }        
        protected override void Validate()
        {
            //for future use
        }

        public override string ToString()
        {
            return string.Format("{0} - {1} - {2}", TrackActionID, ExecutedOn, ActionProcessStatusID);
        }
    }
}
