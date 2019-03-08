using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartTouch.CRM.Plugins.Utilities.ImplicitSync
{
    public static class ConcurrentUsers
    {
        public static IList<ConcurrentUser> ConcurrentUser { get; set; }
        
    }

    public class ConcurrentUser
    {
        public User User { get; set; }
        public string SessionKey { get; set; }
        public DateTime SessionStartTime { get; set; }
        public IEnumerable<DropdownViewModel> DropdownValues { get; set; }
    }
}