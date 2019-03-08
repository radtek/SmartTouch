using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class SearchFiltersDb
    {
        [Key]
        public short SearchFilterID { get; set; }

        [ForeignKey("Fields")]
        public int? FieldID { get; set; }
        public virtual FieldsDb Fields { get; set; }

        [ForeignKey("SearchQualifierTypes")]
        public short SearchQualifierTypeID { get; set; }
        public virtual SearchQualifierTypesDb SearchQualifierTypes { get; set; }

        [ForeignKey("SearchDefinitions")]
        public int SearchDefinitionID { get; set; }
        public virtual SearchDefinitionsDb SearchDefinitions { get; set; }

        [ForeignKey("DropdownValue")]
        public short? DropdownValueID { get; set; }
        public virtual DropdownValueDb DropdownValue { get; set; }

        public string SearchText { get; set; }

        [NotMapped]
        public bool IsCustomField { get; set; }
        [NotMapped]
        public bool IsDropdownField { get; set; }
        [NotMapped]
        public bool IsDateTime { get; set; }
        [NotMapped]
        public int? DropdownId { get; set; }  //This Field is required only if this filter is dropdownfield
    }
}
