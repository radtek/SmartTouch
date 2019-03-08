using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class UserAddressDb
    {
        [Key]
        public int UserAddressMapID { get; set; }

        [ForeignKey("Users")]
        public virtual int UserID { get; set; }
        public virtual UsersDb Users { get; set; }

        [ForeignKey("Addresses")]
        public virtual int AddressId { get; set; }
        public virtual AddressesDb Addresses { get; set; }
    }
}
