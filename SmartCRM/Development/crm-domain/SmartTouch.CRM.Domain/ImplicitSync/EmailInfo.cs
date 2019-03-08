using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ImplicitSync
{
    public class EmailInfo
    {
        public IEnumerable<string> To { get; set; }
        public IEnumerable<string> CC { get; set; }
        public IEnumerable<string> BCC { get; set; }
        public string EmailType { get; set; }
        public DateTime SentDate { get; set; }
        public string Subject { get; set; }
        public string FromEmail { get; set; }
        public string Body { get; set; }
    }
}
