using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class OpportunitiesDb
    {
        [Key]
        public int OpportunityID { get; set; }
        public virtual UsersDb Users { get; set; }
        public string OpportunityName { get; set; }
        public decimal Potential { get; set; }
        [ForeignKey("Statuses")]
        public virtual short StageID { get; set; }
        public virtual DropdownValueDb Statuses {get; set;}
        public DateTime? ExpectedClosingDate { get; set; }
        public string Description { get; set; }
        [ForeignKey("Users")]
        public virtual int Owner { get; set; }
        [ForeignKey("Accounts")]
        public virtual int AccountID { get; set; }
        public virtual AccountsDb Accounts { get; set; }
        public virtual int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public virtual int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public bool? IsDeleted { get; set; }
        public string OpportunityType { get; set; }
        public string ProductType { get; set; }
        public string Address { get; set; }
        public int? ImageID { get; set; }
        public ICollection<OpportunityContactMap> ContactsMap { get; set; }
        public ICollection<OpportunityTagMap> OpportunityTags { get; set; }
        public ICollection<OpportunitiesRelationshipMapDb> OpportunitiesRelations { get; set; }
    }
}
