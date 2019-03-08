using AutoMapper;
using LandmarkIT.Enterprise.CommunicationManager.Requests;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.AccountSettings;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.CustomFields;
using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.Domain.Fields;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Domain.ImageDomains;
using SmartTouch.CRM.Domain.Images;
using SmartTouch.CRM.Domain.ImportData;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Domain.LeadScoreRules;
using SmartTouch.CRM.Domain.Login;
using SmartTouch.CRM.Domain.MarketingMessageCenter;
using SmartTouch.CRM.Domain.Modules;
using SmartTouch.CRM.Domain.Notes;
using SmartTouch.CRM.Domain.Opportunities;
using SmartTouch.CRM.Domain.Reports;
using SmartTouch.CRM.Domain.Search;
using SmartTouch.CRM.Domain.SeedList;
using SmartTouch.CRM.Domain.SuppressedEmails;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.Tours;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.WebAnalytics;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using DA = SmartTouch.CRM.Domain.Actions;

namespace SmartTouch.CRM.ApplicationServices.ObjectMappers
{
    public class ViewModelToEntityProfile : Profile
    {
        public new string ProfileName
        {
            get
            {
                return "EntityToViewModelProfile";
            }
        }

        protected override void Configure()
        {
            this.CreateMap<CampaignViewModel, Campaign>()
                .ForMember(c => c.Id, opt => opt.MapFrom(c => c.CampaignID))
                .ForMember(c => c.CampaignStatus, opt => opt.MapFrom(c => c.CampaignStatus))
                .ForMember(c => c.TagRecipients, opt => opt.MapFrom(c => c.ToTagStatus))
                .ForMember(c => c.SSRecipients, opt => opt.MapFrom(c => c.SSContactsStatus))
                .ForMember(c => c.Contacts, opt => opt.Ignore())
                .ForMember(c => c.Tags, opt => opt.Ignore())
                .ForMember(c => c.ContactTags, opt => opt.Ignore())
                .ForMember(c => c.SearchDefinitions, opt => opt.Ignore())
                .ForMember(c => c.Links, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<CampaignLinkViewModel>, IEnumerable<CampaignLink>>(c.Links)))
                .AfterMap((s, d) =>
                {
                    if (s.TagsList != null)
                    {
                        var tags = new List<Tag>();
                        foreach (TagViewModel tag in s.TagsList)
                        {
                            tags.Add(Mapper.Map<TagViewModel, Tag>(tag));
                        }
                        d.Tags = tags;
                    }

                    if (s.ContactTags != null)
                    {
                        var tags = new List<Tag>();
                        foreach (TagViewModel tag in s.ContactTags)
                        {
                            tags.Add(Mapper.Map<TagViewModel, Tag>(tag));
                        }
                        d.ContactTags = s.IsLinkedToWorkflows ? new List<Tag>() : tags;
                    }

                    if (s.Contacts != null)
                    {
                        var contacts = new List<Contact>();
                        foreach (ContactEntry entry in s.Contacts)
                        {
                            Contact contact;
                            if (entry.ContactType == (int)ContactType.Person)
                                contact = new Person() { Id = entry.Id };
                            else
                                contact = new Company() { Id = entry.Id };
                            contacts.Add(contact);
                        }
                        d.Contacts = contacts;
                    }
                    if (s.CampaignTemplate == null)
                    {
                        d.Template = new CampaignTemplate() { Id = 5 };  //This should be handled. We have to name this template as blank in DB.
                    }
                    else
                    {
                        d.Template = Mapper.Map<CampaignTemplateViewModel, CampaignTemplate>(s.CampaignTemplate);
                    }

                    if ((s.CampaignStatus == CampaignStatus.Scheduled && s.ScheduleTime == null) || s.CampaignStatus == CampaignStatus.Queued)
                        d.ScheduleTime = DateTime.Now.ToUniversalTime();
                    else if (s.CampaignStatus == CampaignStatus.Scheduled)
                        d.ScheduleTime = s.ScheduleTime;
                    else if (s.CampaignStatus == CampaignStatus.Draft)
                        d.ScheduleTime = null;


                    d.SearchDefinitions = new List<SearchDefinition>();
                    if (s.SearchDefinitions != null)
                    {
                        d.SearchDefinitions =
                            (s.SearchDefinitions.Count == 0 || s.IsLinkedToWorkflows) ? new List<SearchDefinition>() :
                            Mapper.Map<IEnumerable<AdvancedSearchViewModel>, IEnumerable<SearchDefinition>>(s.SearchDefinitions).ToList();
                    }
                });

            this.CreateMap<CampaignLinkViewModel, CampaignLink>()
                .ForMember(c => c.CampaignId, opt => opt.MapFrom(c => c.CampaignId))
                .AfterMap((s, d) =>
                {
                    if (s.URL != null)
                    {
                        d.URL.URL = Uri.UnescapeDataString(s.URL.URL);
                    }
                });

            this.CreateMap<CampaignThemeViewModel, CampaignTheme>()
                .ForMember(t => t.CampaignThemeID, opt => opt.MapFrom(c => c.CampaignThemeID));

            this.CreateMap<CampaignTemplateViewModel, CampaignTemplate>()
                .ForMember(c => c.Id, opt => opt.MapFrom(c => c.TemplateId));
            //.AfterMap((s, d) =>
            //{
            //    d.Id = s.TemplateId;
            //});

            this.CreateMap<AddressViewModel, Address>()
               .ForMember(a => a.AddressTypeID, opt => opt.MapFrom(c => (AddressType)c.AddressTypeID))
               .ForMember(a => a.State, opt => opt.MapFrom(c => new State() { Code = c.State.Code == "" ? null : c.State.Code, Name = c.State.Name }))
               .ForMember(a => a.Country, opt => opt.MapFrom(c => new Country() { Code = c.Country.Code == "" ? null : c.Country.Code, Name = c.Country.Name }));

            this.CreateMap<ReportListEntry, Report>()
               .ForMember(t => t.ReportName, opt => opt.MapFrom(c => c.ReportName))
                .ForMember(t => t.LastUpdatedOn, opt => opt.MapFrom(c => c.LastRunOn));
            this.CreateMap<string, Email>().ConvertUsing(c => new Email() { EmailId = c });

            this.CreateMap<ImageViewModel, ImageDNU>().
                ForMember(a => a.Id, opt => opt.MapFrom(c => c.ImageID));

            this.CreateMap<ImageViewModel, Image>()
                .ForMember(a => a.Id, opt => opt.MapFrom(c => c.ImageID))
               .ForMember(a => a.CategoryId, opt => opt.MapFrom(c => (byte)c.ImageCategoryID));

            this.CreateMap<LeadScoreViewModel, LeadScore>()
            .ForMember(l => l.Id, opt => opt.MapFrom(l => l.LeadScoreID));

            this.CreateMap<LeadScoreConditionValueViewModel, LeadScoreConditionValue>();


            this.CreateMap<SubmittedFormViewModel, SubmittedFormData>()
               .ForMember(p => p.AccountID, opt => opt.MapFrom(c => c.AccountId))
               .ForMember(p => p.CreatedOn, opt => opt.MapFrom(c => c.SubmittedOn))
               .ForMember(p => p.FormID, opt => opt.MapFrom(c => c.FormId))
               .ForMember(p => p.IPAddress, opt => opt.MapFrom(c => c.IPAddress))
               .ForMember(p => p.LeadSourceID, opt => opt.MapFrom(c => c.LeadSourceID))
               .ForMember(p => p.Status, opt => opt.MapFrom(c => (int)c.Status))
               .ForMember(p => p.OwnerID, opt => opt.MapFrom(c => c.OwnerId))
               .ForMember(p => p.CreatedBy, opt => opt.MapFrom(c => c.CreatedBy))
               .ForMember(p => p.STITrackingID, opt => opt.MapFrom(c => c.STITrackingID));


            this.CreateMap<SubmittedFormFieldViewModel, SubmittedFormFieldData>()
              .ForMember(s => s.Field, opt => opt.MapFrom(c => c.Key))
              .ForMember(s => s.Value, opt => opt.MapFrom(c => c.Value));

