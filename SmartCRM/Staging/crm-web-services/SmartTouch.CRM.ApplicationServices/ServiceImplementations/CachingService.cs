using AutoMapper;
using LandmarkIT.Enterprise.Utilities.Caching;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.Domain.Roles;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class CachingService : ICachingService
    {
        readonly IAccountRepository accountRepository;
        readonly IRoleRepository roleRepository;
        readonly IDropdownRepository dropdownRepository;
        readonly IUrlService urlService;
        MemoryCacheManager cacheManager;
        public CachingService(IAccountRepository accountRepository, IRoleRepository roleRepository, IDropdownRepository dropdownRepository,IUrlService urlService)
        {
            this.accountRepository = accountRepository;
            this.roleRepository = roleRepository;
            this.urlService = urlService;
            this.dropdownRepository = dropdownRepository;
            cacheManager = new MemoryCacheManager();
        }

        bool ICachingService.IsModulePrivate(AppModules module, int accountId)
        {
            string cacheKey = "datasharing" + accountId;
            IEnumerable<byte> privateModules = new List<byte>();
            if (!cacheManager.IsExists(cacheKey))
            {
                privateModules = accountRepository.GetPrivateModules(accountId);
                cacheManager.Add(cacheKey, privateModules, DateTimeOffset.MaxValue);
            }
            else
            {
                privateModules = cacheManager.Get<IEnumerable<byte>>(cacheKey);
            }
            return privateModules.Contains((byte)module);
        }

        bool ICachingService.IsAccountAdmin(short roleId, int accountId)
        {
            string userPermissionsKey = "userpermissions" + accountId;
            IEnumerable<UserPermission> userPermissions = new List<UserPermission>();
            if (!cacheManager.IsExists(userPermissionsKey))
            {
                userPermissions = AddUserPermissions(accountId);
            }
            else
            {
                userPermissions = cacheManager.Get<IEnumerable<UserPermission>>(userPermissionsKey);
            }
            return userPermissions.Any(c => c.RoleId == roleId && c.ModuleId == (byte)AppModules.AccountSettings);
        }

        public IEnumerable<byte> GetAccountPermissions(int accountId)
        {
            Logger.Current.Verbose("Request received to fetch account permissions");
            string accountPermissionsKey = "accountpermissions" + accountId;
            IEnumerable<byte> accountPermissions = new List<byte>();
            if (!cacheManager.IsExists(accountPermissionsKey))
            {
                accountPermissions = AddAccountPermissions(accountId);
            }
            else
            {
                accountPermissions = cacheManager.Get<IEnumerable<byte>>(accountPermissionsKey);
            }
            Logger.Current.Verbose("Returning permissions");

            return accountPermissions;

        }

        public IEnumerable<UserPermission> GetUserPermissions(int accountId)
        {
            string userPermissionsKey = "userpermissions" + accountId;
            IEnumerable<UserPermission> userPermissions = new List<UserPermission>();
            if (!cacheManager.IsExists(userPermissionsKey))
            {
                userPermissions = AddUserPermissions(accountId);
            }
            else
            {
                userPermissions = cacheManager.Get<IEnumerable<UserPermission>>(userPermissionsKey);
            }
            return userPermissions;
        }

        public bool CheckPermission(int accountId, int roleId, AppModules module)
        {
            if (accountId != 0 && roleId != 0)
            {
                var userPermissions = GetUserPermissions(accountId);
                var accountPermissions = GetAccountPermissions(accountId);
                if (userPermissions != null && userPermissions.Any() && accountPermissions != null && accountPermissions.Any())
                    return userPermissions.Any(u => u.RoleId == roleId && u.ModuleId == (byte)module) && accountPermissions.Contains((byte)module);
                else
                    return false;
            }
            else if (accountId != 0 && roleId == 0)   //In-case of automation-engine
                return true;
            else
                return false;
        }

        public Dictionary<AppModules, bool> CheckModulePermissions(int accountId, int roleId, IEnumerable<AppModules> modules)
        {
            Dictionary<AppModules, bool> modulePermissions = new Dictionary<AppModules, bool>();
            var userPermissions = GetUserPermissions(accountId);
            var accountPermissions = GetAccountPermissions(accountId);
            if (accountId != 0 && roleId != 0 && (modules != null && modules.Any()))
            {
                foreach (var module in modules)
                {
                    bool hasPermission = userPermissions.Any((u => u.RoleId == roleId && u.ModuleId == (byte)module && accountPermissions.Contains((byte)module)));
                    modulePermissions.Add(module, hasPermission);
                }
                return modulePermissions;
            }
            else if (accountId != 0 && roleId == 0)   //In-case of automation-engine
            {
                foreach(var module in modules)
                {
                    bool hasPermission = accountPermissions.Contains((byte)module);
                    modulePermissions.Add(module, hasPermission);
                }
                return modulePermissions;
            }
            else
                return modulePermissions;
        }

        public bool CheckSendMailPermissions(int accountId, int roleId)
        {
            if (accountId != 0 && roleId != 0)
            {
                var userPermissions = GetUserPermissions(accountId);
                if (userPermissions != null && userPermissions.Any())
                    return userPermissions.Any(u => u.RoleId == roleId && u.ModuleId == (byte)AppModules.SendMail);
                else
                    return false;
            }
            else
                return false;
        }

        public IEnumerable<DropdownViewModel> GetDropdownValues(int? accountId)
        {
            Logger.Current.Verbose("Request received for getting Dropdownvalues for accountId : " + accountId);
            string dropdownValuesKey = "dropdownvalues" + accountId;
            IEnumerable<DropdownViewModel> dropdowns = new List<DropdownViewModel>();
            if (!cacheManager.IsExists(dropdownValuesKey))
            {
                dropdowns = AddDropdownValues(accountId);
            }
            else
            {
                dropdowns = (IEnumerable<DropdownViewModel>)cacheManager.Get(dropdownValuesKey);
            }
            return dropdowns;
        }

        public Task<IEnumerable<DropdownViewModel>> GetDropdownValuesAsync(int? accountId)
        {
            Logger.Current.Verbose("Request received for getting Dropdownvalues for accountId : " + accountId);
            string dropdownValuesKey = "dropdownvalues" + accountId;
            IEnumerable<DropdownViewModel> dropdowns = new List<DropdownViewModel>();
            if (!cacheManager.IsExists(dropdownValuesKey))
            {
                dropdowns = AddDropdownValues(accountId);
            }
            else
            {
                dropdowns = cacheManager.Get<IEnumerable<DropdownViewModel>>(dropdownValuesKey);
            }
            return Task<IEnumerable<DropdownViewModel>>.Run(() => dropdowns);
        }

        public IEnumerable<UserPermission> AddUserPermissions(int accountId)
        {
            string userPermissionsKey = "userpermissions" + accountId;
             IEnumerable<UserPermission> userPermissions = roleRepository.GetUserPermissions(accountId);
            cacheManager.Add(userPermissionsKey, userPermissions, DateTimeOffset.MaxValue);
            return cacheManager.Get<IEnumerable<UserPermission>>(userPermissionsKey);
        }

        public IEnumerable<byte> AddAccountPermissions(int accountId)
        {
            string accountPermissionsKey = "accountpermissions" + accountId;
            string oppCustomersKey = "opportunitycustomers" + accountId;
            List<byte> opportunityCustomers = new List<byte>();
            IEnumerable<byte> accountPermissions = accountRepository.GetAccountPermissions(accountId);
            byte? opportunityCustomer = accountRepository.GetOpportunityCustomers(accountId);
            if (opportunityCustomer != null)
                opportunityCustomers.Add(accountRepository.GetOpportunityCustomers(accountId).Value);
            cacheManager.Add(accountPermissionsKey, accountPermissions, DateTimeOffset.MaxValue);
            cacheManager.Add(oppCustomersKey, opportunityCustomers, DateTimeOffset.MaxValue);
            return cacheManager.Get<IEnumerable<byte>>(accountPermissionsKey);
        }

        public IEnumerable<byte> AddDataSharingPermissions(int accountId)
        {
            string cacheKey = "datasharing" + accountId;
             IEnumerable<byte> sharingPermissions = accountRepository.GetPrivateModules(accountId);
            cacheManager.Add(cacheKey, sharingPermissions, DateTimeOffset.MaxValue);
            return cacheManager.Get<IEnumerable<byte>>(cacheKey);
        }

        public IEnumerable<DropdownViewModel> AddDropdownValues(int? accountId)
       {
            string dropdownValuesKey = "dropdownvalues" + accountId;
            IEnumerable<DropdownViewModel> dropdownValues = new List<DropdownViewModel>();

            IEnumerable<Dropdown> dropdowns = dropdownRepository.FindAll("", accountId).ToList();
            if (dropdowns.Any())
            {
                dropdownValues = Mapper.Map<IEnumerable<Dropdown>, IEnumerable<DropdownViewModel>>(dropdowns);
            }
            cacheManager.Add(dropdownValuesKey, dropdownValues, DateTimeOffset.UtcNow.AddMinutes(2));
            return cacheManager.Get<IEnumerable<DropdownViewModel>>(dropdownValuesKey);
        }

        public byte? GetOpportunityCustomers(int userId, int accountId, int roleId)
        {
            string oppCustomersKey = "opportunitycustomers" + accountId;
            var userPermissions = GetUserPermissions(accountId);
            bool hasPermission = userPermissions.Where(w => w.RoleId == roleId && w.ModuleId == (byte)AppModules.Opportunity).Any();
            if (hasPermission)
                return cacheManager.Get<IEnumerable<byte>>(oppCustomersKey).FirstOrDefault();
            else
                return null;
        }

        public bool StoreTemporaryFile(string fileKey, byte[] fileContent)
        {
            cacheManager.Add(fileKey, fileContent, DateTimeOffset.MaxValue);
            return true;
        }

        public byte[] GetTemporaryFile(string fileKey)
        {
            if (cacheManager.IsExists(fileKey))
            {
                byte[] file = cacheManager.Get(fileKey) as byte[];
                cacheManager.Remove(fileKey);
                return file;
            }
            else
                return new List<byte>().ToArray();
        }

        public bool CacheWorkflow(object workFlow)
        {
            string key = "workflow";
            cacheManager.Add(key, workFlow, DateTimeOffset.MaxValue);
            return true;
        }

        public object GetWorkflow(int workflowId)
        {
            string key = "workflow";

            if (cacheManager.IsExists(key))
            {
                var workflow = cacheManager.Get(key);
                //cacheManager.Remove(key);
                return workflow;
            }
            else
                return null;
        }

        public void AddAutomationCampaigns(IEnumerable<Campaign> campaigns)
        {
            if (campaigns != null)
            {
                string campaignPermissionKey = "AutomationCampaign";
                cacheManager.Add(campaignPermissionKey, campaigns, new DateTimeOffset().AddHours(6));
            }
        }

        public bool StoreTemporaryString(string stringKey, string stringcontent)
        {
            cacheManager.Add(stringKey, stringcontent, DateTimeOffset.MaxValue);
            return true;
        }

        public string GetTemporaryStringContent(string fileKey)
        {
            if (cacheManager.IsExists(fileKey))
            {
                string stringcontent = cacheManager.Get(fileKey) as string;
                cacheManager.Remove(fileKey);
                return stringcontent;
            }
            else
                return string.Empty;
        }

        public bool StoreSavedSearchContactIds(string key, IEnumerable<int> contactIds)
        {
            key = "SavedSearchContactIds" + key;
            cacheManager.Add(key, contactIds, DateTimeOffset.Now.AddHours(6));
            return true;
        }

        public IEnumerable<int> GetSavedSearchContactIds(string key)
        {
            return cacheManager.Get(key) as IEnumerable<int>;
        }

        public AccountViewModel GetAccount(int AccountID)
        {
            string accountKey = "account" + AccountID;
            AccountViewModel accountviewmodel = null;

            if (!cacheManager.IsExists(accountKey))
            {
                Account account = accountRepository.GetAccountBasicDetails(AccountID);
                if (account != null)
                {
                    accountviewmodel = Mapper.Map<Account, AccountViewModel>(account);
                    if (accountviewmodel.Image != null)
                    {
                        if (!string.IsNullOrEmpty(accountviewmodel.Image.StorageName))
                        {
                            switch (accountviewmodel.Image.ImageCategoryID)
                            {
                                case ImageCategory.AccountLogo:
                                    accountviewmodel.Image.ImageContent = urlService.GetUrl(accountviewmodel.AccountID, ImageCategory.AccountLogo, accountviewmodel.Image.StorageName);
                                    break;
                                default :
                                    accountviewmodel.Image.ImageContent = string.Empty;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        accountviewmodel.Image = new ImageViewModel();
                    }
                    cacheManager.Add(accountKey, accountviewmodel, DateTimeOffset.MaxValue);
                }
                else
                    return null;
            }
            return cacheManager.Get<AccountViewModel>(accountKey);
        }

        public AccountViewModel AddOrUpdateAccount(int AccountID)
        {
            string accountKey = "account" + AccountID;
            AccountViewModel accountviewmodel = null;
            Account account = accountRepository.GetAccountBasicDetails(AccountID);
            if (account != null)
            {
                accountviewmodel = Mapper.Map<Account, AccountViewModel>(account);
            }
            if (accountviewmodel.Image != null)
            {
                if (!string.IsNullOrEmpty(accountviewmodel.Image.StorageName))
                {
                    switch (accountviewmodel.Image.ImageCategoryID)
                    {
                        case ImageCategory.AccountLogo:
                            accountviewmodel.Image.ImageContent = urlService.GetUrl(accountviewmodel.AccountID, ImageCategory.AccountLogo, accountviewmodel.Image.StorageName);
                            break;
                        default:
                            accountviewmodel.Image.ImageContent = string.Empty;
                            break;
                    }
                }
            }
            else
            {
                accountviewmodel.Image = new ImageViewModel();
            }
            cacheManager.Add(accountKey, accountviewmodel, DateTimeOffset.MaxValue);
            return accountviewmodel;
        }

        public void SaveContactGridData(ContactGridViewModel model, Guid guid)
        {
            string key = "contactsgrid" + guid.ToString();
            DateTimeOffset offSetValue = new DateTimeOffset(DateTime.UtcNow).AddHours(10);
            cacheManager.Add(key, model, offSetValue);
        }

        public ContactGridViewModel GetGridViewModel(string key)
        {
            ContactGridViewModel model = new ContactGridViewModel();
            if (key != null)
            {
                string keyValue = "contactsgrid" + key;
                if (cacheManager.IsExists(keyValue))
                    model = cacheManager.Get<ContactGridViewModel>(keyValue);
            }
            return model;
        }

    }
}
