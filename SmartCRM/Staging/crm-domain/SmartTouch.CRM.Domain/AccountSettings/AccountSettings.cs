using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.AccountSettings
{
    public class AccountSettings : EntityBase<int>, IAggregateRoot
    {
      
        public int AccountSettingsID { get; set; }        
        public short StatusID { get; set; }
        public string ViewName { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
