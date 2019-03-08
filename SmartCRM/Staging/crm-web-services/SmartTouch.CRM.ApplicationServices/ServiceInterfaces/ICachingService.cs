using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Workflows;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface ICachingService
    {
        bool IsModulePrivate(AppModules module, int accountId);
        bool IsAccountAdmin(short roleId,int accountId);
        IEnumerable<byte> GetAccountPermissions(int accountId);
        IEnumerable<UserPermission> GetUserPermissions(int accountId);
        IEnumerable<DropdownViewModel> GetDropdownValues(int? accountId);
        Task<IEnumerable<DropdownViewModel>> GetDropdownValuesAsync(int? accountId);
        IEnumerable<UserPermission> AddUserPermissions(int accountId);
        IEnumerable<byte> AddAccountPermissions(int accountId);
        IEnumerable<byte> AddDataSharingPermissions(int accountId);
        IEnumerable<DropdownViewModel> AddDropdownValues(int? accountId);
        bool CheckPermission(int accountId, int roleId, AppModules module);
        Dictionary<AppModules, bool> CheckModulePermissions(int accountId, int roleId, IEnumerable<AppModules> modules);
        bool CheckSendMailPermissions(int accountId, int roleId);
        byte? GetOpportunityCustomers(int userId, int accountId, int roleId);
        bool StoreTemporaryFile(string fileKey, byte[] fileContent);
        byte[] GetTemporaryFile(string fileKey);
        bool CacheWorkflow(object workFlow);
        object GetWorkflow(int workflowId);
        bool StoreTemporaryString(string stringKey, string stringcontent);
        string GetTemporaryStringContent(string fileKey);
        bool StoreSavedSearchContactIds(string key, IEnumerable<int> contactIds);
        IEnumerable<int> GetSavedSearchContactIds(string key);
        AccountViewModel GetAccount(int AccountID);
        AccountViewModel AddOrUpdateAccount(int AccountID);
        void SaveContactGridData(ContactGridViewModel model, Guid guid);
        ContactGridViewModel GetGridViewModel(string key);
    }
}
