using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Repository.Database
{
    public class OpportunityStageGroupsDb
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int OpportunityStageGroupID { get; set; }

        [ForeignKey("Accounts")]
        public int AccountID { get; set; }
        public virtual AccountsDb Accounts { get; set; }

        [ForeignKey("DropdownValues")]
        public short DropdownValueID { get; set; }
        public virtual DropdownValueDb DropdownValues { get; set; }

        [ForeignKey("OpportunityGroups")]
        public int OpportunityGroupID { get; set; }
        public virtual OpportunityGroupsDb OpportunityGroups { get; set; }

    }
}
