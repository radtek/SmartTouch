using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.SearchEngine.Search;

namespace SmartTouch.CRM.WebService.Tests.Contacts
{
    public class MockSuggestionData
    {
        public static IEnumerable<Suggestion> GetMockSuggestions(int limit)
        {
            IList<Suggestion> suggestions = new List<Suggestion>();
            foreach (int i in Enumerable.Range(1, limit))
            {
                suggestions.Add(new Suggestion() { Text = "suggestion" + i, DocumentId = i });
            }
            return suggestions;
        }
    }
}
