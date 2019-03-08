using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Forms
{
    public class SubmitFormRequest : ServiceRequestBase
    {
        public SubmittedFormViewModel SubmittedFormViewModel { get; set; }
        public FormDataCollection FormData { get; set; }
        public FormSubmissionMethod SubmissionMethod { get; set; }
    }

    public class SubmitFormResponse : ServiceResponseBase
    {
        public PersonViewModel PersonViewModel { get; set; }
        public FormSubmissionEntryViewModel FormSubmissionEntryViewModel { get; set; }
        public HttpStatusCode Status { get; set; }
        public FormAcknowledgement Acknowledgement { get; set; }
    }

    public class FormSubmissionEntryRequest : ServiceRequestBase
    {
        public int  ContactId{ get; set; }
    }

    public class FormSubmissionEntryResponse : ServiceResponseBase
    {
        public FormSubmissionEntryViewModel FormSubmissionEntry { get; set; }
    }
}
