using Microsoft.Owin.Security.Infrastructure;
using SmartTouch.CRM.Domain.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Identity
{
    public class SmartTouchRefreshTokenProvider : IAuthenticationTokenProvider
    {
        public async Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            var clientid = context.Ticket.Properties.Dictionary["as:client_id"];

            if (string.IsNullOrEmpty(clientid))
            {
                return;// Task.FromResult<object>(null);
            }

            var refreshTokenId = Guid.NewGuid().ToString("n");

            var tpaRepo = ApplicationUserManager.ThirdPartyAuthenticationRepository;
            
            var refreshTokenLifeTime = context.OwinContext.Get<string>("as:clientRefreshTokenLifeTime");

            var token = new ClientRefreshToken()
            {
                Id = GetHash(refreshTokenId),
                ThirdPartyClientId = clientid,
                IssuedTo = int.Parse(context.Ticket.Identity.Claims.Where(c=>c.Type=="UserID").FirstOrDefault().Value),
                IssuedOn = DateTime.UtcNow,
                ExpiresOn = DateTime.UtcNow.AddMinutes(Convert.ToDouble(refreshTokenLifeTime)),
                LastUpdatedBy = int.Parse(context.Ticket.Identity.Claims.Where(c => c.Type == "UserID").FirstOrDefault().Value),
                LastUpdatedOn = DateTime.UtcNow
            };

            context.Ticket.Properties.IssuedUtc = token.IssuedOn;
            context.Ticket.Properties.ExpiresUtc = token.ExpiresOn;

            token.ProtectedTicket = context.SerializeTicket();
            
            bool result = tpaRepo.AddRefreshToken(token);

            if (result)
            {
                context.SetToken(refreshTokenId);
            }

            await Task.FromResult(0);
        }
       
        public void Create(AuthenticationTokenCreateContext context)
        {
            throw new NotImplementedException();
        }

        public void Receive(AuthenticationTokenReceiveContext context)
        {
            throw new NotImplementedException();
        }

        public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            var allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin");
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });

            string hashedTokenId = GetHash(context.Token);
            var tpaRepo = ApplicationUserManager.ThirdPartyAuthenticationRepository;
            var refreshToken = tpaRepo.FindRefreshToken(hashedTokenId);

            if (refreshToken != null)
            {
                //Get protectedTicket from refreshToken class
                context.DeserializeTicket(refreshToken.ProtectedTicket);
                tpaRepo.RemoveRefreshToken(hashedTokenId);
            }
            await Task.FromResult(0); 
        }

        private static string GetHash(string input)
        {
            HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();

            byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(input);

            byte[] byteHash = hashAlgorithm.ComputeHash(byteValue);

            return Convert.ToBase64String(byteHash);
        }
    }
}
