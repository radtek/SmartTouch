using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class AttachmentsDb
    {
        [Key]
        public int DocumentID { get; set; }
        public string OriginalFileName { get; set; }
        public string StorageFileName { get; set; }
        public DocumentType DocumentTypeID { get; set; }
        public FileType FileTypeID { get; set; }

        [ForeignKey("Users")]
        public int CreatedBy { get; set; }
        public virtual UsersDb Users { get; set; }

        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string FilePath { get; set; }
        
        [ForeignKey("Contact")]
        public int? ContactID { get; set; }
        public virtual ContactsDb Contact { get; set; }
        
        [ForeignKey("Opportunities")]
        public int? OpportunityID { get; set; }
        public virtual OpportunitiesDb Opportunities { get; set; }
        
        public char? StorageSource { get; set; }
    }
}
