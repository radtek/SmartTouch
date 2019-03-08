using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Communication
{
   public class CommunicationTracker:EntityBase<int>,IAggregateRoot
    {
        //public long CommunicationTrackerID { get; set; }
        public bool? Address { get; set; }
        public System.Guid TrackerGuid { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public CommunicationStatus CommunicationStatusID { get; set; }
        public CommunicationType CommunicationTypeID { get; set; }
        public int? ContactID { get; set; }
        protected override void Validate()
        { }
    }
}
