using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Forms
{
    public class UpdateFormRequest : ServiceRequestBase
    {
        public FormViewModel FormViewModel { get; set; }
    }

    public class UpdateFormResponse : ServiceResponseBase
    {
        public FormViewModel FormViewModel { get; set; }
    }
}
