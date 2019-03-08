using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class OpportunityActionMap
    {
        [Key]
        public int OpportunityActionMapID { get; set; }
        public bool? IsCompleted { get; set; }
        [ForeignKey("Opportunities")]
        public virtual int OpportunityID { get; set; }
        public virtual OpportunitiesDb Opportunities { get; set; }
        [ForeignKey("Action")]
        public virtual int ActionID { get; set; }
        public virtual ActionsDb Action { get; set; }
    }
}
