using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class InsertAPILeadSubmissionRequest:ServiceRequestBase
    {
        public APILeadSubmissionViewModel ApiLeadSubmissionViewModel { get; set; }
    }

    public class InsertAPILeadSubmissionResponse:ServiceResponseBase
    {
        
    }
}
