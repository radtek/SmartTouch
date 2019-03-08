using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;

using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Tags;
using DA = SmartTouch.CRM.Domain.Actions;
using SmartTouch.CRM.Domain.Tours;
using SmartTouch.CRM.Domain.Notes;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Domain.Opportunities;

using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Infrastructure.Domain;
using LandmarkIT.Enterprise.Utilities.Logging;

using Nest;
using SmartTouch.CRM.Domain.SuppressedEmails;

namespace SmartTouch.CRM.SearchEngine.Indexing
{
    public class IndexingService : IIndexingService
    {
        string uri;
        public IndexingService()
        {
            uri = ConfigurationManager.AppSettings["ELASTICSEARCH_INSTANCE"];
        }

        # region CONTACTS
        public int ReIndexAll(IEnumerable<Contact> documents)
        {
            Indexer<Contact, int> indxr = new ContactIndexer<Contact>();
            return indxr.ReIndexAll(documents);
        }

        public int IndexContact(Contact contact)
        {
            Indexer<Contact, int> indxr = IndexerFactory.GetIndexer<Contact, int>();
            return indxr.Index(contact);
        }

        public int UpdateContact(Contact contact)
        {
            Indexer<Contact, int> indxr = IndexerFactory.GetIndexer<Contact, int>();
            return indxr.Index(contact);
        }

        public int UpdatePartialContact(Contact contact)
        {
            Indexer<Contact, int> indxr = IndexerFactory.GetIndexer<Contact, int>();
            return indxr.Update(contact);
        }

        public bool DeleteContactIndex()
        {
            Indexer<Contact, int> indxr = IndexerFactory.GetIndexer<Contact, int>();
            return indxr.DeleteIndex();
        }

        public int RemoveContact(int contactId, int accountId)
        {
            Indexer<Contact, int> indxr = IndexerFactory.GetIndexer<Contact, int>();
            return indxr.Remove(contactId, accountId);
        }

        public int IndexContacts(IEnumerable<Contact> documents)
        {
            Indexer<Contact, int> indxr = IndexerFactory.GetIndexer<Contact, int>();
            return indxr.Index(documents);
        }
        #endregion

        # region CAMPAIGNS
        public int ReIndexAll(IEnumerable<Campaign> documents)
        {
            Indexer<Campaign, int> indxr = new CampaignIndexer<Campaign>();
            return indxr.ReIndexAll(documents);
        }

        public int IndexCampaign(Campaign campaign)
        {
            Indexer<Campaign, int> indxr = IndexerFactory.GetIndexer<Campaign, int>();
            return indxr.Index(campaign);
        }

        public int UpdateCampaign(Campaign campaign)
        {
            Indexer<Campaign, int> indxr = IndexerFactory.GetIndexer<Campaign, int>();
            return indxr.Index(campaign);
        }

        public bool DeleteCampaignIndex()
        {
            Indexer<Campaign, int> indxr = IndexerFactory.GetIndexer<Campaign, int>();
            return indxr.DeleteIndex();
        }

        public int RemoveCampaign(int campaignId, int accountId)
        {
            Indexer<Campaign, int> indxr = IndexerFactory.GetIndexer<Campaign, int>();
            return indxr.Remove(campaignId, accountId);
        }

        public int IndexCampaigns(IEnumerable<Campaign> documents)
        {
            Indexer<Campaign, int> indxr = IndexerFactory.GetIndexer<Campaign, int>();
            return indxr.Index(documents);
        }
        
        public int UpdateCampaigns(IEnumerable<Campaign> documents)
        {
            Indexer<Campaign, int> indxr = IndexerFactory.GetIndexer<Campaign, int>();
            return indxr.Index(documents);
        }
        #endregion

        public int RemoveTag(int tagId, int accountId)
        {
            Indexer<Tag, int> indxr = IndexerFactory.GetIndexer<Tag, int>();
            return indxr.Remove(tagId, accountId);
        }

        public int ReIndexAllTags(IEnumerable<Tag> documents)
        {
            Indexer<Tag, int> indxr = IndexerFactory.GetIndexer<Tag, int>();
            return indxr.ReIndexAll(documents);
        }

        public int IndexTag(Tag tag)
        {
            Indexer<Tag, int> indxr = IndexerFactory.GetIndexer<Tag, int>();
            return indxr.Index(tag);
        }

        public int UpdateTag(Tag tag)
        {
            Indexer<Tag, int> indxr = IndexerFactory.GetIndexer<Tag, int>();
            return indxr.Update(tag);
        }

