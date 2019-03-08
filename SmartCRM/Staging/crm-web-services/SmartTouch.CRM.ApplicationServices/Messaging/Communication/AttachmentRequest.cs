using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Communication
{
    public class AttachmentRequest : ServiceRequestBase
    {
        public AttachmentViewModel AttachmentViewModel { get; set; }
    }

    public class AttachmentResponse : ServiceResponseBase
    {
        public AttachmentViewModel AttachmentViewModel { get; set; }
    }

    public class GetAttachmentsResponse : ServiceResponseBase
    {
        public IEnumerable<AttachmentViewModel> Attachments { get; set; }
        public int TotalRecords { get; set; }
    }

    public class GetAttachmentsRequest :ServiceRequestBase
    {
        public int? ContactId { get; set; }
        public int? OpportunityID { get; set; }
        public string Page { get; set; }
        public int Limit { get; set; }
        public int PageNumber { get; set; }
        public string DateFormat { get; set; }        
    }

    //public class GetGuidRequest : ServiceRequestBase
    //{
    //    public int ContactId { get; set; }
    //    public int CommunicationTypeId { get; set; }
    //}

    //public class GetGuidResponse : ServiceResponseBase
    //{
    //    public CommunicationTrackerViewModel CommunicationTrackerViewModel { get; set; }
    //}
}
