using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LandmarkIT.Enterprise.CommunicationManager.Database
{
    public class ScheduledJobDb
    {
        [Key]
        public int ScheduledJobId { get; set; }
        //public int JobId { get; set; }
        public long Interval { get; set; }
        public DateTime LastRunOn { get; set; }
        public bool IsScheduledOnTime { get; set; }
        public DateTime? NextScheduledTime { get; set; }
        //[ForeignKey("JobId")]
        //public JobDb Job { get; set; }
        public string JobName { get; set; }
    }
}
