using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SmartTouch.CRM.ApplicationServices.Messaging.User;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Tours;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.WebAnalytics;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using DA = SmartTouch.CRM.Domain.Actions;

namespace SmartTouch.CRM.ApplicationServices.Tests.Users
{
    [TestClass]
    public class UserServiceTest
    {
        MockRepository mockRepository = default(MockRepository);
        IUserService userService = default(IUserService);     
        Mock<ICachingService> mockCachingService =default(Mock<ICachingService>);
        Mock<IUserRepository> mockUserRepository = default(Mock<IUserRepository>);
        Mock<IUserSettingsRepository> mockUserSettingRepository = default(Mock<IUserSettingsRepository>);
        Mock<IUserActivitiesRepository> mockUserActivitiesRepository = default(Mock<IUserActivitiesRepository>);
        Mock<ITourRepository> mockTourRepository = default(Mock<ITourRepository>);
        Mock<DA.IActionRepository> mockActionRepository = default(Mock<DA.IActionRepository>);
        Mock<IContactRepository> mockContactRepository = default(Mock<IContactRepository>);
        Mock<IWebAnalyticsProviderRepository> mockWebVisitsRepository = default(Mock<IWebAnalyticsProviderRepository>);
        Mock<IAccountRepository> mockAccountRepository = default(Mock<IAccountRepository>);
        Mock<IServiceProviderRepository> mockServiceProviderRepository = default(Mock<IServiceProviderRepository>);
        Mock<IMessageRepository> messageRepository = default(Mock<IMessageRepository>);

