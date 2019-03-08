using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.ValueObjects;
using System.Text.RegularExpressions;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;

namespace SmartTouch.CRM.Domain.Dropdowns
{
    public class DropdownValue : EntityBase<Int16>, IAggregateRoot
    {
     
        public byte DropdownID { get; set; }
        public int? AccountID { get; set; }
        string dropdownValue;
        public string Value { get { return dropdownValue; } set { dropdownValue = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }
        public bool IsDefault { get; set; }
        public Int16 DropdownValueTypeID { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public Int16 SortID { get; set; }
        public int OpportunityGroupID { get; set; }
        public int ContactId { get; set; }
        public Int32 LeadSouceID { get; set; }
        public Int32 CommunityID { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public int? LastModifiedBy { get; set; }
        public int ContactLeadSourceMapID { get; set; }
        public int ContactCommunityMapID { get; set; }

        //ContactLeadSourceMap -> DropdownValue
        public DateTime LastUpdatedDate { get; set; }
        public bool IsPrimary { get; set; }

        protected override void Validate()
        {
            throw new System.NotImplementedException();
        }
    }
}
