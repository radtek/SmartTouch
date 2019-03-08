using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Hosting;

using SmartTouch.CRM.WebService.Controllers;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.ApplicationServices.Exceptions;
using SmartTouch.CRM.Tests;

using Moq;
using Moq.Linq;
using SmartTouch.CRM.ApplicationServices.Messaging;

namespace SmartTouch.CRM.WebService.Tests.Accounts
{
    [TestClass]
    public class AccountsControllerTest : ControllerTestBase
    {
        Mock<IAccountService> mockAccountService;
        Mock<IWebAnalyticsProviderService> mockWebAnalyticsProviderService;
        Mock<IPushNotificationService> mockPushNotificationService;
        MockRepository mockRepository;

        public const int SAMPLE_ACCOUNT_ID = 1;
        public const string SAMPLE_ACCOUNT_NAME = "Account1";
        public const int SAMPLE_PAGE_NO = 2;
        public const int SAMPLE_LIMIT = 3;

        [TestInitialize]
        public void InitializeTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);
            var mockAService = mockRepository.Create<IAccountService>();
            var mockWAPService = mockRepository.Create<IWebAnalyticsProviderService>();
            var mockPNService = mockRepository.Create<IPushNotificationService>();
            mockAccountService = mockAService;
            mockWebAnalyticsProviderService = mockWAPService;
            mockPushNotificationService = mockPNService;
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        //[TestMethod]
        //public void PutAccount_UpdateAccount_Succeed()
        //{
        //    AccountsController controller = new AccountsController(mockAccountService.Object);
        //    this.SetupControllerTests(controller, "http://localhost/STCRMService/api/accounts", HttpMethod.Get);
        //    var mockResponse = mockRepository.Create<UpdateAccountResponse>();
        //    AccountViewModel newAction = new AccountViewModel() { AccountID = SAMPLE_ACCOUNT_ID };

        //    mockResponse.Setup(c => c.AccountViewModel).Returns(newAction);
        //    mockAccountService.Setup(c => c.UpdateAccount(It.IsAny<UpdateAccountRequest>())).Returns(mockResponse.Object);

        //    var httpResponseMessage = controller.PutTag(It.IsAny<AccountViewModel>());
        //    var postResponse = httpResponseMessage.Content.ReadAsAsync<UpdateAccountResponse>().ContinueWith(
        //        t => { return t.Result; }).Result;
        //    var accountResponse = postResponse.;

