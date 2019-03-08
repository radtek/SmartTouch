using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class FieldsDb
    {
        [Key]
        public int FieldID { get; set; }
        public string Title { get; set; }
        public string FieldCode { get; set; }
        [ForeignKey("FieldInputTypes")]
        public byte FieldInputTypeID { get; set; }
        public FieldInputTypesDb FieldInputTypes { get; set; }

        public string ValidationMessage { get; set; }
        public int? ParentID { get; set; }
        public byte? SortID { get; set; }
        [ForeignKey("CustomFieldSection")]
        public int? CustomFieldSectionID { get; set; }
        [ForeignKey("Account")]
        public int? AccountID { get; set; }
        public AccountsDb Account { get; set; }

        [ForeignKey("Status")]
        public short? StatusID { get; set; }
        public StatusesDb Status { get; set; }
        public CustomFieldSectionDb CustomFieldSection { get; set; }

        public bool IsLeadAdapterField { get; set; }
        public byte? LeadAdapterType { get; set; }

        public ICollection<CustomFieldValueOptionsDb> CustomFieldValueOptions { get; set; }
    }
}
