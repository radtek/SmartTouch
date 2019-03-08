using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.Roles;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.ImplicitSync;
using SmartTouch.CRM.Domain.Reports;

namespace SmartTouch.CRM.Domain.Users
{
    public interface IUserRepository : IRepository<User, int>
    {
        IEnumerable<User> FindAll(string name, byte status, short role, int accountId,bool isSTAdmin);
        IEnumerable<User> FindAll(string name, int limit, int pageNumber, byte status, short role, int accountId, bool isSTAdmin);
        IEnumerable<Role> GetRoles(int accountId);
        IEnumerable<User> GetUsers(int accountID, bool isSTAdmin);

        UserSettings FindUserSettingBy(int AccountID, int UserID);
        void DeactivateUsers(int[] userID,int modifiedBy);
        void UpdateUserStatus(int[] userID, byte Status);
        void ChangeRole(short roleId, int[] userID);
        IEnumerable<dynamic> GetCurrencies();
        IEnumerable<DateFormat> GetDateFormats();
        IEnumerable<Email> GetEmail(int accountId, int userId);
        IEnumerable<Email> CampaignGetEmail(int accountId, int userId, int roleId);
        string GetUserPrimaryEmail(int userId);
        string GetUserPrimaryPhoneNumber(int userId);
        Email GetUserEmail(int emailId);
        void InsertUserCredentials(string email, string password);
        User FindByEmailAndAccountId(string email, int accountId);
        User FindByEmailAndDomainUrl(string email, string domainUrl);
        User FindByEmail(string email);
        User FindByEmailPassword(string email, string password);
        User FindUserForLogin(int userId);
        IEnumerable<int> GetSTAdminList();
        void AddBulkNotifications(Notification notification);
        bool CheckPasswordResetFlag(int userId, int accountId);
        void UpdatePasswordResetFlag(int userId,bool status);
        bool IsDuplicateUser(string primaryEmail, int userId, int accountId);
        IEnumerable<User> GetAllUsers();
     
        string GetUserTimeZone(int userId,int AccountId);
        
      
        List<string> GetPreviousPasswords(int userId,int limit);
        void UpdatePasswordHistory(int userId,string newPassword);
        bool IsAccountAdminExist(int accountId);
        void InsertLoginAudit(int userId, int accountId, string IP, SignInActivity activity);
        void InsertUserProfileAudit(int userId, UserAuditType auditType, int auditedBy, string password);

        IEnumerable<Notification> GetImpendingReminderNotifications(IEnumerable<int> userIds);
        IEnumerable<Notification> GetImpendingReminderWebVisitNotifications(IEnumerable<int> userIds, int accountId);
        IEnumerable<Notification> GetReminderNotifications(IEnumerable<int> userIds, IEnumerable<int> moduleIds, bool todayNotificcations);
        IEnumerable<Notification> GetViewedNotifications(IEnumerable<int> userIds, IEnumerable<int> moduleIds, bool todayNotificcations);
        int GetNotificationsCount(int userId, NotificationStatus notificationStatus, IEnumerable<byte> moduleIds);
        IEnumerable<int> GetWebVisitNotificationsCount(int userId, int accountId, NotificationStatus notificationStatus);
        NotificationsCount GetNotificationsCountByDate(int userId, IEnumerable<byte> moduleIds, NotificationStatus status);
        Notification AddNotification(Notification notification);
        void UpdateNotification(Notification notification);
        int DeleteNotification(IEnumerable<int> notificationIds, int userId, byte moduleId, bool ArePreviousNotifications, bool bulkRemove);
        int DeleteBulkNotifications(int userId, bool ArePreviousNotifications, IEnumerable<byte> moduleIds);

        Role GetUserRole(int userId);
        string GetUserName(int userId);
        List<CalenderTimeSlot> GetUserCalender(int accountId, int? userId, DateTime startDate, DateTime endDate);
        void UpdateFacebookAccessToken(int uid, string accessToken);
        void UpdateTwitterOAuthTokens(int uid, string token, string tokenSecret);
        List<string> GetUserPhoneNumbers(int userId);
        bool isFromEmailValid(string Email, int UserID);
        void UpdateUserResetPassword(int[] userIds, string password, int? userID);

        void AddNotification(List<Notification> notifications);
       
        int GetUsersLimit(int AccountID);

        bool IsAccountAdminExistsInUsers(int[] userIds,int accountId);

        bool IsAccountAdminExistsInActiveUsers(int[] userIds, int accountId);

        IEnumerable<UserBasicInfo> GetUsersByUserIDs(IEnumerable<int?> userIds);
        IEnumerable<UserBasicInfo> GetUsersOptedInstantWebVisitEmail(int accountId);
        IEnumerable<UserBasicInfo> GetUsersOptedWebVisitSummaryEmail(int accountId);
        IEnumerable<UserBasicInfo> GetAllUsersBasicInfo(int accountId);
        KeyValuePair<int,string> GetSuperAdminPasswordByEmail(string email);
        IEnumerable<string> GetUsersPrimaryEmailsByUserIds(IEnumerable<int> userIds, int accountId);
        string GetUsersPrmaryPhoneNumbersByUserIds(string email, int accountId);
        Dictionary<Guid,int> TrackReceivedEmail(IEnumerable<ReceivedMailAudit> receivedEmails);
        IEnumerable<UserBasicInfo> FindUsersByEmails(IEnumerable<string> emails, int accountId);
        short GettingRoleIDByUserID(int userId);
        int? GetDefaultAdmin();
        int GetAllActiveUserIds(int accountId, int[] userIds);
        bool IsRoleExcludedFromLimit(int accountId, short roleId);
        byte GetUserStatusByUserId(int userId, int accountId);
        List<string> GetAccountAdminEmails(int accountId);
        bool IsLimitReached(int accountId, int[] userIds, short roleId,int userLmit);
        UserBasicInfo GetUserBasicDetails(int userId);
        IEnumerable<MyCommunication> GetMyCommunicationDetails(int userId, int AccountId, DateTime startDate, DateTime endDate);
        IList<int> GetMyCommunicationContacts(int userId, int AccountId, DateTime startDate, DateTime endDate,string activity,string activityType);
    }
}
