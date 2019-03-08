using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class FormFieldsDb
    {
        [Key]        
        public int FormFieldID { get; set; }

        [ForeignKey("Form")]
        public int FormID { get; set; }
        public virtual FormsDb Form { get; set; }

        [ForeignKey("Field")]
        public int FieldID { get; set; }
        public virtual FieldsDb Field { get; set; }

        public bool? IsDeleted { get; set; }

        public string DisplayName { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsHidden { get; set; }
        public byte SortID { get; set; }

    }
}
