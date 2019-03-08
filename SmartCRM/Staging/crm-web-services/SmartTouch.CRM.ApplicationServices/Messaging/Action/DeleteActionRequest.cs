using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Action
{
    public class DeleteActionRequest : ServiceRequestBase
    {
        public int ActionId { get; set; }
        public bool DeleteForAll { get; set; }
        public int? ContactId { get; set; }
    }

    public class DeleteActionResponse : ServiceResponseBase
    {
    }
}
