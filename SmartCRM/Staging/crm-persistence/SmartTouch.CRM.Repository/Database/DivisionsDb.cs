using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class DivisionsDb
    {
        [Key]
        public Int16 DivisionID { get; set; }
        public string DivisionName { get; set; }
    }
}
