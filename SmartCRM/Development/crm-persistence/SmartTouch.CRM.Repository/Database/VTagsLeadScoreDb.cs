using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class VTagsLeadScoreDb
    {
        [Key]
        public int TagID { get; set; }
        public int AccountID { get; set; }
        public int Count { get; set; }
    }
}
