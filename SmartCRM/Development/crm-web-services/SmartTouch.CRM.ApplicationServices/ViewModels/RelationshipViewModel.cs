using System.Collections.Generic;
using SmartTouch.CRM.Entities;
using System;
using SmartTouch.CRM.Domain.ValueObjects;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class RelationshipEntry
    {
        public int ContactRelationshipMapID { get; set; }
        public int ContactId { get; set; }
        public string ContactName { get; set; }
        //public RelationshipTypes RelationshipType { get; set; }
        public IEnumerable<dynamic> RelationshipTypes { get; set; }
        public Int16 RelationshipType { get; set; }
        public int? RelatedUserID { get; set; }
        public int? RelatedContactID { get; set; }
        public byte RelatedContactType { get; set; }
        public string DisplayContact { get; set; }
        public string DisplayRelationShipTypeValues { get; set; }
        public string RelatedContact { get; set; }
        public string DisplayRelatedUser { get; set; }
        public string RelationshipTypeName { get; set; }
               
    }
    /// <summary>
    /// Contact relationship
    /// </summary>
    public class RelationshipViewModel 
    {
        public IList<RelationshipEntry> Relationshipentry { get; set; }
        public IEnumerable<dynamic> RelationshipTypes { get; set; }
      
        public IList<ContactEntry> Contacts { get; set; }
        public IList<ContactEntry> RelatedContacts { get; set; }
        public int? RelatedContactID { get; set; }
        public string RelatedContact { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public Int16 RelationshipType { get; set; }
        public string RelationshipTypeName { get; set; }
        public bool SelectAll { get; set; }  
    }
}
