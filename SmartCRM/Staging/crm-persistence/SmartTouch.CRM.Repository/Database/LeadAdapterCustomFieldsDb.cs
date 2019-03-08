using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class LeadAdapterCustomFieldsDb
    {
        [Key]
        public short LeadAdapterCustomFieldID { get; set; }
        public string Title { get; set; }
        [ForeignKey("FieldInputTypes")]
        public byte FieldInputTypeID { get; set; }
        public FieldInputTypesDb FieldInputTypes { get; set; }
        public byte SortID { get; set; }
        [ForeignKey("LeadAdapterTypes")]
        public byte LeadAdapterType { get; set; }
        public LeadAdapterTypesDb LeadAdapterTypes { get; set; }
    }
}
