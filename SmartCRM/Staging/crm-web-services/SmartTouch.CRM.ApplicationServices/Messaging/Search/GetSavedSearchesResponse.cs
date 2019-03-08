using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Search
{
    public class GetSavedSearchesRequest:ServiceRequestBase
    {
        public int Limit { get; set; }
        public string Query { get; set; }
        public int PageNumber { get; set; }        
        public int AccountID { get; set; }
        public bool IsPredefinedSearch { get; set; }
        public bool IsFavoriteSearch { get; set; }
    }

    public class GetSavedSearchesResponse : ServiceResponseBase
    {
        public int TotalHits { get; set; }
        public IEnumerable<AdvancedSearchViewModel> SearchResults { get; set; }
    }

    public class GetSavedSearchesByNameRequest : ServiceRequestBase
    {
        public int Limit { get; set; }
        public string Query { get; set; }
    }
}
