using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.OAuth;
using Owin;
using SmartTouch.CRM.Identity;
using System;
using System.Linq;
using SmartTouch.CRM.Web;
using SmartTouch.CRM.Web.Hubs;
using Microsoft.AspNet.SignalR;
using SmartTouch.CRM.Domain.ThirdPartyAuthentication;
using SmartTouch.CRM.Repository.Database;
using LandmarkIT.Enterprise.CommunicationManager.Repositories;

[assembly: OwinStartup(typeof(SmartTouch.CRM.Web.Startup))]
namespace SmartTouch.CRM.Web
{
    public partial class Startup
    {
        public static OAuthBearerAuthenticationOptions OAuthBearerOptions
        {
            get;
            private set;
        }

        public void ConfigureAuth(IAppBuilder app)
        {
            System.Data.Entity.Database.SetInitializer<CRMDb>(null);
            System.Data.Entity.Database.SetInitializer<EfUnitOfWork>(null);
            ApplicationUserManager.UserStore = IoC.Container.GetInstance<IUserStore<SmartTouch.CRM.Identity.IdentityUser>>();
            ApplicationUserManager.ThirdPartyAuthenticationRepository = IoC.Container.GetInstance<IThirdPartyAuthenticationRepository>();
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Login/Login"),
                ExpireTimeSpan = new TimeSpan(1, 0, 0),
                SlidingExpiration = true
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromHours(8),
                Provider = new SmartTouchAuthorizationServerProvider(),
            });
            OAuthBearerOptions = new OAuthBearerAuthenticationOptions();
            app.UseOAuthBearerAuthentication(OAuthBearerOptions);
            var hubsConfiguration = new HubConfiguration();
            hubsConfiguration.EnableDetailedErrors = true;
            var idProvider = new CustomUserIdProvider();
            GlobalHost.DependencyResolver.Register(typeof(IUserIdProvider), () => idProvider);
            app.MapSignalR(hubsConfiguration);
            //app.Map("https://s.smarttouch.net/signalr", map =>
            //    {
            //        map.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            //        map.RunSignalR(hubsConfiguration);
            //    });
        }
    }
}
