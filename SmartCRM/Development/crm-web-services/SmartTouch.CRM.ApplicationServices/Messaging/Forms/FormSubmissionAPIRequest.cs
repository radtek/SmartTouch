using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Forms
{
    public class FormSubmissionAPIRequest : ServiceRequestBase
    {
        public IList<Dictionary<int,string>> SubmittedValues { get; set; }
    }

    public class FormSubmissionAPIResponse : ServiceResponseBase
    {
        public string Acknowledgement { get; set; }
        public int FormSubmissionId { get; set; }
    }
}
