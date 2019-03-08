using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class ProviderViewModel
    {
        public string Name { get; set; }
        public LandmarkIT.Enterprise.CommunicationManager.Requests.MailProvider Id { get; set; }
        public int ServiceProviderId { get; set; }
    }
}
