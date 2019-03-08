using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Forms
{
    public class GetFormSubmissionRequest : ServiceRequestBase
    {
        public int FormSubmissionID { get; set; }
    }

    public class GetFormSubmissionResponse : ServiceResponseBase
    {
        public FormSubmissionEntryViewModel FormSubmission { get; set; }
    }
}
