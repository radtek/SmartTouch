using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration;
using Newtonsoft.Json;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.Entities;
using System.Data.SqlClient;
using Dapper;
namespace SmartTouch.CRM.MessageQueues
{
    public class PublishSubscribeService : IPublishSubscribeService
    {
        string topicName;
        string leadScoreSubscriptionName;
        string automationSubscriptionName;
        readonly IEnumerable<int> leadScoreMessages;
        readonly IEnumerable<int> automationMessages;
        public PublishSubscribeService()
        {
            this.topicName = ConfigurationManager.AppSettings["SERVICEBUS_TOPICNAME"];
            this.leadScoreSubscriptionName = ConfigurationManager.AppSettings["SERVICEBUS_LEADSCORESUBSCRIPTION_NAME"];
            this.automationSubscriptionName = ConfigurationManager.AppSettings["SERVICEBUS_AUTOMATIONSUBSCRIPTION_NAME"];
            leadScoreMessages = LeadScoreConditionType.AnEmailSent.GetValuesByModule(1);
            automationMessages = LeadScoreConditionType.WorkflowActivated.GetValuesByModule(2);
        }
        private void Send(TopicClient client, Message message)
        {
            Logger.Current.Informational("Sending messaage. ");
            var serializedMessage = JsonConvert.SerializeObject(message);
            BrokeredMessage brokeredMessage = new BrokeredMessage(serializedMessage);
            brokeredMessage.Properties.Add("LeadScoreConditionType", message.LeadScoreConditionType);
            brokeredMessage.ContentType = message.GetType().AssemblyQualifiedName;
            brokeredMessage.ScheduledEnqueueTimeUtc = DateTime.UtcNow;
            Task send = client.SendAsync(brokeredMessage).ContinueWith(t =>
            {
                Logger.Current.Informational("Message sent successfully. ");
                TrackMessage(message, DateTime.UtcNow);
            });
            Task.WaitAll(send);
        }
        private void Send(IEnumerable<Message> messages)
        {
            var tasks = new List<Task>();
            TopicClient topicClient = TopicClient.Create(topicName);
            foreach(Message message in messages)
            {
                Logger.Current.Informational("Sending messaage. ");
                var serializedMessage = JsonConvert.SerializeObject(message);
                BrokeredMessage brokeredMessage = new BrokeredMessage(serializedMessage);
                brokeredMessage.Properties.Add("LeadScoreConditionType", message.LeadScoreConditionType);
                brokeredMessage.ContentType = message.GetType().AssemblyQualifiedName;
                brokeredMessage.ScheduledEnqueueTimeUtc = DateTime.UtcNow;
                Task send = topicClient.SendAsync(brokeredMessage).ContinueWith(t =>
                {
                    Logger.Current.Informational("Message sent successfully. ");
                    TrackMessage(message,DateTime.UtcNow);
                });
                tasks.Add(send);
            }
            Task.WaitAll(tasks.ToArray());
        }
        public void SendMessages(IEnumerable<Message> messages)
        {
            if (messages!= null && messages.Any())
            {
                var leadScoreConditionType = messages.FirstOrDefault().LeadScoreConditionType;
                if (leadScoreMessages.Contains(leadScoreConditionType))
                {
                    messages.ToList().ForEach(m =>
                    {
                        SetLeadScoreMessage(m);
                    });

                }
                else
                {
                    Send(messages);
                }
            }
        }
        public void SendMessage(Message message)
        {
            try
            {
                if(leadScoreMessages.Contains(message.LeadScoreConditionType))
                {
                    SetLeadScoreMessage(message);
                }
                if(automationMessages.Contains(message.LeadScoreConditionType))
                {
                    Logger.Current.Informational("Sending message. " + message.ToString());
                    TopicClient topicClient = TopicClient.Create(topicName);
                    Send(topicClient, message);
                }
            }
            catch (MessagingException ex)
            {
                if (!ex.IsTransient)
                {
                    Logger.Current.Error("Non-Transient error occurred while sending the message." + message.ToString());
                    throw;
                }
                else
                {
                    HandleTransientErrors(ex, message);
                }
            }
        }

        public void SendScheduledMessage(Message message, DateTime scheduleDate)
        {
            try
            {
                Logger.Current.Informational("Sending message with delay. " + message.ToString());
                //CreateTopicIfNotExists();

                //TopicClient topicClient = TopicClient.Create(topicName);
                var serializedMessage = JsonConvert.SerializeObject(message);
                BrokeredMessage brokeredMessage = new BrokeredMessage(serializedMessage);
                brokeredMessage.Properties.Add("LeadScoreConditionType", message.LeadScoreConditionType);
                //Logger.Current.Informational("Sending messaage. message:" + message.ToString());
                brokeredMessage.ScheduledEnqueueTimeUtc = scheduleDate;
                //topicClient.Send(brokeredMessage);
                TrackMessage(message, scheduleDate);
                //Logger.Current.Informational("Message sent successfully. ");
            }
            catch (MessagingException ex)
            {
                if (!ex.IsTransient)
                {
                    Logger.Current.Error("Non-Transient error occurred while sending the message." + message.ToString());
                    throw;
                }
                else
                {
                    HandleTransientErrors(ex, message);
                }
            }
        }

