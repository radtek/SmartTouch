using AutoMapper;
using LandmarkIT.Enterprise.Utilities.Logging;
using LinqKit;
using SmartTouch.CRM.Domain.Modules;
using SmartTouch.CRM.Domain.Roles;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class RoleRepository : Repository<Role, short, RolesDb>, IRoleRepository
    {
        public RoleRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory) { }

        /// <summary>
        /// Updates the specified aggregate.
        /// </summary>
        /// <param name="aggregate">The aggregate.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void Infrastructure.Domain.IRepository<Role, short>.Update(Role aggregate)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Inserts the specified aggregate.
        /// </summary>
        /// <param name="aggregate">The aggregate.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void Infrastructure.Domain.IRepository<Role, short>.Insert(Role aggregate)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes the specified aggregate.
        /// </summary>
        /// <param name="aggregate">The aggregate.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void Infrastructure.Domain.IRepository<Role, short>.Delete(Role aggregate)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        Role Infrastructure.Domain.IReadOnlyRepository<Role, short>.FindBy(short id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        IEnumerable<Role> Infrastructure.Domain.IReadOnlyRepository<Role, short>.FindAll()
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<RolesDb> roles = db.Roles.ToList();
            foreach (RolesDb ls in roles)
            {
                yield return Mapper.Map<RolesDb, Role>(ls);
            }
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Role> FindAll(string name, int limit, int pageNumber, int accountId)
        {
            var predicate = PredicateBuilder.True<RolesDb>();
            var records = (pageNumber - 1) * limit;
            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                predicate = predicate.And(a => a.RoleName.Contains(name));
            }
            predicate = predicate.And(a => a.AccountID == accountId);

            IEnumerable<RolesDb> roles = findRolesSummary(predicate).Skip(records).Take(limit);
            foreach (RolesDb dls in roles)
            {
                yield return ConvertToDomain(dls);
            }
        }

        /// <summary>
        /// Finds the roles summary.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        IEnumerable<RolesDb> findRolesSummary(System.Linq.Expressions.Expression<Func<RolesDb, bool>> predicate)
        {
            IEnumerable<RolesDb> roles = ObjectContextFactory.Create().Roles
                .AsExpandable()
                .Where(predicate).OrderBy(c => c.RoleID).Select(ls =>
                    new Role
                    {
                        Id = ls.RoleID,
                        RoleName = ls.RoleName
                    }).ToList().Select(x => new RolesDb
                    {
                        RoleID = x.Id,
                        RoleName = x.RoleName
                    });
            return roles;
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Role> FindAll(string name, int accountId)
        {
            var predicate = PredicateBuilder.True<RolesDb>();
            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                predicate = predicate.And(a => a.RoleName.Contains(name));
            }

            predicate = predicate.And(a => a.AccountID == accountId);
            IEnumerable<RolesDb> roles = findRolesSummary(predicate);
            foreach (RolesDb dls in roles)
            {
                yield return ConvertToDomain(dls);
            }
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override Role FindBy(short id)
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
        public override RolesDb ConvertToDatabaseType(Role domainType, CRMDb context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="databaseType">Type of the database.</param>
        /// <returns></returns>
        public override Role ConvertToDomain(RolesDb databaseType)
        {
            return Mapper.Map<RolesDb, Role>(databaseType);
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="context">The context.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void PersistValueObjects(Role domainType, RolesDb dbType, CRMDb context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the user permissions.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public List<UserPermission> GetUserPermissions(int accountId)
        {
            var db = ObjectContextFactory.Create();
            Logger.Current.Verbose("Request for getting User-Permissions");
            Logger.Current.Informational("Fetching User-Permissions for AccountId " + accountId);
            var sql = @"select rmp.roleid, rmp.moduleid from rolemodulemap (nolock) rmp inner join roles (nolock) r on rmp.roleid = r.roleid where r.accountid = @id";
            return db.Get<UserPermission>(sql, new { Id = accountId }).ToList();
        }

        /// <summary>
        /// Inserts the role permissions.
        /// </summary>
        /// <param name="roleId">The role identifier.</param>
        /// <param name="modules">The modules.</param>
        public void InsertRolePermissions(short roleId, List<byte> modules)
        {
            Logger.Current.Informational("Request for inserting Role-Permissions for a Role with ID :" + roleId);
            var db = ObjectContextFactory.Create();
            var roleModuleDb = db.RoleModules.Where(r => r.RoleID == roleId).ToList();
            if (modules != null)
            {
                foreach (var module in modules)
                {
                    if (!(roleModuleDb.Exists(f => f.ModuleID == module)))
                    {
                        RoleModulesMapDb roleModuleMap = new RoleModulesMapDb();
                        roleModuleMap.RoleID = (short)roleId;
                        roleModuleMap.ModuleID = module;
                        db.RoleModules.Add(roleModuleMap);
                        db.SaveChanges();
                    }
                }
                var deletedModules = roleModuleDb.Where(a => a.ModuleID != 0 && !modules.Contains(a.ModuleID)).ToList();
                foreach (var RoleModulesMapDb in deletedModules)
                {
                    db.RoleModules.Remove(RoleModulesMapDb);
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Gets the roles.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Role> GetRoles(int accountId)
        {
            Logger.Current.Informational("Request for getting roles of an AccountId :" +accountId);
            var db = ObjectContextFactory.Create();
            var roles = db.Roles.Where(r => r.AccountID == accountId).ToList();
            if (roles != null)
            {
                IEnumerable<Role> Roles = Mapper.Map<IEnumerable<RolesDb>, IEnumerable<Role>>(roles);
                return Roles;
            }
            else return null;
        }

        /// <summary>
        /// Gets the modules.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Module> GetModules(int? accountId)
        {
            Logger.Current.Informational("Request for fetching Modules for an Account "+accountId);
            var db = ObjectContextFactory.Create();
            var moduleIds = new List<byte>();
            if (accountId != null)
            {
                int subscriptionId = db.Accounts.Where(a => a.AccountID == accountId).Select(s => s.SubscriptionID).FirstOrDefault();
                moduleIds = db.SubscriptionModules.Where(s => s.SubscriptionID == subscriptionId && s.AccountID == accountId).Select(s => s.ModuleID).ToList();
            }
            else
            {
                moduleIds = db.Modules.Select(s => s.ModuleID).ToList();
            }
            if (moduleIds != null)
            {
                var moduleDbs = db.Modules.Where(m => moduleIds.Contains(m.ModuleID));

                foreach (ModulesDb moduleDb in moduleDbs)
                {
                    yield return Mapper.Map<ModulesDb, Module>(moduleDb);
                }
            }
            else yield return null;
        }

        /// <summary>
        /// Gets the modules by role.
        /// </summary>
        /// <param name="roleId">The role identifier.</param>
        /// <returns></returns>
        public List<byte> GetModulesByRole(short roleId)
        {
            Logger.Current.Informational("Request for getting modules for RoleId "+roleId);
            var db = ObjectContextFactory.Create();
            return db.RoleModules.Where(r => r.RoleID == roleId).Select(s => s.ModuleID).ToList();            
        } 
    }
}
