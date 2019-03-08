using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public interface IAttachmentViewModel
    {
        long DocumentID { get; set; }
        int? ContactID { get; set; }
        string OriginalFileName { get; set; }
        string StorageFileName { get; set; }
        int DocumentTypeID { get; set; }
        int FileTypeID { get; set; }
        string DocumentType { get; set; }
        string FileType { get; set; }
        int CreatedBy { get; set; }
        DateTime CreatedDate { get; set; }
        int? ModifiedBy { get; set; }
        DateTime? ModifiedDate { get; set; }
        string UserName { get; set; }
        string FilePath { get; set; }
        int? OpportunityID { get; set; }
        char? StorageSource { get; set; }
    }
    public class AttachmentViewModel : IAttachmentViewModel
    {
        public long DocumentID { get; set; }
        public int? ContactID { get; set; }
        public string OriginalFileName { get; set; }
        public string StorageFileName { get; set; }
        public int DocumentTypeID { get; set; }
        public int FileTypeID { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string DocumentType { get; set; }
        public string FileType { get; set; }
        public string UserName { get; set; }
        public string FilePath { get; set; }
        public int UserId { get; set; }
        public int? OpportunityID { get; set; }
        public char? StorageSource { get; set; }
        public string AttachmentTime { get; set; }
    }
  
}
