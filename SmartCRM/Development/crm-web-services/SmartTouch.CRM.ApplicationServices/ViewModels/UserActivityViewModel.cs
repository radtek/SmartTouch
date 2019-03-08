using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public interface IUserActivityViewModel
    {
        int UserActivityLogID { get; set; }
        int EntityId { get; set; }
        List<int> EntityIds { get; set; }
        IEnumerable<UserActivityEntityDetail> EntityDetails { get; set; }

        int UserID { get; set; }
        UserViewModel User { get; set; }
        byte ModuleID { get; set; }
        string ModuleName { get; set; }
        int UserActivityID { get; set; }
        string UserActivityName { get; set; }
        DateTime LogDate { get; set; }
    }

    public class UserActivityViewModel : IUserActivityViewModel
    {
        public int UserActivityLogID { get; set; }
        public int EntityId { get; set; }
        public List<int> EntityIds { get; set; }
        public IEnumerable<UserActivityEntityDetail> EntityDetails { get; set; }

        public int UserID { get; set; }
        public UserViewModel User { get; set; }
        public byte ModuleID { get; set; }
        public string ModuleName { get; set; }
        public int UserActivityID { get; set; }
        public string UserActivityName { get; set; }
        public DateTime LogDate { get; set; }
        public string Message { get; set; }
        public string DateFormat { get; set; }
        public DateTimeFormatInfo DateSeperator { get; set; }

        public string TimeInText
        {
            get
            {
                int differenceInSeconds = (int)(DateTime.Now.ToUniversalTime() - this.LogDate).TotalSeconds;
                int differenceInMinutes = (int)(differenceInSeconds / 60);
                int differenceInHours = (int)(differenceInMinutes / 60);

                if (differenceInSeconds < 60)
                    return "Few seconds ago";
                else if (differenceInMinutes < 60)
                    return differenceInMinutes + " minutes ago";
                else if (differenceInHours == 1)
                {
                    return "an hour ago";
                }
                else if (differenceInHours <= 24)
                    return differenceInHours + " hours ago";
                else
                    if (DateSeperator != null)
                        return this.LogDate.ToString(this.DateFormat, DateSeperator);
                    else
                        return this.LogDate.ToString(this.DateFormat, new DateTimeFormatInfo());
            }
            set { this.TimeInText = value; }
        }
    }

    public class UserActivitiesListViewModel
    {
        public IEnumerable<UserActivityViewModel> UserActivities { get; set; }
    }
}
