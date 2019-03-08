using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Search
{
    public class GetAdvanceSearchFieldsRequest : ServiceRequestBase
    {
        public int accountId { get; set; }
    }

    public class GetAdvanceSearchFieldsResponse : ServiceResponseBase
    {
        public IEnumerable<FieldViewModel> FieldsViewModel { get; set; }
    }
}
