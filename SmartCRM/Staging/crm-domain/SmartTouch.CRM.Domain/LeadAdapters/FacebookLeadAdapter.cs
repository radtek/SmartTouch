using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.LeadAdapters
{
    public class FacebookLeadAdapter : EntityBase<int>, IAggregateRoot
    {
        public string PageAccessToken { get; set; }
        public string AddID { get; set; }
        public string Name { get; set; }
        public long PageID { get; set; }
        public int LeadAdapterAndAccountMapID { get; set; }
        public LeadAdapterAndAccountMap AccountMap { get; set; }
        public DateTime? TokenUpdatedOn { get; set; }
        public string UserAccessToken { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
