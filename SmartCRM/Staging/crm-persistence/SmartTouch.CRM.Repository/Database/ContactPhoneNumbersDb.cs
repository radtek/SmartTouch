using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class ContactPhoneNumbersDb
    {
       [Key]
       public int ContactPhoneNumberID { get; set; }
       [ForeignKey("Contacts")]
       public int ContactID { get; set; }
       public string PhoneNumber { get; set; }
       [ForeignKey("DropdownValues")]
       public Int16 PhoneType { get; set; }
       public bool IsPrimary { get; set; }
       [ForeignKey("Accounts")]
       public int AccountID { get; set; }
       public bool IsDeleted { get; set; }
       public string CountryCode { get; set; }
       public string Extension { get; set; }
       public virtual AccountsDb Accounts { get; set; }
       public virtual DropdownValueDb DropdownValues { get; set; }
       public virtual ContactsDb Contacts { get; set; }
    }
}
