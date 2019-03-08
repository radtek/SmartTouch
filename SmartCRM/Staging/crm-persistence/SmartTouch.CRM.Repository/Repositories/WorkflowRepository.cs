using AutoMapper;
using LinqKit;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Domain.Search;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using LandmarkIT.Enterprise.Extensions;
using System.Data;
using System.Data.Common;
using LandmarkIT.Enterprise.Utilities.Caching;
using LandmarkIT.Enterprise.Utilities.Logging;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class WorkflowRepository : Repository<Workflow, short, WorkflowsDb>, IWorkflowRepository
    {

        public WorkflowRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory)
        { }

        /// <summary>
        /// Finds all workflows count.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="status">The status.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public int FindAllWorkflowsCount(string name, short status, int accountId)
        {
            var predicate = PredicateBuilder.True<WorkflowsDb>();
            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                predicate = predicate.And(a => a.WorkflowName.Contains(name));
            }
            if (status != 0)
            {
                predicate = predicate.And(a => a.Status == status);
            }
            predicate = predicate.And(a => a.AccountID == accountId);
            predicate = predicate.And(a => a.IsDeleted == false);
            var db = ObjectContextFactory.Create();
            return db.Workflows.AsExpandable().Where(predicate).Count();
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="status">The status.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        public IEnumerable<Workflow> FindAll(string name, int limit, int pageNumber, short status, int accountId, string sortField, ListSortDirection direction)
        {
            var predicate = PredicateBuilder.True<WorkflowsDb>();
            //if (status != 0)
            //    predicate = predicate.And(a => a.Status == status);

            IEnumerable<WorkflowsDb> workflows = findWorkflowsSummary2(predicate, sortField, direction, pageNumber, limit, accountId, name, status);
            foreach (WorkflowsDb workflow in workflows)
            {
                yield return ConvertToDomain(workflow);
            }
        }

        /// <summary>
        /// Finds the active workflows.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        public IEnumerable<Workflow> FindActiveWorkflows(int pageNumber, int limit, string sortField = "", ListSortDirection direction = ListSortDirection.Descending)
        {
            var predicate = PredicateBuilder.True<WorkflowsDb>();
            var records = (pageNumber - 1) * limit;
            predicate = predicate.And(a => a.Status == (short)WorkflowStatus.Active);
            predicate = predicate.And(a => a.IsDeleted == false);
            IEnumerable<WorkflowsDb> workflows = findWorkflowsSummaryForEngine(predicate, sortField, direction)
                                                .Skip(records).Take(limit);
            foreach (WorkflowsDb workflow in workflows)
            {
                yield return ConvertToDomain(workflow);
            }
        }

        /// <summary>
        /// Finds the workflows summary.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="AccountID">The account identifier.</param>
        /// <param name="WorkflowName">Name of the workflow.</param>
        /// <returns></returns>
        IEnumerable<WorkflowsDb> findWorkflowsSummary(System.Linq.Expressions.Expression<Func<WorkflowsDb, bool>> predicate, string sortField, ListSortDirection direction, int pageNumber, int limit, int AccountID, string WorkflowName)
        {
            var db = ObjectContextFactory.Create();

            var sql_summary = @"SELECT WF.WorkflowID, WF.AccountID,WF.CreatedBy,WF.CreatedOn,WF.Status,
								   WF.WorkflowName,WF.WorkflowID,WF.DeactivatedOn,
								   WF.IsWorkflowAllowedMoreThanOnce,WF.RemovedWorkflows,
								   WF.AllowParallelWorkflows,WF.ModifiedOn,
								   (SELECT COUNT(DISTINCT CWA.ContactID) FROM dbo.ContactWorkflowAudit CWA 
										INNER JOIN dbo.WorkflowActions WA ON WA.WorkflowActionID = CWA.WorkflowActionID 
									WHERE CWA.WorkflowID = WF.WorkflowID AND WA.WorkflowActionTypeID != 11) AS ContactsStarted,
									(SELECT COUNT(*) FROM(
										(SELECT CWA.ContactID FROM dbo.ContactWorkflowAudit CWA 
											INNER JOIN dbo.WorkflowActions WA ON CWA.WorkflowActionID = WA.WorkflowActionID 
										 WHERE WA.WorkflowActionTypeID = 11 AND CWA.WorkflowID = WF.WorkflowID) 
										INTERSECT
									(SELECT CWA.ContactID FROM dbo.ContactWorkflowAudit CWA 
										INNER JOIN dbo.WorkflowActions WA on CWA.WorkflowActionID = WA.WorkflowActionID 
									 WHERE WA.WorkflowActionTypeID != 11 AND CWA.WorkflowID = WF.WorkflowID))I) AS ContactsFinished,
									(SELECT COUNT(DISTINCT CR.ContactID) FROM dbo.CampaignRecipients (nolock) CR 
                                        INNER JOIN dbo.Contacts C ON CR.ContactID = C.ContactID
										WHERE CR.WorkflowID = WF.WorkflowID AND (CR.DeliveryStatus = 113 or CR.HasUnsubscribed = 1) AND C.IsDeleted = 0 AND CR.AccountID = @ID) AS UnSubscribed
							 FROM dbo.Workflows WF where WF.AccountID = @ID AND WF.WorkflowName LIKE @NAME AND WF.IsDeleted != 1
							--OFFSET @SKIP ROWS -- skip 10 rows
							--FETCH NEXT @Limit ROWS ONLY";

            var records = (pageNumber - 1) * limit;
            var workflows = db.Get<WorkflowsDb>(sql_summary, new { Id = AccountID, Offset = records, Limit = limit, Name = "%" + WorkflowName + "%" }, true);
            workflows = workflows.AsQueryable().Where(predicate).OrderBy(sortField, direction).Skip(records).Take(limit);
            return workflows;
        }

        /// <summary>
        /// Finds the workflows summary2.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="AccountID">The account identifier.</param>
        /// <param name="WorkflowName">Name of the workflow.</param>
        /// <returns></returns>
        IEnumerable<WorkflowsDb> findWorkflowsSummary2(System.Linq.Expressions.Expression<Func<WorkflowsDb, bool>> predicate, string sortField, ListSortDirection direction, int pageNumber, int limit, int AccountID, string WorkflowName, short status)
        {
            var stataues = new List<WorkflowStatus>();
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT W.*, WA.Started ContactsStarted, WA.InProgress ContactsInProgress,WA.Completed ContactsFinished, WA.OptedOut ContactsOptedOut , COUNT(1) OVER() as TotalWorkflowCount 
                         FROM Workflows W (NOLOCK) 
                         LEFT JOIN WorkflowAnalytics WA (NOLOCK) ON W.WorkflowID = WA.WorkflowID
                         WHERE W.AccountID = @AccountID AND W.WorkflowID not in(SELECT ParentWorkflowID  FROM Workflows (NOLOCK) WHERE ParentWorkflowID !=0) AND W.IsDeleted = 0 AND W.WorkflowName LIKE @Name and W.Status IN @Statuses";
            if (status == 0)
            {
                WorkflowStatus[] allStatues = {
                    WorkflowStatus.Active,
                    WorkflowStatus.Draft,
                    WorkflowStatus.InActive,
                    WorkflowStatus.Paused
                };

                stataues.AddRange(allStatues);
            }
            else
            {
                stataues.Add((WorkflowStatus)status);
            }
            var workflows = db.Get<WorkflowsDb>(sql, new { AccountID = AccountID, Name = "%" + WorkflowName + "%", Statuses = stataues });
            workflows = workflows.AsQueryable().Where(predicate).OrderBy(sortField, direction).ToList();
            return workflows.Skip((pageNumber - 1) * limit).Take(limit);
        }

        /// <summary>
        /// Finds the workflows summary for engine.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        IEnumerable<WorkflowsDb> findWorkflowsSummaryForEngine(System.Linq.Expressions.Expression<Func<WorkflowsDb, bool>> predicate, string sortField, ListSortDirection direction)
        {
            var db = ObjectContextFactory.Create();
            WorkflowActionType[] ActionTypes = new WorkflowActionType[]
            {
              WorkflowActionType.AddTag,
              WorkflowActionType.AdjustLeadScore,
              WorkflowActionType.AssignToUser,
              WorkflowActionType.ChangeLifecycle,
              WorkflowActionType.NotifyUser,
              WorkflowActionType.RemoveTag,
              WorkflowActionType.SendCampaign,
              WorkflowActionType.SendEmail,
              WorkflowActionType.SendText,
              WorkflowActionType.SetTimer,
              WorkflowActionType.UpdateField
            };

            IEnumerable<WorkflowsDb> workflows = db.Workflows.Include(c => c.Statusses)
                .AsExpandable()
                .Where(predicate)
                .GroupJoin(db.ContactWorkflowAudit, i => i.WorkflowID, j => j.WorkflowID, (x, y) => new { Workflow = x, ContactAudit = y })
                .Select(a =>
                    new
                    {
                        AccountID = a.Workflow.AccountID,
                        CreatedBy = a.Workflow.CreatedBy,
                        CreatedOn = a.Workflow.CreatedOn,
                        Status = a.Workflow.Statusses,
                        WorkflowName = a.Workflow.WorkflowName,
                        WorkflowID = a.Workflow.WorkflowID,
                        DeactivatedOn = a.Workflow.DeactivatedOn,
                        IsWorkflowAllowedMoreThanOnce = a.Workflow.IsWorkflowAllowedMoreThanOnce,
                        RemovedWorkFlows = a.Workflow.RemovedWorkflows,
                        WorkFlowAllowedForOthers = a.Workflow.AllowParallelWorkflows,
                        ModifiedOn = a.Workflow.ModifiedOn,
                        ContactsStarted = a.ContactAudit.Where(x => x.WorkflowActions.WorkflowActionTypeID != WorkflowActionType.WorkflowEndState).Select(c => c.ContactID).Distinct().Count(),
                        ContactsFinished = a.ContactAudit.Where(x => x.WorkflowActions.WorkflowActionTypeID == WorkflowActionType.WorkflowEndState)
                                                         .Select(x => new { ContactID = x.ContactID })
                                                         .Join(a.ContactAudit.Where(x => ActionTypes.Contains(x.WorkflowActions.WorkflowActionTypeID)),
                                                         wa => wa.ContactID, wj => wj.ContactID,
                                                         (wa, wj) => new
                                                         {
                                                             wa.ContactID
                                                         }).Distinct().Count(),
                        ContactOptedOut = db.CampaignRecipients.Where(x => x.WorkflowID == a.Workflow.WorkflowID
                                                                        && (x.DeliveryStatus == (short)CampaignDeliveryStatus.HardBounce
                                                                            || x.HasUnsubscribed)).Count()
                    }).AsEnumerable().Select(x => new WorkflowsDb
                    {
                        AccountID = x.AccountID,
                        CreatedBy = x.CreatedBy,
                        CreatedOn = x.CreatedOn,
                        DeactivatedOn = x.DeactivatedOn,
                        IsWorkflowAllowedMoreThanOnce = x.IsWorkflowAllowedMoreThanOnce,
                        RemovedWorkflows = x.RemovedWorkFlows,
                        WorkflowID = x.WorkflowID,
                        WorkflowName = x.WorkflowName,
                        AllowParallelWorkflows = x.WorkFlowAllowedForOthers,
                        Status = x.Status.StatusID,
                        Statusses = x.Status,
                        ModifiedOn = x.ModifiedOn,
                        ContactsStarted = x.ContactsStarted,
                        ContactsInProgress = x.ContactsStarted - x.ContactsFinished,
                        ContactsFinished = x.ContactsFinished,
                        ContactsOptedOut = x.ContactOptedOut
                    }).AsQueryable().OrderBy(sortField, direction);
            return workflows;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override Workflow FindBy(short id)
        {
            int accountId = 0;
            WorkflowsDb workflowDatabase = getWorkflowDb(id, accountId);
            return ConvertToDomain(workflowDatabase);
        }

        /// <summary>
        /// Gets the workflow database.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        WorkflowsDb getWorkflowDb(short id, int accountId)
        {
            var db = ObjectContextFactory.Create();

            var sql = @"SELECT W.WorkflowID,W.ParentWorkflowID, W.WorkflowName,W.AccountID,W.Status, W.DeactivatedOn,W.IsWorkflowAllowedMoreThanOnce,W.AllowParallelWorkflows,W.RemovedWorkflows,W.CreatedBy,W.CreatedOn,W.ModifiedBy,W.ModifiedOn,W.IsDeleted, WA.Started ContactsStarted, WA.InProgress ContactsInProgress,WA.Completed ContactsFinished, WA.OptedOut ContactsOptedOut FROM Workflows W (NOLOCK)
                        LEFT JOIN WorkflowAnalytics WA (NOLOCK) ON W.WorkflowID = WA.WorkflowID
                        WHERE W.WorkflowID = @WorkFlowID AND W.IsDeleted = 0 AND W.AccountID =@accountID";

            var workflowDb = db.Get<WorkflowsDb>(sql, new { WorkFlowID = id, accountID = accountId }).SingleOrDefault();
            var status = new StatusesDb
            {
                StatusID = workflowDb.Status,
                Name = ((WorkflowStatus)workflowDb.Status).GetDisplayName()
            };
            workflowDb.Statusses = status;
            workflowDb.Triggers = GetTriggersByWorkflowId(id);
            workflowDb.WorkflowActions = GetWorkflowActionsByWorkflowId(id);
            return workflowDb;
        }

        /// <summary>
        /// Gets the triggers by workflow identifier.
        /// </summary>
        /// <param name="workflowID">The workflow identifier.</param>
        /// <returns></returns>
        private ICollection<WorkflowTriggersDb> GetTriggersByWorkflowId(short workflowID)
        {
            var db = ObjectContextFactory.Create();
            var triggers = db.WorkflowTriggers.Include(x => x.Forms).Include(x => x.Campaigns)
                                      .Include(x => x.ActionTypeDropdownValue)
                                      .Include(x => x.SearchDefinitions)
                                      .Include(x => x.DropdownValues)
                                      .Include(x => x.Tags)
                                      .Include(x => x.OpportunityStageValues)
                                      .Include(x => x.LeadAdapter.LeadAdapterTypes)
                                      .Where(t => t.WorkflowID == workflowID)
                                      .ToList();
            return triggers;
        }

        /// <summary>
        /// Gets the workflow actions by workflow identifier.
        /// </summary>
        /// <param name="workflowID">The workflow identifier.</param>
        /// <returns></returns>
        private ICollection<WorkflowActionsDb> GetWorkflowActionsByWorkflowId(short workflowID)
        {
            var db = ObjectContextFactory.Create();
            var workflowActions = db.WorkflowActions.Where(wa => wa.WorkflowID == workflowID && wa.IsDeleted == false && wa.IsSubAction == false).OrderBy(i => i.OrderNumber).ToList();
            workflowActions.ForEach(wa =>
                {
                    wa.Action = wa.GetAction(db);
                });
            return workflowActions;
        }

        /// <summary>
        /// Gets the end state.
        /// </summary>
        /// <param name="workflowId">The workflow identifier.</param>
        /// <returns></returns>
        public WorkflowAction GetEndState(short workflowId)
        {
            var db = ObjectContextFactory.Create();
            WorkflowActionsDb endState = db.WorkflowActions.Where(wa => wa.WorkflowID == workflowId && wa.WorkflowActionTypeID == WorkflowActionType.WorkflowEndState).FirstOrDefault();
            return Mapper.Map<WorkflowActionsDb, WorkflowAction>(endState);
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<Workflow> FindAll()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Invalid workflow id has been passed. Suspected Id forgery.</exception>
        public override WorkflowsDb ConvertToDatabaseType(Workflow domainType, CRMDb context)
        {
            WorkflowsDb workflowsDb;
            if (domainType.Id > 0)
            {
                workflowsDb = context.Workflows.SingleOrDefault(c => c.WorkflowID == domainType.Id);

                if (workflowsDb == null)
                    throw new ArgumentException("Invalid workflow id has been passed. Suspected Id forgery.");

                workflowsDb = Mapper.Map<Workflow, WorkflowsDb>(domainType, workflowsDb);
            }
            else
            {
                workflowsDb = ConvertToDatabaseType(domainType);
            }
            return workflowsDb;
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="databaseType">Type of the database.</param>
        /// <returns></returns>
        public override Workflow ConvertToDomain(WorkflowsDb databaseType)
        {
            return Mapper.Map<WorkflowsDb, Workflow>(databaseType);
        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <returns></returns>
        public WorkflowsDb ConvertToDatabaseType(Workflow workflow)
        {
            return Mapper.Map<Workflow, WorkflowsDb>(workflow);
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="context">The context.</param>
        public override void PersistValueObjects(Workflow domainType, WorkflowsDb dbType, CRMDb context)
        {
            if (dbType.WorkflowID > 0)
                context.Entry(dbType).Property(p => p.CreatedOn).IsModified = false;

            PersistTriggers(domainType, dbType, context);
            PersistActions(domainType, dbType, context);


        }

        /// <summary>
        /// Persists the triggers.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="context">The context.</param>
        public void PersistTriggers(Workflow domainType, WorkflowsDb dbType, CRMDb context)
        {
            var triggers = context.WorkflowTriggers.Where(wa => wa.WorkflowID == domainType.WorkflowID).Select(t =>
                new WorkflowTrigger
                {
                    WorkflowTriggerID = t.WorkflowTriggerID
                }).ToList();
            var dbTriggers = Mapper.Map<IEnumerable<WorkflowTriggersDb>>(triggers);
            if (dbTriggers != null && dbTriggers.Any())
            {
                var delete = dbTriggers.Where(t => !dbType.Triggers.Select(dt => dt.WorkflowTriggerID).Contains(t.WorkflowTriggerID));

                delete.Each(t =>
                {
                    context.Entry(t).State = System.Data.Entity.EntityState.Deleted;
                });
            }

            dbType.Triggers.ForEach(t =>
            {
                if (t.WorkflowTriggerID == 0)
                    context.Entry(t).State = System.Data.Entity.EntityState.Added;
                else
                    context.Entry(t).State = System.Data.Entity.EntityState.Modified;
            });
        }

        /// <summary>
        /// Persists the actions.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="context">The context.</param>
        public void PersistActions(Workflow domainType, WorkflowsDb dbType, CRMDb context)
        {
            dbType.WorkflowActions.ForEach(wa =>
            {
                if (wa.WorkflowActionID == 0)
                    context.Entry(wa).State = System.Data.Entity.EntityState.Added;
                else
                    context.Entry(wa).State = System.Data.Entity.EntityState.Modified;
            });
        }

        /// <summary>
        /// Updates the action.
        /// </summary>
        /// <param name="workflowAction">The workflow action.</param>
        /// <returns></returns>
        public WorkflowAction UpdateAction(WorkflowAction workflowAction)
        {
            var action = Mapper.Map<WorkflowActionsDb>(workflowAction);
            if (workflowAction.IsDeleted == false)
            {
                using (var db = new CRMDb())
                {
                    action.Action.WorkflowActionID = action.WorkflowActionID;
                    action.Action.WorkflowID = action.WorkflowID;
                    action.Action.Save(db);
                    db.SaveChanges();

                    if (action.WorkflowActionTypeID == WorkflowActionType.SendCampaign || action.WorkflowActionTypeID == WorkflowActionType.LinkActions)
                    {
                        var cAction = action.Action as WorkflowCampaignActionsDb;
                        foreach (var link in cAction.Links)
                        {
                            //remove already existing links
                            var existingLinks = db.WorkflowCampaignActionLinks.Where(cl => cl.LinkID == link.LinkID && cl.ParentWorkflowActionID == cAction.WorkflowCampaignActionID);
                            if (existingLinks.IsAny())
                                existingLinks.ToList().ForEach(el => db.Entry(el).State = System.Data.Entity.EntityState.Deleted);
                            //delete already existing link actions
                            //db.WorkflowActions.Where(a => existingLinks.Select(e=>e.LinkActionID).Contains(a.WorkflowActionID)).ForEach(a => a.IsDeleted = true);
                            link.WorkflowID = action.WorkflowID;
                            link.ParentWorkflowActionID = cAction.WorkflowCampaignActionID;
                            link.Save(db);
                        }
                    }
                }
            }
            return Mapper.Map<WorkflowAction>(action);
        }

        /// <summary>
        /// Updates the link actions.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        public void UpdateLinkActions(Workflow workflow)
        {
            var workflowDb = ConvertToDatabaseType(workflow);
            var campaignActions = workflowDb.WorkflowActions.Where(wa => wa.Action.GetType() == typeof(WorkflowCampaignActionsDb))
                .Select(a => (WorkflowCampaignActionsDb)a.Action);
            using (var context = new CRMDb())
            {
                foreach (var c in campaignActions)
                {
                    //if (c.Links != null)
                    //{
                    //    foreach (var link in c.Links)
                    //    {
                    //        if (link.LinkActions != null)    //Few links wont have actions
                    //        {
                    //            if (link.LinkActions.WorkflowActionTypeID == WorkflowActionType.AdjustLeadScore)
                    //            {
                    //                ((WorkflowLeadScoreActionsDb)link.LinkActions).UpdateLeadScoreRule(workflow.AccountID, workflow.CreatedBy);
                    //            }
                    //            var linkActionId = link.LinkActions.WorkflowActionID;
                    //            var parentCampaignId = c.WorkflowCampaignActionID;

                    //            link.LinkActionID = linkActionId;
                    //            link.ParentWorkflowActionID = parentCampaignId;
                    //            link.SaveActionLink(context);
                    //        }
                    //    }
                    //}
                };
                context.SaveChanges();
            }

        }

        /// <summary>
        /// Inserts the workflow end action.
        /// </summary>
        /// <param name="workflowID">The workflow identifier.</param>
        public void InsertWorkflowEndAction(int workflowID)
        {
            var action = new WorkflowActionsDb()
            {
                WorkflowID = (short)workflowID,
                WorkflowActionID = 0,
                OrderNumber = 0,
                IsDeleted = true,
                IsSubAction = false,
                WorkflowActionTypeID = WorkflowActionType.WorkflowEndState
            };
            using (var db = new CRMDb())
            {
                action.Save(db);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Determines whether [is workflow name unique] [the specified workflow].
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <returns></returns>
        public bool IsWorkflowNameUnique(Workflow workflow)
        {
            var db = ObjectContextFactory.Create();
            List<int> workflowIds = new List<int>();
            if (workflow.ParentWorkflowID == 0)
            {
                workflowIds.Add(workflow.Id);
            }
            else if (workflow.Id != 0)
            {
                IEnumerable<ParentWorkflow> workflowParentIds = GetAllParentWorkflows(workflow.Id);
                workflowIds.AddRange(workflowParentIds.Select(s => s.WorkflowID).ToArray());
            }
            else
            {
                IEnumerable<ParentWorkflow> workflowParentIds = GetAllParentWorkflows(workflow.ParentWorkflowID);
                workflowIds.AddRange(workflowParentIds.Select(s => s.WorkflowID).ToArray());
            }

            var workflowFound = db.Workflows.Where(c => c.WorkflowName.Equals(workflow.WorkflowName, StringComparison.CurrentCultureIgnoreCase)
                                                     && c.AccountID == workflow.AccountID
                                                     && !workflowIds.Contains(c.WorkflowID) && c.IsDeleted == false)
                                            .Select(c => c).FirstOrDefault();
            if (workflowFound != null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks to update status.
        /// </summary>
        /// <param name="workflowId">The workflow identifier.</param>
        /// <returns></returns>
        public IEnumerable<string> checkToUpdateStatus(int workflowId)
        {
            IEnumerable<string> workflowNames = new List<string>();
            using (var db = ObjectContextFactory.Create())
            {
                var actionIds = db.TriggerWorkflowActions.Where(w => w.SiblingWorkflowID == workflowId).Select(s => s.WorkflowActionID).Distinct();
                var workflowIds = db.WorkflowActions.Where(w => actionIds.Contains(w.WorkflowActionID)).Select(s => s.WorkflowID).Distinct();
                workflowNames = db.Workflows.Where(w => workflowIds.Contains(w.WorkflowID) && w.IsDeleted == false && w.Status == (short)WorkflowStatus.Active).Select(s => s.WorkflowName).ToList();
            }
            return workflowNames;
        }

        /// <summary>
        /// Deactivates the workflow.
        /// </summary>
        /// <param name="workflowId">The workflow identifier.</param>
        public void DeactivateWorkflow(short workflowId)
        {
            var db = ObjectContextFactory.Create();
            WorkflowsDb workflowdb = db.Workflows.Where(w => w.WorkflowID == workflowId).FirstOrDefault();
            if (workflowdb != null)
            {
                workflowdb.Status = (short)WorkflowStatus.InActive;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Removes from other workflows.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="removeFromWorkflows">The remove from workflows.</param>
        /// <param name="allowParallelWorkflows">The allow parallel workflows.</param>
        public void RemoveFromOtherWorkflows(int contactId, string removeFromWorkflows, byte allowParallelWorkflows)
        {
            if (contactId != 0)
            {
                var workflowIds = new List<short>();
                var db = ObjectContextFactory.Create();

                if (allowParallelWorkflows == 3 && removeFromWorkflows != null)
                    workflowIds = removeFromWorkflows.Split(',').Select(s =>
                    {
                        short i;
                        return short.TryParse(s, out i) ? i : default(short);
                    }).ToList();

                else if (allowParallelWorkflows == 2)
                    workflowIds = db.Workflows.Where(w => w.Status == (short)WorkflowStatus.Active && w.IsDeleted == false).Select(s => s.WorkflowID).ToList();

                foreach (var id in workflowIds)
                {
                    if (id != 0)
                    {
                        var endStateActionId = db.WorkflowActions.Where(wa => wa.WorkflowID == id && wa.WorkflowActionTypeID == WorkflowActionType.WorkflowEndState).
                                               Select(s => s.WorkflowActionID).FirstOrDefault();
                        if (endStateActionId != 0)
                        {
                            ContactWorkflowAuditDb contactAudit = new ContactWorkflowAuditDb();
                            contactAudit.ActionPerformedOn = DateTime.Now.ToUniversalTime();
                            contactAudit.ContactID = contactId;
                            contactAudit.WorkflowID = id;
                            contactAudit.WorkflowActionID = endStateActionId;
                            db.ContactWorkflowAudit.Add(contactAudit);
                        }
                    }
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Determines whether [is enrolled to remove] [the specified workflow identifier].
        /// </summary>
        /// <param name="workflowId">The workflow identifier.</param>
        /// <returns></returns>
        public IEnumerable<short> IsEnrolledToRemove(int workflowId)
        {
            var db = ObjectContextFactory.Create();
            var workflowIds = new List<short>();
            if (workflowId != 0)
            {
                workflowIds = db.Workflows.Where(w => w.IsDeleted == false && w.Status == (short)WorkflowStatus.Active &&
                    (w.RemovedWorkflows.Contains(workflowId.ToString()) || (w.AllowParallelWorkflows == 2 && w.WorkflowID != workflowId))).Select(s => s.WorkflowID).ToList();
            }
            return workflowIds;
        }

        /// <summary>
        /// Gets the workflow by identifier.
        /// </summary>
        /// <param name="WorkflowID">The workflow identifier.</param>
        /// <returns></returns>
        public Workflow GetWorkflowByID(short WorkflowID, int accountId)
        {
            WorkflowsDb workflowdb = getWorkflowDb(WorkflowID, accountId);
            // IEnumerable<WorkflowsDb> dbs = getWorkflowsDb(WorkflowID);

            return (workflowdb != null) ? ConvertToDomain(workflowdb) : null;
        }

        /// <summary>
        /// Deletes the work flows.
        /// </summary>
        /// <param name="workflowIDs">The workflow i ds.</param>
        public void DeleteWorkFlows(int[] workflowIDs)
        {
            var db = ObjectContextFactory.Create();
            foreach (int workflowid in workflowIDs)
            {
                var workflow = db.Workflows.FirstOrDefault(c => c.WorkflowID == workflowid);
                if (workflow != null)
                {
                    workflow.IsDeleted = true;
                }
            }
            db.SaveChanges();
        }

        /// <summary>
        /// Gets all campaigns.
        /// </summary>
        /// <param name="AccountID">The account identifier.</param>
        /// <param name="IsWorkflowCampaign">if set to <c>true</c> [is workflow campaign].</param>
        /// <returns></returns>
        public IEnumerable<Campaign> GetAllCampaigns(int AccountID, bool IsWorkflowCampaign)
        {
            string whereClause = string.Empty;
            var db = ObjectContextFactory.Create();
            if (IsWorkflowCampaign)
                whereClause = "WHERE C.AccountID=@AccountId AND C.CampaignStatusID=107 AND C.IsDeleted=0 AND IsLinkedToWorkflows=1";
            else
                whereClause = "WHERE C.AccountID=@AccountId AND C.CampaignStatusID IN (101,102,105) AND C.IsDeleted=0";

            string sql = string.Format(@"SELECT C.CampaignID,C.Name FROM Campaigns (NOLOCK) C {0} ORDER BY ISNULL(C.LastUpdatedOn,C.CreatedDate) DESC", whereClause);

            IEnumerable<CampaignsDb> campaigns = db.Get<CampaignsDb>(sql, new { AccountId = AccountID }).ToList();

            foreach (CampaignsDb dc in campaigns)
                yield return Mapper.Map<CampaignsDb, Campaign>(dc);
        }

        /// <summary>
        /// Gets all forms.
        /// </summary>
        /// <param name="AccountID">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Form> GetAllForms(int AccountID)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT F.FormID,F.AccountID,F.Name FROM Forms (NOLOCK) F
                        WHERE F.IsDeleted=0 AND F.AccountID=@AccountId AND F.Status=201
                        ORDER BY F.LastModifiedOn DESC";
            IEnumerable<FormsDb> forms = db.Get<FormsDb>(sql, new { AccountId = AccountID }).ToList();
            foreach (FormsDb form in forms)
                yield return Mapper.Map<FormsDb, Form>(form);
        }

        /// <summary>
        /// Gets all smart searches.
        /// </summary>
        /// <param name="AccountID">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<SearchDefinition> GetAllSmartSearches(int AccountID)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT SD.SearchDefinitionID,SD.AccountID,SD.SearchDefinitionName FROM SearchDefinitions (NOLOCK) SD
                        WHERE SD.SelectAllSearch=0 AND SD.AccountID=@AccountId
                        ORDER BY SD.CreatedOn DESC";
            IEnumerable<SearchDefinitionsDb> searchResults = db.Get<SearchDefinitionsDb>(sql, new { AccountId = AccountID }).ToList();
            foreach (SearchDefinitionsDb result in searchResults)
                yield return Mapper.Map<SearchDefinitionsDb, SearchDefinition>(result);
        }

        /// <summary>
        /// Gets all tags.
        /// </summary>
        /// <param name="AccountID">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Tag> GetAllTags(int AccountID)
        {
            List<Tag> tags = new List<Tag>();
            using (var con = ObjectContextFactory.Create())
            {
                var sql = @"SELECT TagId as ID, Tagname FROM vTags (NOLOCK) WHERE AccountID = @AccountID AND ISNULL(IsDeleted,0)  = 0 ORDER BY TagName ASC";
                tags = con.Get<Tag>(sql, new { AccountID = AccountID }).ToList();
        }
            return tags;
        }

        /// <summary>
        /// Gets the basic contact details.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public UserContactActivitySummary GetBasicContactDetails(int contactId, int accountId)
        {
            var db = ObjectContextFactory.Create();

            //var contact = db.Contacts.Where(con => con.ContactID == contactId && con.IsDeleted == false && con.AccountID == accountId).Select(s => new UserContactActivitySummary()
            //{
            //    contactId = s.ContactID,
            //    ContactName = s.ContactType == ContactType.Person ? (s.FirstName + " " + s.LastName == " " ? db.ContactEmails.Where(ce => ce.ContactID == contactId
            //        && ce.IsPrimary == true).Select(sce => sce.Email).FirstOrDefault() : s.FirstName + " " + s.LastName) : s.Company,
            //    ContactType = s.ContactType,
            //    FirstName = s.FirstName,
            //    LastName = s.LastName,
            //    LastUpdatedBy = null,
            //    Email = db.ContactEmails.Where(ce => ce.ContactID == contactId
            //        && ce.IsPrimary == true).Select(sce => sce.Email).FirstOrDefault(),
            //    PhoneNumber = db.ContactPhoneNumbers.Where(cp => cp.ContactID == contactId && cp.IsPrimary == true).Select(p => p.PhoneNumber).FirstOrDefault(),
            //    PrimaryPhoneTypeValueId = db.DropdownValues.Where(dv => dv.AccountID == accountId && dv.DropdownID == 1 && dv.IsDefault == true).Select(dp => dp.DropdownValueID).FirstOrDefault()
            //}).FirstOrDefault();

            var UserContactActivitySummary = new UserContactActivitySummary();
            var sql = @"SELECT C.ContactID AS contactId, ContactType, FirstName, LastName, CE.Email, CP.PhoneNumber,
                        CASE WHEN C.ContactType = 1 THEN COALESCE(C.FirstName + ' ' + C.LastName, CE.Email) ELSE C.Company END AS ContactName, '' AS LastUpdatedBy 
                        FROM Contacts (NOLOCK) C
                        LEFT JOIN ContactEmails (NOLOCK) CE ON CE.ContactID = C.ContactID AND CE.IsPrimary = 1
                        LEFT JOIN ContactPhoneNumbers (NOLOCK) CP ON CP.ContactID = C.ContactID AND CP.IsPrimary = 1
                        WHERE C.ContactID = @ContactID AND C.AccountID = @AccountID
                        AND C.IsDeleted = 0

                        ;WITH CTE AS (
                        SELECT PhoneType DropdownValueID,1 as Ordr FROM ContactPhoneNumbers (NOLOCK) WHERE ContactID = @ContactID AND AccountID=@AccountID AND IsPrimary = 1 AND IsDeleted = 0
                        UNION 
                        SELECT DropdownValueID,0 FROM DropdownValues (NOLOCK) WHERE AccountID = @AccountID AND DropdownID = 1 AND IsDefault = 1)

                        SELECT TOP 1 DropdownValueID  FROM  CTE ORDER BY   Ordr DESC";
            db.GetMultiple(sql, (r) =>
            {
                UserContactActivitySummary = r.Read<UserContactActivitySummary>().FirstOrDefault();
                int primaryPhoneType = r.Read<short>().FirstOrDefault();
                UserContactActivitySummary.PrimaryPhoneTypeValueId = primaryPhoneType;

            }, new { AccountID = accountId, ContactID = contactId });
            return UserContactActivitySummary;
        }

        /// <summary>
        /// Gets all users.
        /// </summary>
        /// <param name="AccountID">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<User> GetAllUsers(int AccountID)
        {
            var db = ObjectContextFactory.Create();

            var sql = @"SELECT U.UserID,U.FirstName + ' ' + U.LastName AS FirstName  FROM Users (NOLOCK) U
                        WHERE U.AccountID=@AccountId AND U.Status=1 AND U.IsDeleted=0
                        ORDER BY ISNULL(U.ModifiedOn,U.CreatedOn) DESC";


            IEnumerable<UsersDb> users = db.Get<UsersDb>(sql, new { AccountId = AccountID }).ToList();
            foreach (UsersDb user in users)
                yield return Mapper.Map<UsersDb, User>(user);
        }

        /// <summary>
        /// Inserts the contact workflow audit.
        /// </summary>
        /// <param name="workflowId">The workflow identifier.</param>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="workflowActionId">The workflow action identifier.</param>
        /// <param name="messageId">The message identifier.</param>
        public void InsertContactWorkflowAudit(short workflowId, int contactId, int workflowActionId, string messageId)
        {
            messageId = string.IsNullOrEmpty(messageId) ? "00000000-0000-0000-0000-000000000000" : messageId;
            if (contactId != 0 && workflowActionId != 0 && workflowId != 0)
            {
                var db = ObjectContextFactory.Create();
                ContactWorkflowAuditDb contactWorkflowAudit = new ContactWorkflowAuditDb();
                contactWorkflowAudit.ContactID = contactId;
                contactWorkflowAudit.WorkflowID = workflowId;
                contactWorkflowAudit.WorkflowActionID = workflowActionId;
                contactWorkflowAudit.ActionPerformedOn = DateTime.Now.ToUniversalTime();
                contactWorkflowAudit.MessageID = messageId;
                db.ContactWorkflowAudit.Add(contactWorkflowAudit);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Determines whether [has completed workflow] [the specified workflow identifier].
        /// </summary>
        /// <param name="workflowId">The workflow identifier.</param>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="workflowActionId">The workflow action identifier.</param>
        /// <returns></returns>
        public bool HasCompletedWorkflow(int workflowId, int contactId, int workflowActionId)
        {
            var db = ObjectContextFactory.Create();
            bool hasCompleted = db.ContactWorkflowAudit.Where(a => a.WorkflowID == workflowId && a.ContactID == contactId && a.WorkflowActionID == workflowActionId).Any();
            return hasCompleted;
        }

        /// <summary>
        /// Determines whether [is first notification] [the specified workflow identifier].
        /// </summary>
        /// <param name="workflowId">The workflow identifier.</param>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="workflowActionId">The workflow action identifier.</param>
        /// <returns></returns>
        public bool IsFirstNotification(int workflowId, int contactId, int workflowActionId)
        {
            var db = ObjectContextFactory.Create();
            var workflowActions = db.WorkflowActions.Where(w => w.WorkflowID == workflowId && w.WorkflowActionTypeID == WorkflowActionType.NotifyUser && !w.IsDeleted).OrderBy(o => o.OrderNumber).Skip(1).Select(s => s.WorkflowActionID);
            bool isFirstNotifyAction = workflowActions.Contains(workflowActionId) ? false : true;
            return isFirstNotifyAction;
        }

        /// <summary>
        /// Gets the notify user action details.
        /// </summary>
        /// <param name="workflowActionId">The workflow action identifier.</param>
        /// <returns></returns>
        public Dictionary<int, WorkflowNotifyUserAction> GetNotifyUserActionDetails(int workflowActionId)
        {
            Dictionary<int, WorkflowNotifyUserAction> notifyUser = new Dictionary<int, WorkflowNotifyUserAction>();
            var db = ObjectContextFactory.Create();
            var notificationDetailsDb = db.WorkflowNotifyUserActions.Where(s => s.WorkflowActionID == workflowActionId).Select(s => new
            {
                userID = s.UserID,
                notifyType = s.NotifyType,
                messageBody = s.MessageBody
            }).Select(x => new WorkflowNotifyUserActionsDb() { NotifyType = x.notifyType, UserID = x.userID, MessageBody = x.messageBody }).FirstOrDefault();

            if (notificationDetailsDb != null)
            {
                var userIds = notificationDetailsDb.UserID.Split(',').Select(s => Convert.ToInt32(s));
                int accountId = db.Users.Where(a => userIds.Contains(a.UserID)).Select(s => s.AccountID).FirstOrDefault();
                var notificationdetails = Mapper.Map<WorkflowNotifyUserActionsDb, WorkflowNotifyUserAction>(notificationDetailsDb);
                notifyUser.Add(accountId, notificationdetails);
            }
            return notifyUser;
        }

        /// <summary>
        /// Gets the add tag action details.
        /// </summary>
        /// <param name="workflowActionId">The workflow action identifier.</param>
        /// <returns></returns>
        public IEnumerable<int> GetAddTagActionDetails(int workflowActionId)
        {
            var db = ObjectContextFactory.Create();
            var addTagDetails = db.WorkflowTagActions.Where(w => w.WorkflowActionID == workflowActionId).Select(s => s.TagID);
            return addTagDetails;
        }

        /// <summary>
        /// Gets the related campaigns.
        /// </summary>
        /// <param name="workflowID">The workflow identifier.</param>
        /// <returns></returns>
        public IEnumerable<Campaign> GetRelatedCampaigns(short workflowID)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT C.Name,C.CampaignID AS Id FROM WorkflowCampaignActions(NOLOCK) WCA
                        INNER JOIN WorkflowActions(NOLOCK) WA on WA.WorkflowActionID = WCA.WorkflowActionID
                        INNER JOIN Workflows (NOLOCK) W ON W.WorkflowID = WA.WorkflowID
                        INNER JOIN Campaigns (NOLOCK) C ON C.CampaignID = WCA.CampaignID
                        WHERE WA.WorkflowID = @workflowId AND WA.IsDeleted = 0 AND WA.WorkflowActionTypeID != 14 AND C.IsDeleted=0";

            IEnumerable<Campaign> campaigns = db.Get<Campaign>(sql, new { workflowId = workflowID }).ToList();
            return campaigns;
        }

        /// <summary>
        /// Checks if contact entered workflow.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="workflowId">The workflow identifier.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <returns></returns>
        public bool CheckIfContactEnteredWorkflow(int contactId, int workflowId, string messageId)
        {
            var db = ObjectContextFactory.Create();
            bool hasEntered = db.ContactWorkflowAudit.Any(wa => wa.ContactID == contactId && wa.WorkflowID == workflowId);
            return hasEntered;
        }

        /// <summary>
        /// Determines whether this instance [can contact reenter workflow] the specified workflow identifier.
        /// </summary>
        /// <param name="workflowId">The workflow identifier.</param>
        /// <returns></returns>
        public bool CanContactReenterWorkflow(int workflowId)
        {
            var db = ObjectContextFactory.Create();
            return db.Workflows.Where(w => w.WorkflowID == workflowId).Select(s => s.IsWorkflowAllowedMoreThanOnce).SingleOrDefault() ?? false;
        }

        /// <summary>
        /// Gets the last state.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="workflowId">The workflow identifier.</param>
        /// <returns></returns>
        public int GetLastState(int contactId, int workflowId)
        {
            var db = ObjectContextFactory.Create();
            var workflowActionId = default(int);
            if (contactId != 0 && workflowId != 0)
            {
                workflowActionId = db.ContactWorkflowAudit.Where(wa => wa.WorkflowID == workflowId && wa.ContactID == contactId)
                    .Join(db.WorkflowActions.Where(wa => wa.WorkflowID == workflowId && !wa.IsSubAction && !wa.IsDeleted),
                    cwa => cwa.WorkflowActionID, wa => wa.WorkflowActionID,
                    (cwa, wa) => new { cwa.ActionPerformedOn, cwa.WorkflowActionID })
                    .OrderByDescending(c => c.ActionPerformedOn).FirstOrDefault().WorkflowActionID;
            }
            return workflowActionId;
        }

        /// <summary>
        /// Gets the remaining workflows.
        /// </summary>
        /// <param name="WorkflowID">The workflow identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Workflow> GetRemainingWorkflows(short WorkflowID, int accountId)
        {

            var db = ObjectContextFactory.Create();
            var sql = @"SELECT WorkflowID,AccountID,WorkflowName FROM Workflows (NOLOCK) WHERE AccountID=@AccountId AND WorkflowID <> @WorkflowId AND IsDeleted=0 AND Status = 401";
            IEnumerable<WorkflowsDb> workflows = db.Get<WorkflowsDb>(sql, new { AccountId = accountId, WorkflowId = WorkflowID }).ToList();


            foreach (WorkflowsDb workflow in workflows)
                yield return Mapper.Map<WorkflowsDb, Workflow>(workflow);

        }

        /// <summary>
        /// Gets the campaign statistics.
        /// </summary>
        /// <param name="campaignID">The campaign identifier.</param>
        /// <param name="workflowID">The workflow identifier.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <returns></returns>
        public IEnumerable<WorkflowCampaignStatistics> GetCampaignStatistics(int campaignID, short workflowID, DateTime fromDate, DateTime toDate)
        {
            var db = ObjectContextFactory.Create();

            var sql = "[dbo].[Calc_WorkflowCampaignAnalytics]";
            var statistics = new List<WorkflowCampaignStatistics>();
            db.QueryStoredProc(sql, (r) =>
                {
                    statistics = r.Read<WorkflowCampaignStatistics>().ToList();
                }, new { CampaignID = campaignID, WorkflowID = (int)workflowID, FromDate = fromDate, ToDate = toDate }, false);
            return statistics;
        }

        /// <summary>
        /// Updates the workflow status.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <param name="workflowID">The workflow identifier.</param>
        /// <param name="modifiedBy">The modified by.</param>
        public void UpdateWorkflowStatus(WorkflowStatus status, int workflowID, int modifiedBy)
        {
            using (var db = new CRMDb())
            {
                var workflow = db.Workflows.Where(w => w.WorkflowID == workflowID).FirstOrDefault();
                db.Entry(workflow).State = System.Data.Entity.EntityState.Modified;
                workflow.Status = (short)status;
                workflow.ModifiedOn = DateTime.Now.ToUniversalTime();
                workflow.ModifiedBy = modifiedBy;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the contact workflows.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public IEnumerable<Workflow> GetContactWorkflows(int contactId)
        {
            var workflowdb = default(IEnumerable<WorkflowsDb>);
            if (contactId != 0)
            {
                var db = ObjectContextFactory.Create();
                workflowdb = db.Workflows.Where(w => w.Status == (short)WorkflowStatus.Active && w.IsDeleted == false).GroupJoin(db.ContactWorkflowAudit, w => w.WorkflowID, wa => wa.WorkflowID, (w, wa) =>
                    new
                    {
                        workflow = w,
                        contactAudit = wa.Where(x => x.WorkflowActions.WorkflowActionTypeID != WorkflowActionType.WorkflowEndState)
                    }).Select(a => new
                    {
                        WorkflowId = a.workflow.WorkflowID,
                        WorkflowName = a.workflow.WorkflowName
                    }).Distinct().ToList().Select(x => new WorkflowsDb
                    {
                        WorkflowID = x.WorkflowId,
                        WorkflowName = x.WorkflowName
                    });
            }
            return Mapper.Map<IEnumerable<WorkflowsDb>, IEnumerable<Workflow>>(workflowdb);
        }

        /// <summary>
        /// Gets the lead adapters.
        /// </summary>
        /// <param name="AccountID">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<LeadAdapterAndAccountMap> GetLeadAdapters(int AccountID)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT LAM.LeadAdapterAndAccountMapID AS LeadAdapterAndAccountMapId, LAM.AccountID, LAM.ModifiedDateTime AS LastModifiled, LAM.LeadAdapterTypeID AS LeadAdapterTypeID,
                        CASE WHEN LAM.LeadAdapterTypeID = 13 THEN FL.Name ELSE LT.Name END AS LeadAdapterType
                        FROM LeadAdapterAndAccountMap LAM
                        INNER JOIN LeadAdapterTypes LT ON LT.LeadAdapterTypeID = LAM.LeadAdapterTypeID
                        LEFT JOIN FacebookLeadAdapter FL ON FL.LeadAdapterAndAccountMapID = LAM.LeadAdapterAndAccountMapID
                        WHERE LAM.IsDelete = 0 AND LAM.AccountID = @accountID AND LAM.LeadAdapterTypeID != 11";
            IEnumerable<LeadAdapterAndAccountMapDb> leadadapters = db.Get<LeadAdapterAndAccountMapDb>(sql, new { accountID = AccountID }, false);
            foreach (LeadAdapterAndAccountMapDb leadadapter in leadadapters)
                yield return Mapper.Map<LeadAdapterAndAccountMapDb, LeadAdapterAndAccountMap>(leadadapter);
        }

        /// <summary>
        /// Gets the selected link namesin trigger.
        /// </summary>
        /// <param name="LinkIDs">The link i ds.</param>
        /// <returns></returns>
        public string GetSelectedLinkNamesinTrigger(int campaignId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var urls = db.CampaignLinks.Where(i => i.CampaignID == campaignId).Select(x => x.URL);
                return urls.Count() > 0 ? string.Join(",</br>", urls) : "";
            }
        }

        /// <summary>
        /// Gets the scheduled messages.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        public IEnumerable<IDictionary<string, object>> GetScheduledMessages(TimeSpan? time = null)
        {
            if (time == null)
                time = new TimeSpan(0, 1, 0);

            DateTime date = DateTime.UtcNow.AddTicks(time.Value.Ticks);
            var sql = @"select * from smartcrm.dbo.TrackMessages (nolock) where scheduledon <= @date and IsPublished = 0 and leadscoreconditiontype = 17";
            var db = ObjectContextFactory.Create();
            var messages = db.Get(sql, new { date = date });

            foreach (dynamic m in messages)
            {
                yield return m;
            }
        }

        /// <summary>
        /// Updates the message status.
        /// </summary>
        /// <param name="tmid">The tmid.</param>
        /// <param name="isPublished">if set to <c>true</c> [is published].</param>
        public void UpdateMessageStatus(int tmid, bool isPublished)
        {
            var sql = @"update TrackMessages
                        set IsPublished = @isPublished,
                        PublishedOn = @publishedOn
                        where trackMessageId = @tmid";
            var db = ObjectContextFactory.Create();
            db.Execute(sql, new { isPublished = isPublished, publishedOn = DateTime.UtcNow, tmid = tmid });
        }

        /// <summary>
        /// 
        /// </summary>

        /// <summary>
        /// Gets the user status.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public Status GetUserStatus(int userId)
        {
            var db = ObjectContextFactory.Create();
            Status status = db.Users.Where(a => a.UserID == userId).Select(s => s.Status).FirstOrDefault();
            return status;
        }

        /// <summary>
        /// Updates the contact field.
        /// </summary>
        /// <param name="fieldId">The field identifier.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="fieldInputTypeId">The field input type identifier.</param>
        public void UpdateContactField(int fieldId, string fieldValue, int contactId, int fieldInputTypeId, int accountId)
        {
            if (fieldId != 0 && contactId != 0 && !string.IsNullOrEmpty(fieldValue))
            {
                using (var db = ObjectContextFactory.Create())
                {
                    var urlFields = new List<int>() { (int)ContactFields.BlogUrl, (int)ContactFields.FacebookUrl, (int)ContactFields.TwitterUrl, (int)ContactFields.LinkedInUrl, (int)ContactFields.WebsiteUrl, (int)ContactFields.GooglePlusUrl };
                    // Customfields
                    if (fieldId > 200)
                    {
                        var fieldData = db.ContactCustomFields.Where(w => w.ContactID == contactId && w.CustomFieldID == fieldId);
                        if (fieldData.Any())
                        {
                            var customFieldData = fieldData.FirstOrDefault();
                            customFieldData.Value = fieldValue;
                        }
                        else
                        {
                            var customFieldData = new ContactCustomFieldsDb()
                            {
                                ContactID = contactId,
                                CustomFieldID = fieldId,
                                Value = fieldValue
                            };
                            db.ContactCustomFields.Add(customFieldData);
                        }
                    }
                    else if (fieldId == (int)ContactFields.LeadSource)
                    {
                        short leadsourceId = Convert.ToInt16(fieldValue);
                        var leadSourceData = db.ContactLeadSourcesMap.Where(w => w.ContactID == contactId && w.LeadSouceID == leadsourceId).ToList();
                        if (!leadSourceData.IsAny())
                        {
                            var sourceData = new ContactLeadSourceMapDb()
                            {
                                ContactID = contactId,
                                LeadSouceID = Convert.ToInt16(fieldValue),
                                LastUpdatedDate = DateTime.UtcNow,
                                IsPrimaryLeadSource = false
                            };
                            db.ContactLeadSourcesMap.Add(sourceData);
                        }
                    }
                    else if (fieldId == (int)ContactFields.LifecycleStageField)
                    {
                        var lifeCycleData = db.Contacts.Where(w => w.ContactID == contactId && w.AccountID == accountId).FirstOrDefault();
                        if (lifeCycleData != null)
                        {
                            lifeCycleData.LifecycleStage = Convert.ToInt16(fieldValue);
                        }
                    }
                    else if (urlFields.Contains(fieldId))
                    {
                        var communicationId = db.Contacts.Where(c => c.ContactID == contactId && c.AccountID == accountId).Select(s => s.CommunicationID).FirstOrDefault();
                        var communicationData = db.Communications.Where(w => w.CommunicationID == communicationId).FirstOrDefault();
                        if (fieldId == (int)ContactFields.BlogUrl)
                            communicationData.BlogUrl = fieldValue;
                        else if (fieldId == (int)ContactFields.WebsiteUrl)
                            communicationData.WebSiteUrl = fieldValue;
                        else if (fieldId == (int)ContactFields.FacebookUrl)
                            communicationData.FacebookUrl = fieldValue;
                        else if (fieldId == (int)ContactFields.TwitterUrl)
                            communicationData.TwitterUrl = fieldValue;
                        else if (fieldId == (int)ContactFields.LinkedInUrl)
                            communicationData.LinkedInUrl = fieldValue;
                        else if (fieldId == (int)ContactFields.GooglePlusUrl)
                            communicationData.GooglePlusUrl = fieldValue;
                    }
                    else if (fieldId == (int)ContactFields.TitleField)
                    {
                        var contact = db.Contacts.Where(w => w.ContactID == contactId && w.AccountID == accountId).FirstOrDefault();
                        contact.Title = fieldValue;
                    }
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Gets the next batch to process.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TrackAction> GetNextBatchToProcess()
        {
            using (var db = ObjectContextFactory.Create())
            {
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandText = "[Workflow].[GetNextBatch]";

                db.Database.Connection.Open();

                // Run the sproc  
                var reader = cmd.ExecuteReader();

                // trackActions 
                var trackActionsDb = ((IObjectContextAdapter)db).ObjectContext.Translate<TrackActionsDb>(reader).ToList();
                reader.NextResult();

                var trackMessages = ((IObjectContextAdapter)db).ObjectContext.Translate<TrackMessagesDb>(reader).ToList();
                reader.NextResult();

                foreach (var item in trackActionsDb)
                {
                    item.TrackMessage = trackMessages.First(tm => tm.TrackMessageID == item.TrackMessageID);
                }

                var trackActions = Mapper.Map<IEnumerable<TrackActionsDb>, IEnumerable<TrackAction>>(trackActionsDb);

                //Workflow
                var workflows = ((IObjectContextAdapter)db).ObjectContext.Translate<WorkflowsDb>(reader).ToList();
                reader.NextResult();

                foreach (var item in trackActions)
                {
                    var workflow = workflows.First(w => w.WorkflowID == item.WorkflowID);
                    item.Workflow = Mapper.Map<WorkflowsDb, Workflow>(workflow);
                }

                //workflowCampaignActions
                var workflowCampaignActions = ((IObjectContextAdapter)db).ObjectContext.Translate<WorkflowCampaignActionsDb>(reader).ToList();
                reader.NextResult();

                foreach (var item in trackActions.Where(ta => (workflowCampaignActions.Select(wc => wc.WorkflowActionID)).Contains(ta.ActionID)))
                {
                    var dbObject = workflowCampaignActions.First(wc => wc.WorkflowActionID == item.ActionID);
                    item.WorkflowAction = Mapper.Map<WorkflowCampaignActionsDb, WorkflowCampaignAction>(dbObject);
                    item.WorkflowAction.WorkflowActionTypeID = WorkflowActionType.SendCampaign;
                }

                //workflowNotifyUserActions
                var workflowNotifyUserActions = ((IObjectContextAdapter)db).ObjectContext.Translate<WorkflowNotifyUserActionsDb>(reader).ToList();
                reader.NextResult();

                foreach (var item in trackActions.Where(ta => (workflowNotifyUserActions.Select(wc => wc.WorkflowActionID)).Contains(ta.ActionID)))
                {
                    var dbObject = workflowNotifyUserActions.First(wc => wc.WorkflowActionID == item.ActionID);
                    item.WorkflowAction = Mapper.Map<WorkflowNotifyUserActionsDb, WorkflowNotifyUserAction>(dbObject);
                    item.WorkflowAction.WorkflowActionTypeID = WorkflowActionType.NotifyUser;
                }

                //workflowAddTagAction
                var workflowAddTagActions = ((IObjectContextAdapter)db).ObjectContext.Translate<WorkflowTagActionsDb>(reader).ToList();
                reader.NextResult();

                foreach (var item in trackActions.Where(ta => (workflowAddTagActions.Select(wc => wc.WorkflowActionID)).Contains(ta.ActionID)))
                {
                    var dbObject = workflowAddTagActions.First(wc => wc.WorkflowActionID == item.ActionID);
                    item.WorkflowAction = Mapper.Map<WorkflowTagActionsDb, WorkflowTagAction>(dbObject);
                    item.WorkflowAction.WorkflowActionTypeID = WorkflowActionType.AddTag;
                }

                //workflowRemoveTagAction
                var workflowRemoveTagActions = ((IObjectContextAdapter)db).ObjectContext.Translate<WorkflowTagActionsDb>(reader).ToList();
                reader.NextResult();

                foreach (var item in trackActions.Where(ta => (workflowRemoveTagActions.Select(wc => wc.WorkflowActionID)).Contains(ta.ActionID)))
                {
                    var dbObject = workflowRemoveTagActions.First(wc => wc.WorkflowActionID == item.ActionID);
                    item.WorkflowAction = Mapper.Map<WorkflowTagActionsDb, WorkflowTagAction>(dbObject);
                    item.WorkflowAction.WorkflowActionTypeID = WorkflowActionType.RemoveTag;
                }

                //workFlowLeadScoreActions
                var workFlowLeadScoreActions = ((IObjectContextAdapter)db).ObjectContext.Translate<WorkflowLeadScoreActionsDb>(reader).ToList();
                reader.NextResult();

                foreach (var item in trackActions.Where(ta => (workFlowLeadScoreActions.Select(wc => wc.WorkflowActionID)).Contains(ta.ActionID)))
                {
                    var dbObject = workFlowLeadScoreActions.First(wc => wc.WorkflowActionID == item.ActionID);
                    item.WorkflowAction = Mapper.Map<WorkflowLeadScoreActionsDb, WorkflowLeadScoreAction>(dbObject);
                    item.WorkflowAction.WorkflowActionTypeID = WorkflowActionType.AdjustLeadScore;
                }

                //workFlowLifeCycleActions
                var workFlowLifeCycleActions = ((IObjectContextAdapter)db).ObjectContext.Translate<WorkflowLifeCycleActionsDb>(reader).ToList();
                reader.NextResult();

                foreach (var item in trackActions.Where(ta => (workFlowLifeCycleActions.Select(wc => wc.WorkflowActionID)).Contains(ta.ActionID)))
                {
                    var dbObject = workFlowLifeCycleActions.First(wc => wc.WorkflowActionID == item.ActionID);
                    item.WorkflowAction = Mapper.Map<WorkflowLifeCycleActionsDb, WorkflowLifeCycleAction>(dbObject);
                    item.WorkflowAction.WorkflowActionTypeID = WorkflowActionType.ChangeLifecycle;
                }

                //workflowContactFieldActions
                var workflowContactFieldActions = ((IObjectContextAdapter)db).ObjectContext.Translate<WorkflowContactFieldActionsDb>(reader).ToList();
                reader.NextResult();

                foreach (var item in trackActions.Where(ta => (workflowContactFieldActions.Select(wc => wc.WorkflowActionID)).Contains(ta.ActionID)))
                {
                    var dbObject = workflowContactFieldActions.First(wc => wc.WorkflowActionID == item.ActionID);
                    item.WorkflowAction = Mapper.Map<WorkflowContactFieldActionsDb, WorkflowContactFieldAction>(dbObject);
                    item.WorkflowAction.WorkflowActionTypeID = WorkflowActionType.UpdateField;
                }

                //workFlowUserAssignmentActions
                var workFlowUserAssignmentActions = ((IObjectContextAdapter)db).ObjectContext.Translate<WorkFlowUserAssignmentActionsDb>(reader).ToList();
                reader.NextResult();

                foreach (var item in trackActions.Where(ta => (workFlowUserAssignmentActions.Select(wc => wc.WorkflowActionID)).Contains(ta.ActionID)))
                {
                    var dbObject = workFlowUserAssignmentActions.First(wc => wc.WorkflowActionID == item.ActionID);
                    item.WorkflowAction = Mapper.Map<WorkFlowUserAssignmentActionsDb, WorkflowUserAssignmentAction>(dbObject);
                    item.WorkflowAction.WorkflowActionTypeID = WorkflowActionType.AssignToUser;
                }

                //triggerWorkflowActions
                var triggerWorkflowActions = ((IObjectContextAdapter)db).ObjectContext.Translate<TriggerWorkflowActionsDb>(reader).ToList();
                reader.NextResult();

                foreach (var item in trackActions.Where(ta => (triggerWorkflowActions.Select(wc => wc.WorkflowActionID)).Contains(ta.ActionID)))
                {
                    var dbObject = triggerWorkflowActions.First(wc => wc.WorkflowActionID == item.ActionID);
                    item.WorkflowAction = Mapper.Map<TriggerWorkflowActionsDb, TriggerWorkflowAction>(dbObject);
                    item.WorkflowAction.WorkflowActionTypeID = WorkflowActionType.TriggerWorkflow;
                }
                //send emails
                var sendEmailActions = ((IObjectContextAdapter)db).ObjectContext.Translate<WorkflowEmailNotificationActionDb>(reader).ToList();
                reader.NextResult();

                foreach (var item in trackActions.Where(ta => (sendEmailActions.Select(wc => wc.WorkflowActionID)).Contains(ta.ActionID)))
                {
                    var dbObject = sendEmailActions.First(wc => wc.WorkflowActionID == item.ActionID);
                    item.WorkflowAction = Mapper.Map<WorkflowEmailNotificationActionDb, WorkflowEmailNotificationAction>(dbObject);
                    item.WorkflowAction.WorkflowActionTypeID = WorkflowActionType.SendEmail;
                }

                db.Database.Connection.Close();

                return trackActions;
            }
        }

        /// <summary>
        /// Updates the action batch status.
        /// </summary>
        /// <param name="trackActions">The track actions.</param>
        /// <param name="trackActionLogs">The track action logs.</param>
        public void UpdateActionBatchStatus(IEnumerable<TrackAction> trackActions, IEnumerable<TrackActionLog> trackActionLogs)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var trackActionsDataTable = new System.Data.DataTable();
                trackActionsDataTable.Columns.Add("TrackActionID", typeof(long));
                trackActionsDataTable.Columns.Add("ExecutedOn", typeof(DateTime));
                trackActionsDataTable.Columns.Add("ActionProcessStatusID", typeof(short));

                var trackActionLogDataTable = new System.Data.DataTable();
                trackActionLogDataTable.Columns.Add("TrackActionID", typeof(long));
                trackActionLogDataTable.Columns.Add("ErrorMessage", typeof(string));

                foreach (var item in trackActions)
                {
                    trackActionsDataTable.Rows.Add(item.TrackActionID, item.ExecutedOn, (short)((item.ActionProcessStatusID == TrackActionProcessStatus.Undefined) ? TrackActionProcessStatus.Error : item.ActionProcessStatusID));
                }
                trackActionsDataTable.AcceptChanges();

                foreach (var item in trackActionLogs)
                {
                    trackActionLogDataTable.Rows.Add(item.TrackActionID, string.IsNullOrWhiteSpace(item.ErrorMessage) ? "n/a empty message" : item.ErrorMessage);
                }
                trackActionLogDataTable.AcceptChanges();

                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                sqlParameters.Add(new SqlParameter { ParameterName = "@trackActionItems", Value = trackActionsDataTable, SqlDbType = System.Data.SqlDbType.Structured, TypeName = "[Workflow].[TrackActionType]" });
                sqlParameters.Add(new SqlParameter { ParameterName = "@trackActionLogItems", Value = trackActionLogDataTable, SqlDbType = System.Data.SqlDbType.Structured, TypeName = "Workflow.TrackActionLogType" });

                db.ExecuteStoredProcedure("Workflow.updateTrackActionLogs", sqlParameters);

                trackActionsDataTable.Dispose();
                trackActionLogDataTable.Dispose();
            }
        }

        /// <summary>
        /// Gets the name of the saved search.
        /// </summary>
        /// <param name="savedSearchId">The saved search identifier.</param>
        /// <returns></returns>
        public string GetSavedSearchName(int savedSearchId)
        {
            var db = ObjectContextFactory.Create();
            string name = db.SearchDefinitions.Where(s => s.SearchDefinitionID == savedSearchId).Select(n => n.SearchDefinitionName).FirstOrDefault();
            return name;
        }

        /// <summary>
        /// Getting All Notification Contact Fields Data
        /// </summary>
        /// <param name="contactId"></param>
        /// <param name="fields"></param>
        /// <param name="notificationType"></param>
        /// <param name="entityId"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public NotificationData GetAllNotificationContactFieldsData(int? contactId, string fields, int notificationType, int? entityId, int accountId)
        {
            var db = ObjectContextFactory.Create();
            NotificationData notificationData = new NotificationData();
            IEnumerable<NotificationContactFieldData> fielddData = null;
            string formSubmittedData = null;
            db.QueryStoredProc("[dbo].[GetContactFieldsData]", (reader) =>
            {
                fielddData = reader.Read<NotificationContactFieldData>().ToList();
                formSubmittedData = reader.Read<string>().FirstOrDefault();

            }, new
            {
                ContactId = contactId,
                Fields = fields,
                NotificationType = notificationType,
                EntityID = entityId,
                AccountID = accountId
            }, commandTimeout: 600);

            notificationData.FieldsData = fielddData;
            notificationData.SubmittedData = formSubmittedData;
            return notificationData;
        }

        /// <summary>
        /// Get All Campaigns By Name
        /// </summary>
        /// <param name="AccountID"></param>
        /// <param name="IsWorkflowCampaign"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<Campaign> GetCampaigns(int AccountID, bool IsWorkflowCampaign, string name)
        {
            var db = ObjectContextFactory.Create();
            var sql = string.Empty;
            IEnumerable<CampaignsDb> campaigns = null;
            if (IsWorkflowCampaign)
                sql = @"SELECT TOP 10 C.CampaignID,C.Name FROM Campaigns (NOLOCK) C WHERE  C.CampaignStatusID =107 and C.IsLinkedToWorkflows=1 AND C.IsDeleted=0 AND C.AccountID=@id  AND C.name like '%" + name + "%'";
            else
                sql = @"SELECT TOP 10 C.CampaignID,C.Name FROM Campaigns (NOLOCK) C WHERE  C.CampaignStatusID in(101,102,105) AND C.IsDeleted = 0 AND C.AccountID = @id  AND C.name like '%" + name + "%'";
            campaigns = db.Get<CampaignsDb>(sql, new { id = AccountID });

            foreach (CampaignsDb dc in campaigns)
                yield return Mapper.Map<CampaignsDb, Campaign>(dc);
            // return campaigns;

        }

        public void AssignUser(int contactId, int workflowId, int userAssignmentActionID, byte scheduledID)
        {
            var db = ObjectContextFactory.Create();

            var procName = "[dbo].[Workflow_AssignUser]";
            var parms = new List<SqlParameter>
				{   
					new SqlParameter{ParameterName="@contactID", Value = contactId},
					new SqlParameter{ParameterName="@workflowID", Value = workflowId },
					new SqlParameter{ParameterName="@userAssignmentActionID", Value = userAssignmentActionID },
                    new SqlParameter{ParameterName="@ScheduledID", Value = scheduledID}
				};

            db.Database.Connection.Open();
            var cmd = db.Database.Connection.CreateCommand();
            cmd.CommandText = procName;
            cmd.CommandType = CommandType.StoredProcedure;
            parms.ForEach(p =>
            {
                cmd.Parameters.Add(p);
            });

            DbDataReader reader = cmd.ExecuteReader();
        }

        /// <summary>
        /// Updating workflow name.
        /// </summary>
        /// <param name="workflowName"></param>
        /// <param name="workflowId"></param>
        public void UpdateWorkflowName(string workflowName, int workflowId)
        {
            var db = ObjectContextFactory.Create();
            WorkflowsDb workflowDb = db.Workflows.Where(w => w.WorkflowID == workflowId).FirstOrDefault();
            workflowDb.WorkflowName = workflowName;
            db.Entry(workflowDb).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
        }

        /// <summary>
        /// Get NotifyUserAction by Id
        /// </summary>
        /// <param name="workflowActionId"></param>
        /// <returns></returns>
        public WorkflowNotifyUserAction GetNotifyUserActionById(int workflowActionId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT * FROM WorkflowNotifyUserAction (NOLOCK) WHERE WorkflowActionID = @actionId";
            WorkflowNotifyUserActionsDb userActionDb = db.Get<WorkflowNotifyUserActionsDb>(sql, new { actionId = workflowActionId }).FirstOrDefault();
            WorkflowNotifyUserAction userAction = Mapper.Map<WorkflowNotifyUserActionsDb, WorkflowNotifyUserAction>(userActionDb);
            return userAction;

        }

        /// <summary>
        /// Updating Notify User Action
        /// </summary>
        /// <param name="action"></param>
        public void UpdateNotifyUserAction(WorkflowNotifyUserAction action)
        {
            var db = ObjectContextFactory.Create();
            WorkflowNotifyUserActionsDb userActionDb = Mapper.Map<WorkflowNotifyUserAction, WorkflowNotifyUserActionsDb>(action);
            db.Entry(userActionDb).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
        }

        /// <summary>
        /// Getting Workflow user assignment action by actionid
        /// </summary>
        /// <param name="workflowActionId"></param>
        /// <returns></returns>
        public WorkflowAction GetWorkflowAssignmentActionById(int workflowActionId)
        {
            var db = ObjectContextFactory.Create();
            var workflowAction = db.WorkflowActions.Where(wa => wa.WorkflowActionID == workflowActionId).FirstOrDefault();
            workflowAction.Action = workflowAction.GetAction(db);
            WorkflowAction action = Mapper.Map<WorkflowActionsDb, WorkflowAction>(workflowAction);
            return action;
        }

        /// <summary>
        /// Updating Workflow user assignment action
        /// </summary>
        /// <param name="action"></param>
        public void UpdateUserAssignmentAction(WorkflowUserAssignmentAction action)
        {
            var db = ObjectContextFactory.Create();
            WorkFlowUserAssignmentActionsDb userActionDb = Mapper.Map<WorkflowUserAssignmentAction, WorkFlowUserAssignmentActionsDb>(action);
            userActionDb.Save(db);
            db.SaveChanges();
        }

        /// <summary>
        /// Get All Parent Workflows
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public IEnumerable<ParentWorkflow> GetAllParentWorkflows(int workflowId)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<ParentWorkflow> parentWorkflows = new List<ParentWorkflow>();

            db.QueryStoredProc("[dbo].[GetParentWorkflows]", (reader) =>
             {
                 parentWorkflows = reader.Read<ParentWorkflow>().ToList();
             }, new
             {
                 WorkflowId = workflowId,
             }, commandTimeout: 600);

            return parentWorkflows;
        }

        /// <summary>
        /// Getting Workflow Id From Parent wf id
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public int GetWorkflowIdByParentId(int workflowId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT WorkflowID  FROM Workflows WHERE ParentWorkflowID=@workflowId";
            int workflowID = db.Get<int>(sql, new { workflowId = workflowId }).FirstOrDefault();
            return workflowID;

        }

        /// <summary>
        /// Getting Saved Searcg definitions by wf id
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public IEnumerable<int> GetTriggersByParentWorkflowID(IList<int> workflowIds)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT SearchDefinitionID  FROM  WorkflowTriggers (NOLOCK) WHERE WorkflowID IN @workflowIDs AND TriggerTypeID=1 AND IsStartTrigger=1 AND SearchDefinitionID > 0";
            IEnumerable<int> searchDefinitionIds = db.Get<int>(sql, new { workflowIDs = workflowIds }).ToList();
            return searchDefinitionIds;
        }

        /// <summary>
        /// Workflow Name duplicate checking
        /// </summary>
        /// <param name="name"></param>
        /// <param name="workflowId"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public bool WorkflowNameDuplicateCheck(string name, int workflowId, int accountId)
        {
            var db = ObjectContextFactory.Create();
            List<int> workflowIds = new List<int>();
            IEnumerable<ParentWorkflow> workflowParentIds = GetAllParentWorkflows(workflowId);
            workflowIds.AddRange(workflowParentIds.Select(s => s.WorkflowID).ToArray());

            var workflowFound = db.Workflows.Where(c => c.WorkflowName.Equals(name, StringComparison.CurrentCultureIgnoreCase)
                                                    && c.AccountID == accountId
                                                    && !workflowIds.Contains(c.WorkflowID) && c.IsDeleted == false)
                                           .Select(c => c).FirstOrDefault();
            if (workflowFound != null)
            {
                return false;
            }
            return true;
        }

        public bool CheckIfAttachementNeeded(int formId)
        {
            var db = ObjectContextFactory.Create();
            return db.FormSettings.Where(w => w.FormID == formId).Any();
        }

        public WorkflowGoalStatus HasContactMatchedEndTrigger(int contactId, int workflowId, long trackMessageID)
        {
            Logger.Current.Informational(string.Format("Check if contact meets workflow goal, contactid {0}, workflowid {1}, trackmessageID {2}", contactId, workflowId, trackMessageID));
            using (var db = ObjectContextFactory.Create())
            {
                WorkflowGoalStatus status = new WorkflowGoalStatus();
                var sp = "[dbo].[CheckIfMatchedEndTrigger]";
                var parameters = new { contactId = contactId, workflowId = workflowId, trackMessageID = trackMessageID };
                var key = DapperExtensions.GetHashedKey(sp + parameters.ToString());
                var cache = new MemoryCacheManager();
                var cacheResult = cache.Get(key);
                if (cacheResult != null)
                {
                    try
                    {
                        Logger.Current.Informational("getting from cache");
                        status = (WorkflowGoalStatus)cacheResult;
                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Error("An error occured while fetching cached data", ex);
                        db.QueryStoredProc(sp, reader =>
                        {
                            status = reader.Read<WorkflowGoalStatus>().FirstOrDefault();
                        }, parameters);
                        cache.Add(key, (object)status, DateTime.Now.AddMinutes(3));
                    }
                }
                else
                {
                    db.QueryStoredProc(sp, reader =>
                    {
                        status = reader.Read<WorkflowGoalStatus>().FirstOrDefault();
                    }, parameters);
                    cache.Add(key, (object)status, DateTime.Now.AddMinutes(3));
                }
                return status;
            }
        }

        public bool IsCreatedBy(int workflowId, int userId, int accountId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"SELECT WorkflowID FROM Workflows (NOLOCK) WHERE WorkflowID=@WorkflowId AND CreatedBy=@UserId AND AccountID=@AccountId";
                int Id = db.Get<int>(sql, new { WorkflowId = workflowId, UserId = userId, AccountId = accountId }).FirstOrDefault();
                if (Id > 0)
                    return true;
                else
                    return false;

            }
        }

    }

    public class WorkflowCount
    {
        public int WorkflowID { get; set; }
        public int ContactsCount { get; set; }
    }
}
