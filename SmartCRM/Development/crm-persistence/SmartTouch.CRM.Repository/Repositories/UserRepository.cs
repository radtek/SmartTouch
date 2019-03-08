using AutoMapper;
using LandmarkIT.Enterprise.Utilities.Logging;
using LandmarkIT.Enterprise.Extensions;
using LinqKit;
using SmartTouch.CRM.Domain.ImplicitSync;
using SmartTouch.CRM.Domain.Roles;
using SmartTouch.CRM.Domain.Tours;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DA = SmartTouch.CRM.Domain.Actions;
using SmartTouch.CRM.Domain.Reports;
using System.Data.Common;
using System.Data;
using System.Data.SqlClient;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class UserRepository : Repository<User, int, UsersDb>, IUserRepository
    {
        public UserRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory) { }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<User> FindAll()
        {
            IEnumerable<UsersDb> users = ObjectContextFactory.Create().Users.Include(c => c.Addresses).Include(c => c.Communication).Include(c => c.Role);
            foreach (UsersDb dc in users)
            {
                yield return Mapper.Map<UsersDb, User>(dc);
            }
        }

        /// <summary>
        /// Gets the currencies.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<dynamic> GetCurrencies()
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<CurrenciesDb> currencyDb = db.Currencies.ToList();
            if (currencyDb != null)
                return convertToCurrency(currencyDb);
            return null;
        }

        /// <summary>
        /// Gets the user role.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public Role GetUserRole(int userId)
        {
            var db = ObjectContextFactory.Create();
            var sql = "select r.* from users (nolock) u inner join roles (nolock) r on r.roleid = u.roleid where u.userid = @uid";
            RolesDb roleDb = db.Get<RolesDb>(sql, new { uid = userId }).FirstOrDefault();
            if (roleDb != null)
                return Mapper.Map<RolesDb, Role>(roleDb);
            return null;
        }

        /// <summary>
        /// Converts to currency.
        /// </summary>
        /// <param name="currencies">The currencies.</param>
        /// <returns></returns>
        IEnumerable<Currency> convertToCurrency(IEnumerable<CurrenciesDb> currencies)
        {
            foreach (CurrenciesDb currency in currencies)
            {
                yield return new Currency() { CurrencyId = currency.CurrencyID, Format = currency.Format, Symbol = currency.Symbol };
            }
        }

        /// <summary>
        /// Gets the date formats.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DateFormat> GetDateFormats()
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<DateFormatDb> dateFormatDb = db.DateFormats.ToList();
            if (dateFormatDb != null)
                return convertToDateFormat(dateFormatDb);
            return null;
        }

        /// <summary>
        /// Gets the email.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public IEnumerable<Email> GetEmail(int accountId, int userId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT AE.* FROM AccountEmails (NOLOCK) AE 
                        WHERE AE.UserID=@UserId AND (AE.AccountID=@AccountId OR AE.AccountID=1)";
            IEnumerable<AccountEmailsDb> emails = db.Get<AccountEmailsDb>(sql, new { UserId = userId, AccountId = accountId }).ToList();
            return convertToEmail(emails);
        }

        /// <summary>
        /// Campaigns the get email.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public IEnumerable<Email> CampaignGetEmail(int accountId, int userId, int roleId)
        {
            var db = ObjectContextFactory.Create();

            var query = @"DECLARE @ModuleID INT = 0
                              SELECT @ModuleID = ModuleID  from RoleModuleMap (nolock) where ModuleID=74 and RoleID=@RoleId
                              SELECT a.EmailID,a.Email,a.IsPrimary,a.EmailSignature,a.UserID,a.AccountID,a.UserID,a.ServiceProviderID  from AccountEmails (nolock) a
                              LEFT JOIN users (nolock) u ON a.UserID = u.UserID
                              WHERE a.accountid =@AccountId  AND ((u.IsDeleted = 0 and u.Status = 1) OR a.ServiceProviderID >0) AND @ModuleID > 0 
                              AND a.Email NOT LIKE '%@gmail.%'  AND a.Email NOT LIKE '%@yahoo.%%'
                              AND a.Email NOT LIKE '%@outlook.%'AND a.Email NOT LIKE '%@live.%' 
                              AND a.Email NOT LIKE '%@hotmail.%' 
                              AND a.Email NOT LIKE '%@iCloud.%' AND a.Email NOT LIKE '%@mac.%'
                              AND a.Email NOT LIKE '%@me.%' AND a.Email NOT LIKE '%@aol.%'";
            var userEmails = db.Get<AccountEmailsDb>(query, new { AccountId = accountId, RoleId = roleId });
            if (userEmails.Count() > 0)
            {
                return convertToEmail(userEmails);
            }
            else
            {
                var sql = @"SELECT * FROM AccountEmails(NOLOCK) AC
							  WHERE AC.UserID=@UserId  AND AC.Email NOT LIKE '%@gmail.%'  AND AC.Email NOT LIKE '%@yahoo.%%'
                              AND AC.Email NOT LIKE '%@outlook.%'AND AC.Email NOT LIKE '%@live.%' 
                              AND AC.Email NOT LIKE '%@hotmail.%' 
                              AND AC.Email NOT LIKE '%@iCloud.%' AND AC.Email NOT LIKE '%@mac.%'
                              AND AC.Email NOT LIKE '%@me.%' AND AC.Email NOT LIKE '%@aol.%'";
                var emails = db.Get<AccountEmailsDb>(sql, new { UserId = userId }).ToList();

                return convertToEmail(emails);
            }
        }

        /// <summary>
        /// Gets the user primary email.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public string GetUserPrimaryEmail(int userId)
        {
            var db = ObjectContextFactory.Create();
            var primaryEmail = db.Users.Where(u => u.UserID == userId && u.Status == Status.Active).Select(s => s.PrimaryEmail).FirstOrDefault();
            return primaryEmail;
        }

        /// <summary>
        /// Gets the user phone numbers.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public List<string> GetUserPhoneNumbers(int userId)
        {
            var db = ObjectContextFactory.Create();
            List<string> userphones = new List<string>();
            var userPhoneNumbers = db.Users.Where(up => up.UserID == userId).SingleOrDefault();
            if (userPhoneNumbers.WorkPhone != null)
                userphones.Add(userPhoneNumbers.WorkPhone);
            if (userPhoneNumbers.MobilePhone != null)
                userphones.Add(userPhoneNumbers.MobilePhone);
            if (userPhoneNumbers.HomePhone != null)
                userphones.Add(userPhoneNumbers.HomePhone);
            return userphones;
        }

        /// <summary>
        /// Gets the user primary phone number.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public string GetUserPrimaryPhoneNumber(int userId)
        {
            var db = ObjectContextFactory.Create();
            var defaultPhoneNumber = string.Empty;
            var phoneDetails = db.Users.Where(u => u.UserID == userId && u.Status == Status.Active).Select(s => new
            {
                homePhone = s.HomePhone,
                workPhone = s.WorkPhone,
                mobilePhone = s.MobilePhone,
                primaryPhoneType = s.PrimaryPhoneType
            }).FirstOrDefault();
            if (phoneDetails.primaryPhoneType == "M")
                defaultPhoneNumber = phoneDetails.mobilePhone;
            else if (phoneDetails.primaryPhoneType == "H")
                defaultPhoneNumber = phoneDetails.homePhone;
            else if (phoneDetails.primaryPhoneType == "W")
                defaultPhoneNumber = phoneDetails.workPhone;
            return defaultPhoneNumber;
        }

        /// <summary>
        /// Gets the user email.
        /// </summary>
        /// <param name="emailId">The email identifier.</param>
        /// <returns></returns>
        public Email GetUserEmail(int emailId)
        {
            Email email = new Email();
            var db = ObjectContextFactory.Create();
            var emailDb = db.AccountEmails.Where(ae => ae.EmailID == emailId).FirstOrDefault();
            if (emailDb != null)
                email = Mapper.Map<AccountEmailsDb, Email>(emailDb);
            return email;
        }

        /// <summary>
        /// Converts to email.
        /// </summary>
        /// <param name="emails">The emails.</param>
        /// <returns></returns>
        IEnumerable<Email> convertToEmail(IEnumerable<AccountEmailsDb> emails)
        {
            foreach (AccountEmailsDb email in emails)
            {
                yield return new Email() { EmailID = email.EmailID, IsPrimary = email.IsPrimary, EmailId = email.Email, EmailSignature = email.EmailSignature, AccountID = email.AccountID, UserID = email.UserID, ServiceProviderID = email.ServiceProviderID };
            }
        }

        /// <summary>
        /// Converts to date format.
        /// </summary>
        /// <param name="dateFormats">The date formats.</param>
        /// <returns></returns>
        IEnumerable<DateFormat> convertToDateFormat(IEnumerable<DateFormatDb> dateFormats)
        {
            foreach (DateFormatDb date in dateFormats)
            {
                yield return new DateFormat() { DateFormatId = date.DateFormatId, FormatName = date.FormatName.ToUpper() };
            }
        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="db">The database.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Invalid user id has been passed. Suspected Id forgery.</exception>
        public override UsersDb ConvertToDatabaseType(User domainType, CRMDb db)
        {
            UsersDb userDb;

            //Existing Contact
            var UserId = default(int);
            int.TryParse(domainType.Id, out UserId);
            if (UserId > 0)
            {
                userDb = db.Users.Include(c => c.Addresses).Include(c => c.Communication).Include(c => c.Role).SingleOrDefault(c => c.UserID == UserId);
                var emails = db.AccountEmails.Where(a => a.UserID == UserId).ToList();
                userDb.Emails = emails;
                //userDb = db.Users.SingleOrDefault(c => c.UserID == domainType.Id);

                if (userDb == null)
                    throw new ArgumentException("Invalid user id has been passed. Suspected Id forgery.");

                userDb = Mapper.Map<User, UsersDb>(domainType, userDb);
            }
            else //New Contact
            {
                userDb = Mapper.Map<User, UsersDb>(domainType);

                // userDb.Emails = domainType.Email;
            }
            return userDb;
        }

        /// <summary>
        /// Converts to user setting domain.
        /// </summary>
        /// <param name="userSettingsDbObject">The user settings database object.</param>
        /// <returns></returns>
        public UserSettings ConvertToUserSettingDomain(UserSettingsDb userSettingsDbObject)
        {
            var userSettingsDb = getUserSettingsDb(userSettingsDbObject.AccountID, userSettingsDbObject.UserID);
            UserSettings userSettings = new UserSettings();
            Mapper.Map<UserSettingsDb, UserSettings>(userSettingsDb, userSettings);
            return userSettings;
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="userDbObject">The user database object.</param>
        /// <returns></returns>
        public override User ConvertToDomain(UsersDb userDbObject)
        {
            // var userDb = getUserDb(userDbObject.UserID);
            User user = new User();
            Mapper.Map<UsersDb, User>(userDbObject, user);
            return user;
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override User FindBy(int id)
        {
            UsersDb userDatabase = getUserDb(id);
            if (userDatabase != null)
            {
                User userDatabaseConvertedToDomain = ConvertToDomain(userDatabase);
                return userDatabaseConvertedToDomain;
            }
            return null;
        }

        /// <summary>
        /// Gets the user database.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        UsersDb getUserDb(int id)
        {
            var db = ObjectContextFactory.Create();

            var userSql = @"SELECT * FROM Users (NOLOCK) U 
                            INNER JOIN Communications (NOLOCK) C ON C.CommunicationID = U.CommunicationID
                            INNER JOIN Roles (NOLOCK) R ON R.RoleID = U.RoleID
                            INNER JOIN Accounts (NOLOCK) A ON A.AccountID = U.AccountID
                            WHERE U.UserID = @userId
                            SELECT A.* FROM Addresses (NOLOCK) A
                            INNER JOIN UserAddressMap (NOLOCK) UA ON UA.AddressID = A.AddressID
                            WHERE UA.UserID = @userId
                            SELECT * FROM AccountEmails (NOLOCK) AE 
                            WHERE AE.UserID = @userId
                            SELECT A.* FROM Users (NOLOCK) U
                            INNER JOIN AccountAddressMap (NOLOCK) AA ON AA.AccountID = U.AccountID
                            INNER JOIN Addresses (NOLOCK) A ON A.AddressID = AA.AddressID
                            WHERE UserID = @userId";

            var user = new UsersDb();
            db.GetMultiple(userSql, (r) =>
            {
                user = r.Read<UsersDb, CommunicationsDb, RolesDb, AccountsDb, UsersDb>((u, c, ro, a) =>
                {
                    u.Communication = c;
                    u.Role = ro;
                    u.Account = a;
                    return u;
                }, splitOn: "CommunicationID,RoleID,AccountID").FirstOrDefault();
                user.Addresses = r.Read<AddressesDb>().ToList();
                user.Emails = r.Read<AccountEmailsDb>().ToList();
                user.Account.Addresses = r.Read<AddressesDb>().ToList();
            }, new { userId = id });
            return user;
        }

        /// <summary>
        /// Gets the user settings database.
        /// </summary>
        /// <param name="AccountID">The account identifier.</param>
        /// <param name="UserID">The user identifier.</param>
        /// <returns></returns>
        UserSettingsDb getUserSettingsDb(int AccountID, int UserID)
        {
            var db = ObjectContextFactory.Create();
            UserSettingsDb data = db.UserSettings.SingleOrDefault(c => c.UserID == UserID);
            //var emails = db.Users.Include(n => n.Emails).Where(n => n.UserID == UserID && n.AccountID == AccountID).Select(i=>i.Emails).FirstOrDefault();
            //data.Emails = emails;
            return data;
        }

        /// <summary>
        /// Gets the user time zone.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="AccountId">The account identifier.</param>
        /// <returns></returns>
        public string GetUserTimeZone(int userId, int AccountId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"select coalesce(us.TimeZone,a.TimeZone) as TimeZone from users u (nolock) 	                      
                            left join UserSettings (nolock) us on u.UserID = us.UserID
                            left join accounts a (nolock) on a.AccountID = u.AccountID
	                        where u.userid = @userId and u.AccountID in (1, @accountId)";
            var userTimeZone = db.Get<string>(sql, new { userId = userId, accountId = AccountId }).FirstOrDefault();
            return userTimeZone;
        }

        /// <summary>
        /// Determines whether [is account admin exist] [the specified account identifier].
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public bool IsAccountAdminExist(int accountId)
        {
            var db = ObjectContextFactory.Create();
            return db.Roles.Where(r => r.AccountID == accountId).Join(db.RoleModules.Where(r => r.ModuleID == (byte)AppModules.AccountSettings),
                            r => r.RoleID, rm => rm.RoleID, (r, rm) => r).Any();
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="userDb">The user database.</param>
        /// <param name="db">The database.</param>
        public override void PersistValueObjects(User user, UsersDb userDb, CRMDb db)
        {
            persistAddresses(user, userDb, db);
            persistCommunication(user, userDb, db);
            persistEmails(user, userDb, db);
        }

        /// <summary>
        /// Persists the emails.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="userDb">The user database.</param>
        /// <param name="db">The database.</param>
        private void persistEmails(User user, UsersDb userDb, CRMDb db)
        {
            IEnumerable<AccountEmailsDb> emails = Mapper.Map<IEnumerable<Email>, IEnumerable<AccountEmailsDb>>(user.Emails);
            var UserId = default(int);
            int.TryParse(user.Id, out UserId);
            if (UserId > 0 && userDb.Emails != null)
            {
                var deletedEmails = userDb.Emails.Where(a => a.EmailID != 0 && !emails.Select(ad => ad.EmailID).Contains(a.EmailID)).ToList();
                foreach (AccountEmailsDb emailsDb in deletedEmails)
                {
                    // userDb.Emails.Remove(emailsDb);
                    db.Entry<AccountEmailsDb>(emailsDb).State = System.Data.Entity.EntityState.Deleted;
                }
            }
            else
            {
                AccountEmailsDb map = new AccountEmailsDb();
                map.UserID = userDb.UserID;
                map.Email = user.Email.EmailId;
                //  map.EmailStatus = (byte)email.EmailStatusValue;
                map.IsPrimary = user.Email.IsPrimary;
                map.AccountID = user.AccountID;
                db.AccountEmails.Add(map);
            }
        }

        /// <summary>
        /// Persists the addresses.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="userDb">The user database.</param>
        /// <param name="db">The database.</param>
        private void persistAddresses(User user, UsersDb userDb, CRMDb db)
        {
            IEnumerable<AddressesDb> addresses = Mapper.Map<IEnumerable<Address>, IEnumerable<AddressesDb>>(user.Addresses);
            var UserId = default(int);
            int.TryParse(user.Id, out UserId);
            if (UserId > 0 && userDb.Addresses != null)
            {
                var deletedAddresses = userDb.Addresses.Where(a => a.AddressID != 0 && !addresses.Select(ad => ad.AddressID).Contains(a.AddressID)).ToList();
                foreach (AddressesDb addressDb in deletedAddresses)
                {
                    userDb.Addresses.Remove(addressDb);
                }

            }
        }

        /// <summary>
        /// Persists the communication.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="userDb">The user database.</param>
        /// <param name="db">The database.</param>
        void persistCommunication(User user, UsersDb userDb, CRMDb db)
        {
            CommunicationsDb communicationDb;
            communicationDb = Mapper.Map<User, CommunicationsDb>(user);

            if (userDb.Communication != null)
            {
                communicationDb.CommunicationID = userDb.Communication.CommunicationID;
            }
            userDb.Communication = communicationDb;
        }

        /// <summary>
        /// Determines whether [is duplicate user] [the specified primary email].
        /// </summary>
        /// <param name="primaryEmail">The primary email.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public bool IsDuplicateUser(string primaryEmail, int userId, int accountId)
        {
            IQueryable<UsersDb> users = ObjectContextFactory.Create().Users.Where(c => c.AccountID == accountId && c.IsDeleted == false);

            if (userId > 0)
                users = users.Where(c => c.UserID != userId);

            users = users.Where(c => c.PrimaryEmail.Equals(primaryEmail));

            int count = users.Count();

            if (count > 0)
                return true;

            return false;
        }

        /// <summary>
        /// Updates the user status.
        /// </summary>
        /// <param name="userID">The user identifier.</param>
        /// <param name="Status">The status.</param>
        public void UpdateUserStatus(int[] userID, byte Status)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"update dbo.users
                        set status = @status
                        where userid in (select datavalue from dbo.split(@ids, ','))";
            db.Execute(sql, new { Ids = string.Join(",", userID), Status = Status });
        }

        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public string GetUserName(int userId)
        {
            var db = ObjectContextFactory.Create();
            var userName = db.Users.Where(u => u.UserID == userId).Select(u => u.FirstName + " " + u.LastName).FirstOrDefault();
            return userName;
        }

        /// <summary>
        /// Changes the role.
        /// </summary>
        /// <param name="roleId">The role identifier.</param>
        /// <param name="userID">The user identifier.</param>
        public void ChangeRole(short roleId, int[] userID)
        {
            var db = ObjectContextFactory.Create();
            foreach (int user in userID)
            {
                var userRecord = db.Users.Where(u => u.UserID == user).FirstOrDefault();
                userRecord.RoleID = roleId;
            }
            db.SaveChanges();
        }

        /// <summary>
        /// Deactivates the users.
        /// </summary>
        /// <param name="userID">The user identifier.</param>
        /// <param name="modifiedBy">The modified by.</param>
        public void DeactivateUsers(int[] userID, int modifiedBy)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<UsersDb> userRecords = db.Users.Where(u => userID.Contains(u.UserID)).ToArray();
            foreach (UsersDb user in userRecords)
            {
                user.IsDeleted = true;
                user.ModifiedBy = modifiedBy;
                user.ModifiedOn = DateTime.Now.ToUniversalTime();
            }
            db.SaveChanges();
        }

        /// <summary>
        /// Gets the roles.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Role> GetRoles(int accountId)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<RolesDb> rolesDb = db.Roles.Where(u => u.AccountID == accountId).ToList();
            if (rolesDb != null)
                return convertToDomainRole(rolesDb);
            return null;
        }

        /// <summary>
        /// Converts to domain role.
        /// </summary>
        /// <param name="roles">The roles.</param>
        /// <returns></returns>
        private IEnumerable<Role> convertToDomainRole(IEnumerable<RolesDb> roles)
        {
            foreach (RolesDb role in roles)
            {
                yield return new Role() { Id = role.RoleID, RoleName = role.RoleName };
            }
        }

        /// <summary>
        /// Finds the user setting by.
        /// </summary>
        /// <param name="AccountID">The account identifier.</param>
        /// <param name="UserID">The user identifier.</param>
        /// <returns></returns>
        public UserSettings FindUserSettingBy(int AccountID, int UserID)
        {
            var db = ObjectContextFactory.Create();
            UserSettingsDb usersettings = db.UserSettings.Where(i => i.UserID == UserID).SingleOrDefault();
            if (usersettings != null)
                return ConvertToUserSettingDomain(usersettings);
            return null;
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="status">The status.</param>
        /// <param name="role">The role.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="isSTAdmin">if set to <c>true</c> [is st admin].</param>
        /// <returns></returns>
        public IEnumerable<User> FindAll(string name, int limit, int pageNumber, byte status, short role, int accountId, bool isSTAdmin)
        {
            var predicate = PredicateBuilder.True<UsersDb>();
            var records = (pageNumber - 1) * limit;
            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                IEnumerable<short> Role = ObjectContextFactory.Create().Roles.Where(i => i.RoleName.Contains(name)).Select(i => i.RoleID).ToArray();

                var splitNameByEmptySpace = name.Split(' ');

                predicate = predicate.And(a => splitNameByEmptySpace.Contains(a.FirstName))
                                     .Or(a => splitNameByEmptySpace.Contains(a.LastName))
                                     .Or(a => a.LastName.Contains(name))
                                     .Or(a => a.FirstName.Contains(name))
                                     .Or(a => a.PrimaryEmail.Contains(name))
                                     .Or(a => Role.Contains(a.Role.RoleID));
                // .Or(a=> a.FirstName.Join(a.LastName));
                //predicate = predicate.And(a => a.LastName.Contains(name));
            }
            if (status != 0)
            {
                predicate = predicate.And(a => a.Status == (Status)status);
            }
            else if (role != 0)
            {
                predicate = predicate.And(a => a.Role.RoleID == role);
            }
            predicate = predicate.And(a => a.AccountID == accountId);

            IEnumerable<UsersDb> users = findUsersSummary(predicate, accountId, isSTAdmin).Skip(records).Take(limit);
            foreach (UsersDb da in users)
            {
                yield return ConvertToDomain(da);
            }
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="status">The status.</param>
        /// <param name="role">The role.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="isSTAdmin">if set to <c>true</c> [is st admin].</param>
        /// <returns></returns>
        public IEnumerable<User> FindAll(string name, byte status, short role, int accountId, bool isSTAdmin)
        {
            var predicate = PredicateBuilder.True<UsersDb>();
            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                IEnumerable<short> Role = ObjectContextFactory.Create().Roles.Where(i => i.RoleName.Contains(name)).Select(i => i.RoleID).ToArray();
                predicate = predicate.And(a => a.FirstName.Contains(name))
                                     .Or(a => a.LastName.Contains(name))
                                     .Or(a => a.PrimaryEmail.Contains(name))
                                     .Or(a => Role.Contains(a.Role.RoleID));
            }
            if (status != 0)
            {
                predicate = predicate.And(a => a.Status == (Status)status);
            }
            else if (role != 0)
            {
                predicate = predicate.And(a => a.RoleID == role);
            }
            predicate = predicate.And(a => a.AccountID == accountId);
            IEnumerable<UsersDb> users = findUsersSummary(predicate, accountId, isSTAdmin);
            foreach (UsersDb da in users)
            {
                yield return ConvertToDomain(da);
            }
        }

        /// <summary>
        /// Finds the users summary.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="isSTAdmin">if set to <c>true</c> [is st admin].</param>
        /// <returns></returns>
        IEnumerable<UsersDb> findUsersSummary(System.Linq.Expressions.Expression<Func<UsersDb, bool>> predicate, int accountId, bool isSTAdmin)
        {
            var db = ObjectContextFactory.Create();
            var accountAdmin = default(short);
            if (!isSTAdmin)
                accountAdmin = db.Roles.Where(r => r.AccountID == accountId)
                     .Join(db.RoleModules.Where(r => r.ModuleID == (byte)AppModules.AccountSettings), r => r.RoleID, rm => rm.RoleID, (o, i) => o)
                     .Select(s => s.RoleID).FirstOrDefault();

            IEnumerable<UsersDb> users = ObjectContextFactory.Create().Users.Include(c => c.Addresses).Include(c => c.Role).Include(c => c.Communication)
                .AsExpandable()
                .Where(predicate).Where(c => c.IsDeleted != true && c.RoleID != accountAdmin).OrderBy(c => c.PrimaryEmail).Select(a =>
                    new
                    {
                        AccountID = a.AccountID,
                        UserID = a.UserID,
                        FirstName = a.FirstName,
                        LastName = a.LastName,
                        PrimaryEmail = a.PrimaryEmail,
                        RoleID = a.RoleID,
                        Role = a.Role,
                        Status = a.Status
                    }).ToList().Select(x => new UsersDb
                    {
                        AccountID = x.AccountID,
                        UserID = x.UserID,
                        FirstName = x.FirstName,
                        LastName = x.LastName,
                        PrimaryEmail = x.PrimaryEmail,
                        RoleID = x.RoleID,
                        Role = x.Role,
                        Status = x.Status
                    });
            return users;
        }

        /// <summary>
        /// Inserts the user credentials.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="password">The password.</param>
        public void InsertUserCredentials(string email, string password)
        {
            var db = ObjectContextFactory.Create();
            var user = db.Users.Where(e => e.PrimaryEmail == email).SingleOrDefault();
            user.Password = password;
            db.SaveChanges();
        }

        /// <summary>
        /// Finds the by email and account identifier.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public User FindByEmailAndAccountId(string email, int accountId)
        {
            var db = ObjectContextFactory.Create();

            //UsersDb userDb = db.Users.Include(c => c.Role).Where(c => c.PrimaryEmail == email
            //    && (c.AccountID == accountId || c.AccountID == 1)
            //    && c.Status == Status.Active && c.IsDeleted == false).FirstOrDefault();

            var sql = @"select * from users u (nolock) 
                        inner join roles r (nolock) on u.roleid = r.roleid 
                        inner join accounts a (nolock) on a.accountid = u.accountid
                        inner join subscriptions s (nolock) on s.subscriptionid = a.subscriptionid
                        where (u.accountid = @id or u.accountid = 1) and lower(u.primaryemail) = lower(@email) and u.status = @status and u.isdeleted = 0
                        and u.password is not null
                        order by u.accountid desc";

            UsersDb userDb = db.Get<UsersDb, RolesDb, AccountsDb, SubscriptionsDb>(sql, (u, r, a, s) =>
                {
                    u.Role = r;
                    u.Account = a;
                    u.Account.Subscription = s;
                    return u;
                }, new { Id = accountId, Email = email, Status = Status.Active }, "RoleID,AccountID,SubscriptionID").FirstOrDefault();

            Role accountAdminRole = new Role();

            if (userDb != null)
            {

                if (userDb.AccountID == 1 && accountId != 1)
                {
                    var rsql = "select * from roles (nolock) r inner join rolemodulemap (nolock) rmp on rmp.roleid = r.roleid where rmp.moduleid = @moduleid and r.accountid = @id";
                    var accountAdminRoleDb = db.Get<RolesDb>(rsql, new { ModuleId = AppModules.AccountSettings, Id = accountId }).FirstOrDefault();
                    //var accountAdminRoleDb = db.Roles.Where(r => r.AccountID == accountId)
                    //    .Join(db.RoleModules.Where(r => r.ModuleID == (byte)AppModules.AccountSettings), r => r.RoleID, rm => rm.RoleID, (o, i) => o).FirstOrDefault();
                    if (accountAdminRole != null)
                        accountAdminRole = Mapper.Map<RolesDb, Role>(accountAdminRoleDb);
                }

                User userDatabaseConvertedToDomain = ConvertToDomain(userDb);
                if (userDb.AccountID == 1 && accountId == 1)
                {
                    userDatabaseConvertedToDomain.IsSTAdmin = true;
                }
                if (userDb.AccountID == 1 && accountId != 1 && accountAdminRole != null) //If incoming user is ST Admin logging into Account Site the make him Account's admin.
                {
                    userDatabaseConvertedToDomain.AccountID = accountId;
                    userDatabaseConvertedToDomain.RoleID = accountAdminRole.Id;
                    userDatabaseConvertedToDomain.Role = accountAdminRole;
                    userDatabaseConvertedToDomain.IsSTAdmin = true;
                }
                return userDatabaseConvertedToDomain;
            }
            return null;
        }

        /// <summary>
        /// Finds the by email and domain URL.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="domainUrl">The domain URL.</param>
        /// <returns></returns>
        public User FindByEmailAndDomainUrl(string email, string domainUrl)
        {
            Logger.Current.Verbose("Request received by UserRepository/FindByEmailAndDomainUrl method with parameters: " + email + ", " + domainUrl);

            var db = ObjectContextFactory.Create();
            AccountsDb accountDb = db.Accounts.Where(c => c.DomainURL.IndexOf(".") != -1
                        ? c.DomainURL.Substring(0, c.DomainURL.IndexOf(".")) == domainUrl
                        : c.DomainURL == domainUrl).FirstOrDefault();

            if (accountDb != null)
            {
                Logger.Current.Informational("Account Identified: " + accountDb.AccountID);
                UsersDb userDb = db.Users.Include(c => c.Role)
                    .Where(c => c.PrimaryEmail == email && c.IsDeleted == false
                        && c.Status == Status.Active && c.Account.DomainURL == accountDb.DomainURL).FirstOrDefault();
                if (userDb != null)
                {
                    Logger.Current.Informational("User Identified: " + userDb.UserID);
                    User userDatabaseConvertedToDomain = ConvertToDomain(userDb);
                    return userDatabaseConvertedToDomain;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds the user for login.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public User FindUserForLogin(int userId)
        {
            var db = ObjectContextFactory.Create();
            UsersDb userDb = db.Users.Include(c => c.Role).Include(c => c.Account).Where(c => c.UserID == userId && c.IsDeleted == false).FirstOrDefault();

            if (userDb != null)
            {
                User userDatabaseConvertedToDomain = ConvertToDomain(userDb);
                return userDatabaseConvertedToDomain;
            }
            return null;
        }

        /// <summary>
        /// Finds the by email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        public User FindByEmail(string email)
        {
            var db = ObjectContextFactory.Create();
            UsersDb userDb = db.Users.Include(c => c.Role).Where(c => c.PrimaryEmail == email && c.IsDeleted == false && c.Status == Status.Active).FirstOrDefault();
            if (userDb != null)
            {
                User userDatabaseConvertedToDomain = ConvertToDomain(userDb);
                return userDatabaseConvertedToDomain;
            }
            return null;
        }

        /// <summary>
        /// Finds the by email password.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public User FindByEmailPassword(string email, string password)
        {
            var db = ObjectContextFactory.Create();
            UsersDb userDb = db.Users.Include(c => c.Role).Include(c => c.Account)
                .Where(c => c.PrimaryEmail == email && c.IsDeleted == false && c.Password == password && c.Status == Status.Active).FirstOrDefault();
            User user = null;
            if (userDb != null)
            {
                user = ConvertToDomain(userDb);
                return user;
            }
            return user;
        }

        /// <summary>
        /// Gets the st admin list.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<int> GetSTAdminList()
        {
            var db = ObjectContextFactory.Create();
            return db.Users.Where(s => s.AccountID == 1 && s.IsDeleted == false && s.Status == Status.Active).Select(s => s.UserID);
        }

        /// <summary>
        /// Changes the password.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="oldPassword">The old password.</param>
        /// <param name="newPassword">The new password.</param>
        public void ChangePassword(string email, string oldPassword, string newPassword)
        {
            var db = ObjectContextFactory.Create();
            UsersDb userDb = db.Users.Where(e => e.PrimaryEmail == email && e.Password == oldPassword).FirstOrDefault();
            userDb.Password = newPassword;
            db.SaveChanges();
        }

        /// <summary>
        /// Gets all users.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<User> GetAllUsers()
        {
            var db = ObjectContextFactory.Create();
            var userDb = db.Users.Where(c => c.IsDeleted == false).AsQueryable();
            foreach (var user in userDb)
                yield return ConvertToDomain(user);
        }

        /// <summary>
        /// Gets the users.
        /// </summary>
        /// <param name="accountID">The account identifier.</param>
        /// <param name="isSTAdmin">if set to <c>true</c> [is st admin].</param>
        /// <returns></returns>
        IEnumerable<User> IUserRepository.GetUsers(int accountID, bool isSTAdmin)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<dynamic> usersDb;
            if (isSTAdmin == true)
                usersDb = db.Users.Where(c => c.Status == Status.Active && (c.AccountID == accountID || c.AccountID == 1) && c.IsDeleted == false).Select(u => new { UserID = u.UserID, FirstName = (u.FirstName ?? "") + " " + (u.LastName ?? "") + " ( " + (u.PrimaryEmail ?? "") + " ) " }).ToList();
            else
                usersDb = db.Users.Where(c => c.Status == Status.Active && c.AccountID == accountID && c.IsDeleted == false).Select(u => new { UserID = u.UserID, FirstName = (u.FirstName ?? "") + " " + (u.LastName ?? "") + " ( " + (u.PrimaryEmail ?? "") + " ) " }).ToList();
            if (usersDb != null)
                return ConvertToDomainUser(usersDb);
            return null;
        }

        /// <summary>
        /// Converts to domain user.
        /// </summary>
        /// <param name="users">The users.</param>
        /// <returns></returns>
        private IEnumerable<User> ConvertToDomainUser(IEnumerable<dynamic> users)
        {
            foreach (dynamic user in users)
            {
                yield return new User() { Id = Convert.ToString(user.UserID), FirstName = user.FirstName };
            }
        }

        /// <summary>
        /// Checks the password reset flag.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public bool CheckPasswordResetFlag(int userId, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var sql = "select * from dbo.users (nolock) where userid = @id and isdeleted = 0";
            var userDb = db.Get<UsersDb>(sql, new { Id = userId }).FirstOrDefault();  //Here also check for ResetFlag
            if (userDb != null && userDb.PasswordResetFlag == true)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Updates the password reset flag.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="status">if set to <c>true</c> [status].</param>
        public void UpdatePasswordResetFlag(int userId, bool status)
        {
            var db = ObjectContextFactory.Create();
            UsersDb userDb = db.Users.Where(e => e.UserID == userId && e.IsDeleted == false).FirstOrDefault();
            if (userDb != null)
            {
                userDb.PasswordResetFlag = status;
                userDb.PasswordResetOn = DateTime.Now.ToUniversalTime();
            }
            db.SaveChanges();
        }

        /// <summary>
        /// Gets the previous passwords.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="limit">The limit.</param>
        /// <returns></returns>
        public List<string> GetPreviousPasswords(int userId, int limit)
        {
            var db = ObjectContextFactory.Create();
            var previousPasswords = db.UserProfileAudit.Where(u => u.UserID == userId && u.UserAuditTypeID == (byte)UserAuditType.PasswordChange).OrderByDescending(u => u.AuditedOn)
                                    .Select(s => s.Password).Take(limit);
            return previousPasswords.ToList();
        }

        /// <summary>
        /// Updates the password history.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="newPassword">The new password.</param>
        public void UpdatePasswordHistory(int userId, string newPassword)
        {
            var db = ObjectContextFactory.Create();
            if (newPassword != null && userId > 0)
            {
                UserProfileAudit profileAudit = new UserProfileAudit()
                {
                    UserID = userId,
                    UserAuditTypeID = (byte)UserAuditType.PasswordChange,
                    AuditedOn = DateTime.Now.ToUniversalTime(),
                    Password = newPassword,
                    AuditedBy = userId
                };
                db.UserProfileAudit.Add(profileAudit);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Inserts the login audit.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="IP">The ip.</param>
        /// <param name="activity">The activity.</param>
        public void InsertLoginAudit(int userId, int accountId, string IP, SignInActivity activity)
        {
            if (IP != null)
            {
                var db = ObjectContextFactory.Create();
                var sql = @"insert into loginaudit values(@userid, @activity, @ip,  @auditon, @accountId)";
                db.Execute(sql, new { UserId = userId, Ip = IP, Activity = (byte)activity, AuditOn = DateTime.Now.ToUniversalTime(), accountId = accountId });
            }
        }

        /// <summary>
        /// Inserts the user profile audit.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="auditType">Type of the audit.</param>
        /// <param name="auditedBy">The audited by.</param>
        /// <param name="password">The password.</param>
        public void InsertUserProfileAudit(int userId, UserAuditType auditType, int auditedBy, string password)
        {
            var db = ObjectContextFactory.Create();
            UserProfileAudit profileAudit = new UserProfileAudit()
            {
                UserID = userId,
                UserAuditTypeID = (byte)auditType,
                AuditedOn = DateTime.Now.ToUniversalTime(),
                Password = password,
                AuditedBy = auditedBy
            };
            db.UserProfileAudit.Add(profileAudit);
            db.SaveChanges();
        }

        # region Notifications

        public IEnumerable<Notification> GetImpendingReminderNotifications(IEnumerable<int> userIds)
        {
            List<int> moduleIds = new List<int>();
            moduleIds.Add((int)AppModules.ContactActions);
            moduleIds.Add((int)AppModules.OpportunityActions);
            moduleIds.Add((int)AppModules.ContactTours);
            moduleIds.Add((int)AppModules.Accounts);
            moduleIds.Add((int)AppModules.Opportunity);
            moduleIds.Add((int)AppModules.LeadAdapter);
            moduleIds.Add((int)AppModules.ImportData);
            moduleIds.Add((int)AppModules.Campaigns);
            moduleIds.Add((int)AppModules.Contacts);
            moduleIds.Add((int)AppModules.Download);
            moduleIds.Add((int)AppModules.LitmusTest);
            moduleIds.Add((int)AppModules.MailTester);

            return getNotifications(userIds, NotificationStatus.New, true, moduleIds, true);
        }

        public IEnumerable<Notification> GetImpendingReminderWebVisitNotifications(IEnumerable<int> userIds, int accountId)
        {
            IEnumerable<Notification> notifications = new List<Notification>();
            var db = ObjectContextFactory.Create();
            var userAccount = db.Users.Where(c => userIds.Contains(c.UserID)).Select(c => c.AccountID).FirstOrDefault();
            var visiStat = db.WebAnalyticsProviders.Where(c => c.AccountID == userAccount).FirstOrDefault();
            if (visiStat != null && visiStat.NotificationStatus == true)
            {
                notifications = getWebVisitNotifications(userIds, NotificationStatus.New, true, new List<int>(), true);
            }
            return notifications;
        }

        public IEnumerable<Notification> GetReminderNotifications(IEnumerable<int> userIds, IEnumerable<int> moduleIds, bool todayNotifications)
        {
            return getNotifications(userIds, NotificationStatus.New, false, moduleIds, todayNotifications); //.Union(GetViewedNotifications(userIds)).OrderBy(o => (byte)o.Status);
            // return getNotifications(userIds, NotificationStatus.New, false);
        }

        public IEnumerable<Notification> GetViewedNotifications(IEnumerable<int> userIds, IEnumerable<int> moduleIds, bool todayNotifications)      //Called when previously read is clicked
        {
            return getNotifications(userIds, NotificationStatus.Viewed, false, moduleIds, todayNotifications);
        }

        IEnumerable<Notification> getNotifications(IEnumerable<int> userIds, NotificationStatus notificationStatus, bool impending, IEnumerable<int> moduleIds, bool todayNotifications)
        {
            Dictionary<int, Notification> allNotifications = new Dictionary<int, Notification>();
            using (var con = ObjectContextFactory.Create())
            {
                con.Get<Notification, ContactNotificationEntry>("dbo.GetNotifications", (n, c) =>
                {
                    if (!allNotifications.ContainsKey(n.NotificationID))
                    {
                        try
                        {
                            if (n.ModuleID == (byte)AppModules.Opportunity)
                            {
                                if (n.OpportunityEntries == null)
                                    n.OpportunityEntries = new List<OpportunityNotificationEntry>();

                                if (c != null)
                                    n.OpportunityEntries.Add(new OpportunityNotificationEntry()
                        {
                            OpportunityId = c.ContactID,
                            OpportunityName = c.FullName
                        });
                            }
                            else
                            {
                                if (n.ContactEntries == null)
                                    n.ContactEntries = new List<ContactNotificationEntry>();

                                if (c != null)
                                    n.ContactEntries.Add(c);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Current.Error("Error while calculating impending notifications", ex);
                        }


                        allNotifications.Add(n.NotificationID, n);
                    }
                    return n;
                }, new
                {
                    UserIds = string.Join(",", userIds.ToArray()),
                    Impending = impending,
                    IsToday = todayNotifications,
                    Modules = string.Join(",", moduleIds.ToArray()),
                    NotificationStatus = notificationStatus,
                    CountsOnly = 0
                },
                splitOn: "ContactID",
                commandType: System.Data.CommandType.StoredProcedure);
            }
            return allNotifications.Select(v => v.Value);
        }

        IEnumerable<Notification> getWebVisitNotifications(IEnumerable<int> userIds, NotificationStatus notificationStatus, bool impending, IEnumerable<int> moduleIds, bool todayNotifications)
        {
            var db = ObjectContextFactory.Create();

            DateTime currentUtc = DateTime.UtcNow.ToUniversalTime().AddMinutes(-4);
            DateTime withInMinuteUtc = DateTime.UtcNow.ToUniversalTime().AddMinutes(1);
            Logger.Current.Verbose("Getting web notifications between " + currentUtc.ToString() + " and " + withInMinuteUtc.ToString() + " for userId: " + userIds.FirstOrDefault());

            var notificationsSql = @"SELECT COUNT(*) FROM Notifications(NOLOCK) WHERE ModuleID = 35 and NotificationTime <= @withInMinuteUtc AND NotificationTime > @currentUtc AND Status = 1 AND 
                                     UserID IN (SELECT DATAVALUE FROM dbo.Split(@userIds,','))";
            var newNotificationsDb = db.Get<NotificationDb>(notificationsSql, new { withInMinuteUtc = withInMinuteUtc, currentUtc = currentUtc, userIds = string.Join(",", userIds) }).ToList();

            var newNotications = Mapper.Map<IEnumerable<NotificationDb>, IEnumerable<Notification>>(newNotificationsDb);
            return newNotications.OrderByDescending(n => n.Time);
        }

        public int GetNotificationsCount(int userId, NotificationStatus notificationStatus, IEnumerable<byte> moduleIds)
        {
            var db = ObjectContextFactory.Create();
            DateTime currentUtc = DateTime.UtcNow.ToUniversalTime();
            int actionsCount = 0;
            int toursCount = 0;

            if (moduleIds.Contains((byte)AppModules.ContactActions) || moduleIds.Contains((byte)AppModules.OpportunityActions))
            {
                var actionSql = @"SELECT COUNT(*) FROM Actions(NOLOCK) WHERE CreatedBy = @UserId AND RemindbyPopup = 1 AND NotificationStatus = @notificationStatus AND RemindOn < @currentUtc";
                actionsCount = db.Get<int>(actionSql, new { UserId = userId, notificationStatus = (byte)notificationStatus, currentUtc = currentUtc }).FirstOrDefault();
            }

            if (moduleIds.Contains((byte)AppModules.ContactTours))
            {
                var tourSql = @"SELECT COUNT(*) FROM Tours(NOLOCK) WHERE CreatedBy = @UserId AND RemindbyPopup = 1 AND NotificationStatus = @notificationStatus AND ReminderDate < @currentUtc";
                toursCount = db.Get<int>(tourSql, new { UserId = userId, notificationStatus = (byte)notificationStatus, currentUtc = currentUtc }).FirstOrDefault();
            }
            var notificationsSql = @"SELECT COUNT(*) FROM Notifications(NOLOCK) WHERE UserID = @UserId AND Status = @notificationStatus AND ModuleID != 35 AND ModuleID IN (SELECT DATAVALUE FROM dbo.Split(@moduleIds,','))";
            int domainNotifications = db.Get<int>(notificationsSql, new { UserId = userId, notificationStatus = (byte)notificationStatus, moduleIds = string.Join(",", moduleIds) }).FirstOrDefault();

            return actionsCount + toursCount + domainNotifications;
        }

        public IEnumerable<int> GetWebVisitNotificationsCount(int userId, int accountId, NotificationStatus notificationStatus)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"	 select n.NotificationID from notifications n (nolock) 
	                        join users u (nolock) on n.UserID = u.UserID
							join ContactWebVisits cw (nolock) on n.EntityID = cw.ContactWebVisitID
							join contacts c  (nolock) on cw.ContactID = c.ContactID
	                        where n.userid = @userId 
								and n.status = 1 and n.moduleid = 35 and c.AccountID = @accountId and c.IsDeleted = 0
								 order by n.NotificationID";
            var notificationIds = db.Get<int>(sql, new { userId = userId, accountId = accountId, status = notificationStatus });
            return notificationIds;
        }

        public NotificationsCount GetNotificationsCountByDate(int userId, IEnumerable<byte> moduleIds, NotificationStatus status)
        {
            NotificationsCount notificationsCount = new NotificationsCount();
            var allTodayNotifications = 0;
            var allPreviousNotifications = 0;

            Func<IEnumerable<NotificationResult>, List<int>> GetNotificationCounts = (nr) =>
             {
                 int today = 0;
                 int previous = 0;

                 if (nr.IsAny())
                 {
                     today = nr.Where(w => w.IsToday).Select(s => s.NotificationCount).FirstOrDefault();
                     previous = nr.Where(w => !w.IsToday).Select(s => s.NotificationCount).FirstOrDefault();
                     allTodayNotifications = allTodayNotifications + today;
                     allPreviousNotifications = allPreviousNotifications + previous;
                 }
                 return new List<int>()
                {
                    today, previous
                };

             };
            using (var con = ObjectContextFactory.Create())
            {
                con.QueryStoredProc("dbo.GetNotifications", (r) =>
                {
                    var counts = r.Read<NotificationResult>();
                    foreach (var module in moduleIds)
                    {
                        IEnumerable<NotificationResult> notificationResult = counts.Where(w => w.ModuleID == module);
                        var nCount = GetNotificationCounts(notificationResult);
                        switch (module)
                        {
                            case 1:
                                notificationsCount.AccountNotifications = nCount;
                                break;
                            case 5:
                                notificationsCount.ActionNotifications = nCount;
                                break;
                            case 7:
                                notificationsCount.TourNotifications = nCount;
                                break;
                            case 3:
                                notificationsCount.ContactNotifications = nCount;
                                break;
                            case 16:
                                notificationsCount.OpportunityNotifications = nCount;
                                break;
                            case 19:
                                notificationsCount.LeadAdapterNotifications = nCount;
                                break;
                            case 23:
                                notificationsCount.ImportNotifications = nCount;
                                break;
                            case 4:
                                notificationsCount.CampaignNotifications = nCount;
                                break;
                            case 72:
                                notificationsCount.DownloadNotifications = nCount;
                                break;
                            case 77:
                                notificationsCount.CampaignLitmusNotifications = nCount;
                                break;
                            case 79:
                                notificationsCount.MailTesterNotifications = nCount;
                                break;
                        }
                    }
                }, new
                {
                    UserIds = userId.ToString(),
                    Impending = 1,
                    IsToday = 0,
                    Modules = string.Join(",", moduleIds),
                    NotificationStatus = NotificationStatus.New,
                    CountsOnly = 1
                });
            }

            notificationsCount.NotificationsByDate = new List<int>()
            {
                    allTodayNotifications, allPreviousNotifications
            };

            return notificationsCount;
        }

        public Notification AddNotification(Notification notification)
        {
            var db = ObjectContextFactory.Create();
            NotificationDb notificationDb = Mapper.Map<Notification, NotificationDb>(notification);
            db.Notifications.Add(notificationDb);
            db.SaveChanges();
            notification.NotificationID = notificationDb.NotificationID;
            return notification;
        }

        public void AddNotification(List<Notification> notifications)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<NotificationDb> notificationDb = Mapper.Map<IEnumerable<Notification>, IEnumerable<NotificationDb>>(notifications);
            db.Notifications.AddRange(notificationDb);
            db.SaveChanges();

        }

        public void AddBulkNotifications(Notification notification)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<int> STAdminIds = GetSTAdminList();
            STAdminIds.ForEach(st =>
            {
                notification.UserID = st;
                NotificationDb notificationDb = Mapper.Map<Notification, NotificationDb>(notification);
                db.Notifications.Add(notificationDb);
            });
            db.SaveChanges();
        }

        public void UpdateNotification(Notification notification)
        {
            var db = ObjectContextFactory.Create();
            NotificationDb notificationDb = db.Notifications.SingleOrDefault(n => n.NotificationID == notification.NotificationID);
            //notificationDb = Mapper.Map<Notification, NotificationDb>(notification, notificationDb);
            notificationDb.Status = NotificationStatus.Viewed;
            db.SaveChanges();
        }

        public int DeleteNotification(IEnumerable<int> notificationIds, int userId, byte moduleId, bool ArePreviousNotifications, bool bulkRemove)
        {
            var db = ObjectContextFactory.Create();
            int totalDeleted = 0;
            if (!ArePreviousNotifications)         //today
            {
                DateTime today = DateTime.UtcNow;
                if (moduleId == 5)
                {
                    if (!bulkRemove && notificationIds != null && notificationIds.Any())
                    {
                        var action = db.Actions.Where(a => notificationIds.Contains(a.ActionID) && a.RemindbyPopup.HasValue && a.RemindbyPopup.Value && a.CreatedBy == userId && a.RemindOn < today).FirstOrDefault();
                        if (action != null)
                            action.RemindbyPopup = false;
                        totalDeleted += 1;
                    }
                    else
                    {
                        var actions = db.Actions.Where(a => a.CreatedBy == userId && a.RemindbyPopup.HasValue && a.RemindbyPopup.Value && a.RemindOn < today).ToList();
                        if (actions != null && actions.Any())
                        {
                            actions.ForEach(f => { f.RemindbyPopup = false; });
                            totalDeleted += actions.Count();
                        }
                    }
                }
                else if (moduleId == 7)
                {
                    if (!bulkRemove && notificationIds != null && notificationIds.Any())
                    {
                        var tour = db.Tours.Where(a => notificationIds.Contains(a.TourID) && a.RemindbyPopup.HasValue && a.RemindbyPopup.Value && a.CreatedBy == userId && a.ReminderDate < today).FirstOrDefault();
                        if (tour != null)
                            tour.RemindbyPopup = false;
                        totalDeleted += 1;
                    }
                    else
                    {
                        var tours = db.Tours.Where(a => a.CreatedBy == userId && a.RemindbyPopup.HasValue && a.RemindbyPopup.Value && a.ReminderDate < today).ToList();
                        if (tours != null && tours.Any())
                        {
                            tours.ForEach(f => { f.RemindbyPopup = false; });
                            totalDeleted += tours.Count();
                        }
                    }
                }
                else
                {
                    if (!bulkRemove && notificationIds != null && notificationIds.Count() > 0)
                    {
                        var notification = db.Notifications.Where(w => notificationIds.Contains(w.NotificationID) && w.UserID == userId && w.NotificationTime < today && w.ModuleID == moduleId && w.ModuleID != (byte)AppModules.WebAnalytics)
                            .FirstOrDefault();
                        if (notification != null)
                            db.Notifications.Remove(notification);
                        totalDeleted += 1;
                    }
                    else
                    {
                        var notifications = db.Notifications.Where(w => w.UserID == userId && w.NotificationTime < today && w.ModuleID == moduleId && w.ModuleID != (byte)AppModules.WebAnalytics).ToList();
                        if (notifications != null && notifications.Any())
                            db.Notifications.RemoveRange(notifications);
                        totalDeleted += notifications.Count;
                    }

                }
                db.SaveChanges();
            }
            else
            {
                DateTime today = DateTime.UtcNow;
                if (moduleId == 5)
                {
                    if (!bulkRemove && notificationIds != null && notificationIds.Any())
                    {
                        var action = db.Actions.Where(a => notificationIds.Contains(a.ActionID) && a.RemindbyPopup.HasValue && a.RemindbyPopup.Value && a.CreatedBy == userId && a.RemindOn < today).FirstOrDefault();
                        if (action != null)
                            action.RemindbyPopup = false;
                        totalDeleted += 1;
                    }
                    else
                    {
                        var actions = db.Actions.Where(a => a.CreatedBy == userId && a.RemindbyPopup.HasValue && a.RemindbyPopup.Value && a.RemindOn < today).ToList();
                        if (actions != null && actions.Any())
                        {
                            actions.ForEach(f => { f.RemindbyPopup = false; });
                            totalDeleted += actions.Count();
                        }
                    }
                }
                else if (moduleId == 7)
                {
                    if (!bulkRemove && notificationIds != null && notificationIds.Any())
                    {
                        var tour = db.Tours.Where(a => notificationIds.Contains(a.TourID) && a.RemindbyPopup.HasValue && a.RemindbyPopup.Value && a.CreatedBy == userId && a.ReminderDate < today).FirstOrDefault();
                        if (tour != null)
                            tour.RemindbyPopup = false;
                        totalDeleted += 1;
                    }
                    else
                    {
                        var tours = db.Tours.Where(a => a.CreatedBy == userId && a.RemindbyPopup.HasValue && a.RemindbyPopup.Value && a.ReminderDate < today).ToList();
                        if (tours != null && tours.Any())
                        {
                            tours.ForEach(f => { f.RemindbyPopup = false; });
                            totalDeleted += tours.Count();
                        }
                    }
                }
                else
                {
                    if (!bulkRemove && notificationIds != null && notificationIds.Any())
                    {
                        var notification = db.Notifications.Where(w => notificationIds.Contains(w.NotificationID) && w.UserID == userId && w.NotificationTime < today && w.ModuleID == moduleId && w.ModuleID != (byte)AppModules.WebAnalytics)
                            .FirstOrDefault();
                        if (notification != null)
                            db.Notifications.Remove(notification);
                        totalDeleted += 1;
                    }
                    else
                    {
                        var notifications = db.Notifications.Where(w => w.UserID == userId && w.NotificationTime < today && w.ModuleID == moduleId && w.ModuleID != (byte)AppModules.WebAnalytics).ToList();
                        if (notifications != null && notifications.Any())
                            db.Notifications.RemoveRange(notifications);
                        totalDeleted += notifications.Count;
                    }
                }
                db.SaveChanges();
            }
            return totalDeleted;
        }

        public int DeleteBulkNotifications(int userId, bool ArePreviousNotifications, IEnumerable<byte> moduleIds)
        {
            int DeletedCount = 0;
            var today = DateTime.UtcNow.Date;
            var db = ObjectContextFactory.Create();

            if (moduleIds.Contains((byte)AppModules.ContactActions) || moduleIds.Contains((byte)AppModules.OpportunityActions))
            {
                var actions = new List<ActionsDb>();
                if (!ArePreviousNotifications)
                    actions = db.Actions.Where(a => a.CreatedBy == userId && a.RemindbyPopup.HasValue && a.RemindbyPopup.Value && a.RemindOn > today).ToList();
                else
                    actions = db.Actions.Where(a => a.CreatedBy == userId && a.RemindbyPopup.HasValue && a.RemindbyPopup.Value && a.RemindOn < today).ToList();
                if (actions != null && actions.Any())
                {
                    actions.ForEach(f => { f.RemindbyPopup = false; });
                    DeletedCount += actions.Count();
                }
            }
            if (moduleIds.Contains((byte)AppModules.ContactTours))
            {
                var tours = new List<TourDb>();
                if (!ArePreviousNotifications)
                    tours = db.Tours.Where(a => a.CreatedBy == userId && a.RemindbyPopup.HasValue && a.RemindbyPopup.Value && a.ReminderDate > today).ToList();
                else
                    tours = db.Tours.Where(a => a.CreatedBy == userId && a.RemindbyPopup.HasValue && a.RemindbyPopup.Value && a.ReminderDate < today).ToList();
                if (tours != null && tours.Any())
                {
                    tours.ForEach(f => { f.RemindbyPopup = false; });
                    DeletedCount += tours.Count();
                }
            }
            var notifications = new List<NotificationDb>();
            if (!ArePreviousNotifications)
                notifications = db.Notifications.Where(a => a.UserID == userId && a.NotificationTime > today && a.ModuleID != (byte)AppModules.WebAnalytics).ToList();
            else
                notifications = db.Notifications.Where(a => a.UserID == userId && a.NotificationTime < today && a.ModuleID != (byte)AppModules.WebAnalytics).ToList();
            if (notifications != null && notifications.Any())
            {
                db.Notifications.RemoveRange(notifications);
                DeletedCount += notifications.Count();
            }
            db.SaveChanges();

            return DeletedCount;
        }

        #endregion

        /// <summary>
        /// Gets the user shallow.
        /// </summary>
        /// <param name="accountID">The account identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<User> GetUserShallow(int accountID)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the user calender.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public List<CalenderTimeSlot> GetUserCalender(int accountId, int? userId, DateTime startDate, DateTime endDate)
        {
            var db = ObjectContextFactory.Create();
            List<CalenderTimeSlot> calender = new List<CalenderTimeSlot>();
            startDate = startDate.AddMonths(-1);
            endDate = endDate.AddMonths(1);
            var sql = @"SELECT T.TourDate as Start, T.TourDate as [End], ('(T) ' + COALESCE(T.TourDetails,'')) AS [Title] FROM Tours (NOLOCK) T
                            INNER JOIN UserTourMap (NOLOCK) U ON U.TourID = T.TourID
                            WHERE U.UserID = @UserID AND T.AccountID = @AccountID AND T.TourDate BETWEEN @StartDate AND @EndDate
                            UNION ALL
                            SELECT A.RemindOn as [Start], A.RemindOn as [End], ('(R) ' + COALESCE(A.ActionDetails,'')) AS [Title]  FROM Actions (NOLOCK) A
                            INNER JOIN UserActionMap (NOLOCK) UAM ON UAM.ActionID = A.ActionID
                            WHERE A.RemindOn BETWEEN @StartDate AND @EndDate AND A.AccountID = @AccountID AND UAM.UserID = @UserID
                            UNION ALL
                            SELECT C.ScheduleTime AS Start, C.ScheduleTime as [End], ('(C) '+ COALESCE(C.Name,''))  AS [Title] FROM Campaigns (NOLOCK) C
                            WHERE C.ScheduleTime BETWEEN @StartDate AND @EndDate AND C.AccountID = @AccountID AND C.CreatedBy = @UserID AND C.CampaignStatusID = 102 AND C.IsDeleted = 0";

            calender = db.Get<CalenderTimeSlot>(sql, new { StartDate = startDate, EndDate = endDate, AccountID = accountId, UserID = userId }).ToList();
            return calender;
        }

        /// <summary>
        /// Updates the facebook access token.
        /// </summary>
        /// <param name="uid">The uid.</param>
        /// <param name="accessToken">The access token.</param>
        public void UpdateFacebookAccessToken(int uid, string accessToken)
        {
            using (var db = new CRMDb())
            {
                var user = db.Users.Where(u => u.UserID == uid).SingleOrDefault();
                var communication = db.Communications.Where(c => c.CommunicationID == user.CommunicationID.Value).SingleOrDefault();
                db.Entry(communication).State = System.Data.Entity.EntityState.Modified;
                communication.FacebookAccessToken = accessToken;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Updates the twitter o authentication tokens.
        /// </summary>
        /// <param name="uid">The uid.</param>
        /// <param name="token">The token.</param>
        /// <param name="tokenSecret">The token secret.</param>
        public void UpdateTwitterOAuthTokens(int uid, string token, string tokenSecret)
        {
            using (var db = new CRMDb())
            {
                var user = db.Users.Where(u => u.UserID == uid).SingleOrDefault();
                var communication = db.Communications.Where(c => c.CommunicationID == user.CommunicationID.Value).SingleOrDefault();
                db.Entry(communication).State = System.Data.Entity.EntityState.Modified;
                communication.TwitterOAuthToken = token;
                communication.TwitterOAuthTokenSecret = tokenSecret;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the users.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="isAdmin">if set to <c>true</c> [is admin].</param>
        /// <returns></returns>
        public IEnumerable<Owner> GetUsers(int accountId, int userId, bool isAdmin)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<UsersDb> userDb;

            if (isAdmin)
                userDb = db.Users.Where(p => p.IsDeleted == false && (p.AccountID == accountId || p.AccountID == 1) && p.Status == Status.Active).ToList();
            else
                userDb = db.Users.Where(p => p.IsDeleted == false && p.AccountID == accountId && p.Status == Status.Active && p.UserID == userId).ToList();

            if (userDb != null)
                return convertToDomainUsers(userDb);
            return null;
        }

        /// <summary>
        /// Converts to domain users.
        /// </summary>
        /// <param name="users">The users.</param>
        /// <returns></returns>
        IEnumerable<Owner> convertToDomainUsers(IEnumerable<UsersDb> users)
        {
            foreach (UsersDb user in users)
            {
                yield return new Owner() { OwnerId = user.UserID, OwnerName = user.FirstName + " " + user.LastName + " (" + user.PrimaryEmail + ") " };
            }
        }

        /// <summary>
        /// Determines whether [is from email valid] [the specified email].
        /// </summary>
        /// <param name="Email">The email.</param>
        /// <param name="UserID">The user identifier.</param>
        /// <returns></returns>
        public bool isFromEmailValid(string Email, int UserID)
        {
            using (var db = ObjectContextFactory.Create())
            {
                return db.AccountEmails.Where(i => i.UserID == UserID && i.Email == Email).Count() > 0;
            }
        }

        /// <summary>
        /// Updates the user reset password.
        /// </summary>
        /// <param name="userIds">The user ids.</param>
        /// <param name="password">The password.</param>
        /// <param name="userID">The user identifier.</param>
        public void UpdateUserResetPassword(int[] userIds, string password, int? userID)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<UsersDb> userDb = db.Users.ToList().Where(u => userIds.Contains(u.UserID));
            foreach (var user in userDb)
            {
                user.Password = password;
                user.Status = Status.Active;
                user.PasswordResetFlag = false;
                user.PasswordResetOn = null;
                InsertUserProfileAudit(user.UserID, UserAuditType.PasswordChange, (int)userID.Value, user.Password);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the users count.
        /// </summary>
        /// <param name="AccountID">The account identifier.</param>
        /// <returns></returns>
        public int GetUsersLimit(int AccountID)
        {
            using (var db = ObjectContextFactory.Create())
            {
                string excludedRoles = db.SubscriptionModules.Where(w => w.AccountID == AccountID && w.ModuleID == (byte)AppModules.Users).Select(s => s.ExcludedRoles).FirstOrDefault();
                if (!string.IsNullOrEmpty(excludedRoles))
                {
                    IEnumerable<short> roles = excludedRoles.Split(',').Select(s => short.Parse(s));
                    return db.Users.Where(i => i.AccountID == AccountID && i.IsDeleted == false && i.Status == Status.Active && !roles.Contains(i.RoleID)).Count();
                }
                else
                    return db.Users.Where(i => i.AccountID == AccountID && i.IsDeleted == false && i.Status == Status.Active).Count();
            }
        }

        public bool IsRoleExcludedFromLimit(int accountId, short roleId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                string excludedRoles = db.SubscriptionModules.Where(w => w.AccountID == accountId && w.ModuleID == (byte)AppModules.Users).Select(s => s.ExcludedRoles).FirstOrDefault();
                if (!string.IsNullOrEmpty(excludedRoles))
                {
                    IEnumerable<short> roles = excludedRoles.Split(',').Select(s => short.Parse(s));
                    return roles.Any(w => w == roleId);
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// Determines whether [is account admin exists in users] [the specified user ids].
        /// </summary>
        /// <param name="userIds">The user ids.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public bool IsAccountAdminExistsInUsers(int[] userIds, int accountId)
        {
            var db = ObjectContextFactory.Create();

            bool isAccountAdminExists = db.Users.Join(db.Roles, u => u.RoleID, r => r.RoleID, (u, r) => new { u.RoleID, u.UserID, u.AccountID, r.RoleName, u.IsDeleted, u.Status }).
                                    Where(p => p.AccountID == accountId && !userIds.Contains(p.UserID) && p.RoleName == "Account Administrator" && p.IsDeleted == false && p.Status == Status.Active).Count() > 0;
            return isAccountAdminExists;
        }

        /// <summary>
        /// Determines whether [is account admin exists in active users] [the specified user ids].
        /// </summary>
        /// <param name="userIds">The user ids.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public bool IsAccountAdminExistsInActiveUsers(int[] userIds, int accountId)
        {
            var db = ObjectContextFactory.Create();

            bool isAccountAdminExists = db.Users.Join(db.Roles, u => u.RoleID, r => r.RoleID, (u, r) => new { u.RoleID, u.UserID, u.AccountID, r.RoleName, u.IsDeleted, u.Status }).
                                    Where(p => p.AccountID == accountId && !userIds.Contains(p.UserID) && p.RoleName == "Account Administrator" && p.IsDeleted == false && p.Status == Status.Active).Count() > 0;
            return isAccountAdminExists;
        }

        /// <summary>
        /// Gets the users by user i ds.
        /// </summary>
        /// <param name="userIds">The user ids.</param>
        /// <returns></returns>
        public IEnumerable<UserBasicInfo> GetUsersByUserIDs(IEnumerable<int?> userIds)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"select u.UserID,u.AccountID, u.FirstName, u.LastName, ae.Email Email, coalesce(us.TimeZone,a.TimeZone) as TimeZone, a.AccountName from users u (nolock) 
                            inner join accounts a (nolock) on a.AccountID = u.AccountID
	                        left join accountemails ae (nolock) on u.UserID = ae.UserID and ae.isprimary = 1 
                            left join UserSettings us (nolock) on u.UserID = us.UserID
	                        where u.userid in (select datavalue from dbo.split(@userIds, ',')) and u.status=1 and u.isdeleted = 0";
                return db.Get<UserBasicInfo>(sql, new { userIds = string.Join(",", userIds) }).ToList();
            }
        }

        /// <summary>
        /// Gets the users opted instant web visit email.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<UserBasicInfo> GetUsersOptedInstantWebVisitEmail(int accountId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"select u.UserID,u.AccountID, u.FirstName, u.LastName, ae.Email Email, coalesce(us.TimeZone,a.TimeZone) as TimeZone
	                            from WebVisitUserNotificationMap np (nolock) 
	                            left join users u (nolock) on  np.UserID = u.UserID
	                            left join accountemails ae (nolock) on u.UserID = ae.UserID
                                left join UserSettings us (nolock) on u.UserID = us.UserID
                                left join accounts a (nolock) on a.AccountID = u.AccountID
	                            where np.AccountID = @accountId and ae.isprimary = 1 and np.NotificationType = 501 and u.status=1 and u.isdeleted = 0";
                return db.Get<UserBasicInfo>(sql, new { accountId = accountId }).ToList();
            }
        }

        /// <summary>
        /// Gets the users opted web visit summary email.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<UserBasicInfo> GetUsersOptedWebVisitSummaryEmail(int accountId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"select u.UserID,u.AccountID, u.FirstName, u.LastName, ae.Email Email, coalesce(us.TimeZone,a.TimeZone) as TimeZone, a.AccountName 
	                            from WebVisitUserNotificationMap np (nolock) 
	                            left join users u (nolock) on  np.UserID = u.UserID
	                            left join accountemails ae (nolock) on u.UserID = ae.UserID
                                left join UserSettings us (nolock) on u.UserID = us.UserID
                                left join accounts a (nolock) on a.AccountID = u.AccountID
	                            where np.AccountID = @accountId and ae.isprimary = 1 and np.NotificationType = 502 and u.status=1 and u.isdeleted = 0";
                return db.Get<UserBasicInfo>(sql, new { accountId = accountId }).ToList();
            }
        }

        /// <summary>
        /// Gets all users basic information.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<UserBasicInfo> GetAllUsersBasicInfo(int accountId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"select u.UserID, u.AccountID, u.FirstName, u.LastName, ae.Email , coalesce(us.TimeZone,a.TimeZone) as TimeZone, a.AccountName  from users u (nolock) 
                        left join accountemails ae (nolock) on u.UserID = ae.userid
                        left join UserSettings us(nolock)  on u.UserID = us.UserID
                        left join accounts a (nolock) on a.AccountID = u.AccountID
                         where u.accountid = @accountId and u.IsDeleted = 0 and ae.isprimary = 1  and u.status = 1";
                var users = db.Get<UserBasicInfo>(sql, new { accountId = accountId });
                return users;
            }
        }

        public KeyValuePair<int, string> GetSuperAdminPasswordByEmail(string email)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"select u.* from users u (nolock) 
			                inner join AccountEmails ae (nolock) on u.userid = ae.userid
			                where u.AccountID = 1 and ae.email = @email and u.IsDeleted = 0 and u.status = 1";
                var hashedPassword = db.Get<UsersDb>(sql, new { email = email })
                    .Select(c => new KeyValuePair<int, string>(c.UserID, c.Password)).FirstOrDefault();
                return hashedPassword;
            }
        }

        public Dictionary<Guid, int> TrackReceivedEmail(IEnumerable<ReceivedMailAudit> receivedEmails)
        {
            using (var db = ObjectContextFactory.Create())
            {
                Dictionary<Guid, int> receivedMailAuditIds = new Dictionary<Guid, int>() { };
                var sql = @"INSERT INTO ReceivedMailAudit VALUES (@userId, @sentByContactId, @receivedOn, @referenceId)
                            SELECT SCOPE_IDENTITY()";
                foreach (var email in receivedEmails)
                {
                    if (email.UserID > 0)
                    {
                        int receivedMailAuditId = db.Get<int>(sql, new { userId = email.UserID, sentByContactId = email.SentByContactID, receivedOn = email.ReceivedOn, referenceId = email.ReferenceID }).FirstOrDefault();
                        receivedMailAuditIds.Add(email.ReferenceID, receivedMailAuditId);
                    }
                }
                return receivedMailAuditIds;
            }
        }

        public IEnumerable<UserBasicInfo> FindUsersByEmails(IEnumerable<string> emails, int accountId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"select u.UserID, u.AccountID, u.FirstName, u.LastName, ae.Email , coalesce(us.TimeZone,a.TimeZone) as TimeZone, a.AccountName  from users u (nolock) 
                        left join accountemails ae (nolock) on u.UserID = ae.userid
                        left join UserSettings us (nolock) on u.UserID = us.UserID
                        left join accounts a (nolock) on a.AccountID = u.AccountID
                         where u.accountid in (@accountId,1) and u.IsDeleted = 0 and ae.isprimary = 1 and ae.email in (select datavalue from dbo.split(@userEmails, ','))";
                var users = db.Get<UserBasicInfo>(sql, new { userEmails = string.Join(",", emails), accountId = accountId });
                return users;
            }
        }

        public IEnumerable<string> GetUsersPrimaryEmailsByUserIds(IEnumerable<int> userIds, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var userprimaryEmails = db.Users.Where(u => userIds.Contains(u.UserID) && u.Status == Status.Active && u.IsDeleted == false && u.AccountID == accountId).Select(e => e.PrimaryEmail).ToList();
            return userprimaryEmails;
        }

        public List<string> GetAccountAdminEmails(int accountId)
        {
            var db = ObjectContextFactory.Create();
            var accountAdminRole = db.Roles.Where(w => w.RoleName == "Account Administrator" && !w.IsDeleted && w.AccountID == accountId).Select(s => s.RoleID).FirstOrDefault();
            var userEmails = db.Users.Where(w => w.AccountID == accountId && w.RoleID == accountAdminRole && !w.IsDeleted && w.AccountID == accountId && w.Status == Status.Active).Select(s => s.PrimaryEmail).ToList();
            return userEmails;
        }

        public string GetUsersPrmaryPhoneNumbersByUserIds(string email, int accountId)
        {
            var db = ObjectContextFactory.Create();
            string defaultPhoneNumber = string.Empty;
            var phoneDetails = db.Users.Where(u => u.PrimaryEmail == email && u.AccountID == accountId).Select(s => new
            {
                homePhone = s.HomePhone,
                workPhone = s.WorkPhone,
                mobilePhone = s.MobilePhone,
                primaryPhoneType = s.PrimaryPhoneType
            }).FirstOrDefault();
            if (phoneDetails.primaryPhoneType == "M")
                defaultPhoneNumber = phoneDetails.mobilePhone;
            else if (phoneDetails.primaryPhoneType == "H")
                defaultPhoneNumber = phoneDetails.homePhone;
            else if (phoneDetails.primaryPhoneType == "W")
                defaultPhoneNumber = phoneDetails.workPhone;
            return defaultPhoneNumber;

        }

        public short GettingRoleIDByUserID(int userId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT RoleID  FROM Users(NOLOCK) WHERE UserID=@userId";
            short roleId = db.Get<short>(sql, new { userId = userId }).FirstOrDefault();
            return roleId;
        }

        public int? GetDefaultAdmin()
        {
            var db = ObjectContextFactory.Create();
            string userIdValue = db.EnvironmentSettings.Where(w => w.Name.Equals("CRM User")).Select(s => s.Value).FirstOrDefault();
            if (!string.IsNullOrEmpty(userIdValue))
                return int.Parse(userIdValue);
            else
                return null;
        }

        public int GetAllActiveUserIds(int accountId, int[] userIds)
        {
            using (var db = ObjectContextFactory.Create())
            {
                string excludedRoles = db.SubscriptionModules.Where(w => w.AccountID == accountId && w.ModuleID == (byte)AppModules.Users).Select(s => s.ExcludedRoles).FirstOrDefault();
                IEnumerable<int> activeUserCount = new List<int>();
                IEnumerable<int> requestedUsers = userIds;
                if (!string.IsNullOrEmpty(excludedRoles))
                {
                    IEnumerable<short> roles = excludedRoles.Split(',').Select(s => short.Parse(s));
                    activeUserCount = db.Users.Where(i => i.AccountID == accountId && i.IsDeleted == false && i.Status == Status.Active && !roles.Contains(i.RoleID)).Select(s => s.UserID);
                    requestedUsers = db.Users.Where(w => userIds.Contains(w.UserID) && !roles.Contains(w.RoleID) && w.Status != Status.Active).Select(s => s.UserID);
                }
                else
                {
                    activeUserCount = db.Users.Where(i => i.AccountID == accountId && i.IsDeleted == false && i.Status == Status.Active).Select(s => s.UserID);
                    requestedUsers = db.Users.Where(w => userIds.Contains(w.UserID) && w.Status != Status.Active).Select(s => s.UserID);
                }
                return activeUserCount.Count() + requestedUsers.Count();
            }
        }

        public byte GetUserStatusByUserId(int userId, int accountId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"SELECT Status FROM Users (NOLOCK) WHERE UserID=@UserId AND AccountID=@AccountId AND IsDeleted=0 ";
                return db.Get<byte>(sql, new { UserId = userId, AccountId = accountId }).FirstOrDefault();
            }
        }

        public bool IsLimitReached(int accountId, int[] userIds, short roleId,int userLmit)
        {
            using (var db = ObjectContextFactory.Create())
            {
                bool isLimitExceeded = false;
                db.QueryStoredProc("dbo.ValidateUserLimit", reader => 
                {
                    isLimitExceeded = reader.Read<bool>().FirstOrDefault();
                },
                new
                {
                    AccountId = accountId,
                    UserIds = userIds.AsTableValuedParameter("dbo.Contact_List"),
                    TargetRoleId = roleId,
                    UserLimit = userLmit

                });
                return isLimitExceeded;
            }
        }

        /// <summary>
        /// Get Logged in User Basic Info.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public UserBasicInfo GetUserBasicDetails(int userId)
        {
            using(var db = ObjectContextFactory.Create())
            {
                var sql = @"SELECT u.FirstName,u.LastName,u.PrimaryEmail AS Email,r.RoleName FROM Users (NOLOCK) u
                            JOIN Roles (NOLOCK) r on r.RoleID = u.RoleID
                            WHERE u.UserID=@UserId";
                return db.Get<UserBasicInfo>(sql, new { UserId = userId }).FirstOrDefault();
            }
        }

        /// <summary>
        /// Get My Communication Details.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="accountId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public IEnumerable<MyCommunication> GetMyCommunicationDetails(int userId, int accountId, DateTime startDate, DateTime endDate)
        {
            using (var db = ObjectContextFactory.Create())
            {
                IEnumerable<MyCommunication> myCommunications = new List<MyCommunication>();
                db.QueryStoredProc("[dbo].[GetMyCommunicationChartDetails]", (reader) =>
                {
                    myCommunications = reader.Read<MyCommunication>().ToList();
                }, new
                {
                    UserId = userId,
                    AccountId = accountId,
                    StartDate = startDate,
                    EndDate = endDate
                });

                return myCommunications;
            }
        }

        public IList<int> GetMyCommunicationContacts(int userId, int AccountId, DateTime startDate, DateTime endDate, string activity, string activityType)
        {
            using (var db = ObjectContextFactory.Create())
            {
                short categoryId = 0;
                if (activity == "A" || activity == "N")
                    categoryId = Convert.ToInt16(activityType);
                
                IList<int> contactIds = new List<int>();
                db.QueryStoredProc("[dbo].[GetMyCommunicationContacts]", (reader) =>
                {
                    contactIds = reader.Read<int>().ToList();
                }, new
                {
                    UserId = userId,
                    AccountId = AccountId,
                    StartDate = startDate,
                    EndDate = endDate,
                    Activity = activity,
                    ActivityType = activityType,
                    Category = categoryId
                });

                return contactIds;
            }
        }

    }
}
