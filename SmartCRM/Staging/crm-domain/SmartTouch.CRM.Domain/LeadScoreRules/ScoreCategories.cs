using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.LeadScoreRules
{
    public class ScoreCategories : EntityBase<short>, IAggregateRoot
    {
        public string Name { get; set; }
        public byte? ModuleID { get; set; }
        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
