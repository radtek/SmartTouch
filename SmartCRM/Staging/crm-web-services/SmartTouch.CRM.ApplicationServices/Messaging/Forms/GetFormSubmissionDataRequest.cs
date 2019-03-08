using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Forms
{
    public class GetFormSubmissionDataRequest : ServiceRequestBase
    {
      
    }
    public class GetFormSubmissionDataResponse : ServiceResponseBase
    {
        public SubmittedFormViewModel SubmittedFormViewModel { get; set; }
    }
}
