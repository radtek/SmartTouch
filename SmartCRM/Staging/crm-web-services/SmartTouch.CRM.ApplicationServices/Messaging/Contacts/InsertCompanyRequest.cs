using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class InsertCompanyRequest : ServiceRequestBase
    {
        public CompanyViewModel CompanyViewModel { get; set; }
        public byte ModuleId { get; set; }
        public bool isStAdmin { get; set; }
    }

    public class InsertCompanyResponse : ServiceResponseBase
    {
        public virtual CompanyViewModel CompanyViewModel { get; set; }
    }
}
