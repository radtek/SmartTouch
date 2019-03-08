using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Tags
{
    public class GetTagsRequest : ServiceRequestBase
    {
        public string Name { get; set; }
    }

    public class GetTagsResponse : ServiceResponseBase
    {
        public ITagsListViewModel TagsListViewModel { get; set; }
    }
}
