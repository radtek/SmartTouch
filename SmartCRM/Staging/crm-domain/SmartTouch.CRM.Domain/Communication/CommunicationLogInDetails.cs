using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Communication
{
    public class CommunicationLogInDetails : EntityBase<int>, IAggregateRoot
    {
        //public int CommunicationLogID { get; set; }
        public CommunicationType CommunicationTypeID { get; set; }
        public System.Guid LoginToken { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public int UserID { get; set; }
        protected override void Validate()
        { }
    }
}
