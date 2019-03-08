using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class UpdateContactFieldsRequest : ServiceRequestBase
    {
        public int FieldId { get; set; }
        public string FieldValue { get; set; }
        public int ContactId { get; set; }
    }

    public class UpdateContactFieldsResponse : ServiceResponseBase
    {

    }
}
