using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class DeleteUserActivityRequest : ServiceRequestBase
    {
        public int userID { get; set; }
        public int activityLogID { get; set; }
    }

    public class DeleteUserActivityResponse : ServiceResponseBase
    { 
      
    }
}
