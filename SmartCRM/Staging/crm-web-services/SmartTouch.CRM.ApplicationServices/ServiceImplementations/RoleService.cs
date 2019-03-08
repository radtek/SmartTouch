using AutoMapper;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Exceptions;
using SmartTouch.CRM.ApplicationServices.Messaging.Role;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Modules;
using SmartTouch.CRM.Domain.Roles;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class RoleService : IRoleService
    {
        readonly IRoleRepository roleRepository;
        readonly IAccountRepository accountRepository;
        readonly IUnitOfWork unitOfWork;

        public RoleService(IRoleRepository roleRepository, IAccountRepository accountRepository, IUnitOfWork unitOfWork)
        {
            if (roleRepository == null) throw new ArgumentNullException("roleRepository");
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");

            this.roleRepository = roleRepository;
            this.accountRepository = accountRepository;
            this.unitOfWork = unitOfWork;
        }

        GetUserRolePermissionsResponse IRoleService.GetRolePermissions(GetUserRolePermissionsRequest request)
        {
            Logger.Current.Verbose("Request to fetch Role Permissions based on UserId");
            GetUserRolePermissionsResponse response = new GetUserRolePermissionsResponse();
            Logger.Current.Informational("UserId : " + request.accountId);
            List<UserPermission> userPermissions = roleRepository.GetUserPermissions(request.accountId);

            if (userPermissions == null)
            {
                response.Exception = GetUserRolesNotFoundException();
            }
            else
            {
                response.UserPermissions = userPermissions;
            }

            return response;
        }

        public ResourceNotFoundException GetUserRolesNotFoundException()
        {
            return new ResourceNotFoundException("Role-Permissions are not found for the user.");
        }

        public GetRolesResponse GetRoles(GetRolesRequest request)
        {
            Logger.Current.Verbose("Request to fetch Roles of an Account");
            GetRolesResponse response = new GetRolesResponse();
            if (request != null)
            {
                IEnumerable<RoleViewModel> roleViewModel;
                Logger.Current.Informational("Requested roles for AccountId : " + request.AccountId);
                IEnumerable<Role> roles = roleRepository.GetRoles(request.AccountId);
                if (roles != null)
                {
                    roleViewModel = Mapper.Map<IEnumerable<Role>, IEnumerable<RoleViewModel>>(roles);
                    if (roleViewModel != null)
                    {
                        response.RoleViewModel = roleViewModel;
                        response.SubscriptionId = accountRepository.GetSubscriptionIdByAccountId(request.AccountId); 
                    }
                }
                else response.RoleViewModel = null;
            }
            return response;
        }

        public GetRolesResponse GetRolesList(GetRolesRequest request)
        {
            Logger.Current.Verbose("Request to fetch Roles of an Account");
            GetRolesResponse response = new GetRolesResponse();
            IEnumerable<Role> roles = roleRepository.FindAll(request.Query, request.Limit, request.PageNumber, request.AccountId);
            if (roles == null)
            {
                response.Exception = GetRoleNotFoundException();
            }
            else
            {
                IEnumerable<RoleViewModel> leadScoreList = Mapper.Map<IEnumerable<Role>, IEnumerable<RoleViewModel>>(roles);
                response.RoleViewModel = leadScoreList;
                response.TotalHits = roleRepository.FindAll(request.Query, request.AccountId).Count();
                response.SubscriptionId = accountRepository.GetSubscriptionIdByAccountId(request.AccountId);
            }

            return response;
        }

        private Exception GetRoleNotFoundException()
        {
            return new ResourceNotFoundException("The requested role was not found.");
        }

        public GetModulesResponse GetModules(GetModulesRequest request)
        {
            Logger.Current.Verbose("Request to fetch Modules of an Account");
            GetModulesResponse response = new GetModulesResponse();
            IList<ModuleViewModel> moduleViewModels;
            IEnumerable<Module> modules;
            if (request.AccountID != null)
            {
                Logger.Current.Informational("Requested Modules for AccountId : " + request.AccountId);
                modules = roleRepository.GetModules(request.AccountID);
            }
            else
            {
                modules = roleRepository.GetModules(null);
            }
            if (modules != null)
            {
                moduleViewModels = Mapper.Map<IEnumerable<Module>, IEnumerable<ModuleViewModel>>(modules).ToList();
                if (moduleViewModels != null)
                {
                    foreach (var viewModel in moduleViewModels)
                    {
                        var submodule = moduleViewModels.Where(t => t.ParentId == viewModel.ModuleId).ToList();
                        if (submodule != null)
                        {
                            viewModel.SubModules = submodule;
                        }
                    }
                }
                List<ModuleViewModel> newViewModel = moduleViewModels.Where(l => l.ParentId <= 0).ToList();
                response.ModuleViewModel = newViewModel;
            }
            else response.ModuleViewModel = null;
            return response;
        }

        public InsertRolePermissionsResponse InsertRolePermissions(InsertRolePermissionsRequest request)
        {
            Logger.Current.Verbose("Request to insert role permissions");
            InsertRolePermissionsResponse response = new InsertRolePermissionsResponse();
            var rolePermissionsViewModel = request.rolePermissionsViewModel;
            if (rolePermissionsViewModel != null)
            {
                List<ModuleViewModel> modulesViewModel = rolePermissionsViewModel.Modules.ToList();
                List<ModuleViewModel> subModules = new List<ModuleViewModel>();
                foreach (var viewModel in modulesViewModel)
                {
                    if (viewModel.SubModules.Any())
                    {
                        subModules.AddRange(viewModel.SubModules);
                    }
                }
                modulesViewModel.AddRange(subModules);
                List<ModuleViewModel> modules = modulesViewModel.Where(s => s.IsSelected == true).GroupBy(s => s.ModuleId).Select(y => y.First()).ToList();
                List<byte> domainModules = modules.Select(s => s.ModuleId).ToList(); //ConvertToModule(modules);
                roleRepository.InsertRolePermissions(rolePermissionsViewModel.SelectedRole, domainModules);
            }
            return response;
        }

        public GetModulesForRoleResponse GetModulesForRole(GetModulesForRoleRequest request)
        {
            Logger.Current.Verbose("Request to fetch list of Modules for a role");
            GetModulesForRoleResponse response = new GetModulesForRoleResponse();
            Logger.Current.Informational("Requested Modules for RoleId " + request.roleId);
            List<byte> moduleIds = roleRepository.GetModulesByRole(request.roleId);
            if (moduleIds != null)
                response.moduleIds = moduleIds;
            else response.moduleIds = null;
            return response;
        }

        List<Module> ConvertToModule(List<ModuleViewModel> moduleViewModels)
        {
            List<Module> modules = Mapper.Map<List<ModuleViewModel>, List<Module>>(moduleViewModels);
            return modules;
        }
    }
}
