using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class AutoCompleteSuggest
    {
        
        public string[] Input {get;set;}
        
        string output;
        public string Output { get { return output; } set { output = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }
        public SuggesterPayload Payload { get; set; }
        public int? Weight { get; set; }
    }

    public class SuggesterPayload
    {
        public int DocumentId { get; set; }
        public int AccountId { get; set; }
        public int DocumentOwnedBy { get; set; }
        public byte ContactType { get; set; }
        
    }
}
