using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Campaigns
{
    public class CampaignMailTester : ValueObjectBase
    {
        public int CampaignMailTestID { get; set; }
        public int CampaignID {get; set; }
        public Guid UniqueID { get; set; }
        public int Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastpdatedOn { get; set; }
        public string RawData { get; set; }
        public int CreatedBy { get; set; }

        public string Name { get; set; }
        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
