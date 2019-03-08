using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ImportData;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Domain.Subscriptions;
using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.SearchEngine.Indexing;
using SmartTouch.CRM.SearchEngine.Search;
using SmartTouch.CRM.Domain.CustomFields;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.Search;

namespace SmartTouch.CRM.ApplicationServices.Tests.Accounts
{
    [TestClass]
    public class AccountServiceTest
    {
        MockRepository mockRepository;
        IAccountService accountService;
        Mock<IAccountRepository> mockAccountRepository;
        Mock<IContactRepository> mockContactRepository;
        Mock<IImportDataRepository> mockImportRepository;
        Mock<ILeadAdaptersRepository> mockILeadAdaptersRepository;
        Mock<ISubscriptionRepository> mocksubscriptionRepository;
        Mock<IDropdownRepository> mockDropdownRepository;
        Mock<IIndexingService> mockIndexingService;
        Mock<ISearchService<Contact>> mockSearchService;
        Mock<ICommunicationProviderService> mockServiceProviderService;
        Mock<ICustomFieldRepository> mockcustomFieldRepository;
        Mock<IUserRepository> mockUserRepository;
        Mock<ICachingService> mockCacheService;
        Mock<IReportService> mockReportService;
        Mock<IServiceProviderRepository> mockServiceproviderRepository;
        Mock<IUrlService> mockUrlService;
        Mock<IImageService> mockImageService;
        Mock<IUserService> mockUserService;
        Mock<ICustomFieldService> mockCustomFieldService;
        Mock<ITagRepository> mockTagReposiotory;
        Mock<IAdvancedSearchRepository> mockAdvancedsearchRepository;

