using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Forms
{
    public class UpdateFormNameRequest : ServiceRequestBase
    {
        public int FormId { get; set; }
        public string FormName { get; set; }
    }

    public class UpdateFormNameResponse : ServiceResponseBase
    {

    }
}
