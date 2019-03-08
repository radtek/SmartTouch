using AutoMapper;
using LandmarkIT.Enterprise.Utilities.Logging;
using LinqKit;
using SmartTouch.CRM.Domain.Subscriptions;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class SubscriptionRepository : Repository<Subscription, byte, SubscriptionsDb>, ISubscriptionRepository
    {
        public SubscriptionRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory)
        {
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Subscription FindBy(byte id)
        {
            var db = ObjectContextFactory.Create();
            SubscriptionsDb subscriptionDb = db.Subscriptions.Where(s => s.SubscriptionID == id).SingleOrDefault();
            Subscription subscription = ConvertToDomain(subscriptionDb);
            return subscription;
        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override SubscriptionsDb ConvertToDatabaseType(Subscription domainType, CRMDb context)
        {
            SubscriptionsDb subscriptionDb;
            if (domainType != null)
            {
                subscriptionDb = AutoMapper.Mapper.Map<Subscription, SubscriptionsDb>(domainType);
                return subscriptionDb;
            }
            else return null;
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="databaseType">Type of the database.</param>
        /// <returns></returns>
        public override Subscription ConvertToDomain(SubscriptionsDb databaseType)
        {
            Subscription subscription;
            if (databaseType != null)
            {
                subscription = AutoMapper.Mapper.Map<SubscriptionsDb, Subscription>(databaseType);
                return subscription;
            }
            else return null;
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="context">The context.</param>
        public override void PersistValueObjects(Subscription domainType, SubscriptionsDb dbType, CRMDb context)
        {
            //will use this
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<Subscription> FindAll()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Inserts the subscription permissions.
        /// </summary>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <param name="moduleIds">The module ids.</param>
        /// <param name="accountId">The account identifier.</param>
        public void InsertSubscriptionPermissions(byte subscriptionId,List<byte> moduleIds,int accountId)
        {
            Logger.Current.Verbose("Request to insert Subscription Permissions");
            var db = ObjectContextFactory.Create();
            var subscriptionModuleDb = db.SubscriptionModules.Where(r => r.SubscriptionID == subscriptionId && r.AccountID == accountId).ToList();
            var LimitforUsers = db.SubscriptionModules.Where(r=>r.SubscriptionID == (int)AccountSubscription.BDX && r.AccountID == null && r.ModuleID == (int)AppModules.Users).Select(p=>p.Limit).FirstOrDefault();
            if (moduleIds != null)
            {
                Logger.Current.Informational("Request to insert Subscription Permissions for AccountId " + accountId);
                Logger.Current.Informational("Request to change Subscription Permissions for SubscriptionId " + subscriptionId);

                List<byte> newModules = new List<byte>();
                foreach (var moduleId in moduleIds)
                {
                    if (!(subscriptionModuleDb.Exists(f => f.ModuleID == moduleId)))
                    {
                        SubscriptionModuleMapDb subscriptionModuleMap = new SubscriptionModuleMapDb();
                        subscriptionModuleMap.SubscriptionID = (byte)subscriptionId;
                        subscriptionModuleMap.AccountID = accountId;
                        subscriptionModuleMap.ModuleID = moduleId;

                        if (subscriptionId == (int)AccountSubscription.BDX && moduleId == (int)AppModules.Users)
                            subscriptionModuleMap.Limit = LimitforUsers;

                        db.SubscriptionModules.Add(subscriptionModuleMap);
                        newModules.Add(moduleId);
                    }
                }
                if (newModules != null && newModules.Any())
                    addRolePermissions(accountId,newModules,db);

                var deletedModules = subscriptionModuleDb.Where(a => a.ModuleID != 0 && !moduleIds.Contains(a.ModuleID) && a.AccountID== accountId);
                if (deletedModules != null && deletedModules.Any())
                {
                    List<byte> deletedModuleIds = deletedModules.Select(s => s.ModuleID).ToList();
                    removeDataSharingPermissions(accountId, deletedModuleIds, db);
                    removeRolePermissions(accountId, deletedModuleIds, db);
                }
                db.SubscriptionModules.RemoveRange(deletedModules);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Removes the data sharing permissions.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="moduleIds">The module ids.</param>
        /// <param name="db">The database.</param>
        void removeDataSharingPermissions(int accountId, List<byte> moduleIds,CRMDb db)
        {
            Logger.Current.Verbose("Request to remove Data-Sharing permissions");
            if (moduleIds != null)
            {
                Logger.Current.Informational("Removing Data-Sharing permissions for AccountId "+accountId);
                Logger.Current.Informational("No of modules for remove :" + moduleIds.Count);
                var dataAccessPermission = db.DataAccessPermissions.Where(s => s.AccountID == accountId && moduleIds.Contains(s.ModuleID));
                db.DataAccessPermissions.RemoveRange(dataAccessPermission);
            }
        }

        /// <summary>
        /// Removes the role permissions.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="moduleIds">The module ids.</param>
        /// <param name="db">The database.</param>
        void removeRolePermissions(int accountId,List<byte> moduleIds, CRMDb db)
        {
            Logger.Current.Verbose("Request to remove Role Permissions for removed modules in subscription");
            if (moduleIds != null)
            {
                Logger.Current.Informational("Removing Role-Access permissions for AccountId " + accountId);
               var accountRoles = db.Roles.Where(s => s.AccountID == accountId).Select(s => s.RoleID); 
               var roleAccessPermissions = db.RoleModules.Where(s => s.ModuleID != 0 && moduleIds.Contains(s.ModuleID) && accountRoles.Contains(s.RoleID));
               db.RoleModules.RemoveRange(roleAccessPermissions);
            }
        }

        /// <summary>
        /// Adds the role permissions.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="newmoduleIds">The newmodule ids.</param>
        /// <param name="db">The database.</param>
        void addRolePermissions(int accountId, List<byte> newmoduleIds, CRMDb db)
        {
            var crmdb = ObjectContextFactory.Create();
            var roleModules = crmdb.RoleModules.ToList();
            var adminRoleId = db.Roles.Where(r => r.AccountID == accountId)
                    .Join(db.RoleModules.Where(r => r.ModuleID == (byte)AppModules.AccountSettings), r => r.RoleID, rm => rm.RoleID, (o, i) => o)
                    .Select(s => s.RoleID).FirstOrDefault();
            newmoduleIds.ForEach(m =>
            {
                if (!(roleModules.Exists(rm => rm.ModuleID == m && rm.RoleID == adminRoleId)))
                {
                    RoleModulesMapDb roleModuleMap = new RoleModulesMapDb();
                    roleModuleMap.ModuleID = m;
                    roleModuleMap.RoleID = adminRoleId;
                    db.RoleModules.Add(roleModuleMap);
                }
            });
        }

        /// <summary>
        /// Finds the subscription for domainurl.
        /// </summary>
        /// <param name="domainUrl">The domain URL.</param>
        /// <returns></returns>
        public string FindSubscriptionForDomainurl(string domainUrl)
        {
            var predicate = PredicateBuilder.True<SubscriptionsDb>();
            if (!string.IsNullOrEmpty(domainUrl))
            {
                domainUrl = domainUrl.ToLower();
                predicate = predicate.And(a => a.SubscriptionName.Contains(domainUrl));
            }
            return ObjectContextFactory.Create().Subscriptions.OrderBy(p => p.SubscriptionName)
                .AsExpandable()
                .Where(predicate).Select(p=> p.SubscriptionID).ToString();
        }

        public IEnumerable<Subscription> GetAllSubscriptions()
        {
            var procedureName = "[dbo].[Account_Subscriptions_Types]";
            var parms = new List<SqlParameter>
                {
              
                };
            CRMDb context = new CRMDb();
            var objectContext = (context as IObjectContextAdapter).ObjectContext;
            objectContext.CommandTimeout = 400;
            return context.ExecuteStoredProcedure<Subscription>(procedureName, parms);
        }
    }
}