        [TestInitialize]
        public void Initialize()
        {
            InitializeAutoMapper.Initialize();
            mockRepository = new MockRepository(MockBehavior.Default);
            IUnitOfWork mockUnitOfWork = mockRepository.Create<IUnitOfWork>().Object;
            mockAccountRepository = mockRepository.Create<IAccountRepository>();
            mockContactRepository = mockRepository.Create<IContactRepository>();
            mockTagReposiotory = mockRepository.Create<ITagRepository>();
            mockImportRepository = mockRepository.Create<IImportDataRepository>();
            mockILeadAdaptersRepository = mockRepository.Create<ILeadAdaptersRepository>();
            mocksubscriptionRepository = mockRepository.Create<ISubscriptionRepository>();
            mockIndexingService = mockRepository.Create<IIndexingService>();
            mockSearchService = mockRepository.Create<ISearchService<Contact>>();
            mockDropdownRepository = mockRepository.Create<IDropdownRepository>();
            mockServiceProviderService = mockRepository.Create<ICommunicationProviderService>();
            mockcustomFieldRepository = mockRepository.Create<ICustomFieldRepository>();
            mockUserRepository = mockRepository.Create<IUserRepository>();
            mockCacheService = mockRepository.Create<ICachingService>();
            mockServiceproviderRepository = mockRepository.Create<IServiceProviderRepository>();
            mockUrlService = mockRepository.Create<IUrlService>();
            mockImageService = mockRepository.Create<IImageService>();
            mockReportService = mockRepository.Create<IReportService>();
            mockUserService = mockRepository.Create<IUserService>();
            mockCustomFieldService = mockRepository.Create<ICustomFieldService>();
            mockAdvancedsearchRepository = mockRepository.Create<IAdvancedSearchRepository>();



            accountService = new AccountService(mockAccountRepository.Object, mockUnitOfWork, mockContactRepository.Object, mockImportRepository.Object, 
                 mockILeadAdaptersRepository.Object,mocksubscriptionRepository.Object,mockDropdownRepository.Object, mockIndexingService.Object,mockSearchService.Object,
                 mockServiceProviderService.Object,mockcustomFieldRepository.Object,mockUserRepository.Object,mockCacheService.Object,
                 mockServiceproviderRepository.Object, mockUrlService.Object, mockImageService.Object,mockReportService.Object,mockTagReposiotory.Object,
                 mockAdvancedsearchRepository.Object, mockCustomFieldService.Object, mockUserService.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        [TestMethod]
        public void GetAllAccounts_ToGetAllTheAccounts_Succeed()
        {
            var mockAccounts = AccountMockData.GetMockAccounts(mockRepository, 10).ToList();
         //   mockAccountRepository.Setup(cr => cr.FindAll(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<byte>())).Returns(mockAccounts);
            GetAccountsResponse response = accountService.GetAllAccounts(new GetAccountsRequest() { Query = "", Limit = 10, PageNumber = 1 });
            var accounts = response.Accounts;
            mockRepository.VerifyAll();
            Assert.AreEqual(mockAccounts.Count(), accounts.Count());
            Assert.AreEqual(null, response.Exception);
        }

        [TestMethod]
        public void GetAccount_ToGetAccountById_Success()
        {
            GetAccountRequest req = new GetAccountRequest(10) {  RequestedBy = 1};
            Account account = AccountMockData.GetAccountClass();
            mockAccountRepository.Setup(cr => cr.FindBy(It.IsAny<int>())).Returns(account);
            GetAccountResponse response = accountService.GetAccount(req);
            mockRepository.VerifyAll();
            Assert.AreEqual(response.AccountViewModel.AccountID, account.Id);
            Assert.AreEqual(null, response.Exception);
        }

        [TestMethod]
        public void InsertAccount_ToAddNewAccount_Success()
        {
            AccountViewModel viewModel = AccountMockData.GetAccountViewModel();           
            mockAccountRepository.Setup(cr => cr.Insert(It.IsAny<Account>())).Verifiable();
            InsertAccountResponse response = accountService.InsertAccount(new InsertAccountRequest() { AccountViewModel = viewModel });
            //mockRepository.VerifyAll();
            Assert.AreEqual("Add at least one Address", response.Exception.Message);
        }

        [TestMethod]
        public void UpdateAccount_ToUpdateAccount_Success()
        {
            ////AccountViewModel viewModel = AccountMockData.GetUpdatedAccountViewModel();
            //var viewModelList = AccountMockData.AllAccounts(mockRepository).ToList();
            //AccountViewModel viewModel = viewModelList.FirstOrDefault(a => a.AccountID == 1);
            //mockAccountRepository.Setup(cr => cr.Update(It.IsAny<Account>())).Verifiable();
            //UpdateAccountResponse response = accountService.UpdateAccount(new UpdateAccountRequest() { AccountViewModel = viewModel });
            //mockRepository.VerifyAll();
            //Assert.AreEqual("Add at least one Address", response.Exception.Message);

            AccountViewModel viewModel = AccountMockData.GetUpdatedAccountViewModel();
            mockAccountRepository.Setup(cr => cr.Update(It.IsAny<Account>())).Verifiable();
            UpdateAccountResponse response = accountService.UpdateAccount(new UpdateAccountRequest() { AccountViewModel = viewModel });
            mockRepository.VerifyAll();
            Assert.AreEqual(response.Exception, null);
            //Assert.AreEqual("Add at least one Address", response.Exception.Message);
        }

        [TestMethod]
        public void DeleteAccount_ToDeleteAccount_Success()
        {
            Account account = AccountMockData.GetAccountClass();
            mockAccountRepository.Setup(cr => cr.FindBy(It.IsAny<int>())).Returns(account);
            DeleteAccountRequest req = new DeleteAccountRequest(10);
            DeleteAccountResponse response = accountService.DeleteAccount(req);
            mockRepository.VerifyAll();
            Assert.AreEqual(null, response.Exception);
        }

        [TestMethod]
        public void UpdateAccountStatus_UpdateStatus_Success()
        {
            int[] accountId = new int[] { 1, 2, 61 };
            byte status = 1;
            AccountStatusUpdateRequest req = new AccountStatusUpdateRequest();
            req.StatusID = status;
            req.AccountID = accountId;
            mockAccountRepository.Setup(cr => cr.UpdateAccountStatus(It.IsAny<int[]>(), It.IsAny<byte>())).Verifiable();
            AccountStatusUpdateResponse res = accountService.UpdateAccountStatus(req);
            mockRepository.VerifyAll();
            Assert.AreEqual(res.Exception, null);
        }

        //[TestMethod]
        //public void UpdateUsersStatus_UpdateStatus_Failure()
        //{
        //    int[] accountId = new int[] { 1, 2, 61 };
        //    byte status = 1;
        //    AccountStatusUpdateRequest req = new AccountStatusUpdateRequest();
        //    req.StatusID = status;
        //    req.AccountID = accountId;
        //    mockAccountRepository.Setup(cr => cr.UpdateAccountStatus(It.IsAny<int[]>(), It.IsAny<byte>())).Verifiable();
        //    AccountStatusUpdateResponse res = accountService.UpdateAccountStatus(req);
        //    mockRepository.VerifyAll();
        //    Assert.AreEqual(res.Exception, null);
        //}

        [TestMethod]
        public void GetAccountByName_ToLoginInToWebSite_Succeed()
        {            
            GetAccountNameRequest req = new GetAccountNameRequest();
            req.name = "sample";
            //mockAccountRepository.Setup(cr => cr.FindByName(It.IsAny<string>())).Returns(req);            
            GetAccountResponse response = accountService.GetAccountByName(req);         
            mockRepository.VerifyAll();
        }
    }
}
