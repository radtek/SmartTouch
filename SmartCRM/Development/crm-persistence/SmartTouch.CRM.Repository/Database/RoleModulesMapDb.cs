using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class RoleModulesMapDb
    {
        [Key]
        public int RoleModuleMapID { get; set; }

        public virtual short RoleID { get; set; }
        [ForeignKey("RoleID")]
        public virtual RolesDb Role { get; set; }

        public virtual byte ModuleID { get; set; }
        [ForeignKey("ModuleID")]
        public virtual ModulesDb Module { get; set; }
    }
}
