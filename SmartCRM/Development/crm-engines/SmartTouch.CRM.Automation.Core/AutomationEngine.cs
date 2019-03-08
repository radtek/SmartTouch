using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Configuration;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceBus;
using Newtonsoft.Json;

using SmartTouch.CRM.MessageQueues;
using SmartTouch.CRM.Entities;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow;
using SmartTouch.CRM.SearchEngine.Indexing;
using SmartTouch.CRM.SearchEngine.Search;
using SmartTouch.CRM.Automation.Core.States;
using AutoMapper;
using SmartTouch.CRM.ApplicationServices.Messaging.Opportunity;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;

namespace SmartTouch.CRM.Automation.Core
{
    public class AutomationEngine
    {
        IObservable<BrokeredMessage> observable;
        SubscriptionClient client;
        BlockingCollection<Message> blockingCollection = new BlockingCollection<Message>(new ConcurrentQueue<Message>());
        BlockingCollection<Message> searchblockingCollection = new BlockingCollection<Message>(new ConcurrentQueue<Message>());
        BlockingCollection<Message> campaignblockingCollection = new BlockingCollection<Message>(new ConcurrentQueue<Message>());
        MessagingFactory messagingFactory = null;
        string topicName;
        string subscriptionName;

        ConcurrentDictionary<int, AutomationWorkflow> automationWorkflows { get; set; }
        ICachingService cachingService;
        IIndexingService indexingService;
        IAdvancedSearchService advancedSearchService;
        IContactService contactService;
        IWorkflowService workflowService;
        IAccountService accountService;
        ITagService tagService;
        ICampaignService campaignService;
        ILeadScoreService leadScoreService;
        IOpportunitiesService opportunityService;
        IPublishSubscribeService pubSubService;
        ICommunicationService communicationService;

        public AutomationEngine(ICachingService cachingService, IIndexingService indexingService,
            IAdvancedSearchService advancedSearchService, IContactService contactService, IWorkflowService workflowService,
            IAccountService accountService, ITagService tagService, ICampaignService campaignService, ILeadScoreService leadScoreService,
            IPublishSubscribeService pubSubService, IOpportunitiesService opportunityService, ICommunicationService communicationService)
        {
            this.cachingService = cachingService;
            this.indexingService = indexingService;
            this.advancedSearchService = advancedSearchService;
            this.contactService = contactService;
            this.workflowService = workflowService;
            this.accountService = accountService;
            this.tagService = tagService;
            this.campaignService = campaignService;
            this.leadScoreService = leadScoreService;
            this.pubSubService = pubSubService;
            this.communicationService = communicationService;
            this.opportunityService = opportunityService;
        }

        public void Start()
        {
            this.loadWorkflows();
            ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.Http;
            topicName = ConfigurationManager.AppSettings["SERVICEBUS_TOPICNAME"];
            subscriptionName = ConfigurationManager.AppSettings["SERVICEBUS_AUTOMATIONSUBSCRIPTION_NAME"];
            pubSubService.Initialize();

            messagingFactory = CreateMessagingFactory();


            client = messagingFactory.CreateSubscriptionClient(topicName, subscriptionName);

            this.ProcessMessage();
            //this.ProcessDeadMessages();

            Logger.Current.Informational("Service bus subscription client created successfully.");
            observable = Observable.Create<BrokeredMessage>(
                observer =>
                {
                    OnMessageOptions options = new OnMessageOptions();
                    options.AutoComplete = false;
                    options.AutoRenewTimeout = new TimeSpan(0, 0, 1, 0, 0);
                    options.ExceptionReceived += options_ExceptionReceived;
                    client.OnMessage(observer.OnNext, options);
                    return Disposable.Empty;
                }).Publish().RefCount();

            observable.Subscribe(x =>
            {
                Logger.Current.Verbose("Recieved message. BrokeredMessageId:" + x.MessageId);
                x.RenewLock();

                var serializedMessage = x.GetBody<string>();
                Message message = JsonConvert.DeserializeObject<Message>(serializedMessage);
                message.MessageId = x.MessageId;
                message.LockToken = x.LockToken;

                Logger.Current.Verbose("Message recieved successfully, details:" + message.ToString());

                this.Feed(message);
            });
        }

