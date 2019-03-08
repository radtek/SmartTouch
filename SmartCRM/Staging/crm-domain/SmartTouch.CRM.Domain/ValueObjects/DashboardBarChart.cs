using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class DashboardBarChart : ValueObjectBase
    {
        public string DropdownValue { get; set; }
        public short DropdownValueID { get; set; }
        public int TotalVisits { get; set; }
        public int UniqueVisitors { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
