using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Forms
{
    public class CreateAPIFormsRequest : ServiceRequestBase
    {
        public FormViewModel FormViewModel { get; set; }
    }

    public class CreateAPIFormsResponse : ServiceResponseBase
    {
        public FormEntryViewModel ViewModel { get; set; } 
    }
}
