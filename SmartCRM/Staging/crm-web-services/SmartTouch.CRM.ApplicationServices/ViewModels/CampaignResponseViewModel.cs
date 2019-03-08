using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    //public class CampaignResponseViewModel
    //{
    //    public int CampaignResponseId { get; set; }
    //    public string ResponseType { get; set; }
    //    public string FiredAt { get; set; }
    //    public string ContactId { get; set; }
    //    public string ListId { get; set; }
    //    public string Email { get; set; }
    //    public string IpOpt { get; set; }
    //    public string IpSignUp { get; set; }
    //}

    public class CampaignResponseViewModel
    {
        public int CampaignResponseId { get; set; }
        public IList<KeyValuePair<string,string>> ResponseItems { get; set; }
    }
}
