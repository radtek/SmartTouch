using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LandmarkIT.Enterprise.CommunicationManager.Database
{
    public class SendTextQueueDb
    {
        [Key]
        public int SendTextQueueID { get; set; }
        public Guid TokenGuid { get; set; }
        public Guid RequestGuid { get; set; }
        public DateTime? ScheduledTime { get; set; }
        public DateTime QueueTime { get; set; }
    }
}
