using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace SmartTouch.CRM.Repository.Database
{
    public class ActionsMailOperationDb
    {
        [Key]
        public int ActionsMailOperationID { get; set; }
        public int ActionID { get; set; }
        public bool IsScheduled { get; set; }
        public byte IsProcessed { get; set; }
        public int MailBulkOperationID { get; set; }
        public Guid? GroupID { get; set; }
    }
}
