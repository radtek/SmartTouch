using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;


namespace SmartTouch.CRM.Repository.Database
{
    public class OpportunitiesRelationshipMapDb
    {
        [Key]
        public int OpportunityRelationshipMapID { get; set; }
        [ForeignKey("RelationshipTypes")]
        public short RelationshipTypeID { get; set; }
        public virtual DropdownValueDb RelationshipTypes { get; set; }
        [ForeignKey("Opportunities")]
        public int OpportunityID { get; set; }
        public virtual OpportunitiesDb Opportunities { get; set; }
        [ForeignKey("Contacts")]
        public int ContactID { get; set; }
        public virtual ContactsDb Contacts { get; set; }
    }
}
