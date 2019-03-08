using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class CampaignPlainTextContent
    {
        public int CampaignPlainTextContentMapID { get; set; }
        public int CampaignID { get; set; }
        public string PlainTextContent { get; set; }
    }
}
