using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Search
{
    public class GetAdvancedViewColumnsRequest : ServiceRequestBase
    {
        public int EntityId { get; set; }
        public byte EntityType { get; set; }
    }

    public class GetAdvancedViewColumnsResponse : ServiceResponseBase
    {
        public IEnumerable<AVColumnPreferenceViewModel> ColumnPreferenceViewModel { get; set; }
    }
}