        //    mockRepository.VerifyAll();
        //    Assert.IsTrue(postResponse.ActionViewModel.ActionId > 0, "Id is not greater than zero after insert.");
        //    Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
        //    Assert.AreEqual(postResponse.Exception, null);
        //}
        [TestMethod]
        public void PutAccount_UpdateAccount_RuntimeError_500InternalServerError()
        {
            AccountsController controller = new AccountsController(mockAccountService.Object, mockWebAnalyticsProviderService.Object, mockPushNotificationService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/accounts", HttpMethod.Get);
            var mockResponse = mockRepository.Create<UpdateAccountResponse>();

            mockAccountService.Setup(c => c.UpdateAccount(It.IsAny<UpdateAccountRequest>())).
              Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());
            var httpResponseMessage = controller.PutTag(It.IsAny<AccountViewModel>());
            var postResponse = httpResponseMessage.Content.ReadAsAsync<UpdateAccountResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();

            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);
        }

        [TestMethod]
        public void GetAccount_GetAccountsbyAccountId_Succeed()
        {
            AccountsController controller = new AccountsController(mockAccountService.Object, mockWebAnalyticsProviderService.Object, mockPushNotificationService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/accounts/1", HttpMethod.Get);

            GetAccountResponse response = mockRepository.Create<GetAccountResponse>().Object;
            response.AccountViewModel = MockData.CreateMockList<AccountViewModel>(mockRepository).Select(c => c.Object).FirstOrDefault();

            mockAccountService.Setup(c => c.GetAccount(It.IsAny<GetAccountRequest>())).Returns(response);

            var httpResponseMessage = controller.GetAccountById(SAMPLE_ACCOUNT_ID);
            var accountResponse = httpResponseMessage.Content.ReadAsAsync<GetAccountResponse>().ContinueWith(
            t => { return t.Result; }).Result;

            mockRepository.VerifyAll();
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(accountResponse.Exception, null);
        }

        [TestMethod]
        public void GetAccount_GetAccountsbyAccountIdRuntimeError_500InternalServerError()
        {
            AccountsController controller = new AccountsController(mockAccountService.Object, mockWebAnalyticsProviderService.Object, mockPushNotificationService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/accounts/1", HttpMethod.Get);

            var mockResponse = mockRepository.Create<GetAccountResponse>();
            mockAccountService.Setup(cs => cs.GetAccount(It.IsAny<GetAccountRequest>())).Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());

            var httpResponseMessage = controller.GetAccountById(SAMPLE_ACCOUNT_ID);
            var accountResponse = httpResponseMessage.Content.ReadAsAsync<GetAccountResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var exception = accountResponse.Exception;

            mockRepository.VerifyAll();
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(exception, null);
        }

        [TestMethod]
        public void GetAccount_GetAccountsbyAccountName_Succeed()
        {
            AccountsController controller = new AccountsController(mockAccountService.Object, mockWebAnalyticsProviderService.Object, mockPushNotificationService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/accounts/account1", HttpMethod.Get);

            var mockResponse = mockRepository.Create<GetAccountResponse>();

            GetAccountResponse response = mockRepository.Create<GetAccountResponse>().Object;
            response.AccountViewModel = MockData.CreateMockList<AccountViewModel>(mockRepository).Select(c => c.Object).FirstOrDefault();

            mockAccountService.Setup(c => c.GetAccountByName(It.IsAny<GetAccountNameRequest>())).Returns(response);

            var httpResponseMessage = controller.GetAccountByName(SAMPLE_ACCOUNT_NAME);
            var accountResponse = httpResponseMessage.Content.ReadAsAsync<GetAccountResponse>().ContinueWith(
            t => { return t.Result; }).Result;

            mockRepository.VerifyAll();
            //Assert.AreEqual("Account1", accountResponse.AccountViewModel.AccountName);
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(accountResponse.Exception, null);
        }

        [TestMethod]
        public void GetAccount_GetAccountsbyAccountNameRuntimeError_500InternalServerError()
        {
            AccountsController controller = new AccountsController(mockAccountService.Object, mockWebAnalyticsProviderService.Object, mockPushNotificationService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/accounts/account1", HttpMethod.Get);

            var mockResponse = mockRepository.Create<GetAccountResponse>();
            GetAccountResponse response = mockRepository.Create<GetAccountResponse>().Object;
            response.AccountViewModel = MockData.CreateMockList<AccountViewModel>(mockRepository).Select(c => c.Object).FirstOrDefault();
            mockAccountService.Setup(cs => cs.GetAccountByName(It.IsAny<GetAccountNameRequest>())).Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());

            var httpResponseMessage = controller.GetAccountByName(SAMPLE_ACCOUNT_NAME);
            var accountResponse = httpResponseMessage.Content.ReadAsAsync<GetAccountResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var exception = accountResponse.Exception;

            mockRepository.VerifyAll();
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(exception, null);
        }

        [TestMethod]
        public void PutAccount_UpdateAccount_Succeed()
        {

            AccountsController controller = new AccountsController(mockAccountService.Object, mockWebAnalyticsProviderService.Object, mockPushNotificationService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/account", HttpMethod.Get);
            var mockResponse = mockRepository.Create<UpdateAccountResponse>();
            //AccountViewModel newAccount = new AccountViewModel() { AccountID=1, AccountName = "Account" };

            //mockResponse.Setup(c => c.AccountViewModel).Returns(newAccount);
            mockAccountService.Setup(c => c.UpdateAccount(It.IsAny<UpdateAccountRequest>())).Returns(mockResponse.Object);

            var httpResponseMessage = controller.PutTag(It.IsAny<AccountViewModel>());
            var postResponse = httpResponseMessage.Content.ReadAsAsync<UpdateAccountResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var accountResponse = postResponse.AccountViewModel;

            mockRepository.VerifyAll();
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(postResponse.Exception, null);
        }

        [TestMethod]
        public void PutAccount_UpdateAccountRuntimeError_500InternalServerError()
        {
            AccountsController controller = new AccountsController(mockAccountService.Object, mockWebAnalyticsProviderService.Object, mockPushNotificationService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/account", HttpMethod.Get);
            var mockResponse = mockRepository.Create<UpdateAccountResponse>();

            mockAccountService.Setup(c => c.UpdateAccount(It.IsAny<UpdateAccountRequest>())).
              Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());
            var httpResponseMessage = controller.PutTag(It.IsAny<AccountViewModel>());
            var postResponse = httpResponseMessage.Content.ReadAsAsync<UpdateAccountResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();

            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);
        }

        [TestMethod]
        public void DeleteAccount_DeleteAccount_Succeed()
        {
            AccountsController controller = new AccountsController(mockAccountService.Object, mockWebAnalyticsProviderService.Object, mockPushNotificationService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/account/1", HttpMethod.Get);
            var mockResponse = mockRepository.Create<DeleteAccountResponse>();

            mockAccountService.Setup(c => c.DeleteAccount(It.IsAny<DeleteAccountRequest>())).
               Returns(mockResponse.Object);

            var httpResponseMessage = controller.Delete(SAMPLE_ACCOUNT_ID);
            var accountResponse = httpResponseMessage.Content.ReadAsAsync<DeleteAccountResponse>().ContinueWith(
              t => { return t.Result; }).Result;

            mockRepository.VerifyAll();
            Assert.AreEqual(accountResponse.Exception, null);
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
        }

        [TestMethod]
        public void DeleteAccount_DeleteAccount_RuntimeError_500InternalServerError()
        {
            AccountsController controller = new AccountsController(mockAccountService.Object, mockWebAnalyticsProviderService.Object, mockPushNotificationService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/account/1", HttpMethod.Get);
            var mockResponse = mockRepository.Create<DeleteAccountResponse>();

            mockAccountService.Setup(c => c.DeleteAccount(It.IsAny<DeleteAccountRequest>())).
             Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());

            var httpResponseMessage = controller.Delete(SAMPLE_ACCOUNT_ID);
            var postResponse = httpResponseMessage.Content.ReadAsAsync<DeleteAccountResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();

            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);
        }

        [TestMethod]
        public void UpdateAccountStatus_ChangeStatus_Succeed()
        {
            AccountsController controller = new AccountsController(mockAccountService.Object, mockWebAnalyticsProviderService.Object, mockPushNotificationService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/account", HttpMethod.Get);
            var mockResponse = mockRepository.Create<AccountStatusUpdateResponse>();

            mockAccountService.Setup(c => c.UpdateAccountStatus(It.IsAny<AccountStatusUpdateRequest>())).
               Returns(mockResponse.Object);

            var httpResponseMessage = controller.DeleteAccount(It.IsAny<int[]>(), It.IsAny<byte>());
            var accountResponse = httpResponseMessage.Content.ReadAsAsync<AccountStatusUpdateResponse>().ContinueWith(
              t => { return t.Result; }).Result;

            mockRepository.VerifyAll();
            Assert.AreEqual(accountResponse.Exception, null);
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
        }

        [TestMethod]
        public void UpdateAccountStatus_ChangeStatus_RuntimeError_500InternalServerError()
        {
            AccountsController controller = new AccountsController(mockAccountService.Object, mockWebAnalyticsProviderService.Object, mockPushNotificationService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/account", HttpMethod.Get);
            var mockResponse = mockRepository.Create<AccountStatusUpdateResponse>();

            mockAccountService.Setup(c => c.UpdateAccountStatus(It.IsAny<AccountStatusUpdateRequest>())).
             Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());

            var httpResponseMessage = controller.DeleteAccount(It.IsAny<int[]>(), It.IsAny<byte>());
            var postResponse = httpResponseMessage.Content.ReadAsAsync<AccountStatusUpdateResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();

            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);
        }
    }
}
