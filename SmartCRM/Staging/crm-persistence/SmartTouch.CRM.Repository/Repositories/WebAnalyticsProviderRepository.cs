using AutoMapper;
using LandmarkIT.Enterprise.Utilities.Logging;
using LinqKit;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Reports;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.WebAnalytics;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class WebAnalyticsProviderRepository : Repository<WebAnalyticsProvider, int, WebAnalyticsProvidersDb>, IWebAnalyticsProviderRepository
    {
        readonly IUserRepository userRepository;
        public WebAnalyticsProviderRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory, IUserRepository userRepository)
            : base(unitOfWork, objectContextFactory)
        {
            this.userRepository = userRepository;
        }

        /// <summary>
        /// Adds the web visists.
        /// </summary>
        /// <param name="contactWebVisits">The contact web visits.</param>
        /// <param name="webAnalyticsProvider">The web analytics provider.</param>
        /// <param name="lastAPICallTimeStamp">The last API call time stamp.</param>
        /// <param name="splitVisitInterval">The split visit interval.</param>
        /// <returns></returns>
        public List<WebVisit> AddWebVisists(IList<WebVisit> contactWebVisits, WebAnalyticsProvider webAnalyticsProvider, DateTime lastAPICallTimeStamp, short splitVisitInterval)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var knownContactReferences = contactWebVisits.Select(c => c.ContactReference).ToList();
                IEnumerable<WebVisitContact> knownContacts = GetContactsAssociatedWithReferenceIds(knownContactReferences.ToList(), webAnalyticsProvider.AccountID);

                IEnumerable<int> contactIds = knownContacts.Select(c => (int)c.ContactID).ToList().Distinct();

                var contactsPreviousPageVisits = db.WebVisits.Where(c => contactIds.Contains(c.ContactID))
                    .GroupBy(c => c.ContactID).Select(g => g.OrderByDescending(i => i.VisitedOn).FirstOrDefault()).ToList();

                IList<WebVisitsDb> webVisitsDb = new List<WebVisitsDb>();
                IList<WebVisit> allContactsPreviousVisits = new List<WebVisit>();
                Logger.Current.Verbose("Removed unknown visits");
                Logger.Current.Informational("Total Contacts :" + knownContacts.Count());
                Logger.Current.Informational("ContactIds: " + string.Join(",",contactIds));
                foreach (WebVisitContact knownContact in knownContacts)
                {
                    Logger.Current.Informational("Contact ID:" + knownContact.ContactID);
                    IList<WebVisit> associatedWebVisits =
                        contactWebVisits.Where(c => c.ContactReference == knownContact.ReferenceId).OrderBy(c => c.VisitedOn).ToList();

                    associatedWebVisits.ForEach(c => c.ContactID = knownContact.ContactID);

                    var contactPreviousVisit = contactsPreviousPageVisits.Where(c => c.ContactID == knownContact.ContactID).FirstOrDefault();
                    var currentVisitReference = contactPreviousVisit != null && !string.IsNullOrEmpty(contactPreviousVisit.VisitReference)
                        ? contactPreviousVisit.VisitReference : Guid.NewGuid().ToString("D");

                    TimeSpan timeSpan;
                    if (associatedWebVisits != null && associatedWebVisits.Count > 0 && contactPreviousVisit != null)
                    {
                        db.Entry(contactPreviousVisit).State = EntityState.Modified;

                        //associatedWebVisits.FirstOrDefault().VisitReference = currentVisitReference;
                        timeSpan = associatedWebVisits.FirstOrDefault().VisitedOn
                                - contactPreviousVisit.VisitedOn;
                        timeSpan = timeSpan.TotalSeconds > short.MaxValue ? new TimeSpan(0, 0, 0, short.MaxValue) : timeSpan;

                        if (timeSpan.TotalSeconds > splitVisitInterval)
                        {
                            timeSpan = timeSpan.TotalSeconds > splitVisitInterval ? new TimeSpan(0, 0, 0, splitVisitInterval) : timeSpan;
                            contactPreviousVisit.Duration = splitVisitInterval;
                            currentVisitReference = Guid.NewGuid().ToString("D");
                            associatedWebVisits.FirstOrDefault().IsVisit = true;
                        }
                        else
                        {
                            contactPreviousVisit.Duration = (short)timeSpan.TotalSeconds;
                        }
                        associatedWebVisits.FirstOrDefault().VisitReference = currentVisitReference;
                        allContactsPreviousVisits.Add(Mapper.Map<WebVisitsDb, WebVisit>(contactPreviousVisit));
                        Logger.Current.Informational("After converting previous visits");
                    }

                    else if (associatedWebVisits != null && associatedWebVisits.Count > 0 && contactPreviousVisit == null)
                    {
                        currentVisitReference = Guid.NewGuid().ToString("D");

                        associatedWebVisits.FirstOrDefault().IsVisit = true;
                        associatedWebVisits.FirstOrDefault().VisitReference = currentVisitReference;
                    }
                    var pageViewsCount = associatedWebVisits.ToList().Count();

                    for (int i = 1; i < pageViewsCount; i++)
                    {
                        timeSpan = associatedWebVisits[i].VisitedOn - associatedWebVisits[i - 1].VisitedOn;
                        //associatedWebVisits[i - 1].Duration = (short)(timeSpan.TotalSeconds);
                        timeSpan = timeSpan.TotalSeconds > short.MaxValue ? new TimeSpan(0, 0, 0, short.MaxValue) : timeSpan;
                        if (timeSpan.TotalSeconds > splitVisitInterval)
                        {
                            currentVisitReference = Guid.NewGuid().ToString("D");
                            associatedWebVisits[i - 1].Duration = splitVisitInterval; //Correct
                            associatedWebVisits[i].VisitReference = currentVisitReference;
                            associatedWebVisits[i].IsVisit = true;
                        }
                        associatedWebVisits[i].VisitReference = currentVisitReference;//Correct
                    }
                    Logger.Current.Informational("Total visits " + associatedWebVisits.Count());
                    foreach (var visit in associatedWebVisits.OrderByDescending(w => w.VisitedOn))
                    {
                        Logger.Current.Informational("Before adding webvisit " + visit.ContactID);
                        var webVisitDb = Mapper.Map<WebVisit, WebVisitsDb>(visit);
                        db.WebVisits.Add(webVisitDb);
                        webVisitsDb.Add(webVisitDb);
                        Logger.Current.Informational("After adding webvisit " + visit.ContactID);
                    }
                    Logger.Current.Informational("Total visits after adding this contact's visits: " + webVisitsDb.Count());
                }

                IEnumerable<int> contactsAssociated = knownContacts.Select(c => (int)c.ContactID).ToList();
                var contactOwners = db.Contacts.Where(c => contactsAssociated.Contains(c.ContactID) && c.AccountID == webAnalyticsProvider.AccountID).Select(c =>
                    new
                        {
                            ContactID = c.ContactID,
                            OwnerID = c.OwnerID,
                            FirstName = c.FirstName,
                            LastName = c.LastName,
                            Company = c.Company
                        }).ToList();
                var webAnalyticsProviderDb = db.WebAnalyticsProviders.Where(a => a.WebAnalyticsProviderID == webAnalyticsProvider.Id).FirstOrDefault();
                webAnalyticsProviderDb.LastAPICallTimeStamp = lastAPICallTimeStamp;
                db.Entry<WebAnalyticsProvidersDb>(webAnalyticsProviderDb).State = EntityState.Modified;


                db.SaveChanges();
                Logger.Current.Informational("After updating contacts from visits");
                var superAdmins = db.Users.Where(u => u.AccountID == 1 && u.IsDeleted == false).Select(c => c.UserID).ToList();
                if (webAnalyticsProvider.LastAPICallTimeStamp != null)
                {
                    foreach (var visit in webVisitsDb.Where(w => w.IsVisit == true).OrderBy(w => w.VisitedOn))
                    {
                        var contact = knownContacts.Where(k => k.ContactID == visit.ContactID).FirstOrDefault();
                        if (webAnalyticsProviderDb.NotificationStatus == true && contact != null && contact.ContactID > 0)
                        {
                            Notification notification = new Notification();
                            notification.Details = (string.IsNullOrEmpty(contact.ContactName) ? "A contact" : contact.ContactName) + " visited your website";
                            notification.EntityId = visit.ContactWebVisitID;
                            notification.Subject = visit.PageVisited;
                            notification.Time = visit.VisitedOn;
                            notification.Status = NotificationStatus.New;
                            notification.ModuleID = (byte)AppModules.WebAnalytics;
                            notification.UserID = contactOwners.Where(c => c.ContactID == visit.ContactID).Select(c => c.OwnerID).FirstOrDefault();
                            if (notification.UserID != null)
                                userRepository.AddNotification(notification);
                        }

                        var webVisitEmailAudit = new WebVisitEmailAuditDb()
                        {
                            CreatedOn = DateTime.UtcNow,
                            EmailStatus = WebVisitEmailStatus.Queued,
                            VisitReference = visit.VisitReference,

                        };
                        foreach (int superAdmin in superAdmins)
                        {
                            if (contact != null && contact.ContactID > 0)
                            {
                                Notification notification = new Notification();
                                notification.Details = (string.IsNullOrEmpty(contact.ContactName) ? "A contact" : contact.ContactName) + " visited your website";
                                notification.EntityId = visit.ContactWebVisitID;
                                notification.Subject = visit.PageVisited;
                                notification.Time = visit.VisitedOn;
                                notification.Status = NotificationStatus.New;
                                notification.ModuleID = (byte)AppModules.WebAnalytics;
                                notification.UserID = superAdmin;
                                userRepository.AddNotification(notification);
                            }
                        }
                        db.Entry<WebVisitEmailAuditDb>(webVisitEmailAudit).State = EntityState.Added;
                    }
                    db.SaveChanges();
                    Logger.Current.Informational("After updating notifications");
                }
                List<WebVisit> updatedWebVisits = Mapper.Map<IList<WebVisitsDb>, IList<WebVisit>>(webVisitsDb).ToList();
                return allContactsPreviousVisits.Concat(updatedWebVisits).ToList();
            }
        }

        /// <summary>
        /// Gets the contacts associated with reference ids.
        /// </summary>
        /// <param name="knownReferences">The known references.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        IEnumerable<WebVisitContact> GetContactsAssociatedWithReferenceIds(IList<string> knownReferences, int accountId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var contacts = db.ContactIPAddresses
                    .Where(c => knownReferences.Contains(c.UniqueTrackingID.ToString()) && c.Contact.IsDeleted == false && c.Contact.AccountID == accountId)
                    .Select(c => new WebVisitContact
                    {
                        ContactID = c.ContactID,
                        IPAddress = c.IPAddress,
                        ContactName = c.Contact.FirstName + " " + c.Contact.LastName,
                        ReferenceId = c.UniqueTrackingID
                    }).ToList().Distinct();
                return contacts;
            }
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<WebAnalyticsProvider> FindAll()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the webvisits by account.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<WebVisit> FindWebvisitsByAccount(int accountId)
        {
            var db = ObjectContextFactory.Create();
            var webvisits = db.WebVisits.Join(db.Contacts.Where(a => a.AccountID == accountId && a.IsDeleted == false), w => w.ContactID, c => c.ContactID, (w, c) => new { WebVisits = w })
                            .OrderBy(o => o.WebVisits.ContactWebVisitID).Select(s => s.WebVisits).ToList();
            return Mapper.Map<IEnumerable<WebVisitsDb>, IEnumerable<WebVisit>>(webvisits);
        }

        /// <summary>
        /// Finds all web visits.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<WebVisit> FindAllWebVisits(int pageNumber, int limit, int accountId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var webVisits = db.WebVisits
                                    .Join(db.Contacts.Where(c => c.AccountID == accountId)
                                    , wv => wv.ContactID
                                    , c => c.ContactID
                                    , (wv, c) => new { WebVisit = wv })
                                    .OrderBy(wv => wv.WebVisit.ContactWebVisitID).Skip(limit * pageNumber).Take(limit).AsNoTracking().ToList();
                var iterator = 1;
                foreach (var webVisit in webVisits.Select(c => c.WebVisit))
                {
                    yield return new WebVisit()
                    {
                        ContactID = webVisit.ContactID,
                        ContactReference = webVisit.ContactReference,
                        Duration = webVisit.Duration,
                        Id = webVisit.ContactWebVisitID,
                        IPAddress = webVisit.IPAddress,
                        IsVisit = webVisit.IsVisit,
                        PageVisited = webVisit.PageVisited,
                        VisitedOn = webVisit.VisitedOn,
                        VisitReference = webVisit.VisitReference
                    };
                    iterator++;
                }
            }
        }

        /// <summary>
        /// Finds the web visits by owner.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="ownerId">The owner identifier.</param>
        /// <param name="includePreviouslyRead">if set to <c>true</c> [include previously read].</param>
        /// <returns></returns>
        public IEnumerable<Notification> FindWebVisitsByOwner(int pageNumber, int limit, int accountId, int ownerId, bool includePreviouslyRead)
        {
            IList<Notification> notifications = new List<Notification>();

            using (var db = ObjectContextFactory.Create())
            {
                var results = db.Notifications
                    .Join(db.WebVisits, n => n.EntityID, wv => wv.ContactWebVisitID, (n, wv) => new { n, wv })
                    .Join(db.Contacts, nwv => nwv.wv.ContactID, c => c.ContactID, (nwv, c) => new { nwv, c })
                    .Where(w => (w.nwv.n.Status == NotificationStatus.New || w.nwv.n.Status == (includePreviouslyRead ? NotificationStatus.Viewed : NotificationStatus.New))
                        && w.nwv.n.UserID == ownerId
                        && w.nwv.wv.IsVisit == true
                        && w.nwv.n.ModuleID == (byte)AppModules.WebAnalytics
                        && w.c.AccountID == accountId
                        && w.c.IsDeleted == false)
                    .OrderByDescending(w => w.nwv.n.NotificationID)
                    .Skip(pageNumber * limit)
                    .Take(limit)
                    .AsNoTracking();

                if (results != null)
                    foreach (var result in results)
                    {
                        var contactEntry = new List<ContactNotificationEntry>();
                        contactEntry.Add(new ContactNotificationEntry() { ContactID = result.nwv.wv.ContactID });
                        notifications.Add(new Notification()
                        {
                            Details = result.nwv.wv.VisitReference,
                            Duration = result.nwv.wv.Duration,
                            EntityId = result.nwv.wv.ContactWebVisitID,
                            NotificationID = result.nwv.n.NotificationID,
                            Status = result.nwv.n.Status,
                            Subject = result.nwv.n.Details,
                            Time = result.nwv.wv.VisitedOn,
                            UserID = result.nwv.n.UserID,
                            ContactEntries = contactEntry
                        });
                    };
                return notifications.ToList();
            }
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override WebAnalyticsProvider FindBy(int id)
        {
            using (var db = ObjectContextFactory.Create())
            {
                WebAnalyticsProvidersDb provider = db.WebAnalyticsProviders.Where(a => a.AccountID == id).FirstOrDefault();
                if (provider != null)
                {
                    provider.NotificationGroup = db.WebVisitUserNotificationMap
                        .Where(c => c.WebAnalyticsProviderID == provider.WebAnalyticsProviderID)
                        .ToList();
                }
                else
                {
                    provider = new WebAnalyticsProvidersDb();
                }
                return ConvertToDomain(provider);
            }
        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Invalid WebAnalyticsProvider id has been passed. Suspected Id forgery.</exception>
        public override WebAnalyticsProvidersDb ConvertToDatabaseType(WebAnalyticsProvider domainType, CRMDb context)
        {
            WebAnalyticsProvidersDb webAnalyticsProvidersDb;
            if (domainType.Id > 0)
            {
                webAnalyticsProvidersDb = context.WebAnalyticsProviders.SingleOrDefault(c => c.WebAnalyticsProviderID == domainType.Id);

                if (webAnalyticsProvidersDb == null)
                    throw new ArgumentException("Invalid WebAnalyticsProvider id has been passed. Suspected Id forgery.");

                webAnalyticsProvidersDb = Mapper.Map<WebAnalyticsProvider, WebAnalyticsProvidersDb>(domainType, webAnalyticsProvidersDb);
            }
            else
            {
                webAnalyticsProvidersDb = Mapper.Map<WebAnalyticsProvider, WebAnalyticsProvidersDb>(domainType);
            }
            return webAnalyticsProvidersDb;
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="databaseType">Type of the database.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override WebAnalyticsProvider ConvertToDomain(WebAnalyticsProvidersDb databaseType)
        {
            return Mapper.Map<WebAnalyticsProvidersDb, WebAnalyticsProvider>(databaseType);
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="context">The context.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void PersistValueObjects(WebAnalyticsProvider domainType, WebAnalyticsProvidersDb dbType, CRMDb context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the web visit to be emailed.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<WebVisit> GetWebVisitToBeEmailed()
        {
            using (var db = ObjectContextFactory.Create())
            {
                var webVisitEmailAuditsDb = db.WebVisitEmailAudit.Where(w => w.EmailStatus == WebVisitEmailStatus.Queued).Select(w => w.VisitReference).ToList();
                var webVisitsDb = db.WebVisits.Where(w => webVisitEmailAuditsDb.Contains(w.VisitReference)).ToList();
                var webVisits = Mapper.Map<IEnumerable<WebVisitsDb>, IEnumerable<WebVisit>>(webVisitsDb);
                return webVisits;
            }
        }

        /// <summary>
        /// Gets the current web visits.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<WebVisitReport> GetCurrentWebVisits()
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"select wv.ContactId,c.LeadScore, wv.VisitReference,ISNULL(c.FirstName,'') FirstName, ISNULL(c.LastName,'') LastName,
					ISNULL(ep.email,'') Email, ep.PhoneNumber Phone,ep.Zip Zip,c.lifecyclestage LifecycleStageId, c.LeadScore, VisitedOn, PageViews, 
					Duration, vs.Page1, ISNULL(vs.Page2,'') Page2, ISNULL(vs.Page3,'') Page3, wv.Location,Source,c.OwnerID, c.AccountID AccountId, a.AccountName
	                 from(select contactid, VisitReference, max(visitedon) VisitedOn, sum(duration) Duration, count(visitreference) PageViews,ISNULL(Max(city +', ' + Region),'') Location,
	                 ISNULL(Max(ISPName),'') Source
		                from contactwebvisits cw  (nolock) 
		                where  isnull(VisitReference, '000') <> '000'
		                group by visitreference, contactid
	                ) wv 
	                left join GET_VisiStat_Top3_Pages vs (nolock) on wv.VisitReference = vs.VisitReference 
	                left join contacts c (nolock) on wv.contactid = c.contactid 
	                left join GET_Contact_Email_Phone_Zip ep (nolock) on wv.ContactID = ep.contactid
					left join WebVisitEmailAudit wvea(nolock)  on wv.VisitReference = wvea.VisitReference
					left join Accounts a (nolock) on c.AccountID = a.AccountID
					where wvea.EmailStatus = 106
	                order by visitedon desc";
                var visits = db.Get<WebVisitReport>(sql);
                return visits;
            }
        }

        /// <summary>
        /// Updates the web visit notifications.
        /// </summary>
        /// <param name="VisitReferences">The visit references.</param>
        public void UpdateWebVisitNotifications(IEnumerable<KeyValuePair<IEnumerable<string>, string>> VisitReferences)
        {
            using (var db = ObjectContextFactory.Create())
            {
                Logger.Current.Verbose("In UpdateWebVisitNotifications");
                var successEmails = VisitReferences.Where(w => w.Value == "Success").Select(c => c.Key).ToList();
                var failedEmails = VisitReferences.Where(w => w.Value == "Failed").Select(c => c.Key).ToList();
                if (successEmails != null && successEmails.Any())
                {
                    foreach (var successList in successEmails)
                    {
                        var sentEmailAuditsDb = db.WebVisitEmailAudit.Where(w => successList.Contains(w.VisitReference));
                        sentEmailAuditsDb.ForEach(c =>
                        {
                            c.EmailStatus = WebVisitEmailStatus.Sent;
                            c.Remarks = "Sent successfully";
                            c.SentDate = DateTime.Now.ToUniversalTime();
                            db.Entry(c).State = EntityState.Modified;
                        });
                    }
                }
                if (failedEmails != null && failedEmails.Any())
                {
                    foreach (var failedList in failedEmails)
                    {
                        var failedEmailAuditsDb = db.WebVisitEmailAudit.Where(w => failedList.Contains(w.VisitReference));
                        failedEmailAuditsDb.ForEach(c =>
                        {
                            c.EmailStatus = WebVisitEmailStatus.Failed;
                            c.Remarks = "Failed";
                            db.Entry(c).State = EntityState.Modified;
                        });
                    }
                }

                Logger.Current.Verbose("Updated web visit notifications successfully");


                db.SaveChanges();

            }

        }

        /// <summary>
        /// Gets the account ids to send web visit daily summary.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AccountBasicInfo> GetAccountIdsToSendWebVisitDailySummary()
        {
            using (var db = ObjectContextFactory.Create())
            {
                var accounts = db.WebAnalyticsProviders.Join(db.Accounts
                        , w => w.AccountID
                        , a => a.AccountID
                        , (w, a) => new { Account = a, Provider = w })
                    .Where(c => c.Account.IsDeleted == false
                        && c.Account.Status == (byte)AccountStatus.Active
                        && c.Provider.StatusID == WebAnalyticsStatus.Active
                        && c.Provider.DailyStatusEmailOpted == true)
                        .Select(c => new AccountBasicInfo
                        {
                            AccountID = c.Account.AccountID,
                            TimeZone = c.Account.TimeZone,
                            WebAnalyticsID = c.Provider != null ? (short?)c.Provider.WebAnalyticsProviderID : null,
                            AccountName = c.Account.AccountName
                        })
                    .ToList();
                return accounts;
            }
        }

        /// <summary>
        /// Gets the web visit daily summary.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public IEnumerable<WebVisitReport> GetWebVisitDailySummary(int accountId, DateTime startDate, DateTime endDate)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"	select 
	                wv.ContactId,c.LeadScore, wv.VisitReference,ISNULL(c.FirstName,'') FirstName, ISNULL(c.LastName,'') LastName,
					ISNULL(ep.email,'') Email, ep.PhoneNumber Phone,ep.Zip Zip,c.lifecyclestage LifecycleStageId, VisitedOn, PageViews, 
					Duration, vs.Page1, ISNULL(vs.Page2,'') Page2, ISNULL(vs.Page3,'') Page3, wv.Location,Source,c.OwnerID
	                 from(select contactid, VisitReference, max(visitedon) VisitedOn, sum(duration) Duration, count(visitreference) PageViews,ISNULL(Max(city +', ' + Region),'') Location,
	                 ISNULL(Max(ISPName),'') Source
		                from contactwebvisits cw  (nolock) 
		                where  isnull(VisitReference, '000') <> '000' and VisitedOn >= @startDate and VisitedOn <@endDate
		                group by visitreference, contactid
	                ) wv 
	                left join GET_VisiStat_Top3_Pages vs (nolock) on wv.VisitReference = vs.VisitReference 
	                left join contacts c (nolock) on wv.contactid = c.contactid 
	                left join GET_Contact_Email_Phone_Zip ep (nolock) on wv.ContactID = ep.contactid
	                where c.accountid = @accountId order by visitedon desc ";
                var visits = db.Get<WebVisitReport>(sql, new { accountId = accountId, startDate = startDate, endDate = endDate });
                return visits;
            }
        }

        /// <summary>
        /// Gets the web visitreport by web visit identifier.
        /// </summary>
        /// <param name="webVisitId">The web visit identifier.</param>
        /// <returns></returns>
       public WebVisitReport GetWebVisitreportByWebVisitId(int webVisitId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"select 
                        wv.ContactWebVisitID, wv.ContactId,wv.VisitReference,ISNULL(c.FirstName,'') FirstName, ISNULL(c.LastName,'') LastName,
                         ISNULL(ep.email,'') Email, ep.PhoneNumber Phone,ep.Zip Zip,c.lifecyclestage LifecycleStageId,c.LeadScore, VisitedOn, PageViews, 
                         Duration, vs.Page1, ISNULL(vs.Page2,'') Page2, ISNULL(vs.Page3,'') Page3, wv.Location,Source,c.OwnerID,  ep.LeadSource LeadSource
                          from (select ContactWebVisitID,contactid, VisitReference, max(visitedon) VisitedOn, sum(duration) Duration, count(visitreference) PageViews,ISNULL(Max(city +', ' + Region),'') Location,
                          ISNULL(Max(ISPName),'') Source 
                          from contactwebvisits cw (nolock) 
                          where  isnull(VisitReference, '000') <> '000' 
                          group by ContactWebVisitID,visitreference, contactid
                         ) wv 
                         left join GET_VisiStat_Top3_Pages vs (nolock) on wv.VisitReference = vs.VisitReference 
                         left join contacts c (nolock) on wv.contactid = c.contactid 
                         left join GET_Contact_Email_Phone_Zip_PrimaryLeadSource ep (nolock) on wv.ContactID = ep.contactid
                         where wv.contactwebvisitId=@contactwebvisitId ";
            var webVisitReport = db.Get<WebVisitReport>(sql, new { ContactWebVisitID = webVisitId }).FirstOrDefault();

            return webVisitReport;
        }


    }

    public class WebVisitContact
    {
        public int ContactID { get; set; }
        public string IPAddress { get; set; }
        public string ContactName { get; set; }
        public string ReferenceId { get; set; }
    }
}
