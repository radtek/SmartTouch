using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class UpdateContactCustomFieldRequest : ServiceRequestBase
    {
        public int ContactId { get; set; }
        public int FieldId { get; set; }
        public string Value { get; set; }
        public short inputType { get; set; }
        public string DateFormat { get; set; }
    }

    public class UpdateContactCustomFieldResponse: ServiceResponseBase
    {
        public int Result { get; set; }
    }
}
