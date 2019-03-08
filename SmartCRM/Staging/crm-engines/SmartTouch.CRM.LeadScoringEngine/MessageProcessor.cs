using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.LeadScoringEngine.MessageHandlers;
using SmartTouch.CRM.MessageQueues;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.SearchEngine.Indexing;
using SmartTouch.CRM.SearchEngine.Search;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Contacts;

using SimpleInjector;

namespace SmartTouch.CRM.LeadScoringEngine
{
    public class MessageProcessor
    {
        IEnumerable<IMessageHandler> handlers;

        readonly IIndexingService indexingService;
        readonly IContactService contactService;
        readonly ISearchService<Contact> searchService;

        public MessageProcessor()
        {
            IList<IMessageHandler> _handlers = new List<IMessageHandler>();
            _handlers.Add(new ContactActionTagMessageHandler());
            _handlers.Add(new ContactNoteTagMessageHandler());
            _handlers.Add(new TourTypeMessageHandler());
            _handlers.Add(new LeadSourceMessageHandler());
            _handlers.Add(new FormSubmissionMessageHandler());
            _handlers.Add(new CampaignOpenMessageHandler());
            _handlers.Add(new CampaignLinkClickMessageHandler());
            _handlers.Add(new WebVisitMessageHandler());
            _handlers.Add(new WebPageViewMessageHandler());
            _handlers.Add(new WebPageDurationMessageHandler());
            _handlers.Add(new ContactEmailSentMessageHandler());
            this.handlers = _handlers;

            this.indexingService = IoC.Container.GetInstance<IIndexingService>();
            this.contactService = IoC.Container.GetInstance<IContactService>();
            this.searchService = IoC.Container.GetInstance<ISearchService<Contact>>();
        }

        public MessageHandlerStatus Process(Message message)
        {
            MessageHandlerStatus status = MessageHandlerStatus.InvalidMessageHandler;
            foreach (IMessageHandler handler in handlers)
            {
                status = handler.Handle(message);
                if (status == MessageHandlerStatus.InvalidMessageHandler)
                    continue;
                else
                    break;
            }

            if(status == MessageHandlerStatus.FailedToAuditScore)
            {
                Logger.Current.Error("Message failed to audit." + message.ToString());
                return status;
            }
            else if (status == MessageHandlerStatus.InvalidMessageHandler)
            {
                Logger.Current.Informational("Invalid message." + message.ToString());
                return status;
            }
            else if(status == MessageHandlerStatus.LeadScoreAuditedSuccessfully)
                indexIntoElasticSearch(message);

            return status;            
        }

        void indexIntoElasticSearch(Message message)
        {
            var leadScoreResponse = contactService.GetLeadScore(new GetContactLeadScoreRequest() { ContactId = message.ContactId });
            var searchResults = searchService.Search("", new SearchParameters()
            {
                AccountId = message.AccountId,
                Ids = new List<string> { message.ContactId.ToString() },
                Limit = 1,
                PageNumber = 1,
                Types = new List<Type>() { typeof(Person) }
            });

            if (searchResults != null && searchResults.TotalHits > 0)
            {
                var contact = searchResults.Results.First();
                if (contact.GetType().Equals(typeof(Person)))
                {
                    Person person = contact as Person;
                    person.LeadScore = leadScoreResponse.LeadScore;
                    var count = indexingService.UpdatePartialContact(person);
                    if (count > 0)
                        Logger.Current.Informational("Successfully updated the document to elastic search." + message.MessageId);
                    else
                        Logger.Current.Informational("Could not update the document to elastic search." + message.MessageId);
                }
            }
        }
    }
}
