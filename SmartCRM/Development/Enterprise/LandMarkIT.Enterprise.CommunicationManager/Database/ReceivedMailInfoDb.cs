using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace LandmarkIT.Enterprise.CommunicationManager.Database
{
    public class ReceivedMailInfoDb
    {
        [Key]
        public int ReceivedMailID { get; set; }
        public string FromEmail { get; set; }
        public string Recipient { get; set; }
        public EmailRecipientType RecipientType { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string ReferenceID { get; set; }
        public DateTime ReceivedOn { get; set; }
        public DateTime TrackedOn { get; set; }
        public string EmailReferenceID { get; set; }

    }
}
