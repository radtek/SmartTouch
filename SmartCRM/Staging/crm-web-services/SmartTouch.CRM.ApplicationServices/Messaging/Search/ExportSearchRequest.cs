using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Search
{
    public class ExportSearchRequest:ServiceRequestBase
    {
        public AdvancedSearchViewModel SearchViewModel { get; set; }
        public string FileType { get; set; }
        public string DateFormat { get; set; }
        public string TimeZone { get; set; }
        public DownloadType DownloadType { get; set; }
        public IEnumerable<int> SelectedFields { get; set; }
        public List<int> SelectedContactIds { get; set; }
        public IEnumerable<FieldViewModel> SearchFields { get; set; }
    }

    public class ExportSearchResponse : ServiceResponseBase
    {
        public byte[] byteArray { get; set; }
        public string FileName { get; set; }
    }
}
