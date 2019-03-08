using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetCompanyRequest : IntegerIdRequest
    {
        public GetCompanyRequest(int id) : base(id) { }
    }

    public class GetCompanyResponse : ServiceResponseBase
    {
        public CompanyViewModel CompanyViewModel { get; set; }
    }
}
