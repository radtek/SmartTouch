using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Workflows
{
    public class WorkflowNotifyUserAction : WorkflowAction
    {
        [Key]
        public int WorkflowNotifyUserActionID { get; set; }
        public IEnumerable<int> UserID { get; set; }
        public IEnumerable<string> UserNames { get; set; }
        public byte NotifyType { get; set; }
        public string MessageBody { get; set; }
        public string NotificationFields { get; set; }
        public IEnumerable<int> NotificationFieldID { get; set; }

        protected override void Validate()
        {
            base.Validate();
            if (NotifyType == 0)
                AddBrokenRule(WorkflowBusinessRules.NotifybyRequired);
            if (string.IsNullOrEmpty(MessageBody))
                AddBrokenRule(WorkflowBusinessRules.MessageRequried);
            if (UserID != null && !UserID.Any())
                AddBrokenRule(WorkflowBusinessRules.UserRequired);
            if((NotifyType == 2 || NotifyType == 3) && MessageBody.Length > 160)
                AddBrokenRule(WorkflowBusinessRules.MaxLength160);
            if ((NotifyType == 1) && MessageBody.Length > 512)
                AddBrokenRule(WorkflowBusinessRules.MaxLength512);
        }
    }
}
