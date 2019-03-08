using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class ConditionDb
    {
        [Key]
        public byte ConditionID { get; set; }
        public string Name { get; set; }

        [ForeignKey("ScoreCategory")]
        public virtual Int16 ScoreCategoryID { get; set; }
        public virtual ScoreCategoriesDb ScoreCategory { get; set; }
        [ForeignKey("Modules")]
        public byte? ModuleID { get; set; }
        public ModulesDb Modules { get; set; }       
    }
}
