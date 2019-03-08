using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ImportData
{
    public class NeverBounceEmailData : ValueObjectBase
    {
        public string AccountName { get; set; }
        public int AccountID { get; set; }
        public int ImportTotal { get; set; }
        public int BadEmails { get; set; }
        public int GoodEmails { get; set; }
        public string EntityNames { get; set; }
        public short EntityID { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
