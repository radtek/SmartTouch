using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Contacts
{
    public class ContactEmailSummary
    {
        public string From { get; set; }
        public string Subject { get; set; }
        public DateTime SentOn { get; set; }
        public int SentMailDetailID { get; set; }
        public bool Opened { get; set; }
        public int Clicked { get; set; }
        public DateTime? ActivityDate { get; set; }
    }
}
