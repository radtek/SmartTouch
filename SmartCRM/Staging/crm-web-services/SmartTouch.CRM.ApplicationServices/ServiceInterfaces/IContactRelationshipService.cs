using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface IContactRelationshipService
    {
        SaveRelationshipResponse SaveRelationshipMap(SaveRelationshipRequest request);
        GetRelationshipResponse GetContactRelationship(int contactRelationMapID, int accountID);
        GetRelationshipResponse GetContactRelationships(int contactID);
    }
}
