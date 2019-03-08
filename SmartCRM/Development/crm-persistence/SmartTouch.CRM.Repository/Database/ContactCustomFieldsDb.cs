using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class ContactCustomFieldsDb
    {
        [Key]
        public int ContactCustomFieldMapID { get; set; }

        [ForeignKey("Contact")]
        public int ContactID { get; set; }
        public ContactsDb Contact { get; set; }

        [ForeignKey("CustomField")]
        public int CustomFieldID { get; set; }
        public FieldsDb CustomField { get; set; }

        public string Value { get; set; }

        [NotMapped]
        public int FieldInputTypeId { get; set; }
    }
}
