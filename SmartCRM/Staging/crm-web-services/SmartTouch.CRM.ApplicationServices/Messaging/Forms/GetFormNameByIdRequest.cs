using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Forms
{
    public class GetFormNameByIdRequest: ServiceRequestBase
    {
        public int FormId { get; set; }
    }

    public class GetFormNameByIdResponse : ServiceResponseBase
    {
        public string FormName { get; set; }
    }
}
