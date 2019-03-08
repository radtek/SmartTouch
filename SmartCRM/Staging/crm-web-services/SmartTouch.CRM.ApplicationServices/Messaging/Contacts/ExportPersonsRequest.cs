using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class ExportPersonsRequest : ServiceRequestBase
    {
        public string DownLoadAs { get; set; }
        public string DateFormat { get; set; }
        public ExportPersonViewModel ExportViewModel { get; set; }
        public string TimeZone { get; set; }
    }

    public class ExportPersonsResponse : ServiceResponseBase
    {        
         public byte[] byteArray { get; set; }
         public string FileName { get; set; }
    }
}
