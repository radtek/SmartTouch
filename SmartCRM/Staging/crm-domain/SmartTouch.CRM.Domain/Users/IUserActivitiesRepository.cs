using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Users
{
    public interface IUserActivitiesRepository : IRepository<UserActivityLog, int>
    {
        IEnumerable<UserActivityLog> FindAll(int userId, int pageNumber, int accountId, int[] moduleids);
        void DeleteActivity(int userId, int activityLogId, int accountId);
        void InsertChangeOwnerActivity(int entityId, int? userId, AppModules module, UserActivityType activityType);
        void InsertContactReadActivity(int entityId, string entityName, int userId, AppModules module, UserActivityType activityName, int accountId);
        IList<int> GetContactsByActivity(int userId, AppModules moduleName, UserActivityType activityName,int[] contactIDs,string Sort,int accountId);
        UserActivityEntityDetail GetUserActivityEntityDetails(byte moduleId, int entityId,int accountId);
        //IEnumerable<UserActivityList> GetUserActivities(int[] userIds,int[] Modules, int accountId, DateTime startDate, DateTime endDate);
        IEnumerable<int> GetUserModules(int userId, int moduleId);
    }
}
