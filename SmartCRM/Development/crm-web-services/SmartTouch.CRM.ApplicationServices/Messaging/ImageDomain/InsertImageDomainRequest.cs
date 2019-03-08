using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.ImageDomain
{
    public class InsertImageDomainRequest : ServiceRequestBase
    {
        public ImageDomainViewModel ImageDomainViewModel { get; set; }
    }

    public class InsertImageDomainResponse : ServiceRequestBase
    {
        public ImageDomainViewModel ImageDomainViewModel { get; set; }        
    }
}
