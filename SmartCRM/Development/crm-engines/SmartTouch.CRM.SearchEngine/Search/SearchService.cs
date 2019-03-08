using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Linq.Expressions;

using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.Search;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Opportunities;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Domain.ValueObjects;
using LandmarkIT.Enterprise.Utilities.Logging;

using Newtonsoft.Json;
using Nest;
using Elasticsearch.Net;

namespace SmartTouch.CRM.SearchEngine.Search
{
    public class SearchService<T> : ISearchService<T> where T : class
    {
        public SearchResult<T> Search(string q, SearchParameters searchParameters)
        {
            if (typeof(T).Equals(typeof(Contact)) && searchParameters != null)
                return new ContactSearch<T>(searchParameters.AccountId)
                    .SearchContacts(q, searchParameters);
            else
                return new SearchBase<T>(searchParameters.AccountId).Search(q, searchParameters);
        }

        public SearchResult<T> Search(string q, Expression<Func<T, bool>> filter, SearchParameters searchParameters)
        {
            if (typeof(T).Equals(typeof(Contact)) && searchParameters != null)
                return new ContactSearch<T>(searchParameters.AccountId)
                    .SearchContacts(q, filter, searchParameters);
            else
                return new SearchBase<T>(searchParameters.AccountId).Search(q, filter, searchParameters);
        }

        public SearchResult<T> SearchCampaigns(string q, SearchParameters searchParameters)
        {
            if (typeof(T).Equals(typeof(Campaign)) && searchParameters != null)
                return new ContactSearch<T>(searchParameters.AccountId)
                    .SearchCampaigns(q, searchParameters);
            else
                return new SearchBase<T>(searchParameters.AccountId).Search(q, searchParameters);
        }

        public SearchResult<Suggestion> AutoCompleteField(string q, SearchParameters searchParameters)
        {
            if (typeof(T).Equals(typeof(Contact)) && searchParameters != null)
                return new ContactSearch<T>(searchParameters.AccountId)
                    .AutoCompleteField(q, searchParameters);
            else if (typeof(T).Equals(typeof(Tag)) && searchParameters != null)
                return new TagSearch<T>(searchParameters.AccountId)
                    .AutoCompleteField(q, searchParameters);
            else
                return new SearchBase<T>(searchParameters.AccountId).AutoCompleteField(q, searchParameters);
        }

        public SearchResult<T> DuplicateSearch(T t, SearchParameters searchParameters)
        {
            if (typeof(T).Equals(typeof(Contact)))
                return new ContactSearch<T>(searchParameters.AccountId)
                    .DuplicateSearch(t, searchParameters);
            else
                throw new NotImplementedException("Duplicate search for this Type is not implented:" + typeof(T).ToString());
        }

        public SearchResult<Suggestion> QuickSearch(string q, SearchParameters searchParameters)
        {
            return new SearchBase<T>(searchParameters.AccountId).QuickSearch(q, searchParameters);
        }

        public Task<SearchResult<T>> AdvancedSearchAsync(string q, SearchDefinition searchDefinition, SearchParameters searchParameters)
        {
            return new ContactSearch<T>(searchDefinition.AccountID.Value).AdvancedSearchAsync(q, searchDefinition, searchParameters);
        }

        public Task<SearchResult<T>> AdvancedSearchExportAsync(string q, SearchDefinition searchDefinition, SearchParameters searchParameters)
        {
            return new ContactSearch<T>(searchDefinition.AccountID.Value).AdvancedSearchExportAsync(q, searchDefinition, searchParameters);
        }

        public IDictionary<DateTime, long> GetContactsAggregationByDate(SearchDefinition searchDefinition, SearchParameters parameters)
        {
            return new ContactSearch<T>(searchDefinition.AccountID.Value).GetContactsAggregationByDate(searchDefinition, parameters);
        }

        public IDictionary<int, long> GetTopLeadSources(SearchDefinition searchDefinition, SearchParameters parameters)
        {
            return new ContactSearch<T>(searchDefinition.AccountID.Value).GetTopLeadSources(searchDefinition, parameters);
        }

        public SavedSearchActiveContacts GetAggregationBySavedSearch(SearchDefinition searchDefinition, SearchParameters parameters)
        {
            return new ContactSearch<T>(searchDefinition.AccountID.Value).GetAggregationBySavedSearch(searchDefinition, parameters);
        }

        public bool IsOwnedBy(int documentId, int? userId, int accountId)
        {
            if (typeof(T).Equals(typeof(Contact)))
                return new ContactSearch<T>(accountId).IsContactOwnedBy(documentId, userId);
            else if (typeof(T).Equals(typeof(Campaign)) || typeof(T).Equals(typeof(Opportunity)) || typeof(T).Equals(typeof(Form)))
                return new SearchBase<T>(accountId).IsDocumentCreatedBy(documentId, userId);
            else
                throw new NotImplementedException("This type of document is not indexed:" + typeof(T).ToString());
        }

        public bool IsCreatedBy(int documentId, int? userId, int accountId)
        {
            if (typeof(T).Equals(typeof(Contact)))
                return new ContactSearch<T>(accountId).IsContactOwnedBy(documentId, userId);
            else if (typeof(T).Equals(typeof(Campaign)) || typeof(T).Equals(typeof(Opportunity)) || typeof(T).Equals(typeof(Form)))
                return new SearchBase<T>(accountId).IsDocumentCreatedBy(documentId, userId);
            else
                throw new NotImplementedException("This type of document is not indexed:" + typeof(T).ToString());
        }

        public bool SaveQuery(SearchDefinition searchDefinition, SearchParameters searchParameters)
        {
            if (typeof(T).Equals(typeof(Contact)))
                return new ContactSearch<T>(searchParameters.AccountId).SaveQuery(searchDefinition, searchParameters);
            else
                throw new NotImplementedException("This type of document is not indexed:" + typeof(T).ToString());
        }

        public int SaveQueries(IEnumerable<SearchDefinition> searchDefinitions, SearchParameters searchParameters, int accountId)
        {
            if (typeof(T).Equals(typeof(Contact)))
                return new ContactSearch<T>(searchParameters.AccountId).SaveQueries(searchDefinitions, searchParameters, accountId);
            else
                throw new NotImplementedException("This type of document is not indexed:" + typeof(T).ToString());
        }

        public bool RemoveQuery(short searchDefinitionId, SearchParameters searchParameters)
        {
            if (typeof(T).Equals(typeof(Contact)))
                return new ContactSearch<T>(searchParameters.AccountId).RemoveQuery(searchDefinitionId);
            else
                throw new NotImplementedException("This type of document is not indexed:" + typeof(T).ToString());
        }

        public IEnumerable<QueryMatch> FindMatchingQueries(IEnumerable<T> documents, SearchParameters searchParameters)
        {
            if (typeof(T).Equals(typeof(Contact)))
                return new ContactSearch<T>(searchParameters.AccountId).FindMatchingQueries(documents);
            else
                throw new NotImplementedException("This type of document is not indexed:" + typeof(T).ToString());
        }

        public IEnumerable<string> CheckSuppressionList(SearchParameters p)
        {
            return new SearchBase<T>(p.AccountId).CheckSuppressionList(p);
        }

        public IEnumerable<T> SearchSuppressionList(string queryString, SearchParameters p)
        {
            return new SearchBase<T>(p.AccountId).SearchSuppressionList(queryString, p);
        }
    }
}
