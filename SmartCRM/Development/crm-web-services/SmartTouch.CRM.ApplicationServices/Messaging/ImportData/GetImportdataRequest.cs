using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.ImportData
{

    public class GetImportdataRequest : ServiceRequestBase
    {
        public string filename { get; set; }
        // this is for passing the account id from controller to service layer
        public int AccountID { get; set; }
    }

    public class GetImportdataResponse : ServiceResponseBase
    {
        public ImportDataListViewModel ImportDataListViewModel { get; set; }
    }
}
