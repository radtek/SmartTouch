using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
  
    public class SendTextViewModel
    {
        //public List<string> To { get; set; }
        public IEnumerable<dynamic> Contacts { get; set; }
        public string To { get; set; }
        // public MailCredentials credentials { get; set; }
        public string From { get; set; }
        public string Body { get; set; }
        public int UserId { get; set; }
        public int AccountID { get; set; }
        public ServiceProviderViewModel ServiceProvider { get; set; }
        public IEnumerable<dynamic> FromPhones { get; set; }
        // public ICollection<EmailContacts> EmailContacts;
    }
}
