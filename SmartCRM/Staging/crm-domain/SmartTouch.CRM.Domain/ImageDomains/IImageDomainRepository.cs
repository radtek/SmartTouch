using SmartTouch.CRM.Domain.ImageDomains;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ImageDomains
{
    public interface IImageDomainRepository : IRepository<ImageDomain, byte>
    {
        IEnumerable<ImageDomain> FindAll(string name, int limit, int pageNumber, bool status);
        IEnumerable<ImageDomain> GetActiveImageDomains();
        IEnumerable<ImageDomain> ImageDomainsList();
        bool IsDuplicateImageDomain(ImageDomain imageDomain);
        ImageDomain GetImageDomain(byte imageDomainId);
        ImageDomain UpdateImageDomain(ImageDomain imageDomain);
        void DeleteImageDomain(byte imageDomainId );
        byte GetImageDomainsCount();
        bool IsConfiguredWithVMTA(byte imageDomainID);
    }
}
