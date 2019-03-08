using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    class DataAccessPermission : ValueObjectBase
    {
        public int DataAccessPermissionID { get; set; }
        public int AccountID { get; set; }
        public byte ModuleID { get; set; }
        public bool IsPrivate { get; set; }

        protected override void Validate()
        {
        }
    }
}
