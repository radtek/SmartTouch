using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Dropdowns
{
    public class OpportunityGroups : EntityBase<byte>, IAggregateRoot
    {
        public int OpportunityGroupID { get; set; }
        public string OpportunityGroupName { get; set; }
        protected override void Validate()
        {
            throw new System.NotImplementedException();
        }
    }
}
