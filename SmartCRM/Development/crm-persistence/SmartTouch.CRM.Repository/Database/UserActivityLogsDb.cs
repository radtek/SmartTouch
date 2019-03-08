using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class UserActivityLogsDb
    {
        [Key]
        public Int64 UserActivityLogID { get; set; }
        public int EntityID { get; set; }

        [ForeignKey("User")]
        public int? UserID { get; set; }
        public UsersDb User { get; set; }

        [ForeignKey("Module")]
        public byte ModuleID { get; set; }
        public ModulesDb Module { get; set; }

        [ForeignKey("UserActivities")]
        public virtual byte UserActivityID { get; set; }
        public virtual UserActivitiesDb UserActivities { get; set; }

        public DateTime LogDate { get; set; }

        public int? AccountID { get; set; }
        public string EntityName { get; set; }
      
    }

    public class UserActivityListDb {
        public int OwnerId { get; set; }
        public string OwnerName { get; set; }
        public int TotalModuleCount { get; set; }
        public IEnumerable<UserModulesDb> UserModules { get; set; }
    }

    public class UserModulesDb
    {
        public int UserModuleId { get; set; }
        public int ModuleCount { get; set; }
    }

}
