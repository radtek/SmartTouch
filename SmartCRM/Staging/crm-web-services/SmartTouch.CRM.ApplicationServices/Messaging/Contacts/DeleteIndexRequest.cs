using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class DeleteIndexRequest: ServiceRequestBase
    {
        public string Name {get;set;}
    }

    public class DeleteIndexResponse : ServiceResponseBase
    {
        public bool Result { get; set; }
    }
}
