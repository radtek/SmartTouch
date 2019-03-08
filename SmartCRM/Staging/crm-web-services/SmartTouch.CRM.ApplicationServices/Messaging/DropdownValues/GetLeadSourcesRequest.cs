using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.DropdownValues
{
    public class GetLeadSourcesRequest : ServiceRequestBase
    {
        public int DropdownId { get; set; }
    }

    public class GetLeadSourcesResponse : ServiceResponseBase
    {
        public IEnumerable<DropdownValueViewModel> LeadSources { get; set; }
    }
}
