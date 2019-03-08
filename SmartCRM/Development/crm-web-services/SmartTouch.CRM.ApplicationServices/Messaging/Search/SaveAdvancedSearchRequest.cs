using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Search
{
    public class SaveAdvancedSearchRequest:ServiceRequestBase
    {
        public AdvancedSearchViewModel AdvancedSearchViewModel { get; set; }
    }

    public class SaveAdvancedSearchResponse : ServiceResponseBase
    {
        public int SearchDefiniationId { get; set; }
        public AdvancedSearchViewModel AdvancedSearchViewModel { get; set; }
    }
}
