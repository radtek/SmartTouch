using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Configuration;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

using SmartTouch.CRM.MessageQueues;
using SmartTouch.CRM.Entities;
using LandmarkIT.Enterprise.Utilities.Logging;

namespace SmartTouch.CRM.LeadScoringEngine
{
    public class ScoringEngine
    {
        IObservable<BrokeredMessage> observable;
        SubscriptionClient client;
        BlockingCollection<Message> blockingCollection = new BlockingCollection<Message>(new ConcurrentQueue<Message>());
        IPublishSubscribeService pubSubService;

        public ScoringEngine()
        {
            pubSubService = IoC.Container.GetInstance<IPublishSubscribeService>();
        }

        public void Start()
        {

            string topicName = ConfigurationManager.AppSettings["SERVICEBUS_TOPICNAME"];
            string subsriptionName = ConfigurationManager.AppSettings["SERVICEBUS_LEADSCORESUBSCRIPTION_NAME"];
            pubSubService.Initialize();
            Logger.Current.Informational("subscription:" + subsriptionName);
            Logger.Current.Informational("Connection string:" + ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"]);
            this.ProcessMessage();

            client = SubscriptionClient.Create(topicName, subsriptionName);

            Logger.Current.Informational("Service bus message queue client created successfully.");
            observable = Observable.Create<BrokeredMessage>(
                observer =>
                {
                    client.OnMessage(observer.OnNext, new OnMessageOptions()
                    {
                        AutoComplete = false,
                        AutoRenewTimeout = new TimeSpan(0, 0, 1, 0, 0)
                    });
                    return Disposable.Empty;
                }).Publish().RefCount();

            observable.Subscribe(x =>
            {
                Logger.Current.Informational("Recieved message. BrokeredMessageId:" + x.MessageId);
                x.RenewLock();

                var serializedMessage = x.GetBody<string>();
                Message message = JsonConvert.DeserializeObject<Message>(serializedMessage);
                                
                message.MessageId = x.MessageId;
                message.LockToken = x.LockToken;

                Logger.Current.Informational("Message recieved successfully, details:" + message.ToString());
                this.Feed(message);
            });
        }

        public void Feed(Message message)
        {
            blockingCollection.Add(message);
        }

        public void ProcessMessage()
        {
            Task.Factory.StartNew(consumeMessages, TaskCreationOptions.LongRunning);
        }

        void consumeMessages()
        {            
            MessageProcessor processor = new MessageProcessor();
            foreach (Message message in blockingCollection.GetConsumingEnumerable())
            {
                Logger.Current.Informational("Processing message:" + message.MessageId);
                MessageHandlerStatus status = processor.Process(message);
                if (status == MessageHandlerStatus.LeadScoreAuditedSuccessfully)
                {
                    client.Complete(message.LockToken);
                    Logger.Current.Informational("Processed message, marked as complete:" + message.MessageId);
                }
                else if (status == MessageHandlerStatus.LeadScoreRuleNotDefined)
                {
                    client.Complete(message.LockToken);
                    Logger.Current.Informational("Rule not defined for this message, marked as complete:" + message.MessageId);
                }
                else if (status == MessageHandlerStatus.InvalidMessageHandler)
                {
                    client.Complete(message.LockToken);
                    Logger.Current.Informational("Handler not defined for this message, marked as complete:" + message.MessageId);
                }
                else if (status == MessageHandlerStatus.DuplicateLeadScoreRequest)
                {
                    client.Complete(message.LockToken);
                    Logger.Current.Informational("A duplicate lead score request, marked as complete:" + message.MessageId);
                }
                else if (status == MessageHandlerStatus.FailedToAuditScore)
                {
                    client.Abandon(message.LockToken);
                    Logger.Current.Informational("Failed to process this message, Abandoned message to fall back in the queue:" + message.MessageId);
                }
            }
        }
    }

    //public static class BlockingCollectionExtension
    //{
    //    public static Partitioner<T> GetConsumingPartitioner<T>(this BlockingCollection<T> collection)
    //    {
    //        return new BlockingCollectionPartitioner<T>(collection);
    //    }
    //}

    //private class BlockingCollectionPartitioner<T> : Partitioner<T>
    //{
    //    private BlockingCollection<T> _collection;

    //    public BlockingCollectionPartitioner(
    //        BlockingCollection<T> collection)
    //    {
    //        if (collection == null)
    //            throw new ArgumentNullException("collection");
    //        _collection = collection;
    //    }

    //    public override bool SupportsDynamicPartitions
    //    {
    //        get { return true; }
    //    }

    //    public override IList<IEnumerator<T>> GetPartitions(
    //        int partitionCount)
    //    {
    //        if (partitionCount < 1)
    //            throw new ArgumentOutOfRangeException("partitionCount");
    //        var dynamicPartitioner = GetDynamicPartitions();
    //        return Enumerable.Range(0, partitionCount).Select(_ =>
    //            dynamicPartitioner.GetEnumerator()).ToArray();
    //    }

    //    public override IEnumerable<T> GetDynamicPartitions()
    //    {
    //        return _collection.GetConsumingEnumerable();
    //    }
    //}
}
