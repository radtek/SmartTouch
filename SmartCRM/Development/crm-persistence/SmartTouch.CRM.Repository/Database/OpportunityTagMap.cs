using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class OpportunityTagMap
    {
        [Key]
        public int OpportunityTagMapID { get; set; }
        [ForeignKey("Opportunities")]
        public virtual int OpportunityID { get; set; }
        public virtual OpportunitiesDb Opportunities { get; set; }
        [ForeignKey("Tags")]
        public virtual int TagID { get; set; }
        public virtual TagsDb Tags { get; set; }

        [ForeignKey("User")]
        public virtual int TaggedBy { get; set; }
        public virtual UsersDb User { get; set; }

        public DateTime TaggedOn { get; set; }
    }
}
