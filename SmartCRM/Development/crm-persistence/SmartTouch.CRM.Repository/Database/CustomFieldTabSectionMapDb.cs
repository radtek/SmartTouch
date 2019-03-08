using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class CustomFieldTabSectionMapDb
    {
        [Key]
        public int TabSectionMapID { get; set; }

        [ForeignKey("CustomFieldSection")]
        public int CustomFieldTabID { get; set; }
        public CustomFieldSectionDb CustomFieldSection { get; set; }

        [ForeignKey("CustomFieldTab")]
        public int CustomFieldSectionID { get; set; }
        public CustomFieldTabDb CustomFieldTab { get; set; }
    }
}
