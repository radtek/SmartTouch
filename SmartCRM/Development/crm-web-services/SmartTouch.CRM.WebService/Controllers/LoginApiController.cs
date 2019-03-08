using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using SmartTouch.CRM.WebService.Helpers;
using Thinktecture.IdentityModel.Client;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SmartTouch.CRM.WebService.Models;
using SmartTouch.CRM.ApplicationServices.Messaging.Notes;
using System.Web.Http;
using System.Configuration;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using System.Web.Http.Description;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Login;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.ApplicationServices.Messaging.User;

namespace SmartTouch.CRM.WebService.Controllers
{
    /// <summary>
    /// Creating LoginApi controller for LoginApi module
    /// </summary>
    [System.Web.Mvc.AllowAnonymous]
    public class LoginApiController : ApiController
    {
        readonly IThirdPartyClientService thirdPartyClientService;
        readonly IUserService userService;

        /// <summary>
        /// Creating Constructor for Login API Controller for accessing.
        /// </summary>
        /// <param name="thirdPartyClientService"></param>
        /// <param name="userService"></param>
        public LoginApiController(IThirdPartyClientService thirdPartyClientService, IUserService userService)
        {
            this.thirdPartyClientService = thirdPartyClientService;
            this.userService = userService;
        }

        /// <summary>
        /// Use this method to login by passing user name, password and client id
        /// Store the refreshtoken for future purpose.
        /// </summary>
        /// <param name="loginInfo"></param>
        /// <returns></returns>
        [System.Web.Http.Route("login")]
        public HttpResponseMessage Login(LoginInfo loginInfo)
        {
            Validate("Username", loginInfo.UserName);
            Validate("Password", loginInfo.Password);
            Validate("ApiKey", loginInfo.ApiKey);
            var tokens = GetAccessTokenRespnse(loginInfo.UserName, loginInfo.Password, loginInfo.ApiKey);
            return Request.CreateResponse(tokens.Json);
        }

        /// <summary>
        /// Use this method for Mobile APP to login by passing Account Name, user name and password
        /// Store the refreshtoken for future purpose.
        /// </summary>
        /// <param name="loginInfo"></param>
        /// <returns></returns>
        [System.Web.Http.Route("MobileAppLogin")]
        public HttpResponseMessage MobileLogin(LoginAPIViewModel loginInfo)
        {
            LoginAPIViewModel loginViewModel = new LoginAPIViewModel();
            try
            {

                Validate("Username", loginInfo.Email);
                Validate("Password", loginInfo.Password);
                Validate("AccountName", loginInfo.AccountName);

                ThirdPartyClient thirdPartyClient = new ThirdPartyClient();
                var domainName = loginInfo.AccountName + ConfigurationManager.AppSettings["STDOMAIN"].ToString();
                thirdPartyClient = thirdPartyClientService.GetCLientSecretKeyByDomainName(domainName);
                if (thirdPartyClient != null)
                {
                    var tokens = GetAccessTokenRespnse(loginInfo.Email, loginInfo.Password, thirdPartyClient.ID);
                    if (!string.IsNullOrEmpty(tokens.AccessToken))
                    {
                        UserViewModel userViewModel = userService.GetUserDetailsByEmailAndAccountId(new GetUserRequest(1)
                        {
                            AccountId = thirdPartyClient.AccountID,
                            UserName = loginInfo.Email
                        }).User;
                        loginViewModel.AccessToken = tokens.AccessToken;
                        loginViewModel.AccountId = thirdPartyClient.AccountID;
                        loginViewModel.UserModel = userViewModel;
                    }
                    else
                        return Request.CreateResponse(tokens.Json); //loginViewModel.ErrorMessage = tokens.Error;

                }
                else
                {
                    LoginError error = new LoginError();
                    error.error = "Invalid Account";
                    return Request.CreateResponse(error);
                }
            }
            catch (Exception ex)
            {
                loginViewModel.ErrorMessage = ex.Message.Replace("[|", "").Replace("|]", "");
            }
            return Request.CreateResponse(loginViewModel);
        }

        /// <summary>
        /// Use this method to get refreshed accesstoken.
        /// </summary>
        /// <param name="refreshTokenInfo"></param>
        /// <returns>refreshtoken</returns>
        [System.Web.Http.Route("refreshtoken")]
        public HttpResponseMessage RefreshToken(RefreshTokenInfo refreshTokenInfo)
        {
            Validate("RefreshToken", refreshTokenInfo.RefreshToken);
            Validate("ApiKey", refreshTokenInfo.ApiKey);
            var response = GetRefreshTokenResponse(refreshTokenInfo.RefreshToken, refreshTokenInfo.ApiKey);
            return Request.CreateResponse(response.Json);
        }

        /// <summary>
        /// Get Access Token 
        /// </summary>
        /// <param name="username">User Name </param>
        /// <param name="password">Password</param>
        /// <param name="clientId">Client Id </param>
        /// <returns></returns>
        private TokenResponse GetAccessTokenRespnse(string username, string password, string clientId)
        {
            var tokenEndpoint = string.Format("{0}/token", ConfigurationManager.AppSettings["WEBSERVICE_URL"]);
            var client = new OAuth2Client(new Uri(tokenEndpoint), clientId, "");
            return client.RequestResourceOwnerPasswordAsync(username, password).Result;

            //if(!(string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(clientId)))
            //{
            //    var tokenEndpoint = string.Format("{0}/token", ConfigurationManager.AppSettings["WEBSERVICE_URL"]);
            //    var client = new OAuth2Client(new Uri(tokenEndpoint), clientId, "");
            //    return client.RequestResourceOwnerPasswordAsync(username, password).Result;
            //}
            //else
            //{
            //    return new TokenResponse(System.Net.HttpStatusCode.BadRequest, "One of the parameters sent is empty.");
            //}
        }
        /// <summary>
        /// Get Refresh Token
        /// </summary>
        /// <param name="refreshToken">Refresh Token</param>
        /// <param name="clientId">Client Id</param>
        /// <returns></returns>
        private TokenResponse GetRefreshTokenResponse(string refreshToken, string clientId)
        {
            var tokenEndpoint = string.Format("{0}/token", ConfigurationManager.AppSettings["WEBSERVICE_URL"]);
            var client = new OAuth2Client(new Uri(tokenEndpoint), clientId, "");
            return client.RequestRefreshTokenAsync(refreshToken).Result;
        }
        /// <summary>
        /// Validation for Key
        /// </summary>
        /// <param name="key">key </param>
        /// <param name="value">value</param>
        /// <returns></returns>
        private void Validate(string key, string value)
        {
            string validationMessage = string.Empty;
            if (string.IsNullOrEmpty(value))
            {
                validationMessage = key + " is invalid.";
                throw new UnsupportedOperationException(validationMessage);
            }

        }
    }

    public class LoginError
    {
        public string error { get; set; }
    }
}
