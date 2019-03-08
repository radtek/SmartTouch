using SmartTouch.CRM.Domain.Modules;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Users
{
    public class UserActivityLog : EntityBase<int>, IAggregateRoot
    {
        public int UserActivityLogID { get; set; }
        public int EntityID { get; set; }
       
        public int UserID { get; set; }
        public User User { get; set; }
        public byte ModuleID { get; set; }
        public Module Module { get; set; }
        public int UserActivityID { get; set; }
        public UserActivity UserActivity { get; set; }
        public DateTime LogDate { get; set; }
        public string Message { get; set; }
        public UserActivityEntityDetail EntityDetail { get; set; }
        public int AccountID { get; set; }
        public string EntityName { get; set; }

        public IEnumerable<int> Modules { get; set; }

        public IEnumerable<User> Collection { get; set; }
        
        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }

    public class UserActivityList
    {
        public int OwnerId { get; set; }
        public string OwnerName { get; set; }
        public int TotalModuleCount { get; set; }
        public IEnumerable<UserModules> UserModules { get; set; }
    }

    public class UserModules
    {
        public int UserModuleId { get; set; }
        public int ModuleCount { get; set; }
    }
}
