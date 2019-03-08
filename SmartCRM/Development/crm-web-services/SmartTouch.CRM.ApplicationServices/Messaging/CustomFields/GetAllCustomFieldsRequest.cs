using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.CustomFields
{
    public class GetAllCustomFieldTabsRequest : IntegerIdRequest
    {
        public GetAllCustomFieldTabsRequest(int accountId) : base(accountId) 
        {
           AccountId = accountId;
        }
    }

    public class GetAllCustomFieldTabsResponse : ServiceResponseBase
    {
        public CustomFieldTabsViewModel CustomFieldsViewModel { get; set; }
    }

    public class GetAllCustomFieldSectionsRequest : ServiceRequestBase
    {
    }

    public class GetAllCustomFieldSectionsResponse : ServiceResponseBase
    {
        public IEnumerable<CustomFieldSectionViewModel> CustomFieldSections { get; set; }
    }

    public class GetAllCustomFieldsRequest : IntegerIdRequest
    {
        public GetAllCustomFieldsRequest(int accountId)
            : base(accountId) 
        {
           AccountId = accountId;
        }
    }

    public class GetAllCustomFieldsResponse : ServiceResponseBase
    {
        public IEnumerable<FieldViewModel> CustomFields { get; set; }
    }
}
