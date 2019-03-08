using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using SmartTouch.CRM.Domain.Login;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace SmartTouch.CRM.Identity
{
    public class SmartTouchAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        //This method is responsible for validating the 'Client',in our case we have only one client so we will always return that its validated successfully.
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            var tpaRepo = ApplicationUserManager.ThirdPartyAuthenticationRepository;
            string clientId = string.Empty;
            string clientSecret = string.Empty;
            if (!context.TryGetBasicCredentials(out clientId, out clientSecret))
            {
                context.TryGetFormCredentials(out clientId, out clientSecret);
            }
            if (context.ClientId == null)
            {
                context.SetError("invalid_clientId", "ClientId should be sent.");
                context.Rejected();
                return Task.FromResult<object>(null);
            }
            var thirdPartyClient = tpaRepo.FindBy(clientId);

            if (thirdPartyClient == null)
            {
                context.SetError("invalid_clientId", string.Format("Client '{0}' is not registered in the system.", context.ClientId));
                context.Rejected();
                return Task.FromResult<object>(null); ;
            }

            if (!thirdPartyClient.IsActive)
            {
                context.SetError("invalid_clientId", "Client is inactive.");
                context.Rejected();
                return Task.FromResult<object>(null); ;
            }

            context.OwinContext.Set<string>("as:clientAllowedOrigin", thirdPartyClient.AllowedOrigin);
            context.OwinContext.Set<string>("as:clientRefreshTokenLifeTime", thirdPartyClient.RefreshTokenLifeTime.ToString());
            context.OwinContext.Set<string>("as:accountId", thirdPartyClient.AccountID.ToString());

            context.Validated();
            return Task.FromResult<object>(null);
        }


        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            string username = context.UserName;
            var allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin");

            if (allowedOrigin == null) allowedOrigin = "*";

            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });

            var applicationUserManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();

            if(!context.UserName.Contains("|"))
            {
                username = context.UserName + "|" + context.OwinContext.Get<string>("as:accountId");
            }
            

            var user = await applicationUserManager.FindAsync(username, context.Password);

            if (user != null)
            {
                string timeZone = applicationUserManager.GetTimeZone(Convert.ToInt32(user.Id), user.AccountID);
                var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                identity.AddClaim(new Claim(Claims.UserName, user.UserName));
                identity.AddClaim(new Claim(Claims.UserID, user.Id));
                identity.AddClaim(new Claim(Claims.RoleID, user.RoleID.ToString()));
                identity.AddClaim(new Claim(Claims.AccountID, user.AccountID.ToString()));
                identity.AddClaim(new Claim(Claims.RoleName, user.Role.RoleName));
                identity.AddClaim(new Claim(Claims.TimeZone, timeZone));
                identity.AddClaim(new Claim(Claims.IanaTimeZone, GetTimeZoneInfoForTzdbId(timeZone)));
                var props = new AuthenticationProperties(new Dictionary<string, string>
                {
                    { 
                        "as:client_id", (context.ClientId == null) ? string.Empty : context.ClientId
                    },
                    { 
                        "userName", context.UserName
                    }
                });

                var ticket = new AuthenticationTicket(identity, props);
                context.Validated(ticket);
            }
            else
            {
                context.Rejected();
            }
            await Task.FromResult(0);
        }
        public override async Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            await Task.FromResult(0);
        }

        public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            var originalClient = context.Ticket.Properties.Dictionary["as:client_id"];
            var currentClient = context.ClientId;

            if (originalClient != currentClient)
            {
                context.SetError("invalid_clientId", "Refresh token is issued to a different clientId.");
                context.Rejected();
                return Task.FromResult<object>(null);
            }

            // Change auth ticket for refresh token requests
            var newIdentity = new ClaimsIdentity(context.Ticket.Identity);

            foreach(var claim in newIdentity.Claims)
            {
                if (claim.Type == "newClaim")
                {
                    newIdentity.RemoveClaim(claim);
                    break;
                }
            }
            newIdentity.AddClaim(new Claim("newClaim", "newValue"));

            var newTicket = new AuthenticationTicket(newIdentity, context.Ticket.Properties);
            context.Validated(newTicket);

            return Task.FromResult<object>(null);
        }

        private string GetTimeZoneInfoForTzdbId(string tzdbId)
        {
            if (tzdbId.Equals("UTC", StringComparison.OrdinalIgnoreCase))
                return "Etc/UTC";
            var tzdbSource = NodaTime.TimeZones.TzdbDateTimeZoneSource.Default;
            var tzi = TimeZoneInfo.FindSystemTimeZoneById(tzdbId);
            var tzid = tzdbSource.MapTimeZoneId(tzi);
            return tzdbSource.CanonicalIdMap[tzid];
        }
    }
    //resource : http://bitoftech.net/2014/06/01/token-based-authentication-asp-net-web-api-2-owin-asp-net-identity/

}
