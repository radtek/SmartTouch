using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Workflows
{
    public class TrackActionLog : EntityBase<long>, IAggregateRoot
    {
        public long TrackActionLogID { get; set; }
        public long TrackActionID { get; set; }
        public string ErrorMessage { get; set; }

        protected override void Validate()
        {
            //for future use
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", TrackActionID, ErrorMessage);
        }
    }
}
