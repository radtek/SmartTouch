using SmartTouch.CRM.ApplicationServices.Messaging.ImportData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface IImportDataService
    {
        GetContactEmailsResponse GetContactEmails(GetContactEmailsRequest request);        
    }
}
