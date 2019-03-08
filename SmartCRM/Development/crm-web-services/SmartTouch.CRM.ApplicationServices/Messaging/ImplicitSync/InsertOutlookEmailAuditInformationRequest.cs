using SmartTouch.CRM.Domain.ImplicitSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.ImplicitSync
{
    public class InsertOutlookEmailAuditInformationRequest : ServiceRequestBase
    {
        public IEnumerable<OutlookEmailInformation> Emails { get; set; }
        public Guid Guid { get; set; }
        public  DateTime SentUTCDate{ get; set; }
    }
    public class InsertOutlookEmailAuditInformationResponse : ServiceResponseBase
    {
 
    }
}
