using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LandmarkIT.Enterprise.CommunicationManager.Database
{
    public class CronJobHistoryDb
    {
        [Key]
        public int CronJobHistoryID { get; set; }
        public CronJobType CronJobID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Remarks { get; set; }

        [ForeignKey("CronJobID")]
        public CronJobDb CronJob { get; set; }
    }
}
