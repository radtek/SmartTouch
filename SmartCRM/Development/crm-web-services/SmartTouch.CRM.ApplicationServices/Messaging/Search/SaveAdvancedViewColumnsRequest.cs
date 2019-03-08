using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Search
{
    public class SaveAdvancedViewColumnsRequest : ServiceRequestBase
    {
        //public int entityId { get; set; }
        //public byte entityType  {get; set;}
        //public IEnumerable<int> fields  {get; set;}
        //public byte showingType {get; set;}
        public AVColumnPreferenceViewModel model { get; set; }
    }

    public class SaveAdvancedViewColumnsResponse : ServiceResponseBase
    {

    }
}
