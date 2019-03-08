using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Contacts
{
    public class EmailStatistics
    {
        public int Delivered { get; set; }
        public int Opened { get; set; }
        public int Clicked { get; set; }
    }
}