        public Message RecieveMessage(Subscriber subscriber)
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            CreateTopicIfNotExists();
        }

        private void CreateTopicIfNotExists()
        {
            NamespaceManager namespaceManager = NamespaceManager.Create();
            Logger.Current.Informational("Check if topic exists '" + topicName + "'.");

            if (!namespaceManager.TopicExists(topicName))
            {
                Logger.Current.Informational("Topic does not exist, creating the topic '" + topicName + "'.");
                TopicDescription topicDescription = new TopicDescription(topicName)
                {
                    RequiresDuplicateDetection = true
                };
                topicDescription = namespaceManager.CreateTopic(topicDescription);
                Logger.Current.Informational("Topic created successfully. '" + topicName + "'.");
            }

            if (!namespaceManager.SubscriptionExists(topicName, leadScoreSubscriptionName))
            {
                string conditionType = "LeadScoreConditionType != ";
                StringBuilder filter = new StringBuilder(conditionType).Append((byte)LeadScoreConditionType.ContactTagAdded);
                filter.Append(" AND ").Append(conditionType).Append((byte)LeadScoreConditionType.ContactMatchesSavedSearch);
                filter.Append(" AND ").Append(conditionType).Append((byte)LeadScoreConditionType.WorkflowActivated);
                filter.Append(" AND ").Append(conditionType).Append((byte)LeadScoreConditionType.OpportunityStatusChanged);
                filter.Append(" AND ").Append(conditionType).Append((byte)LeadScoreConditionType.ContactWaitPeriodEnded);
                filter.Append(" AND ").Append(conditionType).Append((byte)LeadScoreConditionType.WorkflowInactive);
                filter.Append(" AND ").Append(conditionType).Append((byte)LeadScoreConditionType.WorkflowPaused);
                filter.Append(" AND ").Append(conditionType).Append((byte)LeadScoreConditionType.UnsubscribeEmails);
                filter.Append(" AND ").Append(conditionType).Append((byte)LeadScoreConditionType.ContactLifecycleChange);
                filter.Append(" AND ").Append(conditionType).Append((byte)LeadScoreConditionType.CampaignSent);

                SqlFilter leadScoreFilter = new SqlFilter(filter.ToString());
                SubscriptionDescription leadScoreSubDesc = new SubscriptionDescription(topicName, leadScoreSubscriptionName);
                leadScoreSubDesc.LockDuration = new TimeSpan(0, 3, 0);

                leadScoreSubDesc = namespaceManager.CreateSubscription(leadScoreSubDesc, leadScoreFilter);
                leadScoreSubDesc.EnableDeadLetteringOnMessageExpiration = false;
                namespaceManager.UpdateSubscription(leadScoreSubDesc);
                Logger.Current.Informational("Subscription created successfully. '" + leadScoreSubscriptionName + "'.");
            }

            if (!namespaceManager.SubscriptionExists(topicName, automationSubscriptionName))
            {
                string conditionType = "LeadScoreConditionType != ";
                StringBuilder filter = new StringBuilder(conditionType).Append((byte)LeadScoreConditionType.ContactOpensEmail);
                filter.Append(" AND ").Append(conditionType).Append((byte)LeadScoreConditionType.ContactVisitsWebsite);
                filter.Append(" AND ").Append(conditionType).Append((byte)LeadScoreConditionType.ContactVisitsWebPage);
                filter.Append(" AND ").Append(conditionType).Append((byte)LeadScoreConditionType.PageDuration);
                filter.Append(" AND ").Append(conditionType).Append((byte)LeadScoreConditionType.ContactActionTagAdded);
                filter.Append(" AND ").Append(conditionType).Append((byte)LeadScoreConditionType.ContactNoteTagAdded);
                filter.Append(" AND ").Append(conditionType).Append((byte)LeadScoreConditionType.ContactLeadSource);
                filter.Append(" AND ").Append(conditionType).Append((byte)LeadScoreConditionType.ContactTourType);
                filter.Append(" AND ").Append(conditionType).Append((byte)LeadScoreConditionType.ContactTagRemoved);
                filter.Append(" AND ").Append(conditionType).Append((byte)LeadScoreConditionType.UnsubscribeEmails);

                SqlFilter automationMessageFilter = new SqlFilter(filter.ToString());

                SubscriptionDescription automationSubDesc = new SubscriptionDescription(topicName, automationSubscriptionName);
                automationSubDesc.LockDuration = new TimeSpan(0, 3, 0);
                automationSubDesc = namespaceManager.CreateSubscription(automationSubDesc, automationMessageFilter);
                automationSubDesc.EnableDeadLetteringOnMessageExpiration = false;
                namespaceManager.UpdateSubscription(automationSubDesc);
                Logger.Current.Informational("Subscription created successfully. '" + automationSubscriptionName + "'.");
            }
        }

