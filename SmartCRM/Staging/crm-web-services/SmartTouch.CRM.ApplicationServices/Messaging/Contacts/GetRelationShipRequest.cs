using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetRelationShipRequest : ServiceRequestBase
    {
        public int ContactRelationshipId { get; set; }     
    }

    public class GetRelationshipResponse : ServiceResponseBase
    {
        public RelationshipViewModel RelationshipViewModel { get; set; }
       
    }
}
