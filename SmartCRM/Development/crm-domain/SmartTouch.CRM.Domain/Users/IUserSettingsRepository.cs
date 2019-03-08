using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.Users
{
    public interface IUserSettingsRepository : IRepository<UserSettings, int>
    {
        bool IsDuplicateUserSetting(int AccountId, int UserId);      
        UserSettings GetSettings(int AccountID, int UserID);
        UserSettings GetFirstLoginSettings(int userId);
        void UpdateTCAcceptance(int userId, int accountId);
        bool IsIncludeSignatureByDefault(int userId);
    }
}
