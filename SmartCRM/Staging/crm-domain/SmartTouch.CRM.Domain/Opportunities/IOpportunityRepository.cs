using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.Images;
using System.ComponentModel;

namespace SmartTouch.CRM.Domain.Opportunities
{
    public interface IOpportunityRepository : IRepository<Opportunity, int>
    {
        IEnumerable<Opportunity> FindAll(string name, int accountId);
        IEnumerable<Opportunity> FindAll(string name, int limit, int pageNumber, int accountId);
        void DeleteOpportunities(int[] OpportunityIDs, int modifiedBy);
        void DeleteOpportunityContact(int OpportunityID,int ContactID);
        IEnumerable<int> GetRelatedContacts(int opportunityId);
        bool IsOpportunityUnique(Opportunity opportunity);
        IEnumerable<Opportunity> FindByContact(int ContactID,int userId);
        IEnumerable<DashboardPieChartDetails> OppertunityPipelinefunnelChartDetails(int accountID, int[] userIds, bool isAccountAdmin, DateTime startDate, DateTime endDate);
        IEnumerable<Opportunity> FindAllByOwner(int[] OwnerIDs,DateTime startDate,DateTime endDate);
        IEnumerable<int> GetOpportunityStageContacts(short opportunityStage);
        OpportunityTableType InsertOpportunity(OpportunityTableType opportunity);
        Image GetOpportunityProfileImage(int imageId);
        IEnumerable<Opportunity> SearchByOpportunityName(int accountId, string name);
        void InsertAndUpdateOpportunityBuyers(IEnumerable<OpportunityContactMapTableType> buyers);
        void UpdateOpportunityName(int opportunityId, string oppName,int lastUpdateBy);
        void UpdateOpportunityStage(int opportunityId, int stageId);
        void UpdateOpportunityOwner(int opportunityId, int ownerId);
        void UpdateOpportunityDescription(int opportunityId, string oppDescription, int lastUpdateBy);
        void UpdateOpportunityPotential(int opportunityId, decimal potential, int lastUpdateBy);
        void UpdateOpportunityExpectedCloseDate(int opportunityId, DateTime closedDate);
        void UpdateOpportunityType(int opportunityId, string opptype, int lastUpdateBy);
        void UpdateOpportunityProductType(int opportunityId, string productType, int lastUpdateBy);
        void UpdateOpportunityAddress(int opportunityId, string oppAddress, int lastUpdateBy);
        void UpdateOpportunityImage(int opportunityId, Image image, int lastUpdateBy);
        IEnumerable<OpportunityBuyer> GetAllOpportunityBuyers(int opportunityId,int accountId,int pageNumber,int pageSize);
        OpportunityBuyer GetOpportunityBuyerDetailsById(int buyerId);
        int DeleteOpportunityBuyer(int buyerId);
        IEnumerable<OpportunityBuyer> GetAllContactOpportunities(int contactId);
        string GetOpportunityNameByBuyerId(int buyerId);
        IEnumerable<OpportunityBuyer> GetAllOpportunityBuyerNames(int opportunityId, int accountId);
        IEnumerable<Opportunity> GetOpportunitiesWithBuyersList(int accountId, int pageNumber, int pageSize,string query, string sortField, int[] userIds, DateTime? startDate,DateTime? endDate, ListSortDirection listSortDirection);
        IList<int> GetOpportunityContactIds(int opportunityId);
    }
}
