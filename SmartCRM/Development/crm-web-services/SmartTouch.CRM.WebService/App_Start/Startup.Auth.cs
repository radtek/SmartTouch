using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Owin;
using SmartTouch.CRM.Domain.ThirdPartyAuthentication;
using SmartTouch.CRM.Identity;
using SmartTouch.CRM.WebService.DependencyResolution;
using System;
using System.Web.Http;

namespace SmartTouch.CRM.WebService
{
    /// <summary>
    /// Creating partial class Startup
    /// </summary>
    public partial class Startup
    {
        /// <summary>
        /// OAuthBearerOptions
        /// </summary>
        public static OAuthBearerAuthenticationOptions OAuthBearerOptions { get; private set; }
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        /// <summary>
        /// For Configuration
        /// </summary>
        /// <param name="app">app</param>
        public void ConfigureAuth(IAppBuilder app)
        {            
            ApplicationUserManager.UserStore = IoC.Container.GetInstance<IUserStore<SmartTouch.CRM.Identity.IdentityUser>>();
            ApplicationUserManager.ThirdPartyAuthenticationRepository = IoC.Container.GetInstance<IThirdPartyAuthenticationRepository>(); 
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // Implement custom TicketDataFormat to support all the systems
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Login/Login"),
                //CookieDomain = "*.smarttouch.com",                
                Provider = new CookieAuthenticationProvider
                {
                    //OnValidateIdentity method in the CookieMiddleware to look at the SecurityStamp and reject cookies when it has changed.
                    //It also automatically refreshes the user's claims from the database every refreshInterval if the stamp is unchanged (which takes care of things like changing roles etc)
                    //http://stackoverflow.com/questions/19487322/what-is-asp-net-identitys-iusersecuritystampstoretuser-interface
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, SmartTouch.CRM.Identity.IdentityUser>(
                        validateInterval: TimeSpan.FromMinutes(20), regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                },
                ExpireTimeSpan = new TimeSpan(0, 20, 0),
                SlidingExpiration = true //SlidingExpiration if true means that you can extend your time, suppose after your ExpireTimeSpan min user do something to your site, then the session time again increase to specified min from that time.
            });

            // Use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions
                {
                    AllowInsecureHttp = true,
                    TokenEndpointPath = new PathString("/token"),
                    AccessTokenExpireTimeSpan = TimeSpan.FromDays(3),
                    Provider = new SmartTouchAuthorizationServerProvider(),
                    RefreshTokenProvider = new SmartTouchRefreshTokenProvider()
                });

            // Enable the application to use bearer tokens to authenticate users
            OAuthBearerOptions = new OAuthBearerAuthenticationOptions();
            app.UseOAuthBearerAuthentication(OAuthBearerOptions);            
        }
    }
}