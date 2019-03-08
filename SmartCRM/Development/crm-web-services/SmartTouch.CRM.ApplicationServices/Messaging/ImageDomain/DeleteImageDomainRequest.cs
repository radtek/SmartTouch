using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.ImageDomain
{
    public class DeleteImageDomainRequest : ServiceRequestBase
    {
        public byte ImageDomainId { get; set; }
    }

    public class DeleteImageDomainResponse : ServiceResponseBase
    { 
    }
}
