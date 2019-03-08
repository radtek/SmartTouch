using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.ValueObjects;

namespace SmartTouch.CRM.ApplicationServices.Tests.Users
{
    [TestClass]
    public class UserMockData
    {
        public static IEnumerable<User> GetMockUsers(MockRepository mockRepository, int objectCount)
        {
            IList<User> mockContacts = new List<User>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                //Person person = new Person() { FirstName = "Bharath" + i };
                var mockContact = mockRepository.Create<User>();
                mockContacts.Add(mockContact.Object);
            }

            return mockContacts;
        }


        public static IList<UserViewModel> GetUserViewModel()
        {
            IList<UserViewModel> userViewModelList = new List<UserViewModel>();
            UserViewModel userViewModel = new UserViewModel();
            foreach (int item in Enumerable.Range(1, 5))
            {
                userViewModel.UserID = item;
                userViewModel.FirstName = "Srinivas"+item;
                userViewModel.LastName = "Naidu" + item;
                userViewModel.RoleID = 2;
                userViewModel.PrimaryEmail = "satya@gmail.com" + item;
                userViewModelList.Add(userViewModel);
            }
            return userViewModelList;
        }

        public static User GetUserClass()
        {
            User user = new User();
            user.FirstName = "Srinivas";
            user.LastName = "Naidu";
            user.RoleID = 2;
            user.Email = new Domain.ValueObjects.Email { EmailId = "satya@gmail.com" };
            return user;
        }

        public static UserViewModel GetUpdatedUserViewModel()
        {

            IList<dynamic> url = new List<dynamic>();
            IList<AddressViewModel> addr = new List<AddressViewModel>();
            IList<Phone> Phones = new List<Phone>();

            UserViewModel userViewModel = new UserViewModel();
            userViewModel.FirstName = "Srinivasa";
            userViewModel.LastName = "Naidu";
            userViewModel.RoleID = 2;
            userViewModel.UserID = 61;
            userViewModel.SocialMediaUrls = url;
            userViewModel.Phones = Phones;
            userViewModel.Addresses = addr;
            userViewModel.PrimaryEmail = "satya@gmail.com";
            return userViewModel;
        }


    }
}
