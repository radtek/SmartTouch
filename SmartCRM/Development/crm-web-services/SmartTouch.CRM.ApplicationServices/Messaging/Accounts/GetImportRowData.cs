using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class GetImportRowData : ServiceRequestBase
    {
    }
    public class GetImportRowDataResponce : ServiceResponseBase
    {
        public string RowData { get; set; }
    }
}
