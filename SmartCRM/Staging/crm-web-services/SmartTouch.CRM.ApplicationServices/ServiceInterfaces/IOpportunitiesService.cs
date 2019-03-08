using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartTouch.CRM.ApplicationServices.Messaging.Opportunity;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.Domain.Opportunities;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface IOpportunitiesService
    {
        SearchOpportunityResponse GetAllOpportunities(SearchOpportunityRequest request);      
        InsertOpportunityResponse InsertOpportunity(InsertOpportunityRequest request);
        GetOpportunityResponse getOpportunity(GetOpportunityRequest request);
        UpdateOpportunityResponse UpdateOpportunity(UpdateOpportunityRequest request);
        DeleteOpportunityResponse DeleteOpportunities(DeleteOpportunityRequest request);
        ReIndexDocumentResponse ReIndexOpportunities(ReIndexDocumentRequest request);
        GetOpportunityListByContactResponse GetContactOpportunities(GetOpportunityListByContactRequest request);
        GetOpportunityListByContactResponse GetContactOpportunitiesList(GetOpportunityListByContactRequest request);
        DeleteOpportunityContactResponse DeleteOpportunityContact(DeleteOpportunityContactRequest request);

        IEnumerable<OpportunityViewModel> GetAllOpportunitiesByOwner(int[] ownerId,DateTime startDate,DateTime endDate);
        GetOpportunityStageContactsRsponse GetOpportunityStageContacts(GetOpportunityStageContactsRequest request);
        OpportunityIndexingResponce OpportunityIndexing(OpportunityIndexingRequest request);
        GetOpportunityListResponse GetAllOpportunitiesByName(GetOpportunityListRequest request);
        InsertOpportunityBuyerResponse InsertOpportunityBuyer(InsertOpportunityBuyerRequest request);
        UpdateOpportunityBuyerResponse UpdateOpportunityBuyer(UpdateOpportunityBuyerRequest request);
        UpdateOpportunityViewResponse UpdateOpportunityName(UpdateOpportunityViewRequest request);
        UpdateOpportunityViewResponse UpdateOpportunityStage(UpdateOpportunityViewRequest request);
        UpdateOpportunityViewResponse UpdateOpportunityOwner(UpdateOpportunityViewRequest request);
        UpdateOpportunityViewResponse UpdateOpportunityDescription(UpdateOpportunityViewRequest request);
        UpdateOpportunityViewResponse UpdateOpportunityPotential(UpdateOpportunityViewRequest request);
        UpdateOpportunityViewResponse UpdateOpportunityExpectedCloseDate(UpdateOpportunityViewRequest request);
        UpdateOpportunityViewResponse UpdateOpportunityType(UpdateOpportunityViewRequest request);
        UpdateOpportunityViewResponse UpdateOpportunityProductType(UpdateOpportunityViewRequest request);
        UpdateOpportunityViewResponse UpdateOpportunityAddress(UpdateOpportunityViewRequest request);
        UpdateOpportunityViewResponse UpdateOpportunityImage(UpdateOpportunityViewRequest request);
        GetOpportunityBuyerResponse GetOpportunityBuyers(GetOpportunityBuyerRequest request);
        OpportunityBuyer GetOpportunityBuyerById(int buyerId);
        void DeleteOpportunityBuyer(int buyerId);
        string GetOpportunityNameByOPPContactMapId(int buyerId);
        IEnumerable<OpportunityBuyer> GetAllOpportunityBuyersName(int opportunityId, int accountId);
        GetOpportunityContactsResponse GetOpportunityContacts(GetOpportunityContactsRequest request);
    }
}
