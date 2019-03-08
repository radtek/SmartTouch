using SmartTouch.CRM.ApplicationServices.Messaging.ImageDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface IImageDomainService
    {
        GetImageDomainsResponse GetImageDomains(GetImageDomainsRequest request);
        GetImageDomainsResponse GetActiveImageDomains(GetImageDomainsRequest request);
        InsertImageDomainResponse InsertImageDomain(InsertImageDomainRequest request);
        UpdateImageDomainResponse UpdateImageDomain(UpdateImageDomainRequest request);
        GetImageDomainResponse GetImageDomain(GetImageDomainRequest request);
        DeleteImageDomainResponse DeleteImageDomain(DeleteImageDomainRequest request);
    }
}
