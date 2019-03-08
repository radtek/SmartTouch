using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class AccountEmailsDb
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EmailID { get; set; }
        public string Email { get; set; }
        [ForeignKey("Users")]
        public int? UserID { get; set; }
        [ForeignKey("Accounts")]
        public int AccountID { get; set; }
        public bool IsPrimary { get; set; }
        public string EmailSignature { get; set; }
    
        public virtual AccountsDb Accounts { get; set; }
        public virtual UsersDb Users { get; set; }
        public virtual ServiceProvidersDb ServiceProviders { get; set; }
         [ForeignKey("ServiceProviders")]
        public int ServiceProviderID { get; set; }
    }
}
