using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.ImportData
{

    public class ImportDataRequest : ServiceRequestBase
    {
        public ImportDataListViewModel ImportDataListViewModel { get; set; }
    }

    public class ImportDataResponse : ServiceResponseBase
    {
        public virtual ImportDataListViewModel ImportDataViewModel { get; set; }
    }
}
