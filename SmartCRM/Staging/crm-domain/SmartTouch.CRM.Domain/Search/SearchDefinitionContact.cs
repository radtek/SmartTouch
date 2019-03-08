using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Search
{
    public class SearchDefinitionContact
    {
        public Guid GroupId { get; set; }
        public int SearchDefinitionId { get; set; }
        public int ContactId { get; set; }

    }
}
