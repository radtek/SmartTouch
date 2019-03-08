using AutoMapper;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartTouch.CRM.Repository.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    public class UserSettingsRepository : Repository<UserSettings, int, UserSettingsDb>, IUserSettingsRepository
    {
        public UserSettingsRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory) { }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override UserSettings FindBy(int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="db">The database.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Invalid user id has been passed. Suspected Id forgery.</exception>
        public override UserSettingsDb ConvertToDatabaseType(UserSettings domainType, CRMDb db)
        {
            UserSettingsDb userSettingsDb;

            if (domainType.Id > 0)
            {
                userSettingsDb = db.UserSettings.SingleOrDefault(c => c.UserSettingID == domainType.Id);
                //userDb = db.Users.SingleOrDefault(c => c.UserID == domainType.Id);

                if (userSettingsDb == null)
                    throw new ArgumentException("Invalid user id has been passed. Suspected Id forgery.");

                userSettingsDb = Mapper.Map<UserSettings, UserSettingsDb>(domainType, userSettingsDb);

                if (userSettingsDb.CurrencyID == 0)
                {
                    userSettingsDb.CurrencyID = null;
                }

                if (userSettingsDb.CountryID == "")
                {
                    userSettingsDb.CountryID = null;
                }
            }
            else //New Contact
            {

                userSettingsDb = Mapper.Map<UserSettings, UserSettingsDb>(domainType);
                if (userSettingsDb.CurrencyID == 0)
                {
                    userSettingsDb.CurrencyID = null;
                }

                if (userSettingsDb.CountryID == "")
                {
                    userSettingsDb.CountryID = null;
                }
            }
            return userSettingsDb;
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="userSettingDbObject">The user setting database object.</param>
        /// <returns></returns>
        public override UserSettings ConvertToDomain(UserSettingsDb userSettingDbObject)
        {
            var userSettingsDb = getUserSettingsDb(userSettingDbObject.UserSettingID);
            UserSettings userSettings = new UserSettings();
            Mapper.Map<UserSettingsDb, UserSettings>(userSettingsDb, userSettings);
            return userSettings;
            // throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the user settings database.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        UserSettingsDb getUserSettingsDb(int id)
        {
            var db = ObjectContextFactory.Create();
            return db.UserSettings.SingleOrDefault(c => c.UserSettingID == id);
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="userSettings">The user settings.</param>
        /// <param name="userSettingsDb">The user settings database.</param>
        /// <param name="db">The database.</param>
        public override void PersistValueObjects(UserSettings userSettings, UserSettingsDb userSettingsDb, CRMDb db)
        {
            persistEmails(userSettings, userSettingsDb, db);
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Persists the emails.
        /// </summary>
        /// <param name="userSettings">The user settings.</param>
        /// <param name="userSettingsDb">The user settings database.</param>
        /// <param name="db">The database.</param>
        private void persistEmails(UserSettings userSettings, UserSettingsDb userSettingsDb, CRMDb db)
        {
            if (userSettings.UserID > 0 && userSettings.Emails != null)
            {
                Email email = userSettings.Emails.Where(a => a.IsUpdated == true).SingleOrDefault();
                AccountEmailsDb accountEmails = Mapper.Map<Email, AccountEmailsDb>(email);
                if (accountEmails != null)
                {
                    //if (accountEmails.IsPrimary == true)
                    //{
                    //    UsersDb usersdb = db.Users.Where(u => (u.PrimaryEmail == accountEmails.Email) && (u.AccountID == accountEmails.AccountID) && (u.UserID == accountEmails.UserID)).SingleOrDefault();
                    //    usersdb.EmailSignature = accountEmails.EmailSignature;
                    //    db.SaveChanges();
                    //}
                    AccountEmailsDb accountEmailsdb = db.AccountEmails.Where(ae => ae.EmailID == accountEmails.EmailID).SingleOrDefault();
                    accountEmailsdb.EmailSignature = accountEmails.EmailSignature;
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Determines whether [is duplicate user setting] [the specified account identifier].
        /// </summary>
        /// <param name="AccountID">The account identifier.</param>
        /// <param name="UserID">The user identifier.</param>
        /// <returns></returns>
        public bool IsDuplicateUserSetting(int AccountID, int UserID)
        {
            IQueryable<UserSettingsDb> usersettings = ObjectContextFactory.Create().UserSettings.Where(c => c.AccountID == AccountID && c.UserID == UserID);
            int count = usersettings.Count();
            if (count > 0)
                return true;
            return false;
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<UserSettings> FindAll()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <param name="AccountID">The account identifier.</param>
        /// <param name="UserID">The user identifier.</param>
        /// <returns></returns>
        public UserSettings GetSettings(int AccountID, int UserID)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"select *
                            from usersettings us (nolock) 
                            inner join currencies c (nolock) on us.currencyid = c.currencyid
                            inner join dateformats df (nolock) on df.dateformatid = us.[dateformat]
                            where userid = @id";

                var usersettingsdb = db.Get<UserSettingsDb, CurrenciesDb, DateFormatDb>(sql, (u, c, d) =>
                    {
                        u.Currency = c;
                        u.DateFormat = d.DateFormatId;
                        u.DateFormatName = d.FormatName;
                        return u;
                    }, new { Id = UserID }, splitOn: "CurrencyID,DateFormatID").FirstOrDefault();


                if (usersettingsdb == null)
                {
                    var asql = @"select *
                                from accounts a (nolock) 
                                inner join currencies c (nolock) on a.currencyid = c.currencyid
                                inner join dateformats df (nolock) on df.dateformatid = a.dateformatid
                                 where a.accountid = @aid";

                    var accountsettings = db.Get<AccountsDb, CurrenciesDb, DateFormatDb>(asql, (a, c, d) =>
                    {
                        a.CurrencyID = c.CurrencyID;
                        a.CurrencyFormat = c.Format;
                        a.DateFormatID = d.DateFormatId;
                        a.DateFormat = d.FormatName;
                        return a;
                    }, new { AId = AccountID }, splitOn: "CurrencyID,DateFormatID").FirstOrDefault();

                    UserSettings settings = new UserSettings
                    {
                        CountryID = accountsettings.CountryID,
                        CurrencyFormat = accountsettings.CurrencyFormat,
                        CurrencyID = accountsettings.CurrencyID,
                        DateFormat = accountsettings.DateFormatID,
                        DateFormatType = accountsettings.DateFormat,
                        ItemsPerPage = 25,
                        TimeZone = accountsettings.TimeZone
                    };

                    return settings;
                }
                return new UserSettings
                    {
                        CountryID = usersettingsdb.CountryID,
                        CurrencyFormat = usersettingsdb.Currency.Format,
                        CurrencyID = usersettingsdb.CurrencyID,
                        DateFormat = usersettingsdb.DateFormat,
                        DateFormatType = usersettingsdb.DateFormatName,
                        ItemsPerPage = usersettingsdb.ItemsPerPage,
                        TimeZone = usersettingsdb.TimeZone
                    };
            }
        }

        public UserSettings GetFirstLoginSettings(int userId)
        {
            var db = ObjectContextFactory.Create();
            var settings = db.UserSettings.Where(w => w.UserID == userId).Select(s => new { TC = s.HasAcceptedTC }).ToList().Select(x => new UserSettings() { HasAcceptedTC = x.TC }).FirstOrDefault();
            if (settings == null)      //Newely cerated user therefore no user-settings
                return new UserSettings() { HasAcceptedTC = false };
            else
                return settings;
        }

        public void UpdateTCAcceptance(int userId, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var settings = db.UserSettings.Where(w => w.UserID == userId).FirstOrDefault();
            if (settings != null)
            {
                settings.HasAcceptedTC = true;
                db.Entry(settings).State = System.Data.Entity.EntityState.Modified;
            }
            else   //Newely created user (Has no settings)    
            {
                var accountSettings = db.Accounts.Where(w => w.AccountID == accountId).Select(s => new { CurrencyID = s.CurrencyID, CountryID = s.CountryID, TimeZone = s.TimeZone }).FirstOrDefault();
                UserSettingsDb settingsDb = new UserSettingsDb();
                settingsDb.AccountID = accountId;
                settingsDb.UserID = userId;
                settingsDb.ItemsPerPage = 25; //Default Items per page is 25
                settingsDb.DateFormat = 2;
                settingsDb.CountryID = accountSettings.CountryID;
                settingsDb.CurrencyID = accountSettings.CurrencyID;
                settingsDb.TimeZone = accountSettings.TimeZone;
                settingsDb.HasAcceptedTC = true;
                db.Entry(settingsDb).State = System.Data.Entity.EntityState.Added;
            }
            db.SaveChanges();
        }

        public bool IsIncludeSignatureByDefault(int userId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT IsIncludeSignature FROM UserSettings(NOLOCK) WHERE UserID=@userID";
            bool isIncludeSignature = db.Get<bool>(sql, new { userID = userId }).FirstOrDefault();
            return isIncludeSignature;
        }
    }
}
