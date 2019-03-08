using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public interface ICommunicationViewModel
    {
        int CommunicationID { get; set; }
        string SecondaryEmails { get; set; }
        string FacebookUrl { get; set; }
        string TwitterUrl { get; set; }
        string GooglePlusUrl { get; set; }
        string LinkedInUrl { get; set; }
        string BlogUrl { get; set; }
        string WebSiteUrl { get; set; }
    }

    public class CommunicationViewModel : ICommunicationViewModel
    {
        public int CommunicationID { get; set; }
        public string SecondaryEmails { get; set; }
        public string FacebookUrl { get; set; }
        public string TwitterUrl { get; set; }
        public string GooglePlusUrl { get; set; }
        public string LinkedInUrl { get; set; }
        public string BlogUrl { get; set; }
        public string WebSiteUrl { get; set; }
    }
}
