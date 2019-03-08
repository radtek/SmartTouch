﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Domain.Search;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.LeadAdapters;
using System.ComponentModel;

namespace SmartTouch.CRM.Domain.Workflows
{
    public interface IWorkflowRepository : IRepository<Workflow, short>
    {        
        IEnumerable<Workflow> FindAll(string name, int limit, int pageNumber, short status, int accountId,string sortField, ListSortDirection direction);
        IEnumerable<Workflow> FindActiveWorkflows(int pageNumber, int limit, string sortField, ListSortDirection direction);
        int FindAllWorkflowsCount(string name, short status, int accountId);
        bool IsWorkflowNameUnique(Workflow workflow);
        IEnumerable<string> checkToUpdateStatus(int workflowId);
        Workflow GetWorkflowByID(short WorkflowID,int accountId);
        void DeleteWorkFlows(int[] workflowIDs);
        void UpdateLinkActions(Workflow workflow);
        IEnumerable<Campaign> GetAllCampaigns(int AccountID, bool IsWorkflowCampaign);
        IEnumerable<Form> GetAllForms(int AccountID);
        IEnumerable<SearchDefinition> GetAllSmartSearches(int AccountID);
        IEnumerable<Tag> GetAllTags(int AccountID);
        IEnumerable<User> GetAllUsers(int AccountID);
        UserContactActivitySummary GetBasicContactDetails(int contactId,int accountId);
        void DeactivateWorkflow(short workflowId);
        void RemoveFromOtherWorkflows(int contactId, string removeFromWorkflows, byte allowParallelWorkflows);
        IEnumerable<short> IsEnrolledToRemove(int workflowId);
        void InsertContactWorkflowAudit(short workflowId, int contactId, int workflowActionId, string messageId);
        bool HasCompletedWorkflow(int workflowId, int contactId, int workflowActionId);
        Dictionary<int, WorkflowNotifyUserAction> GetNotifyUserActionDetails(int workflowActionId);
        IEnumerable<int> GetAddTagActionDetails(int workflowActionId);
        WorkflowAction UpdateAction(WorkflowAction workflowAction);
        IEnumerable<Campaign> GetRelatedCampaigns(short workflowID);
        IEnumerable<Workflow> GetRemainingWorkflows(short WorkflowID, int accountId);
        IEnumerable<WorkflowCampaignStatistics> GetCampaignStatistics(int campaignID, short workflowID, DateTime fromDate, DateTime toDate);
        void UpdateWorkflowStatus(WorkflowStatus status, int workflowID, int modifiedBy);
        bool CheckIfContactEnteredWorkflow(int contactId, int workflowId, string messageId);
        bool CanContactReenterWorkflow(int workflowId);
        int GetLastState(int contactId, int workflowId);
        WorkflowAction GetEndState(short workflowId);
        void InsertWorkflowEndAction(int workflowID);
        IEnumerable<Workflow> GetContactWorkflows(int contactId);
        IEnumerable<LeadAdapterAndAccountMap> GetLeadAdapters(int AccountID);
        string GetSelectedLinkNamesinTrigger(int campaignId);
        IEnumerable<IDictionary<string, object>> GetScheduledMessages(TimeSpan? time);
        void UpdateMessageStatus(int tmid, bool isPublished);
        Status GetUserStatus(int userId);
        void UpdateContactField(int fieldId, string fieldValue, int contactId, int fieldInputTypeId,int aacountId);
        IEnumerable<TrackAction> GetNextBatchToProcess();
        void UpdateActionBatchStatus(IEnumerable<TrackAction> trackActions, IEnumerable<TrackActionLog> trackActionLogs);
        string GetSavedSearchName(int savedSearchId);
        NotificationData GetAllNotificationContactFieldsData(int? contactId,string fields,int notificationType,int? entityId,int accountId);
        bool IsFirstNotification(int workflowId, int contactId, int workflowActionId);
        IEnumerable<Campaign> GetCampaigns(int AccountID, bool IsWorkflowCampaign,string name);
        void AssignUser(int contactId, int workflowId, int userAssignmentActionID, byte scheduledID);
        void UpdateWorkflowName(string workflowName, int workflowId);
        WorkflowNotifyUserAction GetNotifyUserActionById(int workflowActionId);
        void UpdateNotifyUserAction(WorkflowNotifyUserAction action);
        WorkflowAction GetWorkflowAssignmentActionById(int workflowActionId);
        void UpdateUserAssignmentAction(WorkflowUserAssignmentAction action);
        IEnumerable<ParentWorkflow> GetAllParentWorkflows(int workflowId);
        int GetWorkflowIdByParentId(int workflowId);
        IEnumerable<int> GetTriggersByParentWorkflowID(IList<int> workflowIds);
        bool WorkflowNameDuplicateCheck(string name, int workflowId, int accountId);
        bool CheckIfAttachementNeeded(int formId);
        WorkflowGoalStatus HasContactMatchedEndTrigger(int contactId, int workflowId, long TrackMessageID);
        bool IsCreatedBy(int workflowId, int userId, int accountId);
    }
}
