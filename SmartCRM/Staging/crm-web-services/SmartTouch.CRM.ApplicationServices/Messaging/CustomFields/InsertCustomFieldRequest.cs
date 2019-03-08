using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.CustomFields
{
    public class InsertCustomFieldTabRequest : ServiceRequestBase
    {
        public CustomFieldTabViewModel CustomFieldTabViewModel { get; set; }
    }
    public class InsertCustomFieldTabResponse : ServiceResponseBase
    {
        public CustomFieldTabViewModel CustomFieldTabViewModel { get; set; }
    }

    public class InsertCustomFieldSectionRequest : ServiceRequestBase
    {
        public CustomFieldSectionViewModel CustomFieldSectionViewModel { get; set; }
    }
    public class InsertCustomFieldSectionResponse : ServiceResponseBase
    {
        public CustomFieldSectionViewModel CustomFieldSectionViewModel { get; set; }
    }

    public class InsertCustomFieldRequest : ServiceRequestBase
    {
        public FieldViewModel CustomFieldViewModel { get; set; }
    }
    public class InsertCustomFieldResponse : ServiceResponseBase
    {
        public FieldViewModel CustomFieldViewModel { get; set; }
    }

    public class SaveAllCustomFieldTabsRequest : ServiceRequestBase
    {
        public CustomFieldTabsViewModel CustomFieldsViewModel { get; set; }
    }

    public class SaveAllCustomFieldTabsResponse : ServiceResponseBase
    {
        public CustomFieldTabsViewModel CustomFieldsViewModel { get; set; }
    }
}
