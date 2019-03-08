using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class LeadAdapterTagMapDb
    {
        [Key]
        public int LeadAdapterTagMapID { get; set; }

        [ForeignKey("LeadAdapter")]
        public int LeadAdapterID { get; set; }
        public LeadAdapterAndAccountMapDb LeadAdapter { get; set; }

        [ForeignKey("Tag")]
        public int TagID { get; set; }
        public TagsDb Tag { get; set; }
    }
}
