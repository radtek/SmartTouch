using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class ScoreCategoriesDb
    {
        [Key]
        public Int16 ScoreCategoryID { get; set; }
        public string Name { get; set; }
        public byte? ModuleID { get; set; }
    }
}
