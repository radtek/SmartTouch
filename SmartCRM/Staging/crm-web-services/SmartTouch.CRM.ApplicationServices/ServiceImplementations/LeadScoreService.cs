using AutoMapper;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.LeadScore;
using SmartTouch.CRM.ApplicationServices.Messaging.Messages;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.LeadScoreRules;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.MessageQueues;
using SmartTouch.CRM.SearchEngine.Indexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class LeadScoreService : ILeadScoreService
    {
        readonly ILeadScoreRuleService leadScoreRuleService;
        readonly IMessageService messageService;
        readonly IContactService contactService;
        readonly IIndexingService indexingService;
        readonly ILeadScoreRepository leadScoreRepository;
        readonly IContactRepository contactRepository;
        readonly IUnitOfWork unitOfWork;
        readonly IAccountService accountService;
        public LeadScoreService(ILeadScoreRepository leadScoreRepository,
            ILeadScoreRuleService leadScoreRuleService, IUnitOfWork unitOfWork, IMessageService messageService, IContactService contactService, IIndexingService indexingService,
            IContactRepository contactRepository, IAccountService accountService)
        {
            //intentionally skipped to write the logic when contactRepository and unitOfWork are null to know when this is actually required;
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
            this.leadScoreRepository = leadScoreRepository;
            this.unitOfWork = unitOfWork;
            this.leadScoreRuleService = leadScoreRuleService;
            this.contactService = contactService;
            this.indexingService = indexingService;
            this.contactRepository = contactRepository;
            this.messageService = messageService;
            this.accountService = accountService;
        }

        public InsertLeadScoreResponse InsertLeadScore(InsertLeadScoreRequest request)
        {
            List<LeadScore> leadscores = new List<LeadScore>();
            if (request.Rules != null && request.Rules.Any())
            {
                Logger.Current.Informational("Request received to insert leadscore");
                foreach (LeadScoreRule rule in request.Rules)
                {
                    LeadScore leadScore = new LeadScore();
                    leadScore.ContactID = request.ContactId;
                    leadScore.LeadScoreRuleID = rule.Id;
                    leadScore.Score = rule.Score.Value;
                    leadScore.AddedOn = DateTime.Now.ToUniversalTime();
                    leadScore.EntityID = request.EntityId;
                    leadscores.Add(leadScore);
                }
                int score = leadScoreRepository.InsertLeadScores(leadscores,request.AccountId);
                ReIndexLeadScoreContact(request.ContactId, request.AccountId, score);
            }
            Logger.Current.Informational("LeadScores inserted successfully.");
            return new InsertLeadScoreResponse();
        }

        public UpdateLeadScoreResponse UpdateLeadScore(UpdateLeadScoreRequest request)
        {
            LeadScore leadScore = Mapper.Map<LeadScoreViewModel, LeadScore>(request.LeadScoreViewModel);

            //isLeadScoreValid(leadScore);
            leadScoreRepository.Update(leadScore);
            unitOfWork.Commit();
            Logger.Current.Informational("LeadScore Updated successfully.");
            return new UpdateLeadScoreResponse();
        }

        public LeadScoreAuditCheckResponse IsScoreAudited(LeadScoreAuditCheckRequest request)
        {
            LeadScoreAuditCheckResponse response = new LeadScoreAuditCheckResponse();
            response.UnAuditedRules = leadScoreRepository.IsScoreAudited(request.ContactId,
                request.AccountId, (int)request.Condition, request.ConditionValue, request.EntityId, request.Rules);
            response.IsAudited = response.UnAuditedRules != null && response.UnAuditedRules.Any() ? false : true;
            return response;
        }

        public void isLeadScoreValid(LeadScore leadScore)
        {
            // Logger.Current.Verbose("Request received to validate tour with TourID " + tour.Id);
            IEnumerable<BusinessRule> brokenRules = leadScore.GetBrokenRules();

            if (brokenRules.Any())
            {
                StringBuilder brokenRulesBuilder = new StringBuilder();
                foreach (BusinessRule rule in brokenRules)
                {
                    brokenRulesBuilder.AppendLine(rule.RuleDescription);
                }

                throw new Exception(brokenRulesBuilder.ToString());
            }
        }

        public AdjustLeadscoreResponse AdjustLeadscore(AdjustLeadscoreRequest request)
        {
            AdjustLeadscoreResponse response = new AdjustLeadscoreResponse();
            Logger.Current.Informational("Reques received for adding leadscore for a contact");
            if (request != null)
            {
                leadScoreRepository.AdjustLeadScore(request.LeadScore, request.ContactId, request.AccountId, request.WorkflowActionId);
                
            }
            ReIndexLeadScoreContact(request.ContactId, request.AccountId, request.LeadScore);
            return response;
        }

        private void addToTopic(int contactId, int accountId, int score)
        {
            if (contactId != 0 && accountId != 0 && score != 0)
            {
                var message = new TrackMessage()
                    {
                        AccountId = accountId,
                        ContactId = contactId,
                        LeadScoreConditionType = (int)LeadScoreConditionType.LeadscoreReached,
                        EntityId = score,
                        LinkedEntityId = 0,
                        UserId = 0
                    };
                messageService.SendMessages(new SendMessagesRequest()
                    {
                        Message = message
                    });
            }
        }

        private void ReIndexLeadScoreContact(int contactId, int accountId, int score)
        {
            var indexingData = new IndexingData()
            {
                EntityIDs = new List<int>() { contactId },
                IndexType = (int)IndexType.Contacts
            };
            accountService.InsertIndexingData(new Messaging.Accounts.InsertIndexingDataRequest()
            {
                IndexingData = indexingData
            });
            addToTopic(contactId, accountId, score);
        }
        
    }
}
