using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Action
{
    public class UpdateActionRequest : ServiceRequestBase
    {
        public ActionViewModel ActionViewModel { get; set; }
        public string AccountPrimaryEmail { get; set; }
        public string AccountAddress { get; set; }
        public string Location { get; set; }
        public string AccountPhoneNumber { get; set; }
        public string AccountDomain { get; set; }
    }

    public class UpdateActionResponse : ServiceResponseBase
    {
        public virtual ActionViewModel ActionViewModel { get; set; }
    }
}
