using AutoMapper;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class ContactRelationshipRepository : Repository<ContactRelationship, int, ContactRelationshipDb>, IContactRelationshipRepository
    {
        public ContactRelationshipRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory) { }

        public override ContactRelationship FindBy(int id)
        {
            ContactRelationshipDb contactRelationshipDatabase = GetContactRelationshipDb(id);

            if (contactRelationshipDatabase != null)
            {
                ContactRelationship contactRelationshipDatabaseConvertedToDomain = ConvertToDomain(contactRelationshipDatabase);
                return contactRelationshipDatabaseConvertedToDomain;
            }
            return null;
        }
        public List<ContactRelationship> FindContactRelationship(int contactID)
        {
            List<ContactRelationshipDb> contactRelationshipDbList = GetContactRelationshipList(contactID);
            List<ContactRelationship> contactRelationshipList = new List<ContactRelationship>();
            foreach (ContactRelationshipDb contactRelationshipDb in contactRelationshipDbList)
            {
                if (contactRelationshipDb != null)
                {
                    if (contactRelationshipDb.RelatedContactID == contactID)
                        contactRelationshipDb.RelatedContact.ContactType = contactRelationshipDb.Contact.ContactType;
                    contactRelationshipList.Add(ConvertToDomain(contactRelationshipDb));
                }
            }
            return contactRelationshipList;
        }
        public override ContactRelationshipDb ConvertToDatabaseType(ContactRelationship domainType, CRMDb context)
        {
            ContactRelationshipDb contactRelationshipMapDb;
            if (domainType.Id > 0)
            {
                contactRelationshipMapDb = context.ContactRelationshipMap.Where(r => r.ContactRelationshipMapID == domainType.Id).SingleOrDefault();
                if (contactRelationshipMapDb == null)
                    throw new ArgumentException("Invalid ContactRelationShipMapId is has been passed. Suspected Id forgery.");
                else
                {
                    contactRelationshipMapDb.RelationshipType = domainType.RelationshipTypeID;
                    contactRelationshipMapDb.RelatedContactID = domainType.RelatedContactID;
                    contactRelationshipMapDb.RelatedUserID = domainType.RelatedUserID;
                }
            }
            else

                contactRelationshipMapDb = Mapper.Map<ContactRelationship, ContactRelationshipDb>(domainType);
            return contactRelationshipMapDb;
        }

        public override ContactRelationship ConvertToDomain(ContactRelationshipDb databaseType)
        {
            return Mapper.Map<ContactRelationshipDb, ContactRelationship>(databaseType);
        }

        public override void PersistValueObjects(ContactRelationship domainType, ContactRelationshipDb dbType, CRMDb context)
        {
            //for future use
            //  persistContactRelationShip(domainType, dbType, context);
        }
        private void persistContactRelationShip(ContactRelationship contactRelationship, ContactRelationshipDb contactRelationShipDb, CRMDb db)
        {
            if (contactRelationship.Id == 0)
            {
                //var relationShipContact = db.ContactRelationshipMap.Where(n => n.ContactID == contactRelationship.ContactId && n.RelationshipType == contactRelationship.RelationshipTypeID &&
                //    n.RelatedContactID == contactRelationship.RelatedContactID);
                // if(relationShipContact!=null)

            }
            // var noteOpportunity = db.OpportunityNoteMap.Where(n => n.NoteID == note.Id).FirstOrDefault();


            //foreach (Contact contact in contactRelationShip)
            //{
            //    if (noteContacts.Count(n => n.ContactID == contact.Id) == 0)
            //    {
            //        db.ContactNotes.Add(new ContactNoteMapDb() { NoteID = notesDb.NoteID, ContactID = contact.Id });
            //    }
            //}

            //IList<int> contactIds = note.Contacts.Where(n => n.Id > 0).Select(n => n.Id).ToList();
            //var unMapNoteContacts = noteContacts.Where(n => !contactIds.Contains(n.ContactID));
            //foreach (ContactNoteMapDb noteContactMapDb in unMapNoteContacts)
            //{
            //    db.ContactNotes.Remove(noteContactMapDb);
            //}
        }
        public void DeleteConatactRelationShip(int contactRelationShipMapID)
        {
            throw new NotImplementedException();
        }

        ContactRelationshipDb GetContactRelationshipDb(int id)
        {
            var db = ObjectContextFactory.Create();
            try
            {
                var contactRelationship = db.ContactRelationshipMap.Include(c => c.RelatedContact).Include(c => c.DropdownValues).Include(c => c.Contact).SingleOrDefault(c => c.ContactRelationshipMapID == id);

                return contactRelationship;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while getting form.", ex);
                return null;
            }
        }
        List<ContactRelationshipDb> GetContactRelationshipList(int contactid)
        {

            var db = ObjectContextFactory.Create();
            List<ContactRelationshipDb> contactRelationshiplist = new List<ContactRelationshipDb>();
            try
            {
                contactRelationshiplist = db.ContactRelationshipMap.Where(w => w.ContactID == contactid || w.RelatedContact.ContactID == contactid).Include(i => i.DropdownValues)
                    .Include(i => i.RelatedContact).Include(i => i.Contact).ToList();

                return contactRelationshiplist;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while getting relationship details of a contact.", ex);
                throw ex;
            }
        }
        public IEnumerable<ContactRelationship> FindAll()
        {
            throw new NotImplementedException();
        }
        public bool IsDuplicateContactRelationship(ContactRelationship contactRelationship)
        {
            var db = ObjectContextFactory.Create();
            int relationcount=0;
            try
            {
                if (contactRelationship.Id == 0)
                {
                    IQueryable<ContactRelationshipDb> contactRelation = db.ContactRelationshipMap.Where(c => c.ContactID == contactRelationship.ContactId && c.RelatedContactID == contactRelationship.RelatedContactID && c.RelationshipType == contactRelationship.RelationshipTypeID);
                    relationcount = contactRelation.Count();
                }
                else
                {
                    IQueryable<ContactRelationshipDb> contactRelation = db.ContactRelationshipMap.Where(c => c.ContactID == contactRelationship.ContactId && 
                        c.RelatedContactID == contactRelationship.RelatedContactID && c.RelationshipType == contactRelationship.RelationshipTypeID&&
                        c.ContactRelationshipMapID!=contactRelationship.Id);
                     relationcount = contactRelation.Count();
                }
                return relationcount > 0;                    
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while getting ontactRelations.", ex);
                return true;
            }
        }


    }
}
