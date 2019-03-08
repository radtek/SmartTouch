using SmartTouch.CRM.ApplicationServices.ViewModels;
using System.Collections.Generic;

namespace SmartTouch.CRM.ApplicationServices.Messaging.LeadScore
{
    public class GetFormsRequest : ServiceRequestBase
    {
        public bool IsSTAdmin { get; set; }
    }

    public class GetFormResponse : ServiceResponseBase
    {
        public IEnumerable<FormEntryViewModel> Forms { get; set; }
    }
}
