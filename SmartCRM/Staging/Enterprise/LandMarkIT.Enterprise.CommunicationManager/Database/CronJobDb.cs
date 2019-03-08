using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LandmarkIT.Enterprise.CommunicationManager.Database
{
    public class CronJobDb
    {
        [Key]
        public CronJobType CronJobID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Expression { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastRunOn { get; set; }
        public bool IsRunning { get; set; }
        public DateTime? LastNotifyDateTime { get; set; }
        public short EstimatedTimeInMin { get; set; }
        public bool IsSqlJob { get; set; }
        public Guid JobUniqueId { get; set; }

        public ICollection<CronJobHistoryDb> JobHistory { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - {1}", CronJobID, Name);
        }
    }
}
