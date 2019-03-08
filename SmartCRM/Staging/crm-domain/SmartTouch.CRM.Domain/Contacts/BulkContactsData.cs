using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Contacts
{
    public class BulkContactsData
    {
        public int BulkContactDataID { get; set; }

        public int BulkOperationID { get; set; }
        public int ContactID { get; set; }
    }
}
