using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class UpdateFacebookConnectionRequest: ServiceRequestBase
    {
        public int UserId { get; set; }
        public string FacebookAccessToken { get; set; }
    }
    public class UpdateFacebookConnectionResponse: ServiceResponseBase
    {

    }
    public class UpdateTwitterConnectionRequest : ServiceRequestBase
    {
        public int UserId { get; set; }
        public string TwitterOAuthToken { get; set; }
        public string TwitterOAuthTokenSecret { get; set; }
    }
    public class UpdateTwitterConnectionResponse : ServiceResponseBase
    {

    }
}
