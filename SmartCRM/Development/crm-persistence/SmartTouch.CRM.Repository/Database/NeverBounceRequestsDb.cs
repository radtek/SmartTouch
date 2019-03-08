using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class NeverBounceRequestDb
    {
        [Key]
        public int NeverBounceRequestID { get; set; }
        public int? ReviewedBy { get; set; }
        public DateTime? ReviewedOn { get; set; }
        public short ServiceStatus { get; set; }
        public int AccountID { get; set; }
        public string Remarks { get; set; }
        public int? EmailsCount { get; set; }
        public DateTime? ScheduledPollingTime { get; set; }
        public int? NeverBounceJobID { get; set; }

        public string PollingRemarks { get; set; }
        public byte? PollingStatus { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
    }
}
