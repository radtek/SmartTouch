using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace SmartTouch.CRM.Domain.Workflows
{
    public class NotificationContactFieldData
    {
        public int FieldID { get; set; }
        [DisplayName("Field Name")]
        public string FieldName { get; set; }
        [DisplayName("Value")]
        public string Value { get; set; }
    }
}
