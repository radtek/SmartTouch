using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Repository.Database
{
    public class OpportunityGroupsDb
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OpportunityGroupID { get; set; }
        public string OpportunityGroupName { get; set; }
    }
}
