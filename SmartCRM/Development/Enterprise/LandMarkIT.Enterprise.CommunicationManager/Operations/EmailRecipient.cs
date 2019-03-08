using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailChimp.Lists;
using SmartTouch.CRM.Domain.Contacts;


namespace LandmarkIT.Enterprise.CommunicationManager.Operations
{   
    public class EmailRecipient
    {        
        public int ContactId { get; set; }
        public string EmailId { get; set; }
        public int CampaignRecipientID { get; set; }
        public Contact ContactInfo { get; set; }
        public IDictionary<string, string> ContactFields { get; set; }
    }
}
