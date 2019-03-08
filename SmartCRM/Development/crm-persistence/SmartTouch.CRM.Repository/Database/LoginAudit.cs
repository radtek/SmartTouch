using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Repository.Database
{
    public class LoginAudit
    {
        public int LoginAuditID { get; set; }
        public virtual int UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual UsersDb User { get; set; }

        public virtual int AccountID { get; set; }
        [ForeignKey("AccountID")]
        public virtual AccountsDb Accounts { get; set; }

        public string IPAddress { get; set; }
        public byte SignInActivity { get; set; }
        public DateTime AuditedOn { get; set; }
    }
}
