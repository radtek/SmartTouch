using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface IAttachmentService
    {
        //AttachmentResponse InsertDocRepository(AttachmentRequest request);
        bool SaveAttachment(SaveAttachmentRequest request);
        AttachmentResponse GeAttachment(AttachmentRequest request);
        GetAttachmentsResponse GetAllAttachments(GetAttachmentsRequest request);
        AttachmentResponse DeleteAttachment(AttachmentRequest request);
     
    }
}
