using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.ComponentModel;

using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.Search;
using LandmarkIT.Enterprise.Extensions;
using SmartTouch.CRM.Domain.ValueObjects;

namespace SmartTouch.CRM.SearchEngine.Search
{
    public interface ISearchService<T>
    {
        /// <summary>
        /// Returns the search results with the given no. of results limit.
        /// </summary>
        /// <param name="query">Search string</param>
        /// <param name="searchParameters">Search parameters. </param>
        /// <returns></returns>
        SearchResult<T> Search(string q, SearchParameters searchParameters);

        SearchResult<T> Search(string q, Expression<Func<T, bool>> filter, SearchParameters searchParameters);        

        SearchResult<T> SearchCampaigns(string q, SearchParameters searchParameters);

        SearchResult<Suggestion> AutoCompleteField(string q, SearchParameters searchParameters);

        /// <summary>
        /// Performs a matching search and returns the match.
        /// </summary>
        /// <param name="query">Search string</param>
        /// <returns></returns>
        SearchResult<T> DuplicateSearch(T t, SearchParameters searchParameters);

        SearchResult<Suggestion> QuickSearch(string q, SearchParameters parameters);

        Task<SearchResult<T>> AdvancedSearchAsync(string query, SearchDefinition searchDefinition, SearchParameters searchParameters);

        Task<SearchResult<T>> AdvancedSearchExportAsync(string query, SearchDefinition searchDefinition, SearchParameters searchParameters);

        bool IsOwnedBy(int documentId, int? userId, int accountId);

        bool IsCreatedBy(int documentId, int? userId, int accountId);

        bool SaveQuery(SearchDefinition searchDefinition, SearchParameters searchParameters);

        int SaveQueries(IEnumerable<SearchDefinition> searchDefinitions, SearchParameters searchParameters, int accountId);
        
        bool RemoveQuery(short searchDefinitionId, SearchParameters searchParameters);

        /// <summary>
        /// Percolates the given contacts against all the registered queries(search definitions).
        /// </summary>
        /// <returns>Returns key value pair, key is ContactId and value is SearchDefinitionId.</returns>
        IEnumerable<QueryMatch> FindMatchingQueries(IEnumerable<T> documents, SearchParameters searchParameters);

        IDictionary<DateTime, long> GetContactsAggregationByDate(SearchDefinition searchDefinition, SearchParameters parameters);

        IDictionary<int, long> GetTopLeadSources(SearchDefinition searchDefinition, SearchParameters parameters);

        IEnumerable<string> CheckSuppressionList(SearchParameters p);

        IEnumerable<T> SearchSuppressionList(string queryString, SearchParameters p);

        SavedSearchActiveContacts GetAggregationBySavedSearch(SearchDefinition searchDefinition, SearchParameters parameters);
    }

    public class SearchParameters
    {
        /// <summary>
        /// Types to be included as part of search.
        /// </summary>
        public IEnumerable<Type> Types { get; set; }

        /// <summary>
        /// Current Page number.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Number of results requested.
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// List of fields that should be included in the response. If empty, all fields are included.
        /// </summary>
       public IEnumerable<ContactFields> Fields { get; set; }

        /// <summary>
        /// If true matches all types of fields. Includes a MatchAll clause in search query.
        /// </summary>
        public bool MatchAll { get; set; }

        public string AutoCompleteFieldName { get; set; }

        public string IndexName { get; set; }

        //public IList<SortField> SortFields { get; set; }
        public ContactSortFieldType? SortField { get; set; }
        public bool IsResultsGrid { get; set; }
        public string ResultsGridSortField { get; set; }

        public int AccountId { get; set; }

        public int? UserID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public IEnumerable<string> Ids { get; set; }
        public ListSortDirection SortDirection { get; set; }

        public IEnumerable<AppModules> PrivateModules { get; set; }

        public bool IsPrivateSearch { get; set; }
        public int? DocumentOwnerId { get; set; }
        public bool IsActiveContactsSearch { get; set; }

        /// <summary>
        /// EntityBase properties in camel casing.
        /// </summary>
        public IEnumerable<string> SortFields { get; set; }

        public override string ToString()
        {
            StringBuilder parameters = new StringBuilder();
            if (Types.IsAny())
            {
                parameters.Append("Types:");
                parameters.Append(string.Join(",", Types.Select(c => c.ToString()).ToArray())).Append(";");
            }
            parameters.Append("PageNumber:").Append(PageNumber).Append(";");
            parameters.Append("Limit:").Append(Limit).Append(";");
            parameters.Append("MatchAll:").Append(MatchAll.ToString()).Append(";");
            //parameters.Append("UserID:").Append(UserID).Append(";");
            //parameters.Append("StartDate:").Append(StartDate).Append(";");
            //parameters.Append("EndDate:").Append(EndDate).Append(";");
            parameters.Append("AutocompleteFieldName:").Append(AutoCompleteFieldName).Append(";");
            parameters.Append("IndexName:").Append(IndexName).Append(";");
            return parameters.ToString();
        }
    }

    public class SearchResult<T>
    {
        public IEnumerable<T> Results { get; set; }
        public long TotalHits { get; set; }
    }

    //public class SortField
    //{
    //    public int Priority { get; set; }
    //    public string Field { get; set; }
    //}
}
