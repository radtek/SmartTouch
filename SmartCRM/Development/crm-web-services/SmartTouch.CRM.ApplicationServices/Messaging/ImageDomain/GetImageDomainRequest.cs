using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.ImageDomain
{
    public class GetImageDomainRequest : ServiceRequestBase
    {
        public byte ImageDomainId { get; set; }
    }

    public class GetImageDomainResponse : ServiceResponseBase
    {
        public ImageDomainViewModel ImageDomainViewModel { get; set; }
    }
}
