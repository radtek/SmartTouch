using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class GetRecentWebVisitsRequest : ServiceRequestBase
    {
        public int Limit { get; set; }
        public byte PageNumber { get; set; }
        public string DateFormat { get; set; }
        public bool IncludePreviouslyRead { get; set; }
    }

    public class GetRecentWebVisitsResponse : ServiceResponseBase
    {
        public IEnumerable<Notification> RecentWebVisits { get; set; }
    }

}
