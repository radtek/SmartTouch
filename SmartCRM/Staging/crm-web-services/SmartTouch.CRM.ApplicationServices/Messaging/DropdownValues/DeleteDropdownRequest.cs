using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;
namespace SmartTouch.CRM.ApplicationServices.Messaging.DropdownValues
{
    public class DeleteDropdownRequest : ServiceRequestBase
    {
        public DropdownValueViewModel DropdownValue { get; set; }
    }
    public class DeleteDropdownResponse : ServiceResponseBase
    {
        public string Response { get; set; }
    }
}
