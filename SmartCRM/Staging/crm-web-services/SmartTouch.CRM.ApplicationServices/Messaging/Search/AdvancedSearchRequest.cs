using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.SearchEngine.Search;
using SmartTouch.CRM.Entities;
using System.ComponentModel;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Search
{
    public class AdvancedSearchRequest<T> : ServiceRequestBase where T : IShallowContact
    {
        public AdvancedSearchViewModel SearchViewModel { get; set; }
        public int Limit { get; set; }
        public int PageNumber { get; set; }
        /// <summary>
        /// List of fields that should be included in the results. If empty all fields are included.
        /// </summary>
        public IEnumerable<ContactFields> Fields { get; set; }
        public bool IsAdvancedSearch { get; set; }
        public bool ViewContacts { get; set; }

        public string Query { get; set; }
        public ContactSortFieldType SortFieldType { get; set; }
        public ContactShowingFieldType ShowingFieldType { get; set; }
        public string SortField { get; set; }
        public ListSortDirection SortDirection { get; set; }
        public bool IsResultsGrid { get; set; }
        public IEnumerable<Type> ContactTypes { get; set; }
        public bool ShowByCreated { get; set; }
    }

    public class AdvancedSearchResponse<T> : ServiceResponseBase where T : IShallowContact
    {
        public SearchResult<T> SearchResult { get; set; }
        public IEnumerable<int> ContactIds { get; set; } 
        //public SearchResult<PersonViewModel> Contact { get; set; }
    }
}
