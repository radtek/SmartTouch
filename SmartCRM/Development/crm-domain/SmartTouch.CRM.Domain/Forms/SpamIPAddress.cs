using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Forms
{
    public class SpamIPAddress
    {
        public string IPAddress { get; set; }
        public bool IsSpam { get; set; }
        public int AcountID { get; set; }
    }
}
