using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.ImageDomain
{
    public class UpdateImageDomainRequest : ServiceRequestBase
    {
        public ImageDomainViewModel ImageDomainViewModel { get; set; }
    }

    public class UpdateImageDomainResponse : ServiceResponseBase
    {
        public ImageDomainViewModel ImageDomainViewModel { get; set; }
    }
}
