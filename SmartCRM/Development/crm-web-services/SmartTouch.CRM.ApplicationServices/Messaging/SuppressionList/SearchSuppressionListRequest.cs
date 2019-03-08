using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.SuppressionList
{
    public class SearchSuppressionListRequest : ServiceRequestBase
    {
        public string Text { get; set; }
        public byte IndexType { get; set; } 
    }

    public class SearchSuppressionListResponse<T> : ServiceResponseBase where T : SmartTouch.CRM.Domain.SuppressedEmails.SuppressionList
    {
        public IEnumerable<T> Results { get; set; }
    }
}
