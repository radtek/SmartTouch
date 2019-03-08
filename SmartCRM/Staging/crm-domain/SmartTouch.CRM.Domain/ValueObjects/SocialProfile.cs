using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class SocialProfile
    {
        public string bio { get; set; }
        public string type { get; set; }
        public string typeId { get; set; }
        public string typeName { get; set; }
        public string url { get; set; }
        public string id { get; set; }
        public string username { get; set; }
        public int? followers { get; set; }
        public int? following { get; set; }
    }
}
