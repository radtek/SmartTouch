using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Contacts
{
    public interface IContactRelationshipRepository : IRepository<ContactRelationship, int>
    {
        void DeleteConatactRelationShip(int contactRelationShipMapID);
        List<ContactRelationship> FindContactRelationship(int contactID);
        bool IsDuplicateContactRelationship(ContactRelationship contactRelationship);
    }
}
