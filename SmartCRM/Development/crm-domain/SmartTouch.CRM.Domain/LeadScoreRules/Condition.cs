using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.LeadScoreRules
{
    public class Condition : EntityBase<byte>, IAggregateRoot
    {
        public string Name { get; set; }
        public ScoreCategories Category { get; set; }
        public short ScoreCategoryID { get; set; }
        public byte? ModuleID { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
