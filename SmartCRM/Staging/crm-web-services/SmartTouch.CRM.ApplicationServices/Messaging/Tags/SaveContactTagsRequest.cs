using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Tags
{
    public class SaveContactTagsRequest : ServiceRequestBase
    {
        public int UserId { get; set; }
        public IEnumerable<ContactEntry> Contacts { get; set; }
        public IEnumerable<TagViewModel> Tags { get; set; }
        public IEnumerable<OpportunitiesList> Opportunities { get; set; }
    }

    public class SaveContactTagsResponse : ServiceResponseBase
    {
        public List<int> TagIds { get; set; }
    }
}
