using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ImportData
{
    public class NeverBounceRequest : EntityBase<int>, IAggregateRoot
    {
        public int NeverBounceRequestID { get; set; }
        public int? ReviewedBy { get; set; }
        public DateTime? ReviewedOn { get; set; }
        public NeverBounceStatus ServiceStatus { get; set; }

        //public int? LeadAdapterJobLogID { get; set; }
        public int AccountID { get; set; }
        public string Remarks { get; set; }
        public int? EmailsCount { get; set; }
        public DateTime? ScheduledPollingTime { get; set; }
        public int? NeverBounceJobID { get; set; }

        public string FileName { get; set; }
        public string PollingRemarks { get; set; }
        public byte PollingStatus { get; set; }
        public int CreatedBy { get; set; }

        public string EntityIds { get; set; }
        public NeverBounceEntityTypes EntityType { get; set; }
        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
