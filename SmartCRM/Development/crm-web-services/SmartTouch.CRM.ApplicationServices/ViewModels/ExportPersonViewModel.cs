using System.Collections.Generic;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.Contacts;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class ExportPersonViewModel
    {
        public int[] ContactID { get; set; }
        public string DownLoadAs { get; set; }
        public ExportFieldOrder SortOrder { get; set; }
        public ExportFieldTypes SortBy { get; set; }
        public int[] selectedFields { get; set; }
        public string DateFormat { get; set; }
        public int PageSize { get; set; }
        public IEnumerable<FieldViewModel> SearchFields { get; set; }
        public IList<Contact> Contacts { get; set; }

        public int SearchDefinitionId { get; set; }
        public bool SelectAll { get; set; }
    }
}
