using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using System.Collections.Generic;

namespace SmartTouch.CRM.ApplicationServices.Messaging.CustomFields
{
    public class GetLeadAdapterCustomFieldRequest : ServiceRequestBase
    {
        public LeadAdapterTypes LeadAdapterType { get; set; }
    }

    public class GetLeadAdapterCustomFieldResponse : ServiceResponseBase
    {
        public IEnumerable<CustomFieldViewModel> CustomFields { get; set; }
    }
}
