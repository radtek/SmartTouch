using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.Messaging.User;
using SmartTouch.CRM.ApplicationServices.Messaging.Search;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics;
using SmartTouch.CRM.ApplicationServices.Messaging.ImplicitSync;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.Domain.Accounts;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface IUserService
    {
        GetUserListResponse GetAllUsers(GetUserListRequest request);
        CheckIfLimitExceededResponse CheckUserLimit(CheckIfLimitExceededRequest request);
        InsertUserResponse InsertUser(InsertUserRequest request);
        UpdateUserResponse UpdateUser(UpdateUserRequest request);
        GetRoleResponse GetRoles(GetRoleRequest request);
        GetUserResponse GetUser(GetUserRequest request);
        GetUserResponse GetUserByUserName(GetUserRequest request);
        IsAccountAdminExistResponse IsAccountAdminExist(IsAccountAdminExistRequest request);
        GetUserListResponse GetUsers(GetUserListRequest request);
        //DeactivateUserResponse UpdateUsersStatus(DeactivateUserRequest request);
        ChangeRoleResponse ChangeRole(ChangeRoleRequest request);
        DeactivateUserResponse DeactivateUser(DeactivateUserRequest request);
        GetEmailResponse GetEmails(GetEmailRequest request);
        GetEmailResponse CampaignGetEmails(GetEmailRequest request);
        GetDateFormatResponse GetDateFormats(GetDateFormatRequest request);
        GetCurrencyResponse GetCurrencies(GetCurrencyRequest request);
        UserStatusResponse UpdateUsersStatus(UserStatusRequest request);
        GetUserSettingResponse GetUserSetting(GetUserSettingRequest request);
        InsertUserSettingsResponse InsertUserSettings(InsertUserSettingsRequest request);

        GetUserActivitiesResponse GetUserActivities(GetUserActivitiesRequest request);
        DeleteUserActivityResponse DeleteUserActivity(DeleteUserActivityRequest request);
        UserReadActivityResponse InsertReadActivity(UserReadActivityRequest request);
        ChangeOwnerLogResponse InsertChangeOwnerActivity(ChangeOwnerLogRequest request);
        GetUserActivityDetailResponse GetUserActivityDetails(GetUserActivityDetailRequest request);
        GetRecentViwedContactsResponse GetRecentlyViewedContacts(GetRecentViwedContactsRequest request);
        GetRequestUserResponse GetUsersList(GetUserListRequest request);

        GetRecentWebVisitsResponse GetRecentWebVisits(GetRecentWebVisitsRequest request);

        GetUserNotificationsResponse GetImpendingReminderNotifications(GetUserNotificationsRequest request);
        GetUserNotificationsResponse GetReminderNotifications(GetUserNotificationsRequest request);
        GetUserNotificationsResponse GetWebVisitNotifications(GetUserNotificationsRequest request);
        GetUserNotificationsResponse GetNotifications(GetUserNotificationsRequest request);
        GetUserNotificationCountResponse GetUnReadNotificationsCount(GetUserNotificationCountRequest request);
        GetUserNotificationCountResponse GetUnReadWebVisitNotificationsCount(GetUserNotificationCountRequest request);
        GetNotificationsCountByDateResponse GetNotificationsCountByDate(GetNotificationsCountByDateRequest request);
        MarkNotificationAsReadResponse MarkNotificationAsRead(MarkNotificationAsReadRequest request);
        DeleteNotificationResponse DeleteNotification(DeleteNotificationRequest request);
        DeleteBulkNotificationsResponse DeleteBulkNotifications(DeleteBulkNotificationsRequest request);

        GetUserRoleResponse GetUserRole(GetUserRoleRequest request);
        GetUserFullNameResponse GetUserFullName(GetUserFullNameRequest request);

        InsertProfileAuditResponse InsertProfileAudit(InsertProfileAuditRequest request);
       
        GetUserCalenderResponse GetUserCalender(GetUserCalenderRequest request);
        //IEnumerable<UserActivityLog> GetAllUserByModule(int ownerId);
        UpdateFacebookConnectionResponse UpdateFacebookAccessToken(UpdateFacebookConnectionRequest request);
        UpdateTwitterConnectionResponse UpdateTwitterOAuthTokens(UpdateTwitterConnectionRequest request);
        InsertUserPasswordResponse InsertUserResetPassword(InsertUserPasswordRequest request);
        GetContactWebVisitReportResponse GetWebVisitDetailsByVisitReference(GetContactWebVisitReportRequest request);
        GetUsersByUserIDsResponse GetUsersByUserIDs(GetUsersByUserIDsRequest request);
        GetUsersOptedInstantWebVisitEmailResponse GetUsersOptedInstantWebVisitEmail(GetUsersOptedInstantWebVisitEmailRequest request);
        GetUsersOptedWebVisitSummaryEmailResponse GetUsersOptedWebVisitSummaryEmail(GetUsersOptedWebVisitSummaryEmailRequest request);

        bool GetAccountSubscription(int accountid);
        GetLoginInfoByUsernameResponse GetSuperAdminByEmail(GetLoginInfoByUsernameRequest request);
        GetUserTimeZoneResponse GetUserTimeZoneByUserID(GetUserTimeZoneRequest request);
        InserImplicitSyncEmailInfoResponse TrackReceivedEmail(InserImplicitSyncEmailInfoRequest request);
        FindUsersByEmailsResponse FindUsersByEmails(FindUsersByEmailsRequest request);
        void AddNotification(AddNotificationRequest request);
        GetFirstLoginUserSettingsResponse GetFirstLoginUserSettings(GetFirstLoginUserSettingsRequest request);
        UpdateTCAcceptanceResponse UpdateTCAcceptance(UpdateTCAcceptanceRequest request);

        GetAccountAddressResponse GetAccountAddress(GetAccountAddressRequest request);
        bool IsIncludeSignatureByDefaultOrNot(int userId);
        int? GetDefaultAccountAdmin();
        int GetActiveUserIds(int account, int[] userIds);
        AccountSubscriptionData GetSubscriptionData(int account);
        void UserLimitEmailNotification(int accountId);
    }
}
