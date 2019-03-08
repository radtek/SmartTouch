using SmartTouch.CRM.Domain.Modules;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Roles
{
    public interface IRoleRepository : IRepository<Roles.Role, short>
    {
        IEnumerable<Module> GetModules(int? accountId);
        IEnumerable<Roles.Role> GetRoles(int accountId);
        List<UserPermission> GetUserPermissions(int accountId);
        List<byte> GetModulesByRole(short roleId);
        void InsertRolePermissions(short roleId, List<byte> modules);
        IEnumerable<Role> FindAll(string name, int limit, int pageNumber, int accountId);
        IEnumerable<Role> FindAll(string name, int accountId);
        //IEnumerable<Role> FindAll(string name);
    }
}
