using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.LeadScoreRules;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using System.Data.SqlClient;
using LandmarkIT.Enterprise.Utilities.Logging;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class MessageRepository : Repository<TrackMessage, long, TrackMessagesDb>, IMessageRepository
    {
        public MessageRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory)
        { }

        public override TrackMessage FindBy(long id)
        {
            var db = ObjectContextFactory.Create();
            var message = db.TrackMessages.Where(tm => tm.TrackMessageID == id).FirstOrDefault();
            return ConvertToDomain(message);
        }

        public override TrackMessagesDb ConvertToDatabaseType(TrackMessage domainType, CRMDb context = null)
        {
            return Mapper.Map<TrackMessage, TrackMessagesDb>(domainType);
        }

        public override TrackMessage ConvertToDomain(TrackMessagesDb databaseType)
        {
            return Mapper.Map<TrackMessagesDb, TrackMessage>(databaseType);
        }

        public override void PersistValueObjects(TrackMessage domainType, TrackMessagesDb dbType, CRMDb context)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// This method sends requested action.
        /// </summary>
        /// <param name="messages"></param>
        public void SendMessages(IEnumerable<TrackMessage> messages)
        {
            try
            {
                var leadScoreMessageTypes = LeadScoreConditionType.AnEmailSent.GetValuesByModule(1);
                var automationMessageTypes = LeadScoreConditionType.WorkflowActivated.GetValuesByModule(2);

                Logger.Current.Informational("Inserting track messages");
                //insert into automation messages
                var db = ObjectContextFactory.Create();
                var automationMessages = messages.Where(tm => automationMessageTypes.Contains(tm.LeadScoreConditionType));
                var automationDbMessages = new List<TrackMessagesDb>();
                automationMessages.Each(a =>
                    {
                        automationDbMessages.Add(Mapper.Map<TrackMessage, TrackMessagesDb>(a));
                    });
                if (automationDbMessages.Any())
                {
                    db.BulkInsert<TrackMessagesDb>(automationDbMessages.ToList());
                    db.SaveChanges();
                }

                //insert into leadscore messages
                var db1 = ObjectContextFactory.Create();
                var leadScoreMessages = messages.Where(tm => leadScoreMessageTypes.Contains(tm.LeadScoreConditionType));
                var leadScoreDbMessages = new List<LeadScoreMessageDb>();
                foreach (var message in leadScoreMessages)
                {
                    var lMessaage = new LeadScoreMessageDb();
                    lMessaage.LeadScoreMessageID = Guid.NewGuid();
                    lMessaage.ContactID = message.ContactId;
                    lMessaage.AccountID = message.AccountId;
                    lMessaage.ConditionValue = message.ConditionValue;
                    lMessaage.UserID = message.UserId;
                    lMessaage.LinkedEntityID = message.LinkedEntityId;
                    lMessaage.EntityID = message.EntityId;
                    lMessaage.LeadScoreConditionType = message.LeadScoreConditionType;
                    lMessaage.CreatedOn = DateTime.UtcNow;
                    lMessaage.LeadScoreProcessStatusID = 701;
                    leadScoreDbMessages.Add(lMessaage);
                }
                if (leadScoreDbMessages.Any())
                {
                    db1.BulkInsert<LeadScoreMessageDb>(leadScoreDbMessages.ToList());
                    db1.SaveChanges();
                }
            }
            catch(Exception ex)
            {
                Logger.Current.Error("Error while sending message/messages", ex);
            }
        }

        /// <summary>
        /// Gets messages related to leadscore
        /// </summary>
        /// <returns></returns>
        public IEnumerable<LeadScoreMessage> GetLeadScoreMessages()
        {
            using (var db = ObjectContextFactory.Create())
            {
                var messages = db.Messages.Where(m => m.LeadScoreProcessStatusID == (int)TrackMessageProcessStatus.ReadyToProcess).OrderBy(m => m.CreatedOn).Take(200);
                if (messages != null)
                    return Mapper.Map<IEnumerable<LeadScoreMessageDb>, IEnumerable<LeadScoreMessage>>(messages);
                else
                    return new List<LeadScoreMessage>();
            }
        }
        /// <summary>
        /// Update leadscore message
        /// </summary>
        /// <param name="leadScoreMessageId"></param>
        /// <param name="remarks"></param>
        public void UpdateLeadScoreMessage(IEnumerable<LeadScoreMessage> messages)
        {
            var messagesDb = Mapper.Map<IEnumerable<LeadScoreMessage>, IEnumerable<LeadScoreMessageDb>>(messages);
            using (var db = ObjectContextFactory.Create())
            {
                messagesDb.ToList().ForEach(m =>
                {
                    if (m.ProcessedOn == DateTime.MinValue)
                        m.ProcessedOn = null;
                    db.Entry<LeadScoreMessageDb>(m).State = System.Data.Entity.EntityState.Modified;
                });
                db.SaveChanges();
            }
        }


        public IEnumerable<TrackMessage> FindAll()
        {
            throw new NotImplementedException();
        }
    }
}
