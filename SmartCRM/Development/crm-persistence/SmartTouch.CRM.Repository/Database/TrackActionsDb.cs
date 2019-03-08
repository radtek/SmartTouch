using SmartTouch.CRM.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class TrackActionsDb
    {
        [Key]
        public long TrackActionID { get; set; }
        public long TrackMessageID { get; set; }
        public int WorkflowID { get; set; }
        public int ActionID { get; set; }
        public byte WorkflowActionTypeID { get; set; }
        public DateTime ScheduledOn { get; set; }
        public DateTime? ExecutedOn { get; set; }

        public DateTime CreatedOn { get; set; }

        public TrackActionProcessStatus ActionProcessStatusID { get; set; }

        [ForeignKey("ActionID")]
        public WorkflowActionsDb WorkflowAction { get; set; }
        [ForeignKey("TrackMessageID")]
        public TrackMessagesDb TrackMessage { get; set; }
    }
}
