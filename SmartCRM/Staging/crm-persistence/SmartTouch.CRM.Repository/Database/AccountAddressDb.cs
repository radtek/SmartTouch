using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class AccountAddressDb
    {
        [Key]
        public int AccountAddressMapID { get; set; }

        [ForeignKey("Accounts")]
        public virtual int AccountID { get; set; }
        public virtual AccountsDb Accounts { get; set; }

        [ForeignKey("Addresses")]
        public virtual int AddressId { get; set; }
        public virtual AddressesDb Addresses { get; set; }
    }
}