        MessagingFactory CreateMessagingFactory()
        {
            string connectionString = ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
            return MessagingFactory.CreateFromConnectionString(connectionString);
        }

        void options_ExceptionReceived(object sender, ExceptionReceivedEventArgs e)
        {
            //Logger.Current.Error("Error occurred while receving the messages, details:", e.Exception);
            ExceptionHandler.Current.HandleException(e.Exception, DefaultExceptionPolicies.LOG_ONLY_POLICY);
        }

        public async void Feed(Message message)
        {
            if (message.LeadScoreConditionType == (int)LeadScoreConditionType.WorkflowActivated)
            {
                IEnumerable<Message> messages = await loadWorkflow((short)message.EntityId, message.AccountId);
                client.Complete(message.LockToken);

                foreach (var feedMessage in messages)
                    blockingCollection.Add(feedMessage);
            }
            else if (message.LeadScoreConditionType == (int)LeadScoreConditionType.WorkflowInactive || message.LeadScoreConditionType == (int)LeadScoreConditionType.WorkflowPaused)
            {
                AutomationWorkflow workflow = null;
                bool result = automationWorkflows.TryRemove(message.EntityId, out workflow);
                if (message.ConditionValue != null && message.ConditionValue.Equals("delayed") && message.ConditionValue.Equals("delayed"))
                {                 
                   workflowService.DeactivateWorkflow(new DeactivateWorkflowRequest() { WorkflowId = message.EntityId });
                }

                if (result && workflow != null)
                    client.Complete(message.LockToken);
            }
            else
                blockingCollection.Add(message);
        }

        public async Task FeedFromTrigger(Message message)
        {
            await this.loadWorkflow((short)message.EntityId, message.EntityId);
        }

        IEnumerable<Message> FeedContactsAffectedByCampaign(int campaignId, int workflowId, int accountId)
        {
            var response = campaignService.GetCampaignRecipients(
                new ApplicationServices.Messaging.Campaigns.GetCampaignRecipientsRequest()
                {
                    CampaignId = campaignId,
                    AccountId = accountId
                });

            IList<Message> messages = new List<Message>();
            foreach (var recipient in response.Recipients)
            {
                messages.Add(new Message()
                {
                    MessageId = Guid.NewGuid().ToString().ToLower(),
                    EntityId = campaignId,
                    ContactId = recipient.ContactID,
                    AccountId = accountId,
                    LeadScoreConditionType = (int)LeadScoreConditionType.CampaignSent,
                    WorkflowId = workflowId
                });
            }
            return messages;
        }

        async Task<IEnumerable<Message>> FeedContactsAffectedBySavedSearch(int searchDefinitionId, int workflowId, int accountId)
        {
            IList<Message> messages = new List<Message>();
            try
            {
                Logger.Current.Informational("For Getting SavedSearch Contacts By SearchdefinitionID In Automation Engine");
                var contactIds = await advancedSearchService.GetSavedSearchContactIds(new ApplicationServices.Messaging.Search.GetSavedSearchContactIdsRequest()
                {
                    SearchDefinitionIds = new int[] { searchDefinitionId },
                    AccountId = accountId
                });

                foreach (var recipient in contactIds)
                {
                    messages.Add(new Message()
                    {
                        MessageId = Guid.NewGuid().ToString().ToLower(),
                        EntityId = searchDefinitionId,
                        ContactId = recipient,
                        AccountId = accountId,
                        LeadScoreConditionType = (int)LeadScoreConditionType.ContactMatchesSavedSearch,
                        WorkflowId = workflowId
                    });
                }
                return messages;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while fetching contactIds for a saved search. ", ex);
                Logger.Current.Informational("SearchdefinitionId : " + searchDefinitionId + " WorkflowId : " + workflowId + " AccountId : " + accountId);
            }

            return messages;
        }

        IEnumerable<Message> FeedContactsAffectedByOpportunityChange(short opportunityStage, int workflowId, int accountId)
        {
            var response = opportunityService.GetOpportunityStageContacts(new GetOpportunityStageContactsRequest()
            {
                StageId = opportunityStage
            });

            var contactIds = response.ContactIds;
            IList<Message> messages = new List<Message>();
            foreach (var Id in contactIds)
            {
                messages.Add(new Message()
                {
                    EntityId = opportunityStage,
                    ContactId = Id,
                    AccountId = accountId,
                    LeadScoreConditionType = (int)LeadScoreConditionType.OpportunityStatusChanged
                });
            }
            return messages;
        }

