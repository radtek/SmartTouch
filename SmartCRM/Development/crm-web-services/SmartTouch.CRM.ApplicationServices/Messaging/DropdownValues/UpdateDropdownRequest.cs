using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.DropdownValues
{
    public class UpdateDropdownRequest : ServiceRequestBase
    {
        public DropdownViewModel DropdownValues { get; set; }
    }
    public class UpdateDropdownResponse: ServiceResponseBase
    {

    }
}
