using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Search;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.Fields;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.Tags;

namespace SmartTouch.CRM.Domain.Search
{
    public interface IAdvancedSearchRepository : IRepository<SearchDefinition, int>
    {

        IEnumerable<SearchDefinition> FindAll(int accountId, int userId, Boolean IsPredefinedSearch, Boolean IsFavoriteSearch);

        IEnumerable<SearchDefinition> FindAll(string name, int limit, int pageNumber, int accountId, int userId);

        IEnumerable<SearchDefinition> FindAllFavoriteSearches(int limit, int pageNumber, int accountId, int userId);

        IEnumerable<SearchDefinition> FindAllDefault();

        IEnumerable<SearchDefinition> FindSearchDefinitions(int accountId);

        string DeleteSearches(List<int> SearchDefinitionIDs);

        DateTime? GetLastRunDate(short SearchDefinitionID);

        bool IsSearchNameUnique(SearchDefinition definition);

        IEnumerable<Field> GetSearchFields(int accountId);

        IEnumerable<Field> GetUpdatableFields(int accountId);

        IEnumerable<FieldValueOption> GetSearchValueOptions(int fieldId);

        bool UpdateSearchDefinition(short SearchDefinitionID);

        void UpdateLastRunActivity(int searchDefinitionId, int userId, int accountId, string searchName);

        void UpdateViewActivity(int searchDefinitionId, int userId, int accountId, string searchName);

        IEnumerable<FieldValueOption> GetLeadAdapters(int accountId);

        IEnumerable<Tag> GetTags(int accountID);

        string GetSearchDescription(int searchDefinitionId);

        IEnumerable<AVColumnPreferences> GetColumnPreferences(int entityId, byte entityType);

        void SaveColumnPreferences(int entityId, byte entityType, IEnumerable<int> fields, byte showingType);
        Dictionary<int, int> GetAllSearchDefinitionIsAndAccountIds();
        void InsertSmartSearchQueue(int searchDefinitionId,int accountId);
        void UpdateSmartSearchQueue(int searchDefinitionId, int accountId,bool status);
        IEnumerable<string> GetSearchDefinitionNamesByIds(List<int> searchdefinitionIds);
    }
}
