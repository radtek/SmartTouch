using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Opportunity
{
    public class GetStageRequest:ServiceRequestBase
    {

    }


    public class GetStageResponse : ServiceResponseBase
    {
        public IEnumerable<dynamic> Stages { get; set; }
    }
}
