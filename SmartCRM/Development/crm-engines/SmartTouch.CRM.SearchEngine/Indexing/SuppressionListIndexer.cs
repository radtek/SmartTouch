using LandmarkIT.Enterprise.Utilities.Logging;
using LandmarkIT.Enterprise.Extensions;
using Nest;
using SmartTouch.CRM.Domain.SuppressedEmails;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.SearchEngine.Indexing
{
    internal class SuppressionListIndexer<T> : Indexer<T, int> where T : SuppressionList
    {
        public override bool SetupIndex(int accountId)
        {
            string indexName = string.Empty;
            if (typeof(T).Equals(typeof(SuppressedEmail)))
                indexName = "suppressedemails" + accountId;
            else if (typeof(T).Equals(typeof(SuppressedDomain)))
                indexName = "suppresseddomains" + accountId;

            var client = ElasticClient(indexName);

            bool result = this.createIndex(indexName, client);
            if (result == false)
                throw new InvalidOperationException("Error occurred while creating index:" + indexName);
            return true;
        }

        public override int ReIndexAll(IEnumerable<T> documents)
        {
            IList<int> accounts = documents.Select(c => c.AccountID).Distinct().ToList();
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            int indexedDocuments = 0;
            foreach (int accountId in accounts)
            {
                var count = reIndexSuppressionList(documents.Where(c => c.AccountID == accountId), accountId);
                indexedDocuments = indexedDocuments + count;
                Console.WriteLine("Indexed " + count + " of account " + accountId + " at " + DateTime.Now.ToString());
            }
            sw.Stop();
            var timeelapsed = sw.Elapsed;
            Logger.Current.Informational("Time elapsed for reindexing suppressedemails to elastic:" + timeelapsed);
            return indexedDocuments;
        }

        public override int Index(IEnumerable<T> documents)
        {
            IList<int> accounts = documents.Select(c => c.AccountID).Distinct().ToList();
            int indexedDocuments = 0;

            foreach (int accountId in accounts)
                indexedDocuments = indexedDocuments + reIndexSuppressionList(documents.Where(d => d.AccountID == accountId), accountId);

            return indexedDocuments;
        }

        public override int Index(T document)
        {
            string indexName = string.Empty;
            if (typeof(T).Equals(typeof(SuppressedEmail)))
                indexName = "suppressedemails" + document.AccountID;
            else if (typeof(T).Equals(typeof(SuppressedDomain)))
                indexName = "suppresseddomains" + document.AccountID;
            
            var client = ElasticClient(indexName);

            int count = 0;
            if (client.IndexExists(indexName).Exists)
                this.Index(new List<T>() { document });
            Logger.Current.Informational("Total Indexed suppressed emails:" + count);
            return count;
        }

        public override int Update(T document)
        {
            throw new NotImplementedException();
        }

        public override int Remove(T document)
        {
            return removeSuppressedEmail(document.Id, document.AccountID);
        }

        public override int Remove(int id)
        {
            throw new NotImplementedException();
        }

        int removeSuppressedEmail(int Id, int accountId)
        {
            string indexName = string.Empty;
            if (typeof(T).Equals(typeof(SuppressedEmail)))
                indexName = "suppressedemails" + accountId;
            else if (typeof(T).Equals(typeof(SuppressedDomain)))
                indexName = "suppresseddomains" + accountId;

            var client = ElasticClient(indexName);
            int count = 0;

            if (client.IndexExists(i => i.Index(indexName)).Exists)
            {
                T email = null;
                var documentById = client.Search<T>(c => c.Query(q => q.Ids(new List<string>() { Id.ToString() })));
                if (documentById != null && documentById.Documents.IsAny())
                {
                    email = documentById.Documents.FirstOrDefault();
                    IList<T> emails = new List<T>() { email };
                    var deleteResult = client.Bulk(b => b.DeleteMany(emails, (bid, c) => bid.Index(indexName)));
                    string dr = deleteResult.ConnectionStatus.ToString();
                    count = deleteResult.Items.Count();
                }
            }
            Logger.Current.Informational("Total Removed suppressed emails:" + count);
            return count;
        }

        public override int Remove(int documentId, int accountId)
        {
            return removeSuppressedEmail(documentId, accountId);
        }

        public override bool DeleteIndex()
        {
            string indexName = string.Empty;
            if (typeof(T).Equals(typeof(SuppressedEmail)))
                indexName = "suppressedemails" + 1;
            else if (typeof(T).Equals(typeof(SuppressedDomain)))
                indexName = "suppresseddomains" + 1;
            var client = ElasticClient(indexName);
            var response = client.DeleteIndex(i => i.Index(indexName));
            return response.ConnectionStatus.Success;
        }

        public override bool IndexExists()
        {
            string indexName = base.GetBaseIndexName();
            var client = ElasticClient(indexName);
            return client.IndexExists(indexName).Exists;
        }

        bool createIndex(string indexName, ElasticClient client)
        {
            if (client.IndexExists(indexName).Exists)
                client.DeleteIndex(new DeleteIndexRequest(indexName));

            var customAnalyzer = new CustomAnalyzer
            {
                Filter = new List<string> { "standard", "lowercase", "stop" },
                Tokenizer = "uax_url_email"
            };

            var duplicateCheckAnalyzer = new CustomAnalyzer
            {
                Filter = new List<string> { "standard", "lowercase" },
                Tokenizer = "keyword"
            };
            var createResult = client.CreateIndex(indexName, index => index
                .Analysis(a => a
                .Analyzers(an => an
                    .Add("custom", customAnalyzer)
                    .Add("duplicateCheckAnalyzer", duplicateCheckAnalyzer))).NumberOfShards(5).NumberOfReplicas(1)
                .AddMapping<T>(pmd => MapSuppressionListFields<T>(pmd)));

            string qe = createResult.ConnectionStatus.ToString();
            Logger.Current.Verbose(qe);
            var refreshResult = client.Refresh(r => r.Index(indexName));
            if (refreshResult != null)
                Logger.Current.Informational("Index is refreshed");
            return createResult.ConnectionStatus.Success;
        }

        private PutMappingDescriptor<T> MapSuppressionListFields<T>(PutMappingDescriptor<T> pmd)
            where T : SuppressionList
        {
            if (typeof(T).Equals(typeof(SuppressedEmail)))
                return pmd
                    .Properties(props => props
                         .String(s => s.Name("email").Index(FieldIndexOption.NotAnalyzed).Store(true).IndexAnalyzer("duplicateCheckAnalyzer").SearchAnalyzer("duplicateCheckAnalyzer")))
                    .TimestampField(t => t
                        .Enabled(true));
            else
                return pmd
                .Properties(props => props
                     .String(s => s.Name("domain").Index(FieldIndexOption.NotAnalyzed).Store(true).IndexAnalyzer("duplicateCheckAnalyzer").SearchAnalyzer("duplicateCheckAnalyzer")))
                .TimestampField(t => t
                    .Enabled(true));
        }

        private int reIndexSuppressionList(IEnumerable<T> documents, int accountId)
        {
            string indexName = string.Empty;
            if (typeof(T).Equals(typeof(SuppressedEmail)))
                indexName = "suppressedemails" + accountId;
            else if (typeof(T).Equals(typeof(SuppressedDomain)))
                indexName = "suppresseddomains" + accountId;

            var client = ElasticClient(indexName);
            int suppressedEmailsCount = 0;
            try
            {
                var result = client.Bulk(b => b.IndexMany(documents, (bid, c) => bid.Type(typeof(T)).Index(indexName)
                    .VersionType(Elasticsearch.Net.VersionType.Internal)));

                string qu = result.ConnectionStatus.ToString();
                suppressedEmailsCount = result.Items != null ? result.Items.Count() : 0;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while re-indexing suppressed emails for account : " + accountId, ex);
            }

            int indexedCount = suppressedEmailsCount;
            Logger.Current.Verbose("Total Reindexed Suppressed Emails:" + indexedCount + ", Account ID :" + accountId);
            return indexedCount;
        }
    }
}
