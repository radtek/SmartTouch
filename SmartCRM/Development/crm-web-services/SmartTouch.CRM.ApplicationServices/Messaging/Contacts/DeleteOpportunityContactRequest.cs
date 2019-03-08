using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class DeleteOpportunityContactRequest:ServiceRequestBase
    {
        public int OpportunityID { get; set; }
        public int ContactID { get; set; }
    }

    public class DeleteOpportunityContactResponse : ServiceResponseBase 
    {
      
    }
}
