using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Identity
{
    public class RoleStore : IRoleStore<IdentityRole>
    {
        private IdentityDbContext _dbContext = default(IdentityDbContext);
        public RoleStore() { _dbContext = new IdentityDbContext(); }
        public RoleStore(string connectionString) { _dbContext = new IdentityDbContext(connectionString); }
        public Task CreateAsync(IdentityRole role)
        {
            if (role == null) { throw new ArgumentNullException("role"); }

            return Task.Run(() =>
            {
                //var entitySet = _dbContext.Set<Roles>();
                //entitySet.Attach(role);
                //_dbContext.Entry<Roles>(role).State = EntityState.Added;
                //_dbContext.SaveChanges();
            });
        }

        public Task DeleteAsync(IdentityRole role)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityRole> FindByIdAsync(string roleId)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityRole> FindByNameAsync(string roleName)
        {            
            throw new NotImplementedException();
        }

        public Task UpdateAsync(IdentityRole role)
        {
            if (role == null) { throw new ArgumentNullException("role"); }

            return Task.Run(() =>
            {
                //var entitySet = _dbContext.Set<Roles>();
                //entitySet.Attach(role);
                //_dbContext.Entry<Roles>(role).State = EntityState.Modified;
                //_dbContext.SaveChanges();
            });
        }

        public void Dispose()
        {
            if (_dbContext != null) { _dbContext.Dispose(); }
        }
    }
}
