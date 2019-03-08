using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class Operation : ValueObjectBase
    {
        public int OperationId { get; set; }
        public string OperationName { get; set; }

       
        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
