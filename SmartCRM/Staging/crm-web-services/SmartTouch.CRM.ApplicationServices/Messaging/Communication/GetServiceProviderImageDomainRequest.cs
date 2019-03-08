using SmartTouch.CRM.Domain.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Communication
{
    public class GetServiceProviderImageDomainRequest : ServiceRequestBase
    {
        public int ServiceProviderID { get; set; }
        public Guid Guid { get; set; }
    }

    public class GetServiceProviderImageDomainResponse : ServiceResponseBase
    {
        public ServiceProvider ServiceProvider { get; set; }
        
    }

}
