using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.SuppressionList
{
    public class DeleteSuppressionEmailRequest : ServiceRequestBase
    {
        public int SuppressionEmailId { get; set; }
    }

    public class DeleteSuppressionEmailResponse : ServiceResponseBase
    {

    }
}
