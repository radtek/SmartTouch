using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Reports
{
   public class ReportContactInfo
    {
       public int ContactID { get; set; }
       public string FullName { get; set; }
       public Byte ContactType { get; set; }
       public DateTime CreatedOn { get; set; }
       public string Email { get; set; }
       public int? ContactEmailID { get; set; }
       public string PhoneNumber { get; set; }
       public int? ContactPhoneNumberID { get; set; }
       public string Owner { get; set; }
    }
}
