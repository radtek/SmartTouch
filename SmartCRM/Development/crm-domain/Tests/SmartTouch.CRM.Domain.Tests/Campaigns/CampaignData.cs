using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Domain.Tests.Campaigns
{
    public class CampaignData
    {        
        public Campaign GetCustomCampaign(string name, string subject, string content, CampaignTemplate campaignTemplate,  int contactsCount,int id = 0)
        {
            Campaign campaign = new Campaign();
            campaign.AccountID = 1;
            campaign.Subject = subject;
            campaign.HTMLContent= content;
            campaign.Name = name;
            campaign.Template = campaignTemplate;
            campaign.Id = id;
            campaign.Contacts = GetSampleContacts(contactsCount);
            return campaign;
        }

        public Campaign GetCustomCampaign(CampaignStatus campaignStatus, byte emailProviderId, string from, int contactsCount, DateTime scheduledTime, DateTime sentOn, Email testEmail)
        {
            Campaign campaign = new Campaign();
            campaign.CampaignStatus = campaignStatus;
            campaign.ServiceProviderID = emailProviderId;
            campaign.From = from;
            campaign.ScheduleTime = scheduledTime;
            campaign.Contacts = GetSampleContacts(contactsCount);
            campaign.TestEmail = testEmail;
            return campaign;
        }
        public Campaign GetCustomCampaign(Campaign campaign, CampaignStatus campaignStatus, byte emailProviderId, string from,  DateTime scheduledTime, DateTime sentOn, Email testEmail)
        {
            Campaign campaign1 = new Campaign();
            campaign1 = GetCustomCampaign(null, "TestSubject", "<div><h1>Hello</h1></div>", new CampaignTemplate() { Id = 1, Type = CampaignTemplateType.Layout }, 1, 5);
            campaign1.CampaignStatus = campaignStatus;
            campaign1.ServiceProviderID = emailProviderId;
            campaign1.From = from;
            campaign1.ScheduleTime = scheduledTime;
            campaign1.TestEmail = testEmail;
            return campaign1;
        }

        public string GetSampleMailingList(int count)
        {
            string mailingList = string.Empty;
            for (int i = 0; i < count; i++)
            {
                Email email = new Email() { EmailId = "sample" + i + "@stcrm.com" };
                mailingList += email + ",";
            }
            return mailingList;
        }

        public IList<Contact> GetSampleContacts(int count)
        {
            IList<Contact> contacts = new List<Contact>();
            for (int i = 0; i < count; i++)
            {
                if (i % 2 == 0)
                {
                    Person person = new Person();
                    //person.Email = new Email() { EmailId = "sample" + i + "@stcrm.com" };
                    contacts.Add(person);
                }
                else
                {
                    Company company = new Company();
                    //company.Email = new Email() { EmailId = "sample" + i + "@stcrm.com" };
                    contacts.Add(company);
                }
                
            }
            return contacts;
        }
    }
}


