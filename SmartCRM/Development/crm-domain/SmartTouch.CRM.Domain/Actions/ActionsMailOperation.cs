using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Actions
{
    public class ActionsMailOperation
    {
        public int ActionID { get; set; }
        public bool IsScheduled { get; set; }
        public byte IsProcessed { get; set; }
        public int MailBulkOperationID { get; set; }
        public Guid? GroupID { get; set; }
    }
}
