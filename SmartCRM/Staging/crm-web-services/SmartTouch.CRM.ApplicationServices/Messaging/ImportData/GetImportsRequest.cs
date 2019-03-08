using System.Collections.Generic;

using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.ImportData
{
    public class GetImportsRequest : ServiceRequestBase
    {
        public int Limit { get; set; }
        public string Query { get; set; }
        public int PageNumber { get; set; }
    }

    public class GetImportsResponse : ServiceResponseBase
    {
        public int TotalHits { get; set; }
        public IEnumerable<ImportListViewModel> Imports { get; set; }
    }
}
