using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.CustomFields
{
    public class GetCustomFieldTabRequest : IntegerIdRequest
    {
        //public int AccountId { get; set; }
        public GetCustomFieldTabRequest(int accountId) : base(accountId) { }
    }

    public class GetCustomFieldTabResponse : ServiceResponseBase
    {
        public FieldViewModel CustomFieldViewModel { get; set; }
    }

    //public class GetContactCustomFieldMapRequest : ServiceRequestBase
    //{
    //    public GetContactCustomFieldMapRequest(int customFieldId, int contactId) { }
    //}

    //public class GetContactCustomFieldMapResponse : ServiceResponseBase
    //{
    //    public int I { get; set; }
    //}
}
