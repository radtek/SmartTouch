using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.Messaging.Forms;
using SmartTouch.CRM.ApplicationServices.Messaging.Search;
using SmartTouch.CRM.ApplicationServices.Messaging.Tags;
using SmartTouch.CRM.ApplicationServices.Messaging.User;
using SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow;
using SmartTouch.CRM.ApplicationServices.Messaging.LeadAdapters;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface IWorkflowService
    {
        GetWorkflowListResponse GetAllWorkFlows(GetWorkflowListRequest request);
        GetActiveWorkflowsResponse GetAllActiveWorkflows(GetActiveWorkflowsRequest request);
        InsertWorkflowResponse InsertWorkflow(InsertWorkflowRequest request);
        UpdateWorkflowResponse UpdateWorkflow(UpdateWorkflowRequest request);
        DeleteWorkflowResponse DeleteWorkflow(DeleteWorkflowRequest request);
        GetWorkflowResponse GetWorkFlow(GetWorkflowRequest request);
        GetWorkflowResponse CopyWorkFlow(GetWorkflowRequest request);
        GetCampaignsResponse GetAllCampaigns(GetCampaignsRequest request);
        GetFormsResponse GetAllForms(GetFormsRequest request);
        GetTagListResponse GetAllTags(GetTagListRequest request);
        GetSavedSearchesResponse GetAllSmartSearches(GetSavedSearchesRequest request);
        GetLeadAdapterListResponse GetAllLeadAdapters(GetLeadAdapterListRequest request);
        GetCampaignLinksResponse GetCampaignLinks(GetCampaignLinksRequest request);
        GetUserListResponse GetAllUsers(GetUserListRequest request);
        GetWorkflowsResponse GetRemainingWorkFlows(GetWorkflowsRequest request);
        GetWorkflowRelatedCampaignsResponse GetRelatedCampaigns(GetWorkflowRelatedCampaignsRequest request);
        CampaignStatisticsByWorkflowResponse GetCampaignStatisticsByWorkflow(CampaignStatisticsByWorkflowRequest request);
        WorkflowStatusResponse UpdateWorkflowStatus(WorkflowStatusRequest request);

        DeactivateWorkflowResponse DeactivateWorkflow(DeactivateWorkflowRequest request);
        RemoveFromOtherWorkflowsResponse RemoveFromOtherWorkflows(RemoveFromOtherWorkflowsRequest request);
        IsEnrolledToRemoveResponse IsEnrolledToRemove(IsEnrolledToRemoveRequest request);
        AssignUserResponse AssignUser(AssignUserRequest request);
        NotifyUserResponse NotifyUser(NotifyUserRequest request);
        InsertContactWorkflowAuditResponse InsertContactWorkflowAudit(InsertContactWorkflowAuditRequest request);
        HasContactEnteredWorkflowResponse HasContactEnteredWorkflow(HasContactEnteredWorkflowRequest request);
        HasContactCompletedWorkflowResponse HasContactCompletedWorkflow(HasContactCompletedWorkflowRequest request);
        CanContactReenterWorkflowResponse CanContactReenterWorkflow(CanContactReenterWorkflowRequest request);
        GetContactLastStateResponse GetLastState(GetContactLastStateRequest request);
        GetWorkflowEndStateResponse GetEndState(GetWorkflowEndStateRequest request);
        UpdateContactFieldResponse UpdateContactField(UpdateContactFieldRequest request);
        GetNextBatchToProcessResponse GetNextBatchToProcess();
        void UpdateActionBatchStatus(UpdateActionBatchStatusRequest request);
        GetCampaignsResponse GetCampaigns(GetCampaignsRequest request);
        void UpdateWorkflowName(string name,int workflowId,int accountId);
        GetNotifyUserActionResponse GetNotifyUserAction(GetNotifyUserActionRequest request);
        UpdateNotifyUserActionResponse UpdateNotifyUserAction(UpdateNotifyUserActionRequest request);
        GetUserAssigmentActionResponse GetUserAssignmentAction(GetUserAssignmentActionRequest request);
        UpdateUserAssignmentActionReponse UpdateUserAssignmentAction(UpdateUserAssignmentActionRequest request);
        int GetWorkflowIdByParentWfId(int parentId);
        bool HasContactMatchedEndTrigger(int contactId, int workflowId,long trackMessageID);
        bool IsWorkflowHasTimerActions(int workflowId);
    }
}