        #region SuppressionList
        public bool SetupSuppressionListIndex<T>(int accountId) where T : SuppressionList
        {
            var emailIndexer = new SuppressionListIndexer<T>();
            emailIndexer.SetupIndex(accountId);
            Console.WriteLine("Created suppressed emails index for account id : " + accountId);
            return true;
        }

        public int RemoveSuppressionDocument<T>(int suppressedEmailId, int accountId) where T : SuppressionList
        {
            Indexer<T, int> indxr = IndexerFactory.GetIndexer<T, int>();
            return indxr.Remove(suppressedEmailId, accountId);
        }

        public int ReIndexAllSuppressionList<T>(IEnumerable<T> documents) where T : SuppressionList
        {
            Indexer<T, int> indxr = new SuppressionListIndexer<T>();
            return indxr.ReIndexAll(documents);
        }

        public int IndexSuppressedEmail(SuppressedEmail doc)
        {
            Indexer<SuppressedEmail, int> indxr = IndexerFactory.GetIndexer<SuppressedEmail, int>();
            return indxr.Index(doc);
        }

        #endregion

        public int ReIndexAllActions(IEnumerable<DA.Action> documents)
        {
            Indexer<DA.Action, int> indxr = IndexerFactory.GetIndexer<DA.Action, int>();
            return indxr.ReIndexAll(documents);
        }

        public IList<int> accountsIndexed = new List<int>();

        public bool staticEntitesIndexed = false;

        public bool SetupDynamicIndices(int accountId)
        {
            SetupContactIndex(accountId);
            SetupTagIndex(accountId);
            SetupCampaignIndex(accountId);
            SetupSuppressionListIndex<SuppressedEmail>(accountId);
            SetupSuppressionListIndex<SuppressedDomain>(accountId);
            return true;
        }

        public bool SetupContactIndex(int accountId)
        {
            var contactIndxr = new ContactIndexer<Contact>();
            contactIndxr.SetupIndex(accountId);
            Console.WriteLine("Created contacts index for account id:" + accountId);
            return true;
        }

        public bool SetupCampaignIndex(int accountId)
        {
            var campaignIndxr = new CampaignIndexer<Campaign>();
            campaignIndxr.SetupIndex(accountId);
            Console.WriteLine("Created campaigns index for account id:" + accountId);
            return true;
        }

        public bool SetupTagIndex(int accountId)
        {
            var tagIndexer = new TagIndexer<Tag>();
            tagIndexer.SetupIndex(accountId);
            accountsIndexed.Add(accountId);
            Console.WriteLine("Created tags index for account id:" + accountId);
            return true;
        }



        public bool SetupStaticIndice<T>() where T : EntityBase<int>
        {
            var indxr = new GenericIndexer<T>();
            return indxr.SetupIndex(0);
        }
        public bool SetupStaticIndices()
        {
            //static indices do not need accountid.
            var formIndexer = new GenericIndexer<Form>();
            formIndexer.SetupIndex(0);

            var formSubmissionIndexer = new GenericIndexer<FormSubmission>();
            formSubmissionIndexer.SetupIndex(0);

            var campaignIndexer = new GenericIndexer<Campaign>();
            campaignIndexer.SetupIndex(0);

            var opportunityIndexer = new GenericIndexer<Opportunity>();
            opportunityIndexer.SetupIndex(0);

            staticEntitesIndexed = true;
            Console.WriteLine("Created indexes for forms, campaigns and opportunities and form-submissions");

            return true;
        }

        public int ReIndexAll<T>(IEnumerable<T> documents) where T : EntityBase<int>
        {
            var indxr = new GenericIndexer<T>();
            return indxr.ReIndexAll(documents);
        }

        public int Index<T>(T document) where T : EntityBase<int>
        {
            var indxr = new GenericIndexer<T>();
            return indxr.Index(document);
        }

        public int Update<T>(T document) where T : EntityBase<int>
        {
            var indxr = new GenericIndexer<T>();
            return indxr.Index(document);
        }

        public int UpdatePartial<T>(T document) where T : EntityBase<int>
        {
            var indxr = new GenericIndexer<T>();
            return indxr.Update(document);
        }

        public int UpdateMany<T>(IEnumerable<T> documents) where T : EntityBase<int>
        {
            var indxr = new GenericIndexer<T>();
            return indxr.ReIndexAll(documents);
        }

        public int Remove<T>(int id) where T : EntityBase<int>
        {
            var indxr = new GenericIndexer<T>();
            return indxr.Remove(id);
        }

        public int Remove<T>(T document) where T : EntityBase<int>
        {
            throw new NotImplementedException();
        }
    }
}
