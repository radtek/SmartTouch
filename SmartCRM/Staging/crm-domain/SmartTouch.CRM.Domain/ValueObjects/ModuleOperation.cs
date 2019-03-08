using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.Subscriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Modules;


namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class ModuleOperation : ValueObjectBase
    {
        public int ModuleOperationId { get; set; }

        
        public virtual int ModuleId { get; set; }
        public virtual Module Module { get; set; }

        public virtual int OperationId { get; set; }
        public virtual Operation Operation { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
