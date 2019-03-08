using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class SectionsDB
    {
        [Key]
        public Int16 SectionID { get; set; }
        public string SectionName { get; set; }
    }
}
