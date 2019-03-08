using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class InsertIndexingDataRequest : ServiceRequestBase
    {
        public IndexingData IndexingData { get; set; }
    }
    public class InsertIndexingDataResponce : ServiceResponseBase
    {

    }
}
