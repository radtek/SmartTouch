using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class WhitePages : ValueObjectBase
    {
        public bool is_valid { get; set; }
        public string street_line_1 { get; set; }
        public string street_line_2 { get; set; }
        public string city { get; set; }
        public string postal_code { get; set; }
        public string zip4 { get; set; }
        public string last_sale_date { get; set; }
        public string total_value { get; set; }
        public IEnumerable<Whitepages_Owner> owners { get; set; }
        public IEnumerable<Whitepages_Owner> current_residents { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }

    public class Whitepages_Owner
    {
        public string id { get; set; }
        public string name { get; set; }
        public string age_range { get; set; }
        public string gender { get; set; }
        public IEnumerable<WhitePagesPhone> phones { get; set; }
    }

    public class WhitePagesPhone
    {
        public string phone_number { get; set; }
    }
}
