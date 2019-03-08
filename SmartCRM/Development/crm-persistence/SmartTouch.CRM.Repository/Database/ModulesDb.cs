using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class ModulesDb
    {
        [Key]
        public byte ModuleID { get; set; }
        public string ModuleName { get; set; }
        public bool IsInternal { get; set; }
        public byte ParentID { get; set; }

        public virtual ICollection<SubscriptionModuleMapDb> SubscriptionModules { get; set; }

        [NotMapped]
        public int? UserLimit { get; set; }
        [NotMapped]
        public string ExcludedRoles { get; set; }
    }
}
