using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using Quartz;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.MessageQueues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Automation.Engine
{
    public class MessagingScheduler: IJob
    {
        IPublishSubscribeService pubSubService;
        IWorkflowRepository workflowRepository;
        private static bool isRunning = default(bool);

        public MessagingScheduler()
        {
            this.pubSubService = IoC.Container.GetInstance<IPublishSubscribeService>();
            this.workflowRepository = IoC.Container.GetInstance<IWorkflowRepository>();
        }
        /// <summary>
        /// job execution
        /// This job gets data from db and publish messages exactly in the given time.
        /// </summary>
        /// <param name="context"></param>
        public void Execute(IJobExecutionContext context)
        {
            if (isRunning) return;
            isRunning = true;
            try
            {
                List<Message> messagesList = new List<Message>();
                Logger.Current.Informational("Querying for scheduled messages.");
                var messages = workflowRepository.GetScheduledMessages(new TimeSpan(0, 0, 0));
                foreach (IDictionary<string, object> m in messages)
                {
                    var k = new Message()
                    {
                        TrackMessageId = CheckIfNull<int>(m["TrackMessageID"]),
                        MessageId = CheckIfNull<string>(m["MessageID"]),
                        ContactId = CheckIfNull<int>(m["ContactID"]),
                        EntityId = CheckIfNull<int>(m["EntityID"]),
                        UserId = CheckIfNull<int>(m["UserID"]),
                        LeadScoreConditionType = CheckIfNull<byte>(m["LeadScoreConditionType"]),
                        AccountId = CheckIfNull<int>(m["AccountID"]),
                        LinkedEntityId = CheckIfNull<int>(m["LinkedEntityID"]),
                        //LockToken = (Guid)m["LockToken"],
                        ConditionValue = CheckIfNull<string>(m["ConditionValue"]),
                        WorkflowId = CheckIfNull<int>(m["WorkflowId"])
                    };
                    messagesList.Add(k);
                }

                messagesList.ForEach(m =>
                {
                    pubSubService.SendMessageWithOutTracking(m);
                    Logger.Current.Informational("Updating publish status of message: " + m.ToString());
                    workflowRepository.UpdateMessageStatus(m.TrackMessageId, true);
                });
            }
            catch(Exception ex)
            {
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
            }
            finally
            {
                isRunning = false;
            }
            
        }
        /// <summary>
        /// Checks given object is null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        private T CheckIfNull<T>(object input)
        {
            if (input == null)
                return default(T);
            return (T)input;
        }
    }
}
