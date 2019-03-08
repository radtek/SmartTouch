using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class DashboardAreaChart : ValueObjectBase
    {
        public Byte DateNo { get; set; }
        public int Present { get; set; }
        public int Previous { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
