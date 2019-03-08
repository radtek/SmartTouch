using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.ThirdPartyClient
{
    public class GetApiKeyByIDRequest : ServiceRequestBase
    {
        public string ID { get; set; }

    }

    //public class GetApiKeyByIDRequest:ServiceRequestBase
    //{
    //    public string ID { get; set; }
    //    public GetApiKeyByIDRequest(string ID) : base(ID) { }
    //}

    public class GetApiKeyByIDResponse: ServiceResponseBase
    {
        public ThirdPartyClientViewModel ThirdPartyClientViewModel { get; set; }
    }
}
