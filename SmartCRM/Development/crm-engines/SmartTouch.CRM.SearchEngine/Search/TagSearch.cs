using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.SearchEngine.Search
{
    internal class TagSearch<T> : SearchBase<T> where T: class
    {
        public TagSearch(int accountId) : base(accountId)
        {
        }

        public override SearchResult<Suggestion> AutoCompleteField(string q, SearchParameters searchParameters)
        {
            return base.AutoCompleteField(q, searchParameters);
        }
    }
}
