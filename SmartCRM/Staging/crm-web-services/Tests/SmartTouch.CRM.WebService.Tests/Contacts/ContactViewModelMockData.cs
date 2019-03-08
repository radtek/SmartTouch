using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.WebService.Controllers;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.ViewModels;

using Moq;
using Moq.Linq;

namespace SmartTouch.CRM.WebService.Tests.Contacts
{
    internal class ContactViewModelMockData
    {
        public static IEnumerable<Mock<PersonViewModel>> GetMockPersons(MockRepository mockRepository)
        {
            IList<Mock<PersonViewModel>> mockPersons = new List<Mock<PersonViewModel>>();
            foreach (int i in Enumerable.Range(1, 10))
            {
                var mockPerson = mockRepository.Create<PersonViewModel>();
                mockPersons.Add(mockPerson);
            }
            return mockPersons;
        }

        public static IEnumerable<Mock<PersonViewModel>> GetMockPersonWithSetups(MockRepository mockRepository)
        {
            IList<Mock<PersonViewModel>> mockPersons = new List<Mock<PersonViewModel>>();
            foreach (int i in Enumerable.Range(1, 10))
            {
                var mockPerson = mockRepository.Create<PersonViewModel>();
                mockPerson.Setup<int>(c => c.ContactID).Returns(i);
                mockPersons.Add(mockPerson);
            }
            return mockPersons;
        }

        public static IEnumerable<Mock<CompanyViewModel>> GetMockCompanies(MockRepository mockRepository)
        {
            IList<Mock<CompanyViewModel>> mockCompanies = new List<Mock<CompanyViewModel>>();
            foreach (int i in Enumerable.Range(1, 10))
            {
                var mockCompany = mockRepository.Create<CompanyViewModel>();
                mockCompanies.Add(mockCompany);
            }
            return mockCompanies;
        }

        public static IEnumerable<Mock<CompanyViewModel>> GetMockCompaniesWithSetups(MockRepository mockRepository)
        {
            IList<Mock<CompanyViewModel>> mockCompanies = new List<Mock<CompanyViewModel>>();
            foreach (int i in Enumerable.Range(1, 10))
            {
                var mockCompany = mockRepository.Create<CompanyViewModel>();
                mockCompany.Setup<int>(c => c.ContactID).Returns(i);
                mockCompanies.Add(mockCompany);
            }
            return mockCompanies;
        }

        //public static IEnumerable<Mock<Suggestion>> GetMockCompaniesWithSetupsbyCompanyName(MockRepository mockRepository)
        //{
        //    IList<Mock<Suggestion>> mockCompanies = new List<Mock<Suggestion>>();
         
            
        //        var mockCompany = mockRepository.Create<Suggestion>();
        //        mockCompany.Setup<string>(c => c.Text).Returns("temp");
        //        mockCompanies.Add(mockCompany);
            
        //    return mockCompanies;
        //}
    }
}
