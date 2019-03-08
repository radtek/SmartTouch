using System;
using System.Collections.Generic;
using System.Linq;
using LandmarkIT.Enterprise.Extensions;
using Quartz;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.LeadScore;
using SmartTouch.CRM.ApplicationServices.Messaging.Messages;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.LeadScoreRules;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler;

namespace SmartTouch.CRM.JobProcessor.Jobs.Leads
{
    public class LeadScoreJob : BaseJob
    {
        private readonly ILeadScoreRuleService _leadScoreRuleService;
        private readonly ILeadScoreService _leadScoreService;
        private readonly IAccountService _accountService;
        private readonly IMessageService _messageService;

        public LeadScoreJob(
            ILeadScoreRuleService leadScoreRuleService,
            ILeadScoreService leadScoreService,
            IAccountService accountService,
            IMessageService messageService)
        {
            _leadScoreRuleService = leadScoreRuleService;
            _leadScoreService = leadScoreService;
            _accountService = accountService;
            _messageService = messageService;
        }

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            var messages = _messageService
                .GetLeadScoreMessages()
                .Messages
                .ToArray();

            if (!messages.IsAny())
                return;

            var auditedMessages = new List<LeadScoreMessage>();
            Log.Informational($"Received messages count: {messages.Length}");
            foreach (var message in messages)
            {
                try
                {
                    Log.Informational($"Processing LeadScore Message message {message}");
                    GetLeadScoreRuleByConditionResponse leadScoreRuleResponse;
                    string conditionValue;
                    int entityId;
                    SetCondtionValueAndEntityValue(message, out entityId, out conditionValue);

                    var leadScoreRuleRequest = new GetLeadScoreRuleByConditionRequest
                    {
                        AccountId = message.AccountID,
                        Condition = (LeadScoreConditionType)message.LeadScoreConditionType,
                        ConditionValue = conditionValue,
                        EntityID = entityId
                    };

                    if (message.LeadScoreConditionType == (byte)LeadScoreConditionType.ContactClicksLink)
                        leadScoreRuleResponse = _leadScoreRuleService.GetCampaignClickLeadScoreRule(leadScoreRuleRequest);
                    else
                        leadScoreRuleResponse = _leadScoreRuleService.GetLeadScoreRules(leadScoreRuleRequest);

                    Log.Informational($"Audit leadscore, condtion value: {conditionValue}, entity id:{entityId}, message {message.ToString()}");

                    if (leadScoreRuleResponse.Rules != null && leadScoreRuleResponse.Rules.Any())
                    {
                        Log.Informational(string.Format("Audit leadscore count, Rule ID: {0}", leadScoreRuleResponse.Rules.Count(), leadScoreRuleResponse.Rules.Select(s => s.Id)));

                        var response = _leadScoreService.IsScoreAudited(
                            new LeadScoreAuditCheckRequest
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
                            Log.Informational($"Update leadscore, message {message.ToString()}");
                            Log.Informational(string.Format("while inserting Audit leadscore count, Rule ID: {0}", leadScoreRuleResponse.Rules.Count(), leadScoreRuleResponse.Rules.Select(s => s.Id)));

                            _leadScoreService.InsertLeadScore(new InsertLeadScoreRequest()
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
                            Log.Informational($"Leadscore already applied, message {message.ToString()}");
                            message.LeadScoreProcessStatusID = (int)TrackMessageProcessStatus.Processed;
                            message.Remarks = "Lead score is already applied";
                        }
                    }
                    else if (leadScoreRuleResponse.Rules == null || !leadScoreRuleResponse.Rules.Any())
                    {
                        Log.Informational($"No rule defined, message {message.ToString()}");
                        message.Remarks = "No rule defined";
                        message.LeadScoreProcessStatusID = (int)TrackMessageProcessStatus.Ignored;
                    }
                    message.ProcessedOn = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    Log.Informational($"Error occurred while processing leadscore {message} {ex}");
                    message.ProcessedOn = DateTime.MinValue;
                    message.LeadScoreProcessStatusID = (int)TrackMessageProcessStatus.Error;
                    message.Remarks = $"Lead score not processed, {ex}";
                }
            }
            _messageService.UpdateLeadScoreMessage(new UpdateLeadScoreMessage()
            {
                Messages = messages
            });
            //update all messages
            if (auditedMessages.Any())
            {
                //index all contacts messages
                var indexingData = new IndexingData
                {
                    EntityIDs = auditedMessages.Select(m => m.ContactID).ToList(),
                    IndexType = (int)IndexType.Contacts
                };
                _accountService.InsertIndexingData(new InsertIndexingDataRequest { IndexingData = indexingData });
            }
        }

        private void SetCondtionValueAndEntityValue(LeadScoreMessage message, out int entityId, out string conditionValue)
        {
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
