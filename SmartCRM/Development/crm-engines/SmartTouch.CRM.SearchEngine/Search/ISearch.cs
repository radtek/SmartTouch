using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.SearchEngine.Search
{
    public interface ISearch<T> where T : EntityBase<int>
    {
        SearchResult<T> Search(string q, SearchParameters parameters);

        SearchResult<T> SearchByIds<D>(string q, IEnumerable<D> ids, SearchParameters searchParameters);

        SearchResult<Suggestion> AutoCompleteField(string q, SearchParameters searchParameters);

        /// <summary>
        /// Performs a matching search and returns the match.
        /// </summary>
        /// <param name="query">Search string</param>
        /// <returns></returns>
        SearchResult<T> DuplicateSearch(T t, SearchParameters searchParameters);

        SearchResult<Suggestion> QuickSearch(string q);
    }
}
