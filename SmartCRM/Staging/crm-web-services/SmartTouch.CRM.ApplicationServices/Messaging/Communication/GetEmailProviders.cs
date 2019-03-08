using SmartTouch.CRM.Domain.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Communication
{
    public class GetEmailProvidersRequest : ServiceRequestBase
    {

    }

    public class GetEmailProvidersResponse : ServiceResponseBase
    {
        public IEnumerable<Guid> ServiceProviderGuids { get; set; }
    }
}
