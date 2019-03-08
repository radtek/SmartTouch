using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;
using System.Text.RegularExpressions;
using SmartTouch.CRM.Domain.ThirdPartyAuthentication;

namespace SmartTouch.CRM.Identity
{
    public class ApplicationUserManager : UserManager<IdentityUser>
    {
        public ApplicationUserManager(IUserStore<IdentityUser> store)
            : base(store)
        {
        }

        public static IUserStore<IdentityUser> UserStore { get; set; }
        public static IThirdPartyAuthenticationRepository ThirdPartyAuthenticationRepository { get; set; }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(UserStore);

            manager.UserValidator = new UserValidator<IdentityUser>(manager) { AllowOnlyAlphanumericUserNames = false };
            //manager.PasswordValidator = new PasswordValidator
            //{
            //    RequiredLength = 6,
            //    RequireNonLetterOrDigit = true,
            //    RequireDigit = true,
            //    RequireLowercase = true,
            //    RequireUppercase = true,
            //};
            manager.PasswordValidator = new CustomPasswordValidator();

            manager.EmailService = new IdentityEmailService();

            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<IdentityUser>(dataProtectionProvider.Create("ASP.NET Identity")) { TokenLifespan = TimeSpan.FromDays(1) };
            }

            return manager;
        }

        public override async Task<IdentityUser> FindAsync(string username, string password)
        {
            var user = default(IdentityUser);
            if (username.Contains("|"))
            {
                var account = username.Split('|')[1];
                user = await base.FindAsync(username, password);    //Calls FindByNameAsync and GetPasswordAsync in UserStore
                if (user != null)
                {
                    int userID;
                    int.TryParse(user.Id, out userID);
                    bool flag = CheckResetFlag(userID, account);
                    if (!flag)
                    {
                        return user;
                    }
                }
                return null;
            }
            return null;
        }

        public async Task<IdentityResult> ResetPasswordAsync(string UserID, string UsedToken, string NewPassword, string emailId, int accountId)   //Resetting password from login screen
        {
            var userStore = Store as UserStore;
            if (await userStore.IsPreviousPassword(UserID, NewPassword))
            {
                return await Task.FromResult(IdentityResult.Failed("You cannot re-use previous password"));
            }

            var Result = await base.ResetPasswordAsync(UserID, UsedToken, NewPassword);

            if (Result.Succeeded)
            {
                await userStore.PasswordAudit(Convert.ToInt32(UserID), PasswordHasher.HashPassword(NewPassword));
                userStore.UpdatePasswordResetFlag(Convert.ToInt32(UserID), false);
            }

            return Result;
        }

        public async Task<IdentityResult> ChangePasswordAsync(string UserID, string CurrentPassword, string NewPassword, string emailId, int accountId)   //Changing password from user profile screen
        {
            var userStore = Store as UserStore;
            if (await userStore.IsPreviousPassword(UserID, NewPassword))
            {
                return await Task.FromResult(IdentityResult.Failed("You cannot re-use previous password"));
            }

            var Result = await base.ChangePasswordAsync(UserID, CurrentPassword, NewPassword);

            if (Result.Succeeded)
            {
                await userStore.PasswordAudit(Convert.ToInt32(UserID), PasswordHasher.HashPassword(NewPassword));
                userStore.UpdatePasswordResetFlag(Convert.ToInt32(UserID), false);
            }

            return Result;
        }

        public void InsertLoginAudit(int userId, int accountID, string IP, SignInActivity activity)
        {
            var userStore = Store as UserStore;
            if (IP != null)
            {
                userStore.InsertLoginAudit(userId, accountID, IP, activity);
            }
        }

        public void InsertUserProfileAudit(int userId, UserAuditType auditType, int auditedBy, string password)
        {
            var userStore = Store as UserStore;
            userStore.InsertUserProfileAudit(userId, auditType, auditedBy, password);
        }

        public void UpdateResetFlag(int userId, bool flagStatus)
        {
            var userStore = Store as UserStore;
            userStore.UpdatePasswordResetFlag(userId, flagStatus);
        }

        public bool CheckResetFlag(int userId, string account)
        {
            var userStore = Store as UserStore;
            bool flag = userStore.CheckPasswordResetFlag(userId, account);
            return flag;
        }

        public string GetTimeZone(int userId, int accountId)
        {
            var userStore = Store as UserStore;
            string timeZone = string.Empty;
            if (userId != 0 && accountId != 0)
            {
                timeZone = userStore.GetTimeZone(userId, accountId);
            }
            return timeZone;
        }

    }

    public class CustomPasswordValidator : IIdentityValidator<string>
    {
        public int RequiredLength = 6;

        public Task<IdentityResult> ValidateAsync(string item)
        {
            if (String.IsNullOrEmpty(item) || item.Length < RequiredLength)
            {
                return Task.FromResult(IdentityResult.Failed(String.Format("Password should be of length {0}", RequiredLength)));
            }
            if (!Regex.IsMatch(item, @"\d"))
            {
                return Task.FromResult(IdentityResult.Failed(String.Format("Password should contain at least one digit.")));
            }
            if (!Regex.IsMatch(item, @"(?=.*[a-z])"))
            {
                return Task.FromResult(IdentityResult.Failed(String.Format("Password should contain at least one lower-case letter.")));
            }
            if (!Regex.IsMatch(item, @"(?=.*[A-Z])"))
            {
                return Task.FromResult(IdentityResult.Failed(String.Format("Password should contain at least one upper-case letter.")));
            }
            if (!Regex.IsMatch(item, @"(?=.*[_\W])"))
            {
                return Task.FromResult(IdentityResult.Failed(String.Format("Password should contain at least one special character.")));
            }
            return Task.FromResult(IdentityResult.Success);
        }
    }
}

//The advantage of ApplicationUserManager class is that we can override methods defined in the UserManager class
//http://forums.asp.net/t/1962085.aspx?2014+01+16+Nightly+Build+blew+away+UserManager+PasswordResetTokens+UserManager+UserConfirmationTokens