        [TestInitialize]
        public void Initialize()
        {
            InitializeAutoMapper.Initialize();
            mockRepository = new MockRepository(MockBehavior.Default);
            IUnitOfWork mockUnitOfWork = mockRepository.Create<IUnitOfWork>().Object;
            mockUserRepository = mockRepository.Create<IUserRepository>();
            mockAccountRepository = mockRepository.Create<IAccountRepository>();
            mockUserSettingRepository = mockRepository.Create<IUserSettingsRepository>();
            mockUserActivitiesRepository = mockRepository.Create<IUserActivitiesRepository>();
            mockTourRepository = mockRepository.Create<ITourRepository>();
            mockActionRepository = mockRepository.Create<DA.IActionRepository>();
            mockCachingService = mockRepository.Create<ICachingService>();
            mockWebVisitsRepository = mockRepository.Create<IWebAnalyticsProviderRepository>();
            mockServiceProviderRepository = mockRepository.Create<IServiceProviderRepository>();
            messageRepository = mockRepository.Create<IMessageRepository>();
            userService = new UserService(mockUserRepository.Object,mockAccountRepository.Object,mockUserSettingRepository.Object,mockTourRepository.Object, 
                                mockActionRepository.Object, mockUnitOfWork, mockUserActivitiesRepository.Object,mockContactRepository.Object,
                                mockCachingService.Object,mockWebVisitsRepository.Object, mockServiceProviderRepository.Object, messageRepository.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        [TestMethod]
        public void GetAllUsers_ToGetAllTheUsers_Succeed()
        {
            var mockUsers = UserMockData.GetMockUsers(mockRepository, 10).ToList();
            mockUserRepository.Setup(cr => cr.FindAll(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<byte>(), It.IsAny<byte>(),It.IsAny<int>(),It.IsAny<bool>())).Returns(mockUsers);
            GetUserListResponse response = userService.GetAllUsers(new GetUserListRequest() { Query = "", Limit=10, PageNumber=1 });
            var users = response.Users;
            mockRepository.VerifyAll();
            Assert.AreEqual(mockUsers.Count(), users.Count());
            Assert.AreEqual(null, response.Exception);
        }


        [TestMethod]
        public void InsertUser_ToAddNewUser_Success()
        { 
            IList<UserViewModel> viewModelList = UserMockData.GetUserViewModel();
            int count = viewModelList.Count();
            viewModelList.Add(new UserViewModel(){ FirstName="UserFirstName",LastName = "UserLastName", RoleID=2, PrimaryEmail="user@gmail.com"});
            User user = UserMockData.GetUserClass();
            mockUserRepository.Setup(cr => cr.Insert(It.IsAny<User>())).Verifiable();
            InsertUserResponse response = userService.InsertUser(new InsertUserRequest() { UserViewModel = viewModelList.Last()});
            mockRepository.VerifyAll();           
            Assert.AreNotEqual(count, viewModelList.Count());
            Assert.AreEqual(null, response.Exception);
        }


        [TestMethod]
        public void UpdateUser_ToUpdateUser_Success()
        {
            UserViewModel viewModel = UserMockData.GetUpdatedUserViewModel();
            mockUserRepository.Setup(cr => cr.Update(It.IsAny<User>())).Verifiable();
            UpdateUserResponse response = userService.UpdateUser(new UpdateUserRequest() { UserViewModel = viewModel });
            mockRepository.VerifyAll();    
        }

        [TestMethod]
        public void GetUser_ToGetUsersById_Success()
        {
            GetUserRequest req = new GetUserRequest(61);
            User user = new User
            {
                Id = "61",
                FirstName = "Srinivas",
                LastName = "Naidu",
                Email = new Domain.ValueObjects.Email { EmailId = "satya@gmail.com" }
            };
            mockUserRepository.Setup(cr => cr.FindBy(It.IsAny<int>())).Returns(user);
            GetUserResponse response = userService.GetUser(req);
            mockRepository.VerifyAll();
            Assert.AreEqual(response.User.UserID, int.Parse(user.Id));
            Assert.AreEqual(null, response.Exception);
        }

        [TestMethod]
        public void UpdateUsersStatus_UpdateStatus_Success()
        {             
            int[] user = new int[] { 1, 2, 61 };
            byte status = 1;
            UserStatusRequest req = new UserStatusRequest();
            req.Status = status;
            req.UserID = user;
            mockUserRepository.Setup(cr => cr.UpdateUserStatus(It.IsAny<int[]>(), It.IsAny<byte>())).Verifiable();
           UserStatusResponse res = userService.UpdateUsersStatus(req);
           mockRepository.VerifyAll();
            Assert.AreEqual(res.Exception, null);
        }


        [TestMethod]
        public void UpdateUsersStatus_UpdateStatus_Failure()
        {
            int[] user = new int[] { 1, 2, 61 };
            byte status = 1;
            UserStatusRequest req = new UserStatusRequest();
            req.Status = status;
            req.UserID = user;
            mockUserRepository.Setup(cr => cr.UpdateUserStatus(It.IsAny<int[]>(), It.IsAny<byte>())).Throws(new InvalidOperationException());
            UserStatusResponse response = userService.UpdateUsersStatus(req);
            mockRepository.VerifyAll();
            Assert.AreEqual(typeof(InvalidOperationException), response.Exception.GetType());
            Assert.AreNotEqual(null, response.Exception);
        }


        [TestMethod]
        public void ChangeRole_ChangeUserRole_Success()
        {
            int[] user = new int[] { 1, 2, 61 };
            byte role = 3;
            ChangeRoleRequest req = new ChangeRoleRequest();
            req.RoleID = role;
            req.UserID = user;
            mockUserRepository.Setup(cr => cr.ChangeRole(It.IsAny<short>(), It.IsAny<int[]>())).Verifiable();
            ChangeRoleResponse res = userService.ChangeRole(req);
            mockRepository.VerifyAll();
            Assert.AreEqual(res.Exception, null);
        }


        [TestMethod]
        public void ChangeRole_ChangeUserRole_Failure()
        {
            int[] user = new int[] { 1, 2, 61 };
            byte role = 3;
            ChangeRoleRequest req = new ChangeRoleRequest();
            req.RoleID = role;
            req.UserID = user;
            mockUserRepository.Setup(cr => cr.ChangeRole(It.IsAny<short>(), It.IsAny<int[]>())).Verifiable();
            ChangeRoleResponse res = userService.ChangeRole(req);
            mockRepository.VerifyAll();
            Assert.AreEqual(res.Exception, null);
        }


        [TestMethod]
        public void DeactivateUser_ForDeactivatingUser_Success()
        {                       
            DeactivateUserRequest req = new DeactivateUserRequest();
            req.UserID = new int[61];
            mockUserRepository.Setup(cr => cr.DeactivateUsers(It.IsAny<int[]>(),It.IsAny<int>())).Verifiable();
            DeactivateUserResponse res = userService.DeactivateUser(req);
            mockRepository.VerifyAll();
            Assert.AreEqual(res.Exception, null);            
        }


        [TestMethod]
        public void DeactivateUser_ForDeactivatingUser_Failure()
        {
            DeactivateUserRequest req = new DeactivateUserRequest();
            req.UserID = new int[61];
            mockUserRepository.Setup(cr => cr.DeactivateUsers(It.IsAny<int[]>(), It.IsAny<int>())).Verifiable();            
            DeactivateUserResponse res = userService.DeactivateUser(req);
            mockRepository.VerifyAll();
            Assert.AreEqual(res.Exception, null);
        }

    }
}
