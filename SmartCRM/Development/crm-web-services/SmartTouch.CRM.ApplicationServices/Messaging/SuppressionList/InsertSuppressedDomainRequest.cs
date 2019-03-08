using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.SuppressionList
{
    public class InsertSuppressedDomainRequest:ServiceRequestBase
    {
        public IEnumerable<SuppressedDomainViewModel> DomainViewModel { get; set; }
    }

    public class InsertSuppressedDomainResponse : ServiceResponseBase
    {

    }
}
