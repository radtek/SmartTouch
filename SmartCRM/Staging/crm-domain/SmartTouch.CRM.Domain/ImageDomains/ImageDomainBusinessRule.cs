using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ImageDomains
{
    public static class ImageDomainBusinessRule
    {
        public static readonly BusinessRule InvalidImageDomain = new BusinessRule("[|Image domain is invalid|]");
        public static readonly BusinessRule EmptyImageDomain = new BusinessRule("[|Image domain cannot be blank|]");

    }
}
