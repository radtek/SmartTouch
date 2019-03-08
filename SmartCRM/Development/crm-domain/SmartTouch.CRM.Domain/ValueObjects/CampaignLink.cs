using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class CampaignLink : ValueObjectBase
    {
        public int CampaignLinkId { get; set; }
        public int CampaignId { get; set; }
        public Url URL { get; set; }
        public string Name { get; set; }
        public byte LinkIndex { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
