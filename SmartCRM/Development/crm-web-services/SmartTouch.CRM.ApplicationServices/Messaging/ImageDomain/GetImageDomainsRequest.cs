using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.ImageDomain
{
    public class GetImageDomainsRequest : ServiceRequestBase
    {
        public int Limit { get; set; }
        public string Query { get; set; }
        public int PageNumber { get; set; }
        public bool Status { get; set; }
    }

    public class GetImageDomainsResponse : ServiceResponseBase
    {
        public int TotalHits { get; set; }
        public IEnumerable<ImageDomainViewModel> ImageDomains { get; set; }
    }
}
