using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class FirstLeadSourceReportResponse : ValueObjectBase
    {
        public int ID { get; set; }
        public string Name { get; set; }

        protected override void Validate()
        {
            //throw new NotImplementedException();
        }
    }
}
