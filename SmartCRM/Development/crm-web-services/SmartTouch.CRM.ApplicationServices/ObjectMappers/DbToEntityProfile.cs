using AutoMapper;
using SmartTouch.CRM.Domain;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.AccountSettings;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.ContactAudit;
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
using SmartTouch.CRM.Domain.Roles;
using SmartTouch.CRM.Domain.Search;
using SmartTouch.CRM.Domain.SeedList;
using SmartTouch.CRM.Domain.Subscriptions;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.Tours;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.WebAnalytics;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Domain.ApplicationTour;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DA = SmartTouch.CRM.Domain.Actions;
using SmartTouch.CRM.Domain.SuppressedEmails;
using SmartTouch.CRM.Domain.Communications;

namespace SmartTouch.CRM.ApplicationServices.ObjectMappers
{
    public class DbToEntityProfile : Profile
    {
        public new string ProfileName
        {
            get
            {
                return "DbToEntityProfile";
            }
        }

        protected override void Configure()
        {
            this.CreateMap<CampaignsDb, Campaign>()
                .ForMember(c => c.Id, opt => opt.MapFrom(c => c.CampaignID))
                .ForMember(c => c.CampaignTypeId, opt => opt.MapFrom(c => c.CampaignTypeID))
                .ForMember(c => c.Contacts, opt => opt.Ignore())
                .AfterMap((s, d) =>
                {
                    d.CampaignStatus = (Entities.CampaignStatus)s.CampaignStatusID;
                    d.Template = new CampaignTemplate() { Id = s.CampaignTemplateID };
                    d.Tags = Mapper.Map<ICollection<TagsDb>, IList<Tag>>(s.Tags);
                    d.SearchDefinitions = Mapper.Map<IEnumerable<SearchDefinitionsDb>, IEnumerable<SearchDefinition>>(s.SearchDefinitions).ToList();
                    d.ContactTags = Mapper.Map<IEnumerable<TagsDb>, IEnumerable<Tag>>(s.Tags).ToList();
                    d.Links = Mapper.Map<IEnumerable<CampaignLinksDb>, IEnumerable<CampaignLink>>(s.Links);
                });

            this.CreateMap<CampaignThemesDb, CampaignTheme>()
                .ForMember(t => t.CampaignThemeID, opt => opt.MapFrom(c => c.CampaignThemeID));

            this.CreateMap<ImportContactData, RawContact>();
            this.CreateMap<ImportColumnMappingsDb, ImportColumnMappings>()
                .ForMember(f => f.Id, opt => opt.MapFrom(c => c.ImportColumnMappingID));

            this.CreateMap<CampaignLinksDb, CampaignLink>()
                .ForMember(c => c.CampaignId, opt => opt.MapFrom(c => c.CampaignID))
                .ForMember(c => c.CampaignLinkId, opt => opt.MapFrom(c => c.CampaignLinkID))
                .ForMember(c => c.URL, opt => opt.Ignore())
                .AfterMap((s, d) =>
                {
                    d.URL = new Url();
                    d.URL.URL = s.URL;
                });

            this.CreateMap<CampaignRecipientsDb, CampaignRecipient>()
                .ForMember(c => c.Id, opt => opt.MapFrom(c => c.CampaignRecipientID))
                .ForMember(c => c.Contact, opt => opt.Ignore())
                .AfterMap((s, d) =>
                {
                    if (s.Contact != null)
                    {
                        if (s.Contact.ContactType == ContactType.Company)
                            d.Contact = Mapper.Map<ContactsDb, Company>(s.Contact);
                        else
                            d.Contact = Mapper.Map<ContactsDb, Person>(s.Contact);
                    }
                });


            this.CreateMap<VCampaignRecipientsDb, CampaignRecipient>();

            #region CampaignTemplateDb to CampaignTemplate
            this.CreateMap<CampaignTemplatesDb, CampaignTemplate>()
                .ForMember(ct => ct.Id, opt => opt.MapFrom(ct => ct.CampaignTemplateID))
                .ForMember(ct => ct.Name, opt => opt.MapFrom(ct => ct.Name))
                .ForMember(ct => ct.ThumbnailImage, opt => opt.MapFrom(ct => Mapper.Map<ImagesDb, Image>(ct.Image)));

            #endregion

            this.CreateMap<DropdownDb, Dropdown>()
                .ForMember(a => a.Id, opt => opt.MapFrom(d => d.DropdownID))
                .ForMember(a => a.Name, opt => opt.MapFrom(d => d.DropdownName))
                .ForMember(a => a.DropdownValues, opt => opt.Ignore())
                .AfterMap((s, d) =>
                {
                    d.DropdownValues = Mapper.Map<IEnumerable<DropdownValueDb>, IEnumerable<DropdownValue>>(s.DropdownValues);

                });
            this.CreateMap<ReportsDb, Report>()
              .ForMember(a => a.Id, opt => opt.MapFrom(d => d.ReportID))
              .ForMember(a => a.ReportType, opt => opt.MapFrom(d => (Entities.Reports)d.ReportType));


            this.CreateMap<SmartTouch.CRM.Repository.Database.SubmittedFormDataDb, SmartTouch.CRM.Domain.Forms.SubmittedFormData>();
            this.CreateMap<SmartTouch.CRM.Repository.Database.SubmittedFormFieldDataDb, SmartTouch.CRM.Domain.Forms.SubmittedFormFieldData>();


            this.CreateMap<DropdownValueDb, DropdownValue>()
                .ForMember(a => a.Id, opt => opt.MapFrom(d => d.DropdownValueID))
                .ForMember(a => a.Value, opt => opt.MapFrom(a => a.DropdownValue))
                .ForMember(a => a.OpportunityGroupID, opt => opt.Ignore())
                .AfterMap((s, d) =>
                {
                    if (s.OpportunityStageGroups != null)
                    {
                        foreach (var group in s.OpportunityStageGroups)
                        {
                            d.OpportunityGroupID = group.OpportunityGroupID;
                        }
                    }
                });
            //.ForMember(a => a.OpportunityGroupID, opt => opt.MapFrom(a =>  a.OpportunityStageGroups.ToArray()[0].OpportunityGroupID));

            this.CreateMap<ContactImagesDb, ImageDNU>()
                .ForMember(a => a.Id, opt => opt.MapFrom(c => c.ContactImageID));

            this.CreateMap<ImagesDb, Image>()
                .ForMember(a => a.Id, opt => opt.MapFrom(c => c.ImageID))
                .ForMember(a => a.ImageCategoryID, opt => opt.MapFrom(c => c.ImageCategoryID));

            this.CreateMap<AccountEmailsDb, Email>()
              .ForMember(a => a.EmailId, opt => opt.MapFrom(a => a.Email));

            this.CreateMap<TaxRateDb, TaxRate>()
             .ForMember(a => a.TaxRateId, opt => opt.MapFrom(a => a.TaxRateId));


            this.CreateMap<AddressesDb, Address>()
                .ForMember(a => a.AddressTypeID, opt => opt.MapFrom(a => a.AddressTypeID))
                .ForMember(a => a.State, opt => opt.Ignore())
                .ForMember(a => a.Country, opt => opt.Ignore())
                .ForMember(a => a.IsDefault, opt => opt.MapFrom(a => a.IsDefault.HasValue ? a.IsDefault.Value : false))
                .AfterMap((s, d) =>
                {
                    if (s.State == null)
                        d.State = new State() { Code = s.StateID == null ? "" : s.StateID };
                    else
                        d.State = Mapper.Map<StatesDb, State>(s.State);

                    if (s.Country == null)
                        d.Country = new Country() { Code = s.CountryID == null ? "" : s.CountryID };
                    else
                        d.Country = Mapper.Map<CountriesDb, Country>(s.Country);

                });

            this.CreateMap<ContactPhoneNumbersDb, Phone>()
                .ForMember(a => a.ContactID, opt => opt.MapFrom(a => a.ContactID))
                .ForMember(a => a.PhoneType, opt => opt.MapFrom(a => a.PhoneType))
                .ForMember(a => a.Number, opt => opt.MapFrom(a => a.PhoneNumber))
                .ForMember(a => a.PhoneTypeName, opt => opt.MapFrom(a => a.DropdownValues != null ? a.DropdownValues.DropdownValue : ""))
                .ForMember(a => a.DropdownValueTypeID, opt => opt.MapFrom(a => a.DropdownValues != null ? a.DropdownValues.DropdownValueTypeID : 0));

            this.CreateMap<ContactEmailsDb, Email>()
                .ForMember(a => a.EmailID, opt => opt.MapFrom(a => a.ContactEmailID))
                .ForMember(a => a.EmailStatusValue, opt => opt.MapFrom(a => (EmailStatus)a.EmailStatus))
                .ForMember(a => a.EmailId, opt => opt.MapFrom(a => a.Email));

            this.CreateMap<StatesDb, State>()
                .ForMember(a => a.Name, opt => opt.MapFrom(a => a.StateName))
                .ForMember(a => a.Code, opt => opt.MapFrom(a => a.StateID));
            this.CreateMap<ContactEmailAuditDb, ContactEmailAudit>()
                    .ForMember(c => c.Id, opt => opt.MapFrom(c => c.ContactEmailAuditID));
            this.CreateMap<ContactTextMessageAuditDb, ContactTextMessageAudit>()
                  .ForMember(c => c.Id, opt => opt.MapFrom(c => c.ContactTextMessageAuditID));
            this.CreateMap<CountriesDb, Country>()
                .ForMember(a => a.Code, opt => opt.MapFrom(a => a.CountryID))
                .ForMember(a => a.Name, opt => opt.MapFrom(a => a.CountryName));

            this.CreateMap<ContactRelationshipDb, ContactRelationship>()
             .ForMember(a => a.Id, opt => opt.MapFrom(a => a.ContactRelationshipMapID))
             .ForMember(a => a.RelationshipTypeID, opt => opt.MapFrom(a => a.RelationshipType))
             .ForMember(a => a.RelationshipName, opt => opt.MapFrom(a => a.DropdownValues != null ? a.DropdownValues.DropdownValue : ""))
             .ForMember(a => a.RelatedContactType, opt => opt.MapFrom(a => a.RelatedContact != null ? a.RelatedContact.ContactType : 0))
            .AfterMap((s, d) =>
            {
                if (s.Contact != null)
                {
                    d.ContactName = s.Contact.FirstName + " " + s.Contact.LastName;
                    if (s.Contact.ContactType == ContactType.Company)
                        d.ContactName = s.Contact.Company;
                }
                if (s.RelatedContact != null)
                {
                    d.RelatedContact.FirstName = s.RelatedContact.FirstName + " " + s.RelatedContact.LastName;
                    if (!string.IsNullOrEmpty(s.RelatedContact.Company))
                    {
                        if (d.RelatedContact.FirstName != "")
                            d.RelatedContact.FirstName = s.RelatedContact.FirstName + " " + s.RelatedContact.LastName + ":" + s.RelatedContact.Company;
                        else
                            d.RelatedContact.FirstName = s.RelatedContact.Company;
                    }
                }
                if (s.User != null)
                    d.RelatedUser.FirstName = s.User.FirstName + "" + s.User.LastName;
                if (s.DropdownValues != null)
                    d.RelationshipName = s.DropdownValues.DropdownValue;
            });

            this.CreateMap<ContactsDb, RawContact>()
                .ForMember(c => c.ContactID, opt => opt.MapFrom(c => c.ContactID))
                .ForMember(c => c.Emails, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<ContactEmailsDb>, ICollection<Email>>(c.ContactEmails)))
                .ForMember(c => c.PrimaryEmail, opt => opt.MapFrom(c => c.PrimaryEmail))
                .ForMember(c => c.CompanyName, opt => opt.MapFrom(c => c.Company))
                .ForMember(c => c.IsDeleted, opt => opt.MapFrom(c => c.IsDeleted))
                .ForMember(c => c.Title, opt => opt.MapFrom(c => c.Title)).
                ForMember(c => c.ContactType, opt => opt.MapFrom(c => c.ContactType));

