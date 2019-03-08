using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.ValueObjects;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public interface IUsersViewModel
    {
        int UserID { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string Company { get; set; }
        string Title { get; set; }
        string PrimaryEmail { get; set; }
        IEnumerable<dynamic> Roles { get; set; }
        IList<AddressViewModel> Addresses { get; set; }
        IEnumerable<dynamic> AddressTypes { get; set; }
        IList<Phone> Phones { get; set; }
        IList<dynamic> SocialMediaUrls { get; set; }
        IEnumerable<Email> Emails { get; set; }
        short RoleID { get; set; }
        string RoleName { get; set; }
        byte Status { get; set; }
        int AccountID { get; set; }
        string Password { get; set; }
        bool IsDeleted { get; set; }
        int CreatedBy { get; set; }
        int? ModifiedBy { get; set; }
       // DateTime CreatedOn { get; set; }
        DateTime? ModifiedOn { get; set; }
        Country DefaultCountry { get; set; }
        State DefaultState { get; set; }
        bool DoNotEmail { get; set; }
    }

    public class UserViewModel : IUsersViewModel
    {
        public IEnumerable<dynamic> Roles { get; set; }
        public int UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string Title { get; set; }
        public string PrimaryEmail { get; set; }
        public IList<AddressViewModel> Addresses { get; set; }
        public IEnumerable<dynamic> AddressTypes { get; set; }
        public IEnumerable<dynamic> PhoneNumberTypes { get; set; }
        public IList<Phone> Phones { get; set; }
        public IList<dynamic> SocialMediaUrls { get; set; }
        public string Name { get { return string.Format("{0} {1}", this.FirstName, this.LastName); } }
        public byte Status { get; set; }
        public short RoleID { get; set; }
        public string RoleName { get; set; }
        public int AccountID { get; set; }
        public IEnumerable<Email> Emails { get; set; }
        public AccountViewModel Account { get; set; }
        public string Password { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string EmailSignature { get; set; }
        public Country DefaultCountry { get; set; }
        public State DefaultState { get; set; }
        public string FacebookAccessToken { get; set; }
        public string TwitterOAuthToken { get; set; }
        public string TwitterOAuthTokenSecret { get; set; }
        public bool DoNotEmail { get; set; }
        public bool? HasTourCompleted { get; set; }
    }

    public class UserEntryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
