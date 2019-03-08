using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class InsertPersonRequest : ServiceRequestBase
    {
        public PersonViewModel PersonViewModel { get; set; }
        public byte ModuleId { get; set; }
        public bool isStAdmin { get; set; }
    }

    public class InsertPersonResponse : ServiceResponseBase
    {
        public virtual PersonViewModel PersonViewModel { get; set; }
    }
}
