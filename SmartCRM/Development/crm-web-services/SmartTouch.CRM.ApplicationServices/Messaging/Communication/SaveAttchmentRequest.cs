using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Communication
{
    public class SaveAttachmentRequest:ServiceRequestBase
    {
        public FilesViewModel[] filesViewModel { get; set; }
        public int? ContactId { get; set; }
        public int CreatedBy { get; set; }
        public char StorageSource { get; set; }
        public int? OpportunityID { get; set; }
    }
   
}
