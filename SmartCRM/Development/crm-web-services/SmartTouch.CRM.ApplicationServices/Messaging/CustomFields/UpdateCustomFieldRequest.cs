using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.CustomFields
{
    public class UpdateCustomFieldTabRequest : ServiceRequestBase
    {
        public CustomFieldTabViewModel CustomFieldTabViewModel { get; set; }
    }

    public class UpdateCustomFieldTabResponse : ServiceResponseBase
    {
        public CustomFieldTabViewModel CustomFieldTabViewModel { get; set; }
    }

    public class UpdateCustomFieldSectionRequest : ServiceRequestBase
    {
        public CustomFieldSectionViewModel CustomFieldSectionViewModel { get; set; }
    }

    public class UpdateCustomFieldSectionResponse : ServiceResponseBase
    {
        public CustomFieldSectionViewModel CustomFieldSectionViewModel { get; set; }
    }


    public class UpdateCustomFieldRequest : ServiceRequestBase
    {
        public FieldViewModel CustomFieldViewModel { get; set; }
    }

    public class UpdateCustomFieldResponse : ServiceResponseBase
    {
        public FieldViewModel CustomFieldViewModel { get; set; }
    }
}