            this.CreateMap<PersonViewModel, Person>()
                .ForMember(p => p.Id, opt => opt.MapFrom(c => c.ContactID))
                .ForMember(p => p.Addresses, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<AddressViewModel>, IEnumerable<Address>>(c.Addresses)))
                .ForMember(p => p.ContactImage, opt => opt.MapFrom(c => Mapper.Map<ImageViewModel, Image>(c.Image)))
                .ForMember(p => p.PartnerType, opt => opt.MapFrom(c => (PartnerType?)c.PartnerType))
                .ForMember(p => p.LifecycleStage, opt => opt.MapFrom(c => (LifecycleStage)c.LifecycleStage))
                .ForMember(p => p.ImageUrl, opt => opt.MapFrom(c => c.ContactImageUrl))
                .ForMember(p => p.Company_Name, opt => opt.MapFrom(c => c.CompanyName))
                .ForMember(p => p.ProfileImageKey, opt => opt.MapFrom(c => c.ProfileImageKey))
                .ForMember(c => c.IndexedOn, opt => opt.MapFrom(c => DateTime.Now.ToUniversalTime()))
                .ForMember(p => p.LeadSources, opt => opt.Ignore())
                .ForMember(p => p.Tags, opt => opt.Ignore())
                .ForMember(p => p.Phones, opt => opt.Ignore())
                .ForMember(p => p.Emails, opt => opt.Ignore())
                .ForMember(p => p.FacebookUrl, opt => opt.Ignore())
                .ForMember(p => p.TwitterUrl, opt => opt.Ignore())
                .ForMember(p => p.GooglePlusUrl, opt => opt.Ignore())
                .ForMember(p => p.LinkedInUrl, opt => opt.Ignore())
                .ForMember(p => p.BlogUrl, opt => opt.Ignore())
                .ForMember(p => p.WebsiteUrl, opt => opt.Ignore())
                .ForMember(p => p.CompanyNameAutoComplete, opt => opt.Ignore())
                .ForMember(p => p.TitleAutoComplete, opt => opt.Ignore())
                .ForMember(p => p.ContactFullNameAutoComplete, opt => opt.Ignore())
                .ForMember(p => p.CustomFields, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<ContactCustomFieldMapViewModel>, IEnumerable<ContactCustomField>>(c.CustomFields)))
                .AfterMap((s, d) =>
                {
                    if (s.Phones != null && s.Phones.Any())
                    {
                        List<Phone> phones = new List<Phone>();
                        foreach (dynamic phone in s.Phones)
                        {
                            if (!String.IsNullOrEmpty(phone.Number))
                            {
                                Phone newPhone = new Phone()
                                {
                                    AccountID = s.AccountID,
                                    ContactID = s.ContactID,
                                    IsPrimary = phone.IsPrimary,
                                    Number = phone.Number,
                                    PhoneType = phone.PhoneType,
                                    PhoneTypeName = phone.PhoneTypeName,
                                    ContactPhoneNumberID = phone.ContactPhoneNumberID,
                                    CountryCode = phone.CountryCode,
                                    Extension = phone.Extension
                                };
                                phones.Add(newPhone);
                                d.Phones = phones;
                            }
                        }
                    }

                    if (s.Emails != null && s.Emails.Any())
                    {
                        List<Email> emails = new List<Email>();
                        foreach (dynamic email in s.Emails)
                        {
                            if (email.EmailId != null)
                            {
                                Email newEmail = new Email()
                                {
                                    AccountID = s.AccountID,
                                    IsPrimary = email.IsPrimary,
                                    EmailId = email.EmailId,
                                    EmailStatusValue = (EmailStatus)email.EmailStatusValue,
                                    EmailID = email.EmailID,
                                    ContactID = s.ContactID
                                };
                                emails.Add(newEmail);
                                d.Emails = emails;
                            }
                        }

                    }

                    if (s.SocialMediaUrls != null && s.SocialMediaUrls.Count > 0)
                    {
                        var facebookUrl = s.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Facebook");
                        if (facebookUrl != null)
                            d.FacebookUrl = new Url() { URL = facebookUrl.URL };
                        var twitterUrl = s.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Twitter");
                        if (twitterUrl != null)
                            d.TwitterUrl = new Url() { URL = twitterUrl.URL };
                        var googlePlusUrl = s.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Google+");
                        if (googlePlusUrl != null)
                            d.GooglePlusUrl = new Url() { URL = googlePlusUrl.URL };
                        var linkedInUrl = s.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "LinkedIn");
                        if (linkedInUrl != null)
                            d.LinkedInUrl = new Url() { URL = linkedInUrl.URL };
                        var blogUrl = s.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Blog");
                        if (blogUrl != null)
                            d.BlogUrl = new Url() { URL = blogUrl.URL };
                        var websiteUrl = s.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Website");
                        if (websiteUrl != null)
                            d.WebsiteUrl = new Url() { URL = websiteUrl.URL };
                    }

                    if (s.SelectedLeadSource != null && s.SelectedLeadSource.Any())
                    {
                        var dropdowns = new List<DropdownValue>();
                        foreach (DropdownValueViewModel item in s.SelectedLeadSource)
                        {
                            if (item != null)
                            {
                                DropdownValue dropdownValues;
                                bool isPrimary = item.DropdownValueID == s.SelectedLeadSource.First().DropdownValueID ? true : false;
                                dropdownValues = new DropdownValue() { Id = item.DropdownValueID, Value = item.DropdownValue, IsPrimary = isPrimary };
                                dropdowns.Add(dropdownValues);
                            }
                        }
                        d.LeadSources = dropdowns;
                    }
                });

            this.CreateMap<PersonViewModel, ContactTableType>()
            .ForMember(p => p.ContactID, opt => opt.MapFrom(c => c.ContactID))
                .ForMember(p => p.Company, opt => opt.MapFrom(c => c.CompanyName))
                .ForMember(p => p.OwnerID, opt => opt.MapFrom(c => c.OwnerId))
                .ForMember(p => p.Addresses, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<AddressViewModel>, IEnumerable<Address>>(c.Addresses)))
                .ForMember(p => p.ContactImage, opt => opt.MapFrom(c => Mapper.Map<ImageViewModel, Image>(c.Image)))
                .ForMember(p => p.PartnerType, opt => opt.MapFrom(c => (PartnerType?)c.PartnerType))
                .ForMember(p => p.LifecycleStage, opt => opt.MapFrom(c => (LifecycleStage)c.LifecycleStage))
                .ForMember(p => p.ContactSource, opt => opt.MapFrom(c => (ContactSource?)c.ContactSource))
                .ForMember(p => p.FirstContactSource, opt => opt.MapFrom(c => (ContactSource?)c.FirstContactSource))
                .ForMember(p => p.ProfileImageKey, opt => opt.MapFrom(c => c.ProfileImageKey))
                .ForMember(p => p.ContactType, opt => opt.Ignore())
                .ForMember(p => p.LeadSources, opt => opt.Ignore())
                .ForMember(p => p.Phones, opt => opt.Ignore())
                .ForMember(p => p.Emails, opt => opt.Ignore())
                .ForMember(p => p.CustomFields, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<ContactCustomFieldMapViewModel>, IEnumerable<ContactCustomField>>(c.CustomFields)))
                .AfterMap((s, d) =>
                {
                    d.ContactType = (byte)ContactType.Person;
                    if (s.Phones != null && s.Phones.Any())
                    {
                        List<Phone> phones = new List<Phone>();
                        foreach (dynamic phone in s.Phones)
                        {
                            if (!String.IsNullOrEmpty(phone.Number))
                            {
                                Phone newPhone = new Phone()
                                {
                                    AccountID = s.AccountID,
                                    ContactID = s.ContactID,
                                    IsPrimary = phone.IsPrimary,
                                    Number = phone.Number,
                                    PhoneType = phone.PhoneType,
                                    PhoneTypeName = phone.PhoneTypeName,
                                    ContactPhoneNumberID = phone.ContactPhoneNumberID,
                                    CountryCode = phone.CountryCode,
                                    Extension = phone.Extension
                                };
                                phones.Add(newPhone);
                                d.Phones = phones;
                            }
                        }
                    }

                    if (s.Emails != null && s.Emails.Any())
                    {
                        List<Email> emails = new List<Email>();
                        foreach (dynamic email in s.Emails)
                        {
                            if (email.EmailId != null)
                            {
                                Email newEmail = new Email()
                                {
                                    AccountID = s.AccountID,
                                    IsPrimary = email.IsPrimary,
                                    EmailId = email.EmailId,
                                    EmailStatusValue = (EmailStatus)email.EmailStatusValue,
                                    EmailID = email.EmailID,
                                    ContactID = s.ContactID,
                                    ContactEmailID = email.EmailID
                                };
                                emails.Add(newEmail);
                                d.Emails = emails;
                            }
                        }

                    }

                    if (s.SelectedLeadSource != null && s.SelectedLeadSource.Any())
                    {
                        var dropdowns = new List<DropdownValue>();
                        foreach (DropdownValueViewModel item in s.SelectedLeadSource)
                        {
                            if (item != null)
                            {
                                DropdownValue dropdownValues;
                                bool isPrimary = item.DropdownValueID == s.SelectedLeadSource.First().DropdownValueID ? true : false;
                                dropdownValues = new DropdownValue() { Id = item.DropdownValueID, Value = item.DropdownValue, IsPrimary = isPrimary };
                                dropdowns.Add(dropdownValues);
                            }
                        }
                        d.LeadSources = dropdowns;
                    }
                });
            this.CreateMap<TagViewModel, Tag>()
                .ForMember(t => t.Id, opt => opt.MapFrom(t => t.TagID))
                .ForMember(t => t.TagName, opt => opt.MapFrom(t => t.TagName))
                .ForMember(t => t.Description, opt => opt.MapFrom(t => t.Description))
                .ForMember(t => t.Count, opt => opt.MapFrom(t => t.Count))
                .ForMember(t => t.TagNameAutoComplete, opt => opt.Ignore());



            this.CreateMap<ITagViewModel, Tag>();

            this.CreateMap<ActionViewModel, DA.Action>()
                .ForMember(p => p.Id, opt => opt.MapFrom(c => c.ActionId))
                .ForMember(p => p.Details, opt => opt.MapFrom(c => c.ActionMessage))
                .ForMember(p => p.ReminderTypes, opt => opt.MapFrom(c => c.SelectedReminderTypes))
                .ForMember(p => p.RemindOn, opt => opt.MapFrom(c => c.RemindOn))
                .ForMember(p => p.Contacts, opt => opt.Ignore())
                .ForMember(p => p.Tags, opt => opt.Ignore())
                .ForMember(p => p.IsCompleted, opt => opt.MapFrom(c => c.IsCompleted))
                .ForMember(p => p.ToEmail, opt => opt.MapFrom(c => c.ToEmail))
                .ForMember(p => p.EmailRequestGuid, opt => opt.MapFrom(c => c.SelectedReminderTypes.Contains(ReminderType.Email) ? c.EmailRequestGuid : null))
                .ForMember(p => p.TextRequestGuid, opt => opt.MapFrom(c => c.SelectedReminderTypes.Contains(ReminderType.TextMessage) ? c.TextRequestGuid : null))
                .AfterMap((s, d) =>
                {
                    if (s.Contacts != null)
                    {
                        var contacts = new List<RawContact>();
                        foreach (ContactEntry entry in s.Contacts)
                        {
                            RawContact contact = new RawContact();
                            contact.ContactID = entry.Id;
                            contacts.Add(contact);
                        }
                        d.Contacts = contacts;
                    }
                    if (s.TagsList != null)
                    {
                        var tags = new List<Tag>();
                        foreach (TagViewModel tag in s.TagsList)
                        {
                            tags.Add(Mapper.Map<TagViewModel, Tag>(tag));
                        }
                        d.Tags = tags;
                    }
                    // d.User = new User() { Id = s.CreatedBy.ToString() };
                });

            this.CreateMap<ContactEntry, Contact>()
                .IgnoreAllUnmapped()
                .ForMember(c => c.Id, opt => opt.MapFrom(c => c.Id));

            this.CreateMap<ContactEntry, RawContact>()
                 .IgnoreAllUnmapped()
                 .ForMember(c => c.ContactID, opt => opt.MapFrom(c => c.Id));

            this.CreateMap<CompanyViewModel, Company>()
               .ForMember(p => p.Id, opt => opt.MapFrom(c => c.ContactID))
               .ForMember(p => p.Addresses, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<AddressViewModel>, IEnumerable<Address>>(c.Addresses)))
               .ForMember(p => p.ContactImage, opt => opt.MapFrom(c => Mapper.Map<ImageViewModel, Image>(c.Image)))
               .ForMember(p => p.Tags, opt => opt.Ignore())
                .ForMember(p => p.FacebookUrl, opt => opt.Ignore())
                .ForMember(p => p.TwitterUrl, opt => opt.Ignore())
                .ForMember(p => p.GooglePlusUrl, opt => opt.Ignore())
                .ForMember(p => p.LinkedInUrl, opt => opt.Ignore())
                .ForMember(p => p.BlogUrl, opt => opt.Ignore())
                .ForMember(p => p.WebsiteUrl, opt => opt.Ignore())
               .ForMember(p => p.ImageUrl, opt => opt.MapFrom(c => c.ContactImageUrl))
                .ForMember(p => p.ProfileImageKey, opt => opt.MapFrom(c => c.ProfileImageKey))
              .ForMember(p => p.CompanyNameAutoComplete, opt => opt.Ignore())
              .ForMember(p => p.TitleAutoComplete, opt => opt.Ignore())
              .ForMember(p => p.Phones, opt => opt.Ignore())
              .ForMember(p => p.Emails, opt => opt.Ignore())
              .ForMember(p => p.OwnerId, opt => opt.MapFrom(c => c.OwnerId))
              .ForMember(p => p.ContactFullNameAutoComplete, opt => opt.Ignore())
                .AfterMap((s, d) =>
                {
                    if (s.Phones != null && s.Phones.Any())
                    {
                        List<Phone> phones = new List<Phone>();
                        foreach (dynamic phone in s.Phones)
                        {
                            if (!String.IsNullOrEmpty(phone.Number))
                            {
                                Phone newPhone = new Phone()
                                {
                                    AccountID = s.AccountID,
                                    IsPrimary = phone.IsPrimary,
                                    Number = phone.Number,
                                    PhoneType = phone.PhoneType,
                                    PhoneTypeName = phone.PhoneTypeName,
                                    ContactID = phone.ContactID,
                                    ContactPhoneNumberID = phone.ContactPhoneNumberID,
                                    CountryCode = phone.CountryCode,
                                    Extension = phone.Extension
                                };
                                phones.Add(newPhone);
                                d.Phones = phones;
                            }
                        }
                    }

                    if (s.Emails != null && s.Emails.Any())
                    {
                        List<Email> emails = new List<Email>();
                        foreach (dynamic email in s.Emails)
                        {
                            if (email.EmailId != null)
                            {
                                Email newEmail = new Email()
                                {
                                    AccountID = s.AccountID,
                                    IsPrimary = email.IsPrimary,
                                    EmailId = email.EmailId,
                                    EmailStatusValue = (EmailStatus)email.EmailStatusValue,
                                    EmailID = email.EmailID
                                };
                                emails.Add(newEmail);
                                d.Emails = emails;
                            }
                        }
                    }

                    if (s.SocialMediaUrls != null && s.SocialMediaUrls.Count > 0)
                    {
                        var facebookUrl = s.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Facebook");
                        if (facebookUrl != null)
                            d.FacebookUrl = new Url() { URL = facebookUrl.URL };
                        var twitterUrl = s.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Twitter");
                        if (twitterUrl != null)
                            d.TwitterUrl = new Url() { URL = twitterUrl.URL };
                        var googlePlusUrl = s.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Google+");
                        if (googlePlusUrl != null)
                            d.GooglePlusUrl = new Url() { URL = googlePlusUrl.URL };
                        var linkedInUrl = s.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "LinkedIn");
                        if (linkedInUrl != null)
                            d.LinkedInUrl = new Url() { URL = linkedInUrl.URL };
                        var blogUrl = s.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Blog");
                        if (blogUrl != null)
                            d.BlogUrl = new Url() { URL = blogUrl.URL };
                        var websiteUrl = s.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Website");
                        if (websiteUrl != null)
                            d.WebsiteUrl = new Url() { URL = websiteUrl.URL };
                    }
                });

            this.CreateMap<CompanyViewModel, ContactTableType>()
               .ForMember(p => p.ContactID, opt => opt.MapFrom(c => c.ContactID))
               .ForMember(p => p.Company, opt => opt.MapFrom(c => c.CompanyName))
               .ForMember(p => p.Addresses, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<AddressViewModel>, IEnumerable<Address>>(c.Addresses)))
               .ForMember(p => p.ContactImage, opt => opt.MapFrom(c => Mapper.Map<ImageViewModel, Image>(c.Image)))
               .ForMember(p => p.ProfileImageKey, opt => opt.MapFrom(c => c.ProfileImageKey))
               .ForMember(p => p.ContactType, opt => opt.Ignore())
               .ForMember(p => p.AccountID, opt => opt.MapFrom(c => c.AccountID))
              .ForMember(p => p.Phones, opt => opt.Ignore())
              .ForMember(p => p.Emails, opt => opt.Ignore())
              .ForMember(p => p.OwnerID, opt => opt.MapFrom(c => c.OwnerId))
              .AfterMap((s, d) =>
                {
                    d.ContactType = (byte)ContactType.Company;
                    d.ContactSource = (byte)ContactSource.Manual;
                    d.SourceType = null;
                    if (s.Phones != null && s.Phones.Any())
                    {
                        List<Phone> phones = new List<Phone>();
                        foreach (dynamic phone in s.Phones)
                        {
                            if (!String.IsNullOrEmpty(phone.Number))
                            {
                                Phone newPhone = new Phone()
                                {
                                    AccountID = s.AccountID,
                                    IsPrimary = phone.IsPrimary,
                                    Number = phone.Number,
                                    PhoneType = phone.PhoneType,
                                    PhoneTypeName = phone.PhoneTypeName,
                                    ContactID = phone.ContactID,
                                    ContactPhoneNumberID = phone.ContactPhoneNumberID,
                                    CountryCode = phone.CountryCode,
                                    Extension = phone.Extension
                                };
                                phones.Add(newPhone);
                                d.Phones = phones;
                            }
                        }
                    }

                    if (s.Emails != null && s.Emails.Any())
                    {
                        List<Email> emails = new List<Email>();
                        foreach (dynamic email in s.Emails)
                        {
                            if (email.EmailId != null)
                            {
                                Email newEmail = new Email()
                                {
                                    AccountID = s.AccountID,
                                    IsPrimary = email.IsPrimary,
                                    EmailId = email.EmailId,
                                    EmailStatusValue = (EmailStatus)email.EmailStatusValue,
                                    EmailID = email.EmailID,
                                    ContactEmailID = email.EmailID

                                };
                                emails.Add(newEmail);
                                d.Emails = emails;
                            }
                        }
                    }
                });

            this.CreateMap<TourViewModel, Tour>()
                .ForMember(t => t.Id, opt => opt.MapFrom(t => t.TourID))
                 .ForMember(p => p.Contacts, opt => opt.Ignore())
                 .ForMember(p => p.ReminderTypes, opt => opt.MapFrom(t => t.SelectedReminderTypes))
                .AfterMap((s, d) =>
                {
                    if (s.Contacts != null)
                    {
                        var contacts = new List<Contact>();
                        foreach (ContactEntry entry in s.Contacts)
                        {
                            Contact contact;
                            if (entry.ContactType == (int)ContactType.Person)
                                contact = new Person() { Id = entry.Id };
                            else
                                contact = new Company() { Id = entry.Id };
                            contacts.Add(contact);
                        }
                        d.Contacts = contacts;
                    }
                    d.User = new User() { Id = s.CreatedBy.ToString() };
                });

            this.CreateMap<ContactListEntry, Person>()
                //.IgnoreAllUnmapped()
                .ForMember(c => c.Id, opt => opt.MapFrom(c => c.ContactID))
                .ForMember(c => c.FirstName, opt => opt.MapFrom(c => c.FirstName))
                 .ForMember(c => c.CompanyName, opt => opt.MapFrom(c => c.CompanyName))
                .ForMember(c => c.LastContacted, opt => opt.MapFrom(c => c.LastTouched))
                .ForMember(c => c.Address, opt => opt.MapFrom(c => c.Address))
                .ForMember(c => c.Email, opt => opt.MapFrom(c => c.PrimaryEmail))
                .ForMember(c => c.Phones, opt => opt.MapFrom(c => c.Phones))
                .ForMember(c => c.LifecycleStage, opt => opt.MapFrom(c => c.LifecycleStage))
                .ForMember(c => c.CreatedOn, opt => opt.MapFrom(c => c.CreatedOn))
                .ForMember(c => c.DateFormat, opt => opt.MapFrom(c => c.DateFormat))
                .ForMember(c => c.LastTouchedThrough, opt => opt.MapFrom(c => c.LastTouchedThrough));

            this.CreateMap<ContactListEntry, ContactExportViewModel>()
               .IgnoreAllUnmapped()
                .ForMember(c => c.FirstName, opt => opt.Ignore())
                .ForMember(c => c.LastName, opt => opt.Ignore())
               .ForMember(c => c.Name, opt => opt.Ignore())
               .ForMember(c => c.LastContacted, opt => opt.MapFrom(c => c.LastContactedDate))
               .ForMember(c => c.LastUpdatedOn, opt => opt.MapFrom(c => c.LastUpdatedOn))
               .ForMember(c => c.LastTouchedThrough, opt => opt.MapFrom(c => c.LastTouchedThrough))
               .ForMember(c => c.LifecycleStage, opt => opt.MapFrom(c => c.LifecycleName))
               .AfterMap((s, d) =>
              {
                  if (s.ContactType == (int)ContactType.Person)
                  {
                      d.Company = s.CompanyName;
                      d.FirstName = s.FirstName;
                      d.LastName = s.LastName;
                  }
                  else
                  {
                      d.Company = s.Name;
                  }

                  d.PrimaryEmail = s.PrimaryEmail == "[|Email Not Available|]" ? "" : s.PrimaryEmail;
                  if (s.PrimaryAddress != null)
                      d.Address = s.PrimaryAddress;
                  string[] primaryPhone = s.Phone.Split('(', ')').ToArray();
                  d.HomePhone = primaryPhone[0];
                  d.PhoneType = primaryPhone[1] == "xxx" ? "" : primaryPhone[1];

              });


            this.CreateMap<UserSettingsViewModel, UserSettings>()
              .ForMember(c => c.Id, opt => opt.MapFrom(c => c.UserSettingId))
              .ForMember(c => c.AccountID, opt => opt.MapFrom(c => c.AccountId))
              .ForMember(c => c.CountryID, opt => opt.MapFrom(c => c.CountryId))
              .ForMember(c => c.CurrencyID, opt => opt.MapFrom(c => c.CurrencyID))
              .ForMember(c => c.UserID, opt => opt.MapFrom(c => c.UserId))
              .ForMember(c => c.ItemsPerPage, opt => opt.MapFrom(c => c.ItemsPerPage))
              .ForMember(c => c.EmailID, opt => opt.MapFrom(c => c.EmailId))
              .ForMember(c => c.IsIncludeSignature, opt => opt.MapFrom(c => c.IsIncludeSignature));


            this.CreateMap<ContactListEntry, Company>()
               .IgnoreAllUnmapped()
               .ForMember(c => c.Id, opt => opt.MapFrom(c => c.ContactID));

            this.CreateMap<CommunicationTrackerViewModel, CommunicationTracker>()
               .ForMember(p => p.Id, opt => opt.MapFrom(c => c.CommunicationTrackerID));

            this.CreateMap<AttachmentViewModel, Attachment>()
                .ForMember(p => p.Id, opt => opt.MapFrom(c => c.DocumentID))
               .ForMember(p => p.DocumentTypeID, opt => opt.MapFrom(c => (DocumentType)c.DocumentTypeID))
              .ForMember(a => a.FileTypeID, opt => opt.MapFrom(c => (FileType)c.FileTypeID));

            this.CreateMap<ServiceProviderViewModel, ServiceProvider>()
                .ForMember(p => p.Id, opt => opt.MapFrom(c => c.CommunicationLogID));

            this.CreateMap<ImageDomainViewModel, ImageDomain>()
                .ForMember(n => n.Id, opt => opt.MapFrom(n => n.ImageDomainId));

            this.CreateMap<SendMailViewModel, SendMailRequest>()
                .ForMember(p => p.BCC, opt => opt.MapFrom(c => string.IsNullOrEmpty(c.BCC) ? null : (c.BCC.IndexOf(';') != -1 ? c.BCC.Split(';') : c.BCC.Split(','))))
                .ForMember(p => p.CC, opt => opt.MapFrom(c => string.IsNullOrEmpty(c.CC) ? null : (c.CC.IndexOf(';') != -1 ? c.CC.Split(';') : c.CC.Split(','))))
                .ForMember(p => p.To, opt => opt.MapFrom(c => string.IsNullOrEmpty(c.To) ? null : (c.To.IndexOf(';') != -1 ? c.BCC.Split(';') : c.To.Split(','))));

            this.CreateMap<SendTextViewModel, SendTextRequest>()
                .ForMember(p => p.To, opt => opt.MapFrom(c => string.IsNullOrEmpty(c.To) ? null : c.To.Split(';')))
                 .ForMember(p => p.SenderId, opt => opt.MapFrom(c => c.UserId.ToString()))
                .ForMember(p => p.Message, opt => opt.MapFrom(c => c.Body));

            this.CreateMap<ProviderRegistrationViewModel, RegisterMailRequest>()
             .ForMember(p => p.UserName, opt => opt.MapFrom(c => c.UserName))
           .ForMember(p => p.Password, opt => opt.MapFrom(c => c.Password))
            .ForMember(p => p.APIKey, opt => opt.MapFrom(c => c.ApiKey))
              .ForMember(p => p.Name, opt => opt.MapFrom(c => c.SenderFriendlyName))
              .ForMember(p => p.MailProviderID, opt => opt.MapFrom(c => c.MailProviderID));

            this.CreateMap<ProviderRegistrationViewModel, RegisterTextRequest>()
           .ForMember(p => p.UserName, opt => opt.MapFrom(c => c.UserName))
         .ForMember(p => p.Password, opt => opt.MapFrom(c => c.Password))
          .ForMember(p => p.Key, opt => opt.MapFrom(c => c.ApiKey))
            .ForMember(p => p.Name, opt => opt.MapFrom(c => c.SenderFriendlyName))
            .ForMember(p => p.Token, opt => opt.MapFrom(c => c.LoginToken))
            .ForMember(p => p.TextProviderID, opt => opt.MapFrom(c => c.TextProviderID));

            this.CreateMap<NoteViewModel, Note>()
                .ForMember(p => p.Id, opt => opt.MapFrom(c => c.NoteId))
                .ForMember(p => p.Details, opt => opt.MapFrom(c => c.NoteDetails))
               .ForMember(p => p.Contacts, opt => opt.Ignore())
               .ForMember(p => p.Tags, opt => opt.Ignore())
               .AfterMap((s, d) =>
               {
                   if (s.Contacts != null)
                   {
                       var contacts = new List<Contact>();
                       foreach (ContactEntry entry in s.Contacts)
                       {
                           Contact contact;
                           if (entry.ContactType == (int)ContactType.Person)
                               contact = new Person() { Id = entry.Id };
                           else
                               contact = new Company() { Id = entry.Id };
                           contacts.Add(contact);
                       }
                       d.Contacts = contacts;
                   }
                   if (s.TagsList != null)
                   {
                       var tags = new List<Tag>();
                       foreach (TagViewModel tag in s.TagsList)
                       {
                           tags.Add(Mapper.Map<TagViewModel, Tag>(tag));
                       }
                       d.Tags = tags;
                   }
                   d.User = new User() { Id = s.CreatedBy.ToString() };
               });

            this.CreateMap<UserViewModel, User>()
                .ForMember(p => p.Id, opt => opt.MapFrom(c => c.UserID))
                .ForMember(p => p.Addresses, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<AddressViewModel>, IEnumerable<Address>>(c.Addresses)))
                .ForMember(p => p.Account, opt => opt.Ignore())
                 .ForMember(p => p.Status, opt => opt.MapFrom(c => (Status)c.Status))
                .ForMember(p => p.Email, opt => opt.MapFrom(c => new Email() { EmailId = c.PrimaryEmail, IsPrimary = true }))
                //.ForMember(p => p.Emails, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<UserViewModel>,IEnumerable<Email>>(c.Emails)))
                //.ForMember(c => c.IndexedOn, opt => opt.MapFrom(c => DateTime.Now))
                .ForMember(p => p.HomePhone, opt => opt.Ignore())
                .ForMember(p => p.WorkPhone, opt => opt.Ignore())
                .ForMember(p => p.MobilePhone, opt => opt.Ignore())
                .ForMember(p => p.FacebookUrl, opt => opt.Ignore())
                .ForMember(p => p.TwitterUrl, opt => opt.Ignore())
                .ForMember(p => p.GooglePlusUrl, opt => opt.Ignore())
                .ForMember(p => p.LinkedInUrl, opt => opt.Ignore())
                .ForMember(p => p.BlogUrl, opt => opt.Ignore())
                .ForMember(p => p.WebsiteUrl, opt => opt.Ignore())
                //.ForMember(a => a.Role, opt => opt.MapFrom(c => new Role() { ID = c.Roles.Code, Name = c.State.Name }))
                //.ForMember(a => a.Country, opt => opt.MapFrom(c =>new Country(){ Code = c.Country.Code, Name = c.Country.Name}));
                //.ForMember(p => p.CompanyNameAutoComplete, opt => opt.Ignore())
                //.ForMember(p => p.TitleAutoComplete, opt => opt.Ignore())
                //.ForMember(p => p.ContactFullNameAutoComplete, opt => opt.Ignore())
                .AfterMap((s, d) =>
                {
                    if (s.Phones != null && s.Phones.Count > 0)
                    {
                        var homePhone = s.Phones.FirstOrDefault(p => p.PhoneType == (short)DropdownValueTypes.Homephone);
                        if (homePhone != null)
                        {
                            d.HomePhone = new Phone() { Number = homePhone.Number, IsPrimary = homePhone.IsPrimary, PhoneTypeName = "Home" };
                            if (d.HomePhone.IsPrimary == true && d.HomePhone.Number != null)
                                d.PrimaryPhoneType = "H";
                        }
                        var workPhone = s.Phones.FirstOrDefault(p => p.PhoneType == (short)DropdownValueTypes.WorkPhone);
                        if (workPhone != null)
                        {
                            d.WorkPhone = new Phone() { Number = workPhone.Number, IsPrimary = workPhone.IsPrimary, PhoneTypeName = "Work" };
                            if (d.WorkPhone.IsPrimary == true && d.WorkPhone.Number != null)
                                d.PrimaryPhoneType = "W";
                        }
                        var mobilePhone = s.Phones.FirstOrDefault(p => p.PhoneType == (short)DropdownValueTypes.MobilePhone);
                        if (mobilePhone != null)
                        {
                            d.MobilePhone = new Phone() { Number = mobilePhone.Number, IsPrimary = mobilePhone.IsPrimary, PhoneTypeName = "Mobile" };
                            if (d.MobilePhone.IsPrimary == true && d.MobilePhone.Number != null)
                                d.PrimaryPhoneType = "M";
                        }


                    }
                    if (s.SocialMediaUrls != null && s.SocialMediaUrls.Count > 0)
                    {
                        var facebookUrl = s.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Facebook");
                        if (facebookUrl != null)
                            d.FacebookUrl = new Url() { URL = facebookUrl.Url };
                        var twitterUrl = s.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Twitter");
                        if (twitterUrl != null)
                            d.TwitterUrl = new Url() { URL = twitterUrl.Url };
                        var googlePlusUrl = s.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Google+");
                        if (googlePlusUrl != null)
                            d.GooglePlusUrl = new Url() { URL = googlePlusUrl.Url };
                        var linkedInUrl = s.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "LinkedIn");
                        if (linkedInUrl != null)
                            d.LinkedInUrl = new Url() { URL = linkedInUrl.Url };
                        var blogUrl = s.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Blog");
                        if (blogUrl != null)
                            d.BlogUrl = new Url() { URL = blogUrl.Url };
                        var websiteUrl = s.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Website");
                        if (websiteUrl != null)
                            d.WebsiteUrl = new Url() { URL = websiteUrl.Url };
                    }



                });

            this.CreateMap<AccountViewModel, Account>()
              .ForMember(p => p.Id, opt => opt.MapFrom(c => c.AccountID))
              .ForMember(p => p.Addresses, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<AddressViewModel>, IEnumerable<Address>>(c.Addresses)))
              .ForMember(c => c.WebAnalyticsProvider, opt => opt.MapFrom(c => Mapper.Map<WebAnalyticsProviderViewModel, WebAnalyticsProvider>(c.WebAnalyticsProvider)))
              .ForMember(p => p.Email, opt => opt.MapFrom(c => new Email() { EmailId = c.PrimaryEmail }))
               .ForMember(p => p.AccountLogo, opt => opt.MapFrom(c => Mapper.Map<ImageViewModel, Image>(c.Image)))
               .ForMember(p => p.HomePhone, opt => opt.Ignore())
               .ForMember(p => p.WorkPhone, opt => opt.Ignore())
               .ForMember(p => p.MobilePhone, opt => opt.Ignore())
               .ForMember(p => p.FacebookUrl, opt => opt.Ignore())
               .ForMember(p => p.TwitterUrl, opt => opt.Ignore())
               .ForMember(p => p.GooglePlusUrl, opt => opt.Ignore())
               .ForMember(p => p.LinkedInUrl, opt => opt.Ignore())
               .ForMember(p => p.BlogUrl, opt => opt.Ignore())
               .ForMember(p => p.WebsiteUrl, opt => opt.Ignore())
               .ForMember(p => p.SecondaryEmails, opt => opt.Ignore())
              .AfterMap((s, d) =>
              {
                  if (s.Phones != null && s.Phones.Count > 0)
                  {
                      var homePhone = s.Phones.FirstOrDefault(p => p.PhoneType == "Home");
                      if (homePhone != null)
                          d.HomePhone = new Phone() { Number = homePhone.PhoneNumber };
                      var workPhone = s.Phones.FirstOrDefault(p => p.PhoneType == "Work");
                      if (workPhone != null)
                          d.WorkPhone = new Phone() { Number = workPhone.PhoneNumber };
                      var mobilePhone = s.Phones.FirstOrDefault(p => p.PhoneType == "Mobile");
                      if (mobilePhone != null)
                          d.MobilePhone = new Phone() { Number = mobilePhone.PhoneNumber };
                  }
                  if (s.SocialMediaUrls != null && s.SocialMediaUrls.Count > 0)
                  {
                      var facebookUrl = s.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Facebook");
                      if (facebookUrl != null)
                          d.FacebookUrl = new Url() { URL = facebookUrl.Url };
                      var twitterUrl = s.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Twitter");
                      if (twitterUrl != null)
                          d.TwitterUrl = new Url() { URL = twitterUrl.Url };
                      var googlePlusUrl = s.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Google+");
                      if (googlePlusUrl != null)
                          d.GooglePlusUrl = new Url() { URL = googlePlusUrl.Url };
                      var linkedInUrl = s.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "LinkedIn");
                      if (linkedInUrl != null)
                          d.LinkedInUrl = new Url() { URL = linkedInUrl.Url };
                      var blogUrl = s.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Blog");
                      if (blogUrl != null)
                          d.BlogUrl = new Url() { URL = blogUrl.Url };
                      var websiteUrl = s.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Website");
                      if (websiteUrl != null)
                          d.WebsiteUrl = new Url() { URL = websiteUrl.Url };
                  }
                  if (s.SecondaryEmails != null && s.SecondaryEmails.Count > 0)
                  {
                      d.SecondaryEmails = s.SecondaryEmails.Select(e => new Email() { EmailId = e.SecondaryEmailId }).ToList();
                  }
              });

            this.CreateMap<WebAnalyticsProviderViewModel, WebAnalyticsProvider>()
                .ForMember(u => u.Id, opt => opt.MapFrom(u => u.WebAnalyticsProviderID));

            this.CreateMap<LeadScoreRuleViewModel, LeadScoreRule>()
                .ForMember(p => p.Id, opt => opt.MapFrom(c => c.LeadScoreRuleID))
                .ForMember(p => p.CreatedOn, opt => opt.MapFrom(c => c.CreatedOn))
                .ForMember(p => p.ModifiedOn, opt => opt.MapFrom(c => c.ModifiedOn))
                .ForMember(p => p.Tags, opt => opt.Ignore())
                .ForMember(p => p.LeadScoreConditionValues, opt => opt.Ignore())
                .AfterMap((s, d) =>
                {
                    d.User = new User() { Id = s.CreatedBy.ToString() };
                    if (s.TagsList != null)
                    {
                        d.Tags = Mapper.Map<IEnumerable<TagViewModel>, IEnumerable<Tag>>(s.TagsList);
                    }
                    if (s.LeadScoreConditionValues != null)
                    {
                        d.LeadScoreConditionValues = Mapper.Map<IEnumerable<LeadScoreConditionValueViewModel>, IEnumerable<LeadScoreConditionValue>>(s.LeadScoreConditionValues);
                    }
                });


            this.CreateMap<LeadAdapterViewModel, LeadAdapterAndAccountMap>()
               .ForMember(p => p.Id, opt => opt.MapFrom(c => c.LeadAdapterAndAccountMapId))
               .ForMember(p => p.LeadAdapterTypeID, opt => opt.MapFrom(c => c.LeadAdapterType))
               .AfterMap((s, d) =>
               {
                   d.Tags = Mapper.Map<IEnumerable<TagViewModel>, IEnumerable<Tag>>(s.TagsList);
                   if (d.Tags != null && d.Tags.Any())
                   {
                       foreach (Tag tag in d.Tags)
                       {
                           if (tag.Id == 0)
                               tag.CreatedBy = s.CreatedBy;
                       }
                   }
                   if (!string.IsNullOrEmpty(s.PageAccessToken))
                   {
                       d.BuilderNumber = " ";
                       d.FacebookLeadAdapter = new FacebookLeadAdapter() 
                       {
                           AddID = s.AddID, PageAccessToken = s.PageAccessToken, Id = s.FacebookLeadAdapterID, 
                           LeadAdapterAndAccountMapID =s.LeadAdapterAndAccountMapId, Name = s.FacebookLeadAdapterName,
                           PageID = s.PageID, UserAccessToken = s.UserAccessToken
                       };
                   }
               });

            this.CreateMap<FormViewModel, Form>()
               .ForMember(p => p.Id, opt => opt.MapFrom(c => c.FormId))
               .ForMember(p => p.Acknowledgement, opt => opt.MapFrom(c => c.Acknowledgement))
               .ForMember(p => p.FormFields, opt => opt.MapFrom(p => Mapper.Map<IList<FormFieldViewModel>, IList<FormField>>(p.FormFields)))
               .ForMember(p => p.CustomFields, opt => opt.Ignore())
               .ForMember(p => p.Tags, opt => opt.Ignore())
               .AfterMap((s, d) =>
               {
                   d.Tags = Mapper.Map<IEnumerable<TagViewModel>, IEnumerable<Tag>>(s.TagsList);
                   if (d.Tags != null && d.Tags.Count() != 0)
                   {
                       foreach (Tag tag in d.Tags)
                       {
                           if (tag.Id == 0)
                           {
                               tag.CreatedBy = s.CreatedBy;
                           }
                       }
                   }
               });

            this.CreateMap<FormFieldViewModel, FormField>()
                 .ForMember(f => f.FormFieldId, opt => opt.MapFrom(f => f.FormFieldId))
                 .ForMember(f => f.Id, opt => opt.MapFrom(f => f.FieldId))
                 .ForMember(f => f.FieldInputTypeId, opt => opt.MapFrom(f => f.FieldInputTypeId));

            this.CreateMap<SubmittedFormViewModel, Person>()
                .IgnoreAllUnmapped()
                //.ForMember(p => p.AccountID, opt => opt.MapFrom(s => s.AccountId))
                //.ForMember(p => p.CompanyName, opt => opt.MapFrom(s => s.SubmittedFormFields.Where(f => f.Key == Entities.ContactFields.CompanyNameField.ToString()).FirstOrDefault().Value))
                //.ForMember(p => p.FirstName, opt => opt.MapFrom(s => s.SubmittedFormFields.Where(f => f.Key == Entities.ContactFields.FirstNameField.ToString()).FirstOrDefault().Value))
                //.ForMember(p => p.LastName, opt => opt.MapFrom(s => s.SubmittedFormFields.Where(f => f.Key == Entities.ContactFields.LastNameField.ToString()).FirstOrDefault().Value))
                .AfterMap((s, d) =>
                {
                    //Person person = new Person();
                    d.FirstName = s.SubmittedFormFields.Where(f => f.Key == "1").Select(f => f.Value).FirstOrDefault();
                    d.LastName = s.SubmittedFormFields.Where(f => f.Key == "2").Select(f => f.Value).FirstOrDefault();

                    // d = person;
                });

            this.CreateMap<ModuleViewModel, Module>()
                .ForMember(p => p.Id, opt => opt.MapFrom(c => c.ModuleId))
                .ForMember(p => p.ModuleName, opt => opt.MapFrom(c => c.ModuleName))
                .ForMember(p => p.ParentID, opt => opt.MapFrom(c => c.ParentId));


            this.CreateMap<CustomFieldTabViewModel, CustomFieldTab>()
                .ForMember(c => c.Id, opt => opt.MapFrom(c => c.CustomFieldTabId))
                .ForMember(c => c.Sections, opt => opt.Ignore())
                .AfterMap((s, d) =>
                {
                    d.Sections = Mapper.Map<IEnumerable<CustomFieldSectionViewModel>, IEnumerable<CustomFieldSection>>(s.Sections);
                });

            this.CreateMap<CustomFieldSectionViewModel, CustomFieldSection>()
                .ForMember(s => s.Id, opt => opt.MapFrom(s => s.CustomFieldSectionId))
                .ForMember(s => s.CustomFields, opt => opt.Ignore())
                .AfterMap((s, d) =>
                {
                    d.CustomFields = Mapper.Map<IEnumerable<CustomFieldViewModel>, IEnumerable<CustomField>>(s.CustomFields);
                });

            this.CreateMap<CustomFieldViewModel, CustomField>()
                .ForMember(c => c.Id, opt => opt.MapFrom(c => c.FieldId))
                .ForMember(c => c.ValueOptions, opt => opt.Ignore())
                .AfterMap((s, d) =>
                {
                    d.ValueOptions = Mapper.Map<IEnumerable<CustomFieldValueOptionViewModel>, IEnumerable<FieldValueOption>>(s.ValueOptions);
                });

            this.CreateMap<FieldViewModel, Field>()
                .ForMember(c => c.Id, opt => opt.MapFrom(c => c.FieldId))
                .ForMember(c => c.ValueOptions, opt => opt.Ignore())
                .ForMember(c => c.DisplayName, opt => opt.MapFrom(c => c.Title))
                .AfterMap((s, d) =>
                {
                    d.ValueOptions = Mapper.Map<IEnumerable<CustomFieldValueOptionViewModel>, IEnumerable<FieldValueOption>>(s.ValueOptions);
                });

            this.CreateMap<CustomFieldValueOptionViewModel, FieldValueOption>()
                .ForMember(c => c.Id, opt => opt.MapFrom(c => c.CustomFieldValueOptionId))
                .ForMember(c => c.FieldId, opt => opt.MapFrom(c => c.CustomFieldId));

            this.CreateMap<ContactCustomFieldMapViewModel, ContactCustomField>()
                .ForMember(c => c.ContactCustomFieldMapId, opt => opt.MapFrom(c => c.ContactCustomFieldMapId));


            this.CreateMap<DropdownViewModel, Dropdown>()
             .ForMember(d => d.Id, opt => opt.MapFrom(d => d.DropdownID))
             .ForMember(d => d.Name, opt => opt.MapFrom(d => d.Dropdownname))
              .ForMember(d => d.AccountID, opt => opt.MapFrom(d => d.AccountID))
             .ForMember(d => d.DropdownValues, opt => opt.Ignore())
             .AfterMap((s, d) =>
                 {
                     d.DropdownValues = Mapper.Map<IEnumerable<DropdownValueViewModel>, IEnumerable<DropdownValue>>(s.DropdownValuesList);
                     d.Id = s.DropdownID;
                 });

            this.CreateMap<DropdownValueViewModel, DropdownValue>()
               .ForMember(a => a.Id, opt => opt.MapFrom(a => a.DropdownValueID))
               .ForMember(a => a.AccountID, opt => opt.MapFrom(a => a.AccountID))
               .ForMember(a => a.Value, opt => opt.MapFrom(a => a.DropdownValue));
            this.CreateMap<RelationshipEntry, ContactRelationship>()
              .ForMember(a => a.Id, opt => opt.MapFrom(a => a.ContactRelationshipMapID))
              .ForMember(a => a.ContactId, opt => opt.MapFrom(a => a.ContactId))
              .ForMember(a => a.RelatedContactID, opt => opt.MapFrom(a => a.RelatedContactID))
              .ForMember(a => a.RelationshipTypeID, opt => opt.MapFrom(a => a.RelationshipType))
              .ForMember(a => a.RelatedUserID, opt => opt.MapFrom(a => a.RelatedUserID))
              .IgnoreAllUnmapped()
             .AfterMap((s, d) =>
            {
                d.ContactId = s.ContactId;
                d.Id = s.ContactRelationshipMapID;
                d.RelatedContactID = s.RelatedContactID;
                d.RelatedUserID = s.RelatedUserID;
                d.RelationshipTypeID = s.RelationshipType;

            });
            //this.CreateMap<RelationshipViewModel, ContactRelationShip>()
            //.ForMember(d => d.Id, opt => opt.Ignore())
            //.AfterMap((s, d) =>
            //{
            //     = Mapper.Map<IEnumerable<RelationshipEntry>, IEnumerable<ContactRelationShip>>(s.Relationshipentry);

            //});
            #region Advanced Search
            this.CreateMap<AdvancedSearchViewModel, SearchDefinition>()
             .ForMember(a => a.Id, opt => opt.MapFrom(a => a.SearchDefinitionID))
             .ForMember(a => a.Name, opt => opt.MapFrom(a => a.SearchDefinitionName))
             .ForMember(a => a.IsPreConfiguredSearch, opt => opt.MapFrom(a => a.IsPreConfiguredSearch))
              .ForMember(a => a.IsFavoriteSearch, opt => opt.MapFrom(a => a.IsFavoriteSearch))
             .ForMember(a => a.PredicateType, opt => opt.MapFrom(a => (SearchPredicateType)a.SearchPredicateTypeID))
             .ForMember(a => a.TagsList, opt => opt.Ignore())
             .ForMember(a => a.Filters, opt => opt.Ignore())
             .ForMember(a => a.SelectedColumns, opt => opt.Ignore())
             .AfterMap((s, d) =>
             {
                 d.TagsList = Mapper.Map<IEnumerable<TagViewModel>, IEnumerable<Tag>>(s.TagsList);
                 d.Filters = Mapper.Map<IEnumerable<FilterViewModel>, IEnumerable<SearchFilter>>(s.SearchFilters);

                 if (s.SelectedColumns != null)
                 {
                     var fields = new List<Field>();
                     foreach (FieldViewModel entry in s.SelectedColumns)
                     {
                         fields.Add(Mapper.Map<FieldViewModel, Field>(entry));
                     }
                     d.SelectedColumns = fields;
                 }

                 if (s.SearchFields != null)
                 {
                     var fields = new List<Field>();
                     foreach (FieldViewModel entry in s.SearchFields)
                     {
                         fields.Add(Mapper.Map<FieldViewModel, Field>(entry));
                     }
                     d.Fields = fields;
                 }


                 if (!string.IsNullOrEmpty(s.CustomPredicateScript))
                     d.CustomPredicateScript = s.CustomPredicateScript.ToUpper();
             });

            this.CreateMap<FilterViewModel, SearchFilter>()
            .ForMember(a => a.SearchFilterId, opt => opt.MapFrom(a => a.SearchFilterID))
            .ForMember(a => a.Field, opt => opt.MapFrom(a => (ContactFields)a.FieldId))
            .ForMember(a => a.Qualifier, opt => opt.MapFrom(a => (SearchQualifier)a.SearchQualifierTypeID))
            .ForMember(a => a.FieldOptionTypeId, opt => opt.MapFrom(a => (short)a.InputTypeId))
            .AfterMap((s, d) =>
            {
                short dropdownvalue = 0;
                short.TryParse(s.FieldId.ToString(), out dropdownvalue);
                if (s.IsDropdownField)
                {
                    d.DropdownValueId = dropdownvalue;
                }
            });

            this.CreateMap<FieldViewModel, Field>()
                .ForMember(p => p.Id, opt => opt.MapFrom(c => c.FieldId));


            #endregion

            #region Opportunityviewmodel to Opportunity
            this.CreateMap<OpportunityViewModel, Opportunity>()
                  .ForMember(p => p.Id, opt => opt.MapFrom(c => c.OpportunityID))
                  .ForMember(p => p.Contacts, opt => opt.Ignore())
                  .ForMember(p => p.PeopleInvolved, opt => opt.Ignore())
                 .ForMember(p => p.ExpectedClosingDate, opt => opt.MapFrom(a => a.ExpectedCloseDate))
                  .AfterMap((s, d) =>
                  {
                      if (s.Contacts != null)
                      {
                          d.Contacts = s.Contacts != null ? s.Contacts.Select(c => c.Id).ToList() : null;
                      }

                      if (s.PeopleInvolved != null)
                      {
                          List<PeopleInvolved> PeopleInvoled = new List<PeopleInvolved>();
                          foreach (PeopleInvolvedViewModel people in s.PeopleInvolved)
                          {
                              PeopleInvolved peopledata = new PeopleInvolved
                              {
                                  PeopleInvolvedID = people.OpportunityRelationMapID,
                                  RelationshipTypeID = people.RelationshipTypeID,
                                  ContactID = people.ContactID
                              };
                              PeopleInvoled.Add(peopledata);
                          }
                          d.PeopleInvolved = PeopleInvoled;
                      }
                  });

            #endregion

            #region Opportunityviewmodel to OpportunityTableType
            this.CreateMap<OpportunityViewModel, OpportunityTableType>()
                .ForMember(p => p.Owner, opt => opt.MapFrom(c => c.OwnerId))
                .ForMember(p => p.ExpectedClosingDate, opt => opt.MapFrom(c => c.ExpectedCloseDate))
                .ForMember(p => p.OpportunityImage, opt => opt.MapFrom(c => c.Image));

            #endregion

            #region Workflowviewmodel to Workflow
            this.CreateMap<WorkFlowViewModel, Workflow>()
            .ForMember(p => p.Id, opt => opt.MapFrom(c => c.WorkflowID))
            .ForMember(p => p.DeactivatedOn, opt => opt.MapFrom(c => c.DeactivatedOn))
            .AfterMap((s, d) =>
            {
                if (s.RemoveFromWorkflows != null && s.RemoveFromWorkflows.Any() && s.RemoveFromWorkflows.Count() > 0)
                    d.RemovefromWorkflows = string.Join(",", s.RemoveFromWorkflows);
                else
                    d.RemovefromWorkflows = null;
            });

            this.CreateMap<BaseWorkflowActionViewModel, BaseWorkflowAction>()
                .Include<WorkflowLeadScoreActionViewModel, WorkflowLeadScoreAction>()
                .Include<WorkflowCampaignActionViewModel, WorkflowCampaignAction>()
                .Include<WorkflowTagActionViewModel, WorkflowTagAction>()
                .Include<WorkflowLifeCycleActionViewModel, WorkflowLifeCycleAction>()
                .Include<WorkflowUserAssignmentActionViewModel, WorkflowUserAssignmentAction>()
                .Include<WorkflowNotifyUserActionViewModel, WorkflowNotifyUserAction>()
                .Include<WorkflowContactFieldActionViewModel, WorkflowContactFieldAction>()
                .Include<WorkflowTimerActionViewModel, WorkflowTimerAction>()
                .Include<WorkflowEmailNotifyActionViewModel, WorkflowEmailNotificationAction>()
                .Include<WorkflowTextNotificationActionViewModel, WorkflowTextNotificationAction>()
                .Include<TriggerWorkflowActionViewModel, TriggerWorkflowAction>();

            this.CreateMap<WorkflowActionViewModel, WorkflowAction>()
                .AfterMap((s, d) =>
                {
                    d.Action = Mapper.Map<BaseWorkflowAction>(s.Action);
                });
            this.CreateMap<WorkflowLeadScoreActionViewModel, WorkflowLeadScoreAction>();
            this.CreateMap<WorkflowCampaignActionViewModel, WorkflowCampaignAction>().
                AfterMap((s, d) =>
                {
                    d.WorkflowActionTypeID = s.WorkflowActionTypeID;
                    //d.LinkAction = Mapper.Map<BaseWorkflowAction>(s.Action);
                    d.Links = Mapper.Map<IEnumerable<WorkflowCampaignActionLinkViewModel>, IEnumerable<WorkflowCampaignActionLink>>(s.CampaignLinks);
                });
            this.CreateMap<WorkflowCampaignActionLinkViewModel, WorkflowCampaignActionLink>()
                  .ForMember(a => a.LinkID, opt => opt.MapFrom(c => c.CampaignLinkId))
                  .ForMember(a => a.Actions, opt => opt.MapFrom(c => c.Actions));

            // this.CreateMap<WorkflowCampaignActionLinkViewModel, WorkflowCampaignActionLink>();
            this.CreateMap<WorkflowEmailNotifyActionViewModel, WorkflowEmailNotificationAction>();
            this.CreateMap<WorkflowTriggerViewModel, WorkflowTrigger>();
            this.CreateMap<TriggerCategoryTypeViewModel, WorkflowTriggerType>();
            this.CreateMap<WorkflowLeadScoreActionViewModel, WorkflowLeadScoreAction>();
            //this.CreateMap<WorkflowCampaignActionViewModel, WorkflowCampaignAction>();
            this.CreateMap<WorkflowTagActionViewModel, WorkflowTagAction>();
            this.CreateMap<WorkflowLifeCycleActionViewModel, WorkflowLifeCycleAction>();
            this.CreateMap<RoundRobinContactAssignmentViewModel, RoundRobinContactAssignment>()
                .ForMember(f => f.IsRoundRobinAssignment, opt => opt.MapFrom(m => m.IsRoundRobinAssignment == "0" ? false : true))
                .ForMember(f => f.UserID, opt => opt.MapFrom(s => !string.IsNullOrEmpty(s.UserID) ? s.UserID : string.Join(",", s.UserIds)));
            this.CreateMap<WorkflowUserAssignmentActionViewModel, WorkflowUserAssignmentAction>()
                .ForMember(s => s.UserNames, opt => opt.Ignore())
                //.ForMember(s => s.RoundRobinAssignment, opt => opt.MapFrom(c => c.RoundRobinAssignment == "0" ? false : true))
                .ForMember(s => s.RoundRobinContactAssignments, opt => opt.MapFrom(m => Mapper.Map<IEnumerable<RoundRobinContactAssignmentViewModel>, IEnumerable<RoundRobinContactAssignment>>(m.RoundRobinContactAssignments)));
                 
            this.CreateMap<WorkflowNotifyUserActionViewModel, WorkflowNotifyUserAction>()
                  .ForMember(s => s.UserID, opt => opt.Ignore())
                  .ForMember(s => s.UserNames, opt => opt.Ignore())
                  .AfterMap((s, d) =>
                  {
                      d.UserID = s.UserIds;
                  })
                  .ForMember(s => s.NotificationFields, opt => opt.Ignore())
                  .AfterMap((s, d) => { d.NotificationFieldID = s.NotificationFieldIds; });
            this.CreateMap<TriggerWorkflowActionViewModel, TriggerWorkflowAction>();
            this.CreateMap<WorkflowContactFieldActionViewModel, WorkflowContactFieldAction>();
            this.CreateMap<WorkflowTextNotificationActionViewModel, WorkflowTextNotificationAction>();
            this.CreateMap<WorkflowTimerActionViewModel, WorkflowTimerAction>()
                  .ForMember(a => a.RunAt, opt => opt.Ignore())
                  .AfterMap((s, d) =>
                  {
                      if (s.TimerType == TimerType.TimeDelay && s.RunAt.HasValue)
                      {
                          DateTime time = s.RunAt.Value;
                          d.RunAt = time.TimeOfDay;
                      }
                      else if (s.TimerType == TimerType.Date && s.RunAtTime.HasValue)
                      {
                          DateTime time = s.RunAtTime.Value;
                          d.RunAt = time.TimeOfDay;
                      }
                  });
            #endregion


            #region Reports
            this.CreateMap<ReportViewModel, SearchDefinition>()
            .ForMember(a => a.Id, opt => opt.MapFrom(a => a.SearchDefinitionID))
            .ForMember(a => a.PredicateType, opt => opt.MapFrom(a => (SearchPredicateType)a.SearchPredicateTypeID))
            .ForMember(a => a.TagsList, opt => opt.Ignore())
            .ForMember(a => a.CustomPredicateScript, opt => opt.MapFrom(a => a.CustomPredicateScript))
            .ForMember(a => a.SelectedColumns, opt => opt.Ignore())
            .AfterMap((s, d) =>
            {
                if (!string.IsNullOrEmpty(s.CustomPredicateScript))
                    d.CustomPredicateScript = s.CustomPredicateScript.ToUpper();
            });
            //  this.CreateMap<StandardReportViewModel, SearchDefinition>()
            //.ForMember(a => a.Id, opt => opt.MapFrom(a => a.SearchDefinitionID))
            //.ForMember(a => a.PredicateType, opt => opt.MapFrom(a => (SearchPredicateType)a.SearchPredicateTypeID))
            //.ForMember(a => a.TagsList, opt => opt.Ignore())
            //.ForMember(a => a.CustomPredicateScript, opt => opt.MapFrom(a => a.CustomPredicateScript))
            //.ForMember(a => a.SelectedColumns, opt => opt.Ignore())
            //.AfterMap((s, d) =>
            //{
            //    if (!string.IsNullOrEmpty(s.CustomPredicateScript))
            //        d.CustomPredicateScript = s.CustomPredicateScript.ToUpper();
            //});
            #endregion

            this.CreateMap<DropdownValueViewModel, FieldValueOption>()
                .ForMember(a => a.Id, opt => opt.MapFrom(a => a.DropdownValueID))
                .ForMember(a => a.FieldId, opt => opt.MapFrom(a => a.DropdownID))
                .ForMember(a => a.Value, opt => opt.MapFrom(a => a.DropdownValue));
            this.CreateMap<UserSocialMediaPostsViewModel, UserSocialMediaPosts>();

            this.CreateMap<CRMOutlookSyncViewModel, CRMOutlookSync>()
                .ForMember(o => o.OutlookSyncId, opt => opt.MapFrom(o => o.OutlookSyncId));

            this.CreateMap<SeedEmailViewModel, SeedEmail>()
               .ForMember(o => o.Id, opt => opt.MapFrom(o => o.SeedID));
            this.CreateMap<ThirdPartyClientViewModel, ThirdPartyClient>()
                .ForMember(o => o.ID, opt => opt.MapFrom(o => o.ID));
            this.CreateMap<AccountSettingsViewModel, AccountSettings>()
                .ForMember(o => o.AccountSettingsID, opt => opt.MapFrom(o => o.AccountSettingsID));
            this.CreateMap<MarketingMessagesViewModel, MarketingMessage>();
            this.CreateMap<MarketingMessageAccountMapViewModel, MarketingMessageAccountMap>();
            this.CreateMap<MarketingMessageContentMapViewModel, MarketingMessageContentMap>();
            #region SuppressionList
            this.CreateMap<SuppressedEmailViewModel, SuppressedEmail>()
                .ForMember(o => o.Id,opt => opt.MapFrom(o => o.SuppressedEmailID));
            this.CreateMap<SuppressedDomainViewModel, SuppressedDomain>()
                .ForMember(o => o.Id, opt => opt.MapFrom(o => o.SuppressedDomainID));
            #endregion

            this.CreateMap<ImportDataViewModel, ImportColumnMappings>();

            #region ApiLeadSubmissions
            this.CreateMap<APILeadSubmissionViewModel, APILeadSubmission>();
            this.CreateMap<Contact, ReportContact>()
                .ForMember(f => f.contactID, opt => opt.MapFrom(m => m.Id))
                .AfterMap((s, d) => 
                {
                    if (s.Emails != null && s.Emails.Any())
                    {
                        d.ContactEmailID = s.Emails.Where(w => w.IsPrimary).Select(se => se.ContactEmailID).FirstOrDefault();
                        d.email = s.Emails.Where(w => w.IsPrimary).Select(se => se.EmailId).FirstOrDefault();
                    }
                });
            #endregion
        }
    }
}
