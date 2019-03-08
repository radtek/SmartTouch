using System.Collections.Generic;

using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Opportunities;
using System.ComponentModel;

namespace SmartTouch.CRM.Domain.Tags
{
    public interface ITagRepository : IRepository<Tag, int>
    {
        IEnumerable<Tag> Search(string name);
        Tag SaveContactTag(int contactId, string tagName,int tagId, int accountId,int userId);
        void SaveContactTag(int contactId, int tagId, int createdBy, int accountID);
        Tag SaveOpportunityTag(int opportunityID, string tagName,int tagId, int accountId, int userId);
        IEnumerable<Tag> FindByContact(int contactId, int accountID);       
        void DeleteForContact(int tagId, int contactId, int accountID);
        void DeleteOpportunityTag(int tagId, int OpportunityID);
        IEnumerable<Tag> FindByIDs(int[] TagIDs);
        IEnumerable<Tag> FindByCampaign(int campaignId);
        IEnumerable<Tag> FindContactTagsByCampaign(int campaignId);
        IEnumerable<Tag> FindByOpportunity(int opportuintyId);
        IEnumerable<Tag> FindAll(int limit, int pageNumber, string name, int accountId, string sortField = "", ListSortDirection direction = ListSortDirection.Descending);
        IEnumerable<Tag> AllTagsByContacts(string name, int limit, int pageNumber, int accountId,bool isAdmin,int userId, out int totalHits, int accountID, string sortField, ListSortDirection direction);
        //IEnumerable<Tag> FindAll(string name, int accountId);
        bool IsDuplicateTag(string tagName, int accountId, int tagId);
        IEnumerable<int> DeleteTags(int[] tagIDs, int accountID);
        void MergeTags(int sourcetagId, int destinationtagId, int accountID);
        Tag FindBy(string tagName, int accountId);
        IEnumerable<Tag> FindAll(int accountId);
        int TotalContactsByTag(int tagId, int accountId);
        int TotalContactsByTag(int tagId, int accountId, int? ownerId);
        int TotalContactsByTags(IEnumerable<int> tagIds, int accountId);
        int TotalContactsByTags(IEnumerable<int> tagIds, int accountId, int? ownerId);
        IEnumerable<Tag> GetPopularTags(int limit, int accountId);
        IEnumerable<Tag> GetRecentTags(int limit, int accountId);
        IEnumerable<RecentPopularTag> GetRecentAndPopularTags(int accountId); 
        List<Tag> SaveContactTags(IEnumerable<int> contact, List<Opportunity> opportunities, IEnumerable<Tag> tag, int accountId, int userId);
        bool isAssociatedWithWorkflows(int[] TagID);
        bool isAssociatedWithLeadScoreRules(int[] TagID);

        List<int> GetContactsByTag(int tagId, int accountID);
        IEnumerable<Tag> SearchTagsByTagName(int AccountID, string tagName, int limit);
        IDictionary<int, int> TotalContactsByTagIds(IEnumerable<int> tagIds,int accountId);
        int GetContactsCountByTagId(int tagId);
        IEnumerable<string> GetTagNamesByIds(List<int> tagIds);
        IEnumerable<Tag> GetTagsByName(string tagName, int accountId);
    }
}
