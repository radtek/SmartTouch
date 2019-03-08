using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Accounts
{
    public class IndexingData
    {          
        public int IndexType { get; set; }
        public IList<int> EntityIDs { get; set; }
        public ILookup<int, bool> Ids { get; set; }         //ContactID, IsPrecolationNeeded
        public IEnumerable<Guid> ReferenceIDs { get; set; }     //ReferenceID, ContactID
    }
}
