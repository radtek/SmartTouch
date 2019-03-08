using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Forms
{
    public class SearchFormsRequest : ServiceRequestBase
    {
        public int Limit { get; set; }
        public string Query { get; set; }
        public int PageNumber { get; set; }
        public int[] FormIDs { get; set; }
        public byte ShowingFieldType { get; set; }
        public byte Status { get; set; }
        public string SortField { get; set; }
        public ListSortDirection SortDirection { get; set; }
        public List<int> UserIds { get; set; }
        public int? UserID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class SearchFormsResponse : ServiceResponseBase
    {
        public long TotalHits { get; set; }
        public IEnumerable<FormViewModel> Forms { get; set; }
    }
}
