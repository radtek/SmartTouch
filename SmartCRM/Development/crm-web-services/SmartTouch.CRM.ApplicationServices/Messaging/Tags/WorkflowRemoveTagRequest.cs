using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Tags
{
    public class WorkflowRemoveTagRequest : ServiceRequestBase
    {
        public int TagId { get; set; }
        public int ContactId { get; set; }
        public int CreatedBy { get; set; }
    }

    public class WorkflowRemoveTagResponse : ServiceResponseBase
    { 
    
    }
}
