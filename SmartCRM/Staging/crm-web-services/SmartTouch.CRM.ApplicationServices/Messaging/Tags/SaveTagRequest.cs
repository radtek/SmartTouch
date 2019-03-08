using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Tags
{
    public class SaveTagRequest : ServiceRequestBase
    {
        public TagViewModel TagViewModel { get; set; }
    }

    public class SaveTagResponse : ServiceResponseBase
    {
        public virtual TagViewModel TagViewModel { get; set; }
        public bool IsAssociatedWithWorkflows { get; set; }
        public bool IsAssociatedWithLeadScoreRules { get; set; }
    }
}
