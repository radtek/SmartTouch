using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class ChangeOwnerRequest:ServiceRequestBase
    {
        public ChangeOwnerViewModel ChangeOwnerViewModel { get; set; }
        public byte ModuleId { get; set; }
    }
    public class ChangeOwnerResponce:ServiceResponseBase
    {

    }
}
