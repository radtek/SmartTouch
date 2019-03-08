using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Forms
{
    public class GetFormsRequest : ServiceRequestBase
    {
        public int Limit { get; set; }
        public string Query { get; set; }
        public int PageNumber { get; set; }
        public byte Status { get; set; }
        public int AccountID { get; set; }
    }

    public class GetFormsResponse : ServiceResponseBase
    {
        public IEnumerable<FormViewModel> Forms { get; set; }        
        public int TotalHits { get; set; }
    }
}
