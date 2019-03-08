using AutoMapper;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.CommunicationManager.Requests;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.User;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.Messaging.Forms;
using SmartTouch.CRM.ApplicationServices.Messaging.LeadAdapters;
using SmartTouch.CRM.ApplicationServices.Messaging.Search;
using SmartTouch.CRM.ApplicationServices.Messaging.Tags;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow;
using SmartTouch.CRM.ApplicationServices.ServiceAgents;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.CustomFields;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Domain.Reports;
using SmartTouch.CRM.Domain.Search;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.WebAnalytics;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository;
using SmartTouch.CRM.Repository.Database;
using SmartTouch.CRM.SearchEngine.Search;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using SmartTouch.CRM.ApplicationServices.ObjectMappers;
using Newtonsoft.Json;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using RestSharp;
using LandmarkIT.Enterprise.Utilities.PDFGeneration;
using SmartTouch.CRM.Domain.Contacts;
using LandmarkIT.Enterprise.Utilities.Caching;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class WorkflowService : IWorkflowService
    {
        readonly IWorkflowRepository workflowRepository;
        readonly ICachingService cachingService;
        readonly ICampaignRepository campaignRepository;
        readonly IUserRepository userRepository;
        readonly IAccountRepository accountRepository;
        readonly IServiceProviderRepository serviceProviderRepository;
        readonly ITagRepository tagRepository;
        readonly IContactRepository contactRepository;
        readonly ISearchService<Workflow> searchService;
        readonly IMessageService messageService;
        readonly IFormService formService;
        readonly IContactService contactService;
        readonly IAdvancedSearchService advancedSearchService;
        readonly ICustomFieldRepository customFieldRepository;

        readonly IUnitOfWork unitOfWork;
        readonly ILeadAdapterService leadAdapterService;
        readonly IWebAnalyticsProviderRepository webAnalyticsProviderRepository;
        readonly IUrlService urlService;
        readonly IAccountService accountService;

        public WorkflowService(IWorkflowRepository workflowRepository, IUnitOfWork unitOfWork, ICachingService cachingService, ISearchService<Workflow> searchService, IContactService contactService,
                               ICampaignRepository campaignRepository, IUserRepository userRepository, IAccountRepository accountRepository, IServiceProviderRepository serviceProvider, IContactRepository contactRepository,
                               ITagRepository tagRepository, IMessageService messageService, IFormService formService, IWebAnalyticsProviderRepository webAnalyticsProviderRepository,
                               IAdvancedSearchService advancedSearchService, ILeadAdapterService leadAdapterService, IUrlService urlService, IAccountService accountService, ICustomFieldRepository customFieldRepository)
        {
            this.workflowRepository = workflowRepository;
            this.unitOfWork = unitOfWork;
            this.searchService = searchService;
            this.cachingService = cachingService;
            this.campaignRepository = campaignRepository;
            this.userRepository = userRepository;
            this.accountRepository = accountRepository;
            this.serviceProviderRepository = serviceProvider;
            this.messageService = messageService;
            this.formService = formService;
            this.advancedSearchService = advancedSearchService;
            this.leadAdapterService = leadAdapterService;
            this.tagRepository = tagRepository;
            this.webAnalyticsProviderRepository = webAnalyticsProviderRepository;
            this.urlService = urlService;
            this.accountService = accountService;
            this.customFieldRepository = customFieldRepository;
            this.contactService = contactService;
            this.contactRepository = contactRepository;
        }


        public GetWorkflowListResponse GetAllWorkFlows(GetWorkflowListRequest request)
        {
            Logger.Current.Verbose("Request received for fetching the workflows");
            GetWorkflowListResponse response = new GetWorkflowListResponse();

            if (request.SortField != null)
            {

                var maps = MapperConfigurationProvider.Instance.FindTypeMapFor<WorkFlowViewModel, Workflow>();

                foreach (var propertyMap in maps.GetPropertyMaps())
                {
                    if (propertyMap.SourceMember != null && request.SortField.Equals(propertyMap.SourceMember.Name))
                    {
                        request.SortField = propertyMap.DestinationProperty.MemberInfo.Name;
                        break;
                    }
                }
            }
            IEnumerable<Workflow> workflows = workflowRepository.FindAll(request.Query, request.Limit, request.PageNumber, (short)request.Status, request.AccountId, request.SortField, request.SortDirection);
            if (workflows == null)
            {
                response.Exception = GetWorkflowNotFoundException();
            }
            else
            {
                IEnumerable<WorkFlowViewModel> workflowslist = Mapper.Map<IEnumerable<Workflow>, IEnumerable<WorkFlowViewModel>>(workflows);
                response.Workflows = workflowslist;
                response.TotalHits = workflowslist.Any() ? workflowslist.Select(s => s.TotalWorkflowCount).FirstOrDefault() : 0;
            }
            return response;
        }

        public GetActiveWorkflowsResponse GetAllActiveWorkflows(GetActiveWorkflowsRequest request)
        {
            Logger.Current.Informational("Request received for fetching all active workflows for all accounts");
            GetActiveWorkflowsResponse response = new GetActiveWorkflowsResponse();
            var Sortfield = GetPropertyName<WorkFlowViewModel, string>(wf => wf.WorkflowName);
            IEnumerable<Workflow> workflows = workflowRepository.FindActiveWorkflows(request.PageNumber, request.Limit, Sortfield, ListSortDirection.Descending);
            if (workflows == null)
                response.Exception = GetWorkflowNotFoundException();
            else
            {
                IEnumerable<WorkFlowViewModel> workflowslist = Mapper.Map<IEnumerable<Workflow>, IEnumerable<WorkFlowViewModel>>(workflows);
                response.Workflows = workflowslist;
            }
            return response;
        }

        private UnsupportedOperationException GetWorkflowNotFoundException()
        {
            return new UnsupportedOperationException("The requested workflow was not found.");
        }

        public InsertWorkflowResponse InsertWorkflow(InsertWorkflowRequest request)
        {
            Logger.Current.Verbose("Request received to insert a new workflow.");
            int workflowId = 0;
            try
            {
                Workflow workflow = Mapper.Map<WorkFlowViewModel, Workflow>(request.WorkflowViewModel);
                workflow.AccountID = request.AccountId;
                workflow.CreatedBy = request.RequestedBy.Value;
                workflow.CreatedOn = DateTime.Now.ToUniversalTime();
                workflow.ModifiedBy = request.RequestedBy.Value;
                workflow.ModifiedOn = DateTime.Now.ToUniversalTime();

                bool isWorkflowNameUnique = workflowRepository.IsWorkflowNameUnique(workflow);
                if (!isWorkflowNameUnique)
                {
                    Logger.Current.Verbose("Duplicate workflow identified," + workflow.WorkflowName);
                    var message = "[|Workflow with name|] \"" + workflow.WorkflowName + "\" [|already exists.|] " + "[|Please choose a different name|]";
                    throw new UnsupportedOperationException(message);
                }
                isWorkflowValid(workflow);
                if (workflow.ParentWorkflowID != 0)
                    workflowRepository.UpdateWorkflowStatus(WorkflowStatus.Archive, workflow.ParentWorkflowID, workflow.ModifiedBy.Value);

                workflowRepository.Insert(workflow);
                workflow = unitOfWork.Commit() as Workflow;
                workflowId = workflow.WorkflowID;
                var actions = workflow.WorkflowActions.ToList();
                var updatedActions = new List<WorkflowAction>();
                actions.ForEach(a =>
                    {
                        updatedActions.Add(UpdateAction(a, request.AccountId, request.RequestedBy.Value));
                    });
                workflow.WorkflowActions = updatedActions;
                //workflowRepository.UpdateLinkActions(workflow);
                workflowRepository.InsertWorkflowEndAction(workflow.WorkflowID);

                workflow.WorkflowActions.Each(a =>
                {
                    if (a.WorkflowActionTypeID == WorkflowActionType.SetTimer)
                    {
                        BaseWorkflowActionViewModel actionViewModel = Mapper.Map<BaseWorkflowAction, BaseWorkflowActionViewModel>(a.Action);
                        if (((WorkflowTimerActionViewModel)actionViewModel).RunAt != null)
                            Logger.Current.Informational("workflow  set timer action time zone(when Inserting workflow in service): " + ((WorkflowTimerActionViewModel)actionViewModel).RunAt);
                        else if (((WorkflowTimerActionViewModel)actionViewModel).RunAtTime != null)
                            Logger.Current.Informational("workflow set timer action time zone(when Inserting workflow in service): " + ((WorkflowTimerActionViewModel)actionViewModel).RunAtTime);
                    }
                });
                Logger.Current.Informational("Workflow inserted successfully.");
                IEnumerable<ParentWorkflow> parentWorkflows = workflowRepository.GetAllParentWorkflows(workflow.ParentWorkflowID);
                IList<int> workflowIds = parentWorkflows.Select(s => s.WorkflowID).ToList();
                IEnumerable<int> searchDefinitionIds = workflowRepository.GetTriggersByParentWorkflowID(workflowIds);

                if (workflow.StatusID == WorkflowStatus.Active)
                    addToTopic(workflow.WorkflowID, request.AccountId);
                if (workflow.StatusID == WorkflowStatus.Active && workflow.DeactivatedOn.HasValue)
                    addToTopic(workflow.WorkflowID, request.AccountId, workflow.DeactivatedOn.Value);

                //IEnumerable<int?> PresentedTriggerIds = workflow.Triggers.Where(a => a.TriggerTypeID == WorkflowTriggerType.SmartSearch && a.IsStartTrigger == true && searchDefinitionIds.Contains(a.SearchDefinitionID.Value)).Select(s => s.SearchDefinitionID).ToList();

                if (searchDefinitionIds != null && searchDefinitionIds.Any())
                    workflow.Triggers = workflow.Triggers.Where(a => a.TriggerTypeID == WorkflowTriggerType.SmartSearch
                                                                             && a.IsStartTrigger == true
                                                                             && !searchDefinitionIds.Contains(a.SearchDefinitionID.Value));


                if (workflow.WorkflowID > 0 && workflow.StatusID == WorkflowStatus.Active && workflow.Triggers != null && workflow.Triggers.Any())//&& PresentedTriggerIds.Count() == 0
                {
                    Logger.Current.Informational("For Getting SavedSearch Contacts By SearchdefinitionID In Workflow Insert");
                    addSavedSearchContactsToTopic(workflow, request.RoleId);
                }
                return new InsertWorkflowResponse();
            }
            catch
            {
                if (workflowId > 0)
                    DeleteWorkflow(new DeleteWorkflowRequest()
                    {
                        WorkflowIDs = new int[]{
                            workflowId
                        }
                    });
                throw;
            }
        }

        private WorkflowAction UpdateAction(WorkflowAction action, int accountId, int by)
        {
            Action<WorkflowLeadScoreAction> updateLeadScoreRule = (a) =>
            {
                var actionDb = Mapper.Map<WorkflowLeadScoreActionsDb>(a);
                actionDb.UpdateLeadScoreRule(accountId, by);
            };
            action = workflowRepository.UpdateAction(action);
            if (action.Action is WorkflowLeadScoreAction)
            {
                updateLeadScoreRule((WorkflowLeadScoreAction)action.Action);
            }
            else if (action.Action is WorkflowCampaignAction)
            {
                var campaignAction = action.Action as WorkflowCampaignAction;
                campaignAction.Links.ToList().ForEach(l =>
                {
                    l.Actions.ToList().ForEach(al =>
                    {
                        if (al.Action is WorkflowLeadScoreAction)
                        {
                            updateLeadScoreRule((WorkflowLeadScoreAction)al.Action);
                        }
                        else if (al.Action is WorkflowUserAssignmentAction)
                        {
                            if (!al.IsDeleted)
                            {
                                var assignmentAction = al.Action as WorkflowUserAssignmentAction;
                                if (assignmentAction.RoundRobinContactAssignments != null && assignmentAction.RoundRobinContactAssignments.Any())
                                    assignmentAction.RoundRobinContactAssignments.ToList().ForEach(f =>
                                    {
                                        var rrAction = Mapper.Map<RoundRobinContactAssignment, RoundRobinContactAssignmentDb>(f);
                                        rrAction.Save(assignmentAction.WorkflowUserAssignmentActionID);
                                    });
                            }

                        }
                    });
                });
            }
            else if (action.Action is WorkflowUserAssignmentAction)
            {
                var assignmentAction = action.Action as WorkflowUserAssignmentAction;
                if (!action.IsDeleted)
                {
                    if (assignmentAction.RoundRobinContactAssignments != null && assignmentAction.RoundRobinContactAssignments.Any())
                        assignmentAction.RoundRobinContactAssignments.ToList().ForEach(f =>
                        {
                            var rrAction = Mapper.Map<RoundRobinContactAssignment, RoundRobinContactAssignmentDb>(f);
                            rrAction.Save(assignmentAction.WorkflowUserAssignmentActionID);
                        });
                }

            }
            return action;
        }

        public UpdateWorkflowResponse UpdateWorkflow(UpdateWorkflowRequest request)
        {
            Logger.Current.Verbose("Request received to update workflow with WorkflowID " + request.WorkflowViewModel.WorkflowID);

            Workflow workflow = Mapper.Map<WorkFlowViewModel, Workflow>(request.WorkflowViewModel);
            workflow.ModifiedBy = request.RequestedBy;
            workflow.ModifiedOn = DateTime.Now.ToUniversalTime();
            isWorkflowValid(workflow);
            bool isWorkflowNameUnique = workflowRepository.IsWorkflowNameUnique(workflow);
            if (!isWorkflowNameUnique)
            {
                Logger.Current.Verbose("Duplicate workflow identified," + workflow.WorkflowName);
                var message = "[|Workflow with name|] \"" + workflow.WorkflowName + "\" [|already exists.|] " + "[|Please choose a different name|]";
                throw new UnsupportedOperationException(message);
            }
            if (workflow.StatusID == WorkflowStatus.InActive || workflow.StatusID == WorkflowStatus.Paused)
            {
                IEnumerable<string> workflowNames = workflowRepository.checkToUpdateStatus(workflow.WorkflowID);
                if (workflowNames != null && workflowNames.Any())
                {
                    Logger.Current.Verbose("Can't update status to Inactive or Paused as this workflow is being used by another workflow");
                    var names = string.Join(", ", workflowNames);
                    var message = "[|This workflow is being used in |] " + names + " [|workflows|]";
                    throw new UnsupportedOperationException(message);
                }
            }
            if (workflow.ParentWorkflowID != 0)
                workflowRepository.UpdateWorkflowStatus(WorkflowStatus.Archive, workflow.ParentWorkflowID, workflow.ModifiedBy.Value);

            workflowRepository.Update(workflow);
            workflow = unitOfWork.Commit() as Workflow;
            var actions = workflow.WorkflowActions.ToList();
            var updatedActions = new List<WorkflowAction>();

            actions.ForEach(a =>
            {
                updatedActions.Add(UpdateAction(a, workflow.AccountID, request.RequestedBy.Value));
            });
            workflow.WorkflowActions = updatedActions;
            //workflowRepository.UpdateLinkActions(workflow);
            workflow.WorkflowActions.Each(a =>
            {
                if (a.WorkflowActionTypeID == WorkflowActionType.SetTimer)
                {
                    BaseWorkflowActionViewModel actionViewModel = Mapper.Map<BaseWorkflowAction, BaseWorkflowActionViewModel>(a.Action);
                    if (((WorkflowTimerActionViewModel)actionViewModel).RunAt != null)
                        Logger.Current.Informational("workflow  set timer action time zone(when updating workflow in service): " + ((WorkflowTimerActionViewModel)actionViewModel).RunAt);
                    else if (((WorkflowTimerActionViewModel)actionViewModel).RunAtTime != null)
                        Logger.Current.Informational("workflow set timer action time zone(when updating workflow in service): " + ((WorkflowTimerActionViewModel)actionViewModel).RunAtTime);
                }
            });
            unitOfWork.Commit();

            IEnumerable<ParentWorkflow> parentWorkflows = workflowRepository.GetAllParentWorkflows(workflow.ParentWorkflowID);
            IList<int> workflowIds = parentWorkflows.Select(s => s.WorkflowID).ToList();
            IEnumerable<int> searchDefinitionIds = workflowRepository.GetTriggersByParentWorkflowID(workflowIds);
            // IEnumerable<int?> PresentedTriggerIds = workflow.Triggers.Where(a => a.TriggerTypeID == WorkflowTriggerType.SmartSearch && a.IsStartTrigger == true && searchDefinitionIds.Contains(a.SearchDefinitionID.Value)).Select(s => s.SearchDefinitionID).ToList();
            if (searchDefinitionIds != null && searchDefinitionIds.Any())
                workflow.Triggers = workflow.Triggers.Where(a => a.TriggerTypeID == WorkflowTriggerType.SmartSearch
                                                                         && a.IsStartTrigger == true
                                                                         && !searchDefinitionIds.Contains(a.SearchDefinitionID.Value));

            if (workflow.StatusID == WorkflowStatus.Active)
                addToTopic(workflow.WorkflowID, request.AccountId);
            if (workflow.StatusID == WorkflowStatus.Active && workflow.DeactivatedOn.HasValue)
                addToTopic(workflow.WorkflowID, request.AccountId, workflow.DeactivatedOn.Value);
            //TODO
            //if the workflow is changed from paused to active then follwing code shouldn't be called.
            if (workflow.WorkflowID > 0 && workflow.StatusID == WorkflowStatus.Active && workflow.Triggers != null && workflow.Triggers.Any()) // && PresentedTriggerIds.Count() == 0)
            {
                Logger.Current.Informational("For Getting SavedSearch Contacts By SearchdefinitionID In Workflow Update");
                addSavedSearchContactsToTopic(workflow, request.RoleId);
            }
            return new UpdateWorkflowResponse();
        }

        public DeleteWorkflowResponse DeleteWorkflow(DeleteWorkflowRequest request)
        {
            Logger.Current.Verbose("Request received for deleting the workflows");
            workflowRepository.DeleteWorkFlows(request.WorkflowIDs);
            return new DeleteWorkflowResponse();
        }

        void isWorkflowValid(Workflow workflow)
        {
            Logger.Current.Verbose("Request received to validate workflow with Workflowid " + workflow.Id);
            IEnumerable<BusinessRule> brokenRules = workflow.GetBrokenRules();
            if (brokenRules != null && brokenRules.Any())
            {
                StringBuilder brokenRulesBuilder = new StringBuilder();
                foreach (BusinessRule rule in brokenRules.Distinct())
                {
                    brokenRulesBuilder.AppendLine(rule.RuleDescription);
                }
                throw new UnsupportedOperationException(brokenRulesBuilder.ToString());
            }
        }

        public GetWorkflowResponse GetWorkFlow(GetWorkflowRequest request)
        {
            GetWorkflowResponse response = new GetWorkflowResponse();
            Logger.Current.Verbose("Request received to fetch the workflow with workflowID: " + request.WorkflowID);
            if (!request.RequestFromAutomationService)
                hasAccess(request.WorkflowID, request.RequestedBy, request.AccountId, request.RoleId);
            Workflow workflow = workflowRepository.GetWorkflowByID(request.WorkflowID, request.AccountId);
            workflow.Triggers.Each(trigger =>
            {
                if (trigger.TriggerTypeID == WorkflowTriggerType.LinkClicked)
                {
                    trigger.SelectedURLs = workflowRepository.GetSelectedLinkNamesinTrigger(trigger.CampaignID.Value);
                }
            });
            WorkFlowViewModel WorkflowViewModel = Mapper.Map<Workflow, WorkFlowViewModel>(workflow);

            IEnumerable<ParentWorkflow> parentworkflows = workflowRepository.GetAllParentWorkflows(request.WorkflowID);

            response.ParentWorkflows = Mapper.Map<IEnumerable<ParentWorkflow>, IEnumerable<ParentWorkflowViewModel>>(parentworkflows);

            WorkflowViewModel.WorkflowActions.ToList().ForEach(a =>
            {
                if (a.WorkflowActionTypeID == WorkflowActionType.UpdateField)
                {
                    var workflowModel = (WorkflowContactFieldActionViewModel)a.Action;
                    var valueOptionRequiredFields = new List<byte>() { 1, 6, 11, 12 };
                    if (valueOptionRequiredFields.Contains((byte)workflowModel.FieldInputTypeId))
                    {
                        int? contactDropdown = workflowModel.FieldID == (int)ContactFields.LeadSource ? (int)DropdownFieldTypes.LeadSources : workflowModel.FieldID == (int)ContactFields.LifecycleStageField ?
                            (int)DropdownFieldTypes.LifeCycle : (int?)null;
                        var searchValueOptionsResponse = advancedSearchService.GetSearchValueOptions(new GetSearchValueOptionsRequest() { AccountId = request.AccountId, ContactDropdownId = contactDropdown, FieldId = workflowModel.FieldID });
                        workflowModel.ValueOptions = searchValueOptionsResponse.FieldValueOptions;

                        if (workflowModel.FieldID > 200) // Below 200 are all standard fields
                            workflowModel.Name = customFieldRepository.GetCustomFieldValueName(workflowModel.FieldID, workflowModel.FieldValue);
                        else if (workflowModel.FieldID != (int)ContactFields.DonotEmail)
                            workflowModel.Name = searchValueOptionsResponse.FieldValueOptions.Where(w => w.Id == Convert.ToInt32(workflowModel.FieldValue)).Select(s => s.Value).FirstOrDefault();
                        else if (workflowModel.FieldID == (int)ContactFields.DonotEmail)
                            workflowModel.Name = workflowModel.FieldValue == "0" ? "No" : "Yes";
                    }
                    else
                        workflowModel.Name = workflowModel.FieldValue;
                    a.Action = (BaseWorkflowActionViewModel)workflowModel;
                }
            });
            response.WorkflowViewModel = WorkflowViewModel;
            return response;
        }

        void hasAccess(short documentId, int? userId, int accountId, short roleId)
        {
            bool isAccountAdmin = cachingService.IsAccountAdmin(roleId, accountId);
            if (!isAccountAdmin)
            {
                // bool isPrivate = cachingService.IsModulePrivate(AppModules.Campaigns, accountId);
                // if (isPrivate && !workflowRepository.IsCreatedBy(documentId, userId, accountId))
                //throw new UnsupportedOperationException("[|Requested user is not authorized to get this workflow|]");
            }
        }

        public GetCampaignsResponse GetAllCampaigns(GetCampaignsRequest request)
        {
            Logger.Current.Verbose("Request received to get all campaigns in the workflow");
            GetCampaignsResponse response = new GetCampaignsResponse();
            IEnumerable<Campaign> campaigns = workflowRepository.GetAllCampaigns(request.AccountId, request.IsWorklflowCampaign);
            response.Campaigns = Mapper.Map<IEnumerable<Campaign>, IEnumerable<CampaignViewModel>>(campaigns);
            return response;
        }

        public GetFormsResponse GetAllForms(GetFormsRequest request)
        {
            Logger.Current.Verbose("Request received to get all forms in workflow module");
            GetFormsResponse response = new GetFormsResponse();
            IEnumerable<Form> forms = workflowRepository.GetAllForms(request.AccountId);
            response.Forms = Mapper.Map<IEnumerable<Form>, IEnumerable<FormViewModel>>(forms);
            return response;
        }

        public GetSavedSearchesResponse GetAllSmartSearches(GetSavedSearchesRequest request)
        {
            Logger.Current.Verbose("Request received to get all smart searches in the workflow module");
            GetSavedSearchesResponse response = new GetSavedSearchesResponse();
            IEnumerable<SearchDefinition> smartsearches = workflowRepository.GetAllSmartSearches(request.AccountId);
            response.SearchResults = Mapper.Map<IEnumerable<SearchDefinition>, IEnumerable<AdvancedSearchViewModel>>(smartsearches);
            return response;
        }

        public GetTagListResponse GetAllTags(GetTagListRequest request)
        {
            Logger.Current.Verbose("Request received to get all tags in the workflow module");
            GetTagListResponse response = new GetTagListResponse();
            IEnumerable<Tag> tags = workflowRepository.GetAllTags(request.AccountId);
            response.Tags = Mapper.Map<IEnumerable<Tag>, IEnumerable<TagViewModel>>(tags);
            return response;
        }

        public GetCampaignLinksResponse GetCampaignLinks(GetCampaignLinksRequest request)
        {
            Logger.Current.Verbose("Request received to get all campaign links for the selected campaign");
            GetCampaignLinksResponse response = new GetCampaignLinksResponse();
            IEnumerable<CampaignLink> campaignLinks = campaignRepository.GetCampaignLinks(request.CampaignID);
            response.CampaignsLinks = Mapper.Map<IEnumerable<CampaignLink>, IEnumerable<CampaignLinkViewModel>>(campaignLinks);
            return response;
        }

        public GetUserListResponse GetAllUsers(GetUserListRequest request)
        {
            Logger.Current.Verbose("Request received to get all users in the workflow module");
            GetUserListResponse response = new GetUserListResponse();
            IEnumerable<User> users = workflowRepository.GetAllUsers(request.AccountId);
            response.Users = Mapper.Map<IEnumerable<User>, IEnumerable<UserViewModel>>(users);
            return response;
        }

        private void addToTopic(int workflowId, int accountId)
        {
            if (workflowId == 0 || accountId == 0)
                return;
            var message = new TrackMessage()
            {
                EntityId = workflowId,
                AccountId = accountId,
                LeadScoreConditionType = (byte)LeadScoreConditionType.WorkflowActivated,
            };
            messageService.SendMessages(new Messaging.Messages.SendMessagesRequest()
            {
                Message = message
            });
        }

        private void addToTopic(int workflowId, int accountId, DateTime deactivateOn)
        {
            if (workflowId == 0 || accountId == 0)
                return;
            var message = new TrackMessage()
            {
                EntityId = workflowId,
                AccountId = accountId,
                LeadScoreConditionType = (int)LeadScoreConditionType.WorkflowInactive,
                ConditionValue = "delayed",
                CreatedOn = deactivateOn
            };
            messageService.SendMessages(new Messaging.Messages.SendMessagesRequest()
            {
                Message = message
            });
        }

        private void addSavedSearchContactsToTopic(Workflow workflow, short roleId)
        {

            if (workflow.Triggers.Where(t => t.IsStartTrigger).First().SearchDefinitionID.HasValue)
            {
                var task = Task.Run(() => advancedSearchService.GetSavedSearchContactIds(new GetSavedSearchContactIdsRequest()
                {
                    AccountId = workflow.AccountID,
                    RequestedBy = workflow.CreatedBy,
                    RoleId = roleId,
                    SearchDefinitionId = workflow.Triggers.Where(t => t.IsStartTrigger).First().SearchDefinitionID.Value
                }));
                var contacts = task.Result;
                var messages = new List<TrackMessage>();
                contacts.ForEach(c =>
                    {
                        var message = new TrackMessage();
                        message.EntityId = workflow.Triggers.Where(t => t.IsStartTrigger).First().SearchDefinitionID.Value;
                        message.ContactId = c;
                        message.AccountId = workflow.AccountID;
                        message.LeadScoreConditionType = (int)LeadScoreConditionType.ContactMatchesSavedSearch;
                        message.UserId = workflow.CreatedBy;
                        messages.Add(message);
                    });
                messageService.SendMessages(new Messaging.Messages.SendMessagesRequest()
                {
                    Messages = messages
                });
            }
        }

        public DeactivateWorkflowResponse DeactivateWorkflow(DeactivateWorkflowRequest request)
        {
            DeactivateWorkflowResponse response = new DeactivateWorkflowResponse();
            Logger.Current.Informational("Request received for deactivating a workflow with Id : " + request.WorkflowId);
            workflowRepository.DeactivateWorkflow((short)request.WorkflowId);
            return response;
        }

        public RemoveFromOtherWorkflowsResponse RemoveFromOtherWorkflows(RemoveFromOtherWorkflowsRequest request)
        {
            Logger.Current.Verbose("Request received to remove the contacts from other workflows");
            RemoveFromOtherWorkflowsResponse response = new RemoveFromOtherWorkflowsResponse();
            workflowRepository.RemoveFromOtherWorkflows(request.ContactId, request.WorkflowIds, request.AllowParallelWorkflows);
            return response;
        }

        public IsEnrolledToRemoveResponse IsEnrolledToRemove(IsEnrolledToRemoveRequest request)
        {
            IsEnrolledToRemoveResponse response = new IsEnrolledToRemoveResponse();
            IEnumerable<short> workflowIds = new List<short>();
            if (request.WorkflowId != 0)
                workflowIds = workflowRepository.IsEnrolledToRemove(request.WorkflowId);
            response.WorkflowIds = workflowIds;
            return response;
        }

        public InsertContactWorkflowAuditResponse InsertContactWorkflowAudit(InsertContactWorkflowAuditRequest request)
        {
            InsertContactWorkflowAuditResponse response = new InsertContactWorkflowAuditResponse();
            Logger.Current.Informational("Request received for auditing workflow state of a contact");
            if (request != null)
                workflowRepository.InsertContactWorkflowAudit((short)request.WorkflowId, request.ContactId, request.WorkflowActionId, request.MessageId);
            return response;
        }

        public AssignUserResponse AssignUser(AssignUserRequest request)
        {
            AssignUserResponse response = new AssignUserResponse();
            workflowRepository.AssignUser(request.ContactId, request.WorkflowID, request.userAssignmentActionID, request.ScheduledID);
            return response;
        }

        public NotifyUserResponse NotifyUser(NotifyUserRequest request)
        {
            Logger.Current.Informational("Request received for notifying a user");
            NotifyUserResponse response = new NotifyUserResponse();
            var status = accountRepository.GetAccountStatus(request.AccountId);
            if (status != AccountStatus.Suspend)
            {
                var userPrimaryEmails = userRepository.GetUsersPrimaryEmailsByUserIds(request.UserIds, request.AccountId).ToList();
                UserContactActivitySummary contactSummary = null;
                if (request.ContactId != 0)
                    contactSummary = workflowRepository.GetBasicContactDetails(request.ContactId, request.AccountId);
                if (contactSummary != null)
                {
                    int accountId = request.AccountId;
                    var details = request.Message;
                    if (accountId != 0 && details != null)
                    {
                        if (request.NotifyType == 1)
                            NotifyByEmail(request.WorkflowName, accountId, details, contactSummary, request.LinkEntityId, request.trigger, request.EntityId, request.NotificationFieldIds, request.WorkflowId
                                , request.ContactId, request.WorkflowActionId, userPrimaryEmails);
                        else if (request.NotifyType == 2)
                            NotifyByText(accountId, details, contactSummary, userPrimaryEmails);
                        else if (request.NotifyType == 3)
                        {
                            NotifyByEmail(request.WorkflowName, accountId, details, contactSummary, request.LinkEntityId, request.trigger, request.EntityId, request.NotificationFieldIds, request.WorkflowId
                                , request.ContactId, request.WorkflowActionId, userPrimaryEmails);
                            NotifyByText(accountId, details, contactSummary, userPrimaryEmails);
                        }
                    }
                }
            }
            return response;
        }

        private void NotifyByEmail(string workflowName, int accountId, string message, UserContactActivitySummary contactSummary, int? LinkEntityId,
            LeadScoreConditionType trigger, int? EntityId, IEnumerable<int> notificationFieldIds, int workflowId, int contactId, int workflowActionId, List<string> userPrimaryEmails)
        {
            Logger.Current.Informational("Request received to notify a user by email");
            string defaultEmail = System.Configuration.ConfigurationManager.AppSettings["Notification_Email"].ToString();
            if (userPrimaryEmails != null && userPrimaryEmails.Any())
            {
                string accountPrimaryEmail = string.Empty;
                Guid loginToken = new Guid();
                Email senderEmail = new Email();
                string Subject = string.Empty;

                string accountsids = System.Configuration.ConfigurationManager.AppSettings["Excluded_Accounts"].ToString();
                bool addBcc = accountsids.Contains(accountId.ToString());
                bool isFirstNotification = workflowRepository.IsFirstNotification(workflowId, contactId, workflowActionId);

                Account account = accountRepository.GetAccountMinDetails(accountId);
                if (account != null)
                {
                    if (account.Email != null)
                        accountPrimaryEmail = account.Email.EmailId;

                    IEnumerable<ServiceProvider> serviceProviders = serviceProviderRepository.GetAccountCommunicationProviders(accountId, CommunicationType.Mail, MailType.TransactionalEmail);
                    if (serviceProviders != null && serviceProviders.FirstOrDefault() != null)
                    {
                        loginToken = serviceProviders.FirstOrDefault().LoginToken;
                        senderEmail = serviceProviderRepository.GetServiceProviderEmail(serviceProviders.FirstOrDefault().Id);
                    }

                    if (loginToken != new Guid() && accountPrimaryEmail != null)
                    {
                        string entityName = string.Empty;
                        string attachmentName = string.Empty;
                        string body = EmailBody(message, accountId, contactSummary, LinkEntityId, account, trigger, workflowName, EntityId, notificationFieldIds, out entityName, out attachmentName);
                        string fromEmail = (senderEmail != null && !string.IsNullOrEmpty(senderEmail.EmailId)) ? senderEmail.EmailId : accountPrimaryEmail;
                        EmailAgent agent = new EmailAgent();
                        string subject = GetSubject(trigger, account.AccountName, workflowName, entityName);

                        var firstUserEmail = userPrimaryEmails.FirstOrDefault();
                        bool HasBccAdded = false;
                        foreach (var email in userPrimaryEmails)
                        {
                            bool isFirstUser = string.Equals(email, firstUserEmail, StringComparison.InvariantCulture);

                            if (!string.IsNullOrEmpty(email) && accountId != 0 && message != null && !defaultEmail.Equals(email, StringComparison.OrdinalIgnoreCase))
                            {
                                SendMailRequest mailRequest = new SendMailRequest();
                                mailRequest.Body = body;
                                mailRequest.From = fromEmail;
                                mailRequest.IsBodyHtml = true;
                                mailRequest.ScheduledTime = DateTime.UtcNow.AddMinutes(2);
                                mailRequest.Subject = subject;
                                mailRequest.To = new List<string>() { email };
                                if (!addBcc && isFirstNotification && (isFirstUser || !HasBccAdded))
                                {
                                    mailRequest.BCC = new List<string>() { defaultEmail };
                                    HasBccAdded = true;
                                }
                                mailRequest.TokenGuid = loginToken;
                                mailRequest.RequestGuid = Guid.NewGuid();
                                mailRequest.AccountDomain = account.DomainURL;
                                mailRequest.NotificationAttachementGuid = attachmentName;
                                mailRequest.CategoryID = GetNotificationsCategory(trigger);
                                mailRequest.AccountID = accountId;
                                if (!string.IsNullOrEmpty(attachmentName))
                                    mailRequest.GetProcessedByClassic = true;
                                agent.SendEmail(mailRequest);
                            }
                        }
                    }
                }
            }
        }

        private string GetSubject(LeadScoreConditionType trigger, string accountName, string workflowName, string entityName)
        {
            string subject = string.Empty;
            if (trigger == LeadScoreConditionType.ContactSubmitsForm)
                return subject = "SmartTouch Form Submission Notification - " + accountName + " : " + entityName + "";
            else if (trigger == LeadScoreConditionType.LeadAdapterSubmitted)
                return subject = "SmartTouch Lead Adapter Notification - " + accountName + " : " + entityName + "";
            else if (trigger == LeadScoreConditionType.ContactVisitsWebPage || trigger == LeadScoreConditionType.PageDuration)
                return subject = "SmartTouch Web Visit Notification - " + accountName + " : " + workflowName + "";
            else if (trigger == LeadScoreConditionType.ContactMatchesSavedSearch)
                return subject = "SmartTouch Saved Search Notification - " + accountName + " : " + entityName + "";
            else if (trigger == LeadScoreConditionType.ContactClicksLink)
                return subject = "SmartTouch Re-Engagement Workflow Notification - " + accountName + "";
            else
                return subject = "SmartTouch - Workflow Notification - " + accountName + " : " + workflowName + "";
        }

        private Int16 GetNotificationsCategory(LeadScoreConditionType trigger)
        {
            if (trigger == LeadScoreConditionType.ContactSubmitsForm)
                return (Int16)EmailNotificationsCategory.WorkflowFormSubmission;
            else if (trigger == LeadScoreConditionType.LeadAdapterSubmitted)
                return (Int16)EmailNotificationsCategory.WorkflowLeadAdapter;
            else if (trigger == LeadScoreConditionType.ContactVisitsWebPage || trigger == LeadScoreConditionType.PageDuration)
                return (Int16)EmailNotificationsCategory.WorkflowWebVisit;
            else if (trigger == LeadScoreConditionType.ContactMatchesSavedSearch)
                return (Int16)EmailNotificationsCategory.WorkflowSavedSearch;
            else if (trigger == LeadScoreConditionType.ContactClicksLink)
                return (Int16)EmailNotificationsCategory.WorkflowCampaignLinkClick;
            else if (trigger == LeadScoreConditionType.CampaignSent)
                return (Int16)EmailNotificationsCategory.WorkflowCampaignSent;
            else if (trigger == LeadScoreConditionType.ContactLifecycleChange)
                return (Int16)EmailNotificationsCategory.WorkflowLifecycleChange;
            else if (trigger == LeadScoreConditionType.LeadscoreReached)
                return (Int16)EmailNotificationsCategory.WorkflowLeadscoreReached;
            else if (trigger == LeadScoreConditionType.ContactActionTagAdded)
                return (Int16)EmailNotificationsCategory.WorkflowTagAdd;
            else if (trigger == LeadScoreConditionType.ContactTagRemoved)
                return (Int16)EmailNotificationsCategory.WorkflowTagRemove;
            else if (trigger == LeadScoreConditionType.AnEmailSent)
                return (Int16)EmailNotificationsCategory.WorkflowSendEmail;
            else
                return (Int16)EmailNotificationsCategory.Default;
        }

        private void NotifyByText(int accountId, string message, UserContactActivitySummary contactSummary, List<string> userPrimaryEmails)
        {
            Logger.Current.Informational("Request received for notifying a user by text");
            if (accountId != 0 && message != null)
            {
                Guid loginToken = new Guid();

                ServiceProvider serviceProviders = serviceProviderRepository.GetSendTextServiceProviders(accountId, CommunicationType.Text);
                if (serviceProviders != null && serviceProviders.LoginToken != new Guid())
                    loginToken = serviceProviders.LoginToken;

                string body = TextBody(message, accountId, contactSummary);
                foreach (var email in userPrimaryEmails)
                {
                    var userPrimaryPhoneNumber = userRepository.GetUsersPrmaryPhoneNumbersByUserIds(email, accountId);
                    SendTextRequest textRequest = new SendTextRequest();
                    if (loginToken != new Guid() && !string.IsNullOrEmpty(userPrimaryPhoneNumber) && serviceProviders.SenderPhoneNumber != null)
                    {
                        textRequest.From = serviceProviders.SenderPhoneNumber;
                        textRequest.Message = body;
                        textRequest.RequestGuid = Guid.NewGuid();
                        textRequest.TokenGuid = loginToken;
                        textRequest.ScheduledTime = DateTime.UtcNow.AddMinutes(2);
                        textRequest.To = new List<string>() { userPrimaryPhoneNumber };
                        TextService textService = new TextService();
                        textService.SendText(textRequest);
                    }
                }
            }
        }

        private string EmailBody(string message, int accountId, UserContactActivitySummary contactSummary, int? LinkEntityId,
            Account accountDetails, LeadScoreConditionType trigger, string workflowName, int? EntityId, IEnumerable<int> notificationFieldIds, out string entityName, out string attachment)
        {
            entityName = string.Empty;
            attachment = string.Empty;
            string customEntityName = string.Empty;
            string customAttachmentName = string.Empty;
            string body = string.Empty;
            if (accountId != 0 && (trigger == LeadScoreConditionType.ContactSubmitsForm || trigger == LeadScoreConditionType.LeadAdapterSubmitted))
            {
                body = FormEmailBody(message, accountId, contactSummary, LinkEntityId, accountDetails, workflowName, notificationFieldIds, trigger, EntityId, out customEntityName, out customAttachmentName);
                entityName = customEntityName;
                attachment = customAttachmentName;
                return body;
            }
            else
            {
                body = DefaultEmailBodyForRemainingTriggers(message, accountId, contactSummary, LinkEntityId, accountDetails, workflowName, notificationFieldIds, trigger, EntityId, out customEntityName);
                entityName = customEntityName;
                return body;
            }

        }

        private string DefaultEmailBodyForRemainingTriggers(string message, int accountId, UserContactActivitySummary contactSummary, int? LinkEntityId, Account accountDetails, string workflowName, IEnumerable<int> notificationFieldIds, LeadScoreConditionType trigger, int? EntityId, out string customEntityName)
        {
            string body = "";
            string accountLogo = string.Empty;
            string accountName = string.Empty;
            string accountImage = string.Empty;
            string details = string.Empty;
            string customName = string.Empty;
            string savedSearchName = string.Empty;
            string campaignLinkURL = string.Empty;
            string notificationDetails = string.Empty;
            FormData formData = new FormData();
            string accountAddress = accountService.GetPrimaryAddress(new GetAddressRequest() { AccountId = accountId }).Address;
            string accountPhoneNumber = accountService.GetPrimaryPhone(new GetPrimaryPhoneRequest() { AccountId = accountId }).PrimaryPhone;
            GetAccountImageStorageNameResponse response = accountService.GetStorageName(new GetAccountImageStorageNameRequest()
            {
                AccountId = accountId
            });

            if (response.AccountLogoInfo != null)
            {
                if (!String.IsNullOrEmpty(response.AccountLogoInfo.StorageName))
                    accountLogo = urlService.GetUrl(accountId, ImageCategory.AccountLogo, response.AccountLogoInfo.StorageName);
                else
                    accountLogo = "";
                accountName = response.AccountLogoInfo.AccountName;
            }
            if (!string.IsNullOrEmpty(accountLogo))
                accountImage = accountImage + "<td align='right' valign='center' style='margin:0px;padding:0px 0px 25px 0px;'><img src='" + accountLogo + "' alt='" + accountName + "' style='width:100px;' width='100'></td>";

            if (!string.IsNullOrEmpty(message))
                details = message;

            if (trigger == LeadScoreConditionType.ContactMatchesSavedSearch)
            {
                string name = workflowRepository.GetSavedSearchName(EntityId.Value);
                savedSearchName = "<tr><td align = 'left' valign = 'center' style = 'margin:0px;padding:0px 0px 5px 0px;font-size:14px; color:#7b7b7b;font-family:arial,sans-serif;' >" +
                                   "Saved Search Name: <span style = 'color:#2B2B2B;'>" + name + "</span></td> </tr> ";
                notificationDetails = "Saved Search Contact Details";
                customName = name;
            }
            else if (trigger == LeadScoreConditionType.ContactClicksLink)
            {
                string campaignName = campaignRepository.GetCampaignNameById(EntityId.Value);
                string URL = campaignRepository.GetCampaignLinkURLByLinkId(LinkEntityId.Value);
                savedSearchName = "<tr><td align = 'left' valign = 'center' style = 'margin:0px;padding:0px 0px 5px 0px;font-size:14px; color:#7b7b7b;font-family:arial,sans-serif;' >" +
                                  "Campaign Name: <span style = 'color:#2B2B2B;'>" + campaignName + "</span></td> </tr> ";
                if (!string.IsNullOrEmpty(URL))
                {
                    campaignLinkURL = "<tr><td align = 'left' valign = 'center' style = 'margin:0px;padding:0px 0px 5px 0px;font-size:14px; color:#7b7b7b;font-family:arial,sans-serif;' >" +
                                  "Link URL: <span style = 'color:#2B2B2B;'>" + URL + "</span></td> </tr> ";
                    notificationDetails = "Campaign Link Clicked Contact Details";
                }

            }
            else
            {
                notificationDetails = "Contact Details:";
            }

            if (contactSummary.PrimaryPhoneTypeValueId != 0)
                notificationFieldIds = notificationFieldIds.Concat(new List<int>() { contactSummary.PrimaryPhoneTypeValueId });

            customEntityName = customName;
            string fields = string.Join(",", notificationFieldIds);
            int notificationType = (int)trigger;
            NotificationData contactFieldsData = workflowRepository.GetAllNotificationContactFieldsData(contactSummary.contactId, fields, notificationType, LinkEntityId, accountId);
            if (contactFieldsData != null && accountId != 0)
            {
                Logger.Current.Informational("constructing email body to notify user about contact from form-submission");
                string link = string.Empty;
                if (accountDetails != null && contactSummary != null && contactSummary.contactId.HasValue)
                {
                    if (contactSummary.ContactType == ContactType.Person)
                        link = "<span style='color:#2B2B2B;'> <a href=" + "'" + "https://" + accountDetails.DomainURL + "/person/" + contactSummary.contactId.Value + "'" + "style=" + "color:#0e749f;" +
                        ">" + contactSummary.ContactName + "</a> </span>";
                    else if (contactSummary.ContactType == ContactType.Company)
                        link = "<span style='color:#2B2B2B;'> <a href=" + "'" + "https://" + accountDetails.DomainURL + "/company/" + contactSummary.contactId.Value + "'" + "style=" + "color: #0e749f;" +
                        ">" + contactSummary.ContactName + "</a> </span>";
                }
                if (contactFieldsData.FieldsData != null && contactFieldsData.FieldsData.Any(f => f.FieldID == 49 || f.FieldID == 28))
                {
                    List<NotificationContactFieldData> fieldData = new List<NotificationContactFieldData>();
                    foreach (var dateField in contactFieldsData.FieldsData)
                    {
                        if ((dateField.FieldID == 49 || dateField.FieldID == 28) && !string.IsNullOrEmpty(dateField.Value))
                            dateField.Value = ConvertDate(dateField.Value, accountId);
                        fieldData.Add(dateField);
                    }
                    contactFieldsData.FieldsData = fieldData;
                }
                string filename = EmailTemplate.WorkFlowDefaultEmailTemplate.ToString() + ".txt";
                string savedFileName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["EMAILTEMPLATES_PHYSICAL_PATH"].ToString(), filename);
                var ContactData = LandmarkIT.Enterprise.Extensions.EnumerableExtentions.GetTable<NotificationContactFieldData>(contactFieldsData.FieldsData, p => p.FieldName, p => p.Value);

                //ILookup<string, string> dic = contactFieldsData.FieldsData.ToLookup(d => d.FieldName, d => d.Value);
                //string attachementName = GeneratePDF(null, dic, accountName, string.Empty, accountLogo, (EmailNotificationsCategory)GetNotificationsCategory(trigger));

                using (StreamReader reader = new StreamReader(savedFileName))
                {
                    do
                    {
                        body = reader.ReadToEnd().Replace("[WorkFlowName]", workflowName).Replace("[AccountName]", accountName).Replace("[AccountImage]", accountImage).Replace("[ContactData]", ContactData).Replace("[ContactName]", link)
                                                 .Replace("[Message]", details).Replace("[ADDRESS]", accountAddress).Replace("[PHONE]", accountPhoneNumber).Replace("[SavedSearchName]", savedSearchName).Replace("[CustomDetails]", notificationDetails).Replace("[LinkURL]", campaignLinkURL);
                    } while (!reader.EndOfStream);
                }
            }
            return body;
        }

        private string FormEmailBody(string message, int accountId, UserContactActivitySummary contactSummary, int? LinkEntityId, Account accountDetails, string workflowName, IEnumerable<int> notificationFieldIds, LeadScoreConditionType trigger, int? entityId, out string customEntityName, out string attachmentName)
        {
            customEntityName = string.Empty;
            attachmentName = string.Empty;
            string body = string.Empty;
            Logger.Current.Informational("AccountId And Trigger: " + accountId + " " + trigger);
            string accountLogo = string.Empty;
            string accountName = string.Empty;
            string accountImage = string.Empty;
            string details = string.Empty;
            string CustomDetails = string.Empty;
            string CustomName = string.Empty;
            int customEntityID = 0;
            string accountAddress = accountService.GetPrimaryAddress(new GetAddressRequest() { AccountId = accountId }).Address;
            string accountPhoneNumber = accountService.GetPrimaryPhone(new GetPrimaryPhoneRequest() { AccountId = accountId }).PrimaryPhone;
            GetAccountImageStorageNameResponse response = accountService.GetStorageName(new GetAccountImageStorageNameRequest()
            {
                AccountId = accountId
            });

            if (response.AccountLogoInfo != null)
            {
                if (!String.IsNullOrEmpty(response.AccountLogoInfo.StorageName))
                    accountLogo = urlService.GetUrl(accountId, ImageCategory.AccountLogo, response.AccountLogoInfo.StorageName);
                else
                    accountLogo = "";
                accountName = response.AccountLogoInfo.AccountName;
            }
            if (!string.IsNullOrEmpty(accountLogo))
                accountImage = accountImage + "<td align='right' valign='center' style='margin:0px;padding:0px 0px 25px 0px;'><img src='" + accountLogo + "' alt='" + accountName + "' style='width:100px;' width='100'></td>";

            if (!string.IsNullOrEmpty(message))
                details = message;
            if (trigger == LeadScoreConditionType.ContactSubmitsForm)
            {
                if (LinkEntityId.HasValue && LinkEntityId != 0)
                {
                    Logger.Current.Informational("Request received to notify user about contact from form-submission");
                    customEntityName = formService.GetFormName(LinkEntityId.Value);
                    CustomDetails = "Form Submission Details:";
                    CustomName = "Form Name";
                    customEntityID = LinkEntityId.Value;
                }
            }
            else if (trigger == LeadScoreConditionType.LeadAdapterSubmitted)
            {
                if (contactSummary != null && contactSummary.contactId.HasValue)
                {
                    Logger.Current.Informational("Request received to notify user about contact from form-submission");
                    customEntityName = leadAdapterService.GetFaceBookHostNameByContactId(contactSummary.contactId.Value);
                    if (string.IsNullOrEmpty(customEntityName))
                        customEntityName = leadAdapterService.GetLeadAdapterName(contactSummary.contactId);

                    CustomDetails = "LeadAdapter Submission Details:";
                    CustomName = "LeadAdapter Name";
                    customEntityID = LinkEntityId.Value;

                }
            }


            if (contactSummary.PrimaryPhoneTypeValueId != 0)
                notificationFieldIds = notificationFieldIds.Concat(new List<int>() { contactSummary.PrimaryPhoneTypeValueId });

            string fields = string.Join(",", notificationFieldIds);
            int notificationType = (int)trigger;
            NotificationData contactFieldsData = workflowRepository.GetAllNotificationContactFieldsData(contactSummary.contactId, fields, notificationType, customEntityID, accountId);

            if (contactFieldsData != null && accountId != 0)
            {
                Logger.Current.Informational("constructing email body to notify user about contact from form-submission");
                string link = string.Empty;
                if (accountDetails != null && contactSummary != null && contactSummary.contactId.HasValue)
                {
                    if (contactSummary.ContactType == ContactType.Person)
                        link = "<span style='color:#2B2B2B;'> <a href=" + "'" + "https://" + accountDetails.DomainURL + "/person/" + contactSummary.contactId.Value + "'" + "style=" + "color:#0e749f;" +
                            ">" + contactSummary.ContactName + "</a> </span>";
                    else if (contactSummary.ContactType == ContactType.Company)
                        link = "<span style='color:#2B2B2B;'> <a href=" + "'" + "https://" + accountDetails.DomainURL + "/company/" + contactSummary.contactId.Value + "'" + "style=" + "color: #0e749f;" +
                            ">" + contactSummary.ContactName + "</a> </span>";
                }

                string filename = EmailTemplate.WorkflowNotificationFields.ToString() + ".txt";
                string savedFileName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["EMAILTEMPLATES_PHYSICAL_PATH"].ToString(), filename);

                if (contactFieldsData.FieldsData != null && contactFieldsData.FieldsData.Any(f => f.FieldID == 49 || f.FieldID == 28))
                {
                    List<NotificationContactFieldData> fieldData = new List<NotificationContactFieldData>();
                    foreach (var dateField in contactFieldsData.FieldsData)
                    {
                        if ((dateField.FieldID == 49 || dateField.FieldID == 28) && !string.IsNullOrEmpty(dateField.Value))
                            dateField.Value = ConvertDate(dateField.Value, accountId);
                        fieldData.Add(dateField);
                    }
                    contactFieldsData.FieldsData = fieldData;
                }
                var ContactData = LandmarkIT.Enterprise.Extensions.EnumerableExtentions.GetTable<NotificationContactFieldData>(contactFieldsData.FieldsData, p => p.FieldName, p => p.Value);
                var formSubmittedData = string.Empty;
                if (!string.IsNullOrEmpty(contactFieldsData.SubmittedData))
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    var deserializedForm = js.Deserialize<Dictionary<string, Dictionary<string, string>>>(contactFieldsData.SubmittedData);
                    var model = (Dictionary<string, Dictionary<string, string>>)deserializedForm;
                    List<string> propertyNames = model.Keys.ToList();

                    foreach (var prop in propertyNames)
                    {
                        Dictionary<string, string> formValues = new Dictionary<string, string>();
                        model.TryGetValue(prop, out formValues);
                        if (formValues != null && formValues.Count > 0)
                        {
                            Logger.Current.Informational("Iterating through the form properties.");
                            formSubmittedData = formSubmittedData + "<tr>\n<td align='left' style='margin:0px;padding:10px;font-size:12px; font-weight:normal;color:#555555;font-family:arial,sans-serif;border-bottom:solid 1px #d9d9d9;" +
                            "' valign='center'>" + prop + "</td>\n<td align='left' style='margin:0px;padding:10px;font-size:12px; font-weight:normal;color:#555555;font-family:arial,sans-serif;border-bottom:solid 1px #d9d9d9;' valign='center'>" + formValues.ElementAt(0).Value + "</td>\n" +
                            "<td align='left' style='margin:0px;padding:10px;font-size:12px; font-weight:normal;color:#555555;font-family:arial,sans-serif;border-bottom:solid 1px #d9d9d9;' valign='center'>" + formValues.ElementAt(1).Value + "</td>\n</tr>\n";
                        }
                    }

                    bool attachmentNeeded = workflowRepository.CheckIfAttachementNeeded(entityId.Value);
                    Logger.Current.Informational("Is attachment needed" + attachmentNeeded.ToString());
                    if (entityId != null && entityId.HasValue && attachmentNeeded)
                    {
                        Logger.Current.Informational("Request received for adding an attachement for the notification email");
                        var jsonData = this.GenerateJSON(model, accountId, entityId.Value, LinkEntityId.Value);
                        attachmentName = PostClassicForAttachement(jsonData);
                        //attachmentName = GeneratePDF(model, null, accountName, customEntityName, accountLogo, EmailNotificationsCategory.WorkflowFormSubmission);
                    }
                    else
                        Logger.Current.Informational("The given form-submission track message didn't have formid (or) the form is not configured for pdf attachments");
                }

                using (StreamReader reader = new StreamReader(savedFileName))
                {
                    do
                    {
                        body = reader.ReadToEnd().Replace("[WorkFlowName]", workflowName).Replace("[AccountName]", accountName).Replace("[AccountImage]", accountImage).Replace("[ContactData]", ContactData).Replace("[CustomEntityName]", customEntityName).Replace("[ContactName]", link)
                                                 .Replace("[Message]", details).Replace("[ADDRESS]", accountAddress).Replace("[PHONE]", accountPhoneNumber).Replace("[FormData]", formSubmittedData)
                                                 .Replace("[CustomDetails]", CustomDetails).Replace("[CustomName]", CustomName);
                    } while (!reader.EndOfStream);
                }
            }

            return body;
        }

        private string GeneratePDF(Dictionary<string, Dictionary<string, string>> model, ILookup<string, string> data, string accountName, string entityName, string accountLogoURL, EmailNotificationsCategory category)
        {
            string pdfPath = System.Configuration.ConfigurationManager.AppSettings["PDF_ATTACHEMENT_PHYSICAL_PATH"].ToString();
            PDFGenerator pdfGen = new PDFGenerator();
            byte[] bytes = (category == EmailNotificationsCategory.WorkflowFormSubmission || category == EmailNotificationsCategory.WorkflowLeadAdapter) ?
                pdfGen.GenerateFormSubmissionPDF(model, accountName, entityName, accountLogoURL, 0) : pdfGen.GetNotificationPDF(data, accountName, entityName, accountLogoURL, 0);
            string fileName = Guid.NewGuid().ToString() + ".pdf";

            FileStream fs = new FileStream(Path.Combine(pdfPath, fileName), FileMode.OpenOrCreate);
            fs.Write(bytes, 0, bytes.Length);
            fs.Close();
            return fileName;
        }

        private string ConvertDate(string date, int accountId)
        {
            if (!string.IsNullOrEmpty(date))
            {
                Logger.Current.Informational("Request received to convert date to account time-zone");
                DateTime dest = DateTime.Parse(date);
                string timeZone = accountRepository.GetUserTimeZone(accountId);
                if (!string.IsNullOrEmpty(timeZone))
                {
                    Logger.Current.Informational("Account time-zone " + timeZone);
                    TimeZoneInfo tzinfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                    DateTime dateTime = TimeZoneInfo.ConvertTime(dest, TimeZoneInfo.Utc, tzinfo);
                    Logger.Current.Informational("Input date : " + dest.ToString());
                    Logger.Current.Informational("Converted date : " + dateTime.ToString());
                    return dateTime.ToString();
                }
                else
                    return date;
            }
            else
                return date;
        }

        private string GenerateJSON(Dictionary<string, Dictionary<string, string>> model, int accountId, int formId, int formSubmissionId)
        {
            Logger.Current.Informational("Request received for generating JSON");
            string json = string.Empty;
            if (model != null && model.Any())
            {
                Dictionary<string, string> newModel = new Dictionary<string, string>();
                foreach (var entry in model)
                {
                    string value = entry.Value.ElementAt(1).Value;
                    if (value.Contains('"'))
                        value = value.Replace('"', '\'');
                    newModel.Add(entry.Key, value);
                }
                newModel.Add("AccountID", accountId.ToString());
                newModel.Add("FormId", formId.ToString());
                newModel.Add("FormSubmissionId", formSubmissionId.ToString());
                json = JsonConvert.SerializeObject(newModel);
            }
            return json;
        }

        public static string cleanForJSON(string s)
        {
            Logger.Current.Informational("Cleaning JSON");
            if (s == null || s.Length == 0)
            {
                return "";
            }

            char c = '\0';
            int i;
            int len = s.Length;
            StringBuilder sb = new StringBuilder(len + 4);
            String t;

            for (i = 0; i < len; i += 1)
            {
                c = s[i];
                switch (c)
                {
                    case '\\':
                    case '"':
                        sb.Append('\\');
                        sb.Append(c);
                        break;
                    case '/':
                        sb.Append('\\');
                        sb.Append(c);
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    default:
                        if (c < ' ')
                        {
                            t = "000" + String.Format("X", c);
                            sb.Append("\\u" + t.Substring(t.Length - 4));
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            Logger.Current.Informational("JSON data to post to classic" + sb.ToString());
            return sb.ToString();
        }

        private string PostClassicForAttachement(string jsonData)
        {
            Logger.Current.Informational("Request received for posting jsondata to classic");
            string fileName = string.Empty;
            string classicURL = "http://207.200.34.228/";
            classicURL = System.Configuration.ConfigurationManager.AppSettings["CLASSIC_NOTIFICATION_URL"].ToString();
            RestClient client = new RestClient(classicURL);
            var request = new RestRequest("services/buffington/", Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Content-type", "application/json");
            request.AddParameter("application/json", jsonData, ParameterType.RequestBody);

            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Logger.Current.Informational("Content returned from classic:" + response.Content);
                    Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Content);
                    fileName = data.Values.FirstOrDefault();
                    if (!string.IsNullOrEmpty(fileName) && fileName.Contains('.'))
                        fileName = fileName.Split('.')[0];
                }
                else
                {
                    Logger.Current.Informational("Json data : " + jsonData);
                    Logger.Current.Error("Not able to post data to classic", response.ErrorException);
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Informational("Json data : " + jsonData);
                Logger.Current.Error("An error occured while making request to classic", ex);
            }
            return fileName;
        }

        private static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        private string TextBody(string message, int accountId, UserContactActivitySummary contactSummary)
        {
            string body = string.Empty;
            string link = string.Empty;
            string details = string.Empty;
            var domainUrl = accountRepository.GetAccountDomainUrl(accountId);
            if (domainUrl != null)
            {
                if (contactSummary != null && contactSummary.contactId.HasValue)
                {
                    if (contactSummary.ContactType == ContactType.Person)
                    {
                        link = " - https://" + domainUrl + "/person/" + contactSummary.contactId.Value;
                        details = "Person detail : " + contactSummary.ContactName;
                    }
                    else if (contactSummary.ContactType == ContactType.Company)
                    {
                        link = " - https://" + domainUrl + "/company/" + contactSummary.contactId.Value;
                        details = "Company detail : " + contactSummary.ContactName;
                    }
                }
            }
            return body = details + link;
        }

        public HasContactEnteredWorkflowResponse HasContactEnteredWorkflow(HasContactEnteredWorkflowRequest request)
        {
            HasContactEnteredWorkflowResponse response = new HasContactEnteredWorkflowResponse();
            bool hasEntered = workflowRepository.CheckIfContactEnteredWorkflow(request.ContactId, request.WorkflowId, request.MessageId);
            response.HasEntered = hasEntered;
            return response;
        }

        public CanContactReenterWorkflowResponse CanContactReenterWorkflow(CanContactReenterWorkflowRequest request)
        {
            Logger.Current.Verbose("Request recevied for checking whether a contact can reenter a workflow");
            CanContactReenterWorkflowResponse response = new CanContactReenterWorkflowResponse();
            bool canEnter = workflowRepository.CanContactReenterWorkflow(request.WorkflowId);
            response.CanReenter = canEnter;
            return response;
        }

        public HasContactCompletedWorkflowResponse HasContactCompletedWorkflow(HasContactCompletedWorkflowRequest request)
        {
            HasContactCompletedWorkflowResponse response = new HasContactCompletedWorkflowResponse();
            Logger.Current.Verbose("Request received to check whether contact completed the workflow");
            if (request.WorkflowId != 0 && request.ContactId != 0 && request.WorkflowActionId != 0)
            {
                bool HasCompleted = workflowRepository.HasCompletedWorkflow(request.WorkflowId, request.ContactId, request.WorkflowActionId);
                response.HasCompleted = HasCompleted;
            }
            return response;
        }

        public GetContactLastStateResponse GetLastState(GetContactLastStateRequest request)
        {
            GetContactLastStateResponse response = new GetContactLastStateResponse();
            Logger.Current.Verbose("Request received for getting the last state of the workflow");
            response.WorkflowActionId = workflowRepository.GetLastState(request.ContactId, request.WorkflowId);
            return response;
        }

        public GetWorkflowEndStateResponse GetEndState(GetWorkflowEndStateRequest request)
        {
            GetWorkflowEndStateResponse response = new GetWorkflowEndStateResponse();
            Logger.Current.Verbose("Request received for getting the end state of the workflow");
            WorkflowAction workflowAction = workflowRepository.GetEndState((short)request.WorkflowId);
            if (workflowAction != null)
                response.WorkflowActionViewModel = Mapper.Map<WorkflowAction, WorkflowActionViewModel>(workflowAction);
            return response;
        }

        public GetWorkflowsResponse GetRemainingWorkFlows(GetWorkflowsRequest request)
        {
            GetWorkflowsResponse response = new GetWorkflowsResponse();
            Logger.Current.Verbose("Request received for getting all the workflows except the workflow with id: " + request.WorkflowID);
            IEnumerable<Workflow> workflows = workflowRepository.GetRemainingWorkflows(request.WorkflowID, request.AccountId);
            if (workflows == null)
                response.Exception = GetWorkflowNotFoundException();
            else
                response.Workflows = Mapper.Map<IEnumerable<Workflow>, IEnumerable<WorkFlowViewModel>>(workflows);
            return response;
        }

        public GetWorkflowRelatedCampaignsResponse GetRelatedCampaigns(GetWorkflowRelatedCampaignsRequest request)
        {
            Logger.Current.Verbose("Request received for getting the related campaigns");
            GetWorkflowRelatedCampaignsResponse response = new GetWorkflowRelatedCampaignsResponse();
            response.Campaigns = Mapper.Map<IEnumerable<Campaign>, IEnumerable<CampaignViewModel>>(workflowRepository.GetRelatedCampaigns(request.WorkflowID));
            return response;
        }

        public CampaignStatisticsByWorkflowResponse GetCampaignStatisticsByWorkflow(CampaignStatisticsByWorkflowRequest request)
        {
            var response = new CampaignStatisticsByWorkflowResponse();
            response.CampaignStatistics = workflowRepository.GetCampaignStatistics(request.CampaignID, request.WorkflowID, request.FromDate, request.ToDate);
            return response;
        }

        public WorkflowStatusResponse UpdateWorkflowStatus(WorkflowStatusRequest request)
        {
            if (request.Status == WorkflowStatus.InActive || request.Status == WorkflowStatus.Paused)
            {
                IEnumerable<string> workflowNames = workflowRepository.checkToUpdateStatus(request.WorkflowID);
                if (workflowNames != null && workflowNames.Any())
                {
                    Logger.Current.Verbose("Can't update status to Inactive or Paused as this workflow is being used by another workflow");
                    var names = string.Join(", ", workflowNames);
                    var message = "[|This workflow is being used in |] " + names + " [|workflow(s)|]";
                    throw new UnsupportedOperationException(message);
                }
            }
            workflowRepository.UpdateWorkflowStatus(request.Status, request.WorkflowID, request.RequestedBy.Value);
            addToTopic(request.WorkflowID, request.AccountId, request.Status);
            return new WorkflowStatusResponse();
        }

        void addToTopic(int workflowId, int accountId, WorkflowStatus status)
        {
            int conditionType = default(int);
            if (status == WorkflowStatus.InActive)
                conditionType = (int)LeadScoreConditionType.WorkflowInactive;
            else if (status == WorkflowStatus.Paused)
                conditionType = (int)LeadScoreConditionType.WorkflowPaused;
            else if (status == WorkflowStatus.Active)
                conditionType = (int)LeadScoreConditionType.WorkflowActivated;

            if (workflowId == 0 || accountId == 0 || conditionType == 0)
                return;

            var message = new TrackMessage()
            {
                EntityId = workflowId,
                AccountId = accountId,
                LeadScoreConditionType = (byte)conditionType,
                LinkedEntityId = (short)status
            };
            messageService.SendMessages(new Messaging.Messages.SendMessagesRequest()
            {
                Message = message
            });
        }

        public GetWorkflowResponse CopyWorkFlow(GetWorkflowRequest request)
        {
            GetWorkflowResponse response = new GetWorkflowResponse();
            Logger.Current.Verbose("Request received to fetch the workflow with workflowID: " + request.WorkflowID);

            hasAccess(request.WorkflowID, request.RequestedBy, request.AccountId, request.RoleId);
            Workflow workflow = workflowRepository.GetWorkflowByID(request.WorkflowID, request.AccountId);
            workflow.WorkflowID = 0;
            workflow.Id = 0;
            workflow.StatusID = WorkflowStatus.Draft;

            if (request.IsNewWorkflow)
                workflow.WorkflowName = workflow.WorkflowName;
            else
                workflow.WorkflowName = string.Empty;

            workflow.Triggers.Each(t =>
                {
                    t.WorkflowID = 0;
                    t.WorkflowTriggerID = 0;
                });

            workflow.WorkflowActions.Each(wa =>
                {
                    wa.WorkflowActionID = 0;
                    wa.WorkflowID = 0;
                    wa.Action = SaveWorkflowActionsAs(wa.Action, wa.WorkflowActionTypeID);
                    wa.Action.WorkflowActionID = 0;
                    if (wa.WorkflowActionTypeID == WorkflowActionType.SendCampaign || wa.WorkflowActionTypeID == WorkflowActionType.LinkActions)  //Link Actions are workflow triggers
                    {
                        var campaignAction = wa.Action as WorkflowCampaignAction;
                        campaignAction.WorkflowCampaignActionID = 0;
                        if (campaignAction.Links != null && campaignAction.Links.Any())
                        {
                            campaignAction.Links.Each(e =>
                            {
                                e.ParentWorkflowActionID = 0;
                                e.LinkActionID = 0;
                                if (e.Actions != null && e.Actions.Any())
                                {
                                    e.Actions.Each(ea =>
                                    {
                                        ea.WorkflowActionID = 0;
                                        if (ea.Action != null)
                                        {
                                            ea.Action.WorkflowActionID = 0;
                                            ea.Action = SaveWorkflowActionsAs(ea.Action, ea.WorkflowActionTypeID);
                                        }
                                    });
                                }
                            });
                        }
                        wa.Action = campaignAction;
                    }
                    else if (wa.WorkflowActionTypeID == WorkflowActionType.AssignToUser)
                    {
                        var assignmentAction = wa.Action as WorkflowUserAssignmentAction;
                        assignmentAction.WorkflowUserAssignmentActionID = 0;
                        if (assignmentAction.RoundRobinContactAssignments != null && assignmentAction.RoundRobinContactAssignments.Any())
                        {
                            assignmentAction.RoundRobinContactAssignments.Each(e =>
                                {
                                    e.RoundRobinContactAssignmentID = 0;
                                    e.WorkFlowUserAssignmentActionID = 0;
                                });
                        }
                        wa.Action = assignmentAction;
                    }
                });

            response.WorkflowViewModel = Mapper.Map<Workflow, WorkFlowViewModel>(workflow);
            return response;
        }

        private BaseWorkflowAction SaveWorkflowActionsAs(BaseWorkflowAction action, WorkflowActionType actiontype)
        {
            var dictionary = new Dictionary<WorkflowActionType, BaseWorkflowAction>();
            dictionary.Add(WorkflowActionType.SendCampaign, action as WorkflowCampaignAction);
            dictionary.Add(WorkflowActionType.SetTimer, action as WorkflowTimerAction);
            dictionary.Add(WorkflowActionType.AddTag, action as WorkflowTagAction);
            dictionary.Add(WorkflowActionType.RemoveTag, action as WorkflowTagAction);
            dictionary.Add(WorkflowActionType.AdjustLeadScore, action as WorkflowLeadScoreAction);
            dictionary.Add(WorkflowActionType.ChangeLifecycle, action as WorkflowLifeCycleAction);
            dictionary.Add(WorkflowActionType.UpdateField, action as WorkflowContactFieldAction);
            dictionary.Add(WorkflowActionType.AssignToUser, action as WorkflowUserAssignmentAction);
            dictionary.Add(WorkflowActionType.NotifyUser, action as WorkflowNotifyUserAction);
            dictionary.Add(WorkflowActionType.SendEmail, action as WorkflowEmailNotificationAction);
            dictionary.Add(WorkflowActionType.TriggerWorkflow, action as TriggerWorkflowAction);
            dictionary.Add(WorkflowActionType.LinkActions, action as WorkflowCampaignAction);
            var value = dictionary[actiontype];
            value.GetType().GetProperties().Each(p =>
            {
                var attributes = p.GetCustomAttributes(typeof(KeyAttribute), false);
                if (attributes != null && attributes.Length > 0)
                {
                    p.SetValue(value, 0);
                }
            });
            return value;
        }

        public GetContactWorkflowsResponse GetContactWorkflows(GetContactWorkflowsRequest request)
        {
            GetContactWorkflowsResponse response = new GetContactWorkflowsResponse();
            Logger.Current.Informational("Request received for fetching workflows of this contact");
            IEnumerable<Workflow> workflows = workflowRepository.GetContactWorkflows(request.ContactId);
            response.WorkflowViewModel = Mapper.Map<IEnumerable<Workflow>, IEnumerable<WorkFlowViewModel>>(workflows);
            return response;
        }

        public GetLeadAdapterListResponse GetAllLeadAdapters(GetLeadAdapterListRequest request)
        {
            GetLeadAdapterListResponse response = new GetLeadAdapterListResponse();
            Logger.Current.Informational("Request received for fetching leadadapters associated with this account");
            IEnumerable<LeadAdapterAndAccountMap> leadadapters = workflowRepository.GetLeadAdapters(request.AccountId);
            response.LeadAdapters = Mapper.Map<IEnumerable<LeadAdapterAndAccountMap>, IEnumerable<LeadAdapterViewModel>>(leadadapters);
            return response;
        }

        public GetCampaignsResponse GetCampaigns(GetCampaignsRequest request)
        {
            Logger.Current.Verbose("Request received to get all campaigns in the workflow");
            GetCampaignsResponse response = new GetCampaignsResponse();
            IEnumerable<Campaign> campaigns = workflowRepository.GetCampaigns(request.AccountId, request.IsWorklflowCampaign, request.Query);
            response.Campaigns = Mapper.Map<IEnumerable<Campaign>, IEnumerable<CampaignViewModel>>(campaigns);
            return response;
        }


        string GetPropertyName<T, TR>(Expression<Func<T, TR>> property)
        {
            var propertyInfo = (property.Body as MemberExpression).Member as PropertyInfo;
            if (propertyInfo == null)
            {
                throw new ArgumentException("The lambda expression 'property' should point to a valid Property");
            }
            return propertyInfo.Name;
        }

        public UpdateContactFieldResponse UpdateContactField(UpdateContactFieldRequest request)
        {
            UpdateContactFieldResponse response = new UpdateContactFieldResponse();
            if (request != null)
            {
                workflowRepository.UpdateContactField(request.FieldID, request.FieldValue, request.ContactID, request.FieldInputTypeID, request.AccountId);
                accountService.InsertIndexingData(new InsertIndexingDataRequest() { IndexingData = new Domain.Accounts.IndexingData() { EntityIDs = new List<int>() { request.ContactID }, IndexType = (int)IndexType.Contacts } });
            }
            return response;
        }

        public GetNextBatchToProcessResponse GetNextBatchToProcess()
        {
            return new GetNextBatchToProcessResponse
            {
                TrackActions = workflowRepository.GetNextBatchToProcess()
            };
        }

        public void UpdateActionBatchStatus(UpdateActionBatchStatusRequest request)
        {
            workflowRepository.UpdateActionBatchStatus(request.TrackActions, request.TrackActionLogs);
        }

        public void UpdateWorkflowName(string name, int workflowId, int accountId)
        {
            bool isWorkflowNameUnique = workflowRepository.WorkflowNameDuplicateCheck(name, workflowId, accountId);
            if (!isWorkflowNameUnique)
            {
                Logger.Current.Verbose("Duplicate workflow identified," + name);
                var message = "[|Workflow with name|] \"" + name + "\" [|already exists.|] " + "[|Please choose a different name|]";
                throw new UnsupportedOperationException(message);
            }
            else
            {
                workflowRepository.UpdateWorkflowName(name, workflowId);
            }

        }

        public GetNotifyUserActionResponse GetNotifyUserAction(GetNotifyUserActionRequest request)
        {
            GetNotifyUserActionResponse response = new GetNotifyUserActionResponse();
            WorkflowNotifyUserAction action = workflowRepository.GetNotifyUserActionById(request.WorkflowActionId);
            WorkflowNotifyUserActionViewModel actionModel = Mapper.Map<WorkflowNotifyUserAction, WorkflowNotifyUserActionViewModel>(action);
            response.NotifyUserViewModel = actionModel;
            return response;
        }

        public UpdateNotifyUserActionResponse UpdateNotifyUserAction(UpdateNotifyUserActionRequest request)
        {
            UpdateNotifyUserActionResponse response = new UpdateNotifyUserActionResponse();
            WorkflowNotifyUserAction action = Mapper.Map<WorkflowNotifyUserActionViewModel, WorkflowNotifyUserAction>(request.NotifyUserActionViewModel);
            workflowRepository.UpdateNotifyUserAction(action);
            return response;
        }

        public GetUserAssigmentActionResponse GetUserAssignmentAction(GetUserAssignmentActionRequest request)
        {
            GetUserAssigmentActionResponse response = new GetUserAssigmentActionResponse();
            WorkflowAction action = workflowRepository.GetWorkflowAssignmentActionById(request.WorkflowActionId);
            WorkflowActionViewModel viewModel = Mapper.Map<WorkflowAction, WorkflowActionViewModel>(action);
            response.ActionViewModel = viewModel;
            return response;
        }

        public UpdateUserAssignmentActionReponse UpdateUserAssignmentAction(UpdateUserAssignmentActionRequest request)
        {
            UpdateUserAssignmentActionReponse response = new UpdateUserAssignmentActionReponse();
            WorkflowUserAssignmentAction action = Mapper.Map<WorkflowUserAssignmentActionViewModel, WorkflowUserAssignmentAction>(request.ActionViewModel);
            workflowRepository.UpdateUserAssignmentAction(action);
            if (action is WorkflowUserAssignmentAction)
            {
                var assignmentAction = action as WorkflowUserAssignmentAction;
                if (assignmentAction.RoundRobinContactAssignments != null && assignmentAction.RoundRobinContactAssignments.Any())
                    assignmentAction.RoundRobinContactAssignments.ToList().ForEach(f =>
                    {
                        var rrAction = Mapper.Map<RoundRobinContactAssignment, RoundRobinContactAssignmentDb>(f);
                        rrAction.Save(assignmentAction.WorkflowUserAssignmentActionID);
                    });
            }
            return response;
        }

        public int GetWorkflowIdByParentWfId(int parentId)
        {
            int workflowId = workflowRepository.GetWorkflowIdByParentId(parentId);
            return workflowId;
        }

        public bool HasContactMatchedEndTrigger(int contactId, int workflowId, long trackMessageID)
        {
            bool hasMatched = false;
            WorkflowGoalStatus goalStatus = workflowRepository.HasContactMatchedEndTrigger(contactId, workflowId, trackMessageID);
            if (goalStatus != null && !goalStatus.HasMatched && goalStatus.LeadScoreConditionType == (byte)LeadScoreConditionType.ContactMatchesSavedSearch && !string.IsNullOrEmpty(goalStatus.SearchDefinitionIds))
            {
                var cache = new MemoryCacheManager();
                string[] searchDefinitionIds = goalStatus.SearchDefinitionIds.Split(',').ToArray();
                Contact person = null;
                person = cache.Get<Contact>(contactId.ToString());
                if (person == null)
                {
                    person = contactRepository.FindAll(new List<int>() { contactId }).FirstOrDefault();
                    if (person == null)
                        return hasMatched = true;
                    cache.Add(contactId.ToString(), person, DateTime.Now.AddMinutes(3));
                }
                FindMatchedSavedSearchesResponse queryResponse = contactService.FindMatchedSavedSearches(new FindMatchedSavedSearchesRequest() { Contact = person, AccountId = goalStatus.AccountId });
                if (queryResponse != null && queryResponse.MatchedSearches != null && queryResponse.MatchedSearches.Any())
                {
                    int[] Ids = queryResponse.MatchedSearches.Select(s => s.SearchDefinitionId).ToArray();
                    foreach (string Id in searchDefinitionIds)
                    {
                        int ID = int.Parse(Id);
                        if (Ids.Contains(ID))
                        {
                            hasMatched = true;
                            break;
                        }
                        else
                            hasMatched = false;
                    }
                }
            }
            else
                hasMatched = goalStatus.HasMatched;
            return hasMatched;
        }
    }
}
