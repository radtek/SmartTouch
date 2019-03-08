using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Login
{
    public class ClientRefreshToken : EntityBase<string>, IAggregateRoot
    {
        public int IssuedTo { get; set; }
        public string ThirdPartyClientId { get; set; }
        public DateTime IssuedOn { get; set; }
        public DateTime ExpiresOn { get; set; }
        public string ProtectedTicket { get; set; }
        public int LastUpdatedBy { get; set; }
        public DateTime LastUpdatedOn { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
