using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using SmartTouch.CRM.Domain.Users;

namespace SmartTouch.CRM.Identity
{
    public class UserValidator : IIdentityValidator<IdentityUser>
        
    {
        IdentityUser _user;

        public UserValidator(IdentityUser user)
        {
            _user = user;
        }
        public Task<IdentityUser> ValidateAsync(string item)
        {

            var errors = new List<string>();
            //if (string.IsNullOrWhiteSpace(item.UserName))
                errors.Add("Enter a valid email address.");

            ////TODO: check user name for duplicates
            ////if (_manager != null)
            ////{
            ////    var otherAccount = await _manager.FindByNameAsync(item.UserName);
            ////    if (otherAccount != null && otherAccount.Id != item.Id)
            ////        errors.Add("Select a different email address. An account has already been created with this email address.");
            ////}

            //return Task.Run(() => errors.Any() ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success);

            return Task.Run(() => new IdentityUser());
        }

        public Task<IdentityResult> ValidateAsync(IdentityUser item)
        {
            throw new NotImplementedException();
        }
    }
}
