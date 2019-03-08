using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.DropdownValues
{
    public class GetDropdownValueRequest : ServiceRequestBase
    {
        public int DropdownID { get; set; }
    }

    public class GetDropdownValueResponse : ServiceResponseBase
    {
        public  DropdownViewModel DropdownValues { get; set; }
        public DropdownValueViewModel DropdownValue { get; set; }
    }
}
