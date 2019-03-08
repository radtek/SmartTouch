using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.Messaging.CustomFields;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface ICustomFieldService
    {
        GetAllCustomFieldTabsResponse GetAllCustomFieldTabs(GetAllCustomFieldTabsRequest request);
        InsertCustomFieldTabResponse InsertCustomFieldTab(InsertCustomFieldTabRequest request);
        UpdateCustomFieldTabResponse UpdateCustomFieldTab(UpdateCustomFieldTabRequest request);
        DeleteCustomFieldTabResponse DeleteCustomFieldTab(DeleteCustomFieldTabRequest request);
        SaveAllCustomFieldTabsResponse SaveAllCustomFieldTabs(SaveAllCustomFieldTabsRequest request);
        GetContactCustomFieldsResponse GetContactCustomFields(GetContactCustomFieldsRequest request);
        

        InsertCustomFieldResponse InsertCustomField(InsertCustomFieldRequest request);
        UpdateCustomFieldResponse UpdateCustomField(UpdateCustomFieldRequest request);
        GetAllCustomFieldsResponse GetAllCustomFields(GetAllCustomFieldsRequest request);
        GetAllCustomFieldsResponse GetAllCustomFieldsForForms(GetAllCustomFieldsRequest request);

        GetCustomFieldsValueOptionsResponse GetCustomFieldValueOptions(GetCustomFieldsValueOptionsRequest request);
        GetLeadAdapterCustomFieldResponse GetLeadAdapterCustomFieldsByType(GetLeadAdapterCustomFieldRequest request);
        GetSavedSearchsCountForCustomFieldResponse GetSavedSearchsCountForCustomFieldById(GetSavedSearchsCountForCustomFieldRequest request);
    }
}
