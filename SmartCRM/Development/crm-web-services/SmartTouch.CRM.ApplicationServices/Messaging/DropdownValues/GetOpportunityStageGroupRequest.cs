using SmartTouch.CRM.Domain.Dropdowns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.DropdownValues
{
    public class GetOpportunityStageGroupRequest : ServiceRequestBase
    {
    }
    public class GetOpportunityStageGroupResponse : ServiceResponseBase
    {
        public GetOpportunityStageGroupResponse() { }
        public IEnumerable<dynamic> OpportunityGroups { get; set; }
    }
}
