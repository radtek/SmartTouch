using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Notes
{
    public class GetContactCountsRequest : ServiceRequestBase
    {
        public int NoteId { get; set; }
    }

    public class GetContactCountsResponse : ServiceResponseBase
    {
        public int Count { get; set; }
        public bool SelectAll { get; set; }
    }
}
