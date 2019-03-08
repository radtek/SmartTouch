using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Forms
{
    public class GetFormRequest : IntegerIdRequest
    {
        public GetFormRequest(int id) : base(id) { }
    }

    public class GetFormResponse : ServiceResponseBase
    {
        public FormViewModel FormViewModel { get; set; }
    }
}
