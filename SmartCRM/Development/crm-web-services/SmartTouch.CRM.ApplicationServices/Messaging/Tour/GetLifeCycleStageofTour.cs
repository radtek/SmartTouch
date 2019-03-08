using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Tour
{
    public class GetLifeCycleStageofTourRequest : ServiceRequestBase
    {
        public int[] ContactIds { get; set; }
    }

    public class GetLifeCycleStageofTourResponse : ServiceResponseBase
    {
        public int LifeCycleStage { get; set; }
    }
}
