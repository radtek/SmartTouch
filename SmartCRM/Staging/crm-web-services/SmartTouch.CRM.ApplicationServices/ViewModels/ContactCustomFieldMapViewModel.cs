using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class ContactCustomFieldMapViewModel
    {
        public int ContactCustomFieldMapId { get; set; }
        public int ContactId { get; set; }
        public int CustomFieldId { get; set; }
        public string Value { get; set; }
        public string Value_Date { get; set; }
        public int FieldInputTypeId { get; set; }
    }
}
