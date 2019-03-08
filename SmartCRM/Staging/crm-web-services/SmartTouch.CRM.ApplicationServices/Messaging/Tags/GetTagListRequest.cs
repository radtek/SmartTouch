using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ViewModels;
using System.ComponentModel;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Tags
{
    public class GetTagListRequest : ServiceRequestBase
    {
        public string Name { get; set; }
        public int Limit { get; set; }
        public int PageNumber { get; set; }
        public string SortField { get; set; }
        public ListSortDirection SortDirection { get; set; }
        public bool IsSTadmin { get; set; }
    }

    public class GetTagListResponse : ServiceResponseBase
    {

        public IEnumerable<TagViewModel> Tags { get; set; }
        public int TotalHits { get; set; }
        public IEnumerable<TagViewModel> PopularTags { get; set; }
        public IEnumerable<TagViewModel> RecentTags { get; set; }
    }
}
