using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging
{
    public class ReIndexDocumentRequest : ServiceRequestBase
    {
    }

    public class ReIndexDocumentResponse : ServiceResponseBase
    {
        public int Documents { get; set; }
    }
}
