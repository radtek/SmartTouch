using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Tags;
using DA = SmartTouch.CRM.Domain.Actions;
using SmartTouch.CRM.Domain.Tours;
using SmartTouch.CRM.Domain.Notes;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.SuppressedEmails;

namespace SmartTouch.CRM.SearchEngine.Indexing
{
    public interface IIndexingService
    {
        int ReIndexAll(IEnumerable<Contact> contacts);
        int IndexContacts(IEnumerable<Contact> contacts);
        int IndexContact(Contact contact);
        int UpdateContact(Contact contact);
        int UpdatePartialContact(Contact contact);
        int RemoveContact(int contactId, int accountId);
        bool DeleteContactIndex();

        int ReIndexAll(IEnumerable<Campaign> documents);
        int IndexCampaign(Campaign campaign);
        int UpdateCampaign(Campaign campaign);
        bool DeleteCampaignIndex();
        int RemoveCampaign(int campaignId, int accountId);
        int IndexCampaigns(IEnumerable<Campaign> documents);

        int ReIndexAllTags(IEnumerable<Tag> tags);        
        int RemoveTag(int tagId, int accountId);
        int IndexTag(Tag tag);
        int UpdateTag(Tag tag);
        int UpdateCampaigns(IEnumerable<Campaign> campaigns);

        bool SetupDynamicIndices(int accountId);
        bool SetupStaticIndices();

        bool SetupSuppressionListIndex<T>(int accountId) where T : SuppressionList;
        int RemoveSuppressionDocument<T>(int suppressedEmailId, int accountId) where T : SuppressionList;
        int ReIndexAllSuppressionList<T>(IEnumerable<T> documents) where T : SuppressionList;
        int IndexSuppressedEmail(SuppressedEmail doc);

        int ReIndexAll<T>(IEnumerable<T> documents) where T : EntityBase<int>;
        int Index<T>(T document) where T : EntityBase<int>;
        int Update<T>(T document) where T : EntityBase<int>;
        int UpdatePartial<T>(T document) where T : EntityBase<int>;
        int UpdateMany<T>(IEnumerable<T> documents) where T : EntityBase<int>;
        int Remove<T>(int id) where T : EntityBase<int>;
        bool SetupStaticIndice<T>() where T : EntityBase<int>;
        bool SetupTagIndex(int accountId);
        bool SetupContactIndex(int accountId);
        bool SetupCampaignIndex(int accountId);
    }

    public class IndexDetail
    {
        public string IndexName { get; set; }
        public IEnumerable<Type> Types { get; set; }
    }
}
