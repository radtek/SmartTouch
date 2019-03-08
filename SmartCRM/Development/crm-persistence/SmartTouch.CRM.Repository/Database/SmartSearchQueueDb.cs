using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class SmartSearchQueueDb
    {
        [Key]
        public int ResultID { get; set; }
        public int SearchDefinitionID { get; set; }
        public int AccountID { get; set; }
        public bool IsProcessed { get; set; }
        public DateTime CreatedOn { get; set; }

    }
}
