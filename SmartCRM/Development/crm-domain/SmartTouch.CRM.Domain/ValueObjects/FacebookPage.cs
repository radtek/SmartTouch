using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class FacebookPage : ValueObjectBase
    {
        public string access_token { get; set; }
        public string name { get; set; }
        public long id { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }

    public class FacebookPageData
    {
        public IEnumerable<FacebookPage> data { get; set; }
    }
}
