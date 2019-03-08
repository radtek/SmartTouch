using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Repository.Repositories;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Entities;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.Domain.Accounts;


namespace SmartTouch.CRM.Identity
{
    public class UserStore : IUserStore<IdentityUser>,
                             IUserClaimStore<IdentityUser>,
                             IUserLoginStore<IdentityUser>,
                             IUserRoleStore<IdentityUser>,
                             IUserPasswordStore<IdentityUser>,
                             IUserEmailStore<IdentityUser>
    {
        readonly IUserRepository userRepository;
        readonly IAccountRepository accountRepository;
        readonly IUserSettingsRepository userSettingsRepository;
        readonly IUnitOfWork unitOfWork;
        //int AccountID { get; set; }
        const int Password_history_lmit = 3;
        IdentityUser currentUser;
        public UserStore(IUserRepository userRepository, IUnitOfWork unitOfWork, IAccountRepository accountRepository, IUserSettingsRepository userSettingsRepository)
        {
            this.userRepository = userRepository;
            this.accountRepository = accountRepository;
            this.userSettingsRepository = userSettingsRepository;
            this.unitOfWork = unitOfWork;
        }

        IdentityUser ConvertToIdentityUser(User user)
        {
            Logger.Current.Verbose("Converting user to IdentityUser");
            IdentityUser identityUser = new IdentityUser();
            identityUser.Account = user.Account;
            identityUser.AccountID = user.AccountID;
            identityUser.Id = user.Id.ToString();
            identityUser.FirstName = user.FirstName;
            identityUser.LastName = user.LastName;
            identityUser.UserName = user.Email.EmailId;
            identityUser.Email = user.Email;
            identityUser.Password = user.Password;
            identityUser.Role = user.Role;
            identityUser.RoleID = user.Role.Id;
            identityUser.Status = user.Status;
            identityUser.IsSTAdmin = user.IsSTAdmin;
            identityUser.HasTourCompleted = user.HasTourCompleted;
            return identityUser;
        }

        #region UserStore
        public Task CreateAsync(IdentityUser user)
        {
            if (user == null) { throw new ArgumentNullException("user"); }
            return Task.Run(() =>
                {
                    userRepository.InsertUserCredentials(user.Email.EmailId, user.Password);
                });
        }

        public Task DeleteAsync(IdentityUser user)
        {
            if (user == null) { throw new ArgumentNullException("user"); }
            return Task.Run(() =>
                {
                    userRepository.Delete(user);
                });
        }

        public Task UpdateAsync(IdentityUser user)
        {
            Logger.Current.Verbose("Updating user");
            User domainuser = userRepository.FindBy(Convert.ToInt32(user.Id));
            if (domainuser == null) { throw new ArgumentNullException("user"); }
            domainuser.Password = user.Password;
            userRepository.Update(domainuser);
            unitOfWork.Commit();
            return Task.Run(() =>
            {
            });
        }

        public Task<IdentityUser> FindByIdAsync(string userId)
        {
            //var result = default(IdentityUser);
            IdentityUser identityUser = null;
            if (currentUser == null || (currentUser != null && !currentUser.Id.ToString().Equals(userId)))
            {
                var user = userRepository.FindUserForLogin(Convert.ToInt32(userId));
                if (user != null)
                {
                    //AccountID = user.AccountID;
                    identityUser = ConvertToIdentityUser(user);
                    currentUser = identityUser;
                }
            }
            else
                identityUser = currentUser;

            return Task<IdentityUser>.Run(() =>
            {
                return identityUser;
                //return result; 
            });
        }

        public Task<IdentityUser> FindByNameAsync(string email)
        {
            var result = default(IdentityUser);
            Logger.Current.Informational("Email :" + email);
            if (email.Contains('|'))
            {
                var emailId = email.Split('|')[0];
                var account = email.Split('|')[1];
                var accountId = default(short);
                short.TryParse(account, out accountId);
                var user = userRepository.FindByEmailAndAccountId(emailId, accountId);
                if (user != null)
                    result = ConvertToIdentityUser(user);
                currentUser = result;
            }
            else
            {
                Console.WriteLine("");
                //var user = userRepository.FindByEmailAndAccountId(email, AccountID);
                //if (user != null) result = ConvertToIdentityUser(user);
            }

            return Task<IdentityUser>.Run(() => { return result; });
        }

        public Task<IdentityUser> FindByResetFlag()
        { var result = default(IdentityUser); return Task<IdentityUser>.Run(() => { return result; }); }

        public void Dispose()
        {
            // if (_dbContext != null) { _dbContext.Dispose(); }
        }
        #endregion

        #region Claims
        public Task AddClaimAsync(IdentityUser user, Claim claim)
        {
            return Task.Run(() => { new Claim(claim.Type, claim.Value); });
            //   throw new NotImplementedException();
            //return Task.Run(() => { var userData = userRepository.FindBy(Convert.ToInt32(user.Id)); }); 
        }

        public Task<IList<Claim>> GetClaimsAsync(IdentityUser user)
        {
            return Task<IList<Claim>>.Run(() =>
            {
                var result = default(IList<Claim>);
                var userData = currentUser;
                int UserID = default(int);
                int.TryParse(userData.Id, out UserID);
                UserSettings settings = userSettingsRepository.GetSettings(user.AccountID, UserID);
                if (userData != null)
                {
                    result = new List<Claim>
                    {
                        new Claim("UserID", userData.Id.ToString()),
                        new Claim("FirstName", userData.FirstName),
                        new Claim("LastName", userData.LastName),
                        new Claim("UserName", userData.UserName),
                        new Claim("RoleID", userData.RoleID.ToString()),
                        new Claim("RoleName", userData.Role.RoleName),
                        new Claim("AccountID",userData.AccountID.ToString()),
                        new Claim("DateFormatId",settings.DateFormatType),
                        new Claim("TimeZone",settings.TimeZone),
                        new Claim("ItemsPerPage",settings.ItemsPerPage.ToString()),
                        new Claim("Currency",settings.CurrencyFormat),
                        new Claim("Subscription",userData.Account.subscription.SubscriptionName),
                      //  new Claim("AccountName",userData.Account.AccountName),
                        new Claim("AccountPrimaryEmail",userData.Account.Email.EmailId),
                        new Claim("UserEmail", userData.Email.EmailId),
                        new Claim("IsSTAdmin",userData.IsSTAdmin.ToString()),
                        new Claim("IanaTimeZone", GetTimeZoneInfoForTzdbId(settings.TimeZone)),
                        new Claim("CountryID", settings.CountryID)
                    };
                }
                return result;
            });
        }

        public string GetTimeZone(int userId, int accountId)
        {
            string timeZone = userRepository.GetUserTimeZone(userId, accountId);
            if (timeZone == null)
            {
                timeZone = accountRepository.GetUserTimeZone(accountId);
            }
            return timeZone;
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
        public Task RemoveClaimAsync(IdentityUser user, Claim claim)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region External Logins
        public Task AddLoginAsync(IdentityUser user, UserLoginInfo login)
        {
            if (user == null) { throw new ArgumentNullException("user"); }

            if (login == null) { throw new ArgumentNullException("login"); }
            return Task.Run(() =>
            {
            });
        }

        public Task<IdentityUser> FindAsync(UserLoginInfo login)
        {
            throw new NotImplementedException();
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(IdentityUser user)
        {
            throw new NotImplementedException();
        }

        public Task RemoveLoginAsync(IdentityUser user, UserLoginInfo login)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Roles
        public Task AddToRoleAsync(IdentityUser user, string role)
        {
            throw new NotImplementedException();
        }

        public Task<IList<string>> GetRolesAsync(IdentityUser user)
        {
            if (user == null) { throw new ArgumentNullException("user"); }
            var userId = default(int);
            int.TryParse(user.Id, out userId);
            //var userDb = userRepository.FindUserForLogin(userId);
            IList<string> roles = new List<string>();
            roles.Add(user.Role.RoleName);
            return Task<IList<string>>.Run(() =>
            {
                return roles;
            });
        }

        public Task<bool> IsInRoleAsync(IdentityUser user, string role)
        {
            if (user == null) { throw new ArgumentNullException("user"); }
            if (role == null) { throw new ArgumentNullException("role"); }
            return Task<bool>.Run(() =>
            {
                //This will be implemented as part of authorization.
                return true;
            });
        }

        public Task RemoveFromRoleAsync(IdentityUser user, string role)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Password
        public Task<bool> HasPasswordAsync(IdentityUser user)
        {
            if (Convert.ToBoolean(user.Password))
            { return Task.FromResult<bool>(true); }
            else { return Task.FromResult<bool>(false); }
        }

        public Task SetPasswordHashAsync(IdentityUser user, string passwordHash)
        {
            return Task.Run(() =>
                {
                    user.Password = passwordHash;
                });
        }

        public Task<string> GetPasswordHashAsync(IdentityUser user)
        {
            if (user == null) { throw new ArgumentNullException("user"); }
            int[] intUserid = new int[] { Convert.ToInt32(user.Id) };
            userRepository.UpdateUserStatus(intUserid, 1);  //Will be called by FindAsync method on login and on password reset.

            return Task<string>.Run(() =>
            {
                // return userRepository.FindUserForLogin(Convert.ToInt32(user.Id)).Password;
                return user.Password;
            });
        }

        #endregion

        #region Email
        public Task<IdentityUser> FindByEmailAsync(string email)
        {
            if (email == null) { throw new ArgumentNullException("user"); }
            User user = userRepository.FindByEmail(email);
            return Task<IdentityUser>.Run(() =>
            {
                return ConvertToIdentityUser(user);
            });
        }

        public Task<string> GetEmailAsync(IdentityUser user)
        {
            return Task<string>.Run(() => (user.Email.EmailId));
        }

        public Task<bool> GetEmailConfirmedAsync(IdentityUser user)
        {
            return Task<bool>.Run(() => (true));
        }

        public Task SetEmailAsync(IdentityUser user, string email)
        {
            throw new NotImplementedException();
        }

        public Task SetEmailConfirmedAsync(IdentityUser user, bool confirmed)
        {
            //int[] intUserid = new int[] { Convert.ToInt32(user.Id) };

            return Task.Run(() =>
            {
                //userRepository.UpdateUserStatus(intUserid, 1);
            });
        }
        #endregion

        public bool CheckPasswordResetFlag(int userId, string account)
        {
            int accountId;
            int.TryParse(account, out accountId);
            return userRepository.CheckPasswordResetFlag(userId, accountId);
        }

        public void UpdatePasswordResetFlag(int userId, bool status)
        {
            userRepository.UpdatePasswordResetFlag(userId, status);
        }

        public Task PasswordAudit(int userId, string newpassword)
        {
            return Task.Run(() => { userRepository.UpdatePasswordHistory(userId, newpassword); });
            //appuser.UserUsedPassword.Add(new UsedPassword() { UserID = appuser.Id, HashPassword = userpassword });
            //return UpdateAsync(appuser);
        }

        public Task<bool> IsPreviousPassword(string userId, string newPassword)
        {
            int userID;
            int.TryParse(userId, out userID);
            List<string> previousPasswords = userRepository.GetPreviousPasswords(userID, Password_history_lmit);
            PasswordHasher passwordHasher = new PasswordHasher();
            if (previousPasswords.Select(up => up).Where(up => passwordHasher.VerifyHashedPassword(up, newPassword) != PasswordVerificationResult.Failed).Any())
            {
                return Task<bool>.Run(() => (true));
            }
            return Task<bool>.Run(() => (false));
        }

        public void InsertLoginAudit(int userId, int accountId, string IP, SignInActivity activity)
        {
            if (IP != null)
            {
                userRepository.InsertLoginAudit(userId, accountId, IP, activity);
            }
        }

        public void InsertUserProfileAudit(int userId, UserAuditType auditType, int auditedBy, string password)
        {
            var hashedPassword = string.Empty;
            if (password != null) hashedPassword = new PasswordHasher().HashPassword(password);
            userRepository.InsertUserProfileAudit(userId, auditType, auditedBy, hashedPassword);
        }
    }
}
