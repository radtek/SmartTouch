using SmartTouch.CRM.Domain.ImplicitSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.ImplicitSync
{
    public class GetOutlookEmailInformationRequest : ServiceRequestBase
    {
        public List<string> EmailsToUpload { get; set; }
    }

    public class GetOutlookEmailInformationResponse : ServiceResponseBase
    {
        public IEnumerable<OutlookEmailInformation> OutlookInformation { get; set; }
    }
}
