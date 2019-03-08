using AutoMapper;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using LinqKit;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class LeadAdaptersRepository : Repository<LeadAdapterAndAccountMap, int, LeadAdapterAndAccountMapDb>, ILeadAdaptersRepository
    {
        public LeadAdaptersRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory)
        {
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="LandmarkIT.Enterprise.Utilities.ExceptionHandling.UnsupportedOperationException">Mehod not implemented</exception>
        public IEnumerable<LeadAdapterAndAccountMap> FindAll()
        {
            throw new UnsupportedOperationException("Mehod not implemented");
        }

        /// <summary>
        /// Deletes the lead adapter.
        /// </summary>
        /// <param name="leadAdapterAndAccountMapId">The lead adapter and account map identifier.</param>
        public void DeleteLeadAdapter(int leadAdapterAndAccountMapId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var leadAdapterDb = db.LeadAdapters.Where(n => n.LeadAdapterAndAccountMapId == leadAdapterAndAccountMapId).FirstOrDefault();
                if (leadAdapterDb == null)
                    return;
                leadAdapterDb.IsDelete = true;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the lead adapters.
        /// </summary>
        /// <param name="accountID">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<LeadAdapterAndAccountMap> GetLeadAdapters(int accountID)
        {
            var db = ObjectContextFactory.Create();
            var leadAdaptersdb = db.LeadAdapters.Where(la => la.AccountID == accountID && la.IsDelete == false).ToList();
            if (leadAdaptersdb != null)
            {
                foreach (LeadAdapterAndAccountMapDb item in leadAdaptersdb)
                    yield return ConvertToDomain(item);
            }
        }

        public IEnumerable<LeadAdapterAndAccountMap> GetLeadData(string BuilderNumber, string CommunityNumber, LeadAdapterTypes leadAdapterType, IEnumerable<Guid> guids)
        {
            var db = ObjectContextFactory.Create();
            var matchedAccounts = db.LeadAdapters.Where(la => (la.BuilderNumber.ToLower().Contains(BuilderNumber)
                && la.CommunityNumber.ToLower().Contains(CommunityNumber))
                && la.IsDelete == false && la.LeadAdapterTypeID == (byte)leadAdapterType).ToList();
            if (matchedAccounts != null)
            {
                matchedAccounts = matchedAccounts.Where(w => guids.Contains(w.RequestGuid)).ToList();
                foreach (LeadAdapterAndAccountMapDb item in matchedAccounts)
                    yield return ConvertToDomain(item);
            }
        }

        public IEnumerable<LeadAdapterAndAccountMap> GetEmptyCommunities(string BuilderNumber, LeadAdapterTypes leadAdapterType, IEnumerable<Guid> guids)
        {
            var db = ObjectContextFactory.Create();
            var matchedAccounts = db.LeadAdapters.Where(la => (la.BuilderNumber.ToLower().Contains(BuilderNumber))
                && la.IsDelete == false && la.LeadAdapterTypeID == (byte)leadAdapterType).ToList();
            if (matchedAccounts != null)
            {
                matchedAccounts = matchedAccounts.Where(w => guids.Contains(w.RequestGuid)).ToList();
                foreach (LeadAdapterAndAccountMapDb item in matchedAccounts)
                    yield return ConvertToDomain(item);
            }
        }

        public bool IsCommunityExists(string CommunityNumber, LeadAdapterTypes leadAdapterType,int accountId)
        {
            var db = ObjectContextFactory.Create();
            bool isExists = db.LeadAdapters.Where(la => la.CommunityNumber.Contains(CommunityNumber) && la.IsDelete == false && la.LeadAdapterTypeID == (byte)leadAdapterType && la.AccountID == accountId).Count() > 0;
            return isExists; 
        }

        /// <summary>
        /// Gets all lead adapters.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<LeadAdapterAndAccountMap> GetAllLeadAdapters()
        {
            Logger.Current.Informational("GetAllLeadAdapters method in the LeadAdapterRepository is called");
            var db = ObjectContextFactory.Create();
            string sql = @"SELECT LAM.LeadAdapterAndAccountMapID AS LeadAdapterAndAccountMapId, LAM.AccountID AS AccountID, LAM.LeadAdapterTypeID  FROM LeadAdapterAndAccountMap (NOLOCK) LAM
                           JOIN Accounts (NOLOCK) A ON A.AccountID = LAM.AccountID
                           WHERE A.Status = 1 AND A.IsDeleted = 0 AND LAM.IsDelete = 0 AND LAM.LeadAdapterTypeID != 11";
            IEnumerable<LeadAdapterAndAccountMapDb> adapters = db.Get<LeadAdapterAndAccountMapDb>(sql);
            Logger.Current.Informational("Total lead adpaters count:  " + adapters.Count());
            if (adapters != null)
            {
                foreach (LeadAdapterAndAccountMapDb item in adapters)
                {
                    yield return ConvertToDomain(item);
                }
            }
        }

        public void UpdateProcessedDate(int accountMapId)
        {
            var db = ObjectContextFactory.Create();
            var accountMap = db.LeadAdapters.Where(w => w.LeadAdapterAndAccountMapId == accountMapId).FirstOrDefault();
            if (accountMap != null)
                accountMap.LastProcessed = DateTime.UtcNow;
            db.SaveChanges();
        }

        /// <summary>
        /// Gets all lead adapters.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<LeadAdapterAndAccountMap> GetImportLeads()
        {
            Logger.Current.Informational("GetAllLeadAdapters method in the LeadAdapterRepository is called");
            var db = ObjectContextFactory.Create();
            var leadAdaptersdb = db.LeadAdapters.Where(Id => Id.IsDelete == false && Id.LeadAdapterTypeID == (byte)LeadAdapterTypes.Import)
                .Join(db.LeadAdapterJobLogs, p => p.LeadAdapterAndAccountMapId, q => q.LeadAdapterAndAccountMapID, (p, q) => new { p, q.LeadAdapterJobStatusID })
                .Where(l => l.LeadAdapterJobStatusID == LeadAdapterJobStatus.Inprogress).Select(l => l.p);
            Logger.Current.Informational("Total lead adpaters count:  " + leadAdaptersdb.Count());
            if (leadAdaptersdb != null)
            {
                foreach (LeadAdapterAndAccountMapDb item in leadAdaptersdb)
                {
                    yield return ConvertToDomain(item);
                }
            }
        }

        /// <summary>
        /// Gets the lead adapter.
        /// </summary>
        /// <param name="accountID">The account identifier.</param>
        /// <param name="leadAdapterType">Type of the lead adapter.</param>
        /// <returns></returns>
        public LeadAdapterAndAccountMap GetLeadAdapter(int accountID, LeadAdapterTypes leadAdapterType)
        {
            var db = ObjectContextFactory.Create();
            var leadAdaptersdb = db.LeadAdapters.Where(la => la.AccountID == accountID && la.LeadAdapterTypeID == (byte)leadAdapterType).FirstOrDefault();
            if (leadAdaptersdb != null)
            {
                LeadAdapterAndAccountMap leadAdapterDatabaseConvertedToDomain = ConvertToDomain(leadAdaptersdb);
                return leadAdapterDatabaseConvertedToDomain;
            }
            return null;
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override LeadAdapterAndAccountMap FindBy(int id)
        {
            LeadAdapterAndAccountMapDb leadAdapterDataBase = getLeadAdapterDb(id);
            if (leadAdapterDataBase != null)
            {
                LeadAdapterAndAccountMap leadAdapterDatabaseConvertedToDomain = ConvertToDomain(leadAdapterDataBase);
                return leadAdapterDatabaseConvertedToDomain;
            }
            return null;
        }

        /// <summary>
        /// Gets the lead adapter job logs by.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="leadAdapterType">Type of the lead adapter.</param>
        /// <returns></returns>
        public DateTime? GetLeadAdapterJobLogsBy(int accountId, byte leadAdapterType)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var result = (from ljd in db.LeadAdapterJobLogs
                              join map in db.LeadAdapters on ljd.LeadAdapterAndAccountMapID equals map.LeadAdapterAndAccountMapId
                              where map.LeadAdapterTypeID == leadAdapterType && map.AccountID == accountId
                              orderby ljd.EndDate descending
                              select ljd).Take(1).FirstOrDefault();
                return result == null ? default(DateTime?) : result.EndDate;
            }
        }

        /// <summary>
        /// Gets the lead adapter database.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        LeadAdapterAndAccountMapDb getLeadAdapterDb(int id)
        {
            var db = ObjectContextFactory.Create();
            LeadAdapterAndAccountMapDb leadadapter = db.LeadAdapters.Include(c => c.Statuses).SingleOrDefault(c => c.LeadAdapterAndAccountMapId == id && c.IsDelete == false);
            if (leadadapter != null)
            {
                leadadapter.Tags = db.LeadAdapterTags.Include(c => c.Tag).Where(i => i.LeadAdapterID == id).ToList();
                if (leadadapter.LeadAdapterTypeID == (byte)LeadAdapterTypes.Facebook)
                    leadadapter.FacebookLeadAdapter = db.FacebookLeadAdapters.Where(w => w.LeadAdapterAndAccountMapID == leadadapter.LeadAdapterAndAccountMapId).FirstOrDefault();
            }
            return leadadapter;
        }

        /// <summary>
        /// Determines whether [is duplicate lead adapter] [the specified lead adapter type].
        /// </summary>
        /// <param name="leadAdapterType">Type of the lead adapter.</param>
        /// <param name="accountID">The account identifier.</param>
        /// <param name="leadAdapterID">The lead adapter identifier.</param>
        /// <returns></returns>
        public bool IsDuplicateLeadAdapter(byte leadAdapterType, int accountID, int leadAdapterID)
        {
            int count = 0;
            var db = ObjectContextFactory.Create();
            if (leadAdapterID != 0)
                count = db.LeadAdapters.Count(c => c.AccountID != accountID && c.LeadAdapterTypeID == leadAdapterType
                                                && c.LeadAdapterAndAccountMapId != leadAdapterID && c.IsDelete == false);
            else
                count = db.LeadAdapters.Count(c => c.AccountID != accountID && c.LeadAdapterTypeID == leadAdapterType
                                                && c.IsDelete == false);
            return count > 0;            
        }

        public bool IsDuplicateFacebookAdapter(int accountId, int leadadapterID, string name)
        {
            int count = 0;
            var db = ObjectContextFactory.Create();
            
            if (leadadapterID != 0)
            {
                var leadAdapter = db.LeadAdapters.Where(w => w.LeadAdapterAndAccountMapId != leadadapterID && w.IsDelete == false && w.LeadAdapterTypeID == (byte)LeadAdapterTypes.Facebook && w.AccountID == accountId)
                    .Select(s => s.LeadAdapterAndAccountMapId);
                if (leadAdapter != null)
                    count = db.FacebookLeadAdapters.Count(w => leadAdapter.Contains(w.LeadAdapterAndAccountMapID) && w.Name.ToLower() == name.ToLower());
            }
            else
            {
                var leadAdapter = db.LeadAdapters.Where(w => w.IsDelete == false && w.LeadAdapterTypeID == (byte)LeadAdapterTypes.Facebook && w.AccountID == accountId)
                    .Select(s => s.LeadAdapterAndAccountMapId);
                if (leadAdapter != null)
                    count = db.FacebookLeadAdapters.Count(w => leadAdapter.Contains(w.LeadAdapterAndAccountMapID) && w.Name.ToLower() == name.ToLower());
            }
            return count > 0;
        }

        public void InsertFacebookLeadAdapter(FacebookLeadAdapter fla)
        {
            if (fla != null)
            {
                var db = ObjectContextFactory.Create();
                var flaDb = Mapper.Map<FacebookLeadAdapter, FacebookLeadAdapterDb>(fla);
                db.FacebookLeadAdapters.Add(flaDb);
                db.SaveChanges();
            }
        }

        public void UpdateFacebookLeadAdapter(FacebookLeadAdapter fla)
        {
            if (fla != null)
            {
                var db = ObjectContextFactory.Create();
                var flaDb = Mapper.Map<FacebookLeadAdapter, FacebookLeadAdapterDb>(fla);
                db.Entry(flaDb).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public void InsertFacebookLeadGen(FacebookLeadGen fbLead)
        {
            if (fbLead != null)
            {
                var db = ObjectContextFactory.Create();
                FacebookLeadGenDb leadGen = Mapper.Map<FacebookLeadGen, FacebookLeadGenDb>(fbLead);
                leadGen.CreatedDate = DateTime.UtcNow;
                db.FacebookLeadGen.Add(leadGen);
                db.SaveChanges();
            }
        }

        public void UpdateFacebookLeadGen(FacebookLeadGen fbLead)
        {
            if (fbLead != null)
            {
                var db = ObjectContextFactory.Create();
                FacebookLeadGenDb leadGen = Mapper.Map<FacebookLeadGen, FacebookLeadGenDb>(fbLead);
                db.Entry(leadGen).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public void UpdateLeadAdapterStatus(int accountMapID, LeadAdapterErrorStatus status, LeadAdapterServiceStatus serviceStatus)
        {
            var db = ObjectContextFactory.Create();
            if (accountMapID != 0)
            {
                var leadAdapter = db.LeadAdapters.Where(w => w.LeadAdapterAndAccountMapId == accountMapID).FirstOrDefault();
                if (leadAdapter != null)
                {
                    leadAdapter.LeadAdapterErrorStatusID = (LeadAdapterErrorStatus)status;
                    leadAdapter.LeadAdapterServiceStatusID = (short)serviceStatus;
                    leadAdapter.LastProcessed = DateTime.UtcNow;
                }
                db.Entry(leadAdapter).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public IEnumerable<FacebookLeadGen> GetFacebookLeadGens(int accountMapID)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<FacebookLeadGen> fbLeadGens = new List<FacebookLeadGen>();
            FacebookLeadAdapterDb facebookLeadAdapter = db.FacebookLeadAdapters.Where(w => w.LeadAdapterAndAccountMapID == accountMapID).FirstOrDefault();
            if (facebookLeadAdapter != null)
            {
                var sql = @";WITH CTE AS
                            (
                               SELECT *, ROW_NUMBER() OVER (PARTITION BY leadgenid ORDER BY createddate DESC) AS Rank_Number
                               FROM FacebookLeadgen
                               where AdID = @AdID and IsProcessed = 0
                            )
                            SELECT * FROM CTE WHERE Rank_Number = 1";
                var newDb = ObjectContextFactory.Create();
                IEnumerable<FacebookLeadGenDb> leadGens = newDb.Get<FacebookLeadGenDb>(sql, new { AdID = facebookLeadAdapter.AddID });
                if (leadGens != null)
                    fbLeadGens = Mapper.Map<IEnumerable<FacebookLeadGenDb>, IEnumerable<FacebookLeadGen>>(leadGens);
                fbLeadGens.ForEach(f => { f.PageAccessToken = facebookLeadAdapter.PageAccessToken; });
            }
            return fbLeadGens;
        }

        public Account GetFacebookApp(int accountId)
        { 
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT FacebookAPPID, FacebookAPPSecret FROM Accounts(NOLOCK) Where AccountID = @accountId";
            var account = db.Get<AccountsDb>(sql, new { accountId = accountId }).FirstOrDefault();
            return Mapper.Map<AccountsDb, Account>(account);
        }

        public bool HasFacebookFields(int accountId)
        {
            var db = ObjectContextFactory.Create();
            return db.Fields.Where(w => w.AccountID == accountId && w.IsLeadAdapterField && w.LeadAdapterType == (byte)LeadAdapterTypes.Facebook).Any();
        }

        public void UpdateFacebookPageToken(string extendedToken, int accountMapID)
        {
            var db = ObjectContextFactory.Create();
            var fbLeadAdapter = db.FacebookLeadAdapters.Where(w => w.LeadAdapterAndAccountMapID == accountMapID).FirstOrDefault();
            if (fbLeadAdapter != null)
            {
                fbLeadAdapter.PageAccessToken = extendedToken;
                fbLeadAdapter.TokenUpdatedOn = DateTime.UtcNow;
            }
            db.Entry(fbLeadAdapter).State = EntityState.Modified;
            db.SaveChanges();

        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="db">The database.</param>
        /// <returns></returns>
        /// <exception cref="LandmarkIT.Enterprise.Utilities.ExceptionHandling.UnsupportedOperationException">Invalid lead adapter id has been passed. Suspected Id forgery.</exception>
        public override LeadAdapterAndAccountMapDb ConvertToDatabaseType(LeadAdapterAndAccountMap domainType, CRMDb db)
        {
            LeadAdapterAndAccountMapDb leadAdapterDb = default(LeadAdapterAndAccountMapDb);
            if (domainType.Id > 0)
            {
                leadAdapterDb = db.LeadAdapters.SingleOrDefault(Id => Id.LeadAdapterAndAccountMapId == domainType.Id && Id.IsDelete == false);
                if (leadAdapterDb == null)
                    throw new UnsupportedOperationException("Invalid lead adapter id has been passed. Suspected Id forgery.");
                else
                    leadAdapterDb = Mapper.Map<LeadAdapterAndAccountMap, LeadAdapterAndAccountMapDb>(domainType as LeadAdapterAndAccountMap, leadAdapterDb);
            }
            else
                leadAdapterDb = Mapper.Map<LeadAdapterAndAccountMap, LeadAdapterAndAccountMapDb>(domainType as LeadAdapterAndAccountMap);
            return leadAdapterDb;
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="leadAdaptersDb">The lead adapters database.</param>
        /// <returns></returns>
        public override LeadAdapterAndAccountMap ConvertToDomain(LeadAdapterAndAccountMapDb leadAdaptersDb)
        {
            LeadAdapterAndAccountMap leadAdapter = new LeadAdapterAndAccountMap();
            Mapper.Map<LeadAdapterAndAccountMapDb, LeadAdapterAndAccountMap>(leadAdaptersDb, leadAdapter);
            return leadAdapter;
        }

        /// <summary>
        /// Converts to lead adapterjob logs domain.
        /// </summary>
        /// <param name="leadAdapterJobLogsDb">The lead adapter job logs database.</param>
        /// <returns></returns>
        public LeadAdapterJobLogs ConvertToLeadAdapterjobLogsDomain(LeadAdapterJobLogsDb leadAdapterJobLogsDb)
        {
            LeadAdapterJobLogs leadAdapterJobLogs = new LeadAdapterJobLogs();
            Mapper.Map<LeadAdapterJobLogsDb, LeadAdapterJobLogs>(leadAdapterJobLogsDb, leadAdapterJobLogs);

            var db = ObjectContextFactory.Create();
            bool successRecords = db.LeadAdapterJobLogDetails.Where(i => i.LeadAdapterJobLogID == leadAdapterJobLogsDb.LeadAdapterJobLogID &&
                                                                    (i.LeadAdapterRecordStatusID == LeadAdapterRecordStatus.Added ||
                                                                    i.LeadAdapterRecordStatusID == LeadAdapterRecordStatus.Updated))
                                                                    .Count() > 0;

            bool failureRecords = db.LeadAdapterJobLogDetails.Where(i => i.LeadAdapterJobLogID == leadAdapterJobLogsDb.LeadAdapterJobLogID &&
                                                                    (i.LeadAdapterRecordStatusID == LeadAdapterRecordStatus.ValidationFailed ||                                                                   
                                                                    i.LeadAdapterRecordStatusID == LeadAdapterRecordStatus.BuilderNumberFailed ||
                                                                    i.LeadAdapterRecordStatusID == LeadAdapterRecordStatus.DuplicateFromFile ||
                                                                    i.LeadAdapterRecordStatusID == LeadAdapterRecordStatus.SystemFailure ||
                                                                    i.LeadAdapterRecordStatusID == LeadAdapterRecordStatus.CommunityNumberFailed ||
                                                                    i.LeadAdapterRecordStatusID == LeadAdapterRecordStatus.Duplicate))
                                                                    .Count() > 0;

            leadAdapterJobLogs.SuccessRecords = successRecords;
            leadAdapterJobLogs.FailureRecords = failureRecords;
            return leadAdapterJobLogs;
        }

        /// <summary>
        /// Determines whether [is duplicate lead adapter] [the specified lead adapter type].
        /// </summary>
        /// <param name="leadAdapterType">Type of the lead adapter.</param>
        /// <param name="accountID">The account identifier.</param>
        /// <param name="leadAdapterAndAccountMapId">The lead adapter and account map identifier.</param>
        /// <returns></returns>
        public bool IsDuplicateLeadAdapter(LeadAdapterTypes leadAdapterType, int accountID, int leadAdapterAndAccountMapId)
        {
            var db = ObjectContextFactory.Create();
            LeadAdapterAndAccountMapDb leadAdapterDb;
            if (leadAdapterAndAccountMapId == 0)
                leadAdapterDb = db.LeadAdapters.Where(la => la.AccountID == accountID
                                                         && la.LeadAdapterTypeID == (int)leadAdapterType
                                                         && la.IsDelete == false)
                                                .FirstOrDefault();
            else
                leadAdapterDb = db.LeadAdapters.Where(la => la.AccountID == accountID
                                                         && la.LeadAdapterTypeID == (int)leadAdapterType
                                                         && la.LeadAdapterAndAccountMapId != leadAdapterAndAccountMapId
                                                         && la.IsDelete == false)
                                                .FirstOrDefault();
            return (leadAdapterDb == null) ? false : true;                
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="context">The context.</param>
        public override void PersistValueObjects(LeadAdapterAndAccountMap domainType, LeadAdapterAndAccountMapDb dbType, CRMDb context)
        {
            PersistLeadAdapterTags(domainType, dbType, context);
        }

        /// <summary>
        /// Persists the lead adapter tags.
        /// </summary>
        /// <param name="leadadapter">The leadadapter.</param>
        /// <param name="leadadapterDb">The leadadapter database.</param>
        /// <param name="db">The database.</param>
        private void PersistLeadAdapterTags(LeadAdapterAndAccountMap leadadapter, LeadAdapterAndAccountMapDb leadadapterDb, CRMDb db)
        {
            var leadadapterTags = db.LeadAdapterTags.Where(a => a.LeadAdapterID == leadadapter.Id);
            if (leadadapter.Tags != null)
            {
                foreach (Tag tag in leadadapter.Tags)
                {
                    var tagexist = db.Tags.Where(p => p.TagID == tag.Id && p.AccountID == tag.AccountID && p.IsDeleted != true).FirstOrDefault();
                    if (tag.Id == 0 || tagexist == null)
                    {
                        var tagDb = db.Tags.SingleOrDefault(t => t.TagName.Equals(tag.TagName) && t.AccountID.Equals(tag.AccountID) && t.IsDeleted != true);
                        if (tagDb == null)
                        {
                            tagDb = Mapper.Map<Tag, TagsDb>(tag);
                            tagDb.IsDeleted = false;
                            tagDb = db.Tags.Add(tagDb);
                        }
                        var leadadapterTag = new LeadAdapterTagMapDb()
                        {
                            LeadAdapter = leadadapterDb,
                            Tag = tagDb
                        };

                        db.LeadAdapterTags.Add(leadadapterTag);
                    }
                    else if (leadadapterTags.Count(a => a.TagID == tag.Id) == 0)
                    {
                        db.LeadAdapterTags.Add(new LeadAdapterTagMapDb() { LeadAdapterID = leadadapter.Id, TagID = tag.Id });
                        db.RefreshAnalytics.Add(new RefreshAnalyticsDb() { EntityID = tag.Id, EntityType = 5, Status = 1, LastModifiedOn = DateTime.Now.ToUniversalTime() });
                    }
                }
                IList<int> tagIds = leadadapter.Tags.Where(a => a.Id > 0).Select(a => a.Id).ToList();
                var unMapLeadAdapterTags = leadadapterTags.Where(a => !tagIds.Contains(a.TagID));
                this.ScheduleAnalyticsRefreshForTags(unMapLeadAdapterTags, db);
                db.LeadAdapterTags.RemoveRange(unMapLeadAdapterTags);
            }
        }

        private void ScheduleAnalyticsRefreshForTags(IEnumerable<LeadAdapterTagMapDb> tags, CRMDb db)
        {
            List<RefreshAnalyticsDb> analytics = new List<RefreshAnalyticsDb>();
            if (tags.IsAny())
            {
                foreach (var tag in tags)
                {
                    RefreshAnalyticsDb refreshAnalytics = new RefreshAnalyticsDb();
                    refreshAnalytics.EntityID = tag.TagID;
                    refreshAnalytics.EntityType = 5;
                    refreshAnalytics.Status = 1;
                    refreshAnalytics.LastModifiedOn = DateTime.Now.ToUniversalTime();
                    analytics.Add(refreshAnalytics);
                }
                db.RefreshAnalytics.AddRange(analytics);
            }
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="accountID">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<LeadAdapterAndAccountMap> FindAll(string name, int limit, int pageNumber, int accountID)
        {
            var records = (pageNumber - 1) * limit;
            string query = @"SELECT LACM.LeadAdapterAndAccountMapID, LACM.LeadAdapterTypeID, CreatedDateTime, LastProcessed, LT.Name AS LeadAdapterType, 
                           S.[Description] AS ServiceStatusMessage, LES.LeadAdapterErrorStatus AS LeadAdapterErrorName, ISNULL(FL.Name, '') AS FacebookLeadAdapterName,COUNT(1) OVER() as TotalCount
                           FROM LeadAdapterAndAccountMap (NOLOCK) LACM
                           INNER JOIN LeadAdapterTypes (NOLOCK) LT ON LT.LeadAdapterTypeID = LACM.LeadAdapterTypeID
                           LEFT JOIN Statuses (NOLOCK) S ON S.StatusID = LACM.LeadAdapterServiceStatusID
                           LEFT JOIN LeadAdapterErrorStatus (NOLOCK) LES ON LES.LeadAdapterErrorStatusID = LACM.LeadAdapterErrorStatusID
                           LEFT JOIN FacebookLeadAdapter(NOLOCK) FL ON FL.LeadAdapterAndAccountMapID = LACM.LeadAdapterAndAccountMapID
                           WHERE AccountID = @accountid AND IsDelete = 0 AND LACM.LeadAdapterTypeID != 11
                           ORDER BY CreatedDateTime DESC";
            IEnumerable<LeadAdapterAndAccountMapDb> leadadaptersDb = ObjectContextFactory.Create().Get<LeadAdapterAndAccountMapDb>(query, new { accountid = accountID }, false);
            if (!string.IsNullOrEmpty(name))
                leadadaptersDb = leadadaptersDb.Where(w => w.LeadAdapterType.ToLower().Contains(name.ToLower()) || w.FacebookLeadAdapterName.ToLower().Contains(name.ToLower()));

            leadadaptersDb = leadadaptersDb.AsQueryable().Skip(records).Take(limit).ToList();
            foreach (LeadAdapterAndAccountMapDb da in leadadaptersDb)
            {
                yield return ConvertToDomain(da);
            }
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="accountID">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<LeadAdapterAndAccountMap> FindAll(string name, int accountID)
        {
            var predicate = PredicateBuilder.True<LeadAdapterAndAccountMapDb>();
            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                predicate = predicate.And(a => a.LeadAdapterTypes.Name.Contains(name));
            }
            predicate = predicate.And(a => a.AccountID == accountID);
            predicate = predicate.And(a => a.LeadAdapterTypeID != (byte)LeadAdapterTypes.Import);
            predicate = predicate.And(a => a.IsDelete == false);

            IEnumerable<LeadAdapterAndAccountMapDb> leadadapters = findLeadadaptersSummary(predicate);
            foreach (LeadAdapterAndAccountMapDb da in leadadapters)
            {
                yield return ConvertToDomain(da);
            }
        }

        /// <summary>
        /// Finds the lead adapter job log all.
        /// </summary>
        /// <param name="limit">The limit.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="leadAdapterAndAccountMapID">The lead adapter and account map identifier.</param>
        /// <returns></returns>
        public IEnumerable<LeadAdapterJobLogs> FindLeadAdapterJobLogAll(int limit, int pageNumber, int leadAdapterAndAccountMapID)
        {
            var predicate = PredicateBuilder.True<LeadAdapterJobLogsDb>();
            var records = (pageNumber - 1) * limit;
            predicate = predicate.And(a => a.LeadAdapterAndAccountMapID == leadAdapterAndAccountMapID);
            predicate = predicate.And(a => a.LeadAdapterJobStatusID != LeadAdapterJobStatus.Inprogress);
            IEnumerable<LeadAdapterJobLogsDb> leadadapters = findLeadAdapterJobLogsSummary(predicate).Skip(records).Take(limit);
            foreach (LeadAdapterJobLogsDb da in leadadapters)
            {
                yield return ConvertToLeadAdapterjobLogsDomain(da);
            }
        }

        /// <summary>
        /// Finds the lead adapter job log all.
        /// </summary>
        /// <param name="leadAdapterAndAccountMapID">The lead adapter and account map identifier.</param>
        /// <returns></returns>
        public IEnumerable<LeadAdapterJobLogs> FindLeadAdapterJobLogAll(int leadAdapterAndAccountMapID)
        {
            var predicate = PredicateBuilder.True<LeadAdapterJobLogsDb>();
            predicate = predicate.And(a => a.LeadAdapterAndAccountMapID == leadAdapterAndAccountMapID);
            predicate = predicate.And(a => a.LeadAdapterJobStatusID != LeadAdapterJobStatus.Inprogress);
            IEnumerable<LeadAdapterJobLogsDb> leadadapters = findLeadAdapterJobLogsSummary(predicate);
            foreach (LeadAdapterJobLogsDb da in leadadapters)
            {
                yield return ConvertToLeadAdapterjobLogsDomain(da);
            }
        }

        /// <summary>
        /// Finds the leadadapters summary.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        IEnumerable<LeadAdapterAndAccountMapDb> findLeadadaptersSummary(System.Linq.Expressions.Expression<Func<LeadAdapterAndAccountMapDb, bool>> predicate)
        {

            IEnumerable<LeadAdapterAndAccountMapDb> leadAdapters = ObjectContextFactory.Create().LeadAdapters.Include(x => x.LeadAdapterErrorStatus)
                .AsExpandable()
                .Where(predicate).OrderByDescending(c => c.AccountID).Select(a =>
                    new
                    {
                        AccountID = a.AccountID,
                        LeadAdapterTypes = a.LeadAdapterTypes,
                        LeadAdapterID = a.LeadAdapterAndAccountMapId,
                        LeadAdapterTypeID = a.LeadAdapterTypeID,
                        CreatedDate = a.CreatedDateTime,
                        LeadAdapterError = a.LeadAdapterErrorStatus,
                        LastProcessed = a.LastProcessed,
                        Statuses = a.Statuses
                    }).ToList().Select(x => new LeadAdapterAndAccountMapDb
                    {
                        AccountID = x.AccountID,
                        LeadAdapterAndAccountMapId = x.LeadAdapterID,
                        LeadAdapterTypeID = x.LeadAdapterTypeID,
                        CreatedDateTime = x.CreatedDate,
                        LeadAdapterErrorStatus = x.LeadAdapterError,
                        LastProcessed = x.LastProcessed,
                        Statuses = x.Statuses
                    }).OrderByDescending(i => i.CreatedDateTime);
            return leadAdapters;
        }

        /// <summary>
        /// Finds the lead adapter job logs summary.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        IEnumerable<LeadAdapterJobLogsDb> findLeadAdapterJobLogsSummary(System.Linq.Expressions.Expression<Func<LeadAdapterJobLogsDb, bool>> predicate)
        {
            IEnumerable<LeadAdapterJobLogsDb> leadAdapterJobLogs = ObjectContextFactory.Create().LeadAdapterJobLogs
                .AsExpandable()
                .Where(predicate).OrderByDescending(c => c.LeadAdapterAndAccountMapID).Select(a =>
                    new
                    {
                        LeadAdapterAndAccountMapID = a.LeadAdapterAndAccountMapID,
                        LeadAdapterJobLogID = a.LeadAdapterJobLogID,
                        CreatedDateTime = a.CreatedDateTime,
                        FileName = a.FileName,
                        Remarks = a.Remarks,
                        LeadAdapterJobStatusID = a.LeadAdapterJobStatusID

                    }).ToList().Select(x => new LeadAdapterJobLogsDb
                    {
                        LeadAdapterAndAccountMapID = x.LeadAdapterAndAccountMapID,
                        LeadAdapterJobLogID = x.LeadAdapterJobLogID,
                        CreatedDateTime = x.CreatedDateTime,
                        FileName = x.FileName,
                        Remarks = x.Remarks,
                        LeadAdapterJobStatusID = x.LeadAdapterJobStatusID
                    }).OrderByDescending(i => i.CreatedDateTime);
            return leadAdapterJobLogs;
        }

        /// <summary>
        /// Creates the lead adapter folders.
        /// </summary>
        /// <param name="accountID">The account identifier.</param>
        /// <param name="leadAdapterBaseDirectory">The lead adapter base directory.</param>
        public void CreateLeadAdapterFolders(string accountID, string leadAdapterBaseDirectory)
        {
            string accountiddirectory = System.IO.Path.Combine(leadAdapterBaseDirectory, accountID);
            System.IO.Directory.CreateDirectory(accountiddirectory);
            string[] allLeadAdapters = Enum.GetNames(typeof(LeadAdapterTypes));
            foreach (var leadadapter in allLeadAdapters)
            {
                string leadadapterdirectory = System.IO.Path.Combine(accountiddirectory, leadadapter);
                System.IO.Directory.CreateDirectory(leadadapterdirectory);
                string leadadapterlocaldirectory = System.IO.Path.Combine(leadadapterdirectory, "Local");
                string leadadapterarchivedirectory = System.IO.Path.Combine(leadadapterdirectory, "Archive");
                System.IO.Directory.CreateDirectory(leadadapterlocaldirectory);
                System.IO.Directory.CreateDirectory(leadadapterarchivedirectory);
            }
        }

        /// <summary>
        /// Gets the lead adapter by identifier.
        /// </summary>
        /// <param name="leadAdapterAndAccountMapID">The lead adapter and account map identifier.</param>
        /// <returns></returns>
        public LeadAdapterAndAccountMap GetLeadAdapterByID(int leadAdapterAndAccountMapID)
        {
            var db = ObjectContextFactory.Create();
            LeadAdapterAndAccountMap accountmap = new LeadAdapterAndAccountMap();
            //var leadAdaptersdb = db.LeadAdapters.Include(p=>p.Account).Where(la => la.LeadAdapterAndAccountMapId == leadAdapterAndAccountMapID).FirstOrDefault();

            string sql = @"SELECT LAM.*, A.AccountName FROM LeadAdapterAndAccountMap (NOLOCK) LAM 
                            JOIN Accounts (NOLOCK) A ON A.AccountID = LAM.AccountID
                            WHERE LAM.LeadAdapterAndAccountMapID = @Id";
            var leadAdaptersdb = db.Get<LeadAdapterAndAccountMapDb>(sql, new { Id = leadAdapterAndAccountMapID }).FirstOrDefault();
            if (leadAdaptersdb != null)
            {
                return ConvertToDomain(leadAdaptersdb);
            }
            return accountmap;
        }

        /// <summary>
        /// Gets the dropdown value identifier.
        /// </summary>
        /// <param name="AccountID">The account identifier.</param>
        /// <param name="DropdownValueTypeID">The dropdown value type identifier.</param>
        /// <returns></returns>
        public short GetDropdownValueID(int AccountID, DropdownValueTypes DropdownValueTypeID)
        {
            var db = ObjectContextFactory.Create();
            return db.DropdownValues.Where(i => i.DropdownID == (byte)DropdownFieldTypes.PhoneNumberType
                                             && i.AccountID == AccountID
                                             && i.DropdownValueTypeID == (short)DropdownValueTypeID)
                                    .Select(x => x.DropdownValueID)
                                    .SingleOrDefault();
        }

        /// <summary>
        /// Determines whether [is linked to workflows] [the specified lead adapter identifier].
        /// </summary>
        /// <param name="LeadAdapterID">The lead adapter identifier.</param>
        /// <returns></returns>
        public bool isLinkedToWorkflows(int LeadAdapterID)
        {
            using (var db = ObjectContextFactory.Create())
            {
                return db.WorkflowTriggers.Where(i => i.LeadAdapterID == LeadAdapterID)
                         .Join(db.Workflows.Where(i => i.IsDeleted != true),
                          i => i.WorkflowID, j => j.WorkflowID,
                          (i, j) => new
                          {
                              i.WorkflowID
                          }).Count() > 0;
            }
        }

        /// <summary>
        /// Gets the lead adapter submitted data by identifier.
        /// </summary>
        /// <param name="JobLogDetailID">The job log detail identifier.</param>
        /// <returns></returns>
        public string GetLeadAdapterSubmittedDataByID(int JobLogDetailID)
        {
            var db = ObjectContextFactory.Create();
            return db.LeadAdapterJobLogDetails.Include(x => x.LeadAdapterJobLogs.LeadAdapter)
                                                           .Where(i => i.LeadAdapterJobLogDetailID == JobLogDetailID)
                                                           .Select(x => x.SubmittedData).FirstOrDefault();
        }

        /// <summary>
        /// Determines whether [is lead adapter already configured] [the specified account identifier].
        /// </summary>
        /// <param name="AccountID">The account identifier.</param>
        /// <param name="leadAdapterType">Type of the lead adapter.</param>
        /// <returns></returns>
        public bool isLeadAdapterAlreadyConfigured(int AccountID, LeadAdapterTypes leadAdapterType)
        {
            using (var db = ObjectContextFactory.Create()) {
                return db.LeadAdapters.Where(i => i.AccountID == AccountID && i.LeadAdapterTypeID == (byte)leadAdapterType && i.IsDelete == false).Count() > 0;
            }
        }

        public int ImpotedLeadByJobId(int jobId)
        {
            var db = ObjectContextFactory.Create();
            int importedBy = db.LeadAdapterJobLogs.Where(l => l.LeadAdapterJobLogID == jobId).Join(db.Users.Where(w => w.AccountID != 1), l => l.CreatedBy, u => u.UserID, (l, u) => new { CreatedBy = l.CreatedBy }).Select(s => s.CreatedBy).FirstOrDefault();
            //if (importedBy == 0)
            //    importedBy = db.Users.Where(w => w.AccountID == 1 && w.FirstName.Equals("SmartTouch CRM", StringComparison.InvariantCultureIgnoreCase)).Select(s => s.UserID).FirstOrDefault();
            return importedBy;
        }

        public byte GetLeadAdapterTypeByJobId(int jobId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT LAM.LeadAdapterTypeID FROM LeadAdapterAndAccountMap (NOLOCK) LAM
                        INNER JOIN LeadAdapterJobLogs (NOLOCK) LJL  ON  LJL.LeadAdapterAndAccountMapID = LAM.LeadAdapterAndAccountMapID
                        WHERE LJL.LeadAdapterJobLogID=@jobId";
            byte leadAdapterType = db.Get<byte>(sql, new { jobId = jobId }).FirstOrDefault();
            return leadAdapterType;
        }

        /// <summary>
        /// Getting All Leadadapter Types.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<LeadAdapterType> GetAllLeadadapterTypes()
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"SELECT LeadAdapterTypeID,Name FROM LeadAdapterTypes (NOLOCK)";
                return db.Get<LeadAdapterType>(sql, new { }).ToList();
            }
        }
    }
}
