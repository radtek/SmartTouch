using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Forms
{
    public class DeleteFormRequest: ServiceRequestBase
    {
        public int[] FormIDs { get; set; }
    }

    public class DeleteFormResponse : ServiceResponseBase
    {
        public FormViewModel FormViewModel { get; set; }
    }
}
