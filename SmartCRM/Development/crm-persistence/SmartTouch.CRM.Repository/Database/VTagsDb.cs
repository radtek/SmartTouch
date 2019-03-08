using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class VTagsDb
    {       
        [Key]
        public int TagID { get; set; }
        public string TagName { get; set; }
        public string Description { get; set; }
        public int? Count { get; set; }
        public int AccountID { get; set; }
        public int? CreatedBy { get; set; }
        public bool? IsDeleted { get; set; }

        [NotMapped]
        public int TotalCount { get; set; }
        [NotMapped]
        public bool     LeadScoreTag { get; set; }
    }
}
