using LandmarkIT.Enterprise.Utilities.Logging;
using Nest;
using SmartTouch.CRM.Domain.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LandmarkIT.Enterprise.Extensions;

namespace SmartTouch.CRM.SearchEngine.Indexing
{
    internal class CampaignIndexer<T> : Indexer<T, int> where T : Campaign
    {
        public override bool SetupIndex(int accountId)
        {
            string indexName = "campaigns" + accountId;
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
                var count = reIndexCampaigns(documents.Where(c => c.AccountID == accountId), accountId);
                indexedDocuments = indexedDocuments + count;
                Console.WriteLine("Indexed " + count + " of account " + accountId + " at " + DateTime.Now.ToString());
            }
            sw.Stop();
            var timeelapsed = sw.Elapsed;
            Logger.Current.Informational("Time elapsed for reindexing contacts to elastic:" + timeelapsed);
            return indexedDocuments;
        }

        public override bool IndexExists()
        {
            string indexName = base.GetBaseIndexName();
            var client = ElasticClient(indexName);
            return client.IndexExists(indexName).Exists;
        }

        public override bool DeleteIndex()
        {
            string indexName = "campaigns";
            var client = ElasticClient(indexName);
            var response = client.DeleteIndex(i => i.Index(indexName));
            return response.ConnectionStatus.Success;
        }

        public override int Index(IEnumerable<T> documents)
        {
            IList<int> accounts = documents.Select(c => c.AccountID).Distinct().ToList();
            int indexedDocuments = 0;

            foreach (int accountId in accounts)
                indexedDocuments = indexedDocuments + indexCampaigns(documents.Where(d => d.AccountID == accountId), accountId);

            return indexedDocuments;
        }

        bool createIndex(string indexName, ElasticClient client)
        {
            if (client.IndexExists(i => i.Index(indexName)).Exists)
            {
                var response = client.DeleteIndex(i => i.Index(indexName));
                Logger.Current.Verbose("Deleted index." + response.ConnectionStatus.ToString());
            }

            //var word_delimiter_filter = new WordDelimiterTokenFilter() { CatenateAll = true, GenerateNumberParts = false, SplitOnCaseChange = false, GenerateWordParts = false, SplitOnNumerics = false, PreserveOriginal = true };
            //var customAnalyzer = new CustomAnalyzer()
            //{
            //    Filter = new List<String>() { "lowercase", "word_delimiter_filter" },
            //    Tokenizer = "whitespace"
            //};
            var createResult = client.CreateIndex(indexName);

            return createResult.ConnectionStatus.Success;
        }

        private int reIndexCampaigns(IEnumerable<Campaign> documents, int accountId)
        {
            string indexName = "campaigns" + accountId;
            var client = ElasticClient(indexName);

            int campaignCount = 0;

            Console.WriteLine("Indexing campaigns from " + documents.Last().Id + " -- " + documents.First().Id);
            var resultCompany = client.Bulk(b => b.IndexMany(documents, (bid, c) => bid.Type(typeof(Campaign)).Index(indexName)));
            campaignCount = resultCompany.Items != null ? resultCompany.Items.Count() : 0;

            int indexedCount = campaignCount;
            Logger.Current.Verbose("Total Reindexed Campaigns:" + indexedCount + ", Account ID :" + accountId);

            return indexedCount;
        }

        private int indexCampaigns(IEnumerable<T> documents, int accountId)
        {
            string indexName = "campaigns" + accountId;
            var client = ElasticClient(indexName);
            if (!client.IndexExists(indexName).Exists)
                this.createIndex(indexName, client);
            int campaignCount = 0;



            var task_resultCampaign = client.BulkAsync(b => b.IndexMany(documents, (bid, c) => bid.Type(typeof(Campaign)).Index(indexName)));
            Task.WaitAll(task_resultCampaign);
            campaignCount = task_resultCampaign.Result.Items != null ? task_resultCampaign.Result.Items.Count() : 0;


            int indexedCount = campaignCount;
            Logger.Current.Verbose("Total Reindexed Campaigns:" + indexedCount);
            return indexedCount;
        }

        public override int Index(T document)
        {
            string indexName = "campaigns" + document.AccountID;
            var client = ElasticClient(indexName);

            int count = 0;
            if (client.IndexExists(indexName).Exists)
            {
                IList<Campaign> campaigns = new List<Campaign>() { document };

                var deleteResult = client.Bulk(b => b.DeleteMany(campaigns, (bid, c) => bid.Type(typeof(Campaign)).Index(indexName)));
                string dr = deleteResult.ConnectionStatus.ToString();
                //Logger.Current.Verbose(dr);
                var result = client.Bulk(b => b.IndexMany(campaigns, (bid, c) => bid.Type(typeof(Campaign)).Index(indexName)
                    .VersionType(Elasticsearch.Net.VersionType.Internal)));
                string qu = result.ConnectionStatus.ToString();
                //Logger.Current.Verbose(qu);
                count = result.Items != null ? result.Items.Count() : 0;
                var refreshResult = client.Refresh(r => r.Index(indexName));
                string qe = refreshResult.ConnectionStatus.ToString();
                //Logger.Current.Verbose(qe);
                if (campaigns != null && campaigns.Any()) Logger.Current.Verbose(string.Format("Indexed Contact : {0}", campaigns.First().Id));
            }
            Logger.Current.Informational("Total Indexed campaigns:" + count);
            return count;
        }

        public override int Update(T document)
        {
            throw new NotImplementedException();
        }

        public override int Remove(T document)
        {
            return removeCampaign(document.Id, document.AccountID);
        }

        public override int Remove(int id)
        {
            throw new NotImplementedException();
        }

        public override int Remove(int documentId, int accountId)
        {
            return removeCampaign(documentId, accountId);
        }

        int removeCampaign(int id, int accountId)
        {
            string indexName = "campaigns" + accountId;
            var client = ElasticClient(indexName);
            int count = 0;

            if (client.IndexExists(i => i.Index(indexName)).Exists)
            {
                Campaign campaign = null;
                var documentById = client.Search<Campaign>(c => c.Types(typeof(Campaign)).Query(q => q.Ids(new List<string>() { id.ToString() })));
                if (documentById != null && documentById.Documents.IsAny())
                {
                    campaign = documentById.Documents.FirstOrDefault();

                    IList<Campaign> campaigns = new List<Campaign>() { campaign };
                    if (campaign.GetType().Equals(typeof(Campaign)))
                    {
                        var deleteResult = client.Bulk(b => b.DeleteMany(campaigns, (bid, c) => bid.Type(typeof(Campaign)).Index(indexName)));
                        string dr = deleteResult.ConnectionStatus.ToString();
                        //Logger.Current.Verbose(dr);
                        count = deleteResult.Items.Count();
                    }
                }
            }
            Logger.Current.Informational("Total Removed Campaigns:" + count);
            return count;
        }

    }
}
