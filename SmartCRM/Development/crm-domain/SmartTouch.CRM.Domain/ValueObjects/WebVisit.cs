using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class WebVisit : EntityBase<int>,IAggregateRoot
    {
        public int ContactID { get; set; }
        public DateTime VisitedOn { get; set; }
        public string PageVisited { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string ISPName { get; set; }
        public short Duration { get; set; }
        public string IPAddress { get; set; }
        public bool IsVisit { get; set; }
        public string VisitReference { get; set; }
        public string ContactReference { get; set; }

        public virtual Contact VisitedBy { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }

}
