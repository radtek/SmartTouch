using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.SuppressedEmails
{
    public class SuppressedDomain : SuppressionList
    {
        public string Domain { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
