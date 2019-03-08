using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Communication
{
    public class GetServiceProviderSenderEmailRequest : ServiceRequestBase
    {
        public int ServiceProviderID{ get; set; }
    }

    public class GetServiceProviderSenderEmailResponse : ServiceResponseBase
    {
        public Email SenderEmail { get; set; }
        public string SenderName { get; set; }
    }


}
