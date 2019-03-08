using Microsoft.AspNet.Identity;
using SmartTouch.CRM.Domain.Users;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Identity
{
    [NotMapped]
    public class IdentityUser : User, IUser
    {
       
        //public string Id { get { return Id.ToString(); } }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<IdentityUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var claimsIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return claimsIdentity;
        }
    }
}
