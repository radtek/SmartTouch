//using System;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Microsoft.Owin.Security;
//using Owin;
//using Microsoft.AspNet.Identity;
//using Microsoft.AspNet.Identity.Owin;
//using Moq;
//using SmartTouch.CRM.ApplicationServices.ViewModels;
//using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using SmartTouch.CRM.Identity;
//using SmartTouch.CRM.Web.Controllers;
//using SmartTouch.CRM.Entities;
//using SmartTouch.CRM.Domain.ValueObjects;

//namespace SmartTouch.CRM.Web.Tests.Login
//{
//    [TestClass]
//    public class LoginControllerTest
//    {
//        [TestMethod]
//        public void TestSuccessfulLogin()
//        {
//            // Arrange
//            var userStore = new Mock<IUserStore<IdentityUser>>();
//            var userManager = new Mock<UserManager<IdentityUser>>(userStore.Object);
//            var accountService = new Mock<IAccountService>();
//            var loginModel = new LoginViewModel
//            {
//                Email = "a@gmail.com",
//                Password = "b",
//                RememberMe = false
//            };
//            var returnUrl = "/foo";
//            var user = new IdentityUser
//            {
//                Email.EmailId = loginModel.Email
//            };
//            var identity = new ClaimsIdentity(DefaultAuthenticationTypes.ApplicationCookie);
//            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
//            identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));

//            userManager.Setup(um => um.FindAsync(loginModel.Email, loginModel.Password)).Returns(Task.FromResult(user));
//            userManager.Setup(um => um.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie)).Returns(Task.FromResult(identity));

//            var controller = new LoginController(accountService.Object);
//            var helper = new LoginMockHelper(controller);

//            // Act
//            var actionResult = controller.Login(loginModel, returnUrl).Result;

//            // Assert
//            var redirectResult = actionResult as RedirectResult;
//            Assert.IsNotNull(redirectResult);
//            Assert.AreEqual(returnUrl, redirectResult.Url);

//            Assert.AreEqual(loginModel.Email, helper.OwinContext.Authentication.AuthenticationResponseGrant.Identity.Name);
//            Assert.AreEqual(DefaultAuthenticationTypes.ExternalCookie, helper.OwinContext.Authentication.AuthenticationResponseRevoke.AuthenticationTypes.First());
//        }

//        [TestMethod]
//        public void TestUnsuccessfulLogin()
//        {
//            // Arrange
//            var userStore = new Mock<IUserStore<IdentityUser>>();
//            var userManager = new Mock<UserManager<IdentityUser>>(userStore.Object);
//            var accountService = new Mock<IAccountService>();
//            var loginModel = new LoginViewModel
//            {
//                Email = "a",
//                Password = "b",
//                RememberMe = false
//            };
//            var returnUrl = "/foo";
//            var user = new IdentityUser
//            {
//                UserName = loginModel.Email
//            };
//            var identity = new ClaimsIdentity(DefaultAuthenticationTypes.ApplicationCookie);
//            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
//            identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));

//            userManager.Setup(um => um.FindAsync(loginModel.UserName, loginModel.Password)).Returns(Task.FromResult<IdentityUser>(null));

//            var controller = new LoginController(accountService.Object);
//            var helper = new MvcMockHelper(controller);

//            // Act
//            var actionResult = controller.Login(loginModel, returnUrl).Result;

//            // Assert
//            Assert.IsTrue(actionResult is ViewResult);
//            var errors = controller.ModelState.Values.First().Errors;
//            Assert.AreEqual(1, errors.Count());
//        }

//        [TestMethod]
//        public void TestSuccessfulRegister()
//        {
//            // Arrange
//            var userStore = new Mock<IUserStore<IdentityUser>>();
//            var userManager = new Mock<UserManager<IdentityUser>>(userStore.Object);
//            var accountService = new Mock<IAccountService>();
//            var registerModel = new RegisterViewModel
//            {
//                Email = "a",
//                Password = "b",
//                ConfirmPassword = "b"
//            };
//            var result = IdentityResult.Success;
//            var identity = new ClaimsIdentity(DefaultAuthenticationTypes.ApplicationCookie);
//            identity.AddClaim(new Claim(ClaimTypes.Name, registerModel.Email));

//            userManager.Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(), registerModel.Password)).Returns(Task.FromResult(result));
//            userManager.Setup(um => um.CreateIdentityAsync(It.IsAny<IdentityUser>(), DefaultAuthenticationTypes.ApplicationCookie)).Returns(Task.FromResult(identity));

//            var controller = new LoginController(accountService.Object);
//            var helper = new MvcMockHelper(controller);

//            // Act
//            var actionResult = controller.Register(registerModel).Result;

//            // Assert
//            var redirectResult = actionResult as RedirectToRouteResult;
//            Assert.IsNotNull(redirectResult);
//            Assert.AreEqual("Home", redirectResult.RouteValues["controller"]);
//            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);

//            Assert.AreEqual(registerModel.UserName, helper.OwinContext.Authentication.AuthenticationResponseGrant.Identity.Name);
//            Assert.AreEqual(DefaultAuthenticationTypes.ExternalCookie, helper.OwinContext.Authentication.AuthenticationResponseRevoke.AuthenticationTypes.First());
//        }
//    }
//}
