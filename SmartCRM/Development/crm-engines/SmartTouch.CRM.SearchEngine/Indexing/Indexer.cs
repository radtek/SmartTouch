using System;
using System.Collections.Generic;
using System.Configuration;

using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Tags;
using DA = SmartTouch.CRM.Domain.Actions;
using SmartTouch.CRM.Domain.Notes;
using SmartTouch.CRM.Domain.Tours;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Opportunities;

using Nest;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.SuppressedEmails;

namespace SmartTouch.CRM.SearchEngine.Indexing
{
    public abstract class Indexer<T, S> where T : EntityBase<S>
    {
        string uri;
        public Indexer()
        {
            uri = ConfigurationManager.AppSettings["ELASTICSEARCH_INSTANCE"];
        }

        protected ElasticClient ElasticClient(string indexName)
        {
            var searchBoxUri = new Uri(uri);
            var settings = new ConnectionSettings(searchBoxUri);
            //settings.MaximumRetries(5);
            settings.SetPingTimeout(1000);
            settings.SetDefaultIndex(indexName);
            settings.PluralizeTypeNames();
            settings.SetJsonSerializerSettingsModifier(s => {
                s.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });
            return new ElasticClient(settings);
        }

        protected string GetBaseIndexName()
        {
            if (typeof(T).Equals(typeof(Contact)))
                return "contacts";
            else if (typeof(T).Equals(typeof(Tag)))
                return "tags";
            else if (typeof(T).Equals(typeof(Campaign)))
                return "campaigns";
            else if (typeof(T).Equals(typeof(Opportunity)))
                return "opportunities";
            else if (typeof(T).Equals(typeof(Form)))
                return "forms";
            else if (typeof(T).Equals(typeof(WebVisit)))
                return "webvisits";
            else if (typeof(T).Equals(typeof(SuppressedEmail)))
                return "suppressedemails";
            else if (typeof(T).Equals(typeof(SuppressedDomain)))
                return "suppresseddomains";
            else
                throw new NotImplementedException();
        }
        public abstract bool SetupIndex(int accountId);
        public abstract int ReIndexAll(IEnumerable<T> documents);
        public abstract int Index(IEnumerable<T> documents);
        public abstract int Index(T document);
        public abstract int Update(T document);
        public abstract int Remove(T document);
        public abstract int Remove(int id);
        public abstract int Remove(int documentId, int accountId);
        public abstract bool DeleteIndex();
        public abstract bool IndexExists();
    }
}
