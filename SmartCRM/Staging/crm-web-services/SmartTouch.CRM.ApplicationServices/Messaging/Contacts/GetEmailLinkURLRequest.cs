using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetEmailLinkURLRequest:ServiceRequestBase
    {
        public int LinkId { get; set; }
    }
    public class GetEmailLinkURLResponse : ServiceResponseBase
    {
        public EmailLinkViewModel EmailLinkViewModel { get; set; }
    }
}
