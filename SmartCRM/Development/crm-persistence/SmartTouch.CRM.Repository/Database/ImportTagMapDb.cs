using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class ImportTagMapDb
    {
        [Key]
        public int ImportTagMapID { get; set; }
        [ForeignKey("JobLogs")]
        public int LeadAdapterJobLogID { get; set; }
        public LeadAdapterJobLogsDb JobLogs { get; set; }
        [ForeignKey("Tag")]
        public virtual int TagID { get; set; }
        public virtual TagsDb Tag { get; set; }
    }
}
