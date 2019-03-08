using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Repository.Database
{
    public class CustomFieldTabDb
    {
        [Key]
        public int CustomFieldTabID { get; set; }
        public string Name { get; set; }
        [ForeignKey("Account")]
        public int AccountID { get; set; }
        public AccountsDb Account { get; set; }
        [ForeignKey("Status")]
        public short StatusID { get; set; }
        public StatusesDb Status { get; set; }
        public byte SortID { get; set; }
        public bool IsLeadAdapterTab { get; set; }
        public ICollection<CustomFieldSectionDb> CustomFieldSections { get; set; }        
    }

    public class CustomFieldSectionDb
    {
        [Key]
        public int CustomFieldSectionID { get; set; }
        public string Name { get; set; }

        [ForeignKey("CustomFieldTab")]
        public int TabID { get; set; }
        public virtual CustomFieldTabDb CustomFieldTab { get; set; }
        
        [ForeignKey("Status")]
        public short StatusID { get; set; }
        public StatusesDb Status { get; set; }

        public byte SortID { get; set; }
        
        public ICollection<FieldsDb> CustomFields { get; set; }
    }

    public class CustomFieldDb
    {
        [Key]
        public int CustomFieldID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public FieldType FieldInputTypeID { get; set; }

        [ForeignKey("CustomFieldSection")]
        public int CustomFieldSectionID { get; set; }
        public virtual CustomFieldSectionDb CustomFieldSection { get; set; }


        public byte SortID { get; set; }

        [ForeignKey("Account")]
        public int AccountID { get; set; }
        public AccountsDb Account { get; set; }

        [ForeignKey("Status")]
        public short StatusID { get; set; }
        public StatusesDb Status { get; set; }

        public ICollection<CustomFieldValueOptionsDb> CustomFieldValueOptions { get; set; }
    }

    public class CustomFieldValueOptionsDb
    {
        [Key]
        public int CustomFieldValueOptionID { get; set; }
        [ForeignKey("Field")]
        public int CustomFieldID { get; set; }
        public virtual FieldsDb Field { get; set; }
        public string Value { get; set; }
        public bool IsDeleted { get; set; }
        public int? Order { get; set; }
    }
}
