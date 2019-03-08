using SmartTouch.CRM.Domain.Contacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ImportData
{
    public class ImportContactsData
    {
        public IList<RawContact> ContactData { get; set; }
        public IList<ImportCustomData> ContactCustomData { get; set; }
        public IList<ImportPhoneData> ContactPhoneData { get; set; }
    }
}
