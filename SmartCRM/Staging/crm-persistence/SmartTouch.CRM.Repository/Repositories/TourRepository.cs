using AutoMapper;
using LandmarkIT.Enterprise.Utilities.Logging;
using LinqKit;
using SmartTouch.CRM.Domain.Tours;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class TourRepository : Repository<Tour, int, TourDb>, ITourRepository
    {
        public TourRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory) { }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Tour> FindAll()
        {
            var tours = ObjectContextFactory.Create().Tours.Include(i => i.TourContacts);
            foreach (var tourDb in tours)
                yield return ConvertToDomain(tourDb);
        }

        /// <summary>
        /// Gets the tour by identifier.
        /// </summary>
        /// <param name="tourId">The tour identifier.</param>
        /// <returns></returns>
        public Tour GetTourByID(int tourId)
        {
            var db = ObjectContextFactory.Create();
            TourDb tourDb = db.Tours.Where(t => t.TourID == tourId).FirstOrDefault();
            if (tourDb.SelectAll == false)
            {
                var contacts = db.ContactTours.Include(n => n.Contact).Where(n => n.TourID == tourId)
                   .Select(a => a.Contact).ToList();
                var userIds = db.UserTours.Where(s => s.TourID == tourId).Select(i => i.UserID).ToList();
                tourDb.OwnerIds = userIds;
                tourDb.Contacts = contacts.Where(c => c.IsDeleted == false).ToList();
            }
            else
            {
                var userIds = db.UserTours.Where(s => s.TourID == tourId).Select(i => i.UserID).ToList();
                tourDb.OwnerIds = userIds;
            }
            if (tourDb != null)
            {
                return ConvertToDomain(tourDb);
            }
            return null;
        }

        /// <summary>
        /// Gets the tour completed status.
        /// </summary>
        /// <param name="tourId">The tour identifier.</param>
        /// <returns></returns>
        public bool GetTourCompletedStatus(int tourId)
        {
            var db = ObjectContextFactory.Create();
            var inCompletedTourContactCount = db.ContactTours.Where(p => p.TourID == tourId && p.IsCompleted == false).Count();
            if (inCompletedTourContactCount == 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Gets the contact completed tours.
        /// </summary>
        /// <param name="tourIds">The tour ids.</param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<int, int>> GetContactCompletedTours(IEnumerable<int> tourIds)
        {
            var db = ObjectContextFactory.Create();
            var completedTours = db.ContactTours.Where(p => tourIds.Contains(p.TourID) && p.IsCompleted == true)
                .Select(c => new KeyValuePair<int, int>(c.TourID, c.ContactID));
            return completedTours;
        }

        /// <summary>
        /// Searches the tour.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<Tour> SearchTour(string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method returns IEnumerable list of Tours when searched by ContactID
        /// </summary>
        /// <param name="contactId"></param>
        /// <returns></returns>
        public IEnumerable<Tour> FindByContact(int[] contactIds)
        {
            var db = ObjectContextFactory.Create();
            var contactTourMapIds = db.ContactTours.Where(c => contactIds.Contains(c.ContactID)).Select(c => c.TourID).ToList();
            IEnumerable<TourDb> toursDb = db.Tours.Where(c => contactTourMapIds.Contains(c.TourID)).ToList();
            IEnumerable<Tour> tours = ConvertDbListToDomainList(toursDb);
            tours.Concat(tours);
            return tours;
        }

        /// <summary>
        /// Finds the by contact.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public IEnumerable<Tour> FindByContact(int contactId)
        {
            var db = ObjectContextFactory.Create();

            var sql = @"DECLARE @UserTourMap TABLE (ID INT IDENTITY(1,1),TourID INT, UserName VARCHAR(MAX))
                        INSERT INTO @UserTourMap
                        SELECT T.TourID, UserName = STUFF(
                                                                 (SELECT ',  '+ U.FirstName +' ' + U.LastName FROM UserTourMap UTM (NOLOCK)
                                                                        INNER JOIN Users U (NOLOCK) ON U.UserID = UTM.UserID
                                                                 WHERE UTM.TourID = T.TourID FOR XML PATH('')),
                                                                 1,
                                                                 2,
                                                                 '') FROM Tours (NOLOCK) T
                        INNER JOIN ContactTourMap (NOLOCK) CTM ON CTM.TourID = T.TourID
                        WHERE CTM.ContactID = @contactID
 
                        SELECT  T.TourID as Id,CTM.IsCompleted,T.TourDetails,T.ReminderDate,T.CommunityID,T.TourDate,UTM.UserName AS UserName  FROM Tours (nolock) T
                        INNER JOIN ContactTourMap (nolock) CTM on CTM.TourID = T.TourID
                        LEFT JOIN @UserTourMap UTM ON UTM.TourID = T.TourID
                        WHERE CTM.ContactID=@contactID";
            var tours = db.Get<Tour>(sql, new { contactID = contactId }).ToList();

            return tours;

        }

        /// <summary>
        /// Tours the completed.
        /// </summary>
        /// <param name="tourId">The tour identifier.</param>
        /// <param name="status">if set to <c>true</c> [status].</param>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="completedForAll">if set to <c>true</c> [completed for all].</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="updatedOn">The updated on.</param>
        public void TourCompleted(int tourId, bool status, int? contactId, bool completedForAll, int userId, DateTime updatedOn)
        {
            var db = ObjectContextFactory.Create();
            var contactTour = new List<ContactTourMapDb>();
            if (!completedForAll)
            {
                contactTour = db.ContactTours.Where(c => c.TourID == tourId && c.ContactID == contactId).ToList();
                contactTour.ForEach(x =>
                {
                    x.IsCompleted = status;
                    x.LastUpdatedBy = userId;
                    x.LastUpdatedOn = updatedOn;
                });
            }
            else
            {
                contactTour = db.ContactTours.Where(c => c.TourID == tourId).ToList();
                contactTour.ForEach(x =>
                {
                    x.IsCompleted = status;
                    x.LastUpdatedBy = userId;
                    x.LastUpdatedOn = updatedOn;
                });
            }
            db.SaveChanges();
        }

        /// <summary>
        /// This method returns the ContactTourMapID of a particular tour for a particular contact
        /// </summary>
        /// <param name="tourId"></param>
        /// <param name="contactId"></param>
        /// <returns></returns>
        public int GetContactTourMapId(int tourId, int contactId)
        {
            return 1;
        }

        /// <summary>
        /// This method takes IEnumerable list of TourDb type and converts them to IEnumerable list of Tour type.
        /// </summary>
        /// <param name="toursDb"></param>
        /// <returns></returns>
        public IEnumerable<Tour> ConvertDbListToDomainList(IEnumerable<TourDb> toursDb)
        {
            var db = ObjectContextFactory.Create();
            foreach (TourDb tourDb in toursDb)
            {
                var contacts = db.ContactTours.Include(n => n.Contact).Where(n => n.TourID == tourDb.TourID)
               .Select(a => a.Contact).ToList();
                tourDb.Contacts = contacts.Where(c => c.IsDeleted == false).ToList();
                Tour tour = ConvertToDomain(tourDb);
                yield return tour;
            }
        }

        /// <summary>
        /// Finds the by tour date.
        /// </summary>
        /// <param name="tourDate">The tour date.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<Tour> FindByTourDate(DateTime tourDate)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the by community.
        /// </summary>
        /// <param name="communityName">Name of the community.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<Tour> FindByCommunity(string communityName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the by details.
        /// </summary>
        /// <param name="details">The details.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<Tour> FindByDetails(string details)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the tour for contact.
        /// </summary>
        /// <param name="tourID">The tour identifier.</param>
        /// <param name="contactId">The contact identifier.</param>
        public void RemoveTourForContact(int tourID, int contactId)
        {
            var db = ObjectContextFactory.Create();
            ContactTourMapDb contactTourMapDb = db.ContactTours.FirstOrDefault(c => c.TourID == tourID && c.ContactID == contactId);
            db.ContactTours.Remove(contactTourMapDb);
            db.SaveChanges();
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="tourDb">The tour database.</param>
        /// <returns></returns>
        public override Tour ConvertToDomain(TourDb tourDb)
        {
            return Mapper.Map<TourDb, Tour>(tourDb);
        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="db">The database.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Invalid tour id has been passed. Suspected Id forgery.</exception>
        public override TourDb ConvertToDatabaseType(Tour domainType, CRMDb db)
        {
            TourDb tourDb;
            if (domainType.Id > 0)
            {
                tourDb = db.Tours.SingleOrDefault(c => c.TourID == domainType.Id);
                if (tourDb == null)
                    throw new ArgumentException("Invalid tour id has been passed. Suspected Id forgery.");
                tourDb = Mapper.Map<Tour, TourDb>(domainType, tourDb);
            }
            else //New Contact
            {
                tourDb = Mapper.Map<Tour, TourDb>(domainType);
            }
            return tourDb;
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="tourID">The tour identifier.</param>
        /// <returns></returns>
        public override Tour FindBy(int tourID)
        {
            var db = ObjectContextFactory.Create();
            ICollection<ContactsDb> contacts = new List<ContactsDb>();
            TourDb tourDatabase = db.Tours.SingleOrDefault(c => c.TourID == tourID);
            if(!tourDatabase.SelectAll)
                 contacts = db.ContactTours.Include(a => a.Contact).Where(a => a.TourID == tourID)
                    .Select(a => a.Contact).ToList();

            tourDatabase.Contacts = contacts;

            if (tourDatabase != null)
                return ConvertToDomain(tourDatabase);
            return null;
        }

        /// <summary>
        /// Finds the by tour identifier.
        /// </summary>
        /// <param name="tourID">The tour identifier.</param>
        /// <returns></returns>
        public Tour FindByTourId(int tourID)
        {
            var db = ObjectContextFactory.Create();
            TourDb tourDatabase = db.Tours.SingleOrDefault(c => c.TourID == tourID);

            ICollection<ContactsDb> contacts = db.ContactTours.Include(a => a.Contact).Where(a => a.TourID == tourID)
                   .Select(a => a.Contact).ToList();

            tourDatabase.Contacts = contacts;

            IList<int> userIds = db.UserTours.Where(t => t.TourID == tourID).Select(s => s.UserID).ToList();

            tourDatabase.OwnerIds = userIds;

            if (tourDatabase != null)
                return ConvertToDomain(tourDatabase);
            return null;
        }

        /// <summary>
        /// Contactses the count.
        /// </summary>
        /// <param name="tourId">The tour identifier.</param>
        /// <returns></returns>
        public int ContactsCount(int tourId)
        {
            var db = ObjectContextFactory.Create();
            int contactsCount = db.ContactTours.Where(a => a.TourID == tourId).Select(a => a.Contact).Count();
            return contactsCount;
        }

        /// <summary>
        /// Gets the tour contacts summary.
        /// </summary>
        /// <param name="tourId">The tour identifier.</param>
        /// <param name="contactIds">The contact ids.</param>
        /// <returns></returns>
        public IEnumerable<TourContactsSummary> GetTourContactsSummary(int tourId, IEnumerable<int> contactIds,int accountId)
        {
            var db = ObjectContextFactory.Create();
            List<TourContactsSummary> contactsSummary = new List<TourContactsSummary>();
            if (contactIds != null && contactIds.Any())
            {
                var contacts = db.Contacts.Where(w => contactIds.Contains(w.ContactID) && w.IsDeleted == false && w.AccountID == accountId).Select(s => new
                {
                    ContactID = s.ContactID,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Company = s.Company,
                    ContactType = s.ContactType,
                    ContactName = s.ContactType == ContactType.Person ? (s.FirstName + " " + s.LastName) : s.Company,
                    PrimaryEmail = db.ContactEmails.Where(cem => cem.ContactID == s.ContactID && cem.IsPrimary == true && cem.IsDeleted == false).Select(se => se.Email).FirstOrDefault() ?? "",
                    PrimaryPhone = db.ContactPhoneNumbers.Where(cp => cp.ContactID == s.ContactID && cp.IsDeleted == false && cp.IsPrimary == true).FirstOrDefault(),
                    Lifecycle = db.DropdownValues.Where(d => d.DropdownValueID == s.LifecycleStage).Select(dr => dr.DropdownValue).FirstOrDefault()
                });
                var contactStatus = db.ContactTours.Where(a => a.TourID == tourId && contactIds.Contains(a.ContactID)).Select(s => new { contactId = s.ContactID, status = s.IsCompleted });

                contacts.ForEach(c =>
                {
                    TourContactsSummary contactSummary = new TourContactsSummary();
                    contactSummary.ContactId = c.ContactID;
                    contactSummary.ContactType = c.ContactType;
                    contactSummary.ContactName = c.ContactName;  // c.ContactType == ContactType.Person ? c.FirstName + " "+c.LastName : c.Company;
                    contactSummary.Status = contactStatus.Where(w => w.contactId == c.ContactID).Select(s => s.status).FirstOrDefault();
                    contactSummary.PrimaryEmail = c.PrimaryEmail;
                    contactSummary.PrimaryPhone = c.PrimaryPhone != null ? c.PrimaryPhone.PhoneNumber : "";
                    contactSummary.PhoneCountryCode = c.PrimaryPhone != null ? c.PrimaryPhone.CountryCode : "";
                    contactSummary.PhoneExtension = c.PrimaryPhone != null ? c.PrimaryPhone.Extension : "";
                    contactSummary.Lifecycle = c.Lifecycle;
                    contactsSummary.Add(contactSummary);
                });
            }
            return contactsSummary;
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="context">The context.</param>
        public override void PersistValueObjects(Tour domainType, TourDb dbType, CRMDb context)
        {
            persistContactTours(domainType, dbType, context);
            PersistUserTours(domainType, dbType, context);
            //persistTourOutlookSync(domainType, dbType, context);
        }

        /// <summary>
        /// Persists the tour outlook synchronize.
        /// </summary>
        /// <param name="tour">The tour.</param>
        /// <param name="toursDb">The tours database.</param>
        /// <param name="db">The database.</param>
        public void UpdateCRMOutlookMap(Tour tour, RequestOrigin? requestedFrom)
        {
            CRMOutlookSyncDb outlookSyncDb;
            var db = ObjectContextFactory.Create();
            if (tour.Id > 0)
            {
                var taskCurrentSyncStatus = requestedFrom != RequestOrigin.Outlook ? (short)OutlookSyncStatus.NotInSync : (short)OutlookSyncStatus.InSync;
                outlookSyncDb = db.CRMOutlookSync.Where(o => o.EntityID == tour.Id && o.EntityType == (byte)AppModules.ContactTours).FirstOrDefault();
                if (outlookSyncDb != null)
                {
                    outlookSyncDb.SyncStatus = taskCurrentSyncStatus;
                }
                else
                {
                    outlookSyncDb = new CRMOutlookSyncDb()
                    {
                        EntityID = tour.Id,
                        SyncStatus = taskCurrentSyncStatus,
                        LastSyncDate = tour.LastUpdatedOn,
                        LastSyncedBy = tour.LastUpdatedBy,
                        EntityType = (byte)AppModules.ContactTours,
                        OutlookKey = string.Empty
                    };
                    db.Entry<CRMOutlookSyncDb>(outlookSyncDb).State = System.Data.Entity.EntityState.Added;
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Persists the contact tours.
        /// </summary>
        /// <param name="tour">The tour.</param>
        /// <param name="tourDb">The tour database.</param>
        /// <param name="db">The database.</param>
        public void persistContactTours(Tour tour, TourDb tourDb, CRMDb db)
        {
            if (tour.SelectAll == false)
            {
                var tourContactsDb = new List<ContactTourMapDb>();
                string ContactIds = string.Empty;
                if(tour.ContactIDS != null && tour.ContactIDS.Any())
                    ContactIds = string.Join(",", tour.ContactIDS);
                else
                    ContactIds = string.Join(",", tour.Contacts.Select(p => p.Id).ToArray());


                var sql = @"select  CAST(r.datavalue AS INTEGER)  from (select * from ContactTourMap cam right outer join  dbo.Split(@ContactIds,',') 
                        split on split.DataValue = cam.contactid and cam.TourID = @TourID) r where r.TourID is null";
                var dbData = ObjectContextFactory.Create();
                var tourContacts = dbData.Get<int>(sql, new { TourID = tour.Id, ContactIds = ContactIds });

                if (tourContacts != null && tourContacts.Any())
                    foreach (int id in tourContacts.Where(w => !w.Equals(0)))
                    {
                        tourContactsDb.Add(new ContactTourMapDb()
                        {
                            TourID = tourDb.TourID,
                            ContactID = id,
                            IsCompleted = tour.IsCompleted,
                            LastUpdatedOn = DateTime.Now.ToUniversalTime(),
                            LastUpdatedBy = tour.CreatedBy
                        });

                    }

                db.ContactTours.AddRange(tourContactsDb);

                IList<int> contactIds = tour.Contacts.Where(n => n.Id > 0).Select(n => n.Id).ToList();
                var unMapTourContacts = db.ContactTours.Where(n => !contactIds.Contains(n.ContactID) && n.TourID == tour.Id).ToArray();
                db.ContactTours.RemoveRange(unMapTourContacts);
            }
        }

        /// <summary>
        /// Persists the user tours.
        /// </summary>
        /// <param name="tour">The tour.</param>
        /// <param name="tourDb">The tour database.</param>
        /// <param name="db">The database.</param>
        public void PersistUserTours(Tour tour, TourDb tourDb, CRMDb db)
        {
                var userTourDb = new List<UserTourMapDb>();
                var dbData = ObjectContextFactory.Create();
                var sql = @"SELECT UAM.UserID  FROM UserTourMap (NOLOCK) UAM WHERE UAM.TourID = @tourId";
                var dbData1 = ObjectContextFactory.Create();
                var tourUsers = dbData1.Get<int>(sql, new { tourId = tour.Id});
                tourUsers = tour.OwnerIds.Except(tourUsers);
                foreach (int userId in tourUsers)
                {
                    userTourDb.Add(new UserTourMapDb()
                    {
                        TourID = tourDb.TourID,
                        UserID = userId,
                        LastUpdatedOn = DateTime.Now.ToUniversalTime(),
                        LastUpdatedBy = tour.CreatedBy,
                        UserEmailGuid = (tour.EmailGuid != null && tour.EmailGuid.Any()) ? tour.EmailGuid.Where(s => s.Key == userId).Select(v => v.Value).FirstOrDefault(): new Guid(),
                        UserTextGuid = (tour.TextGuid != null && tour.TextGuid.Any()) ? tour.TextGuid.Where(s => s.Key == userId).Select(v => v.Value).FirstOrDefault() : new Guid()
                    });
                }

                db.UserTours.AddRange(userTourDb);
                var UnMapTourUsers = db.UserTours.Where(s => !tour.OwnerIds.Contains(s.UserID) && s.TourID == tour.Id).ToArray();
                db.UserTours.RemoveRange(UnMapTourUsers);
        }

        /// <summary>
        /// Deletes the tour.
        /// </summary>
        /// <param name="tourId">The tour identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public Dictionary<int, Guid?> DeleteTour(int tourId, int userId,int contactid)
        {
            var db = ObjectContextFactory.Create();
            if (contactid == 0)
            {
                var tourdb = db.Tours.Where(n => n.TourID == tourId).FirstOrDefault();
                Dictionary<int, Guid?> guids = new Dictionary<int, Guid?>();
                guids.Add(1, tourdb.EmailRequestGuid);
                guids.Add(2, tourdb.TextRequestGuid);
                var contactTours = db.ContactTours.Where(n => n.TourID == tourId).ToList();
                if(contactTours != null)
                    db.ContactTours.RemoveRange(contactTours);

                var userTours = db.UserTours.Where(u => u.TourID == tourId).ToList();
                if(userTours != null)
                    db.UserTours.RemoveRange(userTours);

                db.Tours.Remove(tourdb);
                db.SaveChanges();
                // var tourauditdb1 = db.Tours_Audit.Where(p => p.TourID == tourId).ToList();
                var tourauditdb = db.Tours_Audit.Where(p => p.TourID == tourId && p.AuditAction == "D").FirstOrDefault();
                if (tourauditdb != null)
                {
                    tourauditdb.LastUpdatedBy = userId;
                    db.SaveChanges();
                }
                var outlookCRMSyncEntity = db.CRMOutlookSync.Where(c => c.EntityID == tourId && c.EntityType == (short)AppModules.ContactTours).FirstOrDefault();
                if (outlookCRMSyncEntity != null)
                {
                    outlookCRMSyncEntity.SyncStatus = (short)OutlookSyncStatus.Deleted;
                    db.Entry<CRMOutlookSyncDb>(outlookCRMSyncEntity).State = System.Data.Entity.EntityState.Modified;
                }
                db.SaveChanges();
                return guids;
            }
            else
            {
                var tourDb = db.Tours.Where(n => n.TourID == tourId).FirstOrDefault();
                var userToursdb = db.UserTours.Where(n => n.TourID == tourId).ToList();
                if (userToursdb != null)
                    db.UserTours.RemoveRange(userToursdb);

                var tourcontactdb = db.ContactTours.Where(p => p.TourID == tourId && p.ContactID == contactid).FirstOrDefault();
                if (tourcontactdb != null)
                    db.ContactTours.Remove(tourcontactdb);
                if (tourDb != null)
                    db.Tours.Remove(tourDb);

                var outlookCRMSyncEntity = db.CRMOutlookSync.Where(c => c.EntityID == tourId && c.EntityType == (short)AppModules.ContactTours).FirstOrDefault();
                if (outlookCRMSyncEntity != null)
                {
                    outlookCRMSyncEntity.SyncStatus = (short)OutlookSyncStatus.Deleted;
                    db.Entry<CRMOutlookSyncDb>(outlookCRMSyncEntity).State = System.Data.Entity.EntityState.Modified;
                }
                db.SaveChanges();
                return null;
            }
          
        }

        /// <summary>
        /// Tours the contacts count.
        /// </summary>
        /// <param name="tourId">The tour identifier.</param>
        /// <returns></returns>
        public int TourContactsCount(int tourId)
        {
            var db = ObjectContextFactory.Create();
            int tourContactsCount = db.ContactTours.Where(a => a.TourID == tourId).Select(a => a.Contact).Count();
            return tourContactsCount;
        }

        /// <summary>
        /// Determines whether [is tour from select all] [the specified tour identifier].
        /// </summary>
        /// <param name="tourId">The tour identifier.</param>
        /// <returns></returns>
        public bool IsTourFromSelectAll(int tourId)
        {
            var db = ObjectContextFactory.Create();
            bool selectAll = db.Tours.Where(p => p.TourID == tourId).Select(p => p.SelectAll).FirstOrDefault();
            return selectAll;
        }

        /// <summary>
        /// Gets the contact comunity.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public short[] GetContactComunity(int contactId)
        {
            var db = ObjectContextFactory.Create();
            var contactTour = db.ContactTours.Where(i => i.ContactID == contactId).Select(i => i.TourID);
            short[] getContactTour = db.Tours.Where(c => contactTour.Contains(c.TourID)).Select(c => c.CommunityID).ToArray();
            return getContactTour;
        }

        /// <summary>
        /// Tourses the by leadsource area chart details.
        /// </summary>
        /// <param name="accountID">The account identifier.</param>
        /// <param name="userID">The user identifier.</param>
        /// <param name="isAccountAdmin">if set to <c>true</c> [is account admin].</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <returns></returns>
        public IEnumerable<DashboardAreaChart> ToursByLeadsourceAreaChartDetails(int accountID, int userID, bool isAccountAdmin, DateTime fromDate, DateTime toDate)
        {
            using (var db = ObjectContextFactory.Create())
            {
                Logger.Current.Informational("Created the procedure name to get the ToursByLeadsourcelist for Dashboard ");
                var procedureName = "[dbo].[GET_Account_Traffic_LeadSource_AreaChart]";
                Logger.Current.Informational("Created the parametes for the procedure");
                //var isaccountdminID = 0;
                //if (isAccountAdmin == true)
                //    isaccountdminID = 1;
                var parms = new List<SqlParameter>
                {   
  
                    new SqlParameter{ParameterName ="@AccountID", Value= accountID},
                    new SqlParameter{ParameterName ="@FromDate", Value= fromDate.Date},
                    new SqlParameter{ParameterName="@ToDate ", Value = toDate.Date},
                    new SqlParameter{ParameterName="@IsAdmin", Value = isAccountAdmin},
                    new SqlParameter{ParameterName="@OwnerID", Value = userID},
                   
                };
                // var lstcontacts = context.ExecuteStoredProcedure<int>(procedureName, parms);
                var toursByleadsourcesList = db.ExecuteStoredProcedure<DashboardAreaChart>(procedureName, parms);
                return toursByleadsourcesList;
            }
        }

        /// <summary>
        /// Tourses the by source pie chart details.
        /// </summary>
        /// <param name="accountID">The account identifier.</param>
        /// <param name="userID">The user identifier.</param>
        /// <param name="isAccountAdmin">if set to <c>true</c> [is account admin].</param>
        /// <param name="fromdate">The fromdate.</param>
        /// <param name="toDate">To date.</param>
        /// <returns></returns>
        public IEnumerable<DashboardPieChartDetails> ToursBySourcePieChartDetails(int accountID, int userID, bool isAccountAdmin, DateTime fromdate,
            DateTime toDate)
        {
            using (var db = ObjectContextFactory.Create())
            {
                Logger.Current.Informational("Created the procedure name to get the ToursByLeadsourcelist for Dashboard ");
                var procedureName = "[dbo].[GET_Account_Traffic_LeadSource_PieChart]";
                Logger.Current.Informational("Created the parametes for the procedure");

                var parms = new List<SqlParameter>
                {   
                    new SqlParameter{ParameterName ="@AccountID", Value= accountID},
                    new SqlParameter{ParameterName ="@FromDate", Value= fromdate.Date},
                    new SqlParameter{ParameterName="@ToDate ", Value =toDate.Date},
                    new SqlParameter{ParameterName="@IsAdmin", Value = isAccountAdmin},
                    new SqlParameter{ParameterName="@OwnerID", Value = userID},
                };
                // var lstcontacts = context.ExecuteStoredProcedure<int>(procedureName, parms);
                IEnumerable<DashboardPieChartDetails> details = db.ExecuteStoredProcedure<DashboardPieChartDetails>(procedureName, parms);
                return details;
            }
        }

        /// <summary>
        /// Tourses the by type bar chart details.
        /// </summary>
        /// <param name="accountID">The account identifier.</param>
        /// <param name="userID">The user identifier.</param>
        /// <param name="isAccountAdmin">if set to <c>true</c> [is account admin].</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <returns></returns>
        public IEnumerable<DashboardBarChart> ToursByTypeBarChartDetails(int accountID, int userID, bool isAccountAdmin, DateTime fromDate, DateTime toDate)
        {
            using (var db = ObjectContextFactory.Create())
            {
                Logger.Current.Informational("Created the procedure name to get the ToursByLeadsourcelist for Dashboard ");
                var procedureName = "[dbo].[GET_Account_Traffic_Types_BarChart]";
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
                return db.ExecuteStoredProcedure<DashboardBarChart>(procedureName, parms);
            }
        }

        /// <summary>
        /// Gets the tours to synchronize.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="maxRecords">The maximum records.</param>
        /// <param name="timeStamp">The time stamp.</param>
        /// <param name="firstSync">if set to <c>true</c> [first synchronize].</param>
        /// <param name="operationType">Type of the operation.</param>
        /// <returns></returns>
        public IEnumerable<Tour> GetToursToSyncBackUp(int accountId, int userId, int? maxRecords, DateTime? timeStamp, bool firstSync, CRUDOperationType operationType)
        {
            Logger.Current.Verbose("TourRepository/GetToursToSync, parameters:  " + accountId + ", " + userId + ", " + maxRecords + ", " + timeStamp + ", " + firstSync);
            var db = ObjectContextFactory.Create();
            if (firstSync)
            {
                var userFirstTours = db.CRMOutlookSync.Join(db.Tours.Where(a => a.CreatedBy == userId), co => co.EntityID, t => t.TourID, (co, t) => new
                {
                    TourId = co.EntityID,
                    UserId = t.CreatedBy,
                    AccountId = t.AccountID
                }).ToList();

                var tourIds = userFirstTours.Select(ut => ut.TourId).ToList().Distinct();

                if (operationType == CRUDOperationType.Create)
                {
                    db.CRMOutlookSync.Where(c => tourIds.Contains(c.EntityID)
                       && c.EntityType == (byte)AppModules.ContactTours
                       && c.SyncStatus == (short)OutlookSyncStatus.Syncing)
                       .Each(c => { c.SyncStatus = (short)OutlookSyncStatus.NotInSync; });
                }

                var firstTimeSyncTours = db.Tours.Where(c => c.CreatedBy == userId && !tourIds.Contains(c.TourID))
                    .Select(c => c.TourID);

                foreach (int id in firstTimeSyncTours)
                {
                    CRMOutlookSyncDb tourOutlookSyncDb = new CRMOutlookSyncDb();
                    tourOutlookSyncDb.EntityID = id;
                    tourOutlookSyncDb.SyncStatus = (short)OutlookSyncStatus.NotInSync;
                    tourOutlookSyncDb.EntityType = (byte)AppModules.ContactTours;
                    db.Entry<CRMOutlookSyncDb>(tourOutlookSyncDb).State = System.Data.Entity.EntityState.Added;
                }
                db.SaveChanges();
            }
            var userTours = db.CRMOutlookSync.Where(co => co.SyncStatus == (short)OutlookSyncStatus.NotInSync)
                .Join(db.Tours.Where(a => a.CreatedBy == userId), co => co.EntityID, t => t.TourID, (co, t) => new
            {
                TourId = co.EntityID,
                UserId = t.CreatedBy,
                AccountId = t.AccountID
            });

            var tourIdsToSync = userTours.Select(ut => ut.TourId).ToList().Distinct();

            var toursToSyncDb = db.Tours
                .Where(c => c.AccountID == accountId && c.CreatedBy == userId && tourIdsToSync.Contains(c.TourID))
                .Take((int)maxRecords).ToList();

            return Mapper.Map<IEnumerable<TourDb>, IEnumerable<Tour>>(toursToSyncDb);
        }

        /// <summary>
        /// Gets the tours to synchronize.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="maxNumRecords">The maximum number records.</param>
        /// <param name="timeStamp">The time stamp.</param>
        /// <param name="firstSync">if set to <c>true</c> [first synchronize].</param>
        /// <param name="operationType">Type of the operation.</param>
        /// <returns></returns>
        public IEnumerable<Tour> GetToursToSync(int accountId, int userId, int? maxNumRecords, DateTime? timeStamp, bool firstSync, CRUDOperationType operationType)
        {
            Logger.Current.Verbose("TourRepository/GetToursToSync, parameters:  " + accountId + ", " + userId + ", " + maxNumRecords + ", " + timeStamp + ", " + firstSync);
            var db = ObjectContextFactory.Create();
            //IEnumerable<CRMOutlookSyncDb> contactsToSyncDb;
            if (firstSync)
            {
                var tourIds = db.Get<int>(@"select EntityID from CRMOutlookSync os 
                                                inner join tours c on os.EntityID = c.TourId
                                                where entitytype = 7 and c.createdby = @userId"
                                            , new { userId = userId });

                if (operationType == CRUDOperationType.Create)
                {
                    var sql = @"update CRMOutlookSync set SyncStatus = @notInSync  where (SyncStatus = 11 or syncstatus = 13) and entityid in @tourIds and entitytype = 7";
                    db.Execute(sql, new { notInSync = 11, tourIds = tourIds });
                }

                var firstTimeSyncTours = db.Tours
                    .Where(c => c.CreatedBy == userId
                     && !tourIds.Contains(c.TourID))
                    .Select(c => c.TourID).ToList();

                foreach (int id in firstTimeSyncTours)
                {
                    CRMOutlookSyncDb tourOutlookSyncDb = new CRMOutlookSyncDb();
                    tourOutlookSyncDb.EntityID = id;
                    tourOutlookSyncDb.SyncStatus = (short)OutlookSyncStatus.NotInSync;
                    tourOutlookSyncDb.EntityType = (byte)AppModules.ContactTours;
                    db.Entry<CRMOutlookSyncDb>(tourOutlookSyncDb).State = System.Data.Entity.EntityState.Added;
                }
                db.SaveChanges();
            }

            var tourIdsToSync = db.CRMOutlookSync.Where(c => c.SyncStatus == (short)OutlookSyncStatus.NotInSync && c.EntityType == (byte)AppModules.ContactTours)
                .Select(c => c.EntityID);
            var tourIds1ToSyncDb = db.Tours
               
                .Where(c => c.AccountID == accountId
                    && c.CreatedBy == userId
                    && tourIdsToSync.Contains(c.TourID))
                .Take((int)maxNumRecords)
                .ToList();

            
            var toursToSync = Mapper.Map<IEnumerable<TourDb>, IEnumerable<Tour>>(tourIds1ToSyncDb);

            return toursToSync;
        }

        /// <summary>
        /// Gets the deleted tours to synchronize.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="maxNumRecords">The maximum number records.</param>
        /// <param name="timeStamp">The time stamp.</param>
        /// <returns></returns>
        public IEnumerable<int> GetDeletedToursToSync(int accountId, int userId, int? maxNumRecords, DateTime? timeStamp)
        {
            var db = ObjectContextFactory.Create();

            var tourIds = db.CRMOutlookSync.Where(c =>
                   c.EntityType == (byte)AppModules.ContactTours
                   && c.SyncStatus == (short)OutlookSyncStatus.Deleted
                   && c.User.AccountID == accountId).Select(c => c.EntityID);            

            return tourIds;
        }

        /// <summary>
        /// Get User Guid
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public Guid GetUserEmailGuidByUserId(int userId,int tourId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT UserEmailGuid  FROM UserTourMap (NOLOCK) WHERE UserID=@userID AND TourID=@tourID ";
            Guid userGuid = db.Get<Guid>(sql, new { userID = userId, tourID = tourId }).FirstOrDefault();
            return userGuid;
        }

        /// <summary>
        /// Get All Ownerids
        /// </summary>
        /// <param name="tourId"></param>
        /// <returns></returns>
        public IEnumerable<int> GetAssignedUserIds(int tourId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT UserID  FROM UserTourMap (NOLOCK) WHERE TourID=@tourID";
            IEnumerable<int> userIds = db.Get<int>(sql, new { tourID = tourId }).ToList();
            return userIds;
        }

        public IEnumerable<int> GetContactIds(int tourId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"select ContactID from ContactTourMap(NOLOCK) where tourid = @tourID";
            IEnumerable<int> contactIds = db.Get<int>(sql, new { tourID = tourId }).ToList();
            return contactIds;
        }

        /// <summary>
        /// Gets the user text email unique identifier.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="tourId">The tour identifier.</param>
        /// <returns></returns>
        public Guid GetUserTextEmailGuid(int userId, int tourId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT UserTextGuid   FROM UserTourMap (NOLOCK) WHERE UserID=@userID AND TourID=@tourID ";
            Guid userGuid = db.Get<Guid>(sql, new { userID = userId, tourID = tourId }).FirstOrDefault();
            return userGuid;
        }

        /// <summary>
        /// Gets the user email guids.
        /// </summary>
        /// <param name="tourId">The tour identifier.</param>
        /// <returns></returns>
        public IEnumerable<Guid> GetUserEmailGuids(int tourId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT UserEmailGuid  FROM UserTourMap(NOLOCK) WHERE TourID=@tourID ";
            IEnumerable<Guid> emailGuids = db.Get<Guid>(sql, new { tourID = tourId }).ToList();
            return emailGuids;
        }

        /// <summary>
        /// Gets the user text guids.
        /// </summary>
        /// <param name="tourId">The tour identifier.</param>
        /// <returns></returns>
        public IEnumerable<Guid> GetUserTextGuids(int tourId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT UserTextGuid  FROM UserTourMap(NOLOCK) WHERE TourID=@tourID ";
            IEnumerable<Guid> textGuids = db.Get<Guid>(sql, new { tourID = tourId }).ToList();
            return textGuids;
        }

        public void AddingTourDetailsToContactSummary(List<int> tourIds, List<int> contactIds, int accountId, int ownerId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                db.QueryStoredProc("[dbo].[TourDetailsAddingToContactSummary]", (reader) =>
                {
                }, new
                {
                    TourID = tourIds.AsTableValuedParameter("dbo.Contact_List"),
                    ContactIds= contactIds.AsTableValuedParameter("dbo.Contact_List"),
                    AccountID= accountId,
                    OwnerID= ownerId

                });
            }
        }
    }

}
