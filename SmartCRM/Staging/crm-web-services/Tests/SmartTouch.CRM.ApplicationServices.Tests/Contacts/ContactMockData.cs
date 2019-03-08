using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.Domain.Contacts;

using Moq;
using Moq.Linq;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.Users;

namespace SmartTouch.CRM.ApplicationServices.Tests.Contacts
{
    public class ContactMockData
    {
        public static IEnumerable<Contact> GetMockContacts(MockRepository mockRepository, int objectCount)
        {
            IList<Contact> mockContacts = new List<Contact>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                var mockContact = mockRepository.Create<Person>();
                mockContacts.Add(mockContact.Object);
            }
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                var mockContact = mockRepository.Create<Company>();
                mockContacts.Add(mockContact.Object);
            }
            return mockContacts;
        }

        public static IEnumerable<Person> GetMockPersons(MockRepository mockRepository, int objectCount)
        {
            IList<Person> mockContacts = new List<Person>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                //var mockContact = mockRepository.Create<Person>();
                mockContacts.Add(new Person()
                {
                    Id = 1,
                    Addresses = new List<Address>() { new Address() { AddressID = 1, State = new State() { Code = "NEWYORK" }, Country = new Country() { Code = "US" } } },
                    CustomFields = new List<ContactCustomField>() { new ContactCustomField() { ContactId = 1, Value = "Tab1", CustomFieldId = 2, ContactCustomFieldMapId = 1 } }
                });

                //mockContacts.Add(mockContact.Object);
            }
            return mockContacts;
        }

        public static IEnumerable<Mock<Person>> GetMockPersonsWithSetups(MockRepository mockRepository, int objectCount)
        {
            IList<Mock<Person>> mockContacts = new List<Mock<Person>>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                var mockContact = mockRepository.Create<Person>();
                mockContact.Setup<int>(c => c.Id).Returns(i);
                //mockContact.Setup<IEnumerable<Address>>(c => c.Addresses).Returns(new Address() { AddressID = i, Country = new Country() { Code = "US" }, State = new State() { Code = "Amazon" } });
                mockContacts.Add(mockContact);
            }
            return mockContacts;
        }

        public static IEnumerable<Owner> GetMockUsers(MockRepository mockRepository, int objectCount)
        {
            IList<Owner> mockUsers = new List<Owner>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                var mockUser = mockRepository.Create<Owner>();
                mockUsers.Add(mockUser.Object);
            }
            return mockUsers;
        }

        public static IList<Contact> GetMockContactsForTours(MockRepository mockRepository, int objectCount)
        {
            IList<Contact> mockContacts = new List<Contact>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                //Person person = new Person() { FirstName = "Bharath" + i };
                var mockContact = mockRepository.Create<Person>();
                mockContacts.Add(mockContact.Object);
            }
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                //Person person = new Person() { FirstName = "Bharath" + i };
                var mockContact = mockRepository.Create<Company>();
                mockContacts.Add(mockContact.Object);
            }
            return mockContacts;
        }

        public static IEnumerable<PersonViewModel> GetMockPersonViewModels(MockRepository mockRepository, int objectCount)
        {
            IList<PersonViewModel> mockContacts = new List<PersonViewModel>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                var mockContact = mockRepository.Create<PersonViewModel>();
                mockContacts.Add(mockContact.Object);
            }
            return mockContacts;
        }

        public static PersonViewModel GetMockPersonViewModel(MockRepository mockRepository, int Id)
        {
            var mockContact = new PersonViewModel();
            mockContact.ContactID = 1;
            return mockContact;
        }

        public static IList<ContactEntry> GetMockContactEntrys(MockRepository mockRepository, int objectCount)
        {
            IList<ContactEntry> mockContacts = new List<ContactEntry>();

            foreach (int i in Enumerable.Range(1, objectCount))
            {
                var mockContact = mockRepository.Create<ContactEntry>();
                mockContacts.Add(mockContact.Object);
            }
            return mockContacts;
        }

        public static ContactEntry GetMockContactEntry(MockRepository mockRepository, int Id)
        {
            var mockContactEntry = new ContactEntry() { Id = 1, FullName = "Unit-test" };
            return mockContactEntry;
        }

        public static IEnumerable<ContactCustomFieldMapViewModel> GetContactField(MockRepository mockRepository, int objectCount)
        {
            IList<ContactCustomFieldMapViewModel> mockCustomFields = new List<ContactCustomFieldMapViewModel>();

            foreach (int i in Enumerable.Range(1, objectCount))
            {
                var mockContact = mockRepository.Create<ContactCustomFieldMapViewModel>();
                mockCustomFields.Add(mockContact.Object);
            }
            return mockCustomFields;
        }

        public static CustomFieldTabViewModel GetCustomeFieldTabs(MockRepository mockRepository, int count)
        {
            //CustomFieldTabsViewModel mockCustomFields = new CustomFieldTabsViewModel();
            //CustomFieldTabsViewModel viewModel = new CustomFieldTabsViewModel();
            //IList<CustomFieldTabViewModel> viewModel = new List<CustomFieldTabViewModel>();
            //IList<CustomFieldSectionViewModel> viewModel2 = new List<CustomFieldSectionViewModel>();
            //viewModel2.Add(new CustomFieldSectionViewModel() { CustomFieldSectionId = 1, Name = "Tab2", SortId = 3 });

            //foreach (int i in Enumerable.Range(1, count))
            //{
            //    viewModel.Add(new CustomFieldTabViewModel()
            //    {
            //        AccountId = 1,
            //        CustomFieldTabId = 1,
            //        Name = "Tab1" + i,
            //        Sections = viewModel2,
            //        SortId = 3 
            //    });
            //}
            //return viewModel;
            CustomFieldTabViewModel viewModel = new CustomFieldTabViewModel();
            viewModel.AccountId = 1;
            viewModel.CustomFieldTabId = 1;
            viewModel.Name = "Tab1";
            viewModel.Sections = null;
            viewModel.SortID = 3;
            return viewModel;
        }
    }
}