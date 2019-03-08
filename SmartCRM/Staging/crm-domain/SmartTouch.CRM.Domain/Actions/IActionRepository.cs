using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DA = SmartTouch.CRM.Domain.Actions;

using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain;
using System.ComponentModel;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.Contacts;

namespace SmartTouch.CRM.Domain.Actions
{
    public interface IActionRepository : IRepository<Action, int>
    {
        IEnumerable<DA.Action> FindBy(int[] actionIds);
        Action FindBy(int actionId, int contactId);
        IEnumerable<Action> FindByContact(int contactId);
        List<DA.ActionContactsSummary> GetActionContactsStatus(int actionId, IEnumerable<int> contactIds,int accountId);
        IEnumerable<Action> FindByOpportunity(int opportunityId);
        Tags.Tag FindTag(string actionName);
        List<LastTouchedDetails> ActionCompleted(int actionId, bool status, int? contactId, int opportunityId, bool completedForAll, int userId, DateTime updatedOn, bool isSchedule, int? mailBulkId);
        Dictionary<int,Guid?> DeleteActionForAll(int actionId,int userId);
        Dictionary<int,Guid?> DeleteAction(int actionId, int contactId,int userId);
        int ContactsCount(int actionId);
        DA.UserActions FindByUser(int[] userIds, int accountID, int pageNumber, int limit, string name, string filter, string sortField, bool isdashboard, DateTime StartDate, DateTime EndDate, string filterByActionType, ListSortDirection listSortDirection);
        IEnumerable<DA.Action> FindByUser(int? userID, int accountID, string name, string filter, string sortField, ListSortDirection listSortDirection);

        List<LastTouchedDetails> ActionsMarkedComplete(int[] actionIds, int userId, DateTime updatedOn,bool isScheduled);

        void ActionsMarkedInComplete(int[] actionIds,int userId,DateTime updatedOn,bool isScheduled);
        IEnumerable<int> GetCompletedActionsByIds(IEnumerable<int> actionIds);

        bool GetActionCompletedStatus(int actionId);
        IEnumerable<Action> GetNewActionsToSync(int accountId, int userId, int? maxRecords, DateTime? timeStamp, bool firstSync, CRUDOperationType operationType);
        IEnumerable<Action> GetModifiedActionsToSync(int accountId, int userId, int? maxRecords, DateTime? timeStamp, bool firstSync);
        IEnumerable<int> GetDeletedTasksToSync(int accountId, int userId, int? maxNumRecords, DateTime? timeStamp);
        void UpdateCRMOutlookMap(DA.Action action, RequestOrigin? requestedFrom);

        bool IsActionFromSelectAll(int actionId);

        DA.Action FindByActionId(int actionId);
        IList<int> GetAllOwnerIds(int actionId);
        Guid GetUserEmailGuid(int actionId, int userId);
        Guid GetUserTextGuid(int actionId, int userId);
        IEnumerable<Guid> GetUserEmailGuids(int actionId);
        IEnumerable<Guid> GetUserTextGuids(int actionId);
        int InsertBulkMailOperationDetails(string subject, string actionTemplateHtml,int actionID,string from);
        void InsertActionsMailOperationDetails(List<ActionsMailOperation> actionsmailOperations);
        void DeleteDateFromBulkMailDetails(int mailBulkId);

        IEnumerable<int> GetContactIds(int actionId);
        void UpdateActionsSheduledEmails(int[] actionIds,bool markAsComplete,Guid groupId);
        void UpdateActionSheduleEmailBody(int actionId, int mailBulkId);
        void UpdateActionContactsWithGroupId(int actionId, Guid groupId);
        string GetActionTypeValueById(short actionType);
        void ActionDetailsAddingToNoteSummary(List<int> actionIds,List<int> contactIds,int accountId,int userId);
        string GetActionTypeById(short actionType);// Added for LIT Deployment on 06/05/2018
    }
}
