using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.Domain.Tags;
using LandmarkIT.Enterprise.Utilities.Logging;
using Nest;
using LandmarkIT.Enterprise.Extensions;

namespace SmartTouch.CRM.SearchEngine.Indexing
{
    internal class TagIndexer<T> : Indexer<T, int> where T : Tag
    {
        public override bool SetupIndex(int accountId)
        {
            string indexName = "tags" + accountId;
            var client = ElasticClient(indexName);
            bool result = this.createIndex(indexName, client);
            if (result == false)
                throw new InvalidOperationException("Error occurred while creating index:" + indexName);
            return true;
        }

        public override int ReIndexAll(IEnumerable<T> documents)
        {
            if (!documents.IsAny())
                return 0;

            IList<int> accounts = documents.Select(c => c.AccountID).Distinct().ToList();

            int indexedDocuments = 0;
            foreach (int accountId in accounts)
                indexedDocuments = indexedDocuments + reIndexTags(documents.Where(c => c.AccountID == accountId), accountId);
            return indexedDocuments;
        }
        
        public override bool IndexExists()
        {
            string indexName = base.GetBaseIndexName();
            var client = ElasticClient(indexName);
            return client.IndexExists(i => i.Index(indexName)).Exists;
        }

        public override int Index(T document)
        {
            string indexName = "tags" + document.AccountID;
            var client = ElasticClient(indexName);

            int count = 0;

            if (client.IndexExists(i => i.Index(indexName)).Exists)
            {
                IList<Tag> tags = new List<Tag>() { document };

                //Below code is commented since we only index new tags.
                /*var documentById = client.Search(c => c.Types(types).Query(q => q.Ids(new List<string>() { tag.Id.ToString() })));
                if (documentById.Documents.Count() > 0)
                {
                    Tag tagById = documentById.Documents.FirstOrDefault();
                }                

                var deleteResult = client.DeleteMany(tags.OfType<Tag>(), new SimpleBulkParameters() { Refresh = true });
                string dr = deleteResult.ConnectionStatus.ToString();
                */

                var result = client.Bulk(b => b.IndexMany(tags, (bid, c) => bid.Type(typeof(Tag)).Index(indexName)));
                count = result.Items != null ? result.Items.Count() : 0;
                var refreshResult = client.Refresh(r => r.Index(indexName));
                string qe = refreshResult.ConnectionStatus.ToString();
                Logger.Current.Verbose(qe);
            }
            Logger.Current.Informational("Total Indexed Tags:" + count);
            return count;
        }


        public override int Index(IEnumerable<T> documents)
        {
            throw new NotImplementedException();
        }

        public override int Update(T document)
        {
            string indexName = "tags" + document.AccountID;
            var client = ElasticClient(indexName);

            int count = 0;
            if (client.IndexExists(indexName).Exists)
            {
                var updateResult = client.Update<Tag>(u => u
                        .Doc(document)
                        .Id(document.Id)
                        .Type(typeof(Tag))
                        .VersionType(Elasticsearch.Net.VersionType.Internal)
                        .Refresh(true));

                if (updateResult.IsValid)
                    count = 1;
                string qe = updateResult.ConnectionStatus.ToString();
                Logger.Current.Verbose(qe);
            }
            Logger.Current.Informational("Total Updated tags:" + count);
            return count;
        }

        public override int Remove(T document)
        {
            throw new NotImplementedException();
        }

        public override int Remove(int id)
        {
            throw new NotImplementedException();
        }

        public override int Remove(int documentId, int accountId)
        {
            string indexName = "tags" + accountId;
            var client = ElasticClient(indexName);

            int count = 0;

            if (client.IndexExists(i => i.Index(indexName)).Exists)
            {
                Tag tag = new Tag() { Id = documentId };
                var deleteResult = client.Bulk(b => b.DeleteMany(new List<Tag>() { tag }, (bid, c) => bid.Type(typeof(Tag)).Index(indexName)));

                string dr = deleteResult.ConnectionStatus.ToString();
                Logger.Current.Verbose(dr);
                var refreshResult = client.Refresh(r => r.Index(indexName));
                string qe = refreshResult.ConnectionStatus.ToString();
                Logger.Current.Verbose(qe);
            }
            Logger.Current.Informational("Total Removed Tags:" + count);
            return count;
        }

        public override bool DeleteIndex()
        {
            string indexName = "tags";
            var client = ElasticClient(indexName);
            // delete index if exists at startup
            if (client.IndexExists(i => i.Index(indexName)).Exists)
            {
                var response = client.DeleteIndex(i => i.Index(indexName));
                Logger.Current.Verbose("Deleted index." + response.ConnectionStatus.ToString());
                return response.Acknowledged;
            }
            return false;
        }

        bool createIndex(string indexName, ElasticClient client)
        {
            if (client.IndexExists(i => i.Index(indexName)).Exists)
            {
                var response = client.DeleteIndex(i => i.Index(indexName));
                Logger.Current.Verbose("Deleted index." + response.ConnectionStatus.ToString());
            }

            var word_delimiter_filter = new WordDelimiterTokenFilter() { CatenateAll = true, GenerateNumberParts = false, SplitOnCaseChange = false, GenerateWordParts = false, SplitOnNumerics = false, PreserveOriginal = true };
            var customAnalyzer = new CustomAnalyzer()
            {
                Filter = new List<String>() { "lowercase", "word_delimiter_filter" },
                Tokenizer = "whitespace"
            };
            var createResult = client.CreateIndex(indexName, index => index.Analysis(a => a.TokenFilters(t => t.Add("word_delimiter_filter", word_delimiter_filter)).Analyzers(an => an.Add("custom", customAnalyzer)))
               .AddMapping<Tag>(tmd => MapTagCompletionFields(tmd)));

            return createResult.ConnectionStatus.Success;
        }

        private int reIndexTags(IEnumerable<T> documents, int accountId)
        {
            string indexName = "tags" + accountId;
            var client = ElasticClient(indexName);
            this.createIndex(indexName, client);

            int indexedCount = 0;
            if (documents.IsAny())
            {
                var result = client.Bulk(b => b.IndexMany(documents, (bid, c) => bid.Type(typeof(Tag)).Index(indexName)));
                Logger.Current.Verbose(result.ConnectionStatus.ToString());
                indexedCount = result.Items != null ? result.Items.Count() : 0;
            }
            Logger.Current.Informational("Total ReIndexed Tags:" + indexedCount);
            return indexedCount;
        }

        private PutMappingDescriptor<TContact> MapTagCompletionFields<TContact>(
           PutMappingDescriptor<TContact> tmd)
           where TContact : Tag
        {
            return tmd.Properties(props => props
                 .Completion(s => s
                      .Name(p => p.TagNameAutoComplete)
                      .IndexAnalyzer("custom")
                      .SearchAnalyzer("custom")
                      .MaxInputLength(75)
                      .Payloads()
                      .PreservePositionIncrements()
                      .PreserveSeparators()
                 )
             );
        }
    }    
}
