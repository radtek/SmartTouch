using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.CustomFields
{
    public class GetCustomFieldsValueOptionsRequest : ServiceRequestBase
    {
    }

    public class GetCustomFieldsValueOptionsResponse : ServiceResponseBase
    {
        public IEnumerable<CustomFieldValueOptionViewModel> CustomFieldValueOptions { get; set; }
    }
}
