using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Login
{
    public class ThirdPartyClient : EntityBase<string>, IAggregateRoot 
    {        
        public string ID { get; set; }
        public int AccountID { get; set; }
        public string Name { get; set; }
        public string AccountName { get; set; }
        public bool IsActive { get; set; }
        public int RefreshTokenLifeTime { get; set; }
        public string AllowedOrigin { get; set; }
        public int LastUpdatedBy { get; set; }
        public DateTime LastUpdatedOn { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
