using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LandmarkIT.Enterprise.CommunicationManager.Database
{
    public class JobDb
    {
        [Key]
        public int JobId { get; set; }
        public string JobName { get; set; }
        public string Description { get; set; }
        public string ActivatorClass { get; set; }
        public bool IsActive { get; set; }
        
        //public ICollection<ScheduledJobDb> ScheduledJobs { get; set; }
    }
}
