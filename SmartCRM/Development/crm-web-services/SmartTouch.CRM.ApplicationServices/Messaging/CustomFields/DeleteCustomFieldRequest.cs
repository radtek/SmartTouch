using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.CustomFields
{
    public class DeleteCustomFieldTabRequest : IntegerIdRequest
    {
        public DeleteCustomFieldTabRequest(int tabId) : base(tabId) { }
    }

    public class DeleteCustomFieldTabResponse : ServiceResponseBase
    {
        public CustomFieldTabViewModel CustomFieldTabViewModel { get; set; }
    }

    public class DeleteCustomFieldSectionRequest : ServiceRequestBase
    {
        public int CustomFieldSectionID { get; set; }
    }

    public class DeleteCustomFieldSectionResponse : ServiceResponseBase
    {
        public CustomFieldSectionViewModel CustomFieldSectionViewModel { get; set; }
    }

    public class DeleteCustomFieldRequest : ServiceRequestBase
    {
        public int CustomFieldID { get; set; }
    }


    public class DeleteCustomFieldResponse : ServiceResponseBase
    {
        public FieldViewModel CustomFieldViewModel { get; set; }
    }
}
