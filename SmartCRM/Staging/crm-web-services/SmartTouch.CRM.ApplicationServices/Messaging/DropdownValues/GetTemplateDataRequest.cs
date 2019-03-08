using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.DropdownValues
{
    public class GetTemplateDataRequest:ServiceRequestBase
    {
        public string FileName { get; set; }
    }

    public class GetTemplateDataResponse:ServiceResponseBase
    {
        public string FileCode { get; set; }
    }
}