        public void ProcessMessage()
        {
            //Parallel.ForEach(blockingCollection.GetConsumingPartitioner<Message>(), message =>
            //{
            //    consumeMessage(message);
            //});
            Task.Factory.StartNew(consumeMessages, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(consumeSearchMessages, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(consumeCampaignMessages, TaskCreationOptions.LongRunning);
        }

        //public void ProcessDeadMessages()
        //{
        //    Task.Factory.StartNew(() =>
        //    {
        //        var deadLetterPath = SubscriptionClient.FormatDeadLetterPath(topicName, subscriptionName);
        //        var receiver = messagingFactory.CreateMessageReceiver(deadLetterPath, ReceiveMode.PeekLock);

        //        BrokeredMessage receivedDeadLetterMessage;
        //        while ((receivedDeadLetterMessage = receiver.Receive(TimeSpan.FromSeconds(10))) != null)
        //        {
        //            receivedDeadLetterMessage.RenewLock();
        //            var serializedMessage = receivedDeadLetterMessage.GetBody<string>();
        //            Message message = JsonConvert.DeserializeObject<Message>(serializedMessage);
        //            message.MessageId = receivedDeadLetterMessage.MessageId;
        //            message.LockToken = receivedDeadLetterMessage.LockToken;
        //            consumeMessage(message);
        //        }

        //    }, TaskCreationOptions.LongRunning);
        //}

        void consumeSearchMessages()
        {
            foreach (Message message in searchblockingCollection.GetConsumingEnumerable())
                consumeSearchMessage(message);

            //Parallel.ForEach(searchblockingCollection.GetConsumingPartitionerExt(), message => { consumeSearchMessage(message); });
        }

        void consumeMessages()
        {
            foreach (Message message in blockingCollection.GetConsumingEnumerable())
                consumeMessage(message);
        }

        void consumeCampaignMessages()
        {
            foreach (Message message in campaignblockingCollection.GetConsumingEnumerable())
                consumeCamapaignMessage(message);
        }

        void consumeMessage(Message message)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            try
            {
                Dictionary<int, bool> canComplete = new Dictionary<int, bool>();
                IEnumerable<AutomationWorkflow> workflows = getMatchedWorkflows(message);

                foreach (AutomationWorkflow workflow in workflows)
                {
                    if (canComplete.ContainsKey(workflow.WorkflowId))
                        continue;
                    var result = workflow.ProcessMessage(message, workflow.WorkflowId);

                    if (result == WorkflowStateTransitionStatus.TransitedSuccessfully || result == WorkflowStateTransitionStatus.TransitionDelayed)
                    {
                        var removeFromWorkflows = workflowService.IsEnrolledToRemove(new IsEnrolledToRemoveRequest() { WorkflowId = workflow.WorkflowId }).WorkflowIds;
                        if (removeFromWorkflows != null && removeFromWorkflows.Any())
                        {
                            Dictionary<int, bool> canEnter = new Dictionary<int, bool>();
                            foreach (var id in removeFromWorkflows)
                            {
                                //var startTriggers = automationWorkflows.Values.Where(w => w.WorkflowId == id).SelectMany(s => s.allowedTriggers);
                                //var entries = startTriggers.Where(k => k.Value);
                                //if (entries.Count() > 0)
                                canEnter.Add(id, false);
                                //else
                                //canEnter.Add(id, true);
                            }
                            if (canEnter.Any(d => d.Value == false))
                            {
                                canComplete.Add(workflow.WorkflowId, true);
                                continue;
                            }
                        }
                    }

                    if (result == WorkflowStateTransitionStatus.TransitedSuccessfully || result == WorkflowStateTransitionStatus.UnAuthorizedTransition
                        || result == WorkflowStateTransitionStatus.TransitionDelayed || result == WorkflowStateTransitionStatus.TransitionFailed)
                        canComplete.Add(workflow.WorkflowId, true);
                    else
                        canComplete.Add(workflow.WorkflowId, false);
                }

                if (canComplete.All(d => d.Value == true))
                {
                    sw.Stop();
                    var te = sw.Elapsed;

                    if (!(message.LockToken.ToString() == "00000000-0000-0000-0000-000000000000"))
                    {
                        client.Complete(message.LockToken);
                    }
                    Logger.Current.Verbose("Marked the message as complete, messageid:" + message.MessageId + " Time taken : " + te.TotalMilliseconds);
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                var te = sw.Elapsed;

                client.Abandon(message.LockToken);
                Logger.Current.Error("Exception occurred while consuming the message:" + message.ToString() + "Time taken to process the message : " + te.TotalMilliseconds, ex);
            }
        }

        void consumeSearchMessage(Message message)
        {
            try
            {
                AutomationWorkflow workflow = automationWorkflows.Values.Where(w => w.WorkflowId == message.WorkflowId).FirstOrDefault();
                if (workflow != null)
                    workflow.ProcessMessage(message, workflow.WorkflowId);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error ocured while processing saved-search message : " + ex);
            }
        }

        void consumeCamapaignMessage(Message message)
        {
            try
            {
                AutomationWorkflow workflow = automationWorkflows.Values.Where(w => w.WorkflowId == message.WorkflowId).FirstOrDefault();
                if (workflow != null)
                    workflow.ProcessMessage(message, workflow.WorkflowId);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error ocured while processing campaign sent message : " + ex);
            }
        }

        void loadWorkflows()
        {
            automationWorkflows = new ConcurrentDictionary<int, AutomationWorkflow>();

            var workflowsResponse = workflowService.GetAllActiveWorkflows(
                    new GetActiveWorkflowsRequest() { PageNumber = 1, Limit = 5000 });
            if (workflowsResponse.Workflows != null)
            {
                foreach (var workflowVM in workflowsResponse.Workflows)
                {
                    if (workflowVM == null)
                        continue;
                    var workflowResponse = workflowService.GetWorkFlow(new GetWorkflowRequest()
                    {
                        AccountId = workflowVM.AccountID,
                        WorkflowID = workflowVM.WorkflowID,
                        RequestFromAutomationService = true
                    });

                    if (workflowResponse.WorkflowViewModel == null)
                        continue;
                    var endState = workflowService.GetEndState(new GetWorkflowEndStateRequest() { WorkflowId = workflowVM.WorkflowID });
                    if (endState.WorkflowActionViewModel == null)
                        continue;
                    endState.WorkflowActionViewModel.OrderNumber = workflowResponse.WorkflowViewModel.WorkflowActions.Max(s => s.OrderNumber) + 1;
                    var workflowActions = workflowResponse.WorkflowViewModel.WorkflowActions.ToList();
                    endState.WorkflowActionViewModel.OrderNumber = workflowActions.Max(w => w.OrderNumber) + 1;
                    workflowActions.Add(endState.WorkflowActionViewModel);
                    workflowResponse.WorkflowViewModel.WorkflowActions = workflowActions;

                    var workflowDomain = Mapper.Map<WorkFlowViewModel, Workflow>(workflowResponse.WorkflowViewModel);
                    AutomationWorkflow automationWorkflow = new AutomationWorkflow(workflowDomain, workflowService, tagService,
                        campaignService, contactService, leadScoreService, pubSubService, communicationService);
                    automationWorkflows.AddOrUpdate(workflowDomain.WorkflowID, automationWorkflow, (a, w) => { return w; });
                }
            }
        }

        async Task<IEnumerable<Message>> loadWorkflow(short workflowId, int accountId)
        {
            var workflowResponse = workflowService.GetWorkFlow(new GetWorkflowRequest()
            {
                AccountId = accountId,
                WorkflowID = workflowId,
                RequestFromAutomationService = true
            });
            IList<Message> messages = new List<Message>();

            if (workflowResponse.WorkflowViewModel != null)
            {
                var endState = workflowService.GetEndState(new GetWorkflowEndStateRequest() { WorkflowId = workflowId });
                if (endState.WorkflowActionViewModel != null)
                {
                    endState.WorkflowActionViewModel.OrderNumber = workflowResponse.WorkflowViewModel.WorkflowActions.Max(s => s.OrderNumber) + 1;
                    var workflowActions = workflowResponse.WorkflowViewModel.WorkflowActions.ToList();
                    workflowActions.Add(endState.WorkflowActionViewModel);
                    workflowResponse.WorkflowViewModel.WorkflowActions = workflowActions;

                    var workflowDomain = Mapper.Map<WorkFlowViewModel, Workflow>(workflowResponse.WorkflowViewModel);
                    AutomationWorkflow automationWorkflow = new AutomationWorkflow(workflowDomain, workflowService, tagService, campaignService,
                        contactService, leadScoreService, pubSubService, communicationService);
                    if (!automationWorkflows.ContainsKey(workflowId))
                        automationWorkflows.AddOrUpdate(workflowId, automationWorkflow, (a, w) => { return w; });

                    var sentCampaignTriggers = workflowDomain.Triggers.Where(t => t.TriggerTypeID == WorkflowTriggerType.Campaign && t.CampaignID.HasValue && t.IsStartTrigger);
                    if (sentCampaignTriggers != null && sentCampaignTriggers.Any())
                    {
                        foreach (var campaignId in sentCampaignTriggers.Select(t => t.CampaignID.Value))
                        {
                            IEnumerable<Message> campaignMessages = FeedContactsAffectedByCampaign(campaignId, workflowDomain.WorkflowID, workflowDomain.AccountID);
                            foreach (Message message in campaignMessages)
                                campaignblockingCollection.Add(message);
                        }
                    }

                    var savedSearchTriggers = workflowDomain.Triggers.Where(t => t.TriggerTypeID == WorkflowTriggerType.SmartSearch && t.SearchDefinitionID.HasValue && t.IsStartTrigger);
                    if (savedSearchTriggers != null && savedSearchTriggers.Any())
                    {
                        foreach (var searchDefinitionId in savedSearchTriggers.Select(t => t.SearchDefinitionID.Value))
                        {
                            IEnumerable<Message> savedSearchMessages = await FeedContactsAffectedBySavedSearch(searchDefinitionId, workflowDomain.WorkflowID, workflowDomain.AccountID);
                            foreach (Message message in savedSearchMessages)
                                searchblockingCollection.Add(message);
                        }
                    }

                    var opportunityStageChangeTrigger = workflowDomain.Triggers.Where(t => t.TriggerTypeID == WorkflowTriggerType.OpportunityStatusChanged && t.OpportunityStageID.HasValue && t.IsStartTrigger);
                    if (opportunityStageChangeTrigger != null && opportunityStageChangeTrigger.Any())
                    {
                        IEnumerable<Message> opportunityChangeMessages = FeedContactsAffectedByOpportunityChange(opportunityStageChangeTrigger.FirstOrDefault().OpportunityStageID.Value,
                            workflowDomain.WorkflowID, workflowDomain.AccountID);
                        messages = messages.Concat(opportunityChangeMessages).ToList();
                    }
                }
            }
            return messages;
        }

        IEnumerable<AutomationWorkflow> getMatchedWorkflows(Message message)
        {
            IEnumerable<AutomationWorkflow> workflows = new List<AutomationWorkflow>();

            if ((message.LeadScoreConditionType == (int)LeadScoreConditionType.ContactWaitPeriodEnded || message.LeadScoreConditionType == (int)LeadScoreConditionType.TriggerWorkflow) && message.WorkflowId != 0)
                workflows = automationWorkflows.Values.Where(w => w.WorkflowId == message.WorkflowId);
            else if (message.LeadScoreConditionType != (int)LeadScoreConditionType.ContactClicksLink)
                workflows = automationWorkflows.Values.Where(w => w.AccountId == message.AccountId
                            &&
                            (w.allowedTriggers.IsMatchTrigger(message))
                            ||
                            (w.stopTriggers.IsMatchTrigger(message))
                            );
            else
                workflows = automationWorkflows.Values.Where(w => w.AccountId == message.AccountId
                            &&
                           (w.allowedTriggers.IsMatchTrigger(message))
                           ||
                           (w.stopTriggers.IsMatchTrigger(message))
                           ||
                           w.states.Any(s => s.AllowedTriggers.ContainsKey(LeadScoreConditionType.ContactClicksLink) && s.EntityId == message.LinkedEntityId));
            return workflows;
        }
    }
}
