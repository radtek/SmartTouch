using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Modules
{
    public class Module : EntityBase<byte>, IAggregateRoot
    {
        public string ModuleName { get; set; }
        public bool IsInternal { get; set; }
        public byte ParentID { get; set; }
       
        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
