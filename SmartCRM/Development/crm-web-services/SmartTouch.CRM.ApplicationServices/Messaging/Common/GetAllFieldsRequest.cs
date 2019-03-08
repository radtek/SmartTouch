using System.Collections.Generic;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Common
{
    public class GetAllFieldsRequest : ServiceRequestBase
    {        
    }

    public class GetAllFieldsResponse : ServiceResponseBase
    {
        /*Fields are collection of both contact fields and custom fields*/
        public IEnumerable<FieldViewModel> Fields { get; set; }
    }
}
