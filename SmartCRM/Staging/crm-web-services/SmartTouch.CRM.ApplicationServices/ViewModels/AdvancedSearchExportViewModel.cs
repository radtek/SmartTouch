using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class AdvancedSearchExportViewModel 
    {
        public IEnumerable<FieldViewModel> SearchFields { get; set; }
        public IEnumerable<int> SelectedFields { get; set; }
        public ExportFieldOrder SortOrder { get; set; }
        public ExportFieldTypes SortBy { get; set; }
        public DownloadType DownloadType { get; set; }
        public List<int> SelectedContactIds { get; set; }
    }
}