            this.CreateMap<ContactsDb, Person>()
                .ForMember(c => c.Id, opt => opt.MapFrom(c => c.ContactID))
                .ForMember(c => c.Addresses, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<AddressesDb>, IEnumerable<Address>>(c.Addresses)))
                .ForMember(c => c.Phones, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<ContactPhoneNumbersDb>, IEnumerable<Phone>>(c.ContactPhones)))
                .ForMember(c => c.Emails, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<ContactEmailsDb>, ICollection<Email>>(c.ContactEmails)))
                .ForMember(c => c.WebVisits, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<WebVisitsDb>, IEnumerable<WebVisit>>(c.WebVisits)))
                .ForMember(c => c.Email, opt => opt.MapFrom(c => c.PrimaryEmail))
                .ForMember(c => c.CompanyName, opt => opt.MapFrom(c => c.Company))
                .ForMember(c => c.IsDeleted, opt => opt.MapFrom(c => c.IsDeleted))
                .ForMember(c => c.Title, opt => opt.MapFrom(c => c.Title))
                .ForMember(c => c.ImageUrl, opt => opt.MapFrom(c => c.ContactImageUrl))
                .ForMember(c => c.ProfileImageKey, opt => opt.MapFrom(c => c.ProfileImageKey))
                .ForMember(c => c.ContactRelationships, opt => opt.Ignore())
                .ForMember(c => c.Communities, opt => opt.Ignore())
                .ForMember(c => c.LifecycleStage, opt => opt.MapFrom(c => c.LifecycleStage))
                .ForMember(c => c.LastContactedThrough, opt => opt.MapFrom(c => c.LastContactedThrough))
                .ForMember(c => c.PartnerType, opt =>
                 opt.MapFrom(c => c.PartnerType))
                //.ForMember(c => c.PartnerType, opt =>
                //    opt.MapFrom(c => c.PartnerType.HasValue ? c.PartnerType.Value : PartnerType.Realtor))
                .ForMember(c => c.FacebookUrl, opt =>
                    opt.MapFrom(c => c.Communication != null && c.Communication.FacebookUrl != null ? new Url() { URL = c.Communication.FacebookUrl } : null))
                .ForMember(c => c.TwitterUrl, opt =>
                    opt.MapFrom(c => c.Communication != null && c.Communication.TwitterUrl != null ? new Url() { URL = c.Communication.TwitterUrl } : null))
                .ForMember(c => c.LinkedInUrl, opt =>
                    opt.MapFrom(c => c.Communication != null && c.Communication.LinkedInUrl != null ? new Url() { URL = c.Communication.LinkedInUrl } : null))
                .ForMember(c => c.GooglePlusUrl, opt =>
                    opt.MapFrom(c => c.Communication != null && c.Communication.GooglePlusUrl != null ? new Url() { URL = c.Communication.GooglePlusUrl } : null))
                .ForMember(c => c.BlogUrl, opt =>
                    opt.MapFrom(c => c.Communication != null && c.Communication.BlogUrl != null ? new Url() { URL = c.Communication.BlogUrl } : null))
                .ForMember(c => c.WebsiteUrl, opt =>
                    opt.MapFrom(c => c.Communication != null && c.Communication.WebSiteUrl != null ? new Url() { URL = c.Communication.WebSiteUrl } : null))

                .ForMember(c => c.ContactImage, opt => opt.MapFrom(c => Mapper.Map<ImagesDb, Image>(c.Image)))
                .ForMember(c => c.Owner, opt => opt.Ignore())        //Ignored since while indexing contact entire owner object is being saved.
                .ForMember(c => c.Tags, opt => opt.Ignore())
                .ForMember(c => c.FormSubmissions, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<FormSubmissionDb>, IEnumerable<FormSubmission>>(c.FormSubmissions)))
                .ForMember(c => c.LeadSources, opt => opt.Ignore())
                .ForMember(c => c.Actions, opt => opt.Ignore())
                .ForMember(c => c.TourCommunity, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<ContactTourCommunityMap>, IEnumerable<ContactTourCommunityMap>>(c.TourCommunity)))
                .ForMember(c => c.ContactActions, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<ContactActionMap>, IEnumerable<ContactActionMap>>(c.ContactActions)))
                .ForMember(c => c.ContactNotes, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<ContactNoteMap>, IEnumerable<ContactNoteMap>>(c.ContactNotes)))
                .ForMember(c => c.AllLeadSources, opt => opt.Ignore())
                .ForMember(c => c.CompanyNameAutoComplete, opt => opt.MapFrom(c =>
                    new AutoCompleteSuggest()
                    {
                        Input = new string[] { c.Company },
                        Output = c.Company,
                        Payload = new SuggesterPayload() { DocumentId = c.ContactID, AccountId = c.AccountID, ContactType = (byte)c.ContactType }
                    }))
                    .ForMember(c => c.TitleAutoComplete, opt => opt.MapFrom(c =>
                    new AutoCompleteSuggest()
                    {
                        Input = new string[] { c.Title },
                        Output = c.Title,
                        Payload = new SuggesterPayload() { DocumentId = c.ContactID, AccountId = c.AccountID, ContactType = (byte)c.ContactType }
                    }))
                .AfterMap((s, d) =>
                {
                    if (s.CreaterInfo != null)
                    {
                        d.CreatedBy = s.CreaterInfo.CreatedBy;
                        d.CreatedOn = s.CreaterInfo.CreatedOn.HasValue ? s.CreaterInfo.CreatedOn.Value : default(DateTime);
                    }
                    AutoCompleteSuggest suggest = new AutoCompleteSuggest();
                    string fullName = (s.FirstName + " " + s.LastName).Trim();

                    IList<string> inputs = new List<string>();
                    if (!string.IsNullOrEmpty(s.FirstName))
                        inputs.Add(s.FirstName);
                    if (!string.IsNullOrEmpty(s.LastName))
                        inputs.Add(s.LastName);
                    if (!string.IsNullOrEmpty(fullName))
                        inputs.Add(fullName);

                    if (string.IsNullOrEmpty(fullName) && d.Emails.Any(e => e.IsPrimary))
                    {
                        var primaryEmail = d.Emails.FirstOrDefault(e => e.IsPrimary).EmailId;
                        if (!string.IsNullOrEmpty(s.Company))
                            fullName = primaryEmail + ": " + s.Company;
                        else
                            fullName = primaryEmail;
                        inputs.Add(primaryEmail);
                    }
                    else if (!string.IsNullOrEmpty(fullName) && d.Emails.Any(e => e.IsPrimary))
                    {
                        var primaryEmail = d.Emails.FirstOrDefault(e => e.IsPrimary).EmailId;
                        fullName = fullName + " ( " + primaryEmail + " ) ";
                    }
                    else if (!string.IsNullOrEmpty(fullName) && !string.IsNullOrEmpty(s.Company))
                        fullName = fullName + ": " + s.Company;

                    suggest.Input = inputs.ToArray();
                    suggest.Output = fullName;
                    suggest.Payload = new SuggesterPayload()
                    {
                        DocumentId = s.ContactID,
                        AccountId = s.AccountID,
                        DocumentOwnedBy = s.OwnerID.HasValue ? s.OwnerID.Value : 0,
                        ContactType = (byte)ContactType.Person
                    };
                    d.ContactFullNameAutoComplete = suggest;

                    var emailSuggests = new List<AutoCompleteSuggest>();

                    if (d.DoNotEmail == false)
                    {
                        foreach (var email in d.Emails.Where(e => e.EmailStatusValue == EmailStatus.NotVerified
                            || e.EmailStatusValue == EmailStatus.SoftBounce || e.EmailStatusValue == EmailStatus.Verified))
                        {
                            if (string.IsNullOrEmpty(email.EmailId))
                                continue;

                            IList<string> emailInputs = inputs;
                            emailInputs.Add(email.EmailId);

                            AutoCompleteSuggest emailSuggest = new AutoCompleteSuggest();
                            string name = s.FirstName + " " + s.LastName;
                            string fullEmail = "";

                            if (!string.IsNullOrEmpty(name))
                                fullEmail = name + " <" + email.EmailId + ">" + (email.IsPrimary ? " *" : string.Empty);
                            else
                                fullEmail = email.EmailId + (email.IsPrimary ? " *" : string.Empty);

                            emailSuggest.Input = emailInputs.ToArray();
                            emailSuggest.Output = fullEmail;
                            emailSuggest.Payload = new SuggesterPayload()
                            {
                                DocumentId = email.EmailID,
                                AccountId = s.AccountID,
                                DocumentOwnedBy = s.OwnerID.HasValue ? s.OwnerID.Value : 0,
                                ContactType = (byte)s.ContactType
                            };
                            emailSuggests.Add(emailSuggest);
                        }
                    }
                    d.EmailAutoComplete = emailSuggests;

                    var phoneSuggests = new List<AutoCompleteSuggest>();
                    foreach (var phone in d.Phones)
                    {
                        if (string.IsNullOrEmpty(phone.Number))
                            continue;

                        IList<string> phoneInputs = inputs;

                        AutoCompleteSuggest phoneSuggest = new AutoCompleteSuggest();
                        string name = s.FirstName + " " + s.LastName;
                        string fullPhone = "";
                        string phoneNumber = phone.Number.Replace("(", "").Replace(")", "").Replace("-", "");

                        phoneNumber = Regex.Replace(phoneNumber, @"\s+", "");
                        if (phoneNumber.Length > 10)
                            phoneNumber = phoneNumber.TrimStart('1');
                        phoneInputs.Add(phoneNumber);
                        phoneNumber = Regex.Replace(phoneNumber, @"(\d{3})(\d{3})(\d{4})", "($1) $2-$3");

                        if (!string.IsNullOrEmpty(name))
                            fullPhone = name + " (" + phone.PhoneTypeName + ": " + phoneNumber + " )" + (phone.IsPrimary ? " *" : string.Empty);
                        else if (d.Emails.Any(e => e.IsPrimary))
                            fullPhone = d.Emails.FirstOrDefault(e => e.IsPrimary).EmailId + " (" + phone.PhoneTypeName + ": " + phoneNumber + ")" + (phone.IsPrimary ? " *" : string.Empty);
                        else
                            fullPhone = phoneNumber;

                        phoneSuggest.Input = phoneInputs.ToArray();
                        phoneSuggest.Output = fullPhone;
                        phoneSuggest.Payload = new SuggesterPayload()
                        {
                            DocumentId = phone.ContactPhoneNumberID,
                            AccountId = s.AccountID,
                            DocumentOwnedBy = s.OwnerID.HasValue ? s.OwnerID.Value : 0,
                            ContactType = (byte)s.ContactType
                        };
                        phoneSuggests.Add(phoneSuggest);
                    }
                    d.PhoneNumberAutoComplete = phoneSuggests;

                    if (s.DropDownValues != null && s.DropDownValues.Any())
                    {
                        var leadSources = new List<DropdownValue>();
                        var dropdownFields = s.DropDownValues.Where(i => i.AccountID == s.AccountID);
                        foreach (DropdownValueDb leadSource in dropdownFields)
                        {
                            DropdownValue dropdownvalue = new DropdownValue() { Id = leadSource.DropdownValueID, Value = leadSource.DropdownValue };
                            leadSources.Add(dropdownvalue);
                        }
                        d.AllLeadSources = leadSources;
                    }

                    if (s.ContactLeadSources != null)
                    {
                        var leadSources = new List<DropdownValue>();
                        foreach (ContactLeadSourceMapDb leadSource in s.ContactLeadSources)
                        {
                            if (leadSource.LeadSource != null)
                            {
                                DropdownValue dropdownvalue =
                                    new DropdownValue() { Id = leadSource.LeadSouceID, Value = leadSource.LeadSource.DropdownValue, AccountID = s.AccountID, ContactId = leadSource.ContactID, LastUpdatedDate = leadSource.LastUpdatedDate,
                                    IsPrimary = leadSource.IsPrimaryLeadSource };
                                leadSources.Add(dropdownvalue);
                            }
                            else
                            {
                                DropdownValue dropdownvalue = new DropdownValue() { Id = leadSource.LeadSouceID, AccountID = s.AccountID, ContactId = leadSource.ContactID, LastUpdatedDate = leadSource.LastUpdatedDate,
                                IsPrimary = leadSource.IsPrimaryLeadSource };
                                leadSources.Add(dropdownvalue);
                            }
                        }
                        d.LeadSources = leadSources;
                    }

                    if (s.Communities != null && s.Communities.Any())
                    {
                        var communities = new List<DropdownValue>();
                        foreach (ContactCommunityMapDb community in s.Communities)
                        {
                            if (community.Community != null)
                            {
                                DropdownValue dropdownvalue =
                                    new DropdownValue() { Id = community.CommunityID, Value = community.Community.DropdownValue, AccountID = s.AccountID, ContactId = community.ContactID };
                                communities.Add(dropdownvalue);
                            }
                            else
                            {
                                DropdownValue dropdownvalue = new DropdownValue() { Id = community.CommunityID, AccountID = s.AccountID, ContactId = community.ContactID };
                                communities.Add(dropdownvalue);
                            }
                        }
                        d.Communities = communities;
                    }


                    if (s.LastContacted.HasValue)
                    {
                        DateTime date = s.LastContacted.Value;
                        d.LastContacted = new DateTime(date.Ticks - (date.Ticks % TimeSpan.TicksPerSecond));
                    }

                    if (s.LastUpdatedOn.HasValue)
                    {
                        DateTime date = s.LastUpdatedOn.Value;
                        d.LastUpdatedOn = new DateTime(date.Ticks - (date.Ticks % TimeSpan.TicksPerSecond));
                    }
                    if (s.Tags != null && s.Tags.Any())
                    {
                        var tags = s.Tags.Select(se => new Tag() { Id = se.TagID });
                        d.Tags = tags;
                    }
                });

            this.CreateMap<ContactsDb, Company>()
                .ForMember(c => c.Id, opt => opt.MapFrom(c => c.ContactID))
                .ForMember(c => c.CompanyName, opt => opt.MapFrom(c => c.Company))
                .ForMember(c => c.Addresses, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<AddressesDb>, IEnumerable<Address>>(c.Addresses)))
                .ForMember(c => c.Phones, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<ContactPhoneNumbersDb>, IEnumerable<Phone>>(c.ContactPhones)))
                .ForMember(c => c.Emails, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<ContactEmailsDb>, ICollection<Email>>(c.ContactEmails)))
                .ForMember(c => c.ImageUrl, opt => opt.MapFrom(c => c.ContactImageUrl))
                .ForMember(c => c.IsDeleted, opt => opt.MapFrom(c => c.IsDeleted))
                .ForMember(c => c.Email, opt => opt.MapFrom(c => c.PrimaryEmail))
                .ForMember(c => c.ProfileImageKey, opt => opt.MapFrom(c => c.ProfileImageKey))
                .ForMember(c => c.FacebookUrl, opt => opt.MapFrom(c => c.Communication != null && c.Communication.FacebookUrl != null ? new Url() { URL = c.Communication.FacebookUrl } : null))
                .ForMember(c => c.TwitterUrl, opt => opt.MapFrom(c => c.Communication != null && c.Communication.TwitterUrl != null ? new Url() { URL = c.Communication.TwitterUrl } : null))
                .ForMember(c => c.LinkedInUrl, opt => opt.MapFrom(c => c.Communication != null && c.Communication.LinkedInUrl != null ? new Url() { URL = c.Communication.LinkedInUrl } : null))
                .ForMember(c => c.GooglePlusUrl, opt => opt.MapFrom(c => c.Communication != null && c.Communication.GooglePlusUrl != null ? new Url() { URL = c.Communication.GooglePlusUrl } : null))
                .ForMember(c => c.BlogUrl, opt => opt.MapFrom(c => c.Communication != null && c.Communication.BlogUrl != null ? new Url() { URL = c.Communication.BlogUrl } : null))
                .ForMember(c => c.WebsiteUrl, opt => opt.MapFrom(c => c.Communication != null && c.Communication.WebSiteUrl != null ? new Url() { URL = c.Communication.WebSiteUrl } : null))
                //.ForMember(c => c.IndexedOn, opt => opt.MapFrom(c => DateTime.Now))
                .ForMember(c => c.CompanyNameAutoComplete, opt => opt.MapFrom(c =>
                    new AutoCompleteSuggest() { Input = new string[] { c.Company.Trim() }, Output = c.Company, Payload = new SuggesterPayload() { DocumentId = c.ContactID, AccountId = c.AccountID, ContactType = (byte)c.ContactType } }))
                .ForMember(c => c.ContactImage, opt => opt.MapFrom(c => Mapper.Map<ImagesDb, Image>(c.Image)))
                .ForMember(c => c.Tags, opt => opt.Ignore())
                .ForMember(c => c.Actions, opt => opt.Ignore())
                .ForMember(c => c.TourCommunity, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<ContactTourCommunityMap>, IEnumerable<ContactTourCommunityMap>>(c.TourCommunity)))
                .ForMember(c => c.ContactActions, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<ContactActionMap>, IEnumerable<ContactActionMap>>(c.ContactActions)))
                .ForMember(c => c.ContactNotes, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<ContactNoteMap>, IEnumerable<ContactNoteMap>>(c.ContactNotes)))
                .ForMember(c => c.Communities, opt => opt.Ignore())
                .ForMember(c => c.TitleAutoComplete, opt => opt.Ignore())
                .AfterMap((s, d) =>
                {
                    if (s.CreaterInfo != null)
                    {
                        d.CreatedBy = s.CreaterInfo.CreatedBy;
                        d.CreatedOn = s.CreaterInfo.CreatedOn.HasValue ? s.CreaterInfo.CreatedOn.Value : default(DateTime);
                    }
                    AutoCompleteSuggest suggest = new AutoCompleteSuggest();
                    string fullName = s.Company;
                    IList<string> inputs = new List<string>();
                    if (!string.IsNullOrEmpty(s.Company))
                    {
                        inputs.Add(fullName);
                    }
                    else if (string.IsNullOrEmpty(s.Company) && d.Emails.Any(e => e.IsPrimary))
                    {
                        var primaryEmail = d.Emails.FirstOrDefault(e => e.IsPrimary).EmailId;
                        fullName = primaryEmail;
                        inputs.Add(primaryEmail);
                    }
                    suggest.Input = inputs.ToArray();
                    suggest.Output = fullName;

                    suggest.Payload = new SuggesterPayload()
                    {
                        DocumentId = s.ContactID,
                        AccountId = s.AccountID,
                        DocumentOwnedBy = s.OwnerID.HasValue ? s.OwnerID.Value : 0,
                        ContactType = (byte)ContactType.Company
                    };
                    d.ContactFullNameAutoComplete = suggest;

                    var emailSuggests = new List<AutoCompleteSuggest>();
                    if (d.DoNotEmail == false)
                    {
                        foreach (var email in d.Emails.Where(e => e.EmailStatusValue == EmailStatus.NotVerified
                            || e.EmailStatusValue == EmailStatus.SoftBounce || e.EmailStatusValue == EmailStatus.Verified))
                        {
                            if (string.IsNullOrEmpty(email.EmailId))
                                continue;

                            IList<string> emailInputs = inputs;
                            if (string.IsNullOrEmpty(s.Company))
                                emailInputs.Add(s.Company);

                            emailInputs.Add(email.EmailId);

                            AutoCompleteSuggest emailSuggest = new AutoCompleteSuggest();
                            string name = s.Company;
                            string fullEmail = "";

                            if (!string.IsNullOrEmpty(name))
                                fullEmail = name + " <" + email.EmailId + ">" + (email.IsPrimary ? " *" : string.Empty);
                            else
                                fullEmail = email.EmailId + (email.IsPrimary ? " *" : string.Empty);

                            emailSuggest.Input = emailInputs.ToArray();
                            emailSuggest.Output = fullEmail;
                            emailSuggest.Payload = new SuggesterPayload()
                            {
                                DocumentId = email.EmailID,
                                AccountId = s.AccountID,
                                DocumentOwnedBy = s.OwnerID.HasValue ? s.OwnerID.Value : 0,
                                ContactType = (byte)s.ContactType
                            };
                            emailSuggests.Add(emailSuggest);
                        }
                    }
                    d.EmailAutoComplete = emailSuggests;

                    var phoneSuggests = new List<AutoCompleteSuggest>();
                    foreach (var phone in d.Phones)
                    {
                        if (string.IsNullOrEmpty(phone.Number))
                            continue;

                        IList<string> phoneInputs = inputs;
                        if (string.IsNullOrEmpty(s.Company))
                            phoneInputs.Add(s.Company);

                        AutoCompleteSuggest phoneSuggest = new AutoCompleteSuggest();
                        string name = s.Company;
                        string fullPhone = "";
                        string phoneNumber = phone.Number.Replace("(", "").Replace(")", "").Replace("-", "");
                        phoneNumber = Regex.Replace(phoneNumber, @"\s+", "");
                        if (phoneNumber.Length > 10)
                            phoneNumber = phoneNumber.TrimStart('1');
                        phoneInputs.Add(phoneNumber);
                        phoneNumber = Regex.Replace(phoneNumber, @"(\d{3})(\d{3})(\d{4})", "($1) $2-$3");

                        if (!string.IsNullOrEmpty(name))
                            fullPhone = name + " (" + phone.PhoneTypeName + ": " + phoneNumber + ")" + (phone.IsPrimary ? " *" : string.Empty);
                        else if (d.Emails.Any(e => e.IsPrimary))
                            fullPhone = d.Emails.FirstOrDefault(e => e.IsPrimary).EmailId + " (" + phone.PhoneTypeName + ": " + phoneNumber + ")"
                                + (phone.IsPrimary ? " *" : string.Empty);
                        else
                            fullPhone = phoneNumber;

                        phoneSuggest.Input = phoneInputs.ToArray();
                        phoneSuggest.Output = fullPhone;
                        phoneSuggest.Payload = new SuggesterPayload()
                        {
                            DocumentId = phone.ContactPhoneNumberID,
                            AccountId = s.AccountID,
                            DocumentOwnedBy = s.OwnerID.HasValue ? s.OwnerID.Value : 0,
                            ContactType = (byte)s.ContactType
                        };
                        phoneSuggests.Add(phoneSuggest);
                    }
                    d.PhoneNumberAutoComplete = phoneSuggests;

                    if (s.LastContacted.HasValue)
                    {
                        DateTime date = s.LastContacted.Value;
                        d.LastContacted = new DateTime(date.Ticks - (date.Ticks % TimeSpan.TicksPerSecond));
                    }

                    if (s.LastUpdatedOn.HasValue)
                    {
                        DateTime date = s.LastUpdatedOn.Value;
                        d.LastUpdatedOn = new DateTime(date.Ticks - (date.Ticks % TimeSpan.TicksPerSecond));
                    }
                    if (s.Tags != null && s.Tags.Any())
                    {
                        var tags = s.Tags.Select(se => new Tag() { Id = se.TagID });
                        d.Tags = tags;
                    }
                });

            this.CreateMap<NotesDb, Note>()
                 .ForMember(n => n.Id, opt => opt.MapFrom(n => n.NoteID))
                 .ForMember(n => n.Details, opt => opt.MapFrom(n => n.NoteDetails))
                 .ForMember(n => n.Contacts, opt => opt.Ignore())
                 .ForMember(n => n.Tags, opt => opt.Ignore())
                 .AfterMap((s, d) =>
                 {
                     if (s.Tags != null)
                     {
                         IList<Tag> tags = new List<Tag>();
                         foreach (TagsDb tagDb in s.Tags)
                         {
                             tags.Add(Mapper.Map<TagsDb, Tag>(tagDb));
                         }
                         d.Tags = tags;
                     }

                     if (s.Contacts != null)
                     {
                         IList<Contact> contacts = new List<Contact>();
                         foreach (ContactsDb contactDb in s.Contacts)
                         {
                             if (contactDb.ContactType == ContactType.Person)
                                 contacts.Add(Mapper.Map<ContactsDb, Person>(contactDb));
                             else
                                 contacts.Add(Mapper.Map<ContactsDb, Company>(contactDb));
                         }
                         d.Contacts = contacts;
                     }
                     d.User = Mapper.Map<UsersDb, User>(s.User);
                 });
            this.CreateMap<TagsDb, Tag>()
                          .ForMember(n => n.Id, opt => opt.MapFrom(n => n.TagID))
                          .ForMember(n => n.Description, opt => opt.MapFrom(n => n.Description))
                          .ForMember(n => n.TagName, opt => opt.MapFrom(n => n.TagName))
                          .ForMember(n => n.LeadScoreTag, opt => opt.MapFrom(n => n.LeadScoreTag))
                          .ForMember(n => n.TagNameAutoComplete, opt => opt.Ignore())
                          .AfterMap((s, d) =>
                          {
                              AutoCompleteSuggest suggest = new AutoCompleteSuggest();
                              suggest.Input = new string[] { s.TagName };
                              suggest.Output = s.LeadScoreTag ? s.TagName + " *" : s.TagName;
                              suggest.Payload = new SuggesterPayload() { DocumentId = s.TagID, AccountId = s.AccountID };
                              d.TagNameAutoComplete = suggest;
                          });

            this.CreateMap<VTagsDb, Tag>()
                          .ForMember(n => n.Id, opt => opt.MapFrom(n => n.TagID))
                          .ForMember(n => n.Description, opt => opt.MapFrom(n => n.Description))
                          .ForMember(n => n.TagName, opt => opt.MapFrom(n => n.TagName))
                          .ForMember(n => n.LeadScoreTag, opt => opt.MapFrom(n => n.LeadScoreTag))
                          .ForMember(n => n.TagNameAutoComplete, opt => opt.Ignore())
                          .ForMember(n => n.TotalTagCount, opt => opt.MapFrom(n => n.TotalCount))
                          .AfterMap((s, d) =>
                          {
                              AutoCompleteSuggest suggest = new AutoCompleteSuggest();
                              suggest.Input = new string[] { s.TagName };
                              suggest.Output = s.LeadScoreTag ? s.TagName + " *" : s.TagName;
                              suggest.Payload = new SuggesterPayload() { DocumentId = s.TagID, AccountId = s.AccountID };
                              d.TagNameAutoComplete = suggest;
                          });

            this.CreateMap<CommunicationTrackerDb, CommunicationTracker>()
               .ForMember(n => n.Id, opt => opt.MapFrom(n => n.CommunicationTrackerID));

            this.CreateMap<AttachmentsDb, Attachment>()
             .ForMember(n => n.Id, opt => opt.MapFrom(n => n.DocumentID))
             .ForMember(n => n.UserName, opt => opt.MapFrom(n => n.Users == null ? "" : n.Users.FirstName + " " + n.Users.LastName))
             .ForMember(n => n.AttachmentTime, opt => opt.Ignore());

            this.CreateMap<ServiceProvidersDb, ServiceProvider>()
                 .ForMember(n => n.Id, opt => opt.MapFrom(n => n.ServiceProviderID))
                 .ForMember(n => n.MailType, opt => opt.MapFrom(n => n.EmailType))
                 .ForMember(n => n.Account, opt => opt.MapFrom(n => Mapper.Map<AccountsDb, Account>(n.Accounts)))
                 .ForMember(n => n.ImageDomain, opt => opt.MapFrom(n => Mapper.Map<ImageDomainsDb, ImageDomain>(n.ImageDomain)));

            this.CreateMap<ImageDomainsDb, ImageDomain>()
                .ForMember(n => n.Id, opt => opt.MapFrom(n => n.ImageDomainID))
                .ForMember(n => n.Domain, opt => opt.MapFrom(n => n.ImageDomain));

            this.CreateMap<TourDb, Tour>()
                .ForMember(t => t.Id, opt => opt.MapFrom(t => t.TourID))
                .ForMember(t => t.Contacts, opt => opt.Ignore())
                .AfterMap((s, d) =>
                {
                    IList<ReminderType> reminderTypes = new List<ReminderType>();
                    if (s.RemindbyEmail.HasValue && s.RemindbyEmail.Value)
                        reminderTypes.Add(ReminderType.Email);
                    if (s.RemindbyText.HasValue && s.RemindbyText.Value)
                        reminderTypes.Add(ReminderType.TextMessage);
                    if (s.RemindbyPopup.HasValue && s.RemindbyPopup.Value)
                        reminderTypes.Add(ReminderType.PopUp);
                    d.ReminderTypes = reminderTypes;

                    if (s.Contacts != null)
                    {
                        IList<Contact> contacts = new List<Contact>();
                        foreach (ContactsDb contactDb in s.Contacts)
                        {
                            if (contactDb.ContactType == ContactType.Person)
                                contacts.Add(Mapper.Map<ContactsDb, Person>(contactDb));
                            else
                                contacts.Add(Mapper.Map<ContactsDb, Company>(contactDb));
                        }
                        d.Contacts = contacts;
                    };
                    d.User = Mapper.Map<UsersDb, User>(s.User);

                    if (s.TourContacts != null)
                        d.TourContacts = Mapper.Map<IEnumerable<ContactTourMapDb>, IEnumerable<TourContact>>(s.TourContacts);
                });

            this.CreateMap<ContactTourMapDb, TourContact>()
                .IgnoreAllUnmapped()
                .ForMember(f => f.ContactId, opt => opt.MapFrom(c => c.ContactID))
                .ForMember(c => c.TourId, opt => opt.MapFrom(a => a.TourID))
                .ForMember(c => c.IsCompleted, opt => opt.MapFrom(a => a.IsCompleted))
                .ForMember(c => c.LastUpdatedBy, opt => opt.MapFrom(a => a.LastUpdatedBy))
                .ForMember(c => c.LastUpdatedOn, opt => opt.MapFrom(a => a.LastUpdatedOn));

            this.CreateMap<ActionsDb, DA.Action>()
               .ForMember(c => c.Id, opt => opt.MapFrom(c => c.ActionID))
               .ForMember(c => c.Details, opt => opt.MapFrom(c => c.ActionDetails))
               .ForMember(c => c.RemindOn, opt => opt.MapFrom(c => c.RemindOn))
               .ForMember(n => n.Contacts, opt => opt.Ignore())
               .ForMember(n => n.Tags, opt => opt.Ignore())
               .ForMember(n => n.IsCompleted, opt => opt.Ignore())
               .ForMember(n => n.SelectAll, opt => opt.MapFrom(c => c.SelectAll != null ? c.SelectAll.Value : false))
               .AfterMap((s, d) =>
               {
                   IList<ReminderType> reminderTypes = new List<ReminderType>();
                   if (s.RemindbyEmail.HasValue && s.RemindbyEmail.Value)
                       reminderTypes.Add(ReminderType.Email);
                   if (s.RemindbyText.HasValue && s.RemindbyText.Value)
                       reminderTypes.Add(ReminderType.TextMessage);
                   if (s.RemindbyPopup.HasValue && s.RemindbyPopup.Value)
                       reminderTypes.Add(ReminderType.PopUp);
                   d.ReminderTypes = reminderTypes;

                   if (s.Tags != null)
                   {
                       IList<Tag> tags = new List<Tag>();
                       foreach (TagsDb tagDb in s.Tags)
                       {
                           tags.Add(Mapper.Map<TagsDb, Tag>(tagDb));
                       }
                       d.Tags = tags;
                   }

                   if (s.Contacts != null)
                   {
                       IList<RawContact> contacts = new List<RawContact>();
                       foreach (ContactsDb contactDb in s.Contacts)
                       {
                           contacts.Add(Mapper.Map<ContactsDb, RawContact>(contactDb));
                       }
                       d.Contacts = contacts;
                   }
                   // d.User = Mapper.Map<UsersDb, User>(s.User);
                   if (s.ActionContacts != null)
                   {
                       d.ActionContacts = Mapper.Map<IEnumerable<ContactActionMapDb>, IEnumerable<DA.ActionContact>>(s.ActionContacts).ToList();
                   }
               });

            this.CreateMap<ContactActionMapDb, DA.ActionContact>()
                .IgnoreAllUnmapped()
                .ForMember(c => c.ActionId, opt => opt.MapFrom(a => a.ActionID))
                .ForMember(c => c.ContactId, opt => opt.MapFrom(a => a.ContactID))
                .ForMember(c => c.IsCompleted, opt => opt.MapFrom(a => a.IsCompleted))
                .ForMember(c => c.LastUpdatedBy, opt => opt.MapFrom(a => a.LastUpdatedBy))
                .ForMember(c => c.LastUpdatedOn, opt => opt.MapFrom(a => a.LastUpdatedOn));

            this.CreateMap<ContactTimeLineDb, TimeLineContact>()
           .ForMember(c => c.ID, opt => opt.MapFrom(c => c.TimelineID));

            this.CreateMap<OpportunitiesTimeLineDb, TimeLineContact>()
           .ForMember(c => c.ID, opt => opt.MapFrom(c => c.TimelineID));



            this.CreateMap<AccountsDb, Account>()
                .ForMember(c => c.Id, opt => opt.MapFrom(c => c.AccountID))
                .ForMember(c => c.Addresses, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<AddressesDb>, IEnumerable<Address>>(c.Addresses)))
                .ForMember(c => c.WebAnalyticsProvider, opt => opt.MapFrom(c => Mapper.Map<WebAnalyticsProvidersDb, WebAnalyticsProvider>(c.WebAnalyticsProvider)))
                .ForMember(c => c.subscription, opt => opt.MapFrom(c => Mapper.Map<SubscriptionsDb, Subscription>(c.Subscription)))
                .ForMember(c => c.SubscribedModules, opt => opt.MapFrom(c => Mapper.Map<ICollection<ModulesDb>, ICollection<Module>>(c.SubscribedModules)))
                .ForMember(c => c.HomePhone, opt => opt.MapFrom(c => c.HomePhone != null ? new Phone() { Number = c.HomePhone } : null))
                .ForMember(c => c.WorkPhone, opt => opt.MapFrom(c => c.WorkPhone != null ? new Phone() { Number = c.WorkPhone } : null))
                .ForMember(c => c.AccountLogo, opt => opt.MapFrom(c => Mapper.Map<ImagesDb, Image>(c.Image)))
                .ForMember(c => c.MobilePhone, opt => opt.MapFrom(c => c.MobilePhone != null ? new Phone() { Number = c.MobilePhone } : null))
                .ForMember(c => c.Email, opt => opt.MapFrom(c => c.PrimaryEmail != null ? new Email() { EmailId = c.PrimaryEmail } : null))
                .ForMember(c => c.FacebookUrl, opt => opt.MapFrom(c => c.Communication != null && c.Communication.FacebookUrl != null ? new Url() { URL = c.Communication.FacebookUrl } : null))
                .ForMember(c => c.TwitterUrl, opt => opt.MapFrom(c => c.Communication != null && c.Communication.TwitterUrl != null ? new Url() { URL = c.Communication.TwitterUrl } : null))
                .ForMember(c => c.LinkedInUrl, opt => opt.MapFrom(c => c.Communication != null && c.Communication.LinkedInUrl != null ? new Url() { URL = c.Communication.LinkedInUrl } : null))
                .ForMember(c => c.GooglePlusUrl, opt => opt.MapFrom(c => c.Communication != null && c.Communication.GooglePlusUrl != null ? new Url() { URL = c.Communication.GooglePlusUrl } : null))
                .ForMember(c => c.BlogUrl, opt => opt.MapFrom(c => c.Communication != null && c.Communication.BlogUrl != null ? new Url() { URL = c.Communication.BlogUrl } : null))
                .ForMember(c => c.WebsiteUrl, opt => opt.MapFrom(c => c.Communication != null && c.Communication.WebSiteUrl != null ? new Url() { URL = c.Communication.WebSiteUrl } : null))
                .ForMember(c => c.SubscriptionID, opt => opt.MapFrom(c => c.SubscriptionID))
                .ForMember(c => c.SecondaryEmails, opt => opt.MapFrom(c => c.Communication != null && c.Communication.SecondaryEmails != null
                ? c.Communication.SecondaryEmails.Split(',').Select(e => new Email() { EmailId = e }).ToList() : null))
                .ForMember(c => c.ExcludedRoles, opt => opt.MapFrom(c => c.ExcludedRoles));


            this.CreateMap<BulkOperationsDb, BulkOperations>();

            this.CreateMap<WebAnalyticsProvidersDb, WebAnalyticsProvider>()
                .ForMember(u => u.Id, opt => opt.MapFrom(u => u.WebAnalyticsProviderID))
                .ForMember(u => u.InstantNotificationGroup, opt => opt.Ignore())
                .AfterMap((s, d) =>
                {
                    d.InstantNotificationGroup = s.NotificationGroup != null ? s.NotificationGroup
                        .Where(c => c.NotificationType == WebVisitEmailNotificationType.Instant)
                        .Select(c => c.UserID)
                        .ToList() : new List<int>();

                    d.DailySummaryNotificationGroup = s.NotificationGroup != null ? s.NotificationGroup
                        .Where(c => c.NotificationType == WebVisitEmailNotificationType.DailySummary)
                        .Select(c => c.UserID)
                        .ToList() : new List<int>();
                });

            this.CreateMap<UsersDb, User>()
               .ForMember(u => u.Id, opt => opt.MapFrom(u => u.UserID))
               .ForMember(u => u.Addresses, opt => opt.MapFrom(u => Mapper.Map<ICollection<AddressesDb>, IEnumerable<Address>>(u.Addresses)))
               .ForMember(u => u.Emails, opt => opt.MapFrom(u => Mapper.Map<ICollection<AccountEmailsDb>, IEnumerable<Email>>(u.Emails)))
               .ForMember(p => p.Account, opt => opt.MapFrom(c => Mapper.Map<AccountsDb, Account>(c.Account)))
               .ForMember(c => c.Role, opt => opt.MapFrom(c => Mapper.Map<RolesDb, Role>(c.Role)))
               .ForMember(u => u.Email, opt => opt.MapFrom(u => u.PrimaryEmail != null ? new Email() { EmailId = u.PrimaryEmail } : null))
               .ForMember(c => c.HomePhone, opt => opt.MapFrom(c => c.HomePhone != null ? new Phone() { Number = c.HomePhone, IsPrimary = c.PrimaryPhoneType == "H" ? true : false } : null))
               .ForMember(c => c.WorkPhone, opt => opt.MapFrom(c => c.WorkPhone != null ? new Phone() { Number = c.WorkPhone, IsPrimary = c.PrimaryPhoneType == "W" ? true : false } : null))
               .ForMember(c => c.MobilePhone, opt => opt.MapFrom(c => c.MobilePhone != null ? new Phone() { Number = c.MobilePhone, IsPrimary = c.PrimaryPhoneType == "M" ? true : false } : null))
               .ForMember(c => c.FacebookUrl, opt => opt.MapFrom(c => c.Communication != null && c.Communication.FacebookUrl != null ? new Url() { URL = c.Communication.FacebookUrl } : null))
               .ForMember(c => c.TwitterUrl, opt => opt.MapFrom(c => c.Communication != null && c.Communication.TwitterUrl != null ? new Url() { URL = c.Communication.TwitterUrl } : null))
               .ForMember(c => c.LinkedInUrl, opt => opt.MapFrom(c => c.Communication != null && c.Communication.LinkedInUrl != null ? new Url() { URL = c.Communication.LinkedInUrl } : null))
               .ForMember(c => c.GooglePlusUrl, opt => opt.MapFrom(c => c.Communication != null && c.Communication.GooglePlusUrl != null ? new Url() { URL = c.Communication.GooglePlusUrl } : null))
               .ForMember(c => c.BlogUrl, opt => opt.MapFrom(c => c.Communication != null && c.Communication.BlogUrl != null ? new Url() { URL = c.Communication.BlogUrl } : null))
               .ForMember(c => c.WebsiteUrl, opt => opt.MapFrom(c => c.Communication != null && c.Communication.WebSiteUrl != null ? new Url() { URL = c.Communication.WebSiteUrl } : null))
               .ForMember(c => c.FacebookAccessToken, opt => opt.MapFrom(c => c.Communication != null && c.Communication.FacebookAccessToken != null ? c.Communication.FacebookAccessToken : null))
               .ForMember(c => c.TwitterOAuthToken, opt => opt.MapFrom(c => c.Communication != null && c.Communication.TwitterOAuthToken != null ? c.Communication.TwitterOAuthToken : null))
               .ForMember(c => c.TwitterOAuthTokenSecret, opt => opt.MapFrom(c => c.Communication != null && c.Communication.TwitterOAuthTokenSecret != null ? c.Communication.TwitterOAuthTokenSecret : null))
                //.ForMember(a => a.Role, opt => opt.Ignore())
                // .ForMember(a => a.RoleID, opt => opt.MapFrom(a => a.RoleID))
               .ForMember(c => c.SecondaryEmails, opt => opt.MapFrom(c => c.Communication != null && c.Communication.SecondaryEmails != null
                    ? c.Communication.SecondaryEmails.Split(',').Select(e => new Email() { EmailId = e }).ToList() : null))
                    .AfterMap((s, d) =>
                    {
                        if (s.Account != null)
                            d.Account.Addresses = Mapper.Map<ICollection<AddressesDb>, IEnumerable<Address>>(s.Account.Addresses);
                    });

            #region Authorization
            this.CreateMap<SubscriptionsDb, Subscription>()
                .ForMember(c => c.Id, opt => opt.MapFrom(c => c.SubscriptionID))
                .ForMember(c => c.SubscriptionName, opt => opt.MapFrom(c => c.SubscriptionName));

            this.CreateMap<RolesDb, Role>()
                .ForMember(a => a.Id, opt => opt.MapFrom(a => a.RoleID))
                .ForMember(a => a.RoleName, opt => opt.MapFrom(a => a.RoleName));
            //.ForMember(a => a.ModuleOperations, opt => opt.MapFrom(a => Mapper.Map<ICollection<ModuleOperationsMapDb>, ICollection<ModuleOperation>>(a.ModuleOperations)));

            //this.CreateMap<ModuleOperationsMapDb, ModuleOperation>()
            //    .ForMember(c => c.ModuleOperationId, opt => opt.MapFrom(c => c.ModuleOperationsMapID))
            //    .ForMember(c => c.Module, opt => opt.MapFrom(c => Mapper.Map<ModulesDb, Module>(c.Module)))
            //    .ForMember(c => c.Operation, opt => opt.MapFrom(c => Mapper.Map<OperationsDb, Operation>(c.Operation)));
            //.ForMember(c => c.Subscriptions, opt => opt.MapFrom(c => Mapper.Map<ICollection<SubscriptionsDb>, ICollection<Subscription>>(c.Subscriptions)))  //recent
            //.ForMember(c => c.Roles, opt => opt.MapFrom(c => Mapper.Map<ICollection<RolesDb>, ICollection<Role>>(c.Roles)));

            this.CreateMap<ModulesDb, Module>()
                .ForMember(c => c.Id, opt => opt.MapFrom(c => c.ModuleID))
                .ForMember(c => c.ModuleName, opt => opt.MapFrom(c => c.ModuleName));
            //.ForMember(c => c.Operations, opt => opt.MapFrom(c => Mapper.Map<ICollection<OperationsDb>,ICollection<Operation>>(c.Operations)));

            this.CreateMap<OperationsDb, Operation>()
                .ForMember(c => c.OperationId, opt => opt.MapFrom(c => c.OperationID))
                .ForMember(c => c.OperationName, opt => opt.MapFrom(c => c.OperationName));
            //.ForMember(c => c.Modules, opt => opt.MapFrom(c => Mapper.Map<ICollection<ModulesDb>,ICollection<Module>>(c.Modules)));
            #endregion

            #region UserActivities

            this.CreateMap<UserActivitiesDb, UserActivity>()
                .ForMember(c => c.ActivityName, opt => opt.MapFrom(c => c.ActivityName))
                .ForMember(c => c.Id, opt => opt.MapFrom(c => c.UserActivityID));

            this.CreateMap<UserActivityListDb, UserActivityList>()
                .AfterMap((s, d) =>
                {
                    //List<UserModules> userModules = new List<UserModules>();
                    if (s.UserModules != null && s.UserModules.Any())
                    {
                        IList<UserModules> tags = new List<UserModules>();
                        foreach (UserModulesDb modulesDb in s.UserModules)
                        {
                            tags.Add(Mapper.Map<UserModulesDb, UserModules>(modulesDb));
                        }
                        d.UserModules = tags;
                    }
                });

            this.CreateMap<UserModulesDb, UserModules>();

            this.CreateMap<UserActivityLogsDb, UserActivityLog>()
                .ForMember(c => c.EntityID, opt => opt.MapFrom(c => c.EntityID))
                .ForMember(c => c.Id, opt => opt.MapFrom(c => c.UserActivityLogID))
                .ForMember(c => c.LogDate, opt => opt.MapFrom(c => c.LogDate))
                .ForMember(c => c.ModuleID, opt => opt.MapFrom(c => c.ModuleID))
                .ForMember(c => c.UserActivityID, opt => opt.MapFrom(c => c.UserActivityID))
                .ForMember(c => c.UserID, opt => opt.MapFrom(c => c.UserID))
                .ForMember(c => c.Module, opt => opt.MapFrom(c => Mapper.Map<ModulesDb, Module>(c.Module)))
                .ForMember(c => c.User, opt => opt.MapFrom(c => Mapper.Map<UsersDb, User>(c.User)))
                .ForMember(c => c.UserActivity, opt => opt.MapFrom(c => Mapper.Map<UserActivitiesDb, UserActivity>(c.UserActivities)));

            #endregion

            this.CreateMap<ScoreCategoriesDb, ScoreCategories>()
            .ForMember(u => u.Id, opt => opt.MapFrom(u => u.ScoreCategoryID));

            this.CreateMap<ConditionDb, Condition>()
                .ForMember(u => u.Id, opt => opt.MapFrom(u => u.ConditionID))
                .ForMember(u => u.Category, opt => opt.MapFrom(u => u.ScoreCategory));

            //.ForMember(c => c.Category, opt => opt.MapFrom(c => Mapper.Map<ScoreCategoriesDb, ScoreCategories>(c.ScoreCategory)));

            this.CreateMap<LeadScoreRulesDb, LeadScoreRule>()
                .ForMember(u => u.Id, opt => opt.MapFrom(u => u.LeadScoreRuleID))
                .ForMember(u => u.Tags, opt => opt.Ignore())
                .ForMember(a => a.Category, opt => opt.Ignore())
                .ForMember(a => a.Condition, opt => opt.Ignore())
                .ForMember(a => a.CategoryID, opt => opt.MapFrom(u => u.Condition == null ? 0 : (ScoreCategory)u.Condition.ScoreCategoryID))
                .ForMember(a => a.ConditionID, opt => opt.MapFrom(u => (LeadScoreConditionType)u.ConditionID))
                .AfterMap((s, d) =>
                {
                    if (s.Condition != null)
                    {
                        d.Condition = Mapper.Map<ConditionDb, Condition>(s.Condition);
                        if (s.Condition.ScoreCategory != null)
                            d.Category = Mapper.Map<ScoreCategoriesDb, ScoreCategories>(s.Condition.ScoreCategory);
                    }

                    if (s.SelectedIDs != null && s.SelectedIDs.Any())
                    {
                        if (s.ConditionID == (byte)LeadScoreConditionType.ContactOpensEmail)
                            d.SelectedCampaignID = s.SelectedIDs.ToArray();
                        else if (s.ConditionID == (byte)LeadScoreConditionType.ContactClicksLink)
                        {
                            d.SelectedCampaignID = s.SelectedIDs.ToArray();
                            d.SelectedCampaignLinkID = s.SelectedCampaignLinks == null || s.SelectedCampaignLinks == "" ? null : s.SelectedCampaignLinks.Split(',').Select(int.Parse).ToArray();
                        }
                        else if (s.ConditionID == (byte)LeadScoreConditionType.ContactSubmitsForm)
                            d.SelectedFormID = s.SelectedIDs.ToArray();
                        else if (s.ConditionID == (byte)LeadScoreConditionType.ContactActionTagAdded ||
                                 s.ConditionID == (byte)LeadScoreConditionType.ContactNoteTagAdded)
                        {
                            d.SelectedTagID = s.SelectedIDs.ToArray();
                        }
                        else if (s.ConditionID == (byte)LeadScoreConditionType.ContactLeadSource)
                        {
                            d.SelectedLeadSourceID = s.SelectedIDs.ToArray();
                        }
                        else if (s.ConditionID == (byte)LeadScoreConditionType.ContactTourType)
                            d.SelectedTourTypeID = s.SelectedIDs.ToArray();
                        else if (s.ConditionID == (byte)LeadScoreConditionType.ContactNoteCategoryAdded)
                            d.SelectedNoteCategoryID = s.SelectedIDs.ToArray();
                        else if (s.ConditionID == (byte)LeadScoreConditionType.ContactActionCompleted)
                            d.SelectedActionTypeID = s.SelectedIDs.ToArray();
                    }

                });
            this.CreateMap<LeadScoreDb, LeadScore>()
               .ForMember(a => a.Id, opt => opt.MapFrom(a => a.LeadScoreID));

            this.CreateMap<LeadSourceDb, LeadSource>()
                .ForMember(a => a.Id, opt => opt.MapFrom(a => a.LeadSourceID));
            //.ForMember(a => a.Name, opt => opt.MapFrom(a => a.Name));


            this.CreateMap<LeadScoreConditionValuesDb, LeadScoreConditionValue>()
                .ForMember(c => c.LeadScoreConditionValueId, opt => opt.MapFrom(c => c.LeadScoreConditionValueID))
                .ForMember(c => c.LeadScoreRuleId, opt => opt.MapFrom(c => c.LeadScoreRuleID))
                .ForMember(c => c.ValueType, opt => opt.MapFrom(c => (LeadScoreConditionType)c.ValueType));

            #region UserSettingsDb to UserSettings

            this.CreateMap<UserSettingsDb, UserSettings>()
             .ForMember(a => a.Id, opt => opt.MapFrom(a => a.UserSettingID))
             .ForMember(a => a.CurrencyFormat, opt => opt.MapFrom(a => a.Currency.Format))
             .ForMember(a => a.DateFormatType, opt => opt.MapFrom(a => a.DateFormatName));

            #endregion


            #region OpportunitiesDb to Opportunities

            this.CreateMap<OpportunitiesDb, Opportunity>()
            .ForMember(a => a.Id, opt => opt.MapFrom(a => a.OpportunityID))
            .ForMember(a => a.OwnerId, opt => opt.MapFrom(a => a.Owner))
            .ForMember(a => a.Stage, opt => opt.Ignore())
            .ForMember(a => a.IsDeleted, opt => opt.Ignore())
            .AfterMap((s, d) =>
            {
                if (s.ContactsMap != null)
                {
                    IList<int> contacts = new List<int>();
                    foreach (OpportunityContactMap contactMap in s.ContactsMap)
                    {
                      ///  contactMap.is
                        contacts.Add(contactMap.ContactID);
                    }
                    d.Contacts = contacts;
                }

                if (s.OpportunitiesRelations != null)
                {
                    IList<PeopleInvolved> people = new List<PeopleInvolved>();
                    foreach (OpportunitiesRelationshipMapDb relationMap in s.OpportunitiesRelations)
                    {
                        people.Add(Mapper.Map<OpportunitiesRelationshipMapDb, PeopleInvolved>(relationMap));
                    }
                    d.PeopleInvolved = people;
                }

                if (s.OpportunityTags != null)
                {
                    IList<Tag> tags = new List<Tag>();
                    foreach (OpportunityTagMap tagMap in s.OpportunityTags)
                    {
                        tags.Add(Mapper.Map<TagsDb, Tag>(tagMap.Tags));
                    }
                    d.OpportunityTags = tags;
                }

                if (s.Statuses != null)
                {
                    d.Stage = s.Statuses.DropdownValue;
                }

                if (s.Users != null)
                {
                    d.UserName = s.Users.FirstName + " " + s.Users.LastName + " ( " + s.Users.PrimaryEmail + " ) ";
                }
            });

            this.CreateMap<OpportunitiesRelationshipMapDb, PeopleInvolved>()
             .ForMember(c => c.PeopleInvolvedID, opt => opt.MapFrom(c => c.OpportunityRelationshipMapID))
             .ForMember(c => c.RelationshipTypeName, opt => opt.MapFrom(c => c.RelationshipTypes.DropdownValue))
             .ForMember(c => c.ContactFullName, opt => opt.MapFrom(c => c.Contacts.FirstName + " " + c.Contacts.LastName))
            .ForMember(c => c.CompanyName, opt => opt.MapFrom(c => c.Contacts.Company))
            .ForMember(c => c.ContactType, opt => opt.MapFrom(c => c.Contacts.ContactType))
            .ForMember(c => c.LifeCycleStage, opt => opt.MapFrom(c => c.Contacts.LifecycleStage))
            .AfterMap((s, d) =>
            {
                if (string.IsNullOrEmpty(d.ContactFullName.Trim()))
                {
                    d.ContactFullName = s.Contacts.PrimaryEmail;
                }
            });

            #endregion

            #region AdvancedSearch

            this.CreateMap<SearchDefinitionsDb, SearchDefinition>()
            .ForMember(a => a.Id, opt => opt.MapFrom(a => a.SearchDefinitionID))
            .ForMember(a => a.Name, opt => opt.MapFrom(a => a.SearchDefinitionName))
             .ForMember(a => a.IsPreConfiguredSearch, opt => opt.MapFrom(a => a.IsPreConfiguredSearch))
              .ForMember(a => a.IsFavoriteSearch, opt => opt.MapFrom(a => a.IsFavoriteSearch))
            .ForMember(a => a.PredicateType, opt => opt.MapFrom(a => (SearchPredicateType)a.SearchPredicateTypeID))
            .ForMember(a => a.TagsList, opt => opt.Ignore())
            .ForMember(a => a.Filters, opt => opt.Ignore())
            .ForMember(a => a.Fields, opt => opt.MapFrom(a => Mapper.Map<IEnumerable<FieldsDb>, IEnumerable<Field>>(a.Fields)))
            .AfterMap((s, d) =>
            {
                if (s.SearchTags != null)
                {
                    IList<Tag> tags = new List<Tag>();
                    foreach (SearchDefinitionTagMapDb tagMap in s.SearchTags)
                    {
                        tags.Add(Mapper.Map<TagsDb, Tag>(tagMap.Tags));
                    }
                    d.TagsList = tags;
                }

                if (s.SearchFilters != null)
                {
                    IList<SearchFilter> Filters = new List<SearchFilter>();
                    foreach (SearchFiltersDb filter in s.SearchFilters)
                    {
                        Filters.Add(Mapper.Map<SearchFiltersDb, SearchFilter>(filter));
                    }
                    d.Filters = Filters;
                }
                if (!string.IsNullOrEmpty(s.CustomPredicateScript))
                    d.CustomPredicateScript = s.CustomPredicateScript.ToUpper();
            });

            this.CreateMap<SearchFiltersDb, SearchFilter>()
                .ForMember(a => a.SearchFilterId, opt => opt.MapFrom(a => a.SearchFilterID))
                .ForMember(a => a.DropdownValueId, opt => opt.MapFrom(a => a.DropdownValueID))
                .ForMember(a => a.IsDropdownField, opt => opt.MapFrom(a => a.DropdownValueID != null ? true : false))
                .ForMember(a => a.Qualifier, opt => opt.MapFrom(a => (SearchQualifier)a.SearchQualifierTypeID))
                //.ForMember(a => a.Field, opt => opt.MapFrom(a => (ContactFields)a.FieldID))
                .AfterMap((s, d) =>
                {
                    if (s.FieldID == null)
                    {
                        d.Field = ContactFields.DropdownField;
                    }
                    else
                        d.Field = (ContactFields)s.FieldID;

                    if (s.DropdownValue != null)
                        d.DropdownId = s.DropdownValue.DropdownID;
                    if (s.Fields != null)
                    {
                        d.IsCustomField = s.Fields.AccountID != null ? true : false;
                        d.FieldOptionTypeId = s.Fields.FieldInputTypeID;
                    }
                });

            this.CreateMap<AVColumnPreferencesDb, AVColumnPreferences>();


            #endregion

            #region Workflows

            this.CreateMap<WorkflowsDb, Workflow>()
               .ForMember(n => n.Id, opt => opt.MapFrom(n => n.WorkflowID))
               .ForMember(n => n.StatusID, opt => opt.MapFrom(n => n.Status))
               .ForMember(n => n.RemovefromWorkflows, opt => opt.MapFrom(n => n.RemovedWorkflows))
               .AfterMap((s, d) =>
               {
                   if (s.Statusses != null)
                       d.StatusName = s.Statusses.Name;

               });

            this.CreateMap<WorkflowTriggersDb, WorkflowTrigger>()
                .ForMember(x => x.CampaignName, opt => opt.MapFrom(g => g.Campaigns == null ? "" : g.Campaigns.Name))
                .ForMember(x => x.TagName, opt => opt.MapFrom(g => g.Tags == null ? "" : g.Tags.TagName))
                .ForMember(x => x.SearchDefinitionName, opt => opt.MapFrom(g => g.SearchDefinitions == null ? "" : g.SearchDefinitions.SearchDefinitionName))
                .ForMember(x => x.FormName, opt => opt.MapFrom(g => g.Forms == null ? "" : g.Forms.Name))
                .ForMember(x => x.LifecycleName, opt => opt.MapFrom(g => g.DropdownValues == null ? "" : g.DropdownValues.DropdownValue))
                .ForMember(x => x.OpportunityStageName, opt => opt.MapFrom(g => g.OpportunityStageValues == null ? "" : g.OpportunityStageValues.DropdownValue))
                .ForMember(x => x.LeadAdapterName, opt => opt.MapFrom(g => g.LeadAdapter == null ? "" : g.LeadAdapter.LeadAdapterTypes.Name))
                .ForMember(x => x.ActionTypeName, opt => opt.MapFrom(g => g.ActionTypeDropdownValue == null ? "" : g.ActionTypeDropdownValue.DropdownValue))
                .ForMember(x => x.TourTypeName, opt => opt.MapFrom(g => g.TourTypeDropdownValue == null ? "":g.TourTypeDropdownValue.DropdownValue))
                .ForMember(x => x.SelectedLinks, opt => opt.Ignore())
                .ForMember(x => x.Operator, opt => opt.MapFrom(g => g.DurationOperator))
                .AfterMap((s, d) =>
                {
                    IList<int> campaignLinks = new List<int>();
                    if (!string.IsNullOrEmpty(s.SelectedLinks))
                    {
                        int[] links = Array.ConvertAll(s.SelectedLinks.Split(','), g => int.Parse(g));
                        campaignLinks = links;
                    }
                    d.SelectedLinks = campaignLinks;
                });

            this.CreateMap<BaseWorkflowActionsDb, BaseWorkflowAction>()
                .Include<WorkflowLeadScoreActionsDb, WorkflowLeadScoreAction>()
                .Include<WorkflowCampaignActionsDb, WorkflowCampaignAction>()
                .Include<WorkflowTagActionsDb, WorkflowTagAction>()
                .Include<WorkflowLifeCycleActionsDb, WorkflowLifeCycleAction>()
                .Include<WorkFlowUserAssignmentActionsDb, WorkflowUserAssignmentAction>()
                .Include<WorkflowNotifyUserActionsDb, WorkflowNotifyUserAction>()
                .Include<WorkflowContactFieldActionsDb, WorkflowContactFieldAction>()
                .Include<WorkflowTimerActionsDb, WorkflowTimerAction>()
                .Include<WorkFlowTextNotificationActionsDb, WorkflowTextNotificationAction>()
                .Include<WorkflowEmailNotificationActionDb, WorkflowEmailNotificationAction>()
                .Include<TriggerWorkflowActionsDb, TriggerWorkflowAction>();

            this.CreateMap<WorkflowActionsDb, WorkflowAction>();

            this.CreateMap<WorkflowCampaignActionsDb, WorkflowCampaignAction>()
                  .ForMember(x => x.CampaignName, opt => opt.MapFrom(x => x.Campaigns == null ? "" : x.Campaigns.Name))
                  .AfterMap((s, d) =>
                  {
                      d.Links = Mapper.Map<IEnumerable<WorkflowCampaignActionLinksDb>, IEnumerable<WorkflowCampaignActionLink>>(s.Links);
                  });

            this.CreateMap<WorkflowCampaignActionLinksDb, WorkflowCampaignActionLink>()
                .AfterMap((s, d) =>
                {
                    d.Actions = Mapper.Map<IEnumerable<WorkflowActionsDb>, IEnumerable<WorkflowAction>>(s.LinkActions);
                });
            this.CreateMap<WorkflowLeadScoreActionsDb, WorkflowLeadScoreAction>();

            this.CreateMap<WorkflowTriggerTypesDb, WorkflowTriggerType>();
            this.CreateMap<TriggerWorkflowActionsDb, TriggerWorkflowAction>()
                .ForMember(g => g.WorkflowName, opt => opt.MapFrom(m => m.Workflow == null ? "" : m.Workflow.WorkflowName));

            this.CreateMap<WorkflowTagActionsDb, WorkflowTagAction>()
                .ForMember(g => g.TagName, opt => opt.MapFrom(g => g.Tags == null ? "" : g.Tags.TagName));

            this.CreateMap<WorkflowLifeCycleActionsDb, WorkflowLifeCycleAction>()
                .ForMember(g => g.LifecycleName, opt => opt.MapFrom(g => g.DropdownValues == null ? "" : g.DropdownValues.DropdownValue));

            this.CreateMap<RoundRobinContactAssignmentDb, RoundRobinContactAssignment>();
            this.CreateMap<WorkFlowUserAssignmentActionsDb, WorkflowUserAssignmentAction>()
                 //.ForMember(g => g.UserNames, opt => opt.MapFrom(g => g.Users == null ? "" : (g.Users.FirstName + " " + g.Users.LastName)))
                 .ForMember(g => g.RoundRobinContactAssignments, opt => opt.MapFrom(o => Mapper.Map<IEnumerable<RoundRobinContactAssignmentDb>, IEnumerable<RoundRobinContactAssignmentDb>>(o.RoundRobinContactAssignments)));
                  //.ForMember(d => d.UserID, s => s.Ignore())
                  //.AfterMap((s, d) =>
                  //{
                  //    if (!string.IsNullOrEmpty(s.UserID))
                  //        d.UserID = s.UserID.Split(',').Select(se => Convert.ToInt32(se));
                  //});

            this.CreateMap<WorkflowNotifyUserActionsDb, WorkflowNotifyUserAction>()
                  .ForMember(d => d.UserID, s => s.Ignore())
                  .AfterMap((s, d) =>
                  {
                      if (!string.IsNullOrEmpty(s.UserID))
                      {
                          d.UserID = s.UserID.Split(',').Select(se => Convert.ToInt32(se));
                      }
                  })
                  .ForMember(d => d.NotificationFields, s => s.Ignore())
                  .AfterMap((s, d) =>
                   {
                       if (!string.IsNullOrEmpty(s.NotificationFields))
                       {
                           d.NotificationFieldID = s.NotificationFields.Split(',').Select(se => Convert.ToInt32(se));
                       }
                   });


            this.CreateMap<WorkflowContactFieldActionsDb, WorkflowContactFieldAction>()
                  .ForMember(g => g.FieldName, opt => opt.MapFrom(x => x.Fields.Title))
                  .ForMember(g => g.FieldID, opt => opt.Ignore())
                  .ForMember(g => g.FieldInputTypeId, opt => opt.Ignore())
                  .AfterMap((s, d) =>
                  {
                      if (s.FieldID.HasValue)
                          d.FieldID = s.FieldID.Value;
                      else
                      {
                          d.FieldID = s.DropdownValueID.Value;
                          d.IsDropdownField = true;
                      }

                      if (s.Fields != null)
                          d.FieldInputTypeId = (FieldType)s.Fields.FieldInputTypeID;
                  });

            this.CreateMap<WorkflowEmailNotificationActionDb, WorkflowEmailNotificationAction>();
            this.CreateMap<WorkFlowTextNotificationActionsDb, WorkflowTextNotificationAction>();
            this.CreateMap<WorkflowTimerActionsDb, WorkflowTimerAction>()
                  .ForMember(x => x.DaysOfWeek, opt => opt.Ignore())
                  .AfterMap((s, d) =>
                  {
                      IList<DayOfWeek> daysofWeeks = new List<DayOfWeek>();
                      if (!string.IsNullOrEmpty(s.DaysOfWeek))
                      {
                          var daysdata = s.DaysOfWeek;
                          byte[] bytes = daysdata.Split(',').Select(g => Convert.ToByte(g, 16)).ToArray();
                          foreach (byte byt in bytes)
                              daysofWeeks.Add((DayOfWeek)byt);

                      }
                      d.DaysOfWeek = daysofWeeks;
                  });
            #endregion


            this.CreateMap<FormsDb, Form>()
                      .ForMember(a => a.Id, opt => opt.MapFrom(a => a.FormID))
                      .ForMember(a => a.FormFields, opt => opt.Ignore())
                      .ForMember(a => a.AccountID, opt => opt.MapFrom(a => a.AccountID))
                      .ForMember(a => a.CreatedDate, opt => opt.MapFrom(a => a.CreatedOn))
                      .AfterMap((s, d) =>
                      {
                          d.FormFields = Mapper.Map<ICollection<FormFieldsDb>, IList<FormField>>(s.FormFields);
                          foreach (var formField in d.FormFields)
                          {
                              formField.AccountID = d.AccountID;
                          }
                          if (s.FormTags != null)
                          {
                              IList<Tag> tags = new List<Tag>();
                              foreach (FormTagsDb tagsDb in s.FormTags)
                              {
                                  tags.Add(Mapper.Map<TagsDb, Tag>(tagsDb.Tag));
                              }
                              d.Tags = tags;
                          }
                      });

            this.CreateMap<FormSubmissionDb, FormSubmission>()
                      .ForMember(a => a.Id, opt => opt.MapFrom(a => a.FormSubmissionID))
                      .ForMember(a => a.FormId, opt => opt.MapFrom(a => a.FormID))
                      .ForMember(a => a.ContactId, opt => opt.MapFrom(a => a.ContactID))
                      .ForMember(a => a.Form, opt => opt.MapFrom(a => Mapper.Map<FormsDb, Form>(a.Form)));


            this.CreateMap<FormFieldsDb, FormField>()
                .ForMember(f => f.Id, opt => opt.MapFrom(f => f.FieldID))
                .ForMember(f => f.FieldCode, opt => opt.Ignore())
                .ForMember(f => f.SortId, opt => opt.MapFrom(f => f.SortID))
                .AfterMap((s, d) =>
                {
                    if (s.Field != null)
                    {
                        d.FieldCode = s.Field.FieldCode;
                        d.Title = s.Field.Title;
                        d.ValidationMessage = s.Field.ValidationMessage;
                        d.FieldInputTypeId = (FieldType)s.Field.FieldInputTypeID;
                    }
                });


            this.CreateMap<FieldsDb, Field>()
                .ForMember(a => a.Id, opt => opt.MapFrom(a => a.FieldID))
                .ForMember(a => a.FieldInputTypeId, opt => opt.MapFrom(a => a.FieldInputTypeID))
                .ForMember(a => a.Value, opt => opt.Ignore())
                .ForMember(a => a.StatusId, opt => opt.MapFrom(a => (short?)a.StatusID))
                .ForMember(a => a.Title, opt => opt.MapFrom(a => a.Title))
                .ForMember(a => a.ValueOptions, opt => opt.Ignore())
                .ForMember(a => a.IsCustomField, opt => opt.MapFrom(a => a.AccountID == null ? false : true))
                .ForMember(a => a.IsLeadAdapterField, opt => opt.MapFrom(a => a.IsLeadAdapterField))
                .ForMember(a => a.LeadAdapterType, opt => opt.MapFrom(a => a.LeadAdapterType))
                .AfterMap((s, d) =>
                {
                    var domain = s.CustomFieldValueOptions != null ? s.CustomFieldValueOptions.Where(c => c.IsDeleted == false) : s.CustomFieldValueOptions;
                    d.ValueOptions = Mapper.Map<IEnumerable<CustomFieldValueOptionsDb>, IEnumerable<FieldValueOption>>(domain).OrderBy(c => c.Order);

                    Func<LeadAdapterTypes, string, string> SetTitle = (lt, sourceTitle) =>
                        {
                            var ltype = "(" + lt.ToString() + ")";
                            return sourceTitle + ltype;
                        };

                    if (s.IsLeadAdapterField)
                    {
                        if (s.LeadAdapterType == null) d.Title = s.Title;
                        else d.Title = SetTitle((LeadAdapterTypes)s.LeadAdapterType, s.Title);
                    }
                    else d.Title = s.Title;
                });

            this.CreateMap<FieldsDb, CustomField>()
                .ForMember(a => a.Id, opt => opt.MapFrom(a => a.FieldID))
                .ForMember(a => a.FieldInputTypeId, opt => opt.MapFrom(a => a.FieldInputTypeID))
                .ForMember(a => a.Value, opt => opt.Ignore())
                .ForMember(a => a.AccountID, opt => opt.MapFrom(a => a.AccountID))
                .ForMember(a => a.StatusId, opt => opt.MapFrom(a => (short?)a.StatusID))
                .ForMember(a => a.Title, opt => opt.MapFrom(a => a.Title))
                .ForMember(a => a.ValueOptions, opt => opt.Ignore())
                .ForMember(a => a.SortId, opt => opt.MapFrom(a => a.SortID))
                .ForMember(a => a.SectionId, opt => opt.MapFrom(a => a.CustomFieldSectionID))
                .AfterMap((s, d) =>
                {
                    var domain = s.CustomFieldValueOptions != null ? s.CustomFieldValueOptions.Where(c => c.IsDeleted == false) : s.CustomFieldValueOptions;
                    d.ValueOptions = Mapper.Map<IEnumerable<CustomFieldValueOptionsDb>, IEnumerable<FieldValueOption>>(domain).OrderBy(c => c.Order);
                });

            #region LeadAdapters

            this.CreateMap<LeadAdapterAndAccountMapDb, LeadAdapterAndAccountMap>()
             .ForMember(u => u.Id, opt => opt.MapFrom(u => u.LeadAdapterAndAccountMapId))
             .ForMember(u => u.LeadAdapterErrorStatusID, opt => opt.MapFrom(u => u.LeadAdapterErrorStatusID))
             .ForMember(u => u.AccountName, opt => opt.MapFrom(u => u.AccountName))
             .ForMember(u => u.Tags, opt => opt.Ignore())
             .ForMember(u => u.LeadAdapterServiceStatusID, opt => opt.Ignore())
             .ForMember(u => u.FacebookLeadAdapter, opt => opt.MapFrom(m => Mapper.Map<FacebookLeadAdapterDb, FacebookLeadAdapter>(m.FacebookLeadAdapter)))
             .AfterMap((s, d) =>
             {
                 if (s.LeadAdapterErrorStatus != null)
                 {
                     d.LeadAdapterErrorStatusID = (LeadAdapterErrorStatus)s.LeadAdapterErrorStatus.LeadAdapterErrorStatusID;
                     d.LeadAdapterErrorName = s.LeadAdapterErrorStatus.LeadAdapterErrorStatus;
                 }
                 if (s.Tags != null)
                 {
                     IList<Tag> tags = new List<Tag>();
                     foreach (LeadAdapterTagMapDb tagsDb in s.Tags)
                     {
                         tags.Add(Mapper.Map<TagsDb, Tag>(tagsDb.Tag));
                     }
                     d.Tags = tags;
                 }
                 if (s.Statuses != null)
                 {
                     d.LeadAdapterServiceStatusID = (LeadAdapterServiceStatus)s.Statuses.StatusID;
                     d.ServiceStatusMessage = s.Statuses.Description;
                 }
                 if (s.Account != null)
                     d.AccountName = s.Account.AccountName;
             });

            this.CreateMap<FacebookLeadAdapterDb, FacebookLeadAdapter>()
                .ForMember(f => f.Id, opt => opt.MapFrom(m => m.FacebookLeadAdapterID));

            this.CreateMap<FacebookLeadGenDb, FacebookLeadGen>();

            this.CreateMap<LeadAdapterJobLogDetailsDb, LeadAdapterJobLogDetails>()
             .ForMember(u => u.Id, opt => opt.MapFrom(u => u.LeadAdapterJobLogDetailID))
             .ForMember(u => u.LeadAdapterRecordStatus, opt => opt.MapFrom(u => u.LeadAdapterRecordStatus != null ? u.LeadAdapterRecordStatus.Title : ""));

            this.CreateMap<LeadAdapterJobLogsDb, LeadAdapterJobLogs>()
             .ForMember(u => u.Id, opt => opt.MapFrom(u => u.LeadAdapterJobLogID))
             .AfterMap((s, d) =>
             {
                 if (!string.IsNullOrEmpty(s.FileName))
                 {
                     d.FileName = System.IO.Path.GetFileName(s.FileName);
                 }
             });

            this.CreateMap<LeadAdapterTypesDb, FieldValueOption>()
                .ForMember(u => u.FieldId, opt => opt.MapFrom(u => (int)ContactFields.LeadAdapter))
                .ForMember(u => u.Value, opt => opt.MapFrom(u => u.Name))
                .ForMember(u => u.Id, opt => opt.MapFrom(u => u.LeadAdapterTypeID));


            #endregion


            #region ImportData
            this.CreateMap<GetImportDataDb, ImportData>()
                  .ForMember(u => u.Id, opt => opt.MapFrom(u => u.LeadAdapterJobLogID));

            this.CreateMap<ImportDataSettingsDb, ImportDataSettings>()
                  .ForMember(u => u.ImportDataSettingID, opt => opt.MapFrom(u => u.ImportDataSettingID))
                  .ForMember(u => u.UniqueImportIdentifier, opt => opt.MapFrom(u => u.UniqueImportIdentifier))
                  .ForMember(u => u.UpdateOnDuplicate, opt => opt.MapFrom(u => u.UpdateOnDuplicate));


            #endregion



            this.CreateMap<NotificationDb, Notification>()
                .ForMember(n => n.Time, opt => opt.MapFrom(nt => nt.NotificationTime))
                .ForMember(n => n.EntityId, opt => opt.MapFrom(nt => nt.EntityID.HasValue ? nt.EntityID.Value : 0));

            this.CreateMap<CustomFieldTabDb, CustomFieldTab>()
                .ForMember(c => c.Id, opt => opt.MapFrom(c => c.CustomFieldTabID))
                .ForMember(c => c.AccountId, opt => opt.MapFrom(c => c.AccountID))
                .ForMember(c => c.StatusId, opt => opt.MapFrom(c => c.StatusID))
                .ForMember(c => c.SortId, opt => opt.MapFrom(c => c.SortID))
                .ForMember(c => c.Sections, opt => opt.Ignore())
                .AfterMap((s, d) =>
                {
                    var domain = s.CustomFieldSections != null ? s.CustomFieldSections.Where(cfs => cfs.StatusID != (short)CustomFieldSectionStatus.Deleted) : s.CustomFieldSections;
                    d.Sections = Mapper.Map<IEnumerable<CustomFieldSectionDb>, IEnumerable<CustomFieldSection>>(domain).OrderBy(c => c.SortId);
                });



            this.CreateMap<CustomFieldSectionDb, CustomFieldSection>()
                .ForMember(s => s.Id, opt => opt.MapFrom(s => s.CustomFieldSectionID))
                .ForMember(c => c.StatusId, opt => opt.MapFrom(c => c.StatusID))
                .ForMember(c => c.SortId, opt => opt.MapFrom(c => c.SortID))
                .ForMember(c => c.TabId, opt => opt.MapFrom(c => c.TabID))
                .ForMember(c => c.CustomFields, opt => opt.Ignore())
                .AfterMap((s, d) =>
                {
                    var domain = s.CustomFields != null ? s.CustomFields.Where(c => c.StatusID != (short?)FieldStatus.Deleted) : s.CustomFields;
                    d.CustomFields = Mapper.Map<IEnumerable<FieldsDb>, IEnumerable<CustomField>>(domain).OrderBy(c => c.SortId);
                });


            this.CreateMap<CustomFieldValueOptionsDb, FieldValueOption>()
                .ForMember(c => c.Id, opt => opt.MapFrom(c => c.CustomFieldValueOptionID))
                .ForMember(c => c.FieldId, opt => opt.MapFrom(c => c.CustomFieldID));

            this.CreateMap<ContactCustomFieldsDb, ContactCustomField>()
                .ForMember(c => c.ContactCustomFieldMapId, opt => opt.MapFrom(c => c.ContactCustomFieldMapID))
                .ForMember(c => c.ContactId, opt => opt.MapFrom(c => c.ContactID))
                .ForMember(c => c.CustomFieldId, opt => opt.MapFrom(c => c.CustomFieldID))
                .AfterMap((s, d) =>
                {
                    if (s.CustomField != null)
                        d.FieldInputTypeId = s.CustomField.FieldInputTypeID;
                });

            this.CreateMap<DropdownValueDb, FieldValueOption>()
                .ForMember(c => c.Id, opt => opt.MapFrom(c => c.DropdownValueID))
                .ForMember(c => c.FieldId, opt => opt.MapFrom(c => c.DropdownID))
                .ForMember(c => c.Value, opt => opt.MapFrom(c => c.DropdownValue))
                .ForMember(c => c.IsDeleted, opt => opt.MapFrom(c => c.IsActive));

            this.CreateMap<WebVisitsDb, WebVisit>()
                .ForMember(w => w.Id, opt => opt.MapFrom(w => w.ContactWebVisitID))
                .ForMember(w => w.ContactID, opt => opt.MapFrom(w => w.ContactID))
                .ForMember(w => w.State, opt => opt.MapFrom(w => w.Region));

            this.CreateMap<WebVisitEmailAuditDb, WebVisitEmailAudit>()
                .ForMember(w => w.AuditID, opt => opt.MapFrom(w => w.AuditID));

            this.CreateMap<UserSocialMediaPostsDb, UserSocialMediaPosts>();

            this.CreateMap<CRMOutlookSyncDb, CRMOutlookSync>()
                .ForMember(o => o.OutlookSyncId, opt => opt.MapFrom(o => o.OutlookSyncID));

            this.CreateMap<DashboardSettingsDb, DashboardItems>()
                .ForMember(o => o.Id, opt => opt.MapFrom(o => o.DashBoardID));

            this.CreateMap<ThirdPartyClientsDb, ThirdPartyClient>()
                .ForMember(o => o.ID, opt => opt.MapFrom(o => o.ID))
                .ForMember(o => o.AccountName, opt => opt.MapFrom(o => o.Account.AccountName));

            this.CreateMap<ClientRefreshTokensDb, ClientRefreshToken>();

            this.CreateMap<SeedEmailDb, SeedEmail>()
              .ForMember(o => o.Id, opt => opt.MapFrom(o => o.SeedID));
            this.CreateMap<AccountSettingsDb, AccountSettings>()
                .ForMember(o => o.Id, opt => opt.MapFrom(o => o.AccountID));

            this.CreateMap<LeadScoreMessageDb, LeadScoreMessage>();
            this.CreateMap<TrackMessagesDb, TrackMessage>();
            this.CreateMap<TrackActionsDb, TrackAction>();

            this.CreateMap<ApplicationTourDetailsDb, ApplicationTourDetails>()
                .ForMember(f => f.Division, opt => opt.MapFrom(m => m.Division))
                .ForMember(f => f.Section, opt => opt.MapFrom(m => m.Section));

            this.CreateMap<DivisionsDb, Divisions>();
            this.CreateMap<SectionsDB, Sections>();


            this.CreateMap<MarketingMessagesDb, MarketingMessage>();

            this.CreateMap<MarketingMessageAccountMapDb, MarketingMessageAccountMap>();
            this.CreateMap<MarketingMessageContentMapDb, MarketingMessageContentMap>();

            #region SuppressionList
            this.CreateMap<SuppressedEmailsDb, SuppressedEmail>()
                .ForMember(f => f.Id, opt => opt.MapFrom(f => f.SuppressedEmailID))
                .ForMember(f => f.Email, opt => opt.MapFrom(f => f.Email.ToLower()));

            this.CreateMap<SuppressedDomainsDb, SuppressedDomain>()
                .ForMember(f => f.Id, opt => opt.MapFrom(f => f.SuppressedDomainID))
                .ForMember(f => f.Domain, opt => opt.MapFrom(f => f.Domain.ToLower()));
            #endregion

            this.CreateMap<CommunicationsDb, Communication>()
                .ForMember(f => f.Id, opt => opt.MapFrom(m => m.CommunicationID));

            this.CreateMap<NeverBounceResult, NeverBounceEmailStatusDb>()
                .ForMember(f => f.ContactID, opt => opt.MapFrom(m => m.ContactID))
                .ForMember(f => f.NeverBounceRequestID, opt => opt.MapFrom(m => m.NeverBounceRequestID))
                .ForMember(f => f.ContactEmailID, opt => opt.MapFrom(m => m.ContactEmailID))
                .ForMember(f => f.CreatedOn, opt => opt.MapFrom(m => DateTime.UtcNow))
                .AfterMap((s, d) => 
                {
                    if (s.IsValid)
                        d.EmailStatus = (short)EmailStatus.Verified;
                    else
                        d.EmailStatus = (short)EmailStatus.HardBounce;
                });
        }
    }
}
