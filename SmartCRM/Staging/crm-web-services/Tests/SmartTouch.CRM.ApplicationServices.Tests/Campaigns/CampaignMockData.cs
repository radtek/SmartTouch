using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.Tests.Campaigns
{
    public class CampaignMockData
    {
        public static CampaignViewModel CreateCampaignViewModelWithCustomValues(MockRepository mockRepository, int campaignId
            , string name, string subject, DateTime? scheduleTime, string from, int contactsCount, CampaignStatus campaignStatus, byte templateId, int campaignTagCount,int contactTagCount)
        {
            CampaignViewModel campaignViewModel = new CampaignViewModel();
            campaignViewModel.CampaignID = campaignId;
            campaignViewModel.Name = name;
            campaignViewModel.Subject = subject;
            campaignViewModel.ScheduleTime = scheduleTime;
            campaignViewModel.From = from;
            campaignViewModel.HTMLContent = "test";
            campaignViewModel.CampaignStatus = campaignStatus;
            campaignViewModel.CampaignTemplate = new CampaignTemplateViewModel() { TemplateId = templateId };
            campaignViewModel.Contacts = CampaignMockData.GetMockCampaignContactViewModels(mockRepository, 5);
            var list = new List<TagViewModel>() { new TagViewModel() { TagID = 1 }, new TagViewModel() { TagID = 2 } };
            for (int i = 0; i < campaignTagCount; i++)
            {
                list.Add(new TagViewModel() { TagID = i });
            }
            campaignViewModel.TagsList = list;
            list.Clear();
            for (int i = 0; i < campaignTagCount; i++)
            {
                list.Add(new TagViewModel() { TagID = i });
            }
            campaignViewModel.ContactTags = list;
            return campaignViewModel;
        }

        public static Campaign CreateCampaignWithCustomValues(MockRepository mockRepository, int campaignId
            , string name, DateTime? scheduleTime, string from, int contactsCount, CampaignStatus campaignStatus, CampaignTemplate template)
        {
            Campaign campaign = new Campaign();
            campaign.Id = campaignId;
            campaign.Name = name;
            campaign.ScheduleTime = scheduleTime;
            campaign.From = from;
            campaign.CampaignStatus = campaignStatus;
            campaign.Template = template;
            //campaign.To = CampaignMockData.GetMockContacts(mockRepository, 5);
            return campaign;
        }

        public static Campaign CreateCampaignWithCustomValues(MockRepository mockRepository, int campaignId, string name, DateTime? scheduleTime
            , string from, int contactsCount, CampaignStatus campaignStatus, CampaignTemplate template, string subject, string content, byte emailProviderID)
        {
            Campaign campaign = new Campaign();
            campaign.Id = campaignId;
            campaign.Name = name;
            campaign.ScheduleTime = scheduleTime;
            campaign.From = from;
            campaign.Contacts = CampaignMockData.GetMockContacts(mockRepository, 5).ToList();
            campaign.CampaignStatus = campaignStatus;
            campaign.Template = template;
            campaign.Subject = subject;
            campaign.HTMLContent = content;
            campaign.ServiceProviderID = emailProviderID;

            return campaign;
        }

        public static IEnumerable<Contact> GetMockContacts(MockRepository mockRepository, int objectCount)
        {
            IList<Contact> mockContacts = new List<Contact>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                //Person person = new Person() { FirstName = "Bharath" + i };
                var mockContact = mockRepository.Create<Person>();
                IList<Email> emails = new List<Email>();
                emails.Add(new Email() { EmailId = "email" + i + "@somemail.com" });
                mockContact.Object.Emails = emails;
                mockContacts.Add(mockContact.Object);
            }
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                //Person person = new Person() { FirstName = "Bharath" + i };
                var mockContact = mockRepository.Create<Company>();
                IList<Email> emails = new List<Email>();
                emails.Add(new Email() { EmailId = "email" + i + "@somemail.com" });
                mockContact.Object.Emails = emails;
                mockContacts.Add(mockContact.Object);
            }
            return mockContacts;
        }

        public static IEnumerable<Person> GetPersons(MockRepository mockRepository, int objectCount)
        {
            IList<Person> persons = new List<Person>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                Person person = new Person();
                //person.Email.EmailId = "email" + i + "@somemail.com";
                persons.Add(person);
            }
            return persons;
        }

        public static IEnumerable<Company> GetCompanies(MockRepository mockRepository, int objectCount)
        {
            IList<Company> companies = new List<Company>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                Company company = new Company();
                //company.Email.EmailId = "email" + i + "@somemail.com";
                companies.Add(company);
            }
            return companies;
        }

        public static IList<ContactEntry> GetMockCampaignContactViewModels(MockRepository mockRepository, int objectCount)
        {
            IList<ContactEntry> mockContacts = new List<ContactEntry>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                var mockContact = mockRepository.Create<ContactEntry>();
                mockContacts.Add(mockContact.Object);
            }
            return mockContacts;
        }
    }
}