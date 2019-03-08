using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.LeadScore;
using SmartTouch.CRM.ApplicationServices.Messaging.Messages;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.LeadScoreRules;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.MessageQueues;
using SmartTouch.CRM.SearchEngine.Indexing;
using SmartTouch.CRM.SearchEngine.Search;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.JobProcessor
{
    public class LeadScoreProcessor : CronJobProcessor
    {
        readonly IIndexingService indexingService;
        readonly IContactService contactService;
        readonly ISearchService<Contact> searchService;
        readonly ILeadScoreRuleService leadScoreRuleService;
        readonly ILeadScoreService leadScoreService;
        readonly IAccountService accountService;
        readonly IMessageService messageService;
        public LeadScoreProcessor(CronJobDb cronJob, JobService jobService, string leadScoreProcessorCacheName)
            : base(cronJob, jobService, leadScoreProcessorCacheName)
        {
            this.indexingService = IoC.Container.GetInstance<IIndexingService>();
            this.contactService = IoC.Container.GetInstance<IContactService>();
            this.searchService = IoC.Container.GetInstance<ISearchService<Contact>>();
            this.leadScoreRuleService = IoC.Container.GetInstance<ILeadScoreRuleService>();
            this.leadScoreService = IoC.Container.GetInstance<ILeadScoreService>();
            this.accountService = IoC.Container.GetInstance<IAccountService>();
            this.messageService = IoC.Container.GetInstance<IMessageService>();
        }

        protected override void Execute()
        {
            var messages = messageService.GetLeadScoreMessages().Messages;
            if (messages.IsAny())
            {
                while(true)
                {
                    var auditedMessages = new List<LeadScoreMessage>();
                    Logger.Current.Informational(string.Format("Received messages count: {0}", messages.Count()));
                    foreach (var message in messages)
                    {
                        try
                        {
                            Logger.Current.Informational(string.Format("Processing LeadScore Message message {0}", message.ToString()));
                            GetLeadScoreRuleByConditionResponse leadScoreRuleResponse;
                            string conditionValue = string.Empty;
                            int entityId = 0;
                            SetCondtionValueAndEntityValue(message, out entityId, out conditionValue);

                            var leadScoreRuleRequest = new GetLeadScoreRuleByConditionRequest()
                            {
                                AccountId = message.AccountID,
                                Condition = (LeadScoreConditionType)message.LeadScoreConditionType,
                                ConditionValue = conditionValue,
                                EntityID = entityId
                            };

                            if (message.LeadScoreConditionType == (byte)LeadScoreConditionType.ContactClicksLink) leadScoreRuleResponse = leadScoreRuleService.GetCampaignClickLeadScoreRule(leadScoreRuleRequest);
                            else leadScoreRuleResponse = leadScoreRuleService.GetLeadScoreRules(leadScoreRuleRequest);

                            Logger.Current.Informational(string.Format("Audit leadscore, condtion value: {0}, entity id:{1}, message {2}", conditionValue, entityId, message.ToString()));

                            if (leadScoreRuleResponse.Rules != null && leadScoreRuleResponse.Rules.Any())
                            {
                                Logger.Current.Informational(string.Format("Audit leadscore count, Rule ID: {0}", leadScoreRuleResponse.Rules.Count(), leadScoreRuleResponse.Rules.Select(s => s.Id)));

                                var response = leadScoreService.IsScoreAudited(
                                    new LeadScoreAuditCheckRequest()
                                    {
                                        AccountId = message.AccountID,
                                        ContactId = message.ContactID,
                                        Condition = (LeadScoreConditionType)message.LeadScoreConditionType,
                                        ConditionValue = conditionValue,
                                        EntityId = entityId,
                                        Rules = leadScoreRuleResponse.Rules
                                    });

                                if (!response.IsAudited && (response.UnAuditedRules != null && response.UnAuditedRules.Any()))
                                {
                                    Logger.Current.Informational(string.Format("Update leadscore, message {0}", message.ToString()));
                                    Logger.Current.Informational(string.Format("while inserting Audit leadscore count, Rule ID: {0}", leadScoreRuleResponse.Rules.Count(), leadScoreRuleResponse.Rules.Select(s => s.Id)));

                                    leadScoreService.InsertLeadScore(new InsertLeadScoreRequest()
                                    {
                                        Condition = (LeadScoreConditionType)message.LeadScoreConditionType,
                                        ConditionValue = conditionValue,
                                        ContactId = message.ContactID,
                                        AccountId = message.AccountID,
                                        RequestedBy = message.UserID,
                                        EntityId = entityId,
                                        Rules = response.UnAuditedRules
                                    });
                                    message.Remarks = "Lead score applied";
                                    message.LeadScoreProcessStatusID = (int)TrackMessageProcessStatus.Processed;
                                    auditedMessages.Add(message);
                                }
                                else if (response.IsAudited)
                                {
                                    Logger.Current.Informational(string.Format("Leadscore already applied, message {0}", message.ToString()));
                                    message.LeadScoreProcessStatusID = (int)TrackMessageProcessStatus.Processed;
                                    message.Remarks = "Lead score is already applied";
                                }
                            }
                            else if (leadScoreRuleResponse.Rules == null || !leadScoreRuleResponse.Rules.Any())
                            {
                                Logger.Current.Informational(string.Format("No rule defined, message {0}", message.ToString()));
                                message.Remarks = "No rule defined";
                                message.LeadScoreProcessStatusID = (int)TrackMessageProcessStatus.Ignored;
                            }
                            message.ProcessedOn = DateTime.UtcNow;
                        }
                        catch (Exception ex)
                        {
                            Logger.Current.Informational(string.Format("Error occurred while processing leadscore {0} {1}", message.ToString(), ex));
                            message.ProcessedOn = DateTime.MinValue;
                            message.LeadScoreProcessStatusID = (int)TrackMessageProcessStatus.Error;
                            message.Remarks = string.Format("Lead score not processed, {0}", ex.ToString());
                        }
                    }
                    messageService.UpdateLeadScoreMessage(new UpdateLeadScoreMessage()
                    {
                        Messages = messages
                    });
                    //update all messages
                    if (auditedMessages != null && auditedMessages.Any())
                    {
                        //index all contacts messages
                        var indexingData = new IndexingData()
                        {
                            EntityIDs = auditedMessages.Select(m => m.ContactID).ToList(),
                            IndexType = (int)IndexType.Contacts
                        };
                        accountService.InsertIndexingData(new InsertIndexingDataRequest() { IndexingData = indexingData });
                    }

                    messages = messageService.GetLeadScoreMessages().Messages;
                    if (messages.IsAny() == false) break;

                }
                
            }
           
        }

        /// <summary>
        /// Set entity id and condition value
        /// </summary>
        /// <param name="message"></param>
        /// <param name="entityId"></param>
        /// <param name="conditionValue"></param>
        private void SetCondtionValueAndEntityValue(LeadScoreMessage message, out int entityId, out string conditionValue)
        {
            entityId = 0;
            conditionValue = string.Empty;
            switch ((LeadScoreConditionType)message.LeadScoreConditionType)
            {
                case LeadScoreConditionType.ContactClicksLink:
                    entityId = message.LinkedEntityID;
                    conditionValue = message.EntityID.ToString();
                    break;
                case LeadScoreConditionType.AnEmailSent:
                    conditionValue = string.Empty;
                    entityId = message.EntityID;
                    break;
                case LeadScoreConditionType.ContactSubmitsForm:
                case LeadScoreConditionType.ContactOpensEmail:
                    conditionValue = message.EntityID.ToString();
                    entityId = message.EntityID;
                    break;
                case LeadScoreConditionType.ContactActionTagAdded:
                case LeadScoreConditionType.ContactNoteTagAdded:
                case LeadScoreConditionType.ContactLeadSource:
                case LeadScoreConditionType.ContactTourType:
                case LeadScoreConditionType.ContactNoteCategoryAdded:
                case LeadScoreConditionType.ContactActionCompleted:
                    conditionValue = message.LinkedEntityID.ToString();
                    entityId = message.EntityID;
                    break;
                case LeadScoreConditionType.ContactVisitsWebPage:
                case LeadScoreConditionType.ContactVisitsWebsite:
                case LeadScoreConditionType.PageDuration:
                    conditionValue = message.ConditionValue;
                    entityId = message.EntityID;
                    break;
                default:
                    conditionValue = message.LinkedEntityID.ToString();
                    entityId = message.EntityID;
                    break;
            }
        }
    }
}
