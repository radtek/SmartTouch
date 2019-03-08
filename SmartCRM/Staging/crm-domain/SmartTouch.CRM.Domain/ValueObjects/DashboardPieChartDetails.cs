using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class DashboardPieChartDetails : ValueObjectBase
    {

        public string DropdownValue { get; set; }
        public short DropdownValueID { get; set; }
        public int TotalCount { get; set; }
        public decimal? Potential { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }


}
