using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.ValueObjects;
using System.Collections;

namespace SmartTouch.CRM.ApplicationServices.Tests.Accounts
{
    public class AccountMockData
    {
        public static IEnumerable<Account> GetMockAccounts(MockRepository mockRepository, int objectCount)
        {
            IList<Account> mockContacts = new List<Account>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                //Person person = new Person() { FirstName = "Bharath" + i };
                var mockContact = mockRepository.Create<Account>();
                mockContact.Object.Id = 1;
                mockContacts.Add(mockContact.Object);
            }

            return mockContacts;
        }

        public static IEnumerable<AccountViewModel> AllAccounts(MockRepository mockRepository)
        {
            IList<AccountViewModel> mockAccounts = new List<AccountViewModel>();
            IList<dynamic> urls = new List<dynamic>();
            foreach (int i in Enumerable.Range(1, 5))
            {
                var mockAccount = new AccountViewModel()
                {
                    AccountID = i,
                    AccountName = "Account" + i,
                    FirstName = "Srinivas",                    
                    PrimaryEmail = "satya@gmail.com", 
                    SocialMediaUrls = urls 
                };
                mockAccounts.Add(mockAccount);
            }
            return mockAccounts;
        }

        public static AccountViewModel GetAccountViewModel()
        {

            IList<dynamic> url = new List<dynamic>();
            IList<AddressViewModel> addr = new List<AddressViewModel>() { new AddressViewModel() { Country = new Country { Name = "India" }, State = new State { Name = "Andhra Pradesh" } } };
            IEnumerable<ContactEntry> contact = new List<ContactEntry>() { new ContactEntry() { Id = 1, FullName = "Unit-test" } };
            IList<Phone> phones = new List<Phone>();
            phones.Add(new Phone() { Number = "9999999999", PhoneType = 1, IsPrimary = true });

            AccountViewModel userViewModel = new AccountViewModel();

            userViewModel.FirstName = "Srinivas";
            userViewModel.LastName = "Naidu";
            userViewModel.AccountName = "Srinivas Naidu";
            userViewModel.PrimaryEmail = "satya@gmail.com";
            //userViewModel.Phones = (IList)phones.Cast<dynamic>();
            userViewModel.Addresses = addr;
            userViewModel.SocialMediaUrls = url;
            return userViewModel;
        }

        public static Account GetAccountClass()
        {
            Account acc = new Account();
            acc.FirstName = "Srinivas";
            acc.Id = 10;
            acc.LastName = "Naidu";
            acc.AccountName = "Srinivas Naidu";
            acc.Email = new Domain.ValueObjects.Email { EmailId = "satya@gmail.com" };
            return acc;
        }

        public static AccountViewModel GetUpdatedAccountViewModel()
        {

            IList<dynamic> url = new List<dynamic>();
            IList<AddressViewModel> addr = new List<AddressViewModel>() { new AddressViewModel() { AddressLine1 = "US1", AddressLine2 = "US2", City = "USCITY", State = new State() { Code = "US" }, Country = new Country() { Code = "NewYork" } } };
            IList<dynamic> phones = new List<dynamic>();
            phones.Add(new { Number = "9999999999", PhoneType = "Home", IsPrimary = true });
            phones.Add(new { Number = "8888888888", PhoneType = "Mobile", IsPrimary = false });

            AccountViewModel accountViewModel = new AccountViewModel();
            accountViewModel.FirstName = "Srinivasa";
            accountViewModel.LastName = "Naidu";
            accountViewModel.AccountID = 10;
            accountViewModel.AccountName = "Srinivas Naidu";
            accountViewModel.SocialMediaUrls = url;
            accountViewModel.Phones = phones;
            accountViewModel.Addresses = addr;
            accountViewModel.PrimaryEmail = "satya@gmail.com";
            return accountViewModel;
        }
    }
}
