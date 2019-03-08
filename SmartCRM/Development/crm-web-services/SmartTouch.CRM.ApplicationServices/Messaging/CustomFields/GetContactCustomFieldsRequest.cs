using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.CustomFields
{
    public class GetContactCustomFieldsRequest : IntegerIdRequest
    {
        public GetContactCustomFieldsRequest(int contactId)
            : base(contactId)
        { }
    }

    public class GetContactCustomFieldsResponse : ServiceResponseBase
    {
        public IEnumerable<ContactCustomFieldMapViewModel> ContactCustomFields { get; set; }
        public IEnumerable<CustomFieldTabViewModel> CustomFieldTabs { get; set; }
    }
}

