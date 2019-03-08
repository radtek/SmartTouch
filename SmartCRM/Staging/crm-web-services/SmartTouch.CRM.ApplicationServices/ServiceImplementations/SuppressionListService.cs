using SmartTouch.CRM.ApplicationServices.Messaging.SuppressionList;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.SuppressedEmails;
using SmartTouch.CRM.Domain.SuppressionList;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.SearchEngine.Indexing;
using LandmarkIT.Enterprise.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.SearchEngine.Search;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class SuppressionListService : ISuppressionListService
    {
        readonly IIndexingService indexingService;
        readonly ISearchService<SuppressedEmail> suppressionEmailSearchService;
        readonly ISearchService<SuppressedDomain> suppressionDomainSearchService;

        readonly ISuppressionListRepository suppressionListRepository;
        readonly IUnitOfWork unitOfWork;

        public SuppressionListService(ISuppressionListRepository suppressionListRepository, IUnitOfWork unitOfWork, IIndexingService indexingService, ISearchService<SuppressedEmail> emailSearchService
            , ISearchService<SuppressedDomain> domainSearchService)
        {
            if (suppressionListRepository == null)
                throw new ArgumentNullException("suppressionListRepository");

            this.suppressionListRepository = suppressionListRepository;
            this.unitOfWork = unitOfWork;
            this.indexingService = indexingService;
            this.suppressionEmailSearchService = emailSearchService;
            this.suppressionDomainSearchService = domainSearchService;
        }

        public InsertSuppressedEmailResponse InsertSuppressedEmails(InsertSuppressedEmailRequest request)
        {
            InsertSuppressedEmailResponse response = new InsertSuppressedEmailResponse();
            if (request.EmailViewModel.IsAny())
            {
                IEnumerable<SuppressedEmail> emails = AutoMapper.Mapper.Map<IEnumerable<SuppressedEmailViewModel>, IEnumerable<SuppressedEmail>>(request.EmailViewModel);
                emails = suppressionListRepository.InsertSuppressedEmailsList(emails);
                indexingService.ReIndexAllSuppressionList<SuppressedEmail>(emails);

            }
            return response;
        }

        public InsertSuppressedDomainResponse InertSuppressedDomains(InsertSuppressedDomainRequest request)
        {
            InsertSuppressedDomainResponse response = new InsertSuppressedDomainResponse();
            if (request.DomainViewModel.IsAny())
            {
                IEnumerable<SuppressedDomain> domains = AutoMapper.Mapper.Map<IEnumerable<SuppressedDomainViewModel>, IEnumerable<SuppressedDomain>>(request.DomainViewModel);
                domains = suppressionListRepository.InsertSuppressedDomainsList(domains);
                indexingService.ReIndexAllSuppressionList<SuppressedDomain>(domains);
            }
            return response;
        }

        public GetSuppressionEmailsResponse GetSuppressionEmails(GetSuppressionEmailsRequest request)
        {
            GetSuppressionEmailsResponse response = new GetSuppressionEmailsResponse();
            if (request.AccountId != 0)
                response.SuppressedEmails = suppressionListRepository.FindAll(request.AccountId);
            return response;
        }

        public ReIndexSuppressionListResponse ReIndexSuppressionList(ReIndexSuppressionListRequest request)
        {
            ReIndexSuppressionListResponse response = new ReIndexSuppressionListResponse();

            int indexedCount = 0;
            if (request.AccountId != 0)
            {
                if (request.IndexType == 1)
                {
                    int lastIndexedsuppressionEmailId = 0;
                    indexingService.SetupSuppressionListIndex<SuppressedEmail>(request.AccountId);
                    Console.WriteLine("Suppression Email starting from: " + lastIndexedsuppressionEmailId);
                    while (true)
                    {
                        IEnumerable<SuppressedEmail> documents = suppressionListRepository.FindAllEmails(request.AccountId, lastIndexedsuppressionEmailId, request.SuppressionListBatchCount);
                        if (documents != null && documents.IsAny())
                        {
                            indexedCount = indexedCount + indexingService.ReIndexAllSuppressionList<SuppressedEmail>(documents);
                            lastIndexedsuppressionEmailId = documents.Max(c => c.Id);
                        }
                        else
                            break;
                        Console.WriteLine("Last indexed SuppressedEmailID: " + lastIndexedsuppressionEmailId);
                        Console.WriteLine("----------------------------------------------------");
                        Console.WriteLine("");
                    }
                }
                else
                {
                    int lastIndexedsuppressionDomainId = 0;
                    indexingService.SetupSuppressionListIndex<SuppressedDomain>(request.AccountId);
                    Console.WriteLine("Suppression Email starting from: " + lastIndexedsuppressionDomainId);
                    while (true)
                    {
                        IEnumerable<SuppressedDomain> documents = suppressionListRepository.FindAllDomains(request.AccountId, lastIndexedsuppressionDomainId, request.SuppressionListBatchCount);
                        if (documents != null && documents.IsAny())
                        {
                            indexedCount = indexedCount + indexingService.ReIndexAllSuppressionList<SuppressedDomain>(documents);
                            lastIndexedsuppressionDomainId = documents.Max(c => c.Id);
                        }
                        else
                            break;
                        Console.WriteLine("Last indexed SuppressedDomainID: " + lastIndexedsuppressionDomainId);
                        Console.WriteLine("----------------------------------------------------");
                        Console.WriteLine("");
                    }
                }

            }
            response.IndexedListCount = indexedCount;
            return response;
        }

        public DeleteSuppressionEmailResponse RemoveSuppressionEmail(DeleteSuppressionEmailRequest request)
        {
            DeleteSuppressionEmailResponse response = new DeleteSuppressionEmailResponse();
            if (request.SuppressionEmailId != 0)
            {
                suppressionListRepository.RemoveSuppressedEmail(request.SuppressionEmailId);
                indexingService.Remove<SuppressedEmail>(request.SuppressionEmailId);
            }
            return response;
        }

        public CheckSuppressionEmailsResponse CheckSuppressionEmails(CheckSuppressionEmailsRequest request)
        {
            CheckSuppressionEmailsResponse response = new CheckSuppressionEmailsResponse();
            if (request.Emails != null && request.Emails.IsAny())
                response.SuppressedEmails = suppressionEmailSearchService.CheckSuppressionList(new SearchParameters() { AccountId = request.AccountId, Ids = request.Emails });
            return response;
        }

        public SearchSuppressionListResponse<T> SearchSuppressionList<T>(SearchSuppressionListRequest request) where T : SuppressionList
        {
            SearchSuppressionListResponse<T> response = new SearchSuppressionListResponse<T>();
            if (request.AccountId != 0 && !string.IsNullOrEmpty(request.Text))
            {
                Type type;
                if (request.IndexType == 1)
                {
                    type = typeof(SuppressedEmail);
                    response.Results = (IEnumerable<T>)suppressionEmailSearchService.SearchSuppressionList(request.Text,
                    new SearchParameters() { AccountId = request.AccountId, Types = new List<Type>() { type } });
                }
                else
                {
                    type = typeof(SuppressedDomain);
                    response.Results = (IEnumerable<T>)suppressionDomainSearchService.SearchSuppressionList(request.Text,
                    new SearchParameters() { AccountId = request.AccountId, Types = new List<Type>() { type } });
                }
            }
            return response;
        }
    }
}
