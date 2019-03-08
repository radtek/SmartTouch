using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class Photo
    {
        public string type { get; set; }
        public string typeId { get; set; }
        public string typeName { get; set; }
        public string url { get; set; }
        public bool isPrimary { get; set; }
        public string photoBytesMD5 { get; set; }
    }
}
