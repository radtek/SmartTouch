using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.SearchEngine.Search
{
    public class Suggestion
    {
        public int DocumentId { get; set; }
        public int AccountId { get; set; }
        public int DocumentOwnedBy { get; set; }
        public byte ContactType { get; set; }
        public string Text { get; set; }
        public SearchableEntity Entity { get; set; }                
    }
}
