using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Tags
{
    public class DeleteTagRequest : ServiceRequestBase
    {
        public string TagName { get; set; }
        public int TagId { get; set; }
        public int ContactID { get; set; }
        public int OpportunityID { get; set; }
    }

    public class DeleteTagIdsRequest : ServiceRequestBase
    {
        public int[] TagID { get; set; }
    }

    public class DeleteTagResponse : ServiceResponseBase
    {
    }
}
