using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class RolePermissionsViewModel
    {
        public IEnumerable<RoleViewModel> Roles { get; set; }
        public short SelectedRole { get; set; }
        public IEnumerable<ModuleViewModel> Modules { get; set; }
    }

    public class ModuleViewModel
    {
        public byte ModuleId { get; set; }
        public string ModuleName { get; set; }
        public int ParentId { get; set; }
        public bool IsSelected { get; set; }
        public bool IsPrivate { get; set; }
        public IEnumerable<ModuleViewModel> SubModules { get; set; }
    }

    public class RoleViewModel
    {
        public string RoleName { get; set; }
        public short RoleId { get; set; }
        public byte SubscriptionID { get; set; }
    }
}
