using AutoMapper;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class ContactRelationshipService : IContactRelationshipService
    {
        readonly IContactRelationshipRepository contactRelationshipRepository;
        ICachingService cachingService;
        readonly IUnitOfWork unitOfWork;
        readonly IAccountService accountService;

        public ContactRelationshipService(IContactRelationshipRepository contactRelationshipRepository, ICachingService cachingService,
             IUnitOfWork unitOfWork, IAccountService accountService)
        {
            if (contactRelationshipRepository == null) throw new ArgumentNullException("contactRelationshipRepository");
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
            this.contactRelationshipRepository = contactRelationshipRepository;
            this.cachingService = cachingService;
            this.unitOfWork = unitOfWork;
            this.accountService = accountService;
        }

        public SaveRelationshipResponse SaveRelationshipMap(SaveRelationshipRequest request)
        {
           
            if (request.RelationshipViewModel.SelectAll == true)
            {
                var distinctRelationTypes = request.RelationshipViewModel.Relationshipentry.Select(c => c.RelationshipType).ToList();
                
                foreach (var relationType in distinctRelationTypes)
                {
                    var firstRelation = request.RelationshipViewModel.Relationshipentry.Where(c => c.RelationshipType == relationType).First();
                    BulkOperations operationData = new BulkOperations()
                    {
                        OperationID = (int)firstRelation.RelatedContactID,
                        OperationType = (int)BulkOperationTypes.Relationship,
                        SearchCriteria = request.SelectAllSearchCriteria,
                        AdvancedSearchCriteria = request.AdvancedSearchCritieria,
                        SearchDefinitionID = null,
                        AccountID = request.AccountId,
                        UserID = (int)request.RequestedBy,
                        RoleID = request.RoleId,
                        RelationType = relationType                     
                    };
                    InsertBulkOperationRequest bulkOperationRequest = new InsertBulkOperationRequest()
                    {
                        OperationData = operationData,
                        AccountId = request.AccountId,
                        RequestedBy = (int)request.RequestedBy,
                        CreatedOn = DateTime.Now.ToUniversalTime().AddMinutes(1),
                        RoleId = request.RoleId,
                        DrillDownContactIds = request.DrillDownContactIds
                    };
                    accountService.InsertBulkOperation(bulkOperationRequest);
                }
            }
            else
            {
                foreach (var entry in request.RelationshipViewModel.Relationshipentry)
                {
                    ContactRelationship contactRelationShip = Mapper.Map<RelationshipEntry, ContactRelationship>(entry);
                    if (request.RelationshipViewModel.SelectAll == false)
                    {
                        bool IsDuplicate = contactRelationshipRepository.IsDuplicateContactRelationship(contactRelationShip);

                        if (IsDuplicate)
                            throw new UnsupportedOperationException("[|ContactRelationship already exists with|]" + " --" + entry.RelatedContact);
                    }
                    if (contactRelationShip.Id == 0)
                    {
                        contactRelationShip.CreatedBy = request.UserId;
                        contactRelationShip.CreatedOn = DateTime.Now.ToUniversalTime();
                        contactRelationshipRepository.Insert(contactRelationShip);
                    }

                    else
                        contactRelationshipRepository.Update(contactRelationShip);
                }
            }
            unitOfWork.Commit();
            return new SaveRelationshipResponse();
        }

        public GetRelationshipResponse GetContactRelationship(int contactRelationMapID, int accountID)
        {
            GetRelationshipResponse response = new GetRelationshipResponse();
            RelationshipViewModel viewModel = new RelationshipViewModel();
            var dropdowns = cachingService.GetDropdownValues(accountID);

            ContactRelationship relationship = contactRelationshipRepository.FindBy(contactRelationMapID);
            IList<ContactRelationship> relationshipList = new List<ContactRelationship>();
            relationshipList.Add(relationship);
            viewModel.Relationshipentry = Mapper.Map<IEnumerable<ContactRelationship>, IEnumerable<RelationshipEntry>>(relationshipList).ToList();
            viewModel.RelationshipTypes = dropdowns.Where(s => s.DropdownID == (short)DropdownFieldTypes.RelationshipType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive);
            response.RelationshipViewModel = viewModel;
            return response;
        }

        public GetRelationshipResponse GetContactRelationships(int contactID)
        {
            GetRelationshipResponse response = new GetRelationshipResponse();
            RelationshipViewModel Relationship = new RelationshipViewModel();

            Relationship.Relationshipentry = Mapper.Map<IEnumerable<ContactRelationship>, IEnumerable<RelationshipEntry>>(contactRelationshipRepository.FindContactRelationship(contactID)).ToList();
            foreach (RelationshipEntry entry in Relationship.Relationshipentry)
            {
                if (entry.RelatedContactID == contactID)
                {
                    entry.RelationshipTypeName = "Associated Contact";
                    entry.RelatedContact = entry.ContactName;
                    entry.RelatedContactID = entry.ContactId;
                }
            }
            response.RelationshipViewModel = Relationship;
            return response;
        }
    }
}
