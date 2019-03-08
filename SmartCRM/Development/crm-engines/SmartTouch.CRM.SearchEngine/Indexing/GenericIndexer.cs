using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DA = SmartTouch.CRM.Domain.Actions;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Tags;
using Elasticsearch.Net;
using Nest;
using LandmarkIT.Enterprise.Utilities.Logging;
using LandmarkIT.Enterprise.Extensions;

namespace SmartTouch.CRM.SearchEngine.Indexing
{
    internal class GenericIndexer<T> : Indexer<T, int> where T : EntityBase<int>
    {
        public override bool SetupIndex(int accountId)
        {
            string indexName = base.GetBaseIndexName();
            var client = ElasticClient(indexName);

            if (client.IndexExists(indexName).Exists)
                client.DeleteIndex(new DeleteIndexRequest(indexName));
            var createResult = client.CreateIndex(indexName, index => index);            
            if (!createResult.ConnectionStatus.Success)
                throw new InvalidOperationException("Error occurred while creating index:" + indexName);

            return true;
        }

        public override bool IndexExists()
        {
            string indexName = base.GetBaseIndexName();
            var client = ElasticClient(indexName);
            return client.IndexExists(indexName).Exists;
        }

        public override int ReIndexAll(IEnumerable<T> documents)
        {
            string indexName = base.GetBaseIndexName();
            var client = ElasticClient(indexName);

            int indexedCount = 0;
            if (documents.IsAny() && client.IndexExists(indexName).Exists)
            {
                var result = client.Bulk(b => b.IndexMany(documents, (bid, c) => bid.Type(typeof(T)).Index(indexName).VersionType(Elasticsearch.Net.VersionType.Internal)));

                Logger.Current.Verbose(result.ConnectionStatus.ToString());
                indexedCount = result.Items != null ? result.Items.Count() : 0;
            }

            Logger.Current.Informational("Total ReIndexed documents:" + indexedCount);
            return indexedCount;
        }

        public override int Index(IEnumerable<T> documents)
        {
            return indexDocuments(documents);
        }

        public override int Index(T document)
        {
            IList<T> actions = new List<T>() { document };
            return indexDocuments(actions);
        }

        int indexDocuments(IEnumerable<T> documents)
        {
            string indexName = base.GetBaseIndexName();
            var client = ElasticClient(indexName);

            int count = 0;

            if (client.IndexExists(indexName).Exists)
            {
                if (documents != null && documents.Count() > 1)
                {
                    var result = client.Bulk(b => b.IndexMany(documents, (bid, c) => bid.Type(typeof(T)).Index(indexName).VersionType(Elasticsearch.Net.VersionType.Internal)));
                    count = result.Items != null ? result.Items.Count() : 0;
                }
                else
                {
                    var result = client.Index<T>(documents.FirstOrDefault(), f => f.Type(typeof(T)).Index(indexName).VersionType(Elasticsearch.Net.VersionType.Internal));
                    count = 1;
                }
                var refreshResult = client.Refresh(r => r.Index(indexName));
                string qe = refreshResult.ConnectionStatus.ToString();
                Logger.Current.Verbose(qe);
            }
            Logger.Current.Informational("Total Indexed documents:" + count);
            return count;
        }

        public override int Update(T document)
        {
            string indexName = base.GetBaseIndexName();
            var client = ElasticClient(indexName);

            int count = 0;
            if (client.IndexExists(indexName).Exists)
            {
                var updateResult = client.Update<T>(u => u
                       .Doc(document)
                       .Id(document.Id)
                       .Type(typeof(T))
                       .VersionType(Elasticsearch.Net.VersionType.Internal)
                       .Refresh(true));

                if (updateResult.IsValid)
                    count = 1;
                string qe = updateResult.ConnectionStatus.ToString();
                Logger.Current.Verbose(qe);
            }
            Logger.Current.Informational("Total Updated documents:" + count);
            return count;
        }

        public override int Remove(T document)
        {
            throw new NotImplementedException();
        }

        public override int Remove(int id)
        {
            string indexName = base.GetBaseIndexName();
            var client = ElasticClient(indexName);

            int count = 0;

            if (client.IndexExists(indexName).Exists)
            {

                var deleteResult = client.Delete<T>(d => d.Id(id.ToString()).Index(indexName).Type(typeof(T)));
                //var deleteResult = client.Bulk(b => b.DeleteMany(new List<long>() { (long)id }, (bid, c) => bid.Type(typeof(T)).Index(indexName)));

                string dr = deleteResult.ConnectionStatus.ToString();
                Logger.Current.Verbose(dr);
                var refreshResult = client.Refresh(r => r.Index(indexName));
                if (refreshResult != null)
                    Logger.Current.Informational("Document is refreshed");
            }
            Logger.Current.Informational("Total Removed documents:" + count);
            return count;
        }

        public override int Remove(int documentId, int accountId)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteIndex()
        {
            throw new NotImplementedException();
        }
    }
}
