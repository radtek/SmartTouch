using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Repository.Database
{
    public class LeadAdapterAndAccountMapDb
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LeadAdapterAndAccountMapId { get; set; }
        public Guid RequestGuid { get; set; }
        public string BuilderNumber { get; set; }
        public string ArchivePath { get; set; }

        public string LocalFilePath { get; set; }
        public virtual LeadAdapterTypesDb LeadAdapterTypes { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public int? ModifiedBy { get; set; }
        public bool IsDelete { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
        [ForeignKey("Account")]
        public virtual int AccountID { get; set; }
        public virtual AccountsDb Account { get; set; }
        [ForeignKey("LeadAdapterTypes")]
        public virtual byte LeadAdapterTypeID { get; set; }

        [NotMapped]
        public string LeadAdapterType { get; set; }
        [NotMapped]
        public string LeadAdapterErrorName { get; set; }
        [NotMapped]
        public string ServiceStatusMessage { get; set; }

        public LeadAdapterErrorStatus? LeadAdapterErrorStatusID { get; set; }
        [ForeignKey("LeadAdapterErrorStatusID")]
        public LeadAdapterErrorStatusDb LeadAdapterErrorStatus { get; set; }
        
        public DateTime? LastProcessed { get; set; }

        [ForeignKey("DropdownValues")]
        public short? LeadSourceType { get; set; }
        public DropdownValueDb DropdownValues { get; set; }

        public short? LeadAdapterServiceStatusID { get; set; }
        [ForeignKey("LeadAdapterServiceStatusID")]
        public StatusesDb Statuses { get; set; }

        public ICollection<LeadAdapterTagMapDb> Tags { get; set; }
        public string CommunityNumber { get; set; }

        [NotMapped]
        public FacebookLeadAdapterDb FacebookLeadAdapter { get; set; }
        [NotMapped]
        public string FacebookLeadAdapterName { get; set; }
        [NotMapped]
        public string AccountName { get; set; }
        [NotMapped]
        public int TotalCount { get; set; }
    }
}
