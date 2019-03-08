using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface IFindSpamService
    {
        bool SpamCheck(IDictionary<string, string> fields, int accountId,string IpAddress, int formId,bool isFormSubmission, out string spamRemarks);
    }
}
