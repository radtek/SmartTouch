using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.LeadAdapters
{
    public class GetFacebookAppRequest : ServiceRequestBase
    {

    }

    public class GetFacebookAppResponse : ServiceRequestBase
    {
        public string FacebookAppID { get; set; }
        public string FacebookAppSecret { get; set; }
    }
}
