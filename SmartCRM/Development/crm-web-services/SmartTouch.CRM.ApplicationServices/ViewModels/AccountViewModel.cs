using SmartTouch.CRM.Domain.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    /// <summary>
    /// AccountViewModel properties, interface is for unit testing using mock objects.
    /// </summary>
    public interface IAccountViewModel
    {
        int AccountID { get; set; }
        string AccountName { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string Company { get; set; }
        int CreatedBy { get; set; }
        string PrimaryEmail { get; set; }
        IList<dynamic> SecondaryEmails { get; set; }
        IList<dynamic> SocialMediaUrls { get; set; }
        IList<dynamic> Phones { get; set; }
        IList<dynamic> Subscriptions { get; set; }
        IList<dynamic> Countries { get; set; }
        string CountryID { get; set; }
        IList<dynamic> DateFormats { get; set; }
        byte DateFormatID { get; set; }
        IList<dynamic> Currency { get; set; }
        DateTime? ModifiedOn { get; set; }
        int? ModifiedBy { get; set; }
        DateTime CreatedOn { get; set; }
        int ContactsCount { get; set; }
        int EmailsCount { get; set; }
        string DateFormat { get; set; }

        int? LogoImageID { get; set; }
        ImageViewModel Image { get; set; }

        byte CurrencyID { get; set; }
        string CurrencyFormat { get; set; }
        IList<AddressViewModel> Addresses { get; set; }
        CommunicationViewModel Communication { get; set; }
        byte Status { get; set; }
        string StatusMessage { get; set; }
        string PrivacyPolicy { get; set; }
        string DomainURL { get; set; }
        string PreviousDomainURL { get; set; }
        string TimeZone { get; set; }
        IEnumerable<ModuleViewModel> Modules { get; set; }
        IEnumerable<ModuleViewModel> SubscribedModules { get; set; }
        IEnumerable<ProviderRegistrationViewModel> ServiceProviderRegistrationDetails { get; set; }
        byte? OpportunityCustomers { get; set; }
        string FacebookAPPID { get; set; }
        string FacebookAPPSecret { get; set; }
        string TwitterAPIKey { get; set; }
        string TwitterAPISecret { get; set; }
        bool? Disclaimer { get; set; }
        string LitmusAPIKey { get; set; }
        IEnumerable<ProviderViewModel> CampaignProviders { get; set; }
        IEnumerable<Role> Roles { get; set; }
    }

    public class AccountViewModel : IAccountViewModel
    {
        public int AccountID { get; set; }
        public string AccountName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string PrimaryEmail { get; set; }
        public IList<dynamic> Phones { get; set; }
        public IList<dynamic> SocialMediaUrls { get; set; }
        public IList<dynamic> SecondaryEmails { get; set; }

        public IEnumerable<dynamic> AddressTypes { get; set; }
        public IList<dynamic> Subscriptions { get; set; }
        public IList<AddressViewModel> Addresses { get; set; }
        public CommunicationViewModel Communication { get; set; }
        public byte Status { get; set; }
        public string StatusMessage { get; set; }
        public byte PreviousStatus { get; set; }
        public string PrivacyPolicy { get; set; }
        public string SubscriptionName { get; set; }
        public byte SubscriptionId { get; set; }
        public IList<dynamic> Countries { get; set; }
        public string CountryID { get; set; }
        public IList<dynamic> DateFormats { get; set; }
        public byte DateFormatID { get; set; }
        public IList<dynamic> Currency { get; set; }
        public byte CurrencyID { get; set; }
        public string CurrencyFormat { get; set; }
        public string DomainURL { get; set; }
        public string PreviousDomainURL { get; set; }
        public string TimeZone { get; set; }
        public string GoogleDriveClientID { get; set; }
        public string GoogleDriveAPIKey { get; set; }
        public string DropboxAppKey { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int ContactsCount { get; set; }
        public int EmailsCount { get; set; }
        public string DateFormat { get; set; }
        public int SenderReputationCount { get; set; }
        public int ActiveUsersCount { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? LastCampaignSent { get; set; }
        public byte SubscriptionID { get; set; }

        public int? LogoImageID { get; set; }
        public ImageViewModel Image { get; set; }
        public string AccountLogoUrl { get; set; }

        public IEnumerable<ProviderRegistrationViewModel> ServiceProviderRegistrationDetails { get; set; }
        public IEnumerable<ProviderViewModel> CampaignProviders { get; set; }
        public string DefaultVMTA { get; set; }
        public IEnumerable<ModuleViewModel> Modules { get; set; }
        public IEnumerable<ModuleViewModel> SubscribedModules { get; set; }
        public byte? OpportunityCustomers { get; set; }
        public WebAnalyticsProviderViewModel WebAnalyticsProvider { get; set; }
        public string FacebookAPPID { get; set; }
        public string FacebookAPPSecret { get; set; }
        public string TwitterAPIKey { get; set; }
        public string TwitterAPISecret { get; set; }
        public string HelpURL { get; set; }
        public bool ShowTC { get; set; }
        public string TC { get; set; }
        public bool? Disclaimer { get; set; }
        public string LitmusAPIKey { get; set; }
        public IEnumerable<ImageDomainViewModel> ImageDomains { get; set; }

        public int? UserLimit { get; set; }
        public int? PreviousUserLimit { get; set; }
        public IEnumerable<Role> Roles { get; set; }
        public IEnumerable<short> SelectedRoles { get; set; }
    }

    public interface IAccountsListViewModel
    {
        IEnumerable<AccountViewModel> Accounts { get; set; }
    }

    public class AccountsListViewModel : IAccountsListViewModel
    {
        public IEnumerable<AccountViewModel> Accounts { get; set; }
    }

    public class AccountListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
