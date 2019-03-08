using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.SeedList
{
    public class GetSeedListRquest : ServiceRequestBase
    {
    }
    public class GetSeedListResponse : ServiceResponseBase
    {
        public IEnumerable<SeedEmailViewModel> SeedEmailViewModel { get; set; }
    }
}
