using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow
{
    public class UpdateContactFieldRequest : ServiceRequestBase
    {
        public int FieldID { get; set; }
        public string FieldValue { get; set; }
        public int ContactID { get; set; }
        public int FieldInputTypeID { get; set; }
    }

    public class UpdateContactFieldResponse : ServiceResponseBase
    {

    }
}
