using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Fields;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Forms
{
    public class GetAllContactFieldsRequest : ServiceRequestBase
    {
    }

    public class GetAllContactFieldsResponse : ServiceResponseBase
    {
        public IList<FieldViewModel> ContactFields { get; set; }
    }
}
