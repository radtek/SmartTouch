using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class AccountsDb
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AccountID { get; set; }
        public string AccountName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string PrimaryEmail { get; set; }
        public string HomePhone { get; set; }
        public string MobilePhone { get; set; }
        public string WorkPhone { get; set; }
        public string PrivacyPolicy { get; set; }
        [ForeignKey("Communication")]
        public virtual int? CommunicationID { get; set; }
        public virtual CommunicationsDb Communication { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int ContactsCount { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int EmailsCount { get; set; }

        [ForeignKey("Subscription")]
        public virtual byte SubscriptionID { get; set; }
        public virtual SubscriptionsDb Subscription { get; set; }

        public byte DateFormatID { get; set; }
        public byte CurrencyID { get; set; }
        [NotMapped]
        public string CurrencyFormat { get; set; }
        public string CountryID { get; set; }
        public string TimeZone { get; set; }
        public byte Status { get; set; }        
        public string StatusMessage  { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public string GoogleDriveClientID { get; set; }
        public string GoogleDriveAPIKey { get; set; }
        public string DropboxAppKey { get; set; }
        public ICollection<AddressesDb> Addresses { get; set; }
        public string DomainURL { get; set; }
        [NotMapped]
        public ICollection<ModulesDb> SubscribedModules { get; set; }
        public byte? OpportunityCustomers { get; set; }
        [NotMapped]
        public virtual WebAnalyticsProvidersDb WebAnalyticsProvider { get; set; }

        [ForeignKey("Image")]
        public virtual int? LogoImageID { get; set; }
        public virtual ImagesDb Image { get; set; }
        public string FacebookAPPID { get; set; }
        public string FacebookAPPSecret { get; set; }
        public string TwitterAPIKey { get; set; }
        public string TwitterAPISecret { get; set; }
        [NotMapped]
        public string DateFormat { get; set; }
        public string HelpURL { get; set; }

        public bool ShowTC { get; set; }
        public string TC { get; set; }
        public bool? Disclaimer { get; set; }
        public string LitmusAPIKey { get; set; }

        [NotMapped]
        public int? UserLimit { get; set; }
        [NotMapped]
        public string ExcludedRoles { get; set; }
    }
}
