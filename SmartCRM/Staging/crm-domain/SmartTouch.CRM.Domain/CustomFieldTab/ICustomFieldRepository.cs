using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.Fields;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Domain.CustomFields
{
    public interface ICustomFieldRepository : IRepository<CustomFieldTab, int>
    {
        bool IsCustomFieldTabNameUnique(CustomFieldTab customFieldTab);

        void DeactivateCustomField(int[] customFieldIds);

        void DeactivateCustomFieldTab(int customFieldTabId);

        IEnumerable<CustomFieldTab> GetAllCustomFieldTabs(int accountId);

        IEnumerable<CustomFieldTab> FindAll(string name, int limit, int pageNumber, byte status, int AccountID);

        IEnumerable<CustomFieldTab> FindAll(string name, byte status, int AccountID);

        CustomFieldTab GetCustomFieldById(int customFieldId);

        IEnumerable<ContactCustomField> ContactCustomFields(int contactId);

        IEnumerable<Field> GetAllCustomFieldsByAccountId(int accountId);

        IEnumerable<Field> GetAllCustomFieldsForForms(int accountId);

        IEnumerable<Field> GetAllActiveCustomFieldsByAccountID(int accountId);

        IEnumerable<Field> GetAllCustomFieldsForImports(int accountId);

        IEnumerable<FieldValueOption> GetCustomFieldsValueOptions(int accountId);

        CustomFieldTab GetCustomFieldTabByName(string TabName, int AccountID);

        CustomFieldTab GetLeadAdapterCustomFieldTab(int AccountID);

        IEnumerable<CustomField> GetLeadAdapterCustomFields(LeadAdapterTypes LeadAdapterType, int AccountID);

        int GetSavedSearchsCountForCustomFieldById(int fieldId, int? valueOptionId);

        string GetCustomFieldValueName(int fieldId, string valueOptions);
        //string[] GetAllFieldTitles(List<int> fieldIds);
    }
}
