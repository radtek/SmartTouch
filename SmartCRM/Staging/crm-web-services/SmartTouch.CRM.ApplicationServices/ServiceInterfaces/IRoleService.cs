using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.Messaging.Role;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface IRoleService 
    {
        GetUserRolePermissionsResponse GetRolePermissions(GetUserRolePermissionsRequest request);
        GetRolesResponse GetRoles(GetRolesRequest request);
        GetModulesResponse GetModules(GetModulesRequest request);
        GetModulesForRoleResponse GetModulesForRole(GetModulesForRoleRequest request);
        InsertRolePermissionsResponse InsertRolePermissions(InsertRolePermissionsRequest request);
        GetRolesResponse GetRolesList(GetRolesRequest request);
    }
}
