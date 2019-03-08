using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class OperationsDb
    {
        [Key]
        public short OperationID { get; set; }
        public string OperationName {get; set;}

        //public virtual ICollection<ModulesDb> Modules { get; set; }
    }
}
