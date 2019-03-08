using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class PDFNotificationDetails
    {
        public string AccountName { get; set; }
        public string EntityName { get; set; }
        public string AccountLogoURL { get; set; }
        public string AccountAddress { get; set; }
        public string AccountPhone { get; set; }
    }
}
