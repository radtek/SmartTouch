using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.SuppressionList
{
    public class InsertSuppressedEmailRequest: ServiceRequestBase
    {
        public IEnumerable<SuppressedEmailViewModel> EmailViewModel { get; set; }
    }

    public class InsertSuppressedEmailResponse : ServiceResponseBase
    {

    }
}
