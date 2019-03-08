using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Contacts
{
    public class ContactRelationship : EntityBase<int>, IAggregateRoot, IEquatable<ContactRelationship>
    {
        public int ContactId { get; set; }
        public short RelationshipTypeID { get; set; }
        public int? RelatedContactID { get; set; }
        public int? RelatedUserID { get; set; }
        public Person RelatedContact { get; set; }
        public User RelatedUser { get; set; }
        public string RelationshipName { get; set; }
        public ContactType RelatedContactType { get; set; }
        public string ContactName { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool SelectAll { get; set; }  

        protected override void Validate()
        {
        }

        public override int GetHashCode()
        {
            int result = 29;
            result = result * 13 + ContactId.GetHashCode();

            if(RelatedContactID.HasValue)
                result = result * 13 + RelatedContactID.Value.GetHashCode();
            else if(RelatedUserID.HasValue)
                result = result * 13 + RelatedUserID.Value.GetHashCode();

            return result;
        }

        public override bool Equals(object entity)
        {
            return Equals(entity as ContactRelationship);
        }
        public bool Equals(ContactRelationship other)
        {
            if (other == null) return false;
                
            if (other.RelatedContactID.HasValue && this.RelatedContactID.HasValue)
            {
                if (other.ContactId == this.ContactId && other.Id == this.Id
                                   && other.RelatedContactID.Value == this.RelatedContactID.Value
                                   && other.RelationshipTypeID == this.RelationshipTypeID)
                    return true;
                else
                    return false;
            }
            else if (other.RelatedUserID.HasValue && this.RelatedUserID.HasValue)
            {
                if (other.ContactId == this.ContactId && other.Id == this.Id
                                   && other.RelatedUserID.Value == this.RelatedUserID.Value
                                   && other.RelationshipTypeID == this.RelationshipTypeID)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
    }
}
