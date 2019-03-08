using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Communication
{
    public class GetServiceProviderRequest : ServiceRequestBase
    {
        public CommunicationType CommunicationTypeId { get; set; }
        public int CommunicationLogInDetailId { get; set; }
        public MailType MailType { get; set; }
    }

    public class GetServiceProviderResponse : ServiceResponseBase
    {
        public IEnumerable<ServiceProviderViewModel> ServiceProviderViewModel { get; set; }
    }
}
