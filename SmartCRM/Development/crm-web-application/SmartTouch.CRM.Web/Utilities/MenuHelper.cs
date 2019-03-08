using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using System.Collections.Generic;
using System.Linq;
using SmartTouch.CRM.Identity;
using System.Security.Claims;
using SmartTouch.CRM.Domain.ValueObjects;
using System.Threading;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Repository.Database;

namespace SmartTouch.CRM.Web.Utilities
{
    public static class MenuHelper
    {
        public static MenuItemViewModel GetMenuItemsByMenuCategory(MenuCategory topMenucategory, MenuCategory leftMenuCategory, bool isAdvancedGrid)
        {
            ICachingService cachingService = IoC.Container.GetInstance<ICachingService>();
            var result = new List<MenuItem>();
            var usersPermissions = cachingService.GetUserPermissions(Thread.CurrentPrincipal.Identity.ToAccountID());
            var accountPermissions = cachingService.GetAccountPermissions(Thread.CurrentPrincipal.Identity.ToAccountID());
            var userModules = usersPermissions.Where(s => s.RoleId == (short)Thread.CurrentPrincipal.Identity.ToRoleID() && accountPermissions.Contains(s.ModuleId)).Select(r => r.ModuleId).ToList();
            MenuItemViewModel viewModel = new MenuItemViewModel();
            if (leftMenuCategory == MenuCategory.LeftMenuAccountSettings)
            {
                viewModel.IsAccountSettings = true;
            }
            var menus = new MenuService().GetMenu(topMenucategory.ToString(), leftMenuCategory.ToString(), userModules);
            if (isAdvancedGrid)
                menus = menus.Where(w => w.MenuID != 1 && w.MenuID != 2).ToList();
            foreach (var parentItem in menus.Where(mi => mi.ParentMenuID.Equals(null)))
            {
                result.Add(parentItem);
                var currentItem = (MenuItem)parentItem;
                foreach (var childItem in menus.Where(ci => ci.ParentMenuID == parentItem.MenuID).OrderBy(c => c.SortingID))
                {
                    result.Find(r => r.MenuId == currentItem.MenuId).Children.Add(childItem);
                }
            }
            result.OrderBy(o => o.SortingId).ToList();
            viewModel.TopMenuItems = result.GroupBy(s => s.Category).Where(s => s.Key == topMenucategory).SelectMany(group => group).OrderBy(group => group.SortingId).ToList();
            viewModel.LeftMenuItems = result.GroupBy(s => s.Category).Where(s => s.Key == leftMenuCategory).SelectMany(group => group).OrderBy(group => group.SortingId).ToList();
            return viewModel;
        }

        public static List<TopMenuPermissions> CheckPermission(List<AppModules> modules)
        {
            ICachingService cachingService = IoC.Container.GetInstance<ICachingService>();
            var usersPermissions = cachingService.GetUserPermissions(Thread.CurrentPrincipal.Identity.ToAccountID());
            List<byte> userModules = usersPermissions.Where(s => s.RoleId == (short)Thread.CurrentPrincipal.Identity.ToRoleID()).Select(s => s.ModuleId).ToList();
            List<TopMenuPermissions> Permissions = new List<TopMenuPermissions>();
            foreach (AppModules appModule in modules)
            {
                if (userModules.Contains((byte)appModule))
                {
                    Permissions.Add(new TopMenuPermissions()
                    {
                        HasPermission = true,
                        Module = appModule
                    });
                }
            }
            return Permissions;
        }

        public static AccountConfigModule AccountConfigPermission()
        {
            AccountConfigModule configModule = new AccountConfigModule();
            ICachingService cachingService = IoC.Container.GetInstance<ICachingService>();
            var usersPermissions = cachingService.GetUserPermissions(Thread.CurrentPrincipal.Identity.ToAccountID());
            var accountModules = cachingService.GetAccountPermissions(Thread.CurrentPrincipal.Identity.ToAccountID());
            List<byte> userModules = usersPermissions.Where(s => s.RoleId == (short)Thread.CurrentPrincipal.Identity.ToRoleID()).Select(s => s.ModuleId).ToList();
            List<ModuleDetails> accountConfigModules = new List<ModuleDetails>();
            accountConfigModules.Add(new ModuleDetails()
            {
                Module = AppModules.Accounts,
                Controller = "Account",
                ActionMethod = "AccountList"
            });
            accountConfigModules.Add(new ModuleDetails()
            {
                Module = AppModules.Users,
                Controller = "User",
                ActionMethod = "UserList"
            });
            accountConfigModules.Add(new ModuleDetails()
            {
                Module = AppModules.Roles,
                Controller = "Role",
                ActionMethod = "AddRolePermissions"
            });
            accountConfigModules.Add(new ModuleDetails()
            {
                Module = AppModules.CustomFields,
                Controller = "CustomField",
                ActionMethod = "CustomFields"
            });
            accountConfigModules.Add(new ModuleDetails()
            {
                Module = AppModules.LeadScore,
                Controller = "LeadScore",
                ActionMethod = "RulesList"
            });
            accountConfigModules.Add(new ModuleDetails()
            {
                Module = AppModules.ImportData,
                Controller = "ImportData",
                ActionMethod = "ImportDataList"
            });
            accountConfigModules.Add(new ModuleDetails()
            {
                Module = AppModules.Tags,
                Controller = "Tag",
                ActionMethod = "TagList"
            });
            accountConfigModules.Add(new ModuleDetails()
            {
                Module = AppModules.DropdownFields,
                Controller = "DropdownValues",
                ActionMethod = "DropdownValuesList"
            });
            accountConfigModules.Add(new ModuleDetails()
            {
                Module = AppModules.LeadAdapter,
                Controller = "LeadAdapter",
                ActionMethod = "LeadAdapterList"
            });
            List<AppModules> configModules = new List<AppModules>();
            configModules = accountConfigModules.Select(s => s.Module).ToList();
            if (configModules.Any(cm => userModules.Contains((byte)cm) && accountModules.Contains((byte)cm)))
            {
                configModule.HasModule = true;
                configModule.Controller = accountConfigModules.FirstOrDefault(f => userModules.Contains((byte)f.Module) && accountModules.Contains((byte)f.Module)).Controller;
                configModule.ActionMethod = accountConfigModules.FirstOrDefault(f => userModules.Contains((byte)f.Module) && accountModules.Contains((byte)f.Module)).ActionMethod;
                return configModule;
            }
            else
            {
                configModule.HasModule = false;
                return configModule;
            }
        }

        public static byte? GetOpportunityCustomers()
        {
            ICachingService cachingService = IoC.Container.GetInstance<ICachingService>();
            return cachingService.GetOpportunityCustomers(Thread.CurrentPrincipal.Identity.ToUserID(), Thread.CurrentPrincipal.Identity.ToAccountID(), Thread.CurrentPrincipal.Identity.ToRoleID());
        }
    }
    public class TopMenuPermissions
    {
        public AppModules Module{ get; set; }
        public bool HasPermission{ get; set; }
    }
    public class AccountConfigModule
    {
        public bool HasModule{ get; set; }
        public string Controller{ get; set; }
        public string ActionMethod{ get; set; }
    }
    public class ModuleDetails
    {
        public AppModules Module{ get; set; }
        public string Controller{ get; set; }
        public string ActionMethod{ get; set; }
    }
}
