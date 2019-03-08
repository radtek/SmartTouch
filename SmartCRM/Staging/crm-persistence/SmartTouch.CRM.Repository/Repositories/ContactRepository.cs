using AutoMapper;
using Dapper;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.Logging;
using LinqKit;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Communications;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Domain.Images;
using SmartTouch.CRM.Domain.ImplicitSync;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class ContactRepository : Repository<Contact, int, ContactsDb>, IContactRepository
    {
        public ContactRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory)
        { }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Contact> FindAll()
        {
            IEnumerable<ContactsDb> contacts = ObjectContextFactory.Create().Contacts
                .Include(c => c.Addresses).Include(c => c.Communication).Include(c => c.ContactEmails)
                .Include(c => c.ContactPhones).Include(c => c.Image).Where(c => !c.IsDeleted).AsNoTracking();//.Take(100000);

            foreach (ContactsDb dc in contacts)
            {
                dc.ContactEmails = dc.ContactEmails.Where(i => i.IsDeleted == false).ToList();
                dc.ContactPhones = dc.ContactPhones.Where(p => p.IsDeleted == false).ToList();
                if (dc.ContactType == ContactType.Person)
                    yield return Mapper.Map<ContactsDb, Person>(dc);
                else
                    yield return Mapper.Map<ContactsDb, Company>(dc);
            }
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="limit">The limit.</param>
        /// <returns></returns>
        public IEnumerable<Contact> FindAll(int pageNumber, int limit)
        {
            using (var db = ObjectContextFactory.Create())
            {
                IList<Contact> contacts = new List<Contact>();

                IEnumerable<ContactsDb> contactDbs = db.Contacts
                    .Include(c => c.Addresses).Include(c => c.Communication).Include(c => c.ContactEmails)
                    .Include(c => c.ContactPhones).Include(c => c.Image).Include(c => c.ContactLeadSources)
                    .Include(c => c.CustomFields.Select(cf => cf.CustomField))
                    .Where(c => !c.IsDeleted)
                    .OrderBy(c => c.ContactID)
                    .Skip(limit * pageNumber)
                    .Take(limit).AsNoTracking().ToList();

                short active = (short)FieldStatus.Active;
                var customFieldIds = db.Fields.Where(f => f.StatusID.Value == active).Select(f => f.FieldID).ToList();

                var contactIds = contactDbs.Select(c => c.ContactID).ToList();
                var results = db.ContactsAudit
                    .Where(c => c.AuditAction == "I" && contactIds.Contains(c.ContactID))
                    .Select(s => new ContactCreatorInfo()
                    {
                        ContactId = s.ContactID,
                        CreatedOn = s.LastUpdatedOn,
                        CreatedBy = s.LastUpdatedBy
                    }).ToList();


                foreach (ContactsDb dc in contactDbs)
                {
                    dc.ContactEmails = dc.ContactEmails.Where(i => i.IsDeleted == false).ToList();
                    dc.ContactPhones = dc.ContactPhones.Where(p => p.IsDeleted == false).ToList();
                    dc.CustomFields = dc.CustomFields.Where(cf => customFieldIds.Contains(cf.CustomFieldID)).ToList();
                    var result = results.FirstOrDefault(r => r.ContactId == dc.ContactID);

                    if (dc.ContactType == ContactType.Person)
                    {
                        var person = Mapper.Map<ContactsDb, Person>(dc);
                        if (result != null && result.CreatedOn.HasValue)
                            person.CreatedOn = result.CreatedOn.Value;

                        if (result != null)
                            person.CreatedBy = result.CreatedBy;

                        contacts.Add(person);
                    }
                    else
                    {
                        var company = Mapper.Map<ContactsDb, Company>(dc);
                        if (result != null && result.CreatedOn.HasValue)
                            company.CreatedOn = result.CreatedOn.Value;

                        if (result != null)
                            company.CreatedBy = result.CreatedBy;

                        contacts.Add(company);
                    }
                }
                return contacts;
            }
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Contact> FindAll(int pageNumber, int limit, int accountId, int lastIndexedContact)
        {
            using (var db = ObjectContextFactory.Create())
            {
                IList<Contact> contacts = new List<Contact>();
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                if (lastIndexedContact == 0)
                    lastIndexedContact = db.Contacts.Where(c => c.AccountID == accountId).OrderByDescending(c => c.ContactID).Take(1).Select(s => s.ContactID).FirstOrDefault();

                Console.WriteLine("Fetching Contacts: " + DateTime.Now.ToString());
                var sql = @"SELECT C.ContactID FROM Contacts (NOLOCK) C 
                            WHERE C.AccountID = @AccountID AND C.IsDeleted = 0 AND C.ContactID <= @LastIndexedContact
                            ORDER BY C.ContactID DESC
                            OFFSET 0 ROWS
                            FETCH NEXT @Take ROWS ONLY";
                var newDb = ObjectContextFactory.Create();
                var contactIds = newDb.Get<int>(sql, new { AccountID = accountId, LastIndexedContact = lastIndexedContact, Take = limit });

                if (contactIds.IsAny() && contactIds.Count() == 1 && lastIndexedContact == contactIds.FirstOrDefault())
                    return null;
                if (contactIds != null)
                    contacts = this.FindAll(contactIds.ToList()).ToList();
                Console.WriteLine("Fetched Additional Information: " + DateTime.Now.ToString());

                sw.Stop();
                var timeelapsed = sw.Elapsed;
                Logger.Current.Informational("Time elapsed to fetch contacts from Db:" + timeelapsed);
                return contacts;
            }
        }

        /// <summary>
        /// Gets the contact creators information.
        /// </summary>
        /// <param name="contactIds">The contact ids.</param>
        /// <returns></returns>
        public IEnumerable<ContactCreatorInfo> GetContactCreatorsInfo(IEnumerable<int> contactIds)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"select c.contactid as ContactId, coalesce(ca.lastupdatedon,getutcdate()) as CreatedOn, ca.lastupdatedby as CreatedBy from contacts_audit ca (nolock)
                        right outer join @tbl c on c.contactid = ca.contactid and ca.auditaction = 'I'";
            return db.Get<ContactCreatorInfo>(sql, new { tbl = contactIds.AsTableValuedParameter("dbo.LastTouchedDetails", new string[] { "ContactID", "LastTouchedDate", "ActionID" }) });
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="ContactIDs">The contact i ds.</param>
        /// <returns></returns>
        public IEnumerable<Contact> FindAll(IList<int> ContactIDs, bool isElastic = true)
        {
            Logger.Current.Informational(string.Format("Total Contacts {0}", ContactIDs.Count()));
            var ids = ContactIDs.Distinct();
            var db = ObjectContextFactory.Create();
            IEnumerable<ContactsDb> cs = null;
            IEnumerable<IDictionary<string, object>> addresses = null;
            IEnumerable<CommunicationsDb> communications = null;
            IEnumerable<ContactEmailsDb> contactEmails = null;
            IEnumerable<ContactPhoneNumbersDb> phones = null;
            IEnumerable<ImagesDb> images = null;
            IEnumerable<ContactLeadSourceMapDb> contactLeadSources = null;
            IEnumerable<ContactCustomFieldsDb> customFields = null;
            IEnumerable<DropdownValueDb> phonetypes = null;
            IEnumerable<StatesDb> addressStates = null;
            IEnumerable<CountriesDb> addressCountries = null;
            IEnumerable<WebVisitsDb> webVisits = null;
            IEnumerable<ContactTagMapDb> contactTags = null;
            IEnumerable<FormSubmissionDb> formSubmissions = null;
            IEnumerable<ContactCreatorInfo> createrInfo = null;
            IEnumerable<ContactCommunityMapDb> contactCommunities = null;
            IEnumerable<ContactNoteSummary> contactSummary = null;
            IEnumerable<ContactTourCommunityMap> contactTours = null;
            IEnumerable<ContactActionMap> contactActions = null;
            IEnumerable<ContactNoteMap> contactNotes = null;
            IEnumerable<ActiveContacts> activeContacts = null;

            string sp = "dbo.GET_Contact_FullData";
            if (isElastic)
                sp = "dbo.GET_ElasticSearch_ContactData";
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            db.QueryStoredProc(sp, (reader) =>
                 {
                     cs = reader.Read<ContactsDb>().ToList();
                     addresses = reader.Read().ToList().Cast<IDictionary<string, object>>();
                     communications = reader.Read<CommunicationsDb>().ToList();
                     contactEmails = reader.Read<ContactEmailsDb>().ToList();
                     phones = reader.Read<ContactPhoneNumbersDb>().ToList();
                     images = reader.Read<ImagesDb>().ToList();
                     contactLeadSources = reader.Read<ContactLeadSourceMapDb>().ToList();
                     customFields = reader.Read<ContactCustomFieldsDb>().ToList();
                     phonetypes = reader.Read<DropdownValueDb>().ToList();
                     addressStates = reader.Read<StatesDb>().ToList();
                     addressCountries = reader.Read<CountriesDb>().ToList();
                     webVisits = reader.Read<WebVisitsDb>().ToList();
                     contactTags = reader.Read<ContactTagMapDb>().ToList();
                     formSubmissions = reader.Read<FormSubmissionDb>().ToList();
                     createrInfo = reader.Read<ContactCreatorInfo>().ToList();
                     contactCommunities = reader.Read<ContactCommunityMapDb, DropdownValueDb, ContactCommunityMapDb>((c, d) =>
                         {
                             c.Community = d;
                             return c;
                         }, splitOn: "DropDownValueID").ToList();
                     contactSummary = reader.Read<ContactNoteSummary>().ToList();
                     if (isElastic)
                     {
                         contactTours = reader.Read<ContactTourCommunityMap>().ToList();
                         contactActions = reader.Read<ContactActionMap>().ToList();
                         contactNotes = reader.Read<ContactNoteMap>().ToList();
                         activeContacts = reader.Read<ActiveContacts>().ToList();
                     }
                 }, new { ContactsList = ids.AsTableValuedParameter("dbo.Contact_List") }, commandTimeout: 600);
            
            Func<IDictionary<string, object>, AddressesDb> GetAddress = (v) => new AddressesDb()
            {
                AddressID = (int)v["AddressID"],
                AddressLine1 = (string)v["AddressLine1"],
                AddressLine2 = (string)v["AddressLine2"],
                AddressTypeID = (short)v["AddressTypeID"],
                City = (string)v["City"],
                StateID = (string)v["StateID"],
                CountryID = (string)v["CountryID"],
                ZipCode = (string)v["ZipCode"],
                IsDefault = (bool?)v["IsDefault"],
                State = addressStates.Where(p => p.StateID == (string)v["StateID"]).FirstOrDefault(),
                Country = addressCountries.Where(p => p.CountryID == (string)v["CountryID"]).FirstOrDefault()
            };
            foreach (var c in cs)
            {
                if (addresses.IsAny())
                {
                    var adrs = addresses.Where(a => (int)a["ContactID"] == c.ContactID);
                    if (adrs.IsAny())
                        c.Addresses = new List<AddressesDb>();
                    foreach (var a in adrs)
                        c.Addresses.Add(GetAddress(a));
                }
                if (contactEmails.IsAny())
                {
                    c.ContactEmails = contactEmails.Where(e => e.ContactID == c.ContactID).ToList();
                    if (c.ContactEmails.IsAny())
                        c.PrimaryEmail = c.ContactEmails.Where(w => w.IsPrimary).Select(s => s.Email).FirstOrDefault();
                }
                if (communications.IsAny())
                    c.Communication = communications.Where(cm => cm.CommunicationID == c.CommunicationID).FirstOrDefault();
                if (phones.IsAny())
                {
                    c.ContactPhones = phones.Where(p => p.ContactID == c.ContactID).ToList();
                    foreach (ContactPhoneNumbersDb phdb in c.ContactPhones)
                        phdb.DropdownValues = phonetypes.Where(p => p.DropdownValueID == phdb.PhoneType).FirstOrDefault();
                }
                if (images.IsAny())
                    c.Image = images.Where(i => i.ImageID == c.ImageID).FirstOrDefault();
                if (contactLeadSources.IsAny())
                    c.ContactLeadSources = contactLeadSources.Where(cl => cl.ContactID == c.ContactID).ToList();
                if (customFields.IsAny())
                    c.CustomFields = customFields.Where(cf => cf.ContactID == c.ContactID).ToList();
                if (webVisits.IsAny())
                    c.WebVisits = webVisits.Where(w => w.ContactID == c.ContactID).ToList();
                if (formSubmissions.IsAny())
                    c.FormSubmissions = formSubmissions.Where(w => w.ContactID == c.ContactID).ToList();
                if (contactTags.IsAny())
                    c.Tags = contactTags.Where(w => w.ContactID == c.ContactID).ToList();
                if (createrInfo.IsAny())
                    c.CreaterInfo = createrInfo.Where(w => w.ContactId == c.ContactID).FirstOrDefault();
                if (contactCommunities != null && contactCommunities.IsAny())
                    c.Communities = contactCommunities.Where(w => w.ContactID == c.ContactID).ToList();
                if (contactTours.IsAny())
                    c.TourCommunity = contactTours.Where(w => w.ContactId == c.ContactID).ToList();
                if (contactActions.IsAny())
                    c.ContactActions = contactActions.Where(w => w.ContactId == c.ContactID).ToList();
                if (contactNotes.IsAny())
                    c.ContactNotes = contactNotes.Where(w => w.ContactID == c.ContactID).ToList();
                if (activeContacts.IsAny())
                {
                    var isActive = activeContacts.Where(w => w.ContactID == c.ContactID).FirstOrDefault();
                    c.IsActive = isActive != null ? isActive.IsActive : false;
                }
            }
            sw.Stop();
            var te = sw.Elapsed;
            Logger.Current.Informational("Time taken to get contact details :" + te);
            var persons = cs.Where(c => c.ContactType == ContactType.Person);
            Logger.Current.Verbose("Filtering companies");
            var companies = cs.Where(c => c.ContactType == ContactType.Company);
            Logger.Current.Verbose("Processing Persons");
            var p1 = Mapper.Map<IEnumerable<ContactsDb>, IEnumerable<Person>>(persons);
            Logger.Current.Verbose("Processing Companies");
            var c1 = Mapper.Map<IEnumerable<ContactsDb>, IEnumerable<Company>>(companies);
            var result = new List<Contact>();
            Logger.Current.Verbose("Adding persons to range");
            result.AddRange(p1);
            Logger.Current.Verbose("Adding companies to range");
            result.AddRange(c1);
            
            if (contactSummary.IsAny())
            {
                Logger.Current.Verbose("Found contact summaries");
                foreach (ContactNoteSummary noteSummary in contactSummary)
                {
                    var contact = result.Where(c => c.Id == noteSummary.ContactId).FirstOrDefault();
                    if (contact != null)
                    {
                        Logger.Current.Verbose(string.Format("Found Contact - {0}", noteSummary.ContactId));
                        contact.NoteSummary = noteSummary.NoteDetails;
                        contact.LastNoteDate = noteSummary.LastNoteDate;
                        contact.LastNote = noteSummary.LastNote;
                        contact.LastNoteCategory = noteSummary.LastNoteCategory;
                    }
                    else
                    {
                        Logger.Current.Verbose(string.Format("Unable to find Contact - {0}", noteSummary.ContactId));
                    }
                }
            }
            Logger.Current.Verbose(string.Format("Returning results. Total results: {0}", result.Count()));

            return result;
        }

        /// <summary>
        /// Finds the created user by contact identifier.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public int? FindCreatedUserByContactId(int contactId)
        {
            var db = ObjectContextFactory.Create();
            int? createdUser = db.ContactsAudit.Where(c => c.ContactID == contactId && c.AuditAction == "I").Select(c => c.LastUpdatedBy).FirstOrDefault();
            return createdUser;
        }

        /// <summary>
        /// Gets the type of the contact.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public ContactType GetContactType(int contactId)
        {
            var db = ObjectContextFactory.Create();
            var contactType = db.Contacts.Where(c => c.ContactID == contactId).Select(c => c.ContactType).First();
            return contactType;
        }

        /// <summary>
        /// Checks the is deleted contact.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public bool CheckIsDeletedContact(int contactId, int accountId)
        {
            var db = ObjectContextFactory.Create();
            bool isDeletedContact = db.Contacts.Where(c => c.ContactID == contactId && c.AccountID == accountId).Select(c => c.IsDeleted).FirstOrDefault();
            return isDeletedContact;
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override Contact FindBy(int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public Contact FindBy(int id, int accountId)
        {
            ContactsDb contactDatabase = getContactDb(id, accountId);

            if (contactDatabase != null)
            {
                Contact contactDatabaseConvertedToDomain = ConvertToDomain(contactDatabase);
                return contactDatabaseConvertedToDomain;
            }
            return null;
        }

        /// <summary>
        /// Finds the by contact identifier.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public Contact FindByContactId(int contactId, int accountId)
        {
            Contact contact = getContactDbOptimized(contactId, accountId);
            return contact;
        }

        /// <summary>
        /// Gets all contacts by ids.
        /// </summary>
        /// <param name="ContactID">The contact identifier.</param>
        /// <param name="SortBy">The sort by.</param>
        /// <param name="SortOrder">The sort order.</param>
        /// <returns></returns>
        public Contact GetContactsById(int ContactID)
        {
            ContactsDb contact = ObjectContextFactory.Create().
                Contacts.Include(c => c.ContactEmails).
                Include(c => c.ContactPhones).
                Include(c => c.Addresses).Include(c => c.CustomFields).
                Where(i => i.ContactID == ContactID).FirstOrDefault();


            if (contact != null)
            {
                contact.ContactEmails = contact.ContactEmails.Where(i => i.IsDeleted == false).ToList();
                contact.ContactPhones = contact.ContactPhones.Where(p => p.IsDeleted == false).ToList();
            }
            return ConvertToDomain(contact);

        }

        public Contact GetDeletedContact(int contactId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT ContactID AS Id, AccountID from CONTACTS (NOLOCK) WHERE ContactID = @ContactId";
            var contact = db.Get<Person>(sql, new { ContactId = contactId }).FirstOrDefault();
            return contact;
        }
            
        /// <summary>
        /// Gets the contacts by contact i ds.
        /// </summary>
        /// <param name="contactIds">The contact ids.</param>
        /// <returns></returns>
        public IEnumerable<Person> GetContactsByContactIDs(IEnumerable<int> contactIds)
        {
            IEnumerable<Contact> contacts = FindAll(contactIds.ToList());
            var persons = Mapper.Map<IEnumerable<Contact>, IEnumerable<Person>>(contacts);
            return persons;
        }

        /// <summary>
        /// Gets all contact by ids.
        /// </summary>
        /// <param name="Campanyids">The campanyids.</param>
        /// <returns></returns>
        public IEnumerable<Contact> GetAllContactByIds(List<int?> Campanyids, int accountId)
        {
            IEnumerable<int> ids = Campanyids.Select(s => s.Value).AsEnumerable();
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT C.* FROM Contacts(NOLOCK) C 
                        JOIN @tbl TC ON TC.ContactID = C.ContactID 
                        WHERE C.AccountID=@accountId";
            IEnumerable<ContactsDb> contacts = db.Get<ContactsDb>(sql, new { tbl = ids.AsTableValuedParameter("dbo.Contact_List"), accountId = accountId }).ToList();

            foreach (ContactsDb contact in contacts)
            {
                yield return ConvertToDomain(contact);
            }
        }

        /// <summary>
        /// Gets the contacts.
        /// </summary>
        /// <param name="contactIds">The contact ids.</param>
        /// <returns></returns>
        public IEnumerable<Contact> GetContacts(IEnumerable<int> contactIds, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var contacts = db.Contacts.Where(w => contactIds.Contains(w.ContactID) && w.AccountID == accountId && w.IsDeleted == false).Select(s => new
            {
                ContactID = s.ContactID,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Company = s.Company,
                ContactType = s.ContactType,
                EmailId = db.ContactEmails.Where(ce => ce.ContactID == s.ContactID && ce.AccountID == accountId
                                        && ce.IsPrimary == true && ce.IsDeleted == false).Select(ce => ce.Email).FirstOrDefault()
            }).ToList().Select(x => new ContactsDb
            {
                ContactID = x.ContactID,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Company = x.Company,
                ContactType = x.ContactType,
                PrimaryEmail = x.EmailId
            });
            if (contacts != null && contacts.Any())
                foreach (ContactsDb dc in contacts)
                {
                    if (dc.ContactType == ContactType.Person)
                        yield return Mapper.Map<ContactsDb, Person>(dc);
                    else
                        yield return Mapper.Map<ContactsDb, Company>(dc);
                }
            else yield return null;
        }

        /// <summary>
        /// Gets all contacts by user ids.
        /// </summary>
        /// <param name="OwnerIds">The owner ids.</param>
        /// <returns></returns>
        public IEnumerable<Contact> GetAllContactsByUserIds(int[] OwnerIds)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<ContactsDb> contacts = db.Contacts.Include(p => p.ContactLeadSources)
                .Where(i => i.ContactType == ContactType.Person && OwnerIds.Contains((int)i.OwnerID));

            foreach (ContactsDb contact in contacts)
            {
                yield return ConvertToDomain(contact);
            }
        }

        /// <summary>
        /// Gets the contact database.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        ContactsDb getContactDb(int id, int accountId)
        {
            var db = ObjectContextFactory.Create();

            var customFieldIds = db.Fields.Where(f => f.StatusID == (short?)FieldStatus.Active).Select(f => f.FieldID).ToList();

            ContactsDb contact = db.Contacts.Include(c => c.Addresses).Include(c => c.ContactPhones).Include(c => c.ContactEmails)
                .Include(c => c.CustomFields).Include(c => c.WebVisits).Include(c => c.Tags).Include(c => c.FormSubmissions).Where(c => c.ContactID == id && c.IsDeleted == false && c.AccountID == accountId).SingleOrDefault();

            if (contact != null)
            {
                contact.ContactEmails = contact.ContactEmails.Where(i => i.IsDeleted == false).ToList();
                contact.ContactPhones = contact.ContactPhones.Where(p => p.IsDeleted == false).ToList();
                contact.ContactLeadSources = contact.ContactLeadSources.OrderByDescending(i => i.IsPrimaryLeadSource).ToList();
                contact.CustomFields = contact.CustomFields.Where(cf => customFieldIds.Contains(cf.CustomFieldID)).ToList();
            }

            return contact;
        }

        /// <summary>
        /// Gets the contact database optimized.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        Contact getContactDbOptimized(int contactId, int accountId)
        {
            var contactIds = new List<int>();
            contactIds.Add(contactId);
            var contacts = FindAll(contactIds, false);
            if (contacts.IsAny())
                return contacts.FirstOrDefault();
            else
                return default(Contact);
        }

        /// <summary>
        /// Finds the contacts summary.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        IEnumerable<ContactsDb> findContactsSummary(Expression<Func<ContactsDb, bool>> predicate)
        {
            IEnumerable<ContactsDb> contacts = ObjectContextFactory.Create().Contacts.Include(c => c.Addresses)
                .AsExpandable()
                 .Where(predicate).Where(c => c.IsDeleted != true).Select(c =>
                        new
                        {
                            ContactID = c.ContactID,
                            FirstName = c.FirstName,
                            LastName = c.LastName,
                            Company = c.Company,
                            MobilePhone = c.MobilePhone,
                            WorkPhone = c.WorkPhone,
                            PrimaryEmail = c.PrimaryEmail,
                            LifecycleStage = c.LifecycleStage,
                            LastContacted = c.LastContacted,
                            ContactType = c.ContactType,
                            Addresses = c.Addresses,
                            //Emails = c.Emails,
                            LastUpdatedOn = c.LastUpdatedOn,
                            LastUpdatedBy = c.LastUpdatedBy
                        }).ToList().OrderByDescending(x => x.ContactID).Select(x => new ContactsDb
                        {
                            ContactID = x.ContactID,
                            FirstName = x.FirstName,
                            LastName = x.LastName,
                            Company = x.Company,
                            PrimaryEmail = x.PrimaryEmail,
                            MobilePhone = x.MobilePhone,
                            WorkPhone = x.WorkPhone,
                            LifecycleStage = x.LifecycleStage,
                            LastContacted = x.LastContacted,
                            ContactType = x.ContactType,
                            Addresses = x.Addresses,
                            //Emails = c.Emails,
                            LastUpdatedOn = x.LastUpdatedOn,
                            LastUpdatedBy = x.LastUpdatedBy
                        });
            return contacts;
        }

        /// <summary>
        /// Gets the contacts by import.
        /// </summary>
        /// <param name="LeadAdapterJobID">The lead adapter job identifier.</param>
        /// <param name="recordStatus">The record status.</param>
        /// <returns></returns>
        public IEnumerable<int> GetContactsByImport(int LeadAdapterJobID, string recordStatus)
        {
            var db = ObjectContextFactory.Create();
            List<int> importedContacts = new List<int>();
            string procedure = "[dbo].[GetContactsByImport]";
            db.QueryStoredProc(procedure, (reader) => { importedContacts = reader.Read<int>().ToList(); }, new { LeadAdapterJobID = LeadAdapterJobID, recordStatus = recordStatus });
            return importedContacts;
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public IEnumerable<Contact> FindAll(string name)
        {
            var predicate = PredicateBuilder.True<ContactsDb>();

            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                predicate = predicate.And(c => c.FirstName.Contains(name) || c.LastName.Contains(name) ||
                    (c.ContactType == ContactType.Company && c.Company.Contains(name)));
            }

            IEnumerable<ContactsDb> contacts = findContactsSummary(predicate);
            foreach (ContactsDb dc in contacts)
            {
                if (dc.ContactType == ContactType.Person)
                    yield return Mapper.Map<ContactsDb, Person>(dc);
                else
                    yield return Mapper.Map<ContactsDb, Company>(dc);
            }
        }

        /// <summary>
        /// Finds the persons.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public IEnumerable<Contact> FindPersons(string name)
        {
            var predicate = PredicateBuilder.True<ContactsDb>();
            predicate.And(c => c.ContactType == ContactType.Person);

            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                predicate = predicate.And(c => c.FirstName.ToLower().Contains(name) || c.LastName.ToLower().Contains(name));
            }

            IEnumerable<ContactsDb> contacts = findContactsSummary(predicate);

            foreach (ContactsDb dc in contacts)
            {
                yield return ConvertToDomain(dc);
            }
        }

        /// <summary>
        /// Finds the companies.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public IEnumerable<Contact> FindCompanies(string name)
        {
            var predicate = PredicateBuilder.True<ContactsDb>();
            predicate.And(c => c.ContactType == ContactType.Company);

            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                predicate = predicate.And(c => c.Company.Contains(name));
            }

            IEnumerable<ContactsDb> contacts = findContactsSummary(predicate);
            foreach (ContactsDb dc in contacts)
            {
                yield return ConvertToDomain(dc);
            }
        }

        /// <summary>
        /// Gets the contact by user identifier.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="contactIDs">The contact ids.</param>
        /// <returns></returns>
        public IList<int> GetContactByUserId(int userId, int[] contactIDs, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var contactIds = db.Contacts.Where(c => c.IsDeleted == false && c.OwnerID == userId && c.AccountID == accountId).Select(c => c.ContactID).ToList();
            if (contactIDs != null)
                contactIds = contactIds.Where(p => contactIDs.Contains(p)).ToList();
            return contactIds;
        }

        /// <summary>
        /// Gets active contacts from search definition result
        /// </summary>
        /// <param name="contactIds">List of contact IDs</param>
        /// <param name="accountId">Account ID</param>
        /// <returns></returns>
        public IEnumerable<int> GetSearchDefinitionActiveContacts(List<int> contactIds, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var validStatuses = new List<byte>()
                {
                    (byte)EmailStatus.NotVerified, (byte)EmailStatus.Verified, (byte)EmailStatus.SoftBounce
                };
            var sql = @"SELECT ac.ContactID FROM ActiveContacts (NOLOCK) ac
                            WHERE ac.Accountid = @accountId AND ac.IsDeleted = 0 AND EmailStatus IN (SELECT DATAVALUE FROM dbo.Split(@emailStatuses,','))";
            var contacts = db.Get<int>(sql, new { accountid = accountId, emailStatuses = string.Join(",", validStatuses.ToList()) }, true)
                .Join(contactIds.Distinct(), c => c, cf => cf, (c, cf) => c).Distinct();
            return contacts;
        }

        /// <summary>
        /// Gets the contacts by user ids.
        /// </summary>
        /// <param name="ownerIds">The owner ids.</param>
        /// <returns></returns>
        public IList<int> GetContactsByUserIds(int[] ownerIds, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var contactIds = db.Contacts.Where(c => c.IsDeleted == false && ownerIds.Contains((int)c.OwnerID) && c.AccountID == accountId).Select(c => c.ContactID).ToList();
            return contactIds;
        }

        /// <summary>
        /// Finds the name of the company.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public Company FindCompanyName(string name, int companyId, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var Company = new Company();
            if (companyId != 0)
                Company = db.Contacts.Where(w => !w.IsDeleted && w.ContactID == companyId && w.ContactType == ContactType.Company && w.AccountID == accountId).Select(x => new Company() { Id = x.ContactID, CompanyName = x.Company }).FirstOrDefault();
            else
                Company = db.Contacts
                      .Where(c => !c.IsDeleted && c.ContactType == ContactType.Company && c.Company == name && c.AccountID == accountId)
                      .Select(x => new Company() { Id = x.ContactID, CompanyName = x.Company }).FirstOrDefault();
            return Company ?? new Company();
        }

        /// <summary>
        /// Deactivates the contact.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="updatedBy">The updated by.</param>
        public void DeactivateContact(int contactId, int updatedBy, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var contact = db.Contacts.SingleOrDefault(c => c.ContactID == contactId && c.AccountID == accountId);
            if (contact == null)
                return;
            contact.IsDeleted = true;
            contact.LastUpdatedBy = updatedBy;
            contact.LastUpdatedOn = DateTime.Now.ToUniversalTime();
            RefreshAnalyticsDb refreshAnalytics = new RefreshAnalyticsDb();
            refreshAnalytics.EntityID = contact.ContactID;
            refreshAnalytics.EntityType = (byte)IndexType.Contacts_Delete;
            refreshAnalytics.Status = 1;
            refreshAnalytics.LastModifiedOn = DateTime.Now.ToUniversalTime();

            db.RefreshAnalytics.Add(refreshAnalytics);

            IEnumerable<LeadAdapterJobLogDetailsDb> joblogdetails = db.LeadAdapterJobLogDetails.Where(i => i.ReferenceId == contact.ReferenceId);
            if (joblogdetails != null)
                db.LeadAdapterJobLogDetails.RemoveRange(joblogdetails);
            var removeContactSource = db.ContactLeadSourcesMap.Where(c => c.ContactID == contactId).FirstOrDefault();
            if (removeContactSource != null)
                db.ContactLeadSourcesMap.Remove(removeContactSource);
            db.SaveChanges();
        }

        /// <summary>
        /// Gets the contact form submissions.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public IEnumerable<FormSubmission> GetContactFormSubmissions(int contactId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var submittedForms = db.FormSubmissions.Where(f => f.ContactID == contactId).Select(c => c.FormID).ToList();  /// Use join to have one db call.
                var formSubmissions = db.FormSubmissions.Where(f => submittedForms.Contains(f.FormID)).Include(f => f.Form.FormTags).ToList();
                return Mapper.Map<IEnumerable<FormSubmissionDb>, IEnumerable<FormSubmission>>(formSubmissions);
            }
        }

        /// <summary>
        /// Gets the contacts form submissions list.
        /// </summary>
        /// <param name="contactIds">The contact ids.</param>
        /// <returns></returns>
        public IEnumerable<FormSubmission> GetContactsFormSubmissionsList(int[] contactIds)
        {
            using (var db = ObjectContextFactory.Create())
            {
                //  var submittedForms = db.FormSubmissions.Where(f => contactIds.Contains(f.ContactID)).Select(c => c.FormID).ToList();  /// Use join to have one db call.
                var formSubmissions = db.FormSubmissions.Where(f => contactIds.Contains(f.ContactID)).Include(f => f.Form.FormTags).ToList();
                return Mapper.Map<IEnumerable<FormSubmissionDb>, IEnumerable<FormSubmission>>(formSubmissions);
            }
        }

        /// <summary>
        /// Deactivates the contacts list.
        /// </summary>
        /// <param name="contactids">The contactids.</param>
        /// <param name="updatedBy">The updated by.</param>
        /// <returns></returns>
        public int DeactivateContactsList(int[] contactids, int updatedBy, int accountId)
        {
            int contactsDeleted = 0;
            var db = ObjectContextFactory.Create();
            if (contactids != null && contactids.Any())
            {
                IEnumerable<ContactsDb> contactsdb = db.Contacts.Where(i => contactids.Contains(i.ContactID) && i.AccountID == accountId);
                List<RefreshAnalyticsDb> refreshAnalyticsDb = new List<RefreshAnalyticsDb>();
                foreach (ContactsDb contact in contactsdb)
                {
                    if (contact != null)
                    {
                        contact.IsDeleted = true;
                        contact.LastUpdatedBy = updatedBy;
                        contact.LastUpdatedOn = DateTime.Now.ToUniversalTime();
                        contactsDeleted = contactsDeleted + 1;
                        RefreshAnalyticsDb refreshAnalytics = new RefreshAnalyticsDb();
                        refreshAnalytics.EntityID = contact.ContactID;
                        refreshAnalytics.EntityType = (byte)IndexType.Contacts_Delete;
                        refreshAnalytics.Status = 1;
                        refreshAnalytics.LastModifiedOn = DateTime.Now.ToUniversalTime();
                        refreshAnalyticsDb.Add(refreshAnalytics);
                    }

                }

                if (refreshAnalyticsDb.IsAny())
                    db.RefreshAnalytics.AddRange(refreshAnalyticsDb);

                IEnumerable<LeadAdapterJobLogDetailsDb> joblogdetails = db.LeadAdapterJobLogDetails.Where(i => contactsdb.Select(x => x.ReferenceId).Contains(i.ReferenceId));
                db.LeadAdapterJobLogDetails.RemoveRange(joblogdetails);
                IEnumerable<ContactLeadSourceMapDb> leadSourceMap = db.ContactLeadSourcesMap.Where(x => contactsdb.Select(c => c.ContactID).Contains(x.ContactID));
                if (leadSourceMap != null)
                    db.ContactLeadSourcesMap.RemoveRange(leadSourceMap);
                db.SaveChanges();
                return contactsDeleted;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Determines whether [is duplicate person] [the specified first name].
        /// </summary>
        /// <param name="firstName">The first name.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="primaryEmail">The primary email.</param>
        /// <param name="company">The company.</param>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public bool IsDuplicatePerson(string firstName, string lastName, string primaryEmail, string company, int contactId, int accountId)
        {
            IQueryable<ContactsDb> contacts = ObjectContextFactory.Create().Contacts;
            contacts = contacts.Where(c => c.FirstName.Equals(firstName) && c.LastName.Equals(lastName) && c.ContactType == ContactType.Person && c.AccountID == accountId);

            if (contactId > 0)
                contacts = contacts.Where(c => c.ContactID != contactId);

            if (!string.IsNullOrEmpty(company))
                contacts = contacts.Where(c => c.Company.Equals(company));
            else if (!string.IsNullOrEmpty(primaryEmail))
                contacts = contacts.Where(c => c.PrimaryEmail.Equals(primaryEmail));
            else
                return false;

            return contacts.Any();
        }

        /// <summary>
        /// Determines whether [is duplicate company] [the specified company name].
        /// </summary>
        /// <param name="companyName">Name of the company.</param>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public bool IsDuplicateCompany(string companyName, int contactId, int accountId)
        {
            return ObjectContextFactory.Create().Contacts.Any(c => c.ContactID != contactId && c.Company.Equals(companyName) && c.ContactType == ContactType.Company && c.AccountID == accountId);
        }

        /// <summary>
        /// Gets the states.
        /// </summary>
        /// <param name="countryCode">The country code.</param>
        /// <returns></returns>
        public IEnumerable<State> GetStates(string countryCode)
        {
            var db = ObjectContextFactory.Create();
            var sql = "SELECT StateID as Code, StateName as Name FROM States (NOLOCK) WHERE CountryID = @country ORDER BY StateName ASC";
            var states = db.Get<State>(sql, new { country = countryCode }, true, new TimeSpan(4, 0, 0));
            return states;
        }

        /// <summary>
        /// Gets all states.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<State> GetAllStates()
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<StatesDb> statesDb = db.States.OrderByDescending(s => s.CountryID).ThenBy(s => s.StateName).ToList();
            if (statesDb != null)
                return convertToDomainStates(statesDb);
            return null;
        }

        /// <summary>
        /// Gets the countries.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Country> GetCountries()
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<CountriesDb> countriesDb = db.Countries.OrderByDescending(c => c.CountryID).ToList();
            if (countriesDb != null)
                return convertToDomainCountry(countriesDb);
            return null;
        }

        /// <summary>
        /// Gets the users.
        /// </summary>
        /// <param name="AccountId">The account identifier.</param>
        /// <param name="UserId">The user identifier.</param>
        /// <param name="IsSTadmin">if set to <c>true</c> [is stadmin].</param>
        /// <returns></returns>
        public IEnumerable<Owner> GetUsers(int AccountId, int UserId, bool IsSTadmin)
        {
            var db = ObjectContextFactory.Create();
            string sql = string.Empty;
            if (UserId == 0 && IsSTadmin)
                sql = @"SELECT UserId as OwnerID, FirstName + ' ' + LastName +' (' + PrimaryEmail + ') ' AS OwnerName, IsDeleted, AccountID  FROM Users (NOLOCK) WHERE AccountId IN (@accountId, 1) AND Status = @status";
            else if (UserId == 0 && !IsSTadmin)
                sql = @"SELECT UserId as OwnerID, FirstName + ' ' + LastName +' (' + PrimaryEmail + ') ' AS OwnerName, IsDeleted, AccountID  FROM Users (NOLOCK) WHERE AccountId IN (@accountId) AND Status = @status";
            else
                sql = @"SELECT UserId as OwnerID, FirstName + ' ' + LastName +' (' + PrimaryEmail + ') ' AS OwnerName, IsDeleted, AccountID  FROM Users (NOLOCK) WHERE AccountId IN (@accountId, 1) AND Status = @status and UserId = @userId";

            var owners = db.Get<Owner>(sql, new { accountId = AccountId, userId = UserId, status = Status.Active }, getFromCache: true).ToList();
            return owners;
        }

        /// <summary>
        /// Gets the user names.
        /// </summary>
        /// <param name="OwnerIds">The owner ids.</param>
        /// <returns></returns>
        public IEnumerable<Owner> GetUserNames(IEnumerable<int?> OwnerIds)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<Owner> owners = db.Users.Where(u => OwnerIds.Contains(u.UserID)).Select(s =>
                new
                {
                    OwnerId = s.UserID,
                    OwnerName = s.FirstName + " " + s.LastName
                }).ToList().Select(x => new Owner()
                {
                    OwnerId = x.OwnerId,
                    OwnerName = x.OwnerName
                });
            return owners;
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="contactsDb">The contacts database.</param>
        /// <param name="db">The database.</param>
        public override void PersistValueObjects(Contact domainType, ContactsDb contactsDb, CRMDb db)
        {
            persistAddresses(domainType, contactsDb, db);
            persistCommunication(domainType, contactsDb, db);
            persistRelationship(domainType, contactsDb, db);
            persistContactImage(domainType, contactsDb, db);
            persistCustomFields(domainType, contactsDb, db);
            persistPhones(domainType, contactsDb, db);
            persistEmails(domainType, contactsDb, db);
            persistContactLeadSouce(domainType, contactsDb, db);
            persistContactCommunities(domainType, contactsDb, db);
            //PersistContactOutlookSync(domainType, contactsDb, db);
            contactsDb.Company = domainType.Company_Name;
        }

        /// <summary>
        /// Excetes the sored proc.
        /// </summary>
        /// <param name="contactID">The contact identifier.</param>
        /// <param name="flag">The flag.</param>
        public void ExecuteStoredProc(int contactID, byte flag)
        {
            var db = ObjectContextFactory.Create();
            var procedureName = "[dbo].[INSERT_UPDATE_Contact_Data]";
            var parms = new List<SqlParameter>
                {
                    new SqlParameter{ ParameterName="@ContactID", Value=contactID.ToString(), SqlDbType= SqlDbType.Int },
                    new SqlParameter{ ParameterName="@ActionFlag", Value=flag.ToString(), SqlDbType= SqlDbType.TinyInt }
                };
            db.ExecuteStoredProcedure(procedureName, parms);
        }

        /// <summary>
        /// Updates the Sync Table when ever a contact is updated outside Outlook. 
        /// </summary>
        /// <param name="contact"></param>
        public void PersistContactOutlookSync(Contact contact)
        {
            var db = ObjectContextFactory.Create();
            CRMOutlookSyncDb outlookSyncDb;

            if (contact.Id > 0)
            {
                outlookSyncDb = db.CRMOutlookSync.Where(o => o.EntityID == contact.Id && o.EntityType == (byte)AppModules.Contacts).FirstOrDefault();
                if (outlookSyncDb != null)
                {
                    outlookSyncDb.SyncStatus = (short)OutlookSyncStatus.NotInSync;
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Persists the contact communities.
        /// </summary>
        /// <param name="contact">The contact.</param>
        /// <param name="contactsDb">The contacts database.</param>
        /// <param name="db">The database.</param>
        private void persistContactCommunities(Contact contact, ContactsDb contactsDb, CRMDb db)
        {
            var contactCommunities = db.ContactCommunityMap.Where(n => n.ContactID == contact.Id).ToList();

            if (contact.Communities != null && contact.Communities.Any())
            {
                foreach (DropdownValue dropdownValue in contact.Communities.Where(c => c.Id > 0))
                {
                    ContactCommunityMapDb contactCommunityMap = contactCommunities.Where(i => i.CommunityID == dropdownValue.Id).FirstOrDefault();
                    if (contactCommunityMap == null)
                    {
                        var insertContactCommunityMap = new ContactCommunityMapDb()
                        {
                            ContactID = contact.Id,
                            CommunityID = dropdownValue.Id,
                            CreatedOn = DateTime.Now.ToUniversalTime()
                        };

                        db.ContactCommunityMap.Add(insertContactCommunityMap);
                    }
                    else
                    {
                        db.Entry(contactCommunityMap).State = System.Data.Entity.EntityState.Modified;
                        contactCommunityMap.LastModifiedOn = DateTime.Now.ToUniversalTime();
                    }
                }
            }
            //IEnumerable<short> leadCommunityIds = contact.Communities.Where(c => c.Id > 0).Select(c => c.Id);
            //var unMapContactCommunities = contactCommunities.Where(c => !leadCommunityIds.Contains(c.CommunityID));
            //db.ContactCommunityMap.RemoveRange(unMapContactCommunities);           
        }

        /// <summary>
        /// Persists the custom fields.
        /// </summary>
        /// <param name="contact">The contact.</param>
        /// <param name="contactsDb">The contacts database.</param>
        /// <param name="db">The database.</param>
        void persistCustomFields(Contact contact, ContactsDb contactsDb, CRMDb db)
        {
            var contactCustomFieldsDb = db.ContactCustomFields.Where(c => c.ContactID == contact.Id).ToList();
            foreach (ContactCustomField contactCustomField in contact.CustomFields)
            {
                ContactCustomFieldsDb newContactCustomFieldsDb = Mapper.Map<ContactCustomField, ContactCustomFieldsDb>(contactCustomField);
                var isFound = contactCustomFieldsDb.Where(c => c.CustomFieldID == contactCustomField.CustomFieldId).FirstOrDefault();
                if (isFound == null)
                {
                    db.ContactCustomFields.Add(newContactCustomFieldsDb);
                }
                else
                {
                    isFound.Value = newContactCustomFieldsDb.Value;
                    db.Entry<ContactCustomFieldsDb>(isFound).State = System.Data.Entity.EntityState.Modified;
                }
            }
            if (contact.Id > 0)
            {
                foreach (ContactCustomFieldsDb contactCustomFieldDb in contactCustomFieldsDb)
                {
                    var isFound = contact.CustomFields.Where(c => c.CustomFieldId == contactCustomFieldDb.CustomFieldID).FirstOrDefault();
                    if (isFound == null)
                    {
                        db.Entry<ContactCustomFieldsDb>(contactCustomFieldDb).State = System.Data.Entity.EntityState.Deleted;

                        //db.Entry<ContactCustomFieldsDb>(contactCustomFieldDb).State = EntityState.Modified;
                    }
                }
            }
        }

        /// <summary>
        /// Persists the addresses.
        /// </summary>
        /// <param name="contact">The contact.</param>
        /// <param name="contactsDb">The contacts database.</param>
        /// <param name="db">The database.</param>
        void persistAddresses(Contact contact, ContactsDb contactsDb, CRMDb db)
        {
            IEnumerable<AddressesDb> addresses = Mapper.Map<IEnumerable<Address>, IEnumerable<AddressesDb>>(contact.Addresses);

            //Existing contact
            if (contact.Id > 0)
            {
                //Existing contact, delete addresses in db if deleted in UI.
                var deletedAddresses = contactsDb.Addresses.Where(a => a.AddressID != 0
                    && !addresses.Select(ad => ad.AddressID).Contains(a.AddressID)).ToList();
                foreach (AddressesDb addressDb in deletedAddresses)
                {
                    contactsDb.Addresses.Remove(addressDb);
                }
            }
        }

        /// <summary>
        /// Persists the emails.
        /// </summary>
        /// <param name="contact">The contact.</param>
        /// <param name="contactsDb">The contacts database.</param>
        /// <param name="db">The database.</param>
        void persistEmails(Contact contact, ContactsDb contactsDb, CRMDb db)
        {
            var contactEmails = db.ContactEmails.Where(c => c.ContactID == contact.Id && c.IsDeleted == false).ToList();
            if (contact.Emails != null)
            {
                foreach (var email in contact.Emails)
                {
                    var contctEmailMap = contactEmails.SingleOrDefault(r => r.ContactEmailID == email.EmailID);
                    if (contctEmailMap != null)
                    {
                        contctEmailMap.Email = email.EmailId;
                        contctEmailMap.EmailStatus = (byte)email.EmailStatusValue;
                        contctEmailMap.IsPrimary = email.IsPrimary;
                    }
                    else
                    {
                        ContactEmailsDb map = new ContactEmailsDb();
                        map.ContactID = contactsDb.ContactID;
                        map.Email = email.EmailId;
                        map.EmailStatus = (byte)email.EmailStatusValue;
                        map.IsPrimary = email.IsPrimary;
                        map.AccountID = email.AccountID;
                        db.ContactEmails.Add(map);
                    }
                }
                IList<int> emailIds = contact.Emails.Where(n => n.EmailID > 0).Select(n => n.EmailID).ToList();
                var unMapContactsEmails = contactEmails.Where(n => !emailIds.Contains(n.ContactEmailID));
                foreach (ContactEmailsDb contactEmailMapDb in unMapContactsEmails)
                {
                    var contactEmailAuditRecords = db.ContactEmailAudit.Where(ce => ce.ContactEmailID == contactEmailMapDb.ContactEmailID);
                    db.ContactEmailAudit.RemoveRange(contactEmailAuditRecords);
                    db.ContactEmails.Remove(contactEmailMapDb);
                }
            }
            else
            {
                foreach (var contactEmail in contactEmails)
                {
                    contactEmail.IsPrimary = false;
                    contactEmail.IsDeleted = true;
                    //db.ContactEmails.Remove(contactEmail);
                    db.Entry(contactEmail).State = System.Data.Entity.EntityState.Modified;
                }
            }
        }

        /// <summary>
        /// Persists the phones.
        /// </summary>
        /// <param name="contact">The contact.</param>
        /// <param name="contactsDb">The contacts database.</param>
        /// <param name="db">The database.</param>
        void persistPhones(Contact contact, ContactsDb contactsDb, CRMDb db)
        {
            var contactPhones = db.ContactPhoneNumbers.Where(c => c.ContactID == contact.Id).ToList();
            if (contact.Phones != null)
            {
                foreach (var phone in contact.Phones)
                {
                    if (phone.ContactPhoneNumberID != 0)
                    {
                        var contctPhoneMap = contactPhones.SingleOrDefault(r => r.ContactPhoneNumberID == phone.ContactPhoneNumberID);
                        var dropdownvalue = db.DropdownValues.Where(p => p.DropdownValueID == phone.PhoneType).FirstOrDefault();
                        contctPhoneMap.PhoneNumber = phone.Number;
                        contctPhoneMap.PhoneType = phone.PhoneType;
                        contctPhoneMap.IsPrimary = phone.IsPrimary;
                        contctPhoneMap.DropdownValues = dropdownvalue;
                    }
                    else
                    {
                        var dropodwnvalue = db.DropdownValues.Where(c => c.DropdownValueID == phone.PhoneType).FirstOrDefault();
                        //ContactPhoneNumbersDb contactPhoneNumbersDb;
                        ContactPhoneNumbersDb map = new ContactPhoneNumbersDb();
                        map.ContactID = contactsDb.ContactID;
                        map.PhoneNumber = phone.Number;
                        map.PhoneType = phone.PhoneType;
                        map.IsPrimary = phone.IsPrimary;
                        map.AccountID = phone.AccountID;
                        map.DropdownValues = dropodwnvalue;
                        db.ContactPhoneNumbers.Add(map);

                        //   map = Mapper.Map<Phone, ContactPhoneNumbersDb>(phone);

                    }

                }
                IList<int> phoneIds = contact.Phones.Where(n => n.ContactPhoneNumberID > 0).Select(n => n.ContactPhoneNumberID).ToList();
                var unMapContactsPhones = contactPhones.Where(n => !phoneIds.Contains(n.ContactPhoneNumberID));
                foreach (ContactPhoneNumbersDb contactPhoneMapDb in unMapContactsPhones)
                {
                    var contactPhoneAuditRecords = db.ContactTextMessage.Where(ce => ce.ContactPhoneNumberID == contactPhoneMapDb.ContactPhoneNumberID);
                    db.ContactTextMessage.RemoveRange(contactPhoneAuditRecords);
                    db.ContactPhoneNumbers.Remove(contactPhoneMapDb);
                }
            }
            else if (contactPhones != null && contactPhones.Any())
            {
                foreach (var contactPhone in contactPhones)
                {
                    //db.ContactPhoneNumbers.Remove(contactPhone);
                    contactPhone.IsDeleted = true;
                    contactPhone.IsPrimary = false;
                    db.Entry(contactPhone).State = System.Data.Entity.EntityState.Modified;
                }
            }
        }

        /// <summary>
        /// Persists the communication.
        /// </summary>
        /// <param name="contact">The contact.</param>
        /// <param name="contactsDb">The contacts database.</param>
        /// <param name="db">The database.</param>
        void persistCommunication(Contact contact, ContactsDb contactsDb, CRMDb db)
        {
            CommunicationsDb communicationDb;
            if (contact.GetType().Equals(typeof(Person)))
                communicationDb = Mapper.Map<Person, CommunicationsDb>(contact as Person);
            else
                communicationDb = Mapper.Map<Company, CommunicationsDb>(contact as Company);

            if (contactsDb.Communication != null)
                communicationDb.CommunicationID = contactsDb.Communication.CommunicationID;
            contactsDb.Communication = communicationDb;
        }

        /// <summary>
        /// Persists the relationship.
        /// </summary>
        /// <param name="contact">The contact.</param>
        /// <param name="contactsDb">The contacts database.</param>
        /// <param name="db">The database.</param>
        public void persistRelationship(Contact contact, ContactsDb contactsDb, CRMDb db)
        {
            if (contact.ContactRelationships == null)
                return;
            var contactrelation = db.ContactRelationshipMap.Where(c => c.ContactID == contact.Id);
            foreach (var entry in contact.ContactRelationships)
            {
                if (entry.Id != 0)
                {
                    var contctrelationmap = contactrelation.SingleOrDefault(r => r.ContactRelationshipMapID == entry.Id);
                    contctrelationmap.RelationshipType = entry.RelationshipTypeID;


                    contctrelationmap.RelatedUserID = null;
                    contctrelationmap.RelatedContactID = entry.RelatedContactID.Value;
                }
                else
                {
                    ContactRelationshipDb map = new ContactRelationshipDb();
                    map.ContactID = contact.Id;
                    map.RelationshipType = entry.RelationshipTypeID;
                    map.RelatedUserID = null;
                    map.RelatedContactID = entry.RelatedContactID;
                    map.ContactRelationshipMapID = entry.Id;
                    db.ContactRelationshipMap.Add(map);
                }
            }
        }

        /// <summary>
        /// Persists the contact image.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="contactsDb">The contacts database.</param>
        /// <param name="db">The database.</param>
        void persistContactImage(Contact domainType, ContactsDb contactsDb, CRMDb db)
        {
            ImagesDb imagesdb = Mapper.Map<Image, ImagesDb>(domainType.ContactImage);
            if (imagesdb != null)
            {
                var varImage = db.Images.Where(Id => Id.ImageID == imagesdb.ImageID).FirstOrDefault();
                if (varImage != null)
                {
                    varImage.StorageName = domainType.ContactImage.StorageName;
                    varImage.FriendlyName = domainType.ContactImage.FriendlyName;
                    varImage.OriginalName = domainType.ContactImage.OriginalName;
                }
                else
                {
                    contactsDb.Image = imagesdb;
                }
            }
        }

        /// <summary>
        /// Persists the contact lead souce.
        /// </summary>
        /// <param name="contact">The contact.</param>
        /// <param name="contactsDb">The contacts database.</param>
        /// <param name="db">The database.</param>
        private void persistContactLeadSouce(Contact contact, ContactsDb contactsDb, CRMDb db)
        {
            var contactLeadSources = db.ContactLeadSourcesMap.Where(n => n.ContactID == contact.Id).ToList();

            if (contact.LeadSources != null && contact.LeadSources.Any())
            {
                foreach (DropdownValue dropdownValue in contact.LeadSources)
                {
                    ContactLeadSourceMapDb contactleadsourcemap = contactLeadSources.Where(i => i.LeadSouceID == dropdownValue.Id).FirstOrDefault();
                    if (contactleadsourcemap == null)
                    {
                        var insertcontactleadsrcmap = new ContactLeadSourceMapDb()
                        {
                            ContactID = contact.Id,
                            LeadSouceID = dropdownValue.Id,
                            IsPrimaryLeadSource = false,
                            LastUpdatedDate = DateTime.UtcNow
                        };
                        if (dropdownValue == contact.LeadSources.First())
                            insertcontactleadsrcmap.IsPrimaryLeadSource = true;
                        db.ContactLeadSourcesMap.Add(insertcontactleadsrcmap);
                    }
                    else
                    {
                        if (dropdownValue.IsPrimary != contactleadsourcemap.IsPrimaryLeadSource)
                            contactleadsourcemap.LastUpdatedDate = DateTime.UtcNow;
                        if (dropdownValue == contact.LeadSources.First())
                            contactleadsourcemap.IsPrimaryLeadSource = true;
                        else
                            contactleadsourcemap.IsPrimaryLeadSource = false;
                        db.Entry(contactleadsourcemap).State = System.Data.Entity.EntityState.Modified;
                    }
                }

                IEnumerable<short> leadSourceIds = contact.LeadSources.Where(c => c.Id > 0).Select(c => c.Id);
                var unMapContactLeadSource = contactLeadSources.Where(c => !leadSourceIds.Contains(c.LeadSouceID));
                db.ContactLeadSourcesMap.RemoveRange(unMapContactLeadSource);
            }
            else
            {
                db.ContactLeadSourcesMap.RemoveRange(contactLeadSources);
            }
        }

        /// <summary>
        /// Gets the contact image.
        /// </summary>
        /// <param name="ImageID">The image identifier.</param>
        /// <returns></returns>
        public Image GetContactImage(int? ImageID)
        {
            if (ImageID != null && ImageID != 0)
            {
                using (var db = new CRMDb())
                {
                    ImagesDb imagesDb = db.Images.Where(Id => Id.ImageID == ImageID).FirstOrDefault();
                    return Mapper.Map<ImagesDb, Image>(imagesDb);
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the relation ships.
        /// </summary>
        /// <param name="lstValues">The contact relationship values.</param>
        /// <returns></returns>
        public IList<ContactRelationship> GetRelationShips(List<ContactRelationshipDb> lstValues)
        {
            if (lstValues == null)
                return new List<ContactRelationship>();
            var db = ObjectContextFactory.Create();
            int[] relatedContactIds = lstValues.Where(c => c.RelatedContactID.HasValue)
                .Select(c => c.RelatedContactID.Value).Distinct().ToArray();

            var relatedContacts = db.Contacts.Where(c => relatedContactIds.Contains(c.ContactID))
                .Select(c => new
                {
                    ContactID = c.ContactID,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Company = c.Company,
                    ContactType = c.ContactType
                });

            List<ContactRelationship> lstContactRelationship = new List<ContactRelationship>();
            for (int i = 0; i < lstValues.Count; i++)
            {
                var lstRelation = lstValues[i];
                ContactRelationship modelContactRelationship = new ContactRelationship();
                modelContactRelationship.RelatedContact = new Person { FirstName = "", CompanyName = "" };

                modelContactRelationship.ContactId = lstRelation.ContactID;
                modelContactRelationship.Id = lstRelation.ContactRelationshipMapID;
                modelContactRelationship.RelationshipTypeID = lstRelation.RelationshipType;
                modelContactRelationship.RelationshipName = lstRelation.DropdownValues != null ? lstRelation.DropdownValues.DropdownValue : "";
                var contactid = lstValues[i].RelatedContactID.Value;
                var varContact = relatedContacts.SingleOrDefault(c => c.ContactID == contactid); //db.Contacts.SingleOrDefault(c => c.ContactID == contactid);
                if (varContact.ContactType == ContactType.Person)
                {

                    modelContactRelationship.RelatedContact.FirstName = varContact.FirstName + " " + varContact.LastName;
                }
                else
                    modelContactRelationship.RelatedContact.CompanyName = varContact.Company;

                // modelContactRelationship.RelatedContact.LastName = varContact.
                modelContactRelationship.RelatedContactID = lstRelation.RelatedContactID;
                lstContactRelationship.Add(modelContactRelationship);
            }
            return lstContactRelationship;
        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="db">The database.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Invalid contact id has been passed. Suspected Id forgery.</exception>
        public override ContactsDb ConvertToDatabaseType(Contact domainType, CRMDb db)
        {
            ContactsDb contactDb;

            //Existing Contact
            if (domainType.Id > 0)
            {
                contactDb = db.Contacts.Include(c => c.Addresses).Include(c => c.Communication).Include("CustomFields.CustomField").Where(c => c.ContactID == domainType.Id && c.AccountID == domainType.AccountID).FirstOrDefault();

                if (contactDb == null)
                    throw new ArgumentException("Invalid contact id has been passed. Suspected Id forgery.");

                if (domainType is Person)
                    contactDb = Mapper.Map<Person, ContactsDb>(domainType as Person, contactDb);
                else
                    contactDb = Mapper.Map<Company, ContactsDb>(domainType as Company, contactDb);
            }
            else //New Contact
            {
                if (domainType is Person)
                    contactDb = Mapper.Map<Person, ContactsDb>(domainType as Person);
                else
                    contactDb = Mapper.Map<Company, ContactsDb>(domainType as Company);
            }
            return contactDb;
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="contactDbObject">The contact database object.</param>
        /// <returns></returns>
        public override Contact ConvertToDomain(ContactsDb contactDbObject)
        {
            var db = ObjectContextFactory.Create();
            var audit = db.ContactsAudit.FirstOrDefault(c => c.AuditAction == "I" && c.ContactID == contactDbObject.ContactID);

            if (contactDbObject.ContactType == ContactType.Person)
            {
                Person person = new Person();
                Mapper.Map<ContactsDb, Contact>(contactDbObject, person);

                if (audit != null)
                {
                    person.CreatedBy = audit.LastUpdatedBy;
                    DateTime date = audit.LastUpdatedOn.Value;
                    person.CreatedOn = new DateTime(date.Ticks - (date.Ticks % TimeSpan.TicksPerSecond));
                }
                return person;
            }
            else
            {
                Company company = new Company();
                Mapper.Map<ContactsDb, Contact>(contactDbObject, company);
                if (audit != null)
                {
                    company.CreatedBy = audit.LastUpdatedBy;
                    DateTime date = audit.LastUpdatedOn.Value;
                    company.CreatedOn = new DateTime(date.Ticks - (date.Ticks % TimeSpan.TicksPerSecond));
                }
                return company;
            }
        }

        /// <summary>
        /// Converts to domain states.
        /// </summary>
        /// <param name="states">The states.</param>
        /// <returns></returns>
        IEnumerable<State> convertToDomainStates(IEnumerable<StatesDb> states)
        {
            foreach (StatesDb state in states)
            {
                yield return new State() { Code = state.StateID, Name = state.StateName };
            }
        }

        /// <summary>
        /// Converts to domain country.
        /// </summary>
        /// <param name="countries">The countries.</param>
        /// <returns></returns>
        IEnumerable<Country> convertToDomainCountry(IEnumerable<CountriesDb> countries)
        {
            foreach (CountriesDb country in countries)
            {
                yield return new Country() { Code = country.CountryID, Name = country.CountryName };
            }
        }

        /// <summary>
        /// Converts to domain users.
        /// </summary>
        /// <param name="users">The users.</param>
        /// <returns></returns>
        IEnumerable<Owner> convertToDomainUsers(IEnumerable<UsersDb> users)
        {
            foreach (UsersDb user in users)
            {
                yield return new Owner() { OwnerId = user.UserID, OwnerName = user.FirstName + " " + user.LastName + " (" + user.PrimaryEmail + ") ", IsDeleted = user.IsDeleted };
            }
        }

        /// <summary>
        /// Finds the timelines asynchronous.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="OpportunityID">The opportunity identifier.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="module">The module.</param>
        /// <param name="period">The period.</param>
        /// <param name="PageName">Name of the page.</param>
        /// <param name="Activities">The activities.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <returns></returns>
        public async Task<IEnumerable<TimeLineContact>> FindTimelinesAsync(int? contactId, int? OpportunityID, int limit, int pageNumber, string module,
            string period, string PageName, string[] Activities, DateTime fromDate, DateTime toDate)
        {
            var db = ObjectContextFactory.Create();
            var records = (pageNumber - 1) * limit;
            var prevdate = DateTime.Now.ToUniversalTime();
            Expression<Func<ContactTimeLineDb, bool>> contactspredicate = PredicateBuilder.True<ContactTimeLineDb>();
            Expression<Func<OpportunitiesTimeLineDb, bool>> opportunitiespredicate = PredicateBuilder.True<OpportunitiesTimeLineDb>();
            if (Activities == null)
                Activities = new string[0];
            if (period != "")
            {
                Convert.ToInt32("-" + period);
                if (period == "1")
                {
                    prevdate = DateTime.MinValue;
                }
                else if (period == "2")
                {
                    prevdate = prevdate.AddDays(-6);
                }
                else if (period == "3")
                {
                    prevdate = prevdate.AddDays(-29);
                }
                else if (period == "4")
                {
                    prevdate = prevdate.AddDays(-59);
                }
                else if (period == "5")
                {
                    prevdate = prevdate.AddDays(-89);
                }
                else if (period == "6")
                {
                    fromDate = fromDate.AddDays(-1);
                    toDate = toDate.AddDays(1);
                }

            }
            if (PageName == "contacts")
            {

                contactspredicate = contactspredicate.And(x => x.ContactID == contactId).And(x => Activities.Contains(x.Module))
                                                     .And(x => x.AuditDate != null);
                if (period != "")
                {
                    if (period == "6")
                    {
                        contactspredicate = contactspredicate.And(x => x.AuditDate > fromDate && x.AuditDate < toDate);
                    }
                    else
                    {
                        contactspredicate = contactspredicate.And(x => x.AuditDate > prevdate);
                    }

                }
                IEnumerable<ContactTimeLineDb> timeLines = await db.GetContactTimeLines.AsExpandable().Where(contactspredicate)
                    .OrderByDescending(x => x.AuditDate).Skip(records).Take(limit).ToListAsync();
                IList<TimeLineContact> timelines = Mapper.Map<IEnumerable<ContactTimeLineDb>, IEnumerable<TimeLineContact>>(timeLines).ToList();
                return timelines;
            }
            else
            {
                opportunitiespredicate = opportunitiespredicate.And(x => x.OpportunityID == OpportunityID).And(x => Activities.Contains(x.Module));
                if (period != "")
                {
                    if (period == "6")
                    {
                        opportunitiespredicate = opportunitiespredicate.And(x => x.AuditDate > fromDate && x.AuditDate < toDate);
                    }
                    else
                    {
                        opportunitiespredicate = opportunitiespredicate.And(x => x.AuditDate > prevdate);
                    }
                }
                IEnumerable<OpportunitiesTimeLineDb> timeLines = await db.GetOpportunityTimeLines.AsExpandable()
                    .Where(opportunitiespredicate).OrderByDescending(x => x.AuditDate).Skip(records).Take(limit).ToListAsync();
                IList<TimeLineContact> timelines = Mapper.Map<IEnumerable<OpportunitiesTimeLineDb>, IEnumerable<TimeLineContact>>(timeLines).ToList();
                return timelines;
            }

        }

        /// <summary>
        /// Finds the timelines async2.
        /// </summary>
        /// <param name="ContactID">The contact identifier.</param>
        /// <param name="OpportunityID">The opportunity identifier.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="module">The module.</param>
        /// <param name="period">The period.</param>
        /// <param name="PageName">Name of the page.</param>
        /// <param name="Activities">The activities.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="AccountID">The account identifier.</param>
        /// <returns></returns>
        public async Task<IEnumerable<TimeLineContact>> FindTimelinesAsync2(int? ContactID, int? OpportunityID, int limit, int pageNumber, string module,
                string period, string PageName, string[] Activities, DateTime fromDate, DateTime toDate, int AccountID)
        {
            try
            {
                var db = ObjectContextFactory.Create();
                var records = (pageNumber - 1) * limit;
                var prevdate = DateTime.Now.ToUniversalTime();
                //var contactspredicate = PredicateBuilder.True<ContactTimeLineDb>();
                Expression<Func<OpportunitiesTimeLineDb, bool>> opportunitiespredicate = PredicateBuilder.True<OpportunitiesTimeLineDb>();
                if (Activities == null) Activities = new string[0];

                if (period != "")
                {
                    Convert.ToInt32("-" + period);
                    if (period == "1")
                    {
                        prevdate = DateTime.MinValue;
                    }
                    else if (period == "2")
                    {
                        prevdate = prevdate.AddDays(-6);
                    }
                    else if (period == "3")
                    {
                        prevdate = prevdate.AddDays(-29);
                    }
                    else if (period == "4")
                    {
                        prevdate = prevdate.AddDays(-59);
                    }
                    else if (period == "5")
                    {
                        prevdate = prevdate.AddDays(-89);
                    }
                    else if (period == "6")
                    {
                        fromDate = fromDate.AddDays(-1);
                        toDate = toDate.AddDays(1);
                    }

                }
                if (PageName == "contacts")
                {
                    //contactspredicate = contactspredicate.And(x => x.ContactID == ContactID).And(x => Activities.Contains(x.Module))
                    //                                     .And(x => x.AuditDate != null);
                    //if (period != "")
                    //{
                    //    if (period == "6")
                    //    {
                    //        contactspredicate = contactspredicate.And(x => x.AuditDate > fromDate && x.AuditDate < toDate);
                    //    }
                    //    else
                    //    {
                    //        contactspredicate = contactspredicate.And(x => x.AuditDate > prevdate);
                    //    }
                    //}

                    var procedureName = "[dbo].[GET_Contact_TimelineData]";

                    var parms = new List<SqlParameter>
                    {
                        new SqlParameter{ParameterName="@AccountID", Value=AccountID },
                        new SqlParameter{ParameterName="@ContactID", Value=ContactID }
                    };

                    CRMDb context = new CRMDb();
                    var timeLinecontent = context.ExecuteStoredProcedure<ContactTimeLineDb>(procedureName, parms).ToList();

                    var timelinedata = timeLinecontent.Where(i => i.ContactID == ContactID && Activities.Contains(i.Module) && i.AuditDate != null).ToList();
                    if (period != "")
                    {
                        if (period == "6")
                        {
                            timelinedata = timelinedata.Where(x => x.AuditDate > fromDate && x.AuditDate < toDate).ToList();
                        }
                        else if (period == "1")
                        {
                            //will take care
                        }
                        else
                        {
                            timelinedata = timelinedata.Where(x => x.AuditDate > prevdate).ToList();
                        }
                    }
                    timelinedata = timelinedata.Skip(records).Take(limit).ToList();
                    IList<TimeLineContact> timelines = Mapper.Map<IEnumerable<ContactTimeLineDb>, IEnumerable<TimeLineContact>>(timelinedata).ToList();
                    return timelines;
                }
                else
                {
                    opportunitiespredicate = opportunitiespredicate.And(x => x.OpportunityID == OpportunityID).And(x => Activities.Contains(x.Module));
                    if (period != "")
                    {
                        if (period == "6")
                        {
                            opportunitiespredicate = opportunitiespredicate.And(x => x.AuditDate > fromDate && x.AuditDate < toDate);
                        }
                        else
                        {
                            opportunitiespredicate = opportunitiespredicate.And(x => x.AuditDate > prevdate);
                        }
                    }
                    IEnumerable<OpportunitiesTimeLineDb> timeLines = await db.GetOpportunityTimeLines.AsExpandable()
                        .Where(opportunitiespredicate).OrderByDescending(x => x.AuditDate).Skip(records).Take(limit).ToListAsync();
                    IList<TimeLineContact> timelines = Mapper.Map<IEnumerable<OpportunitiesTimeLineDb>, IEnumerable<TimeLineContact>>(timeLines).ToList();
                    return timelines;
                }
            }
            catch (SqlException ex)
            {
                Logger.Current.Error("An error occurred in Import Data Stored Proc Insertion sql exception", ex);
                throw;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occurred in Import Data Stored Proc Insertion", ex);
                throw;
            }
        }

        /// <summary>
        /// Gets the opportunity summary.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="Period">The period.</param>
        /// <returns></returns>
        public OpportunitySummary GetOpportunitySummary(int contactId, string Period)
        {
            var prevdate = DateTime.MinValue;
            if (Period != "")
            {
                if (Period == "1")
                    prevdate = DateTime.MinValue;
                else if (Period == "2")
                    prevdate = DateTime.Now.AddDays(-6).ToUniversalTime();
                else if (Period == "3")
                    prevdate = DateTime.Now.AddDays(-29).ToUniversalTime();
                else if (Period == "4")
                    prevdate = DateTime.Now.AddDays(-59).ToUniversalTime();
                else if (Period == "5")
                    prevdate = DateTime.Now.AddDays(-89).ToUniversalTime();
            }
            var Potential = 0;
            decimal ValueOfPotential = 0;
            IList<int> NumberOfPotential = new List<int>();
            IList<int> NumberOfWon = new List<int>();
            int Won = 0;
            decimal ValueOfWon = 0;
            var db = ObjectContextFactory.Create();
            var procedure = "[dbo].[GetOpportunitySummary]";
            db.QueryStoredProc(procedure, (reader) =>
                                   {
                                       OpportunityObjects data = reader.Read<OpportunityObjects>().FirstOrDefault();
                                       if (data != null)
                                       {
                                           Potential = data.Potential;
                                           Won = data.Won;
                                           ValueOfPotential = data.ValueOfPotential;
                                           ValueOfWon = data.ValueOfWon;
                                       }
                                       NumberOfPotential = reader.Read<int>().ToList();
                                       NumberOfWon = reader.Read<int>().ToList();
                                   }, new { ContactID = contactId, Period = (prevdate == DateTime.MinValue) ? (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue : prevdate }, commandTimeout: 600);

            OpportunitySummary summary = new OpportunitySummary();
            summary.NumberOfPotential = Potential;
            summary.NumberOfWon = Won;
            summary.ValueOfPotential = ValueOfPotential;
            summary.ValueOfWon = ValueOfWon;
            summary.NumberOfPotentialOpportunities = NumberOfPotential;
            summary.NumberOfWonOpportunities = NumberOfWon;

            return summary;
        }

        public class OpportunityObjects
        {
            public int Potential { get; set; }
            public int Won { get; set; }
            public int ValueOfPotential { get; set; }
            public int ValueOfWon { get; set; }
        }

        /// <summary>
        /// Gets the persons count.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public int GetPersonsCount(int contactId, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var PersonCount = db.Contacts.Where(p => p.CompanyID == contactId && p.AccountID == accountId && p.IsDeleted == false).Count();
            return PersonCount;
        }

        /// <summary>
        /// Gets the persons of company.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public List<int> GetPersonsOfCompany(int contactId, int accountId)
        {
            List<int> NumberOfPesrons = new List<int>();
            var db = ObjectContextFactory.Create();
            var Persons = db.Contacts.Where(p => p.CompanyID == contactId && p.AccountID == accountId && p.IsDeleted == false).Select(p => new { p.ContactID }).ToArray();
            foreach (var person in Persons)
            {
                NumberOfPesrons.Add(person.ContactID);
            }
            return NumberOfPesrons;
        }

        /// <summary>
        /// Determines whether [is existed personsfor companies] [the specified contact ids].
        /// </summary>
        /// <param name="contactIds">The contact ids.</param>
        /// <returns></returns>
        public bool IsExistedPersonsforCompanies(int[] contactIds, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var companies = db.Contacts.Where(p => contactIds.Contains(p.ContactID) && p.ContactType == ContactType.Company && p.IsDeleted == false && p.AccountID == accountId).Select(c => c.ContactID).ToArray();
            var people = db.Contacts.Where(p => companies.Contains((int)p.CompanyID) && p.ContactType == ContactType.Person && p.IsDeleted == false && !contactIds.Contains(p.ContactID) && p.AccountID == accountId).Count();
            return people > 0;
        }

        /// <summary>
        /// Finds the timelines total records.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="OpportunityID">The opportunity identifier.</param>
        /// <param name="module">The module.</param>
        /// <param name="period">The period.</param>
        /// <param name="PageName">Name of the page.</param>
        /// <param name="Activities">The activities.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="timeLineGroup">The time line group.</param>
        /// <returns></returns>
        public int FindTimelinesTotalRecords(int? contactId, int? OpportunityID, string module, string period, string PageName,
            string[] Activities, DateTime fromDate, DateTime toDate, out IEnumerable<TimeLineGroup> timeLineGroup)
        {
            var db = ObjectContextFactory.Create();
            var prevdate = DateTime.Now.ToUniversalTime();
            if (period != "")
            {
                if (period == "1")
                {
                    prevdate = DateTime.MinValue;
                }
                else if (period == "2")
                {
                    prevdate = prevdate.AddDays(-6);
                }
                else if (period == "3")
                {
                    prevdate = prevdate.AddDays(-29);
                }
                else if (period == "4")
                {
                    prevdate = prevdate.AddDays(-59);
                }
                else if (period == "5")
                {
                    prevdate = prevdate.AddDays(-89);
                }
                else if (period == "6")
                {
                    fromDate = fromDate.AddDays(-1);
                    toDate = toDate.AddDays(1);
                }
            }

            Expression<Func<ContactTimeLineDb, bool>> contactspredicate = PredicateBuilder.True<ContactTimeLineDb>();
            Expression<Func<OpportunitiesTimeLineDb, bool>> opportunitiespredicate = PredicateBuilder.True<OpportunitiesTimeLineDb>();
            if (Activities == null)
                Activities = new string[0];

            if (PageName == "contacts")
            {
                contactspredicate = contactspredicate.And(x => x.ContactID == contactId).And(x => Activities.Contains(x.Module))
                                                     .And(x => x.AuditDate != null);
                if (period != "")
                {
                    if (period == "6")
                    {
                        contactspredicate = contactspredicate.And(x => x.AuditDate > fromDate && x.AuditDate < toDate);
                    }
                    else
                    {
                        contactspredicate = contactspredicate.And(x => x.AuditDate > prevdate);
                    }
                }

                timeLineGroup = db.GetContactTimeLines.AsExpandable().Where(contactspredicate).GroupBy(x => x.AuditDate.Value.Year)
                                                    .Select(g => new TimeLineGroup()
                                                    {
                                                        Year = g.Key,
                                                        YearCount = g.Count(),
                                                        Months = g.GroupBy(mnth => mnth.AuditDate.Value.Month)
                                                        .Select(o => new TimeLineMonthGroup()
                                                        {
                                                            Month = o.Key,
                                                            MonthCount = o.Count()
                                                        })
                                                    }).ToList();

                return db.GetContactTimeLines.AsExpandable().Where(contactspredicate).Count();
            }
            else
            {
                opportunitiespredicate = opportunitiespredicate.And(x => x.OpportunityID == OpportunityID).And(x => Activities.Contains(x.Module));
                if (period != "")
                {
                    if (period == "6")
                    {
                        opportunitiespredicate = opportunitiespredicate.And(x => x.AuditDate > fromDate && x.AuditDate < toDate);
                    }
                    else
                    {
                        opportunitiespredicate = opportunitiespredicate.And(x => x.AuditDate > prevdate);
                    }
                }

                timeLineGroup = db.GetOpportunityTimeLines.AsExpandable().Where(opportunitiespredicate).GroupBy(x => x.AuditDate.Year)
                                                    .Select(g => new TimeLineGroup()
                                                    {
                                                        Year = g.Key,
                                                        YearCount = g.Count(),
                                                        Months = g.GroupBy(mnth => mnth.AuditDate.Month)
                                                        .Select(o => new TimeLineMonthGroup()
                                                        {
                                                            Month = o.Key,
                                                            MonthCount = o.Count()
                                                        })
                                                    }).ToList();

                return db.GetOpportunityTimeLines.AsExpandable().Where(opportunitiespredicate).Count();
            }
        }

        /// <summary>
        /// Deletes the relation.
        /// </summary>
        /// <param name="relationId">The relation identifier.</param>
        public void DeleteRelation(int relationId)
        {
            var db = ObjectContextFactory.Create();
            var relationdb = db.ContactRelationshipMap.Where(n => n.ContactRelationshipMapID == relationId).FirstOrDefault();
            db.ContactRelationshipMap.Remove(relationdb);
            db.SaveChanges();
        }

        /// <summary>
        /// Changes the owner.
        /// </summary>
        /// <param name="OwnerId">The owner identifier.</param>
        /// <param name="Contacts">The contacts.</param>
        public void ChangeOwner(int? OwnerId, IEnumerable<int> Contacts, int accountId)
        {
            var db = ObjectContextFactory.Create();
            // List<int> contactIds = new List<int>();
            //  foreach (var contact in Contacts)
            // {
            var contactdb = db.Contacts.Where(p => Contacts.Contains(p.ContactID) && p.AccountID == accountId).ToArray();
            contactdb.ForEach(p => p.OwnerID = OwnerId);

            // contactIds.Add(contact.ContactID);
            //  }
            db.SaveChanges();
            //  return contactIds;
        }

        /// <summary>
        /// Updates the contact image.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="image">The image.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="lastUpdatedOn">The last updated on.</param>
        /// <param name="lastUpdatedBy">The last updated by.</param>
        /// <returns></returns>
        public int UpdateContactImage(int contactId, Image image, int accountId, int? userId, DateTime lastUpdatedOn, int? lastUpdatedBy)
        {
            var db = ObjectContextFactory.Create();
            if (image != null)
            {
                var varImage = db.Images.Where(Id => Id.ImageID == image.Id).FirstOrDefault();
                if (varImage != null)
                {
                    varImage.StorageName = image.StorageName;
                    varImage.FriendlyName = image.FriendlyName;
                    varImage.OriginalName = image.OriginalName;
                }
                else
                {
                    ImagesDb newImage = new ImagesDb();
                    newImage.ImageCategoryID = ImageCategory.ContactProfile;
                    newImage.AccountID = accountId;
                    newImage.FriendlyName = image.FriendlyName;
                    newImage.OriginalName = image.OriginalName;
                    newImage.StorageName = image.StorageName;
                    newImage.CreatedDate = DateTime.Now.ToUniversalTime();
                    newImage.CreatedBy = (int)userId;
                    db.Images.Add(newImage);
                    db.SaveChanges();

                    ContactsDb contacts = db.Contacts.Where(p => p.ContactID == contactId && p.AccountID == accountId).FirstOrDefault();
                    contacts.ImageID = newImage.ImageID;
                }
                var contactsdb = db.Contacts.Where(p => p.ContactID == contactId && p.AccountID == accountId).FirstOrDefault();
                contactsdb.LastUpdatedBy = lastUpdatedBy;
                contactsdb.LastUpdatedOn = lastUpdatedOn;
                contactsdb.ContactSource = ContactSource.Manual;
                db.SaveChanges();
            }
            return contactId;
        }

        /// <summary>
        /// Updates the name of the contact.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="firstName">The first name.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="lastUpdatedOn">The last updated on.</param>
        /// <param name="lastUpdatedBy">The last updated by.</param>
        /// <returns></returns>
        public int UpdateContactName(int contactId, string firstName, string lastName, DateTime lastUpdatedOn, int? lastUpdatedBy, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var contactdb = db.Contacts.Where(p => p.ContactID == contactId && p.AccountID == accountId).FirstOrDefault();
            contactdb.FirstName = firstName;
            contactdb.LastName = lastName;
            contactdb.LastUpdatedBy = lastUpdatedBy;
            contactdb.LastUpdatedOn = lastUpdatedOn;
            contactdb.ContactSource = ContactSource.Manual;
            db.SaveChanges();
            return contactdb.ContactID;
        }

        /// <summary>
        /// Updates the contact title.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="title">The title.</param>
        /// <param name="lastUpdatedOn">The last updated on.</param>
        /// <param name="lastUpdatedBy">The last updated by.</param>
        /// <returns></returns>
        public int UpdateContactTitle(int contactId, string title, DateTime lastUpdatedOn, int? lastUpdatedBy, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var contactdb = db.Contacts.Where(p => p.ContactID == contactId && p.AccountID == accountId).FirstOrDefault();
            contactdb.Title = title;
            contactdb.LastUpdatedBy = lastUpdatedBy;
            contactdb.LastUpdatedOn = lastUpdatedOn;
            contactdb.ContactSource = ContactSource.Manual;
            db.SaveChanges();
            return contactdb.ContactID;
        }

        /// <summary>
        /// Updates the name of the company.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="companyName">Name of the company.</param>
        /// <param name="lastUpdatedOn">The last updated on.</param>
        /// <param name="lastUpdatedBy">The last updated by.</param>
        /// <returns></returns>
        public int UpdateCompanyName(int contactId, string companyName, DateTime lastUpdatedOn, int? lastUpdatedBy, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var contactdb = db.Contacts.Where(p => p.ContactID == contactId && p.AccountID == accountId).FirstOrDefault();
            contactdb.Company = companyName;
            contactdb.LastUpdatedBy = lastUpdatedBy;
            contactdb.LastUpdatedOn = lastUpdatedOn;
            contactdb.ContactSource = ContactSource.Manual;
            db.SaveChanges();
            return contactdb.ContactID;
        }

        /// <summary>
        /// Updates the contact phone.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="phone">The phone.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="lastUpdatedOn">The last updated on.</param>
        /// <param name="lastUpdatedBy">The last updated by.</param>
        /// <returns></returns>
        public int UpdateContactPhone(int contactId, Phone phone, int accountId, DateTime lastUpdatedOn, int? lastUpdatedBy)
        {
            var db = ObjectContextFactory.Create();
            var contactPhonedb = db.ContactPhoneNumbers.Where(p => p.ContactID == contactId && p.IsPrimary == true && p.IsDeleted == false && p.AccountID == accountId).FirstOrDefault();
            var dropodwnvalue = db.DropdownValues.Where(c => c.DropdownValueID == phone.PhoneType).FirstOrDefault();
            if (contactPhonedb == null)
            {
                ContactPhoneNumbersDb map = new ContactPhoneNumbersDb();
                map.ContactID = contactId;
                map.PhoneNumber = phone.Number;
                map.PhoneType = phone.PhoneType;
                map.IsPrimary = phone.IsPrimary;
                map.CountryCode = phone.CountryCode;
                map.Extension = phone.Extension;
                map.AccountID = accountId;
                map.DropdownValues = dropodwnvalue;
                db.ContactPhoneNumbers.Add(map);
            }
            else
            {
                contactPhonedb.PhoneNumber = phone.Number;
                contactPhonedb.PhoneType = phone.PhoneType;
                contactPhonedb.IsPrimary = phone.IsPrimary;
                contactPhonedb.CountryCode = phone.CountryCode;
                contactPhonedb.Extension = phone.Extension;
                contactPhonedb.DropdownValues = dropodwnvalue;
            }

            var contactsdb = db.Contacts.Where(p => p.ContactID == contactId && p.AccountID == accountId).FirstOrDefault();
            contactsdb.LastUpdatedBy = lastUpdatedBy;
            contactsdb.LastUpdatedOn = lastUpdatedOn;
            contactsdb.ContactSource = ContactSource.Manual;
            db.SaveChanges();

            return contactId;
        }

        /// <summary>
        /// Updates the contact lifecycle stage.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="lifecycleStage">The lifecycle stage.</param>
        /// <param name="lastUpdatedOn">The last updated on.</param>
        /// <param name="lastUpdatedBy">The last updated by.</param>
        /// <returns></returns>
        public int UpdateContactLifecycleStage(int contactId, short lifecycleStage, DateTime lastUpdatedOn, int? lastUpdatedBy, int accountId)
        {
            var db = ObjectContextFactory.Create();
            ContactsDb contactDb = db.Contacts.Where(w => w.ContactID == contactId && w.AccountID == accountId).FirstOrDefault();
            if (contactDb != null)
            {
                this.UpdateLifecycleStage(contactId, lifecycleStage);       //To get life cycle change update in timeline
                contactDb.LifecycleStage = lifecycleStage;
                contactDb.LastUpdatedBy = lastUpdatedBy;
                contactDb.LastUpdatedOn = lastUpdatedOn;
                contactDb.ContactSource = ContactSource.Manual;
                contactDb.SourceType = null;
                db.SaveChanges();
                return contactDb.ContactID;
            }
            return contactId;
        }

        /// <summary>
        /// Updates the contact addresses.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="address">The address.</param>
        /// <param name="lastUpdatedOn">The last updated on.</param>
        /// <param name="lastUpdatedBy">The last updated by.</param>
        /// <returns></returns>
        public int UpdateContactAddresses(int contactId, Address address, DateTime lastUpdatedOn, int? lastUpdatedBy, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var contactsDb = db.Contacts.Include(p => p.Addresses).Where(c => c.ContactID == contactId && c.AccountID == accountId).FirstOrDefault();

            //   var primaryAddress = contactAddressdb.Where(p => p.IsDefault == true).FirstOrDefault();

            AddressesDb addressDb = Mapper.Map<Address, AddressesDb>(address);

            var primaryAddress = contactsDb.Addresses.Where(p => p.IsDefault == true).FirstOrDefault();

            if (primaryAddress != null)
            {
                var addressvalue = db.Addresses.Where(p => p.AddressID == primaryAddress.AddressID).FirstOrDefault();

                if (addressvalue != null)
                {
                    addressvalue.AddressLine1 = address.AddressLine1;
                    addressvalue.CountryID = address.Country.Code;
                    addressvalue.City = address.City;
                    addressvalue.AddressLine2 = address.AddressLine2;
                    addressvalue.AddressTypeID = address.AddressTypeID;
                    addressvalue.StateID = address.State.Code;
                    addressvalue.ZipCode = address.ZipCode;
                }
            }
            else
            {
                contactsDb.Addresses.Add(addressDb);
            }
            var contactsdb = db.Contacts.Where(p => p.ContactID == contactId && p.AccountID == accountId).FirstOrDefault();
            contactsdb.LastUpdatedBy = lastUpdatedBy;
            contactsdb.LastUpdatedOn = lastUpdatedOn;
            contactsdb.ContactSource = ContactSource.Manual;
            db.SaveChanges();
            return contactId;
        }

        /// <summary>
        /// Updates the contact lead source.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="leadsourceId">The leadsource identifier.</param>
        /// <param name="lastUpdatedOn">The last updated on.</param>
        /// <param name="lastUpdatedBy">The last updated by.</param>
        /// <returns></returns>
        public int UpdateContactLeadSource(int contactId, short leadsourceId, DateTime lastUpdatedOn, int? lastUpdatedBy, int accountId)
        {
            var db = ObjectContextFactory.Create();
            ContactLeadSourceMapDb contactLeadSourceDb = db.ContactLeadSourcesMap.Where(w => w.ContactID == contactId && w.IsPrimaryLeadSource == true).FirstOrDefault();
            if (contactLeadSourceDb != null)
            {
                contactLeadSourceDb.IsPrimaryLeadSource = false;
                var oldLeadSource = db.ContactLeadSourcesMap.Where(w => w.LeadSouceID == leadsourceId && w.ContactID == contactId).FirstOrDefault();
                if (oldLeadSource == null)
                {
                    ContactLeadSourceMapDb leadMap = new ContactLeadSourceMapDb() { ContactID = contactId, IsPrimaryLeadSource = true, LastUpdatedDate = DateTime.UtcNow, LeadSouceID = leadsourceId };
                    db.ContactLeadSourcesMap.Add(leadMap);
                }
                else
                {
                    oldLeadSource.LastUpdatedDate = DateTime.UtcNow;
                    oldLeadSource.IsPrimaryLeadSource = true;
                }
                var contactsdb = db.Contacts.Where(p => p.ContactID == contactId && p.AccountID == accountId).FirstOrDefault();
                contactsdb.LastUpdatedBy = lastUpdatedBy;
                contactsdb.LastUpdatedOn = lastUpdatedOn;
                contactsdb.ContactSource = ContactSource.Manual;
                db.SaveChanges();
                return contactLeadSourceDb.ContactID;
            }
            else
            {
                var db_new = ObjectContextFactory.Create();
                var sql = @"INSERT INTO ContactLeadSourceMap VALUES(@contactId,@leadsourceId,1,GETUTCDATE())";
                db_new.Execute(sql, new { contactId = contactId, leadsourceId = leadsourceId });
            }
            return contactId;
        }

        /// <summary>
        /// Updates the contact email.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="email">The email.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="lastUpdatedOn">The last updated on.</param>
        /// <param name="lastUpdatedBy">The last updated by.</param>
        /// <returns></returns>
        public int UpdateContactEmail(int contactId, Email email, int accountId, DateTime lastUpdatedOn, int? lastUpdatedBy)
        {
            Logger.Current.Informational("Request received for updating contact email in repository layer.");
            var db = ObjectContextFactory.Create();
            ContactEmailsDb contactEmailDb = db.ContactEmails.Where(p => p.ContactID == contactId && p.AccountID == accountId && p.IsPrimary == true && p.IsDeleted == false).FirstOrDefault();
            if (contactEmailDb != null)
            {
                Logger.Current.Informational("Contact has an email, updating with status : " + (byte)email.EmailStatusValue);
                contactEmailDb.Email = email.EmailId;
                contactEmailDb.EmailStatus = (byte)email.EmailStatusValue;
                contactEmailDb.IsPrimary = email.IsPrimary;
            }
            else
            {
                ContactEmailsDb map = new ContactEmailsDb();
                map.ContactID = contactId;
                map.Email = email.EmailId;
                map.EmailStatus = (byte)email.EmailStatusValue;
                map.IsPrimary = email.IsPrimary;
                map.AccountID = accountId;
                db.ContactEmails.Add(map);
            }
            var contactsdb = db.Contacts.Where(p => p.ContactID == contactId && p.AccountID == accountId).FirstOrDefault();
            contactsdb.LastUpdatedBy = lastUpdatedBy;
            contactsdb.LastUpdatedOn = lastUpdatedOn;
            contactsdb.ContactSource = ContactSource.Manual;
            db.SaveChanges();
            return contactId;
        }

        /// <summary>
        /// Inserts the contact image.
        /// </summary>
        /// <param name="contactImage">The contact image.</param>
        /// <param name="db">The database.</param>
        public void InsertContactImage(ContactImagesDb contactImage, CRMDb db)
        {
            //var ContactImagesDb = db.ContactImagesDb.Where(Id => Id.StorageName == contactImage.StorageName).SingleOrDefault();
            //ContactImagesDb.ImageContent = ContactImagesDb.ImageContent;
        }

        /// <summary>
        /// Fetches the images.
        /// </summary>
        /// <param name="contacts">The contacts.</param>
        /// <returns></returns>
        public IEnumerable<Contact> FetchImages(IEnumerable<Contact> contacts)
        {
            List<Contact> updatedContact = new List<Contact>();
            Guid[] ProfileImageKeys = contacts.Select(p => p.ProfileImageKey.HasValue ? p.ProfileImageKey.Value : Guid.Empty).ToArray();
            var db = ObjectContextFactory.Create();
            var varContactImageContent = (from c in db.ContactImagesDb where ProfileImageKeys.Contains(c.StorageName) select new { c.StorageName, c.ImageContent }).ToList();
            foreach (var varItems in contacts)
            {
                var Image = varContactImageContent.Where(id => id.StorageName == varItems.ProfileImageKey).Select(c => c.ImageContent).FirstOrDefault();
                if (Image != null) { varItems.ImageUrl = Image; }
                updatedContact.Add(varItems);
            }
            return (IEnumerable<Contact>)updatedContact;
        }

        /// <summary>
        /// Gets the email signatures by.
        /// </summary>
        /// <param name="UserID">The user identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Email> GetEmailSignaturesBy(int UserID, int accountId)
        {
            var db = ObjectContextFactory.Create();
            List<AccountEmailsDb> emailsdb = db.AccountEmails.Where(E => E.UserID == UserID && E.AccountID == accountId).ToList();
            foreach (AccountEmailsDb emaildb in emailsdb)
            {
                yield return Mapper.Map<AccountEmailsDb, Email>(emaildb);
            }
        }

        /// <summary>
        /// Gets the lead score.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public int GetLeadScore(int contactId)
        {
            var db = ObjectContextFactory.Create();

            var score = db.Contacts.Where(c => c.ContactID == contactId).Select(c => c.LeadScore).FirstOrDefault();
            return score;
        }

        /// <summary>
        /// Gets the contact lead score.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="period">The period.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public async Task<int> GetContactLeadScore(int contactId, DateTime period, int accountId)
        {
            var db = ObjectContextFactory.Create();
            int? score = await db.LeadScores.Where(c => c.ContactID == contactId && c.AddedOn > period).SumAsync(p => (int?)p.Score);
            if (score == null)
                return 0;
            return (int)score;
        }

        /// <summary>
        /// Gets the contacts by tag.
        /// </summary>
        /// <param name="TagID">The tag identifier.</param>
        /// <param name="tagType">Type of the tag.</param>
        /// <returns></returns>
        public IEnumerable<int> GetContactsByTag(int tagID, int tagType, int accountID)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<int> contactTags = new List<int>();
            Logger.Current.Verbose("Fetching contacts for tag: " + tagID);
            var validStatuses = new List<byte>()
                {
                    (byte)EmailStatus.NotVerified, (byte)EmailStatus.Verified, (byte)EmailStatus.SoftBounce
                };
            if (tagType == (int)RecipientType.All || tagType == 0)
            {
                var sql = @" SELECT CTM.ContactID FROM ContactTagMap (NOLOCK) CTM
                                WHERE CTM.AccountID = @accountId AND CTM.TagID = @tagId";

                contactTags = db.Get<int>(sql, new { tagId = tagID, accountId = accountID });
            }
            else if (tagType == (int)RecipientType.Active)
            {

                var sql = @" SELECT AC.ContactID FROM ActiveContacts (NOLOCK) AC
                                INNER JOIN ContactTagMap CTM (NOLOCK) ON CTM.ContactID = AC.ContactID AND AC.AccountId = CTM.AccountId
                                WHERE CTM.AccountID = @accountId AND AC.IsDeleted = 0 AND AC.EmailStatus IN (SELECT DATAVALUE FROM dbo.SPlit(@emailStatuses,',')) AND CTM.TagID = @tagId";

                contactTags = db.Get<int>(sql, new { tagId = tagID, accountId = accountID, emailStatuses = string.Join(",", validStatuses.ToList()) });
            }
            Logger.Current.Informational("GetContactsByTag. Contacts found: " + contactTags.Count());
            return contactTags;
        }

        /// <summary>
        /// Gets the action related contacts.
        /// </summary>
        /// <param name="ActionID">The action identifier.</param>
        /// <returns></returns>
        public IList<int> GetActionRelatedContacts(int ActionID)
        {
            var db = ObjectContextFactory.Create();
            IList<int> contactTags = db.ActionContacts.Where(i => i.ActionID == ActionID).Select(i => i.ContactID).ToList();
            return contactTags;
        }

        /// <summary>
        /// Updates the contact email.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="email">The email.</param>
        /// <param name="emailStatus">The email status.</param>
        /// <param name="complainedStatus">The complained status.</param>
        /// <returns></returns>
        public Email UpdateContactEmail(int contactId, string email, EmailStatus emailStatus, int accountId, short? complainedStatus)
        {
            if (email != null && contactId != 0)
            {
                using (var db = new CRMDb())
                {
                    ContactEmailsDb contactEmailDb = db.ContactEmails.Where(c => c.ContactID == contactId && c.AccountID == accountId && c.Email == email && c.IsDeleted == false).FirstOrDefault();
                    if (contactEmailDb != null)
                    {
                        if (complainedStatus == null)
                            contactEmailDb.EmailStatus = (short)emailStatus;
                        else if (complainedStatus != null)
                            contactEmailDb.EmailStatus = (short)complainedStatus;
                        Email contactEmail = Mapper.Map<ContactEmailsDb, Email>(contactEmailDb);

                        db.Entry(contactEmailDb).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        return contactEmail;
                    }
                    else
                        return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the contact identifier by email and campaign.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns></returns>
        public int GetContactIdByEmailAndCampaign(string email, int campaignId, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var campaignRecipient = db.CampaignRecipients.Where(cr => cr.CampaignID == campaignId && cr.To == email && cr.AccountID == accountId).FirstOrDefault();
            return (campaignRecipient != null) ? campaignRecipient.ContactID : 0;
        }

        /// <summary>
        /// Gets the contact campaign summary.
        /// </summary>
        /// <param name="contactID">The contact identifier.</param>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        public Task<CampaignStatistics> GetContactCampaignSummary(int contactID, DateTime period, int accountId)
        {
            CampaignStatistics campaignstatistics = new CampaignStatistics();
            var db = ObjectContextFactory.Create();

            db.QueryStoredProc("[dbo].[GET_Contact_Campaign_Summary]", (reader) =>
            {
                campaignstatistics =  reader.Read<CampaignStatistics>().FirstOrDefault();
            }, new { AccountID = accountId, ContactID = contactID, Date = period });

            return Task<CampaignStatistics>.Run(() => campaignstatistics);
        }

        /// <summary>
        /// Gets the contact Email Stats.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="contactId"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public Task<EmailStatistics> GetContactEmailStatistics(int accountId, int contactId, DateTime period)
        {
            EmailStatistics emailstatistics = new EmailStatistics();
            var db = ObjectContextFactory.Create();

            db.QueryStoredProc("[dbo].[Get_Email_Stats_Summary]", (reader) =>
            {
                emailstatistics = reader.Read<EmailStatistics>().FirstOrDefault();
            }, new { AccountID = accountId, ContactID = contactId, Date = period });

            return Task<EmailStatistics>.Run(() => emailstatistics);
        }

        /// <summary>
        /// Tracks the contact ip address.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="stiTrackingId">The sti tracking identifier.</param>
        public void TrackContactIPAddress(int contactId, string ipAddress, string stiTrackingId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var contactIpMapping = db.ContactIPAddresses.Where(c => c.ContactID == contactId && c.UniqueTrackingID == stiTrackingId).FirstOrDefault();
                if (contactIpMapping == null)
                {
                    db.ContactIPAddresses.Add(new ContactIPAddressesDb()
                    {
                        ContactID = contactId,
                        IPAddress = ipAddress,
                        IdentifiedOn = DateTime.Now.ToUniversalTime(),
                        UniqueTrackingID = stiTrackingId
                    });
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Gets the contact lead score list.
        /// </summary>
        /// <param name="accountID">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Contact> GetContactLeadScoreList(int accountID)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<ContactsDb> contactlist = db.Contacts.Where(c => c.AccountID == accountID && c.ContactType == ContactType.Person);
            List<Contact> personList = new List<Contact>();
            foreach (ContactsDb contact in contactlist)
            {
                personList.Add(ConvertToDomain(contact));
            }
            return personList;
        }

        /// <summary>
        /// Changes the lifecycle.
        /// </summary>
        /// <param name="dropdownValueId">The dropdown value identifier.</param>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public Person ChangeLifecycle(short dropdownValueId, int contactId, int accountId)
        {
            var db = ObjectContextFactory.Create();
            Person contact = default(Person);
            if (dropdownValueId != 0 && contactId != 0)
            {
                ContactsDb contactDb = db.Contacts.Where(w => w.ContactID == contactId && w.AccountID == accountId).FirstOrDefault();
                contactDb.LifecycleStage = dropdownValueId;
                db.SaveChanges();
                ContactsDb updatedContact = db.Contacts.Where(w => w.ContactID == contactId && w.AccountID == accountId).Include(c => c.Addresses).Include(c => c.Communication).Include(c => c.ContactEmails)
                                            .Include(c => c.ContactPhones).Include(c => c.Image).Include(c => c.CustomFields).FirstOrDefault();
                if (updatedContact != null)
                {
                    updatedContact.ContactEmails = updatedContact.ContactEmails.Where(i => i.IsDeleted == false).ToList();
                    updatedContact.ContactPhones = updatedContact.ContactPhones.Where(p => p.IsDeleted == false).ToList();
                }
                contact = Mapper.Map<ContactsDb, Person>(updatedContact);
            }
            return contact;
        }

        /// <summary>
        /// Updates the last touched.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="lastContactedOn">The last contacted on.</param>
        /// <param name="moduleId">The module identifier.</param>
        public void UpdateLastTouched(int contactId, DateTime lastContactedOn, byte moduleId, int accountId)
        {
            var db = ObjectContextFactory.Create();
            if (contactId != 0)
            {
                ContactsDb contactDb = db.Contacts.Where(w => w.ContactID == contactId && w.AccountID == accountId).FirstOrDefault();
                contactDb.LastContacted = lastContactedOn;
                contactDb.LastContactedThrough = moduleId;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Changes the owner.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="contactId">The contact identifier.</param>
        public void ChangeOwner(int userId, int contactId)
        {
            var db = ObjectContextFactory.Create();
            if (userId != 0)
            {
                ContactsDb contact = db.Contacts.Where(w => w.ContactID == contactId).FirstOrDefault();
                contact.OwnerID = userId;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Finds the contact ids by ip.
        /// </summary>
        /// <param name="ip">The ip.</param>
        /// <returns></returns>
        public IEnumerable<int> FindContactIdsByIP(string ip)
        {
            using (var db = ObjectContextFactory.Create())
            {
                IEnumerable<int> contactIds = db.ContactIPAddresses.Where(c => c.IPAddress == ip).Select(c => c.ContactID).ToList();
                return contactIds;
            }
        }

        /// <summary>
        /// Finds the contact ids by reference ids.
        /// </summary>
        /// <param name="referenceIds">The reference ids.</param>
        /// <returns></returns>
        public IEnumerable<int> FindContactIdsByReferenceIds(IEnumerable<string> referenceIds)
        {
            using (var db = ObjectContextFactory.Create())
            {
                IEnumerable<int> contactIds = db.Contacts.Where(c => referenceIds.Contains(c.ReferenceId.ToString())).AsNoTracking().Select(c => c.ContactID);
                return contactIds;
            }
        }

        /// <summary>
        /// Gets the top five lead sources.
        /// </summary>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="accountID">The account identifier.</param>
        /// <param name="OwnerIds">The owner ids.</param>
        /// <returns></returns>
        public IEnumerable<dynamic> GetTopFiveLeadSources(DateTime fromDate, DateTime toDate, int accountID, int[] OwnerIds)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var contcats = (db.ContactsAudit.Where(ca => ca.AccountID == accountID && ca.AuditAction == "I" &&
                   DbFunctions.TruncateTime(ca.LastUpdatedOn) > fromDate.Date && DbFunctions.TruncateTime(ca.LastUpdatedOn) < toDate.Date).Select(c => c.ContactID));
                var contactList = db.ContactLeadSourcesMap.Where(cl => contcats.Contains(cl.ContactID))
                    .GroupBy(g => g.LeadSouceID).Select(gs => new
                    {
                        ContactIDCount = gs.Select(c => c.ContactID).Distinct().Count(),
                        LeadSourceID = gs.Select(c => c.LeadSouceID).FirstOrDefault(),
                        LeadSource = gs.Select(c => c.LeadSource).FirstOrDefault().DropdownValue,
                    }).OrderByDescending(cl => cl.ContactIDCount).Take(5).ToList();
                return contactList;
            }
        }

        /// <summary>
        /// Gets the top leads by custom date.
        /// </summary>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="accountID">The account identifier.</param>
        /// <param name="OwnerIds">The owner ids.</param>
        /// <param name="IsCompared">if set to <c>true</c> [is compared].</param>
        /// <param name="previousDate">The previous date.</param>
        /// <param name="previousLeadCount">The previous lead count.</param>
        /// <returns></returns>
        public IEnumerable<dynamic> GetTopLeadsByCustomDate(DateTime fromDate, DateTime toDate, int accountID, int[] OwnerIds, bool IsCompared
            , DateTime previousDate, out int previousLeadCount)
        {
            using (var db = ObjectContextFactory.Create())
            {

                var defaultLeadType = db.DropdownValues.Where(d => d.AccountID == accountID && d.DropdownID == (byte)DropdownFieldTypes.LifeCycle &&
                    d.DropdownValueTypeID == (short)DropdownValueTypes.Lead).Select(c => c.DropdownValueID).FirstOrDefault();
                var topLeadsByDate = db.ContactsAudit.Where(ca => ca.AccountID == accountID && ca.AuditAction == "I" &&
                   DbFunctions.TruncateTime(ca.LastUpdatedOn) > fromDate.Date && DbFunctions.TruncateTime(ca.LastUpdatedOn) < toDate.Date
                   && ca.LeadSource != null && ca.LifecycleStage == defaultLeadType).GroupBy(g => g.LastUpdatedOn.Value).Select(gs => new
                   {
                       ContactsCount = gs.Select(c => c.ContactID).Distinct().Count(),
                       Createdon = gs.Select(c => c.LastUpdatedOn).FirstOrDefault()
                   }).ToList();
                if (IsCompared == true)
                {
                    previousLeadCount = db.ContactsAudit.Where(ca => ca.AccountID == accountID && ca.AuditAction == "I" &&
                     DbFunctions.TruncateTime(ca.LastUpdatedOn) > fromDate.Date && DbFunctions.TruncateTime(ca.LastUpdatedOn) < toDate.Date
                     && ca.LeadSource != null && ca.LifecycleStage == defaultLeadType).GroupBy(g => g.LastUpdatedOn.Value).Select(gs => new
                     {
                         ContactsCount = gs.Select(c => c.ContactID).Distinct().Count(),
                         Createdon = gs.Select(c => c.LastUpdatedOn).FirstOrDefault()
                     }).Count();
                }
                else
                    previousLeadCount = 0;

                return topLeadsByDate;
            }
        }

        /// <summary>
        /// Filters the known ips.
        /// </summary>
        /// <param name="ips">The ips.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<string> FilterKnownIps(IEnumerable<string> ips, int accountId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                IEnumerable<string> knownIps = db.ContactIPAddresses.Where(c => ips.Contains(c.IPAddress) && c.Contact.AccountID == accountId)
                    .Select(c => c.IPAddress).ToList();
                return knownIps.Distinct();
            }
        }

        /// <summary>
        /// Filters the known identities.
        /// </summary>
        /// <param name="identities">The identities.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<string> FilterKnownIdentities(IEnumerable<string> identities, int accountId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"SELECT CIPA.UniqueTrackingID FROM ContactIPAddresses (NOLOCK) CIPA 
                            JOIN Contacts (NOLOCK) C ON C.ContactID = CIPA.ContactID
                            WHERE CIPA.UniqueTrackingID IN @Identities AND C.AccountID=@AccountId";
                IEnumerable<string> knownIdentities = db.Get<string>(sql, new { Identities = identities, AccountId = accountId }).ToList();

                return knownIdentities.Distinct();
            }
        }

        /// <summary>
        /// News the leads pie chart details.
        /// </summary>
        /// <param name="accountID">The account identifier.</param>
        /// <param name="userID">The user identifier.</param>
        /// <param name="isAccountAdmin">if set to <c>true</c> [is account admin].</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <returns></returns>
        public IEnumerable<DashboardPieChartDetails> NewLeadsPieChartDetails(int accountID, int userID, bool isAccountAdmin, DateTime fromDate, DateTime toDate)
        {
            using (var db = ObjectContextFactory.Create())
            {
                Logger.Current.Informational("Created the procedure name to get the NewLeadsPieChart ");
                var procedureName = "[dbo].[GET_Account_NewLeads_PieChart]";
                Logger.Current.Informational("Created the parametes for the procedure");

                var parms = new List<SqlParameter>
                {
                    new SqlParameter{ParameterName ="@AccountID", Value= accountID},
                    new SqlParameter{ParameterName ="@FromDate", Value= fromDate.Date},
                    new SqlParameter{ParameterName="@ToDate ", Value = toDate.Date},
                    new SqlParameter{ParameterName="@IsAdmin", Value = isAccountAdmin},
                    new SqlParameter{ParameterName="@OwnerID", Value = userID},
                };
                // var lstcontacts = context.ExecuteStoredProcedure<int>(procedureName, parms);
                return db.ExecuteStoredProcedure<DashboardPieChartDetails>(procedureName, parms);
            }
        }

        /// <summary>
        /// News the leads area chart details.
        /// </summary>
        /// <param name="accountID">The account identifier.</param>
        /// <param name="userID">The user identifier.</param>
        /// <param name="isAccountAdmin">if set to <c>true</c> [is account admin].</param>
        /// <param name="previousCount">The previous count.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <returns></returns>
        public IEnumerable<DashboardAreaChart> NewLeadsAreaChartDetails(int accountID, int userID, bool isAccountAdmin, out int previousCount, DateTime fromDate, DateTime toDate)
        {
            using (var db = ObjectContextFactory.Create())
            {
                Logger.Current.Informational("Created the procedure name to get the NewLeadsPieChart ");
                var procedureName = "[dbo].[GET_Account_NewLeads_AreaChart]";
                Logger.Current.Informational("Created the parametes for the procedure");

                var outputParam = new SqlParameter { ParameterName = "@PreviousValue", SqlDbType = System.Data.SqlDbType.Int, Direction = System.Data.ParameterDirection.Output };
                var parms = new List<SqlParameter>
                {
                    new SqlParameter{ParameterName ="@AccountID", Value = accountID},
                    new SqlParameter{ParameterName ="@FromDate", Value = fromDate.Date},
                    new SqlParameter{ParameterName="@ToDate ", Value = toDate.Date},
                    new SqlParameter{ParameterName="@IsAdmin", Value = isAccountAdmin},
                    new SqlParameter{ParameterName="@OwnerID", Value = userID},
                  outputParam
                };
                // var lstcontacts = context.ExecuteStoredProcedure<int>(procedureName, parms);
                var details = db.ExecuteStoredProcedure<DashboardAreaChart>(procedureName, parms);
                previousCount = (int)outputParam.Value;
                return details;
            }
        }

        /// <summary>
        /// Gets the contacts for workflow.
        /// </summary>
        /// <param name="WorkflowID">The workflow identifier.</param>
        /// <param name="WorkflowContactState">State of the workflow contact.</param>
        /// <returns></returns>
        public IEnumerable<int> GetContactsForWorkflow(short WorkflowID, WorkflowContactsState WorkflowContactState)
        {
            string procedure = "[dbo].[GetContactsForWorkflow]";
            var db = ObjectContextFactory.Create();
            IEnumerable<int> Ids = new List<int>();
            db.QueryStoredProc(procedure, (reader) => { Ids = reader.Read<int>(); }, new { WorkflowId = WorkflowID, State = (byte)WorkflowContactState });
            return Ids;
        }

        /// <summary>
        /// Gets the contact web visits count.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        public async Task<int> GetContactWebVisitsCount(int contactId, DateTime period)
        {
            using (var db = ObjectContextFactory.Create())
            {
                int webVisitsCount = await db.WebVisits.Where(w => w.ContactID == contactId && w.IsVisit == true && w.VisitedOn > period).CountAsync();
                return webVisitsCount;
            }
        }

        /// <summary>
        /// Gets the contact web visits.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        public IEnumerable<WebVisit> GetContactWebVisits(int contactId, string period)
        {
            var prevDate = DateTime.MinValue;
            if (period != "")
            {
                if (period == "1")
                    prevDate = DateTime.MinValue;
                else if (period == "2")
                    prevDate = DateTime.Now.AddDays(-6).ToUniversalTime();
                else if (period == "3")
                    prevDate = DateTime.Now.AddDays(-29).ToUniversalTime();
                else if (period == "4")
                    prevDate = DateTime.Now.AddDays(-59).ToUniversalTime();
                else if (period == "5")
                    prevDate = DateTime.Now.AddDays(-89).ToUniversalTime();
            }
            using (var db = ObjectContextFactory.Create())
            {
                var webVisitsDb = db.WebVisits.Where(w => w.ContactID == contactId && w.VisitedOn > prevDate).OrderByDescending(w => w.VisitedOn);

                var webVisits = Mapper.Map<IEnumerable<WebVisitsDb>, IEnumerable<WebVisit>>(webVisitsDb);
                return webVisits;
            }
        }

        /// <summary>
        /// Gets the contacts for campaign.
        /// </summary>
        /// <param name="CampaignID">The campaign identifier.</param>
        /// <param name="CampaignDrillDownActivity">The campaign drill down activity.</param>
        /// <param name="CampaignLinkID">The campaign link identifier.</param>
        /// <returns></returns>
        public IEnumerable<int> GetContactsForCampaign(int CampaignID, CampaignDrillDownActivity CampaignDrillDownActivity, int accountId, int? CampaignLinkID)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<int> contacts = null;
            db.QueryStoredProc("[dbo].[Get_Campaign_Related_Contacts]", (reader) =>
            {
                contacts = reader.Read<int>().ToList();
            },
            new
            {
                campaignID = CampaignID,
                accountId = accountId,
                drillDownActivity = (int)CampaignDrillDownActivity,
                linkId = CampaignLinkID.HasValue ? CampaignLinkID : 0
            });

            Logger.Current.Informational("Count of contacts : " + (contacts.IsAny()? contacts.Count():0));
            return contacts;
        }

        /// <summary>
        /// Gets the workflow contacts for campaign.
        /// </summary>
        /// <param name="WorkflowID">The workflow identifier.</param>
        /// <param name="CampaignID">The campaign identifier.</param>
        /// <param name="CampaignDrillDownActivity">The campaign drill down activity.</param>
        /// <returns></returns>
        public IEnumerable<int> GetWorkflowContactsForCampaign(short WorkflowID, int CampaignID, CampaignDrillDownActivity CampaignDrillDownActivity, DateTime? fromDate, DateTime? toDate)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<int> contacts = null;
            var procedureName = "[dbo].[Get_WorkFlow_Campaign_Related_Contacts]";
            var parms = new List<SqlParameter>
                {
                    new SqlParameter{ParameterName ="@WorkflowId", Value= WorkflowID },
                    new SqlParameter{ParameterName ="@CampaignId", Value= CampaignID },
                    new SqlParameter{ParameterName ="@CampaignDrillDownType", Value= (byte)CampaignDrillDownActivity},
                    new SqlParameter{ParameterName ="@FromDate", Value= fromDate},
                    new SqlParameter{ParameterName ="@EndDate", Value= toDate}

                };

            contacts = db.ExecuteStoredProcedure<int>(procedureName, parms);
            return contacts;
        }

        /// <summary>
        /// Updates the last touched information.
        /// </summary>
        /// <param name="LastTouchedInformation">The last touched information.</param>
        /// <param name="ModuleID">The module identifier.</param>
        public void UpdateLastTouchedInformation(List<LastTouchedDetails> LastTouchedInformation, AppModules ModuleID, short? ActionType)
        {
            Logger.Current.Informational("InsertLastTouchedInformation method called with lasttouched information and module id");
            //DataTable lasttouchedinfo = ToDataTable<LeadAdapterJobLogDetails>(leadAdapterJobLog.LeadAdapterJobLogDetails.ToList());
            try
            {
                var procedureName = "[dbo].[Update_LastTouchedInformation]";
                DataTable lasttoucheddata = ToDataTable<LastTouchedDetails>(LastTouchedInformation);
                var parms = new List<SqlParameter>
                {
                    new SqlParameter{ParameterName="@ModuleID", Value = ((byte)ModuleID == (byte)AppModules.ContactActions ? (short)ActionType : (short)ModuleID)},
                    new SqlParameter{ParameterName="@LastTouchedInformation", Value=lasttoucheddata, SqlDbType= SqlDbType.Structured, TypeName="dbo.LastTouchedDetails" }
                };
                Logger.Current.Informational("Created the instance for the crmdb method and called the execute stored proecedure method with the instance");
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                CRMDb context = new CRMDb();
                context.ExecuteStoredProcedure(procedureName, parms, 360);
                sw.Stop();
                var te = sw.Elapsed;
                Logger.Current.Informational("Time taken to update LastTouched information :" + te);
            }
            catch (Exception ex)
            {
                Logger.Current.Informational("An execpetion has occuered in the InsertLastTouchedInformation method: " + ex);
                throw;
            }
        }

        /// <summary>
        /// To the data table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                switch (prop.PropertyType.Name)
                {
                    case "Int32":
                        dataTable.Columns.Add(prop.Name, typeof(Int32));
                        break;
                    case "DateTime":
                        dataTable.Columns.Add(prop.Name, typeof(DateTime));
                        break;
                    default:
                        dataTable.Columns.Add(prop.Name);
                        break;
                }
                //Setting column names as Property names
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        /// <summary>
        /// Gets the contact by email identifier.
        /// </summary>
        /// <param name="ContactEmailIDs">The contact email i ds.</param>
        /// <returns></returns>
        public IEnumerable<int> GetContactByEmailID(IEnumerable<int> ContactEmailIDs)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT CE.ContactID FROM ContactEmails(NOLOCK) CE
                        INNER JOIN @tbl T ON T.ContactId = CE.ContactEmailID
                        WHERE CE.IsDeleted = 0";
            return db.Get<int>(sql, new { tbl = ContactEmailIDs.AsTableValuedParameter("dbo.Contact_List") });
            //return db.ContactEmails.Where(g => ContactEmailIDs.Contains(g.ContactEmailID) && g.IsDeleted == false).Select(x => x.ContactID);
        }

        /// <summary>
        /// Gets the contact by phone number identifier.
        /// </summary>
        /// <param name="ContactPhoneNumberIDs">The contact phone number i ds.</param>
        /// <returns></returns>
        public IEnumerable<int> GetContactByPhoneNumberID(IEnumerable<int> ContactPhoneNumberIDs)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT PE.ContactID FROM ContactPhoneNumbers(NOLOCK) PE
                        INNER JOIN @tbl T ON T.ContactId = PE.ContactPhoneNumberID
                        WHERE PE.IsDeleted = 0";
            return db.Get<int>(sql, new { tbl = ContactPhoneNumberIDs.AsTableValuedParameter("dbo.Contact_List") });
            //return db.ContactPhoneNumbers.Where(g => ContactPhoneNumberIDs.Contains(g.ContactPhoneNumberID) && g.IsDeleted == false).Select(x => x.ContactID);
        }

        /// <summary>
        /// Updates the lifecycle stage.
        /// </summary>
        /// <param name="ContactID">The contact identifier.</param>
        /// <param name="LifecycleStageID">The lifecycle stage identifier.</param>
        public void UpdateLifecycleStage(int ContactID, short LifecycleStageID)
        {
            var procedureName = "[dbo].[Update_Contact_LifeCycleStage]";
            var parms = new List<SqlParameter>
                {
                    new SqlParameter{ParameterName="@ContactID", Value = ContactID },
                    new SqlParameter{ParameterName="@LifeCycleStage", Value = LifecycleStageID }
                };
            CRMDb context = new CRMDb();
            context.ExecuteStoredProcedure(procedureName, parms);
        }

        /// <summary>
        /// Gets the company details.
        /// </summary>
        /// <param name="ContactID">The contact identifier.</param>
        /// <returns></returns>
        public IEnumerable<Company> GetCompanyDetails(IEnumerable<int> ContactID, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var companydetails = db.Contacts.Where(i => ContactID.Contains(i.ContactID) && i.AccountID == accountId).Select(x => new Company()
            {
                CompanyID = x.CompanyID,
                CompanyName = x.ContactType == ContactType.Company ? x.Company :
                                db.Contacts.Where(g => g.ContactID == x.CompanyID && g.AccountID == accountId).Select(y => y.Company).FirstOrDefault(),
                Id = x.ContactID
            });
            return companydetails;
        }

        /// <summary>
        /// Gets the contacts to synchronize.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="maxNumRecords">The maximum number records.</param>
        /// <param name="timeStamp">The time stamp.</param>
        /// <param name="firstSync">if set to <c>true</c> [first synchronize].</param>
        /// <param name="operationType">Type of the operation.</param>
        /// <returns></returns>
        public IEnumerable<Person> GetContactsToSync(int accountId, int userId, int? maxNumRecords, DateTime? timeStamp, bool firstSync, CRUDOperationType operationType)
        {
            Logger.Current.Verbose("ContactRepository/GetContactsToSync, parameters:  " + accountId + ", " + userId + ", " + maxNumRecords + ", " + timeStamp + ", " + firstSync);
            var db = ObjectContextFactory.Create();
            //IEnumerable<CRMOutlookSyncDb> contactsToSyncDb;
            if (firstSync)
            {
                var contactIds = db.Get<int>(@"select EntityID from CRMOutlookSync (nolock) os 
                                                inner join contacts (nolock) c on os.EntityID = c.contactid
                                                where entitytype = 3 and c.OwnerID = @userId"
                                            , new { userId = userId });

                var batchCount = 1000;
                var iteratorCount = Math.Ceiling((float)contactIds.Count() / (float)batchCount);
                if (operationType == CRUDOperationType.Create)
                {
                    Logger.Current.Informational("The user is owner for: " + contactIds.Count() + " contacts");
                    //Looping because the CONTAINS attribute do not support more characters.
                    for (int i = 0; i < iteratorCount; i++)
                    {
                        var sql = @"update CRMOutlookSync set SyncStatus = @notInSync  where (SyncStatus = 11 or syncstatus = 13) and entityid in @contactIds";
                        db.Execute(sql, new { notInSync = 11, contactIds = contactIds.Skip(batchCount * i).Take(batchCount) });
                    }
                }

                var firstTimeSyncContacts = db.Contacts
                     .Where(c => c.OwnerID == userId
                         && !contactIds.Contains(c.ContactID)
                         && c.IsDeleted == false
                         && c.ContactType == ContactType.Person
                         && c.AccountID == accountId)
                     .Select(c => c.ContactID).ToList();

                foreach (int id in firstTimeSyncContacts)
                {
                    CRMOutlookSyncDb contactOutlookSyncDb = new CRMOutlookSyncDb();
                    contactOutlookSyncDb.EntityID = id;
                    contactOutlookSyncDb.SyncStatus = (short)OutlookSyncStatus.NotInSync;
                    contactOutlookSyncDb.EntityType = (byte)AppModules.Contacts;
                    db.Entry<CRMOutlookSyncDb>(contactOutlookSyncDb).State = System.Data.Entity.EntityState.Added;
                }
                db.SaveChanges();
            }

            var contactIdsToSync = db.CRMOutlookSync.Where(c => c.SyncStatus == (short)OutlookSyncStatus.NotInSync && c.EntityType == (byte)AppModules.Contacts)
                .Select(c => c.EntityID);

            var accountSpecificContacts = db.Contacts
                 .Where(c => c.AccountID == accountId
                     && c.OwnerID == userId
                     && c.IsDeleted == false
                     && contactIdsToSync.Contains(c.ContactID))
                 .Take((int)maxNumRecords)
                 .Select(c => c.ContactID).ToList();

            Logger.Current.Informational("Account Specific Contacts Count : " + accountSpecificContacts.Count() + ", AccountID : " + accountId);

            var contactsSP = this.FindAll(accountSpecificContacts, true);
            if (contactsSP.IsAny())
                return contactsSP.Where(w => w.OwnerId == userId && w.AccountID == accountId).Take((int)maxNumRecords).Cast<Person>().ToList();
            else
                return new List<Person>();
            //foreach (ContactsDb contact in contacts1ToSyncDb)
            //{
            //    contact.ContactEmails = contact.ContactEmails.Where(i => i.IsDeleted == false).ToList();
            //    contact.ContactPhones = contact.ContactPhones.Where(p => p.IsDeleted == false).ToList();
            //}
            //var contactsToSync = Mapper.Map<IEnumerable<ContactsDb>, IEnumerable<Person>>(contacts1ToSyncDb);

        }

        /// <summary>
        /// Gets the entity outlook synchronize map.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="maxNumRecords">The maximum number records.</param>
        /// <param name="timeStamp">The time stamp.</param>
        /// <param name="entityIds">The entity ids.</param>
        /// <returns></returns>
        public IEnumerable<CRMOutlookSync> GetEntityOutlookSyncMap(int accountId, int userId, int? maxNumRecords, DateTime? timeStamp, IEnumerable<int> entityIds)
        {
            IEnumerable<CRMOutlookSyncDb> contactsToSyncDb;
            var db = ObjectContextFactory.Create();
            contactsToSyncDb = db.CRMOutlookSync.Where(c => entityIds.Contains(c.EntityID));
            contactsToSyncDb.ForEach(c =>
            {
                c.SyncStatus = (short)OutlookSyncStatus.Syncing;
                c.LastSyncedBy = userId;
                db.Entry<CRMOutlookSyncDb>(c).State = System.Data.Entity.EntityState.Modified;
            });
            db.SaveChanges();
            var contactsToSync = Mapper.Map<IEnumerable<CRMOutlookSyncDb>, IEnumerable<CRMOutlookSync>>(contactsToSyncDb);

            return contactsToSync;
        }

        /// <summary>
        /// Updates the synced entities.
        /// </summary>
        /// <param name="outlookKeys">The outlook keys.</param>
        public void UpdateSyncedEntities(Dictionary<int, string> outlookKeys)
        {
            var db = ObjectContextFactory.Create();
            var entitiesToUpdate = db.CRMOutlookSync.Where(c => outlookKeys.Keys.Contains(c.EntityID));

            entitiesToUpdate.ForEach(c =>
            {
                c.OutlookKey = outlookKeys.Single(o => o.Key == c.EntityID).Value;
                c.LastSyncDate = DateTime.Now.ToUniversalTime();
                c.SyncStatus = (short)OutlookSyncStatus.InSync;
            });
            db.SaveChanges();
        }

        /// <summary>
        /// Gets the deleted contacts to synchronize.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="maxNumRecords">The maximum number records.</param>
        /// <param name="timeStamp">The time stamp.</param>
        /// <returns></returns>
        public IEnumerable<int> GetDeletedContactsToSync(int accountId, int userId, int? maxNumRecords, DateTime? timeStamp)
        {
            var db = ObjectContextFactory.Create();

            var userContacts = db.CRMOutlookSync
                .Join(db.Contacts, crm => crm.EntityID, c => c.ContactID, (crm, c) => new { crm, c })
                .Where(x => x.crm.EntityType == (byte)AppModules.Contacts
                    && x.crm.SyncStatus != (short)OutlookSyncStatus.Deleted
                    && x.c.OwnerID == userId
                    && x.c.ContactType == ContactType.Person
                    && x.c.AccountID == accountId);

            var contactIdsToSync = userContacts.Select(ut => ut.c.ContactID).ToList().Distinct();

            //var deletedContacts = db.Contacts
            //    .Where(c => c.IsDeleted == true
            //        && c.OwnerID == userId
            //        && c.AccountID == accountId
            //        && c.ContactType == ContactType.Person
            //        && contactIdsToSync.Contains(c.ContactID))
            //    .Select(c => c.ContactID)
            //    .Take((int)maxNumRecords)
            //    .ToList();
            var sql = @"SELECT TOP (@maxNumRecords) ContactID FROM Contacts (NOLOCK)
                        WHERE IsDeleted = 1 AND OwnerID = @UserId AND AccountID = @AccountId
                        AND ContactType = 1 AND ContactID IN (SELECT ContactID FROM @tbl)";
            if (!maxNumRecords.HasValue)
                maxNumRecords = 25;
            var newDb = ObjectContextFactory.Create();
            var deletedContacts = newDb.Get<int>(sql, new { maxNumRecords = maxNumRecords, UserId = userId, AccountId = accountId, tbl = contactIdsToSync.AsTableValuedParameter("dbo.Contact_List") });

            db.CRMOutlookSync.Where(c => deletedContacts.Contains(c.EntityID)).ForEach(c =>
            {
                c.SyncStatus = (short)OutlookSyncStatus.Deleted;
            });
            db.SaveChanges();
            return deletedContacts;
        }

        /// <summary>
        /// Gets the email information.
        /// </summary>
        /// <param name="Emails">The emails.</param>
        /// <param name="AccountID">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<OutlookEmailInformation> GetEmailInformation(IEnumerable<string> Emails, int AccountID)
        {
            var db = ObjectContextFactory.Create();
            //IEnumerable<OutlookEmailInformation> OutlookEmails = db.ContactEmails.Include(x => x.Contact)
            //                                                       .Where(i => i.AccountID == AccountID && Emails.Contains(i.Email) && i.Contact.IsDeleted == false && i.IsDeleted == false)
            //                                                        .Select(x => new OutlookEmailInformation()
            //                                                        {
            //                                                            ContactEmailID = x.ContactEmailID,
            //                                                            ContactID = x.ContactID,
            //                                                            Email = x.Email
            //                                                        });

            var sql = @" SELECT ContactEmailID, CE.ContactID, Email FROM ContactEmails (NOLOCK) CE
                         JOIN Contacts (NOLOCK) C ON C.ContactID = CE.ContactID
                         WHERE CE.IsDeleted = 0 AND C.IsDeleted = 0 AND CE.Email IN @Emails AND CE.AccountID = @AccountId";
            return db.Get<OutlookEmailInformation>(sql, new { AccountId = AccountID, Emails = Emails });
        }

        /// <summary>
        /// Inserts the outlook email audit information.
        /// </summary>
        /// <param name="Emails">The emails.</param>
        /// <param name="AccountID">The account identifier.</param>
        /// <param name="UserID">The user identifier.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="sentUTCDate">The sent UTC date.</param>
        public void InsertOutlookEmailAuditInformation(IEnumerable<OutlookEmailInformation> Emails, int AccountID, int UserID, Guid guid, DateTime sentUTCDate)
        {
            using (var db = ObjectContextFactory.Create())
            {
                foreach (OutlookEmailInformation email in Emails)
                {
                    ContactEmailAuditDb contactemailaudit = new ContactEmailAuditDb()
                    {
                        ContactEmailID = email.ContactEmailID,
                        RequestGuid = guid,
                        SentBy = UserID,
                        SentOn = sentUTCDate,
                        Status = (byte)CommunicationStatus.Success
                    };
                    db.Entry(contactemailaudit).State = System.Data.Entity.EntityState.Added;
                    db.ContactEmailAudit.Add(contactemailaudit);

                    ContactsDb contact = db.Contacts.Where(i => i.ContactID == email.ContactID && i.AccountID == AccountID).FirstOrDefault();
                    contact.LastContactedThrough = (byte)AppModules.SendMail;
                    contact.LastContacted = sentUTCDate;
                    db.Entry(contact).State = System.Data.Entity.EntityState.Modified;
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Finds the contacts by primary emails.
        /// </summary>
        /// <param name="emails">The emails.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<int> FindContactsByPrimaryEmails(IEnumerable<string> emails, int accountId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                IEnumerable<int> contactIds = db.ContactEmails.Join(db.Contacts,
                        ce => ce.ContactID,
                        c => c.ContactID,
                        (ce, c) => new { ContactsDb = c, ContactEmailsDb = ce })
                            .Where(c => emails.Contains(c.ContactEmailsDb.Email) && c.ContactEmailsDb.IsPrimary && c.ContactEmailsDb.AccountID == accountId
                                && c.ContactEmailsDb.IsDeleted == false && c.ContactsDb.IsDeleted == false)
                            .Select(c => c.ContactEmailsDb.ContactID).ToList();
                return contactIds;
            }
        }

        /// <summary>
        /// Find contacts by primary or secondary email
        /// </summary>
        /// <param name="email"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public IEnumerable<int> FindContactsByEmail(string email, int accountId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"select ce.ContactID from contactemails ce
                            inner join contacts c on ce.ContactID = c.ContactID
                            where email = @email and ce.accountid = @accountId and c.IsDeleted  = 0";
                var contactIds = db.Get<int>(sql, new { email = email, accountId = accountId }).ToList();
                return contactIds;
            }
        }

        /// <summary>
        /// Finds the timelines total records2.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="OpportunityID">The opportunity identifier.</param>
        /// <param name="module">The module.</param>
        /// <param name="period">The period.</param>
        /// <param name="PageName">Name of the page.</param>
        /// <param name="Activities">The activities.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="timeLineGroup">The time line group.</param>
        /// <param name="AccountID">The account identifier.</param>
        /// <returns></returns>
        public int FindTimelinesTotalRecords2(int? contactId, int? OpportunityID, string module, string period, string PageName, string[] Activities, DateTime fromDate, DateTime toDate, out IEnumerable<TimeLineGroup> timeLineGroup, int AccountID)
        {
            var db = ObjectContextFactory.Create();
            var prevdate = DateTime.Now.ToUniversalTime();
            if (period != "")
            {
                if (period == "1")
                {
                    prevdate = DateTime.MinValue;
                }
                else if (period == "2")
                {
                    prevdate = prevdate.AddDays(-6);
                }
                else if (period == "3")
                {
                    prevdate = prevdate.AddDays(-29);
                }
                else if (period == "4")
                {
                    prevdate = prevdate.AddDays(-59);
                }
                else if (period == "5")
                {
                    prevdate = prevdate.AddDays(-89);
                }
                else if (period == "6")
                {
                    fromDate = fromDate.AddDays(-1);
                    toDate = toDate.AddDays(1);
                }
            }

            var contactpredicate = PredicateBuilder.True<ContactTimeLineDb>();
            Expression<Func<ContactTimeLineDb, bool>> contactspredicate = PredicateBuilder.True<ContactTimeLineDb>();
            Expression<Func<OpportunitiesTimeLineDb, bool>> opportunitiespredicate = PredicateBuilder.True<OpportunitiesTimeLineDb>();
            if (Activities == null)
                Activities = new string[0];

            if (PageName == "contacts")
            {
                contactpredicate = contactpredicate.And(x => x.ContactID == contactId).And(x => Activities.Contains(x.Module))
                                                   .And(x => x.AuditDate != null);
                contactspredicate = contactspredicate.And(x => x.ContactID == contactId).And(x => Activities.Contains(x.Module))
                                                     .And(x => x.AuditDate != null);
                if (period != "")
                {
                    if (period == "6")
                    {
                        contactpredicate = contactpredicate.And(x => x.AuditDate > fromDate && x.AuditDate < toDate);
                        contactspredicate = contactspredicate.And(x => x.AuditDate > fromDate && x.AuditDate < toDate);
                    }
                    else
                    {
                        contactpredicate = contactpredicate.And(x => x.AuditDate > prevdate);
                        contactspredicate = contactspredicate.And(x => x.AuditDate > prevdate);
                    }
                }


                var procedureName = "[dbo].[GET_Contact_TimelineData]";

                var parms = new List<SqlParameter>
                    {
                         new SqlParameter{ParameterName="@AccountID", Value=AccountID },
                        new SqlParameter{ParameterName="@ContactID", Value=contactId }
                    };

                CRMDb context = new CRMDb();
                var timeLinecontent = context.ExecuteStoredProcedure<ContactTimeLineDb>(procedureName, parms).ToList();

                if (period != "")
                {
                    if (period == "6")
                    {
                        timeLinecontent = timeLinecontent.Where(x => x.AuditDate > fromDate && x.AuditDate < toDate).ToList();
                    }
                    else if (period == "1")
                    {
                        //will take care
                    }
                    else
                    {
                        timeLinecontent = timeLinecontent.Where(x => x.AuditDate > prevdate).ToList();
                    }
                }



                timeLineGroup = timeLinecontent.Where(i => i.ContactID == contactId && Activities.Contains(i.Module) && i.AuditDate != null).GroupBy(x => x.AuditDate.Value.Year)
                                                    .Select(g => new TimeLineGroup()
                                                    {
                                                        Year = g.Key,
                                                        YearCount = g.Count(),
                                                        Months = g.GroupBy(mnth => mnth.AuditDate.Value.Month)
                                                        .Select(o => new TimeLineMonthGroup()
                                                        {
                                                            Month = o.Key,
                                                            MonthCount = o.Count()
                                                        })
                                                    }).ToList();

                return timeLinecontent.Where(i => i.ContactID == contactId && Activities.Contains(i.Module) && i.AuditDate != null).Count();
            }
            else
            {
                opportunitiespredicate = opportunitiespredicate.And(x => x.OpportunityID == OpportunityID).And(x => Activities.Contains(x.Module));
                if (period != "")
                {
                    if (period == "6")
                    {
                        opportunitiespredicate = opportunitiespredicate.And(x => x.AuditDate > fromDate && x.AuditDate < toDate);
                    }
                    else
                    {
                        opportunitiespredicate = opportunitiespredicate.And(x => x.AuditDate > prevdate);
                    }
                }

                timeLineGroup = db.GetOpportunityTimeLines.AsExpandable().Where(opportunitiespredicate).GroupBy(x => x.AuditDate.Year)
                                                    .Select(g => new TimeLineGroup()
                                                    {
                                                        Year = g.Key,
                                                        YearCount = g.Count(),
                                                        Months = g.GroupBy(mnth => mnth.AuditDate.Month)
                                                        .Select(o => new TimeLineMonthGroup()
                                                        {
                                                            Month = o.Key,
                                                            MonthCount = o.Count()
                                                        })
                                                    }).ToList();

                return db.GetOpportunityTimeLines.AsExpandable().Where(opportunitiespredicate).Count();
            }
        }

        /// <summary>
        /// Gets the tax rate based on zip code.
        /// </summary>
        /// <param name="ZipCode">The zip code.</param>
        /// <returns></returns>
        public TaxRate GetTaxRateBasedOnZipCode(string ZipCode)
        {
            using (var db = ObjectContextFactory.Create())
            {
                TaxRateDb taxRateDb = db.TaxRates.Where(i => i.ZIPCode == (ZipCode == null ? string.Empty : ZipCode.Replace(" ", string.Empty))).FirstOrDefault();

                return (taxRateDb == null) ? null : Mapper.Map<TaxRateDb, TaxRate>(taxRateDb);
            }
        }

        /// <summary>
        /// Gets the email identifier.
        /// </summary>
        /// <param name="emailID">The email identifier.</param>
        /// <param name="contactID">The contact identifier.</param>
        /// <returns></returns>
        public int GetEmailID(string emailID, int contactID)
        {
            var db = ObjectContextFactory.Create();
            int ContactEmail = db.ContactEmails.Where(e => e.Email == emailID && e.ContactID == contactID).Select(e => e.ContactEmailID).FirstOrDefault();
            return ContactEmail;
        }

        /// <summary>
        /// Gets the web visit details by visit reference.
        /// </summary>
        /// <param name="visitReference">The visit reference.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public IEnumerable<WebVisit> GetWebVisitDetailsByVisitReference(string visitReference, int userId)
        {
            Logger.Current.Verbose("In GetWebVisitDetailsByVisitReference/ VisitReference: " + visitReference);
            var db = ObjectContextFactory.Create();
            var webVisitDetails = db.WebVisits
                .Where(w => w.VisitReference == visitReference)
                .ToList();

            if (webVisitDetails.IsAny())
            {
                var webVisitID = webVisitDetails.Where(w => w.IsVisit == true).FirstOrDefault();
                if (webVisitID != null)
                {
                    var notification = db.Notifications.Where(c => c.EntityID == webVisitID.ContactWebVisitID && c.UserID == userId).FirstOrDefault();
                    if (notification != null)
                    {
                        Logger.Current.Informational("Updating the notification as read. Notification Id: " + notification.NotificationID);
                        notification.Status = NotificationStatus.Viewed;
                        db.Entry<NotificationDb>(notification).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        Logger.Current.Informational("Updated the notification as read. Notification Id: " + notification.NotificationID);
                    }
                }
            }
            return Mapper.Map<IEnumerable<WebVisitsDb>, IEnumerable<WebVisit>>(webVisitDetails);
        }

        /// <summary>
        /// Gets the contact web visits summary.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="totalHits">The total hits.</param>
        /// <returns></returns>
        public IEnumerable<ContactWebVisitSummary> GetContactWebVisitsSummary(int contactId, short pageNumber, short pageSize, out int totalHits)
        {
            var db = ObjectContextFactory.Create();
            var offset = pageSize * (pageNumber - 1);
            //            var sql = @"select cwv.VisitReference, cwv.visitedon,cwv.contactwebvisitid,cwv.duration,v.Page1,v.Page2,v.Page3,cwv.ispname, cwv.city from contactwebvisits cwv
            //                        left join GET_VisiStat_Top3_Pages v on cwv.visitreference= v.visitreference
            //                        left join contacts c  on cwv.contactid=c.contactid 
            //                        where cwv.contactid = @contactId and cwv.isvisit = 1  order by cwv.ContactWebVisitID
            //                        OFFSET @offset ROWS 
            //                        FETCH NEXT @pageSize ROWS ONLY";

            var sql1 = @"select * from (select VisitReference, max(visitedon) VisitedOn, sum(duration) Duration, count(visitreference) PageViews
                            ,ISNULL(Max(city +', ' + Region),'') Location, Max(IspName) Source
                         from contactwebvisits (nolock) cw 
                         where  VisitReference is not null and contactid = @contactId
                         group by visitreference) tmp
                         left join GET_VisiStat_Top3_Pages vs on tmp.VisitReference = vs.VisitReference 
                         order by visitedon desc OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            var sqlTotalHits = @"select count(*) from (select VisitReference, max(visitedon) VisitedOn, sum(duration) Duration, count(visitreference) PageViews,ISNULL(Max(city +', ' + Region),'') Location
                         from contactwebvisits (nolock) cw 
                         where  VisitReference is not null and contactid = @contactId
                         group by visitreference
                         )tmp";
            var visits = db.Get<ContactWebVisitSummary>(sql1, new { contactId = contactId, pageNumber = pageNumber, pageSize = pageSize, offset = offset }).ToList();
            totalHits = db.Get<int>(sqlTotalHits, new { contactId = contactId }).Sum();
            return visits;
        }

        /// <summary>
        /// Gets the web visit by visit identifier.
        /// </summary>
        /// <param name="visitID">The visit identifier.</param>
        /// <returns></returns>
        public IEnumerable<WebVisit> GetWebVisitByVisitID(int visitID)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"select * from ContactWebVisits where VisitReference = (
                            select cwv.VisitReference from contactwebvisits cwv where cwv.ContactWebVisitID = @visitID)";
                var visits = db.Get<WebVisit>(sql, new { visitID = visitID });
                return visits;
            }
        }

        /// <summary>
        /// Gets the contact reference identifier.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public Guid? GetContactReferenceId(int contactId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                Logger.Current.Informational("Request received to get contact reference id for contact: " + contactId);
                var contact = db.Contacts.Where(c => c.ContactID == contactId).FirstOrDefault();

                Guid? referenceId = contact.ReferenceId;
                if (referenceId == null || (referenceId.HasValue && referenceId.Value.ToString() == "00000000-0000-0000-0000-000000000000"))
                {
                    Logger.Current.Informational("Modifying referenceId from all zeros " + contactId);
                    db.Entry<ContactsDb>(contact).State = System.Data.Entity.EntityState.Modified;
                    contact.ReferenceId = Guid.NewGuid();
                    referenceId = contact.ReferenceId;
                }
                var ipAddressMap = db.ContactIPAddresses.Where(c => c.ContactID == contactId && c.UniqueTrackingID == referenceId.Value.ToString()).FirstOrDefault();

                if (ipAddressMap == null)
                {
                    Logger.Current.Informational("Adding a record in contactIpAddresses table. ContactId " + contactId + ". ReferenceId: " + referenceId.Value.ToString());
                    ContactIPAddressesDb ipAddressDb = new ContactIPAddressesDb()
                    {
                        ContactID = contactId,
                        IdentifiedOn = DateTime.Now.ToUniversalTime(),
                        IPAddress = "",
                        UniqueTrackingID = referenceId.Value.ToString()
                    };
                    db.Entry<ContactIPAddressesDb>(ipAddressDb).State = System.Data.Entity.EntityState.Added;
                }
                db.SaveChanges();
                Logger.Current.Informational("Returning contact referenceid : " + referenceId.Value.ToString());
                return referenceId;
            }
        }

        /// <summary>
        /// Finds the last name of the contacts of user by first and.
        /// </summary>
        /// <param name="contactNames">The contact names.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<int> FindContactsOfUserByFirstAndLastName(IEnumerable<string> contactNames, int userId, int accountId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                List<int> contactIds = new List<int>();
                foreach (var contactName in contactNames)
                {
                    var sql = @"select contactid from GET_ContactBasicInfo where Name = @name and ownerid = @userId and accountid = @accountId";
                    var sql1 = @"select contactid from GET_ContactBasicInfo where NameWithoutTitle = @name and ownerid = @userId and accountid = @accountId";

                    var contactId = db.Get<int?>(sql, new { name = contactName, userId = userId, accountId = accountId }).FirstOrDefault();
                    if (contactId != null)
                        contactIds.Add((int)contactId);
                    else
                    {
                        var contactID = db.Get<int?>(sql1, new { name = contactName, userId = userId, accountId = accountId }).FirstOrDefault();
                        if (contactID != null)
                            contactIds.Add((int)contactID);
                    }
                }
                return contactIds;
            }
        }

        /// <summary>
        /// Inserts the bulk operation.
        /// </summary>
        /// <param name="bulkOperations">The bulk operations.</param>
        /// <param name="contactIds">The contact ids.</param>
        public void InsertBulkOperation(BulkOperations bulkOperations, int[] contactIds)
        {
            BulkOperationsDb bulkOperationsdb = Mapper.Map<BulkOperations, BulkOperationsDb>(bulkOperations);
            bulkOperationsdb.Status = (byte)BulkOperationStatus.Created;
            using (var db = ObjectContextFactory.Create())
            {
                db.BulkOperations.Add(bulkOperationsdb);
                db.SaveChanges();

                IList<BulkContactData> contactsData = new List<BulkContactData>();

                if (contactIds != null)
                {

                    foreach (int id in contactIds)
                    {
                        BulkContactData _cd = new BulkContactData();
                        _cd.BulkOperationID = bulkOperationsdb.BulkOperationID;
                        _cd.ContactID = id;
                        contactsData.Add(_cd);
                    }
                    db.BulkContactData.AddRange(contactsData);
                    Logger.Current.Informational("Completed inserting bulk contacts " + bulkOperationsdb.BulkOperationID);
                }
                db.SaveChanges();

            }
        }

        /// <summary>
        /// Updates the export bulk operation.
        /// </summary>
        /// <param name="operationId">The operation identifier.</param>
        /// <param name="fileKey">The file key.</param>
        /// <param name="fileName">Name of the file.</param>
        public void UpdateExportBulkOperation(int operationId, string fileKey, string fileName)
        {
            var db = ObjectContextFactory.Create();
            var data = db.BulkOperations.Where(p => p.BulkOperationID == operationId).FirstOrDefault();
            if (data != null)
            {
                data.ExportFileKey = fileKey;
                data.ExportFileName = fileName;
            }
            db.SaveChanges();
        }

        /// <summary>
        /// Fetch company name by company ID
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public string GetCompanyNameById(int companyId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"select isnull(Company,'') from contacts (nolock) where contactid = @id";
                var companyName = db.Get<string>(sql, new { id = companyId }).FirstOrDefault();
                return companyName ?? "";
            }
        }

        /// <summary>
        /// Fetch PrimaryEmail by Contact ID
        /// </summary>
        /// <param name="contactId"></param>
        /// <returns></returns>
        public IEnumerable<Person> GetEmailById(IEnumerable<int> contactIds)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT C.FirstName,C.LastName,CE.Email AS PrimaryEmail,C.ContactID  FROM Contacts (NOLOCK) C
                        INNER JOIN ContactEmails (NOLOCK) CE ON CE.ContactID = C.ContactID
                        WHERE C.ContactID in (SELECT DataValue FROM dbo.Split(@contactids, ',')) AND CE.IsPrimary=1";
            IEnumerable<ContactsDb> contacts = db.Get<ContactsDb>(sql, new { contactids = string.Join(",", contactIds.ToArray()) }).ToList();
            IEnumerable<Person> persons = Mapper.Map<IEnumerable<ContactsDb>, IEnumerable<Person>>(contacts);
            return persons;
        }

        /// <summary>
        /// Fetch AccountID by Contact ID
        /// </summary>
        /// <param name="contactId"></param>
        /// <returns></returns>
        public int GetAccountIdById(int contactId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"select isnull(AccountID,'') from contacts (nolock) where contactid = @id";
                int accountId = db.Get<int>(sql, new { id = contactId }).FirstOrDefault();
                return accountId;
            }
        }

        /// <summary>
        /// Updates the contact last touched through.
        /// </summary>
        /// <param name="contactIds">The contact ids.</param>
        public void UpdateContactLastTouchedThrough(IEnumerable<int> contactIds, int accountId)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<ContactsDb> contacts = db.Contacts.Where(c => contactIds.Contains(c.ContactID) && c.AccountID == accountId).ToList();
            foreach (var contact in contacts)
            {
                contact.LastContactedThrough = (byte)LastTouchedValues.SendMail;
                db.Entry(contact).State = System.Data.Entity.EntityState.Modified;
            }
            db.SaveChanges();
        }

        /// <summary>
        /// Gets the contact summary.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public string GetContactSummary(int contactId)
        {
            Logger.Current.Verbose("ContactRepository/GetContactSummary ContactId: " + contactId);
            using (var db = ObjectContextFactory.Create())
            {
                var query = @"SELECT ContactSummary FROM ContactSummary (NOLOCK) WHERE contactid = @contactId ORDER BY LastNoteDate DESC";
                var contactSummary = db.Get<string>(query, new { contactId = contactId }).FirstOrDefault() ?? "";
                return contactSummary;
            }
        }

        /// <summary>
        /// Gets the contact note summary.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public ContactNoteSummary GetContactNoteSummary(int contactId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sqlQuery = @"select NoteDetailSummary, LastNoteDate from [ContactSummary] where contactId = @contactId";
                var contactSummary = db.Get<ContactNoteSummary>(sqlQuery, new { contactId = contactId }).FirstOrDefault();
                return contactSummary;
            }
        }

        /// <summary>
        /// Gets all contact owners.
        /// </summary>
        /// <param name="contactIds">The contact ids.</param>
        /// <param name="userIds">The user ids.</param>
        /// <param name="ownerId">The owner identifier.</param>
        /// <returns></returns>
        public IEnumerable<ContactOwner> GetAllContactOwners(IEnumerable<int> contactIds, IEnumerable<int> userIds, int ownerId)
        {
            var procedureName = "[dbo].[Get_Contact_Owner]";
            var parms = new List<SqlParameter>
                {
                    new SqlParameter{ParameterName ="@ContactIds", Value= string.Join(",",contactIds)},
                    new SqlParameter{ParameterName ="@UserIds", Value= string.Join(",",userIds) },
                    new SqlParameter{ParameterName ="@OwnerID", Value= ownerId}

                };
            var db = ObjectContextFactory.Create();
            IEnumerable<ContactOwner> data = db.ExecuteStoredProcedure<ContactOwner>(procedureName, parms);
            return data;
        }

        public IEnumerable<Contact> CheckDuplicate(string firstName, string lastName, string email, string company, int contactID, int accountID, byte contactType)
        {
            var procName = "[dbo].[Contact_Duplicate_Check]";
            var parms = new List<SqlParameter>
                {
                    new SqlParameter{ParameterName="@firstName", Value = firstName ?? "" },
                    new SqlParameter{ParameterName="@lastName", Value = lastName ?? "" },
                    new SqlParameter{ParameterName="@email", Value = email ?? "" },
                    new SqlParameter{ParameterName="@company", Value = company ?? "" },
                    new SqlParameter{ParameterName="@contactID", Value = contactID },
                    new SqlParameter{ParameterName="@accountID", Value = accountID },
                    new SqlParameter{ParameterName="@contactType", Value = contactType },
                };

            var db = ObjectContextFactory.Create();

            var objectContext = (db as IObjectContextAdapter).ObjectContext;
            objectContext.CommandTimeout = 400;

            List<ContactsDb> contactsDB = new List<ContactsDb>();
            var duplicateContactID = db.ExecuteStoredProcedure<int?>(procName, parms).FirstOrDefault();
            if (duplicateContactID != null && duplicateContactID != 0)
                return this.FindAll(new List<int>() { (int)duplicateContactID });
            else
                return null;
        }

        public bool IsNewContact(int contactId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                Logger.Current.Verbose("Checkig contact audit count for contact: " + contactId);
                var sql = @"select 1 from Contacts_Audit (nolock) where contactid = @contactId";
                int auditCount = db.Get<int>(sql, new { contactId = contactId }).Sum();
                Logger.Current.Informational(auditCount + " audits found for contact: " + contactId);
                return auditCount <= 1 ? true : false;
            }
        }

        public ContactTableType InsertAndUpdateContact(ContactTableType contact)
        {
            if (contact.ContactID != 0 && contact.ContactType == (byte)ContactType.Company)
            {
                contact.CompanyID = contact.ContactID;
                contact.ContactID = 0;
            }
                
            if (contact.Addresses.IsAny())
            {
                contact.Addresses.Each(a =>
                {
                    if (a.State != null)
                        a.StateID = a.State.Code == "" ? null : a.State.Code;
                    if (a.Country != null)
                        a.CountryID = a.Country.Code == "" ? null : a.Country.Code;
                });
            }
            else
                contact.Addresses = new List<Address>() { };

            if (contact.Emails.IsAny())
                contact.Emails.ForEach(a => a.EmailSatus = (byte)a.EmailStatusValue);
            else
                contact.Emails = new List<Email>() { };

            if (!contact.Phones.IsAny())
                contact.Phones = new List<Phone>() { };

            if (contact.Communities.IsAny())
                contact.Communities.ForEach(a => a.CommunityID = a.Id);
            else
                contact.Communities = new List<DropdownValue>() { };

            if (contact.LeadSources.IsAny())
            {
                contact.LeadSources.ForEach(a => a.LeadSouceID = a.Id);
                contact.LeadSources.ForEach(a => a.LastUpdatedDate = DateTime.Now.ToUniversalTime());
            }
            else
                contact.LeadSources = new List<DropdownValue>() { };
            

            IEnumerable<Image> images = new List<Image>();
            IEnumerable<ContactTableType> contacts = new List<ContactTableType>()
            {
                contact
            };

            if (contact.ContactImage != null)
                images = new List<Image>() { contact.ContactImage };

            CummunicationTableType communication = new CummunicationTableType();

            contact.SocialMediaUrls.ForEach(s =>
            {
                if (s.MediaType == "Facebook")
                    communication.FacebookUrl = s.URL;
                if (s.MediaType == "Twitter")
                    communication.TwitterUrl = s.URL;
                if (s.MediaType == "Google+")
                    communication.GooglePlusUrl = s.URL;
                if (s.MediaType == "LinkedIn")
                    communication.LinkedInUrl = s.URL;
                if (s.MediaType == "Blog")
                    communication.BlogUrl = s.URL;
                if (s.MediaType == "Website")
                    communication.WebSiteUrl = s.URL;
            });

            var communications = new List<CummunicationTableType>()
            {
                communication
            };


            using (var db = ObjectContextFactory.Create())
            {
                db.QueryStoredProc("dbo.SaveContact", (reader) =>
                {
                    contact.ContactID = reader.Read<int>().FirstOrDefault();
                    contact.CompanyID = reader.Read<int>().FirstOrDefault();

                }, new
                {
                    contact = contacts.AsTableValuedParameter("dbo.ContactType", new string[] { "ContactID", "FirstName", "LastName", "Company", "CommunicationID", "Title", "ContactImageUrl", "AccountID", "LeadSource", "HomePhone", "WorkPhone", "MobilePhone", "PrimaryEmail", "ContactType", "SSN", "LifecycleStage", "DoNotEmail", "LastContacted", "IsDeleted", "ProfileImageKey", "ImageID", "ReferenceID", "LastUpdatedBy", "LastUpdatedOn", "OwnerID", "PartnerType", "ContactSource", "SourceType", "CompanyID", "LastContactedThrough", "FirstContactSource", "FirstSourceType", "LeadScore", "IncludeInReports" }),
                    addresses = contact.Addresses.AsTableValuedParameter("dbo.AddresseType", new string[] { "AddressID", "AddressTypeID", "AddressLine1", "AddressLine2", "City", "StateID", "CountryID", "ZipCode", "IsDefault" }),
                    communication = communications.AsTableValuedParameter("dbo.CommunicationType", new string[] { "CommunicationID", "SecondaryEmails", "FacebookUrl", "TwitterUrl", "GooglePlusUrl", "LinkedInUrl", "BlogUrl", "WebSiteUrl", "FacebookAccessToken", "TwitterOAuthToken", "TwitterOAuthTokenSecret" }),
                    image = images.AsTableValuedParameter("dbo.ImageType", new string[] { "ImageID", "FriendlyName", "StorageName", "OriginalName", "CreatedBy", "CreatedDate", "CategoryId", "AccountID" }),
                    contactCustomFieldMaps = contact.CustomFields.AsTableValuedParameter("dbo.ContactCustomFieldMapType", new string[] { "ContactCustomFieldMapId", "ContactId", "CustomFieldId", "Value" }),
                    contactPhoneNumbers = contact.Phones.AsTableValuedParameter("dbo.ContactPhoneNumberType", new string[] { "ContactPhoneNumberID", "ContactID", "Number", "PhoneType", "IsPrimary", "AccountID", "IsDeleted", "CountryCode", "Extension" }),
                    contactEmails = contact.Emails.AsTableValuedParameter("dbo.ContactEmailType", new string[] { "ContactEmailID", "ContactID", "EmailId", "EmailSatus", "IsPrimary", "AccountID", "SnoozeUntil", "IsDeleted" }),
                    contactLeadSourceMap = contact.LeadSources.AsTableValuedParameter("dbo.ContactLeadSourceMapType", new string[] { "ContactLeadSourceMapID", "ContactId", "LeadSouceID", "IsPrimary", "LastUpdatedDate" }),
                    contactCommunityMap = contact.Communities.AsTableValuedParameter("dbo.ContactCommunityMapType", new string[] { "ContactCommunityMapID", "ContactId", "CommunityID", "CreatedOn", "CreatedBy", "LastModifiedOn", "LastModifiedBy", "IsDeleted" }),
                });
                
                return contact;

            }

        }

        public int UpdateContactCustomField(int contactId, int fieldId, string newValue)
        {
            Logger.Current.Verbose("In ContactRepository/UpdateContactCustomField");
            using (var db = ObjectContextFactory.Create())
            {
                var sqlParams = new List<SqlParameter>();
                sqlParams.Add(new SqlParameter() { ParameterName = "@ContactId", Value = contactId, SqlDbType = SqlDbType.Int });
                sqlParams.Add(new SqlParameter() { ParameterName = "@FieldId", Value = fieldId, SqlDbType = SqlDbType.Int });
                sqlParams.Add(new SqlParameter() { ParameterName = "@Value", Value = ToDBNull(newValue) });
                var result = db.ExecuteStoredProcedure<int>("[dbo].[UpdateContactCustomField]", sqlParams).FirstOrDefault();
                ExecuteStoredProc(contactId, 2);
                return result;
            }
        }

        public static object ToDBNull(object value)
        {
            if (null != value)
                return value;
            return DBNull.Value;
        }

        public int InsertAPILeadSubmissionData(APILeadSubmission apileadsubmission)
        {
            int apiLeadSubmissionID = 0;
            var db = ObjectContextFactory.Create();
            //pass SELECT SCOPE_IDENTITY() to insert query by kiran on 30/05/2018 NEXG-3014
            var sql = @"INSERT INTO APILeadSubmissions(ContactID,AccountID,OwnerID,SubmittedData,SubmittedOn,IsProcessed,Remarks,FormID,IPAddress,FieldUpdatedOn,FieldUpdatedBy)
                        VALUES(@contactId,@accountId,@ownerId,@submittedData,@submittedOn,@isProcessed,@remarks, @FormId, @IPAddress,@FieldUpdatedOn,@FieldUpdatedBy) SELECT SCOPE_IDENTITY();";
            
            /* Commented by kiran on 30/05/2018 NEXG-3014
            //db.Execute(sql, new
            //{
            //    contactId = apileadsubmission.ContactID,
            //    accountId = apileadsubmission.AccountID,
            //    ownerId = apileadsubmission.OwnerID,
            //    submittedData = apileadsubmission.SubmittedData,
            //    submittedOn = apileadsubmission.SubmittedOn,
            //    isProcessed = apileadsubmission.IsProcessed,
            //    remarks = apileadsubmission.Remarks,
            //    FormId = apileadsubmission.FormID,
            //    IPAddress = apileadsubmission.IPAddress,
            //    FieldUpdatedOn = DateTime.Now.ToUniversalTime(),
            //    FieldUpdatedBy = apileadsubmission.OwnerID
            //});
            */
            //added by kiran on 30/05/2018 NEXG-3014
            apiLeadSubmissionID = db.ExecuteWithOutPut(sql, new
            {
                contactId = apileadsubmission.ContactID,
                accountId = apileadsubmission.AccountID,
                ownerId = apileadsubmission.OwnerID,
                submittedData = apileadsubmission.SubmittedData,
                submittedOn = apileadsubmission.SubmittedOn,
                isProcessed = apileadsubmission.IsProcessed,
                remarks = apileadsubmission.Remarks,
                FormId = apileadsubmission.FormID,
                IPAddress = apileadsubmission.IPAddress,
                FieldUpdatedOn = DateTime.Now.ToUniversalTime(),
                FieldUpdatedBy = apileadsubmission.OwnerID
            });
            //End
            return apiLeadSubmissionID;
        }

        public void UpdateAPILeadSubmissionData(int? contactId, byte isProcessed, string remarks, int apiLeadSubmissionId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"UPDATE APILeadSubmissions SET ContactID=@contactId,IsProcessed =@isProcessed,Remarks=@remarks WHERE APILeadSubmissionID=@apiLeadSubmissionId";
            db.Execute(sql, new { contactId = contactId, isProcessed = isProcessed, remarks = remarks, apiLeadSubmissionId = apiLeadSubmissionId });
        }

        public APILeadSubmission GetAPILeadSubmittedData()
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT TOP 1 * FROM APILeadSubmissions WHERE IsProcessed=@isProcessed ORDER BY APILeadSubmissionID ASC";
            APILeadSubmission apiLeadSubmissionData = db.Get<APILeadSubmission>(sql, new { isProcessed = (byte)SubmittedFormStatus.ReadyToProcess }).FirstOrDefault();
            return apiLeadSubmissionData;
        }

        /// <summary>
        /// Get API Lead Submission Data by apiLeadSubmissionID NEXG-3014
        /// </summary>
        /// <param name="apiLeadSubmissionID"></param>
        /// <returns></returns>
        public APILeadSubmission GetAPILeadSubmittedData(int apiLeadSubmissionID)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT * FROM APILeadSubmissions WHERE APILeadSubmissionID=@APILeadSubmissionID";
            APILeadSubmission apiLeadSubmissionData = db.Get<APILeadSubmission>(sql, new { APILeadSubmissionID = apiLeadSubmissionID }).FirstOrDefault();
            return apiLeadSubmissionData;
        }

        /// <summary>
        /// Inserting Bulk savedSearchContacts
        /// </summary>
        /// <param name="savedSearchContacts"></param>
        public void InsertBulkSavedSearchesContacts(List<SmartSearchContact> savedSearchContacts)
        {
            var db = ObjectContextFactory.Create();
            List<SmartSearchContactsDb> contactsDb = (Mapper.Map<IEnumerable<SmartSearchContact>, IEnumerable<SmartSearchContactsDb>>(savedSearchContacts)).ToList();
            db.BulkInsert<SmartSearchContactsDb>(contactsDb);
            db.SaveChanges();
        }

        public void DeleteSavedSearchContactsBySearchDefinitionId(int searchDefinitionId, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"DELETE FROM SmartSearchContacts WHERE SearchDefinitionID=@searchDefinitionId AND AccountID=@accountId";
            db.Execute(sql, new { searchDefinitionId = searchDefinitionId, accountId = accountId });
        }

        public NightlyScheduledDeliverabilityReport GetSenderRecipientInfoNightlyReport()
        {
            var db = ObjectContextFactory.Create();
            NightlyScheduledDeliverabilityReport nightlyReport = new NightlyScheduledDeliverabilityReport();

            db.QueryStoredProc("dbo.GetSenderRecipientInfoNightlyReport", (reader) =>
            {
                nightlyReport.DayReport = reader.Read<SenderRecipientInfoNightlyReport>().ToList();
                nightlyReport.SevenDaysReport = reader.Read<SenderRecipientInfoNightlyReport>().ToList();

            });

            return nightlyReport;

        }

        public IEnumerable<CampaignSenderRecipientNightlyReport> GetCampaignSenderRecipientInfoNightlyReport()
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<CampaignSenderRecipientNightlyReport> campaignReport = null;

            db.QueryStoredProc("dbo.DailyCampaignReport", (reader) =>
            {
                campaignReport = reader.Read<CampaignSenderRecipientNightlyReport>().ToList();
            });

            return campaignReport;

        }

        /// <summary>
        ///  For Getting Contact Email Engagement Details.
        /// </summary>
        /// <param name="contactId"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public ContactEmailEngagementDetails GetContactEmailEngagementDtails(int contactId, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var procedureName = "[dbo].[ContactEmailEngagementSummary]";
            var parms = new List<SqlParameter>
                {
                    new SqlParameter{ParameterName ="@contactId", Value= contactId},
                    new SqlParameter{ParameterName ="@accountId", Value= accountId },

                };
            return db.ExecuteStoredProcedure<ContactEmailEngagementDetails>(procedureName, parms).FirstOrDefault();
        }

        public IEnumerable<ContactWorkflowSummary> GetContactWorkflowSummaryDetails(int contactId, int accountId)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<ContactWorkflowSummary> contactWorkflowDetails = null;

            db.QueryStoredProc("[dbo].[ContactStatusInWorkflows]", (reader) =>
            {
                contactWorkflowDetails = reader.Read<ContactWorkflowSummary>().ToList();
            }, new { contactID = contactId, accountId = accountId });

            return contactWorkflowDetails;
        }

        public IEnumerable<ContactEmailSummary> GetContactEmailSummaryDetails(int contactId, int accountId)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<ContactEmailSummary> contactEmailDetails = null;

            db.QueryStoredProc("[dbo].[Get_Send_Email_Details_Of_Contact]", (reader) =>
            {
                contactEmailDetails = reader.Read<ContactEmailSummary>().ToList();
            }, new { contactId = contactId, accountId = accountId });

            return contactEmailDetails;
        }

        public IEnumerable<ContactCampaigSummary> GetContactCampaignSummaryDetails(int contactId, int accountId)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<ContactCampaigSummary> contactCampaignDetails = null;

            db.QueryStoredProc("[dbo].[Get_Contact_Campaign_Engagement_Details]", (reader) =>
            {
                contactCampaignDetails = reader.Read<ContactCampaigSummary>().ToList();
            }, new { contactId = contactId, accountId = accountId });

            return contactCampaignDetails;
        }

        public IEnumerable<Contact> GetContacts(int accountId, int limit, int pageNumber)
        {
            IEnumerable<ContactsDb> contacts = new List<ContactsDb>();
            Logger.Current.Informational("Request for fetching contacts with accountId : " + accountId + ", pagenumber : " + pageNumber);
            var db = ObjectContextFactory.Create();

            contacts = db.Contacts.Include(c => c.Communication).Include(c => c.Image).Where(c => !c.IsDeleted && c.AccountID == accountId)
                       .OrderByDescending(c => c.LastUpdatedOn).Skip(limit * pageNumber).Take(limit)
                       .Select(s =>
                           new
                           {
                               contact = s,
                               email = db.ContactEmails.Where(ce => ce.ContactID == s.ContactID && ce.IsPrimary && !ce.IsDeleted).Select(ce => ce.Email).FirstOrDefault()
                           }).ToList().Select(x =>
                               new ContactsDb()
                               {
                                   ContactID = x.contact.ContactID,
                                   AccountID = x.contact.AccountID,
                                   FirstName = x.contact.FirstName,
                                   LastName = x.contact.LastName,
                                   Company = x.contact.Company,
                                   ContactImageUrl = x.contact.ContactImageUrl,
                                   CommunicationID = x.contact.CommunicationID,
                                   Communication = x.contact.Communication,
                                   Image = x.contact.Image,
                                   ImageID = x.contact.ImageID,
                                   PrimaryEmail = x.email
                               });

            if (contacts != null && contacts.Any())
                foreach (ContactsDb dc in contacts)
                {
                    if (dc.ContactType == ContactType.Person)
                        yield return Mapper.Map<ContactsDb, Person>(dc);
                    else
                        yield return Mapper.Map<ContactsDb, Company>(dc);
                }
            else yield return null;
        }

        public void UpdateContact(Contact con, int communicationId)
        {
            Logger.Current.Informational("Request for updating a contact");

            if (con != null)
            {
                var contact = Mapper.Map<Contact, ContactsDb>(con);
                using (var updateDb = ObjectContextFactory.Create())
                {
                    string sql = string.Empty;
                    string communicationClause = string.Empty;
                    string imageClause = string.Empty;
                    if (communicationId > 0)
                        communicationClause = "CommunicationID = @communicationId";

                    if (contact.ImageID.HasValue && contact.ImageID > 0)
                        imageClause = "ImageID = @imageId";

                    if (communicationClause.Length > 0 || imageClause.Length > 0)
                    {
                        string saperator = (communicationClause.Length > 0 && imageClause.Length > 0) ? ", " : string.Empty;

                        sql = @"UPDATE Contacts SET " + communicationClause + saperator + imageClause + " WHERE ContactID =@contactId";

                        updateDb.Execute(sql, new { communicationId = communicationId, imageId = contact.ImageID, contactId = contact.ContactID });
                    }
                }
            }
        }

        public int UpdateImage(Image image, int userId)
        {
            Logger.Current.Informational("Request for updating an image");
            var db1 = ObjectContextFactory.Create();
            int imageId = image.ImageID;
            if (image != null)
            {
                if (image.ImageID != 0)
                {
                    var varImage = db1.Images.Where(Id => Id.ImageID == image.ImageID).FirstOrDefault();
                    if (varImage != null)
                    {
                        varImage.StorageName = image.StorageName;
                        varImage.FriendlyName = image.FriendlyName;
                        varImage.OriginalName = image.OriginalName;
                    }
                }
                else
                {
                    ImagesDb newImage = new ImagesDb();
                    newImage.ImageCategoryID = ImageCategory.ContactProfile;
                    newImage.AccountID = image.AccountID;
                    newImage.FriendlyName = image.FriendlyName;
                    newImage.OriginalName = image.OriginalName;
                    newImage.StorageName = image.StorageName;
                    newImage.CreatedDate = DateTime.Now.ToUniversalTime();
                    newImage.CreatedBy = userId;
                    db1.Images.Add(newImage);
                    int updatedCount = db1.SaveChanges();
                    imageId = newImage.ImageID;
                }
            }
            return imageId;
        }

        public int InsertAndUpdateCommunication(Communication commu, int contactId, int accountId)
        {
            Logger.Current.Informational("Request for updating an image");
            var db1 = ObjectContextFactory.Create();
            int communicationId = 0;
            if (commu != null && contactId != 0)
            {
                CommunicationsDb communication = Mapper.Map<Communication, CommunicationsDb>(commu);
                CommunicationsDb communicationDb = db1.Contacts.Where(w => w.ContactID == contactId && w.AccountID == accountId && !w.IsDeleted).Include(i => i.Communication).Select(s => s.Communication).FirstOrDefault();
                if (communicationDb != null)
                    communication.CommunicationID = communicationDb.CommunicationID;

                if (communication.CommunicationID != 0)
                {
                    var db2 = ObjectContextFactory.Create();
                    var sql = @"UPDATE Communications
                                SET FacebookUrl = @FbURL, TwitterUrl = @TwURL, GooglePlusUrl = @GooURL, LinkedInUrl = @LinkURL
                                WHERE CommunicationID = @ID";
                    db2.Execute(sql, new { ID = communication.CommunicationID, FbURL = communication.FacebookUrl, TwURL = communication.TwitterUrl, GooURL = communication.GooglePlusUrl, LinkURL = communication.LinkedInUrl });
                    communicationId = communication.CommunicationID;
                }
                else
                {
                    db1.Entry<CommunicationsDb>(communication).State = System.Data.Entity.EntityState.Added;
                    db1.SaveChanges();
                    communicationId = communication.CommunicationID;
                }
            }
            return communicationId;
        }

        public List<ContactOwnerPhone> GetContactOwerPhoneNubers(List<int> contactIds)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT C.ContactID, CASE
                        WHEN U.PrimaryPhoneType = 'H' THEN U.HomePhone
	                    WHEN U.PrimaryPhoneType = 'W' THEN U.WorkPhone
	                    WHEN U.PrimaryPhoneType = 'M' THEN U.MobilePhone
	                    ELSE CASE
		                    WHEN A.HomePhone IS NOT NULL THEN A.HomePhone
		                    WHEN A.WorkPhone IS NOT NULL THEN A.WorkPhone
		                    WHEN A.MobilePhone IS NOT NULL THEN A.MobilePhone END
	                    END AS OwnerNumber FROM Contacts (NOLOCK) C
                    JOIN Users (NOLOCK) U ON U.UserID = C.OwnerID AND U.IsDeleted=0 and U.Status=1
                    JOIN Accounts (NOLOCK) A ON A.AccountID = C.AccountID
                    WHERE C.ContactID IN @ContactIds";
            List<ContactOwnerPhone> ownerNumbers = db.Get<ContactOwnerPhone>(sql, new { ContactIds = contactIds }).ToList();
            return ownerNumbers;
        }

        /// <summary>
        /// Get All NeverBounce Contact Bad Email Contact Ids.
        /// </summary>
        /// <param name="neverBounceRequestId"></param>
        /// <returns></returns>
        public IEnumerable<int> GetNeverBounceBadEmailContactIds(int neverBounceRequestId, byte emailStatus)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT NBES.ContactID FROM NeverBounceEmailStatus (NOLOCK) NBES WHERE NBES.NeverBounceRequestID=@NeverBounceID AND NBES.EmailStatus= " + emailStatus;
            return db.Get<int>(sql, new { NeverBounceID = neverBounceRequestId }).ToList();
        }

        public List<LinkClickedDetails> GetEmailClickedLinkURLs(int sentMailDetailId,int contactId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @";WITH CTE AS(
                        SELECT EL.LinkURL AS URL,ES.ActivityDate AS ClickedDate,ROW_NUMBER() over(PARTITION BY EL.LinkURL ORDER BY ES.ActivityDate) AS RN FROM EnterpriseCommunication..EmailStatistics (NOLOCK) ES
                        JOIN EnterpriseCommunication..EmailLinks (NOLOCK) EL ON EL.EmailLinkID= ES.EmailLinkID
                        WHERE ES.SentMailDetailID=@SentMailDetailId AND ES.ContactID=@ContactId AND ES.ActivityType=2)
			            SELECT * FROM CTE WHERE RN=1 ORDER BY ClickedDate DESC";
            return db.Get<LinkClickedDetails>(sql, new { SentMailDetailId = sentMailDetailId , ContactId = contactId }).ToList();
        }

        public List<LinkClickedDetails> GetCampaignClickedLinkURLs(int campaignid, int campaignRecipientId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @";WITH CTE AS
		                    (
			                    SELECT CL.URL,CS.ActivityDate AS ClickedDate,ROW_NUMBER() OVER(PARTITION BY CL.URL ORDER BY CS.ActivityDate) AS RN from CampaignStatistics (nolock) cs 
			                    join CampaignLinks (nolock) CL on CL.CampaignLinkID = CS.CampaignLinkID
			                    where CS.CampaignID=@CampaignId and CS.CampaignRecipientID=@RecipientId and cs.ActivityType=2
		                    )
		                    SELECT * FROM CTE WHERE RN=1 ORDER BY ClickedDate DESC";
            return db.Get<LinkClickedDetails>(sql, new { CampaignId = campaignid, RecipientId= campaignRecipientId }).ToList();
        }
    }
}
