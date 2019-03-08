using SmartTouch.CRM.ApplicationServices.Messaging.DropdownValues;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface IDropdownValuesService
    {

        GetDropdownListResponse GetAll(GetDropdownListRequest request);
        GetDropdownValueResponse GetDropdownValue(GetDropdownValueRequest request);
        InsertDropdownResponse InsertDropdownValue(InsertDropdownRequest request);
        GetDropdownListResponse GetAllByAccountID(string name, int? accountID);
        GetLeadSourcesResponse GetLeadSources(GetLeadSourcesRequest request);
        GetOpportunityStageGroupResponse GetOppoertunityStageGroups(GetOpportunityStageGroupRequest request);
        GetDropdownValueResponse GetDropdownValue(short dropDownValueId);
        GetTemplateDataResponse GetTemplateData(GetTemplateDataRequest request);
       // DeleteDropdownResponse DeleteDropdownValue(DeleteDropdownRequest request);
    }
}
