using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Communication
{
    public class Attachment : EntityBase<int>, IAggregateRoot
    {
        public int? ContactID { get; set; }
        public string OriginalFileName { get; set; }
        public string StorageFileName { get; set; }
        public DocumentType DocumentTypeID { get; set; }
        public FileType FileTypeID { get; set; }
        public int CreatedBy { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string FilePath { get; set; }
        public char? StorageSource { get; set; }
        public int? OpportunityID { get; set; }
        public string AttachmentTime { get; set; }

        protected override void Validate()
        {
            //need to implement server side validations
        }
    }
}
