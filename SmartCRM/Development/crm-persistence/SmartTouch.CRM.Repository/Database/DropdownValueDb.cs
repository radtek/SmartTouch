using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class DropdownValueDb
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public short DropdownValueID { get; set; }
        [ForeignKey("Dropdowns")]
        public byte DropdownID { get; set; }
        public DropdownDb Dropdowns { get; set; }
        [ForeignKey("Accounts")]
        public int? AccountID { get; set; }
        public AccountsDb Accounts { get; set; }
        public string DropdownValue { get; set; }
        public bool IsDefault { get; set; }
        public bool IsDeleted { get; set; }
        public Int16 SortID { get; set; }
        [ForeignKey("DropdownValueTypes")]
        public Int16 DropdownValueTypeID { get; set; }
        public bool IsActive { get; set; }
        public DropdownValueTypeDb DropdownValueTypes { get; set; }
        public ICollection<ContactsDb> Contacts { get; set; }
        public ICollection<ContactRelationshipDb> Relations { get; set; }
        public ICollection<OpportunityStageGroupsDb> OpportunityStageGroups { get; set; }
    }
}
