using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Search
{
    public class GetSearchRequest:ServiceRequestBase
    {
        public int SearchDefinitionID { get; set; }
        public bool IncludeSearchResults { get; set; }
        public int Limit { get; set; }
        public bool IsRunSearchRequest { get; set; }
        public bool IsAutomationRequest { get; set; }
        public bool IsSTAdmin { get; set; }
        public string Query { get; set; }
    }

    public class GetSearchResponse : ServiceResponseBase
    {
        public AdvancedSearchViewModel SearchViewModel { get; set; }
    }
}
