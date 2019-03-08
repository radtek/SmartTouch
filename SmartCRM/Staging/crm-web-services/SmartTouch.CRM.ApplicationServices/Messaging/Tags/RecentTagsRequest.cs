using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Tags
{
    public class RecentTagsRequest : ServiceRequestBase
    {
        public int Limit { get; set; }
        public int[] TagsList { get; set; }
    }

    public class RecentTagsResponse : ServiceResponseBase
    {
        public IEnumerable<TagViewModel> TagsViewModel { get; set; }
    }
}
