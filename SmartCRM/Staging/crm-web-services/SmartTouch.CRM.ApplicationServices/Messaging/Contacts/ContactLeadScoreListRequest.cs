using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class ContactLeadScoreListRequest : ServiceRequestBase
    {

    }
    public class ContactLeadScoreListResponse : ServiceResponseBase
    {
        public List<ContactLeadScoreListViewModel> ContactLeadScorelist { get; set; }
    }
}
