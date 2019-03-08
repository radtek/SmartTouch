using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Tags
{
    public class GetRecentAndPopularTagsRequest : ServiceRequestBase
    {
        public int[] TagsList { get; set; }
    }

    public class GetRecentAndPopularTagsResponse : ServiceResponseBase
    {
        public IEnumerable<TagViewModel> RecentTags { get; set; }
        public IEnumerable<TagViewModel> PopularTags { get; set; }
    }
}
