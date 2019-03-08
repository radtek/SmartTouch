using AutoMapper;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Notes;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class NoteRepository : Repository<Note, int, NotesDb>, INoteRepository
    {
        public NoteRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory)
        { }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Note> FindAll()
        {
            var notes = ObjectContextFactory.Create().Notes;
            foreach (var noteDb in notes)
                yield return ConvertToDomain(noteDb);
        }

        /// <summary>
        /// Finds the tag.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <returns></returns>
        public Tag FindTag(string tagName)
        {
            var target = default(Tag);
            var tagDatabase = ObjectContextFactory.Create().Tags.FirstOrDefault(c => c.TagName.Equals(tagName));
            if (tagDatabase != null) target = ConvertToDomain(tagDatabase);
            return target;
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="tagDatabase">The tag database.</param>
        /// <returns></returns>
        private Tag ConvertToDomain(TagsDb tagDatabase)
        {
            return new Tag()
            {
                Id = tagDatabase.TagID,
                TagName = tagDatabase.TagName,
                Description = tagDatabase.Description,
                Count = tagDatabase.Count
            };
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="noteId">The note identifier.</param>
        /// <returns></returns>
        public override Note FindBy(int noteId)
        {
            var db = ObjectContextFactory.Create();
            NotesDb notesDb = db.Notes.SingleOrDefault(c => c.NoteID == noteId);
            var tags = db.NoteTags.Include(n => n.Tags).Where(n => n.NoteID == noteId).Select(n => n.Tags).ToList();
            var contacts = db.ContactNotes.Include(n => n.Contact).Where(n => n.NoteID == noteId)
                  .Select(a => a.Contact).ToList();
            notesDb.Contacts = contacts.Where(c => c.IsDeleted == false).ToList();
            notesDb.Tags = tags;

            if (notesDb != null)
                return ConvertToDomain(notesDb);
            return null;
        }

        /// <summary>
        ///  Finds the by.
        /// </summary>
        /// <param name="noteId"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public Note FindNoteById(int noteId, int accountId)
        {
            var db = ObjectContextFactory.Create();
            NotesDb notesDb = db.Notes.Where(n => n.NoteID == noteId && n.AccountID == accountId).FirstOrDefault();
            var tags = db.NoteTags.Include(n => n.Tags).Where(n => n.NoteID == noteId).Select(n => n.Tags).ToList();
            var contacts = db.ContactNotes.Include(n => n.Contact).Where(n => n.NoteID == noteId)
                  .Select(a => a.Contact).ToList();
            notesDb.Contacts = contacts.Where(c => c.IsDeleted == false).ToList();
            notesDb.Tags = tags;

            if (notesDb != null)
                return ConvertToDomain(notesDb);
            return null;
        }

        /// <summary>
        /// Get Note by NoteId. Will include contacts only if tags were applied to Note.
        /// </summary>
        /// <param name="noteId"></param>
        /// <returns></returns>
        public Note GetNoteById(int noteId,int accountId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                NotesDb notesDb = db.Notes.Where(n => n.NoteID == noteId && n.AccountID == accountId).FirstOrDefault();
                var noteTags = db.NoteTags.Include(n => n.Tags).Where(n => n.NoteID == noteId).Select(n => n.Tags).ToList();
                if (noteTags.Count() > 0)
                {
                    var contacts = db.ContactNotes.Include(n => n.Contact).Where(n => n.NoteID == noteId)
                                        .Select(a => a.Contact).ToList();
                    notesDb.Contacts = contacts.Where(c => c.IsDeleted == false).ToList();

                }
                notesDb.Tags = noteTags;
                if (notesDb != null)
                    return ConvertToDomain(notesDb);
                return null;
            }
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="noteId">The note identifier.</param>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public Note FindBy(int noteId, int contactId,int accountId)
        {
            var db = ObjectContextFactory.Create();
            NotesDb notesDb = db.Notes.Where(n => n.NoteID == noteId && n.AccountID == accountId).FirstOrDefault();
            var tags = db.NoteTags.Include(n => n.Tags).Where(n => n.NoteID == noteId).Select(n => n.Tags).ToList();

            var tagids = tags.Select(p => p.TagID).ToArray();

            var leadscoreTags = db.LeadScoreRules.Where(l => l.IsActive == true && (l.ConditionID == 6 || l.ConditionID == 7) &&
            tagids.Select(s => s.ToString()).Contains(l.ConditionValue)).Select(s => s.ConditionValue).ToArray();


            foreach (TagsDb tag in tags)
            {
                tag.TagName = tag.TagName + (leadscoreTags.Contains(tag.TagID.ToString()) ? " *" : "");
            }


            notesDb.Tags = tags;

            if (notesDb.SelectAll == false)
            {
                var contacts = db.ContactNotes.Include(n => n.Contact).Where(n => n.NoteID == noteId)
                    .Select(a => a.Contact).ToList();
                notesDb.Contacts = contacts.Where(c => c.IsDeleted == false).ToList();
            }
            if (notesDb != null)
                return ConvertToDomain(notesDb);
            return null;
        }

        /// <summary>
        /// Finds the by contact.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public IEnumerable<Note> FindByContact(int contactId,int accountId)
        {
            var db = ObjectContextFactory.Create();
            var contactNotes = db.ContactNotes.Where(c => c.ContactID == contactId).Select(a => a.NoteID).ToList();

            var actions = db.Notes.Where(a => contactNotes.Contains(a.NoteID) && a.AccountID == accountId);

            if (actions != null)
            {
                foreach (var item in actions)
                {
                    yield return ConvertToDomain(item);
                }
            }
        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="note">The note.</param>
        /// <param name="db">The database.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Invalid note id has been passed. Suspected Id forgery.</exception>
        public override NotesDb ConvertToDatabaseType(Note note, CRMDb db)
        {
            NotesDb notesDb;

            if (note.Id > 0)
            {
                notesDb = db.Notes.Where(n => n.NoteID == note.Id && n.AccountID == note.AccountId).FirstOrDefault();
                if (notesDb == null)
                    throw new ArgumentException("Invalid note id has been passed. Suspected Id forgery.");
                notesDb = Mapper.Map<Note, NotesDb>(note, notesDb);
            }
            else
            {
                notesDb = Mapper.Map<Note, NotesDb>(note);
            }
            return notesDb;
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="db">The database.</param>
        public override void PersistValueObjects(Note domainType, NotesDb dbType, CRMDb db)
        {
            persistContactNotes(domainType, dbType, db);
            persistNoteTags(domainType, dbType, db);
        }

        /// <summary>
        /// Persists the contact notes.
        /// </summary>
        /// <param name="note">The note.</param>
        /// <param name="notesDb">The notes database.</param>
        /// <param name="db">The database.</param>
        private void persistContactNotes(Note note, NotesDb notesDb, CRMDb db)
        {
            var noteOpportunity = db.OpportunityNoteMap.Where(n => n.NoteID == note.Id).FirstOrDefault();
            if (note.OppurtunityId != 0 && noteOpportunity == null)
            {
                db.OpportunityNoteMap.Add(new OpportunityNoteMap() { OpportunityID = note.OppurtunityId, NoteID = notesDb.NoteID });
            }
            List<ContactNoteMapDb> ContactNoteMap = new List<ContactNoteMapDb>();
            if (note.SelectAll == false)
            {
                if (note.Id == 0)
                {
                    foreach (Contact contact in note.Contacts)
                        ContactNoteMap.Add(new ContactNoteMapDb() { NoteID = notesDb.NoteID, ContactID = contact.Id });
                    db.ContactNotes.AddRange(ContactNoteMap);
                }
                else
                {
                    var noteContacts = db.ContactNotes.Where(n => n.NoteID == note.Id);
                    var contactIds = note.Contacts.Where(n => n.Id > 0).Select(n => n.Id);
                    var contactstoinsert = contactIds.Except(noteContacts.Select(x => x.ContactID));
                    foreach (int contactid in contactstoinsert)
                        ContactNoteMap.Add(new ContactNoteMapDb() { NoteID = notesDb.NoteID, ContactID = contactid });
                    db.ContactNotes.AddRange(ContactNoteMap);
                    var unMapNoteContacts = noteContacts.Where(n => !contactIds.Contains(n.ContactID));
                    db.ContactNotes.RemoveRange(unMapNoteContacts);
                }
            }
        }

        /// <summary>
        /// Persists the note tags.
        /// </summary>
        /// <param name="note">The note.</param>
        /// <param name="notesDb">The notes database.</param>
        /// <param name="db">The database.</param>
        void persistNoteTags(Note note, NotesDb notesDb, CRMDb db)
        {
            var noteTags = db.NoteTags.Where(a => a.NoteID == note.Id).ToList();

            foreach (Tag tag in note.Tags)
            {
                if (tag.Id == 0)
                {
                    var tagDb = db.Tags.Where(t => t.TagName.Equals(tag.TagName) && t.AccountID == tag.AccountID && t.IsDeleted != true).FirstOrDefault();
                    if (tagDb == null)
                    {
                        tagDb = Mapper.Map<Tag, TagsDb>(tag);
                        tagDb.IsDeleted = false;
                        tagDb = db.Tags.Add(tagDb);
                    }
                    var noteTag = new NoteTagsMapDb()
                    {
                        Note = notesDb,
                        Tags = tagDb
                    };

                    db.NoteTags.Add(noteTag);
                }
                else if (!noteTags.Any(a => a.TagID == tag.Id))
                {
                    db.NoteTags.Add(new NoteTagsMapDb() { NoteID = notesDb.NoteID, TagID = tag.Id });
                    db.RefreshAnalytics.Add(new RefreshAnalyticsDb() { EntityID = tag.Id, EntityType = 5, Status = 1, LastModifiedOn = DateTime.Now.ToUniversalTime() });
                }
            }

            IList<int> tagIds = note.Tags.Where(a => a.Id > 0).Select(a => a.Id).ToList();
            var unMapNoteTags = noteTags.Where(a => !tagIds.Contains(a.TagID));
            foreach (NoteTagsMapDb noteTagMapDb in unMapNoteTags)
            {
                db.NoteTags.Remove(noteTagMapDb);
                db.RefreshAnalytics.Add(new RefreshAnalyticsDb() { EntityID = noteTagMapDb.TagID, EntityType = 5, Status = 1, LastModifiedOn = DateTime.Now.ToUniversalTime() });
            }
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="noteDb">The note database.</param>
        /// <returns></returns>
        public override Note ConvertToDomain(NotesDb noteDb)
        {
            return Mapper.Map<NotesDb, Note>(noteDb);
        }

        /// <summary>
        /// Deletes the note.
        /// </summary>
        /// <param name="noteId">The note identifier.</param>
        public IEnumerable<int> DeleteNote(int noteId, int contactId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var contactsRelated = new List<int>();
                var procedureName = @"[dbo].[DeleteNote]";
                var parms = new List<SqlParameter>
                {                   
                    new SqlParameter{ParameterName ="@noteId", Value= noteId}
                };
                contactsRelated = db.ExecuteStoredProcedure<int>(procedureName, parms).ToList();
                return contactsRelated;
            }
        }

        public bool IsNoteFromSelectAll(int noteId,int accountId)
        {
            var db = ObjectContextFactory.Create();
            bool selectAll = db.Notes.Where(p => p.NoteID == noteId && p.AccountID == accountId).Select(p => p.SelectAll).FirstOrDefault();
            return selectAll;
        }

        /// <summary>
        /// Contactses the count.
        /// </summary>
        /// <param name="noteId">The note identifier.</param>
        /// <returns></returns>
        public int ContactsCount(int noteId)
        {
            var db = ObjectContextFactory.Create();
            int contactsCount = db.ContactNotes.Where(a => a.NoteID == noteId).Select(a => a.Contact).Count();
            return contactsCount;
        }

        /// <summary>
        /// Getting Action Details NoteCategoryId
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="dropdownValueTypeId"></param>
        /// <param name="dropdownFieldTypeId"></param>
        /// <returns></returns>
        public short GetActionDetailsNoteCategoryID(int accountId, short dropdownValueTypeId, byte dropdownFieldTypeId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"SELECT DropdownValueID FROM DropdownValues (NOLOCK) WHERE AccountID=@AccountId AND DropdownValueTypeID=@DropdownValueTypeId AND DropdownID=@DropdownId AND IsDeleted=0";
                return db.Get<short>(sql, new { AccountId = accountId, DropdownValueTypeId = dropdownValueTypeId, DropdownId = dropdownFieldTypeId }).FirstOrDefault();
            }
        }
    }
}

