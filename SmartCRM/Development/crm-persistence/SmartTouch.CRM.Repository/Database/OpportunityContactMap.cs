using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class OpportunityContactMap
    {
        [Key]
        public int OpportunityContactMapID { get; set; }
        [ForeignKey("Opportunities")]
        public virtual int OpportunityID { get; set; }
        public virtual OpportunitiesDb Opportunities { get; set; }
        [ForeignKey("Contacts")]
        public virtual int ContactID { get; set; }
        public virtual ContactsDb Contacts { get; set; }
        public short StageID { get; set; }
        public bool IsDeleted { get; set; }

    }
}
