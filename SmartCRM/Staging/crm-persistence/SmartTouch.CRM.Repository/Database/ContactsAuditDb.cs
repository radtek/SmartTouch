using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class ContactsAuditDb
    {
        [Key]
        public long AuditID { get; set; }
        public int ContactID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public int? CommunicationID { get; set; }
        public string Title { get; set; }
        public string ContactImageUrl { get; set; }
        public int AccountID { get; set; }

        public int LeadScore { get; set; }

        public string LeadSource { get; set; }
        public string HomePhone { get; set; }
        public string MobilePhone { get; set; }
        public string WorkPhone { get; set; }
        public string PrimaryEmail { get; set; }
        public ContactType ContactType { get; set; }
        public string SSN { get; set; }
        public bool? DoNotEmail { get; set; }
        public short? LifecycleStage { get; set; }
        public short? PartnerType { get; set; }
        public DateTime? LastContacted { get; set; }

        public int? OwnerID { get; set; }
        public int? ImageID { get; set; }

        public int? CompanyID { get; set; }
        public Guid? ProfileImageKey { get; set; }
       
        public int? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
    

        public bool IsDeleted { get; set; }

        public int? SourceType { get; set; }
        public ContactSource? ContactSource { get; set; }
        //public int LastUpdatedBy { get; set; }
        //public DateTime LastUpdatedOn { get; set; }
        public string AuditAction { get; set; }
        public DateTime AuditDate { get; set; }
        public string AuditUser { get; set; }
        public string AuditApp { get; set; }
        public Boolean AuditStatus { get; set; }
        public Guid? ReferenceId { get; set; }
      
    }
}
