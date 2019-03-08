using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class ICommunityViewModel
    {
        string CommunityName { get; set; }
        int AccountID { get; set; }

        string Street { get; set; }
        string City { get; set; }
        string State { get; set; }
        string Country { get; set; }
    }

    public class CommunityViewModel : ICommunityViewModel
    {
        public string CommunityName { get; set; }
        public int AccountID { get; set; }

        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }
}
