using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Repository.Database
{
    public class UserProfileAudit
    {
        public int UserProfileAuditID { get; set; }
        public virtual int UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual UsersDb User { get; set; }

        public byte UserAuditTypeID { get; set; }
        public DateTime AuditedOn { get; set; }
        public string Password { get; set; }
        public virtual int AuditedBy { get; set; }
        [ForeignKey("AuditedBy")]
        public virtual UsersDb AuditedUser { get; set; }
    }
}
