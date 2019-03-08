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

namespace SmartTouch.CRM.MessageQueues
{
    public class MessageQueuingService : IMessageQueuingService
    {
        string queueName;
        public MessageQueuingService()
        {
            this.queueName = ConfigurationManager.AppSettings["LeadScoreQueue"];            
        }

        public void EnQueue(Message message)
        {
            try
            {
                Logger.Current.Informational("Recieved messaage. " + message.ToString());
                CreateQueueIfNotExists();                
                QueueClient queueClient = QueueClient.Create(queueName);
                var serializedMessage = JsonConvert.SerializeObject(message);
                BrokeredMessage brokeredMessage = new BrokeredMessage(serializedMessage);
                
                Logger.Current.Informational("Sending messaage. ");
                queueClient.Send(brokeredMessage);
                
                Logger.Current.Informational("Message sent successfully. ");
            }
            catch (MessagingException ex)
            {
                if (!ex.IsTransient)
                {
                    Logger.Current.Error("Non-Transient error occurred while queing the message." + message.ToString());
                    throw;
                }
                else
                {
                    HandleTransientErrors(ex, message);
                }
            }
        }

        public Message DeQueue()
        {
            throw new NotImplementedException();
        }

        private void CreateQueueIfNotExists()
        {
            NamespaceManager namespaceManager = NamespaceManager.Create();           
            Logger.Current.Informational("Check if queue exists '" + queueName+"'.");

            if (!namespaceManager.QueueExists(queueName))
            {
                Logger.Current.Informational("Queue does not exist, creating the queue '" + queueName + "'.");
                QueueDescription queueDescription = new QueueDescription(queueName);
                queueDescription.LockDuration = new TimeSpan(0, 5, 0);
                queueDescription = namespaceManager.CreateQueue(queueDescription);
                
                Logger.Current.Informational("Queue created successfully. '" + queueName + "'.");
            }
        }

        private void HandleTransientErrors(MessagingException e, Message message)
        {
            Logger.Current.Error("Transient error occurred while queing the message." + message.ToString());
            //If transient error/exception, let's back-off for 2 seconds and retry
            Console.WriteLine(e.Message);
            Console.WriteLine("Will retry sending the message in 2 seconds");
            Thread.Sleep(2000);
        }
    }
}