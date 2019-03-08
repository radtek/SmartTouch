using AutoMapper;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.Domain.Users;
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
    public class UserActivitiesRepository : Repository<UserActivityLog, int, UserActivityLogsDb>, IUserActivitiesRepository
    {
        public UserActivitiesRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory) { }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override UserActivityLog FindBy(int id)
        {            
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override UserActivityLogsDb ConvertToDatabaseType(UserActivityLog domainType, CRMDb context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="databaseType">Type of the database.</param>
        /// <returns></returns>
        public override UserActivityLog ConvertToDomain(UserActivityLogsDb databaseType)
        {
            return Mapper.Map<UserActivityLogsDb, UserActivityLog>(databaseType);
        }

        /// <summary>
        /// Converts to domain user activity.
        /// </summary>
        /// <param name="databaseType">Type of the database.</param>
        /// <returns></returns>
        public IEnumerable<UserActivityLog> ConvertToDomainUserActivity(IEnumerable<UserActivityLogsDb> databaseType)
        {
            return Mapper.Map<IEnumerable<UserActivityLogsDb>, IEnumerable<UserActivityLog>>(databaseType);
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="context">The context.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void PersistValueObjects(UserActivityLog domainType, UserActivityLogsDb dbType, CRMDb context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<UserActivityLog> FindAll()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="moduleids">The moduleids.</param>
        /// <returns></returns>
        public IEnumerable<UserActivityLog> FindAll(int userId, int pageNumber, int accountId, int[] moduleids)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<int> modules = (moduleids == null || (moduleids != null && !moduleids.Any())) ? new int[] { 1, 2, 3, 4, 5, 6, 7, 9, 10, 16, 24, 31, 33 } : moduleids;
            var skip = (pageNumber - 1) * 20;
            var take = 20;

            DateTime toDate = DateTime.UtcNow.AddDays(-5 * (pageNumber - 1));
            DateTime fromDate = DateTime.UtcNow.AddDays(-5 * pageNumber);
            IEnumerable<UserActivityLogsDb> logs = new List<UserActivityLogsDb>();
//            string logsSql = @"SELECT * FROM UserActivityLogs (NOLOCK) WHERE UserID = @userId AND AccountID = @accountId AND ModuleID IN @ModuleIds 
//                             ORDER BY LogDate DESC OFFSET @skip ROWS
//                             FETCH NEXT @take ROWS ONLY";
//            IEnumerable<UserActivityLogsDb> logs = db.Get<UserActivityLogsDb>(logsSql, new { userId = userId, accountId = accountId, ModuleIds = modules, skip = skip, take = take });


            db.QueryStoredProc("[dbo].[GetUserActivities]", (reader) =>
            {
                logs = reader.Read<UserActivityLogsDb>();
            }, new
            {
                UserID = userId,
                AccountID = accountId,
                ModuleIds = modules.AsTableValuedParameter("dbo.Contact_List"),
                Skip = skip,
                Take = take
            });

            if (logs != null)
            {
                foreach (UserActivityLogsDb log in logs)
                {
                    yield return Mapper.Map<UserActivityLogsDb, UserActivityLog>(log);
                }
            }
        }

        ///// <summary>
        ///// Gets the user activities.
        ///// </summary>
        ///// <param name="userIds">The user ids.</param>
        ///// <param name="modules">The modules.</param>
        ///// <param name="accountId">The account identifier.</param>
        ///// <param name="fromDate">From date.</param>
        ///// <param name="toDate">To date.</param>
        ///// <returns></returns>
        //public IEnumerable<UserActivityList> GetUserActivities(int[] userIds, int[] modules, int accountId, DateTime fromDate, DateTime toDate)
        //{
        //    var db = ObjectContextFactory.Create();

        //    string userids = string.Join(",", userIds);
        //    string moduleids = string.Join(",", modules);

        //    List<UserActivityListDb> activities = db.Users.Where(i => i.AccountID == accountId && i.IsDeleted == false && userIds.Contains(i.UserID))
        //        .GroupJoin(db.UserActivitiesLog.Where(y => y.UserID != null && modules.Contains(y.ModuleID)
        //            && y.UserActivityID == (byte)UserActivityType.Create && y.LogDate >= fromDate.Date && y.LogDate <= toDate.Date),
        //            u => u.UserID, ual => ual.UserID,
        //            (u, ual) => new { user = u, useractivities = ual }).ToList()
        //            .Select(r => new UserActivityListDb
        //                {
        //                    OwnerId = r.user.UserID,
        //                    OwnerName = r.user.FirstName + " " + r.user.LastName,
        //                    TotalModuleCount = r.useractivities.Select(c => c.ModuleID).Count(),

        //                    UserModules = r.useractivities.GroupBy(x => x.ModuleID).Select(so => new UserModulesDb
        //                    {
        //                        ModuleCount = so.Count(),
        //                        UserModuleId = so.Key
        //                    }).ToList()
        //                }).ToList();
        //    //Group join -> outer join
        //    //Join -> Inner join

        //    IList<UserActivityList> userActivityList = new List<UserActivityList>();
        //    foreach (var activity in activities)
        //    {
        //        var logDomain = ConvertToDomainUserActivity(activity);
        //        userActivityList.Add(logDomain);
        //    }

        //    return userActivityList;
        //}

        /// <summary>
        /// Converts to domain user activity.
        /// </summary>
        /// <param name="userActivitiesList">The user activities list.</param>
        /// <returns></returns>
        private UserActivityList ConvertToDomainUserActivity(UserActivityListDb userActivitiesList)
        {
            return Mapper.Map<UserActivityListDb, UserActivityList>(userActivitiesList);
        }

        /// <summary>
        /// Gets the user modules.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="moduleId">The module identifier.</param>
        /// <returns></returns>
        public IEnumerable<int> GetUserModules(int userId, int moduleId)
        {
            var modules = ObjectContextFactory.Create().UserActivitiesLog
                .Where(u => u.UserID == userId && u.ModuleID == moduleId && u.UserActivityID == 1).Select(o => (int)o.ModuleID).ToList();
            return modules;
        }

        /// <summary>
        /// Converts to user activity.
        /// </summary>
        /// <param name="useractivitieslist">The useractivitieslist.</param>
        /// <returns></returns>
        private IEnumerable<UserActivityLog> convertToUserActivity(IEnumerable<UserActivityLogsDb> useractivitieslist)
        {
            foreach (UserActivityLogsDb userActivity in useractivitieslist)
            {
                yield return new UserActivityLog() { ModuleID = userActivity.ModuleID, UserActivityID = userActivity.UserActivityID };
            }
        }

        /// <summary>
        /// Gets the user activity entity details.
        /// </summary>
        /// <param name="moduleId">The module identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns></returns>
        public UserActivityEntityDetail GetUserActivityEntityDetails(byte moduleId, int entityId,int accountId)
        {
            UserActivityEntityDetail entityDetail;
            var db = ObjectContextFactory.Create();
            if (moduleId == (byte)AppModules.Contacts)
            {
                entityDetail = db.Contacts.Where(c => c.ContactID == entityId && c.IsDeleted == false && c.AccountID == accountId).Select(s => new UserActivityEntityDetail()
                {
                    EntityId = s.ContactID,
                    EntityName = (s.ContactType == ContactType.Person) ? (s.FirstName + " " + s.LastName) : s.Company,
                    ContactType = (byte)s.ContactType
                }).FirstOrDefault();

                if (entityDetail == null)
                {
                    entityDetail = db.Contacts.Where(c => c.ContactID == entityId && c.IsDeleted == true && c.AccountID == accountId).Select(s => new UserActivityEntityDetail()
                    {
                        // EntityId = s.ContactID,
                        EntityName = (s.ContactType == ContactType.Person) ? (s.FirstName + " " + s.LastName) : s.Company,
                        //  ContactType = (byte)s.ContactType
                    }).FirstOrDefault();
                }
            }
            else if (moduleId == (byte)AppModules.Opportunity)
            {
                entityDetail = db.Opportunities.Where(o => o.OpportunityID == entityId && (o.IsDeleted == false || o.IsDeleted == null)).Select(s => new UserActivityEntityDetail()
                {
                    EntityId = s.OpportunityID,
                    EntityName = s.OpportunityName,
                }).FirstOrDefault();

                if (entityDetail == null)
                {
                    entityDetail = db.UserActivitiesLog.Where(f => f.EntityID == entityId && f.AccountID == accountId && f.ModuleID == moduleId).Select(s => new UserActivityEntityDetail()
                    {
                        EntityId = 0,
                        EntityName = s.EntityName,
                    }).FirstOrDefault();
                }
            }
            else if (moduleId == (byte)AppModules.Campaigns)
            {
                entityDetail = db.Campaigns.Where(ca => ca.CampaignID == entityId && ca.IsDeleted == false).Select(s => new UserActivityEntityDetail()
                {
                    EntityId = s.CampaignID,
                    EntityName = s.Name,
                }).FirstOrDefault();

                if (entityDetail == null)
                {
                    entityDetail = db.UserActivitiesLog.Where(f => f.EntityID == entityId && f.AccountID == accountId && f.ModuleID == moduleId).Select(s => new UserActivityEntityDetail()
                    {
                        EntityId = 0,
                        EntityName = s.EntityName,
                    }).FirstOrDefault();
                }
            }
            else if (moduleId == (byte)AppModules.Forms)
            {
                entityDetail = db.Forms.Where(f => f.FormID == entityId && f.IsDeleted == false).Select(s => new UserActivityEntityDetail()
                {
                    EntityId = s.FormID,
                    EntityName = s.Name,
                }).FirstOrDefault();
                if (entityDetail == null)
                {
                    entityDetail = db.UserActivitiesLog.Where(f => f.EntityID == entityId && f.AccountID == accountId && f.ModuleID == moduleId).Select(s => new UserActivityEntityDetail()
                    {
                        EntityId = 0,
                        EntityName = s.EntityName,
                    }).FirstOrDefault();
                }
            }
            else if (moduleId == (byte)AppModules.ContactActions)
            {
                entityDetail = db.Actions.Where(f => f.ActionID == entityId).Select(s => new UserActivityEntityDetail()
                {
                    EntityId = s.ActionID,
                    EntityName = s.ActionDetails,
                }).FirstOrDefault();
            }
            else if (moduleId == (byte)AppModules.ContactNotes)
            {
                entityDetail = db.Notes.Where(f => f.NoteID == entityId && f.AccountID == accountId).Select(s => new UserActivityEntityDetail()
                {
                    EntityId = s.NoteID,
                    EntityName = s.NoteDetails,
                }).FirstOrDefault();
            }
            else if (moduleId == (byte)AppModules.Tags)
            {
                entityDetail = db.Tags.Where(f => f.TagID == entityId && f.IsDeleted != true).Select(s => new UserActivityEntityDetail()
                {
                    EntityId = s.TagID,
                    EntityName = s.TagName,
                }).FirstOrDefault();
            }
            else if (moduleId == (byte)AppModules.AdvancedSearch)
            {
                entityDetail = db.SearchDefinitions.Where(f => f.SearchDefinitionID == entityId).Select(s => new UserActivityEntityDetail()
                {
                    EntityId = s.SearchDefinitionID,
                    EntityName = s.SearchDefinitionName,
                }).FirstOrDefault();

                if (entityDetail == null)
                {
                    entityDetail = db.UserActivitiesLog.Where(f => f.EntityID == entityId && f.AccountID == accountId && f.ModuleID == moduleId).Select(s => new UserActivityEntityDetail()
                    {
                        EntityId = 0,
                        EntityName = s.EntityName,
                    }).FirstOrDefault();
                }
            }

            else if (moduleId == (byte)AppModules.Users)
            {
                entityDetail = db.Users.Where(f => f.UserID == entityId && f.IsDeleted == false).Select(s => new UserActivityEntityDetail()
                {
                    EntityId = s.UserID,
                    EntityName = s.FirstName + " " + s.LastName,
                }).FirstOrDefault();
                if (entityDetail == null)
                {
                    entityDetail = db.Users.Where(f => f.UserID == entityId && f.IsDeleted == true).Select(s => new UserActivityEntityDetail()
                    {
                        //EntityId = s.UserID,
                        EntityName = s.FirstName + " " + s.LastName,
                    }).FirstOrDefault();
                }
            }

            else if (moduleId == (byte)AppModules.Accounts)
            {
                entityDetail = db.Accounts.Where(f => f.AccountID == entityId && f.IsDeleted == false).Select(s => new UserActivityEntityDetail()
                {
                    EntityId = s.AccountID,
                    EntityName = s.FirstName + " " + s.LastName,
                }).FirstOrDefault();
                if (entityDetail == null)
                {
                    entityDetail = db.Accounts.Where(f => f.AccountID == entityId && f.IsDeleted == true).Select(s => new UserActivityEntityDetail()
                    {

                        EntityName = s.FirstName + " " + s.LastName,
                    }).FirstOrDefault();
                }
            }

            else if (moduleId == (byte)AppModules.Reports)
            {
                entityDetail = db.Reports.Where(r => r.ReportID == entityId).Select(s => new UserActivityEntityDetail()
                {
                    EntityId = s.ReportID,
                    EntityName = s.ReportName,
                    ReportType = s.ReportType
                }).FirstOrDefault();
            }

            else if (moduleId == (byte)AppModules.Automation)
            {
                entityDetail = db.Workflows.Where(w => w.WorkflowID == entityId).Select(w => new UserActivityEntityDetail()
                {
                    EntityId = w.IsDeleted ? 0 : w.WorkflowID,
                    EntityName = w.WorkflowName,
                }).FirstOrDefault();
            }

            else
                entityDetail = null;
            return entityDetail;
        }

        /// <summary>
        /// Deletes the activity.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="activityLogId">The activity log identifier.</param>
        public void DeleteActivity(int userId, int activityLogId, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var userActivities = ObjectContextFactory.Create().UserActivitiesLog.Where(a => a.UserID == userId && a.AccountID == accountId && a.UserActivityLogID == activityLogId).FirstOrDefault();
            db.UserActivitiesLog.Remove(userActivities);
            db.SaveChanges();
        }

        /// <summary>
        /// Inserts the contact read activity.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="entityName">Name of the entity.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="module">The module.</param>
        /// <param name="activityName">Name of the activity.</param>
        /// <param name="accountId">The account identifier.</param>
        public void InsertContactReadActivity(int entityId, string entityName, int userId, AppModules module, UserActivityType activityName, int accountId)
        {
            var db = ObjectContextFactory.Create();
            UserActivityLogsDb userActivityLog = new UserActivityLogsDb()
            {
                EntityID = entityId,
                EntityName = entityName,
                LogDate = DateTime.Now.ToUniversalTime(),
                ModuleID = (byte)module,
                UserActivityID = (byte)activityName,
                UserID = userId,
                AccountID = accountId
            };
            db.UserActivitiesLog.Add(userActivityLog);
            db.SaveChanges();
        }

        /// <summary>
        /// Inserts the change owner activity.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="module">The module.</param>
        /// <param name="activityType">Type of the activity.</param>
        public void InsertChangeOwnerActivity(int entityId, int? userId, AppModules module, UserActivityType activityType)
        {
            var db = ObjectContextFactory.Create();
            UserActivityLogsDb activityLog = new UserActivityLogsDb()
            {
                EntityID = entityId,
                LogDate = DateTime.Now.ToUniversalTime(),
                UserID = userId,
                ModuleID = (byte)module,
                UserActivityID = (byte)activityType
            };
            db.UserActivitiesLog.Add(activityLog);
            db.SaveChanges();
        }

        /// <summary>
        /// Gets the contacts by activity.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="activityName">Name of the activity.</param>
        /// <param name="contactIDs">The contact i ds.</param>
        /// <param name="sort">The sort.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IList<int> GetContactsByActivity(int userId, AppModules moduleName, UserActivityType activityName, int[] contactIDs, string sort,int accountId)
        {
            var db = ObjectContextFactory.Create();
            IList<int> activities = new List<int>();
            /*For Recently Updated */
            if (sort == "1")
            {
                activities = db.UserActivitiesLog
              .Where(c => c.UserID == userId && c.AccountID == accountId && c.ModuleID == (byte)moduleName
                  && c.UserActivityID == (byte)activityName && c.AccountID == accountId).
                  Join(db.Contacts, u => u.EntityID, c => c.ContactID, (u, c) => new { u.EntityID, u.LogDate, c.IsDeleted, c.LastUpdatedOn,c.AccountID })
                  .Where(i => i.IsDeleted == false && i.AccountID == accountId).OrderByDescending(u => u.LastUpdatedOn).Take(1000).Select(i => i.EntityID).ToList();
            }
            /*For Full Name */
            else if (sort == "2")
            {
                        activities = db.UserActivitiesLog
              .Where(c => c.UserID == userId && c.AccountID == accountId && c.ModuleID == (byte)moduleName
                  && c.UserActivityID == (byte)activityName && c.AccountID == accountId).
                  Join(db.Contacts, u => u.EntityID, c => c.ContactID, (u, c) => new { u.EntityID, u.LogDate, c.IsDeleted, name = c.FirstName + " " + c.LastName,c.AccountID })
                  .Where(i => i.IsDeleted == false && i.AccountID == accountId).OrderBy(u => u.name).Take(1000).Select(i => i.EntityID).ToList();
             }
            /*For Company Name */
            else if (sort == "3")
            {
                        activities = db.UserActivitiesLog
              .Where(c => c.UserID == userId && c.AccountID == accountId && c.ModuleID == (byte)moduleName
                  && c.UserActivityID == (byte)activityName && c.AccountID == accountId).
                  Join(db.Contacts, u => u.EntityID, c => c.ContactID, (u, c) => new { u.EntityID, u.LogDate, c.IsDeleted, c.Company,c.AccountID })
                  .Where(i => i.IsDeleted == false && i.AccountID == accountId).OrderBy(u => u.Company).Take(1000).Select(i => i.EntityID).ToList();
            }
            /*For Recently Viewed */
            else
            {

                activities = db.UserActivitiesLog
                .Where(c => c.UserID == userId && c.AccountID == accountId && c.ModuleID == (byte)moduleName
                 && c.UserActivityID == (byte)activityName && c.AccountID == accountId).
                 Join(db.Contacts, u => u.EntityID, c => c.ContactID, (u, c) => new { u.EntityID, u.LogDate, c.IsDeleted,c.AccountID })
               .Where(i => i.IsDeleted == false && i.AccountID == accountId).OrderByDescending(u => u.LogDate).Take(1000).Select(i => i.EntityID).ToList();
            }

            var recentlyViewedContactIds = activities.Select(u => u).GroupBy(g => g).Select(f => f.First()).ToList();

            if(sort =="")
            {
                Logger.Current.Informational("RecentlyViewed Contacts Ids: " + recentlyViewedContactIds.Count());
            }
            else if (sort == "1")
            {
                Logger.Current.Informational("RecentlyUpdated Contacts Ids: " + recentlyViewedContactIds.Count());
            }
            else if (sort == "2")
            {
                Logger.Current.Informational("FullName Contacts Ids: " + recentlyViewedContactIds.Count());
            }
            else if (sort == "3")
            {
                Logger.Current.Informational("Company Contacts Ids: " + recentlyViewedContactIds.Count());
            }

            if (contactIDs != null)
                recentlyViewedContactIds = recentlyViewedContactIds.Where(p => contactIDs.Contains(p)).ToList();

            return recentlyViewedContactIds;
        }
    }
}
