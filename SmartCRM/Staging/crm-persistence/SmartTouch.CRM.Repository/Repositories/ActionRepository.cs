using AutoMapper;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.Logging;
using LinqKit;
using SmartTouch.CRM.Domain.Actions;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using DA = SmartTouch.CRM.Domain.Actions;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class ActionContact
    {
        public int ContactId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public ContactType ContactType { get; set; }
        public string Email { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? RemindOn { get; set; }
        public string ActionDetails { get; set; }
        public int ActionId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ActionTypeValue { get; set; }
        public DateTime ActionDate { get; set; }
        public DateTime ActionEndTime { get; set; }
        public DateTime ActionStartTime { get; set; }
        public string UserName { get; set; }
        public short? ActionTypeId { get; set; }
        public int TotalActionContacts { get; set; }
        public int TotalCount { get; set; }
    }

    public class ActionRepository : Repository<DA.Action, int, ActionsDb>, DA.IActionRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="objectContextFactory">The object context factory.</param>
        public ActionRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory)
        { }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DA.Action> FindAll()
        {
            var actions = ObjectContextFactory.Create().Actions.Include(i => i.ActionContacts).Include(i => i.ActionTags);

            foreach (var actionDb in actions)
                yield return ConvertToDomain(actionDb);
        }

        /// <summary>
        /// Finds the tag.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <returns></returns>
        public Tag FindTag(string tagName)
        {
            var target = default(Tag);
            var tagDatabase = ObjectContextFactory.Create().Tags.FirstOrDefault(c => c.TagName.Equals(tagName));
            if (tagDatabase != null) target = ConvertToDomain(tagDatabase);
            return target;
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="tagDatabase">The tag database.</param>
        /// <returns></returns>
        private Tag ConvertToDomain(TagsDb tagDatabase)
        {
            Tag tag = new Tag()
            {
                Id = tagDatabase.TagID,
                TagName = tagDatabase.TagName,
                Description = tagDatabase.Description,
                Count = tagDatabase.Count
            };
            return tag;
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="actionId">The action identifier.</param>
        /// <returns></returns>
        public override DA.Action FindBy(int actionId)
        {
            var db = ObjectContextFactory.Create();
            ICollection<ContactsDb> contacts = new List<ContactsDb>();
            ActionsDb actionDatabase = db.Actions.SingleOrDefault(c => c.ActionID == actionId);
            var tags = db.ActionTags.Include(a => a.Tag).Where(a => a.ActionID == actionId).Select(a => a.Tag).ToList();
            actionDatabase.Tags = tags;
            var userDb = ObjectContextFactory.Create();
            var sql = @"SELECT UserName =  STUFF((SELECT ',  ' + (U.FirstName + ' '+ U.LastName)   
                        FROM UserActionMap (NOLOCK) UAM 
                        INNER JOIN Users (NOLOCK) U ON U.UserID = UAM.UserID WHERE UAM.ActionID = A.ActionID FOR XML PATH('')),1,2,'') FROM Actions (NOLOCK) A 
                        WHERE A.ActionID=@ActionId";
            actionDatabase.UserName = userDb.Get<string>(sql, new { ActionId = actionId }).FirstOrDefault();

            if (actionDatabase.SelectAll != null && !actionDatabase.SelectAll.Value)
            {
                contacts = db.ActionContacts.Include(a => a.Contact).Where(a => a.ActionID == actionId)
              .Select(a => a.Contact).ToList();
                actionDatabase.Contacts = contacts;
                actionDatabase.OwnerIds = db.ActionUsers.Where(a => a.ActionID == actionId).Select(u => u.UserID).ToArray();
            }


            if (actionDatabase != null)
                return ConvertToDomain(actionDatabase);
            return null;
        }

        public DA.Action FindByActionId(int actionId)
        {
            var db = ObjectContextFactory.Create();
            ActionsDb actionDatabase = db.Actions.SingleOrDefault(c => c.ActionID == actionId);
            var tags = db.ActionTags.Include(a => a.Tag).Where(a => a.ActionID == actionId).Select(a => a.Tag).ToList();
            actionDatabase.Tags = tags;
            ICollection<ContactsDb> contacts = db.ActionContacts.Include(a => a.Contact).Where(a => a.ActionID == actionId)
               .Select(a => a.Contact).ToList();
            actionDatabase.Contacts = contacts;
            actionDatabase.OwnerIds = db.ActionUsers.Where(a => a.ActionID == actionId).Select(u => u.UserID).ToArray();

            if (actionDatabase != null)
                return ConvertToDomain(actionDatabase);
            return null;
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="actionIds">The action ids.</param>
        /// <returns></returns>
        public IEnumerable<DA.Action> FindBy(int[] actionIds)
        {
            IEnumerable<DA.Action> actions = new List<DA.Action>();
            if (actionIds.Any())
            {
                var db = ObjectContextFactory.Create();
                var actionsDb = db.Actions.Where(p => actionIds.Contains(p.ActionID)).Include(i => i.ActionContacts).Include(i => i.ActionTags).ToList();
                actions = Mapper.Map<IEnumerable<ActionsDb>, IEnumerable<DA.Action>>(actionsDb);
            }
            return actions;
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="actionId">The action identifier.</param>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public DA.Action FindBy(int actionId, int contactId)
        {
            var db = ObjectContextFactory.Create();
            ActionsDb actionDatabase = db.Actions.SingleOrDefault(c => c.ActionID == actionId);
            var tags = db.ActionTags.Include(a => a.Tag).Where(a => a.ActionID == actionId).Select(a => a.Tag).ToList();

            var tagids = tags.Select(p => p.TagID).ToArray();

            var leadscoreTags = db.LeadScoreRules.Where(l => l.IsActive == true && (l.ConditionID == 6 || l.ConditionID == 7) &&
            tagids.Select(s => s.ToString()).Contains(l.ConditionValue)).Select(s => s.ConditionValue).ToArray();


            foreach (TagsDb tag in tags)
            {
                tag.TagName = tag.TagName + (leadscoreTags.Contains(tag.TagID.ToString()) ? " *" : "");
            }

            actionDatabase.Tags = tags;

            if (actionDatabase.SelectAll == false)
            {
                var contacts = db.ActionContacts.Include(a => a.Contact).Where(a => a.ActionID == actionId)
                    .Select(a => a.Contact);
                actionDatabase.Contacts = contacts.Include(p => p.ContactEmails).Where(a => a.IsDeleted == false).ToList();

                actionDatabase.OwnerIds = db.ActionUsers.Where(u => u.ActionID == actionId).Select(s => s.UserID).ToArray();
            }
            else
                actionDatabase.OwnerIds = db.ActionUsers.Where(u => u.ActionID == actionId).Select(s => s.UserID).ToArray();

            string actionTemplateHtml = GetActionTemplateHTML(actionDatabase.ActionID);
            actionDatabase.ActionTemplateHtml = !string.IsNullOrEmpty(actionTemplateHtml)? actionTemplateHtml : string.Empty;


            if (actionDatabase != null)
                return ConvertToDomain(actionDatabase);
            return null;
        }

        /// <summary>
        /// Finds the by contact.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public IEnumerable<DA.Action> FindByContact(int contactId)
        {
            var db = ObjectContextFactory.Create();
            var actionsSql = @"SELECT A.AccountID AS AccountId, A.ActionID AS Id, A.ActionDetails AS Details, A.CreatedBy, A.CreatedOn, CAM.IsCompleted,DV.DropdownValue AS ActionTypeValue,A.ActionDate,a.ActionStartTime,A.MailBulkId,CAM.GROUPID,      
                                UserName =  STUFF((SELECT ',  ' + (U.FirstName + ' '+ U.LastName)
                                FROM UserActionMap (NOLOCK) UAM 
                                INNER JOIN Users (NOLOCK) U ON U.UserID = UAM.UserID WHERE UAM.ActionID = CAM.ActionID FOR XML PATH('')),1,2,'') FROM ContactActionMap (NOLOCK) CAM
                                INNER JOIN Actions(NOLOCK) A ON A.ActionID = CAM.ActionID
							    INNER JOIN DropdownValues(NOLOCK) DV ON DV.DropdownValueID = A.ActionType
                                WHERE CAM.ContactID = @contactid";
            return db.Get<DA.Action>(actionsSql, new { contactid = contactId }).ToList();
        }

        /// <summary>
        /// Gets the action completed status.
        /// </summary>
        /// <param name="actionId">The action identifier.</param>
        /// <returns></returns>
        public bool GetActionCompletedStatus(int actionId)
        {
            var db = ObjectContextFactory.Create();
            var incompletedactions = db.ActionContacts.Where(p => p.ActionID == actionId && p.IsCompleted == false).Count();
            if (incompletedactions == 0)
                return true;
            else
                return false;

        }

        /// <summary>
        /// Gets the completed actions by ids.
        /// </summary>
        /// <param name="actionIds">The action ids.</param>
        /// <returns></returns>
        public IEnumerable<int> GetCompletedActionsByIds(IEnumerable<int> actionIds)
        {
            var db = ObjectContextFactory.Create();
            var actionContactMaps = db.ActionContacts.Where(p => actionIds.Contains(p.ActionID)).ToList();
            var incompleteActions = actionContactMaps.Where(c => c.IsCompleted == false).Select(c => c.ActionID).Distinct().ToList();
            return actionContactMaps.Where(c => !incompleteActions.Contains(c.ActionID)).Select(c => c.ActionID).ToList();
        }

        /// <summary>
        /// Gets the action contacts status.
        /// </summary>
        /// <param name="actionId">The action identifier.</param>
        /// <param name="contactIds">The contact ids.</param>
        /// <returns></returns>
        public List<DA.ActionContactsSummary> GetActionContactsStatus(int actionId, IEnumerable<int> contactIds,int accountId)
        {
            var db = ObjectContextFactory.Create();
            List<DA.ActionContactsSummary> contactsSummary = new List<DA.ActionContactsSummary>();
            if (contactIds != null && contactIds.Any())
            {
                var contacts = db.Contacts.Where(w => contactIds.Contains(w.ContactID) && w.IsDeleted == false && w.AccountID == accountId).Select(s => new
                {
                    ContactID = s.ContactID,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Company = s.Company,
                    ContactType = s.ContactType,
                    ContactName = s.ContactType == ContactType.Person ? (s.FirstName + " " + s.LastName) : s.Company,
                    PrimaryEmail = db.ContactEmails.Where(cem => cem.ContactID == s.ContactID && cem.IsPrimary == true && cem.IsDeleted == false).Select(se => se.Email).FirstOrDefault() ?? "",
                    PrimaryPhone = db.ContactPhoneNumbers.Where(cp => cp.ContactID == s.ContactID && cp.IsDeleted == false && cp.IsPrimary == true).FirstOrDefault(),
                    Lifecycle = db.DropdownValues.Where(d => d.DropdownValueID == s.LifecycleStage).Select(dr => dr.DropdownValue).FirstOrDefault()
                });
                var contactStatus = db.ActionContacts.Where(a => a.ActionID == actionId && contactIds.Contains(a.ContactID)).Select(s => new { contactId = s.ContactID, status = s.IsCompleted });

                contacts.ForEach(c =>
                  {
                      DA.ActionContactsSummary contactSummary = new DA.ActionContactsSummary();
                      contactSummary.ContactId = c.ContactID;
                      contactSummary.ContactType = c.ContactType;
                      contactSummary.ContactName = c.ContactName;  // c.ContactType == ContactType.Person ? c.FirstName + " "+c.LastName : c.Company;
                      contactSummary.Status = contactStatus.Where(w => w.contactId == c.ContactID).Select(s => s.status).FirstOrDefault() ?? false;
                      contactSummary.PrimaryEmail = c.PrimaryEmail;
                      contactSummary.PrimaryPhone = c.PrimaryPhone != null ? c.PrimaryPhone.PhoneNumber : "";
                      contactSummary.PhoneCountryCode = c.PrimaryPhone != null ? c.PrimaryPhone.CountryCode : "";
                      contactSummary.PhoneExtension = c.PrimaryPhone != null ? c.PrimaryPhone.Extension : "";
                      contactSummary.Lifecycle = c.Lifecycle;
                      contactsSummary.Add(contactSummary);
                  });
            }
            return contactsSummary;
        }

        /// <summary>
        /// Finds the by user.
        /// </summary>
        /// <param name="userID">The user identifier.</param>
        /// <param name="accountID">The account identifier.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="name">The name.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="isdashboard">if set to <c>true</c> [isdashboard].</param>
        /// <param name="StartDate">The start date.</param>
        /// <param name="EndDate">The end date.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        public UserActions FindByUser(int[] userIds, int accountID, int pageNumber, int limit, string name, string filter, string sortField, bool isdashboard, DateTime StartDate, DateTime EndDate, string filterByActionType,ListSortDirection direction = ListSortDirection.Descending)
        {
            int actionTypeId = 0;
            if(!string.IsNullOrEmpty(filterByActionType))
                int.TryParse(filterByActionType, out actionTypeId) ;
            
            byte isCompleted = 0;

            if(!string.IsNullOrEmpty(filter))
            {
                if (filter == "1")
                    isCompleted = 1;
                else if (filter == "2")
                    isCompleted = 2;
            }

            if (sortField == "ActionDateTime")
                sortField = "ActionDate";
            else if (sortField == "Details")
                sortField = "ActionDetails";

            var minDate = (StartDate == null || StartDate == DateTime.MinValue) ? (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue : StartDate;
            var maxDate = (EndDate == null || EndDate == DateTime.MinValue) ? (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue : EndDate;
            string dir = "ASC";
            if (direction == ListSortDirection.Descending) dir = "DESC";
            UserActions userActions = new UserActions();
            var db = ObjectContextFactory.Create();
            
            var parameters = new
            {
                AccountId = accountID,
                Users = userIds.AsTableValuedParameter("dbo.Contact_List"),
                PageNumber = pageNumber,
                PageSize = limit,
                SortColumn = sortField,
                SortDirection = dir,
                SearchBy = name,
                Filter1ForACCompleted = isCompleted,
                Filter2ForACType = actionTypeId,
                StartDate = minDate,
                EndDate = maxDate
            };
            Func<ActionContact, DateTime> ActionDateTime = (ac) =>
               {
                   DateTime date = ac.ActionDate;
                   DateTime endtime = ac.ActionStartTime;
                   DateTime Datevalue = new DateTime(date.Year, date.Month, date.Day, endtime.Hour, endtime.Minute, endtime.Second);
                   return Datevalue;
               };

            Func<ActionContact, string> ContactName = (ac) =>
            {
                string contactName = string.Empty;
                if(ac.TotalActionContacts > 1)
                {
                    if (ac.ContactType == ContactType.Person)
                        contactName = (!string.IsNullOrEmpty(ac.FirstName) && !string.IsNullOrEmpty(ac.LastName)) ? ac.FirstName + " " + ac.LastName + "..." : ac.Email + "...";
                    else
                        contactName = !string.IsNullOrEmpty(ac.Company) ? ac.Company + "..." : ac.Email + "...";
                }
                else
                {
                    if (ac.ContactType == ContactType.Person)
                        contactName = (!string.IsNullOrEmpty(ac.FirstName) && !string.IsNullOrEmpty(ac.LastName)) ? ac.FirstName + " " + ac.LastName : ac.Email;
                    else
                        contactName = !string.IsNullOrEmpty(ac.Company) ? ac.Company : ac.Email;
                }

                return contactName;
            };

            List<ActionContact> actionContacts = new List<ActionContact>();

            db.QueryStoredProc("[dbo].[Get_User_Created_Actions_List]", (r) =>
             {
                 actionContacts = r.Read<ActionContact>().ToList();
             }, parameters);

            var actions = actionContacts.IsAny()? actionContacts.Select(ac =>
                new DA.Action()
                {
                    Id = ac.ActionId,
                    Details = ac.ActionDetails,
                    RemindOn = ac.RemindOn,
                    IsCompleted = ac.IsCompleted,
                    CreatedOn = ac.CreatedOn,
                    ActionTypeValue = ac.ActionTypeValue,
                    ActionDateTime = ActionDateTime(ac),
                    ContactName = ContactName(ac),
                   // Contacts = RawContact(ac).ToList(),
                    ContactId = ac.ContactId,
                    ContactType = (byte)ac.ContactType,
                    UserName = ac.UserName,
                    ActionType = ac.ActionTypeId,
                    TotalCount = ac.TotalCount
                }): new List<DA.Action>() { };

            //if (StartDate != DateTime.MinValue && StartDate != null && EndDate != null && EndDate != DateTime.MinValue)
            //    actions = actions.Where(i => i.CreatedOn.Date >= StartDate.Date && i.CreatedOn.Date <= EndDate.Date);
            //if (!string.IsNullOrEmpty(name))
            //    actions = actions.Where(a => a.Details.Contains(name));

            //if (filter == "1")
            //    actions = actions.Where(a => a.IsCompleted == true);
            //else if (filter == "2" || filter == "" || filter == null)
            //    actions = actions.Where(a => a.IsCompleted == false);

            //if (!string.IsNullOrEmpty(filterByActionType))
            //    actions = actions.Where(a => a.ActionType == actionTypeId);

            userActions.TotalCount = actions.IsAny()?actions.Select(t => t.TotalCount).FirstOrDefault():0;

            userActions.Actions = actions;
            return userActions;
        }

        /// <summary>
        /// Finds the by user.
        /// </summary>
        /// <param name="userID">The user identifier.</param>
        /// <param name="accountID">The account identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        public IEnumerable<DA.Action> FindByUser(int? userID, int accountID, string name, string filter, string sortField, ListSortDirection direction = ListSortDirection.Descending)
        {
            var db = ObjectContextFactory.Create();
            var predicate = PredicateBuilder.True<ActionsDb>();
            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                predicate = predicate.And(a => a.ActionDetails.Contains(name));
            }
            predicate = predicate.And(a => a.CreatedBy == userID);
            var userCreatedActions = db.Actions.Where(p => p.AccountID == accountID).AsExpandable().Where(predicate).OrderByDescending(c => c.CreatedOn).ToList();
            IEnumerable<DA.Action> actions = Mapper.Map<IEnumerable<ActionsDb>, IEnumerable<DA.Action>>(userCreatedActions);
            return actions;
        }

        /// <summary>
        /// Finds the by opportunity.
        /// </summary>
        /// <param name="opportunityId">The opportunity identifier.</param>
        /// <returns></returns>
        public IEnumerable<DA.Action> FindByOpportunity(int opportunityId)
        {
            var db = ObjectContextFactory.Create();
            var actionsSql = @"SELECT A.AccountID AS AccountId, A.ActionID AS Id, A.ActionDetails AS Details, A.CreatedBy, A.CreatedOn, CAM.IsCompleted,DV.DropdownValue AS ActionTypeValue,A.ActionDate,A.MailBulkId, 
                               UserName =  STUFF((SELECT ',  ' + (U.FirstName + ' '+ U.LastName)
                               FROM UserActionMap (NOLOCK) UAM 
                               INNER JOIN Users (NOLOCK) U ON U.UserID = UAM.UserID WHERE UAM.ActionID = CAM.ActionID FOR XML PATH('')),1,2,'') FROM OpportunityActionMap (NOLOCK) CAM
	                           JOIN Actions(NOLOCK) A ON A.ActionID = CAM.ActionID
	                           INNER JOIN DropdownValues(NOLOCK) DV ON DV.DropdownValueID = A.ActionType
	                           WHERE CAM.OpportunityID = @OpportunityId";
            return db.Get<DA.Action>(actionsSql, new { OpportunityId = opportunityId }).ToList();
            //var db = ObjectContextFactory.Create();
            //var contactActions = db.OpportunityActionMap.Include(a => a.Action).Where(c => c.OpportunityID == opportunityId).Select(a => new { Action = a.Action, IsCompleted = a.IsCompleted }).ToList();


            //if (contactActions != null)
            //{
            //    foreach (var item in contactActions)
            //    {
            //        var action = FindBy(item.Action.ActionID);
            //        action.IsCompleted = item.IsCompleted;
            //        yield return action;
            //    }
            //}
        }

        /// <summary>
        /// Actions the completed.
        /// </summary>
        /// <param name="actionId">The action identifier.</param>
        /// <param name="status">if set to <c>true</c> [status].</param>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="opportunityId">The opportunity identifier.</param>
        /// <param name="completedForAll">if set to <c>true</c> [completed for all].</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="updatedOn">The updated on.</param>
        public List<LastTouchedDetails> ActionCompleted(int actionId, bool status, int? contactId, int opportunityId, bool completedForAll, int userId, DateTime updatedOn,bool isScheduled,int? mailBulkId)
        {
            var db = ObjectContextFactory.Create();
            var contactAction = new List<ContactActionMapDb>();
            List<LastTouchedDetails> lastTouchedList = new List<LastTouchedDetails>();
            Guid guid = Guid.NewGuid();
            if (!completedForAll)
            {
               
                contactAction = db.ActionContacts.Where(c => c.ActionID == actionId && c.ContactID == contactId && c.IsCompleted != status ).ToList();

                if (isScheduled == true)
                    InsertActionsMailOperation(actionId, status == true ? false : true, 1, mailBulkId.HasValue ? mailBulkId.Value:0, guid);

                    contactAction.ForEach(f =>
                    {
                        f.IsCompleted = status;
                        f.LastUpdatedBy = userId;
                        f.LastUpdatedOn = updatedOn;
                        f.GroupID = guid;
                    });
              
                LastTouchedDetails lastTouchedDetails = new LastTouchedDetails();
                lastTouchedDetails.ContactID = (int)contactId;
                lastTouchedDetails.LastTouchedDate = DateTime.UtcNow;
                lastTouchedDetails.ActionID = actionId;
                lastTouchedList.Add(lastTouchedDetails);

            }
            else if (opportunityId != 0 || completedForAll == true)
            {
                
                contactAction = db.ActionContacts.Where(c => c.ActionID == actionId && c.IsCompleted != status).ToList();

                if (isScheduled == true)
                    InsertActionsMailOperation(actionId, status == true ? false : true, 1, mailBulkId.HasValue ? mailBulkId.Value : 0, guid);


                foreach (ContactActionMapDb opportunityContact in contactAction)
                {
                    opportunityContact.IsCompleted = status;
                    opportunityContact.LastUpdatedBy = userId;
                    opportunityContact.LastUpdatedOn = updatedOn;
                    opportunityContact.GroupID = guid;

                    LastTouchedDetails lastTouchedDetails = new LastTouchedDetails();
                    lastTouchedDetails.ContactID = opportunityContact.ContactID;
                    lastTouchedDetails.LastTouchedDate = DateTime.UtcNow;
                    lastTouchedDetails.ActionID = actionId;
                    lastTouchedList.Add(lastTouchedDetails);

                }

            }
      

            var opportunityAction = db.OpportunityActionMap.Where(c => c.ActionID == actionId && c.OpportunityID == opportunityId).SingleOrDefault();
            if (opportunityAction != null)
                opportunityAction.IsCompleted = status;

            db.SaveChanges();

            return lastTouchedList;
        }

        /// <summary>
        /// Actionses the marked complete.
        /// </summary>
        /// <param name="actionIds">The action ids.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="updatedOn">The updated on.</param>
        public List<LastTouchedDetails> ActionsMarkedComplete(int[] actionIds, int userId, DateTime updatedOn,bool isScheduled)
        {
            var db = ObjectContextFactory.Create();
            Guid groupId = Guid.NewGuid();
            if (isScheduled == true)
                UpdateActionsSheduledEmails(actionIds, true, groupId);

            List<LastTouchedDetails> lastTouchedList = new List<LastTouchedDetails>();
            var contactAction = db.ActionContacts.Where(c => actionIds.Contains(c.ActionID) && c.IsCompleted == false).ToList();
            contactAction.ForEach(f =>
            {
                f.IsCompleted = true;
                f.LastUpdatedBy = userId;
                f.LastUpdatedOn = updatedOn;
                if (isScheduled == true)
                    f.GroupID = groupId;


                LastTouchedDetails lastTouchedDetails = new LastTouchedDetails();
                lastTouchedDetails.ContactID = f.ContactID;
                lastTouchedDetails.LastTouchedDate = DateTime.UtcNow;
                lastTouchedDetails.ActionID = f.ActionID;
                lastTouchedList.Add(lastTouchedDetails);
            });

            db.SaveChanges();
            return lastTouchedList;
        }

        /// <summary>
        /// Actionses the marked in complete.
        /// </summary>
        /// <param name="actionIds">The action ids.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="updatedOn">The updated on.</param>
        public void ActionsMarkedInComplete(int[] actionIds, int userId, DateTime updatedOn, bool isScheduled)
        {
            var db = ObjectContextFactory.Create();
            Guid groupId = Guid.NewGuid();
            if (isScheduled == true)
                UpdateActionsSheduledEmails(actionIds, false, groupId);

            var contactAction = db.ActionContacts.Where(c => actionIds.Contains(c.ActionID) && c.IsCompleted == true).ToList();
            contactAction.ForEach(f =>
            {
                f.IsCompleted = false;
                f.LastUpdatedBy = userId;
                f.LastUpdatedOn = updatedOn;
                if (isScheduled == true)
                    f.GroupID = groupId;
            });

            db.SaveChanges();
        }

        /// <summary>
        /// Deletes the action for all.
        /// </summary>
        /// <param name="actionId">The action identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public Dictionary<int, Guid?> DeleteActionForAll(int actionId, int userId)
        {
            var db = ObjectContextFactory.Create();
            var actiondb = db.Actions.Where(n => n.ActionID == actionId).FirstOrDefault();
            Dictionary<int, Guid?> guids = new Dictionary<int, Guid?>();
            if (actiondb != null)
            {
                guids.Add(1, actiondb.EmailRequestGuid);
                guids.Add(2, actiondb.TextRequestGuid);
            }
            //Guid? emailRequestGuid = actiondb.EmailRequestGuid;
            var actionOpportunity = db.OpportunityActionMap.Where(n => n.ActionID == actionId).FirstOrDefault();

            var actionContacts = db.ActionContacts.Where(n => n.ActionID == actionId);
            var actionTags = db.ActionTags.Where(n => n.ActionID == actionId);
            var userActions = db.ActionUsers.Where(a => a.ActionID == actionId);
            if (actionContacts != null)
                db.ActionContacts.RemoveRange(actionContacts);
            if (actionTags != null)
                db.ActionTags.RemoveRange(actionTags);
            if (userActions != null)
                db.ActionUsers.RemoveRange(userActions);
            if (actionOpportunity != null)
                db.OpportunityActionMap.Remove(actionOpportunity);
            if (actiondb != null)
                db.Actions.Remove(actiondb);
            var outlookCRMSyncEntity = db.CRMOutlookSync.Where(c => c.EntityID == actionId && c.EntityType == (short)AppModules.ContactActions).FirstOrDefault();
            if (outlookCRMSyncEntity != null)
            {
                outlookCRMSyncEntity.SyncStatus = (short)OutlookSyncStatus.Deleted;
                db.Entry<CRMOutlookSyncDb>(outlookCRMSyncEntity).State = EntityState.Modified;
            }
            db.SaveChanges();

            // var tourauditdb1 = db.Tours_Audit.Where(p => p.TourID == tourId).ToList();
            var actionauditdb = db.Actions_Audit.Where(p => p.ActionID == actionId && p.AuditAction == "D").FirstOrDefault();
            if (actionauditdb != null)
            {
                actionauditdb.LastUpdatedBy = userId;
                db.SaveChanges();
            }

            return guids;
        }

        /// <summary>
        /// Deletes the action.
        /// </summary>
        /// <param name="actionId">The action identifier.</param>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public Dictionary<int, Guid?> DeleteAction(int actionId, int contactId, int userId)
        {
            var db = ObjectContextFactory.Create();
            var actiondb = db.Actions.Where(a => a.ActionID == actionId).FirstOrDefault();
            var action = db.ActionContacts.Where(a => a.ActionID == actionId);
            var actionOpportunity = db.OpportunityActionMap.Where(n => n.ActionID == actionId);
            var actionTags = db.ActionTags.Where(n => n.ActionID == actionId);
            var userActionDb = db.ActionUsers.Where(ac => ac.ActionID == actionId).ToList();
            Dictionary<int, Guid?> guids = new Dictionary<int, Guid?>();
            guids.Add(1, actiondb.EmailRequestGuid);
            guids.Add(2, actiondb.TextRequestGuid);
            //Guid? emailRequestGuid = actiondb.FirstOrDefault().EmailRequestGuid;
            var contactActions = db.ActionContacts.Where(a => a.ActionID == actionId && a.ContactID == contactId);
            if (contactId != 0 && contactActions.Count() == 1 && contactActions != null)
            {
                db.ActionContacts.RemoveRange(contactActions);
                if (action.Count() == 1)
                {
                    db.ActionTags.RemoveRange(actionTags);
                    db.ActionUsers.RemoveRange(userActionDb);
                    db.Actions.Remove(actiondb);
                }
                
            }
            if (contactId == 0)
            {
               
                db.OpportunityActionMap.RemoveRange(actionOpportunity);
                db.ActionTags.RemoveRange(actionTags);
                db.ActionContacts.RemoveRange(action);
                db.ActionUsers.RemoveRange(userActionDb);
                db.Actions.Remove(actiondb);
                var outlookCRMSyncEntity = db.CRMOutlookSync.Where(c => c.EntityID == actionId && c.EntityType == (short)AppModules.ContactActions).FirstOrDefault();
                if (outlookCRMSyncEntity != null)
                {
                    outlookCRMSyncEntity.SyncStatus = (short)OutlookSyncStatus.Deleted;
                    db.Entry<CRMOutlookSyncDb>(outlookCRMSyncEntity).State = EntityState.Modified;
                }
            }

            db.SaveChanges();

            // var tourauditdb1 = db.Tours_Audit.Where(p => p.TourID == tourId).ToList();
            var actionauditdb = db.Actions_Audit.Where(p => p.ActionID == actionId && p.AuditAction == "D").FirstOrDefault();
            if (actionauditdb != null)
            {
                actionauditdb.LastUpdatedBy = userId;
                db.SaveChanges();
            }

            return guids;
        }

        /// <summary>
        /// Contactses the count.
        /// </summary>
        /// <param name="actionId">The action identifier.</param>
        /// <returns></returns>
        public int ContactsCount(int actionId)
        {
            var db = ObjectContextFactory.Create();
            int contactsCount = db.ActionContacts.Where(a => a.ActionID == actionId).Select(a => a.Contact).Count();
            return contactsCount;
        }

        public bool IsActionFromSelectAll(int actionId)
        {
            var db = ObjectContextFactory.Create();
            bool? selectAll = db.Actions.Where(a => a.ActionID == actionId).Select(a => a.SelectAll).FirstOrDefault();
            return selectAll ?? false;
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="actionDb">The action database.</param>
        /// <returns></returns>
        public override DA.Action ConvertToDomain(ActionsDb actionDb)
        {
            return Mapper.Map<ActionsDb, DA.Action>(actionDb);
        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Invalid action id has been passed. Suspected Id forgery.</exception>
        public override ActionsDb ConvertToDatabaseType(DA.Action action, CRMDb context)
        {
            ActionsDb actionsDb;

            if (action.Id > 0)
            {
                actionsDb = context.Actions.SingleOrDefault(c => c.ActionID == action.Id);

                /* Below code is not required as the loading of contacts and tags has been taken care in the persist value object methods.
                var tags = context.ActionTags.Include(a => a.Tag).Where(a => at.ActionID == action.Id).Select(a=>a.Tag).ToList();
                actionsDb.Tags = tags;

                var contacts = context.ActionContacts.Include(a => a.Contact).Where(a => a.ActionID == action.Id).Select(a=>a.Contact).ToList();
                actionsDb.Contacts = contacts;
                */

                if (actionsDb == null)
                    throw new ArgumentException("Invalid action id has been passed. Suspected Id forgery.");
                actionsDb = Mapper.Map<DA.Action, ActionsDb>(action, actionsDb);

            }
            else
            {
                actionsDb = Mapper.Map<DA.Action, ActionsDb>(action);
            }
            return actionsDb;
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="db">The database.</param>
        public override void PersistValueObjects(DA.Action domainType, ActionsDb dbType, CRMDb db)
        {

            PersistActionContacts(domainType, dbType, db);
            PersistActionTags(domainType, dbType, db);
            PersistActionUsers(domainType, dbType, db);

        }
        /// <summary>
        /// Persists the action outlook synchronize.
        /// </summary>
        /// <param name="action">The action.</param>
        public void UpdateCRMOutlookMap(DA.Action action, RequestOrigin? requestedFrom)
        {
            var db = ObjectContextFactory.Create();
            CRMOutlookSyncDb outlookSyncDb;

            if (action.Id > 0)
            {
                var tourCurrentSyncStatus = requestedFrom != RequestOrigin.Outlook ? (short)OutlookSyncStatus.NotInSync : (short)OutlookSyncStatus.InSync;

                outlookSyncDb = db.CRMOutlookSync.Where(o => o.EntityID == action.Id && o.EntityType == (byte)AppModules.ContactActions).FirstOrDefault();
                if (outlookSyncDb != null)
                {
                    outlookSyncDb.SyncStatus = tourCurrentSyncStatus;
                }
                else
                {
                    outlookSyncDb = new CRMOutlookSyncDb()
                    {
                        EntityID = action.Id,
                        SyncStatus = tourCurrentSyncStatus,
                        LastSyncDate = action.LastUpdatedOn,
                        LastSyncedBy = action.LastUpdatedBy,
                        EntityType = (byte)AppModules.ContactActions
                    };
                    db.Entry<CRMOutlookSyncDb>(outlookSyncDb).State = EntityState.Added;
                }
            }
            db.SaveChanges();
        }
        /// <summary>
        /// Persists the action tags.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="actionDb">The action database.</param>
        /// <param name="db">The database.</param>
        private void PersistActionTags(DA.Action action, ActionsDb actionDb, CRMDb db)
        {
            //var actionTags = db.ActionTags.Where(a => a.ActionID == action.Id).ToList();
            var actionTagsDb = new List<ActionTagsMapDb>();

            if (action.Tags != null)
            {
                foreach (Tag tag in action.Tags)
                {
                    var actiontag = db.ActionTags.Where(p => p.ActionID == action.Id && p.TagID == tag.Id).FirstOrDefault();

                    if (tag.Id == 0)
                    {
                        var tagDb = db.Tags.SingleOrDefault(t => t.TagName.Equals(tag.TagName) && t.AccountID == tag.AccountID && t.IsDeleted != true);
                        if (tagDb == null)
                        {
                            tagDb = Mapper.Map<Tag, TagsDb>(tag);
                            tagDb.IsDeleted = false;
                            tagDb = db.Tags.Add(tagDb);
                        }
                        var actionTag = new ActionTagsMapDb()
                        {
                            Action = actionDb,
                            Tag = tagDb
                        };
                        actionTagsDb.Add(actionTag);
                    }
                    else if (actiontag == null)
                    {
                        actionTagsDb.Add(new ActionTagsMapDb() { ActionID = actionDb.ActionID, TagID = tag.Id });
                        db.RefreshAnalytics.Add(new RefreshAnalyticsDb() { EntityID = tag.Id, EntityType = 5, Status = 1, LastModifiedOn = DateTime.Now.ToUniversalTime() });
                    }
                }
            }
            db.ActionTags.AddRange(actionTagsDb);

            IList<int> tagIds = action.Tags.Where(a => a.Id > 0).Select(a => a.Id).ToList();
            var unMapActionTags = db.ActionTags.Where(a => !tagIds.Contains(a.TagID) && a.ActionID == action.Id);
            this.ScheduleAnalyticsRefreshForTags(unMapActionTags,db);
            db.ActionTags.RemoveRange(unMapActionTags);

        }

        private void ScheduleAnalyticsRefreshForTags(IEnumerable<ActionTagsMapDb> tags,CRMDb db)
        {
            List<RefreshAnalyticsDb> analytics = new List<RefreshAnalyticsDb>();
            if (tags.IsAny())
            {
                foreach (var tag in tags)
                {
                    RefreshAnalyticsDb refreshAnalytics = new RefreshAnalyticsDb();
                    refreshAnalytics.EntityID = tag.TagID;
                    refreshAnalytics.EntityType = 5;
                    refreshAnalytics.Status = 1;
                    refreshAnalytics.LastModifiedOn = DateTime.Now.ToUniversalTime();
                    analytics.Add(refreshAnalytics);
                }
                db.RefreshAnalytics.AddRange(analytics);
            }
        }

        /// <summary>
        /// Persists the action contacts.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="actionDb">The action database.</param>
        /// <param name="db">The database.</param>
        void PersistActionContacts(DA.Action action, ActionsDb actionDb, CRMDb db)
        {
            //var actionContacts = db.ActionContacts.Where(a => a.ActionID == action.Id).ToList();
            var actionOpportunity = db.OpportunityActionMap.Where(a => a.ActionID == action.Id).FirstOrDefault();

            if (action.OppurtunityId != 0 && actionOpportunity == null)
            {
                db.OpportunityActionMap.Add(new OpportunityActionMap() { OpportunityID = action.OppurtunityId, ActionID = actionDb.ActionID, IsCompleted = action.MarkAsCompleted == true ? true : false });
            }

            if (action.SelectAll == false)
            {
                var actionContactDb = new List<ContactActionMapDb>();
                //string ContactIds = string.Join(",", action.Contacts.Select(p => p.ContactID).ToArray());

                var sql = @"select cam.contactid from ContactActionMap cam where ActionID = @ActionId";

                // var sql = @"select  CAST(r.datavalue AS INTEGER)  from (select * from ContactActionMap cam right outer join dbo.Split(@ContactIds,',') split on split.DataValue = cam.contactid 
                //and cam.actionid = @ActionId) r where r.actionid is null";
                var dbData = ObjectContextFactory.Create();
                var actionContacts = dbData.Get<int>(sql, new { ActionId = action.Id });

                if (action.ContactIDS != null && action.ContactIDS.Any())
                    actionContacts = action.ContactIDS;
                else
                    actionContacts = action.Contacts.Select(p => p.ContactID).ToArray().Except(actionContacts);

                foreach (int contactId in actionContacts)
                {
                    actionContactDb.Add(new ContactActionMapDb() { ActionID = actionDb.ActionID, ContactID = contactId, IsCompleted = action.MarkAsCompleted == true ? true : false, LastUpdatedOn = action.CreatedOn, LastUpdatedBy = action.CreatedBy });
                }
                db.ActionContacts.AddRange(actionContactDb);
                IList<int> contactIds = action.Contacts.Where(a => a.ContactID > 0).Select(a => a.ContactID).ToList();
                var unMapActionContacts = db.ActionContacts.Where(a => !contactIds.Contains(a.ContactID) && a.ActionID == action.Id);

                db.ActionContacts.RemoveRange(unMapActionContacts);
            }
        }

        /// <summary>
        /// Persists the action users.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="actionDb">The action database.</param>
        /// <param name="db">The database.</param>
        public void PersistActionUsers(DA.Action action, ActionsDb actionDb, CRMDb db)
        {
            var actionUserDb = new List<UserActionMapDb>();
            var sql = @"SELECT UAM.UserID  FROM UserActionMap (NOLOCK) UAM WHERE UAM.ActionID = @actionId";
            var dbData = ObjectContextFactory.Create();
            var actionContacts = dbData.Get<int>(sql, new { actionId = action.Id });
            actionContacts = action.OwnerIds.Except(actionContacts);
            foreach (int userId in actionContacts)
            {
                actionUserDb.Add(new UserActionMapDb()
                {
                    ActionID = actionDb.ActionID,
                    UserID = userId,
                    LastUpdatedOn = action.CreatedOn,
                    LastUpdatedBy = action.CreatedBy,
                    UserEmailGuid = (action.EmailGuids != null && action.EmailGuids.Any()) ? action.EmailGuids.Where(s => s.Key == userId).Select(v => v.Value).FirstOrDefault() : new Guid(),
                    UserTextGuid = (action.TextGuids != null && action.TextGuids.Any()) ? action.TextGuids.Where(s => s.Key == userId).Select(v => v.Value).FirstOrDefault() : new Guid()
                });
            }
            db.ActionUsers.AddRange(actionUserDb);
            IList<int> userIds = action.OwnerIds;
            var unMapActionUsers = db.ActionUsers.Where(a => !userIds.Contains(a.UserID) && a.ActionID == action.Id);
            db.ActionUsers.RemoveRange(unMapActionUsers);
        }

        /// <summary>
        /// Gets the new actions to synchronize.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="maxNumRecords">The maximum number records.</param>
        /// <param name="timeStamp">The time stamp.</param>
        /// <param name="firstSync">if set to <c>true</c> [first synchronize].</param>
        /// <param name="operationType">Type of the operation.</param>
        /// <returns></returns>
        public IEnumerable<DA.Action> GetNewActionsToSync(int accountId, int userId, int? maxNumRecords, DateTime? timeStamp, bool firstSync, CRUDOperationType operationType)
        {
            Logger.Current.Verbose("ActionRepository/GetActionsToSync, parameters:  " + accountId + ", " + userId + ", " + maxNumRecords + ", " + timeStamp + ", " + firstSync);
            var db = ObjectContextFactory.Create();
            if (firstSync)
            {
                IList<int> actionIds = db.CRMOutlookSync
                    .Join(db.Actions,
                        crm => crm.EntityID,
                        action => action.ActionID,
                        (crm, action) => new { CRMOutlookSyncDb = crm, ActionsDb = action })
                    .Where(c => c.CRMOutlookSyncDb.EntityType == (byte)AppModules.ContactActions
                        && c.ActionsDb.AccountID == accountId
                        && c.ActionsDb.CreatedBy == userId)
                    .Select(c => c.CRMOutlookSyncDb.EntityID).ToList();

                if (operationType == CRUDOperationType.Create)
                {
                    db.CRMOutlookSync.Where(c => actionIds.Contains(c.EntityID)
                        && c.EntityType == (byte)AppModules.ContactActions
                        && c.SyncStatus == (short)OutlookSyncStatus.Syncing)
                        .ForEach(c =>
                        {
                            c.SyncStatus = (short)OutlookSyncStatus.NotInSync;
                        });
                }

                var firstTimeSyncActions = db.Actions
                    .Where(c => c.CreatedBy == userId && !actionIds.Contains(c.ActionID))
                    .Select(c => c.ActionID);

                foreach (int id in firstTimeSyncActions)
                {
                    CRMOutlookSyncDb actionOutlookSyncDb = new CRMOutlookSyncDb();
                    actionOutlookSyncDb.EntityID = id;
                    actionOutlookSyncDb.SyncStatus = (short)OutlookSyncStatus.NotInSync;
                    actionOutlookSyncDb.EntityType = (byte)AppModules.ContactActions;
                    db.Entry<CRMOutlookSyncDb>(actionOutlookSyncDb).State = System.Data.Entity.EntityState.Added;
                }
                db.SaveChanges();
                Logger.Current.Verbose("GetActionsToSync() / firstSync setup completed");
            }

            var actionIdsToSync = db.CRMOutlookSync.Where(c => c.SyncStatus == (short)OutlookSyncStatus.NotInSync
                 && c.EntityType == (byte)AppModules.ContactActions
                 && string.IsNullOrEmpty(c.OutlookKey))
                .Select(c => c.EntityID);

            var actionsToSyncDb = db.Actions
                .Where(c => c.AccountID == accountId
                    && c.CreatedBy == userId
                    && actionIdsToSync.Contains(c.ActionID))
                .Take((int)maxNumRecords).ToList();

            var actionsToSync = Mapper.Map<IEnumerable<ActionsDb>, IEnumerable<DA.Action>>(actionsToSyncDb);

            return actionsToSync;
        }

        /// <summary>
        /// Gets the modified actions to synchronize.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="maxNumRecords">The maximum number records.</param>
        /// <param name="timeStamp">The time stamp.</param>
        /// <param name="firstSync">if set to <c>true</c> [first synchronize].</param>
        /// <returns></returns>
        public IEnumerable<DA.Action> GetModifiedActionsToSync(int accountId, int userId, int? maxNumRecords, DateTime? timeStamp, bool firstSync)
        {
            Logger.Current.Verbose("ActionRepository/GetActionsToSync, parameters:  " + accountId + ", " + userId + ", " + maxNumRecords + ", " + timeStamp + ", " + firstSync);
            var db = ObjectContextFactory.Create();

            var actionIdsToSync = db.CRMOutlookSync.Where(c => c.SyncStatus == (short)OutlookSyncStatus.NotInSync
                 && c.EntityType == (byte)AppModules.ContactActions && !string.IsNullOrEmpty(c.OutlookKey))
                .Select(c => c.EntityID);

            var actionsToSyncDb = db.Actions
                .Where(c => c.AccountID == accountId && c.CreatedBy == userId && actionIdsToSync.Contains(c.ActionID))
                .Take((int)maxNumRecords).ToList();

            var actionsToSync = Mapper.Map<IEnumerable<ActionsDb>, IEnumerable<DA.Action>>(actionsToSyncDb);

            return actionsToSync;
        }

        /// <summary>
        /// Gets the deleted tasks to synchronize.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="maxNumRecords">The maximum number records.</param>
        /// <param name="timeStamp">The time stamp.</param>
        /// <returns></returns>
        public IEnumerable<int> GetDeletedTasksToSync(int accountId, int userId, int? maxNumRecords, DateTime? timeStamp)
        {
            var db = ObjectContextFactory.Create();

            var actionIds = db.CRMOutlookSync.Where(c =>
                   c.EntityType == (byte)AppModules.ContactActions
                   && c.SyncStatus == (short)OutlookSyncStatus.Deleted
                   && c.User.AccountID == accountId).Select(c => c.EntityID);

            return actionIds;
        }

        /// <summary>
        /// Gets the Assigned User ids 
        /// </summary>
        /// <param name="actionId">The action Id</param>
        /// <returns></returns>
        public IList<int> GetAllOwnerIds(int actionId)
        {
            var db = ObjectContextFactory.Create();
            IList<int> ownerIds = null;
            ownerIds = db.ActionUsers.Where(a => a.ActionID == actionId).Select(s => s.UserID).ToArray();
            return ownerIds;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tourId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Guid GetUserEmailGuid(int actionId, int userId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT UserEmailGuid  FROM UserActionMap (NOLOCK) WHERE ActionID=@actionID AND UserID=@userID";
            Guid userEmailGuid = db.Get<Guid>(sql, new { actionID = actionId, userID = userId }).FirstOrDefault();
            return userEmailGuid;
        }

        public Guid GetUserTextGuid(int actionId, int userId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT UserTextGuid   FROM UserActionMap (NOLOCK) WHERE ActionID=@actionID AND UserID=@userID";
            Guid userTextGuid = db.Get<Guid>(sql, new { actionID = actionId, userID = userId }).FirstOrDefault();
            return userTextGuid;
        }

        public IEnumerable<Guid> GetUserEmailGuids(int actionId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT UserEmailGuid  FROM UserActionMap (NOLOCK) WHERE ActionID=@actionID";
            IEnumerable<Guid> emailGuids = db.Get<Guid>(sql, new { actionID = actionId }).ToList();
            return emailGuids;
        }

        public IEnumerable<Guid> GetUserTextGuids(int actionId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT UserTextGuid  FROM UserActionMap (NOLOCK) WHERE ActionID=@actionID ";
            IEnumerable<Guid> textGuids = db.Get<Guid>(sql, new { actionID = actionId }).ToList();
            return textGuids;
        }

        public int InsertBulkMailOperationDetails(string subject, string actionTemplateHtml,int actionID,string from)
        {

            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"INSERT INTO MailBulkOperations([From],[To],CC,BCC,[Subject],Body) VALUES(@from,NULL,NULL,NULL,@subject,@body)
                            SELECT SCOPE_IDENTITY()";
                return db.Get<int>(sql, new { from = from, subject = subject, body = actionTemplateHtml }).FirstOrDefault();
            }
        }

        public void InsertActionsMailOperationDetails(List<ActionsMailOperation> actionsmailOperations)
        {
            using (var db = ObjectContextFactory.Create())
            {
                List<ActionsMailOperationDb> actionMailOperationDb = Mapper.Map<IEnumerable<ActionsMailOperation>, IEnumerable<ActionsMailOperationDb>>(actionsmailOperations).ToList();
                db.BulkInsert<ActionsMailOperationDb>(actionMailOperationDb);
                db.SaveChanges();
            }
        }

        public void DeleteDateFromBulkMailDetails(int mailBulkId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"DELETE FROM ActionsMailOperations WHERE MailBulkOperationID=@MailbulkId
	                        DELETE FROM MailBulkOperations WHERE MailBulkOperationID=@MailbulkId";
               db.Execute(sql, new { MailbulkId = mailBulkId });
            }
        }

        public string GetActionTemplateHTML(int actionID)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"SELECT MBO.Body FROM MailBulkOperations(nolock) MBO
                            JOIN Actions(nolock) A ON A.MailBulkId = MBO.MailBulkOperationID
                            WHERE A.ActionID=@actionId";
                 return  db.Get<string>(sql, new { actionId = actionID }).FirstOrDefault();
            }
        }

        public IEnumerable<int> GetContactIds(int actionId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT ContactID from ContactActionMap (NOLOCK) where ActionID = @actionID";
            IEnumerable<int> contactIds = db.Get<int>(sql, new { actionID = actionId }).ToList();
            return contactIds;
        } 

        public void UpdateActionsSheduledEmails(int[] actionIds, bool markAsComplete,Guid groupId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                db.QueryStoredProc("[dbo].[Scheduling_Action_Mails]", r => { }, new
                {
                    ActionIds = string.Join(",", actionIds),
                    MarkAsComplete = markAsComplete,
                    @GroupId = groupId
                });
            }
        }

        public void UpdateActionSheduleEmailBody(int actionId, int mailBulkId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"UPDATE A 
                            SET 
                            A.MailBulkId=@mailBulkId
                            FROM ACTIONS A WHERE A.ActionID=@actionId";
                db.Execute(sql, new { mailBulkId = mailBulkId, actionId = actionId });
            }
        }

        public void InsertActionsMailOperation(int actionId,bool isSchedule,byte isProcessed,int mailBulkId,Guid guid)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"INSERT INTO ActionsMailOperations VALUES(@ActionId,@IsScheduled,@IsProcessed,@MailBulkId,@guid)";
                db.Execute(sql, new { ActionId = actionId, IsScheduled = isSchedule, IsProcessed= isProcessed, MailBulkId = mailBulkId, guid = guid });
            }
        }

        public void UpdateActionContactsWithGroupId(int actionId, Guid groupId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"UPDATE CA
                            SET
                            CA.GroupID=@GroupId
                            FROM ContactActionMap CA WHERE CA.ActionID = @ActionId";
                db.Execute(sql, new { GroupId = groupId, ActionId = actionId });
            }
        }

        public string GetActionTypeValueById(short actionType)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT DropdownValue FROM DropdownValues(NOLOCK) WHERE DropdownValueID=@DropdownValueId";
            return db.Get<string>(sql, new { DropdownValueId = actionType }).FirstOrDefault();
        }

        public void ActionDetailsAddingToNoteSummary(List<int> actionIds, List<int> contactIds, int accountId, int userId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                db.QueryStoredProc("[dbo].[ActionDetailsAddingToNoteSummary]", (r) => { },
                new
                {
                    ActionIds = actionIds.AsTableValuedParameter("dbo.Contact_List"),
                    ContactIds = contactIds.AsTableValuedParameter("dbo.Contact_List"),
                    AccountID = accountId,
                    OwnerID = userId
                });
            }
        }

        public string GetActionTypeById(short actionType)
        {
            using(var db = ObjectContextFactory.Create())
            {
                var sql = @"SELECT DropdownValue FROM DropdownValues(NOLOCK) WHERE DropdownValueID=@ID";
                return db.Get<string>(sql, new { ID = actionType }).FirstOrDefault();
            }
        }
    }
}
