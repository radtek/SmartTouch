using System;

using SmartTouch.CRM.Infrastructure.Domain;
using System.Collections.Generic;


namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class OpportunitySummary : ValueObjectBase
    {
        public int NumberOfWon { get; set; }
        public decimal ValueOfWon { get; set; }
        public int NumberOfPotential { get; set; }
        public decimal ValueOfPotential { get; set; }
        public IList<int> NumberOfWonOpportunities { get; set; }
        public IList<int> NumberOfPotentialOpportunities { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
