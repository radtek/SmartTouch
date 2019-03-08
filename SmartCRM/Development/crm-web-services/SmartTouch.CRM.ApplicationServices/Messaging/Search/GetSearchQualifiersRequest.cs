using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Search
{
    public class GetSearchValueOptionsRequest : ServiceRequestBase
    {
        public int FieldId { get; set; }
        public int? ContactDropdownId { get; set; }
        public bool IsSTAdmin { get; set; }
    }

    public class GetSearchValueOptionsResponse : ServiceResponseBase
    {
        public IEnumerable<FieldValueOption> FieldValueOptions { get; set; }
    }
}