        private void HandleTransientErrors(MessagingException e, Message message)
        {
            Logger.Current.Error("Transient error occurred while sending the message." + message.ToString());
            //If transient error/exception, let's back-off for 2 seconds and retry
            Console.WriteLine(e.Message);
            Console.WriteLine("Will retry sending the message in 2 seconds");
            Thread.Sleep(200);
        }

        private void TrackMessage(Message message, DateTime schedule)
        {
            var cs = System.Configuration.ConfigurationManager.ConnectionStrings["CRMDb"].ToString();
            var isPublished = 1;
            DateTime? publishedOn = DateTime.UtcNow;
            if(message.LeadScoreConditionType == 17)
            {
                isPublished = 0;
                publishedOn = null;
            }
            try
            {
                Logger.Current.Informational("Inserting in to tracking  messages.");
                using (var db = new SqlConnection(cs))
                {
                    var sql = @"insert into TrackMessages (messageid, entityid, userid, leadscoreconditiontype, contactid, accountid, linkedentityid, locktoken, conditionvalue,createdon, scheduledon, ispublished, publishedOn, workflowId)    
                            values (@messageid, @entityid, @userid, @leadscoreconditiontype, @contactid, @accountid, @linkedentityid, @locktoken, @conditionvalue,@createdon,@scheduledon,@ispublished,@publishedon, @workflowid)";
                    db.Execute(sql, new
                    {
                        messageid = message.MessageId,
                        entityid = message.EntityId,
                        userid = message.UserId,
                        leadscoreconditiontype = message.LeadScoreConditionType,
                        contactid = message.ContactId,
                        accountid = message.AccountId,
                        linkedentityid = message.LinkedEntityId,
                        locktoken = message.LockToken,
                        conditionvalue = message.ConditionValue,
                        ScheduledOn = schedule,
                        createdon = DateTime.UtcNow,
                        ispublished = isPublished,
                        publishedon = publishedOn,
                        workflowid = message.WorkflowId
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occurred while updating message to the tracking .", ex);
            }
            
        }

        private void SetLeadScoreMessage(Message message)
        {
            var sql = @"insert into [dbo].[leadscoremessages]
                           ([leadscoremessageid]
                           ,[entityid]
                           ,[userid]
                           ,[leadscoreconditiontype]
                           ,[contactid]
                           ,[accountid]
                           ,[linkedentityid]
                           ,[conditionvalue]
                           ,[createdon])
                     values
                           (@leadscoremessageid
                           ,@entityid
                           ,@userid
                           ,@leadscoreconditiontype 
                           ,@contactid 
                           ,@accountid 
                           ,@linkedentityid 
                           ,@conditionvalue
                           ,@createdon)";
            
            var cs = System.Configuration.ConfigurationManager.ConnectionStrings["CRMDb"].ToString();
            try
            {
                using (var db = new SqlConnection(cs))
                {
                    db.Execute(sql, new
                    {
                        leadscoremessageid = Guid.NewGuid(),
                        entityid = message.EntityId,
                        userid = message.UserId,
                        leadscoreconditiontype = message.LeadScoreConditionType,
                        contactid = message.ContactId,
                        accountid = message.AccountId,
                        linkedentityid = message.LinkedEntityId,
                        conditionvalue = message.ConditionValue,
                        createdon = DateTime.UtcNow
                    });
                }
            }
            catch(Exception ex)
            {
                Logger.Current.Error("An error occurred while inserting lead score message.", ex);
            }
            

        }

        public void SendMessageWithOutTracking(Message message)
        {
            try
            {
                Logger.Current.Informational("Sending message. " + message.ToString());
                TopicClient topicClient = TopicClient.Create(topicName);
                
                var serializedMessage = JsonConvert.SerializeObject(message);
                BrokeredMessage brokeredMessage = new BrokeredMessage(serializedMessage);
                brokeredMessage.Properties.Add("LeadScoreConditionType", message.LeadScoreConditionType);
                brokeredMessage.ContentType = message.GetType().AssemblyQualifiedName;
                brokeredMessage.ScheduledEnqueueTimeUtc = DateTime.UtcNow;

                Task send = topicClient.SendAsync(brokeredMessage).ContinueWith(t =>
                {
                    Logger.Current.Informational("Message sent successfully. ");
                });

                Task.WaitAll(send);
            }
            catch (MessagingException ex)
            {
                if (!ex.IsTransient)
                {
                    Logger.Current.Error("Non-Transient error occurred while sending the message." + message.ToString());
                    throw;
                }
                else
                {
                    HandleTransientErrors(ex, message);
                }
            }
        }
    }
}
