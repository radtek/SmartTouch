using Kendo.Mvc.UI;
using Newtonsoft.Json;
using SmartTouch.CRM.ApplicationServices.Messaging.Role;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Web.Utilities;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartTouch.CRM.Web.Controllers
{
    public class RoleController : SmartTouchController
    {
        IRoleService roleService;

        ICachingService cachingService;

        public RoleController(IRoleService roleService, ICachingService cachingService)
        {
            this.roleService = roleService;
            this.cachingService = cachingService;
        }

        [Route("roleconfiguration/{roleId}")]
        [SmarttouchAuthorize(Entities.AppModules.Roles, Entities.AppOperations.Create)]
        [MenuType(MenuCategory.Roles, MenuCategory.LeftMenuAccountConfiguration)]
        public ActionResult AddRolePermissions(short roleId)
        {
            RolePermissionsViewModel viewModel = new RolePermissionsViewModel();
            GetRolesResponse rolesResponse = roleService.GetRoles(new GetRolesRequest()
            {
                AccountId = this.Identity.ToAccountID()
            });
            if (rolesResponse != null && !rolesResponse.RoleViewModel.Any(r => r.RoleId == roleId))
                return RedirectToAction("NotFound", "Error");
            GetModulesResponse modulesResponse = roleService.GetModules(new GetModulesRequest()
            {
                AccountID = this.Identity.ToAccountID()
            });
            if (!this.Identity.IsSTAdmin())
            {
                if(rolesResponse.SubscriptionId == 2)
                    rolesResponse.RoleViewModel = rolesResponse.RoleViewModel.Where(s => s.RoleName != "Account Administrator");
                else
                    rolesResponse.RoleViewModel = rolesResponse.RoleViewModel.Where(s => s.RoleName != "Account Administrator" && s.RoleName != "Marketing Administrator" && s.RoleName != "Sales Administrator" && s.RoleName != "Marketing");


                modulesResponse.ModuleViewModel = modulesResponse.ModuleViewModel.Where(s => s.ModuleId != (byte)AppModules.AccountSettings);
            }
            viewModel.Modules = modulesResponse.ModuleViewModel;
            viewModel.Roles = rolesResponse.RoleViewModel;
            GetModulesForRoleResponse response = roleService.GetModulesForRole(new GetModulesForRoleRequest()
            {
                roleId = roleId
            });
            foreach (var item in response.moduleIds)
            {
                foreach (var module in viewModel.Modules)
                {
                    if (module.ModuleId == item)
                    {
                        module.IsSelected = true;
                    }
                    if (module.SubModules != null)
                    {
                        foreach (var submodule in module.SubModules)
                        {
                            if (submodule.ModuleId == item)
                            {
                                submodule.IsSelected = true;
                            }
                        }
                    }
                }
            }
            viewModel.SelectedRole = roleId;
            return View("RolePermissions", viewModel);
        }

        [HttpPost]
        public JsonResult InsertRolePermissions(string rolePermissionViewodel)
        {
            RolePermissionsViewModel PermissionViewodel = JsonConvert.DeserializeObject<RolePermissionsViewModel>(rolePermissionViewodel);
            InsertRolePermissionsResponse response = roleService.InsertRolePermissions(new InsertRolePermissionsRequest()
            {
                rolePermissionsViewModel = PermissionViewodel
            });
            if (response.Exception == null)
            {
                int accountId = this.Identity.ToAccountID();
                cachingService.AddUserPermissions(accountId);
            }
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetRolePermissionsForRole(short roleId)
        {
            GetModulesForRoleResponse response = roleService.GetModulesForRole(new GetModulesForRoleRequest()
            {
                roleId = roleId
            });
            return Json(new
            {
                success = true,
                response = response.moduleIds
            }, JsonRequestBehavior.AllowGet);
        }

        public void AddCookie(string cookieName, string Value, int days)
        {
            HttpCookie CartCookie = new HttpCookie(cookieName, Value);
            CartCookie.Expires = DateTime.Now.AddDays(days);
            Response.Cookies.Add(CartCookie);
        }

        [SmarttouchAuthorize(AppModules.Roles, AppOperations.Read)]
        public ActionResult RolesViewRead([DataSourceRequest] DataSourceRequest request, string name)
        {
            AddCookie("leadscorepagesize", request.PageSize.ToString(), 1);
            AddCookie("leadscorepagenumber", request.Page.ToString(), 1);
            GetRolesResponse response = roleService.GetRolesList(new GetRolesRequest()
            {
                Query = name,
                Limit = request.PageSize,
                PageNumber = request.Page,
                AccountId = UserExtensions.ToAccountID(this.Identity)
            });
            if (!this.Identity.IsSTAdmin())
            {
                if(response.SubscriptionId == 2)
                    response.RoleViewModel = response.RoleViewModel.Where(s => s.RoleName != "Account Administrator");
                else
                    response.RoleViewModel = response.RoleViewModel.Where(s => s.RoleName != "Account Administrator" && s.RoleName != "Marketing Administrator" && s.RoleName != "Sales Administrator" && s.RoleName != "Marketing");

            }
            

            int count = response.RoleViewModel.Count();
            return Json(new DataSourceResult
            {
                Data = response.RoleViewModel,
                Total = count
            }, JsonRequestBehavior.AllowGet);
        }

        [Route("roles")]
        [SmarttouchAuthorize(AppModules.Roles, AppOperations.Read)]
        [MenuType(MenuCategory.RolesList, MenuCategory.LeftMenuAccountConfiguration)]
        [OutputCache(Duration = 30)]
        public ActionResult RolesList(int? accountId)
        {
            RoleViewModel viewModel = new RoleViewModel();
            ViewBag.leadscoreID = 0;
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            return View("RolesList", viewModel);
        }
    }
}
