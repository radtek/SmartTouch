using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.LeadAdapters
{
    public class GetImportDataRequest:ServiceRequestBase
    {
        public int AccountID { get; set; }
        public int Limit { get; set; }
        public string Query { get; set; }
        public int PageNumber { get; set; }
    }
    public class GetImportDataResponse : ServiceResponseBase
    {
        public IEnumerable<ImportDataViewModel> ImportData { get; set; }
        public int TotalHits { get; set; }
    }
}
