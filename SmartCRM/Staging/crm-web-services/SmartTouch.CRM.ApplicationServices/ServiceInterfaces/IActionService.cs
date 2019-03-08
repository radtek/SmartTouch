using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.Messaging.Action;
using SmartTouch.CRM.ApplicationServices.Messaging.Tags;
using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.ImplicitSync;


namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface IActionService
    {
        InsertActionResponse InsertAction(InsertActionRequest request);
        //DeleteTagResponse DeleteTag(DeleteTagRequest request);
        UpdateActionResponse UpdateAction(UpdateActionRequest request);
        DeactivateActionContactResponse ContactDeleteForAction(DeactivateActionContactRequest request);
        GetActionResponse GetAction(GetActionRequest request);
        GetActionListResponse GetContactActions(GetActionListRequest request);
        GetActionListResponse GetOpportunityActions(GetActionListRequest request);
        DeleteActionResponse DeleteAction(DeleteActionRequest request);
        CompletedActionResponse ActionStatus(CompletedActionRequest request);
        GetContactsCountResponse ActionContactsCount(GetContactsCountRequest resquest);
        ReIndexDocumentResponse ReIndexActions(ReIndexDocumentRequest request);
        GetActionListResponse GetUserCreatedActions(GetActionListRequest request);

        CompletedActionsResponse ActionsMarkedComplete(CompletedActionsRequest completedActionsRequest);

        DeleteActionsResponse ActionsDelete(DeleteActionsRequest deleteActionsRequest);

        CompletedActionsResponse ActionsMarkedInComplete(CompletedActionsRequest completedActionsRequest);

        GetTasksToSyncResponse GetNewActionsToSync(GetTasksToSyncRequest request);
        GetTasksToSyncResponse GetModifiedActionsToSync(GetTasksToSyncRequest request);

        GetTasksToSyncResponse GetDeletedTasksToSync(GetTasksToSyncRequest request);
        IList<int> GetAllAssignedUserIds(int actionId);
        //Commented for LIT Deployment on 06/05/2018
        //Domain.Actions.Action UpdateActionBulkData(int actionId, int accountId, int userId, string accountPrimaryEmail, string accountDomain, bool icsCanlender,string AccountAddress,bool isCompleted, IEnumerable<int> contactIDs, bool isPrivate, int ownerID,IDictionary<int,Guid> emailGuids, IDictionary<int, Guid> textGuids);
        //Added for LIT Deployment on 06/05/2018
        Domain.Actions.Action UpdateActionBulkData(int actionId, int accountId, int userId, string accountPrimaryEmail, string accountDomain, bool icsCanlender, bool icsCanlenderToContacts, string AccountAddress, bool isCompleted, IEnumerable<int> contactIDs, bool isPrivate, int ownerID, IDictionary<int, Guid> emailGuids, IDictionary<int, Guid> textGuids);
        BulkSendEmailResponse BulkMailSendForActionContacts(BulkSendEmailRequest request);
    }
}
