using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Repository.Database
{
    public class NotificationDb
    {
        [Key]
        public int NotificationID { get; set; }
        public int? EntityID { get; set; }

        public string Subject { get; set; }
        public string Details { get; set; }
        public DateTime NotificationTime { get; set; }
        public NotificationStatus Status { get; set; }

        [ForeignKey("Module")]
        public virtual byte ModuleID { get; set; }
        public virtual ModulesDb Module { get; set; }

        [ForeignKey("User")]
        public virtual int? UserID { get; set; }
        public virtual UsersDb User { get; set; }

        public string DownloadFile { get; set; }
    }
}
