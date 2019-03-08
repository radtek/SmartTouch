using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class ImportColumnMappingsDb
    {
        [Key]
        public int ImportColumnMappingID { get; set; }
        public string SheetColumnName { get; set; }
        public bool IsCustomField { get; set; }
        public bool IsDropDownField { get; set; }
        public string ContactFieldName { get; set; }
        public int JobID { get; set; }
    }
}
