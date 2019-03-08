using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Reports
{
    public class Report : EntityBase<int>, IAggregateRoot
    {
        public string ReportName { get; set; }
        public int? AccountID { get; set; }
        public DateTime? LastRunOn { get; set; }

        public Entities.Reports ReportType { get; set; }
        public int? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public int? CreatedOn { get; set; }
        public DateTime? CreatedBy { get; set; }

        protected override void Validate()
        {
        }
    }

    public class DashboardItems : EntityBase<byte>, IAggregateRoot
    {        
        public string Report { get; set; }
        public bool? Value { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
