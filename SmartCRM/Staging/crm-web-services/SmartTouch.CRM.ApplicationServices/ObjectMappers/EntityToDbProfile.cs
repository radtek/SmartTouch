using AutoMapper;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.AccountSettings;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.Communications;
using SmartTouch.CRM.Domain.ContactAudit;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.CustomFields;
using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.Domain.Fields;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Domain.ImageDomains;
using SmartTouch.CRM.Domain.Images;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Domain.LeadScoreRules;
using SmartTouch.CRM.Domain.Login;
using SmartTouch.CRM.Domain.MarketingMessageCenter;
using SmartTouch.CRM.Domain.Notes;
using SmartTouch.CRM.Domain.Opportunities;
using SmartTouch.CRM.Domain.Search;
using SmartTouch.CRM.Domain.SeedList;
using SmartTouch.CRM.Domain.Subscriptions;
using SmartTouch.CRM.Domain.SuppressedEmails;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.Tours;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.WebAnalytics;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Repository;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DA = SmartTouch.CRM.Domain.Actions;

namespace SmartTouch.CRM.ApplicationServices.ObjectMappers
{
    public class EntityToDbProfile : Profile
    {
        public new string ProfileName
        {
            get
            {
                return "EntityToDbProfile";
            }
        }

        protected override void Configure()
        {
            this.CreateMap<Campaign, CampaignsDb>()
                .ForMember(c => c.CampaignID, opt => opt.MapFrom(c => c.Id))
                .ForMember(c => c.CampaignTypeID, opt => opt.MapFrom(c => (byte?)c.CampaignTypeId))
                .ForMember(c => c.CampaignType, opt => opt.Ignore())
                .ForMember(c => c.Contacts, opt => opt.Ignore())
                .ForMember(c => c.Tags, opt => opt.Ignore())
                .ForMember(c => c.SearchDefinitions, opt => opt.Ignore())
                .ForMember(c => c.CampaignStatusID, opt => opt.MapFrom(c => c.CampaignStatus))
                .ForMember(c => c.IsDeleted, opt => opt.MapFrom(c => false)) //Revisit. See if this property is still needed.
                .AfterMap((s, d) =>
                {
                    d.CampaignTemplateID = s.Template.Id;
                    d.Links = Mapper.Map<IEnumerable<CampaignLink>, IEnumerable<CampaignLinksDb>>(s.Links);
                });

            this.CreateMap<CampaignTheme, CampaignThemesDb>()
                .ForMember(t => t.CampaignThemeID, opt => opt.MapFrom(c => c.CampaignThemeID));
            this.CreateMap<CampaignTemplate, CampaignTemplatesDb>()
              .ForMember(t => t.CampaignTemplateID, opt => opt.MapFrom(t => t.Id))
              .ForMember(t => t.ThumbnailImage, opt => opt.MapFrom(t => t.ThumbnailImageId));
           //   .ForMember(a => a.ThumbnailImage, opt => opt.MapFrom(a =>  Mapper.Map<Image, ImagesDb>(a.ThumbnailImage)));
              
              
              
            this.CreateMap<CampaignLink, CampaignLinksDb>()
                .ForMember(c => c.CampaignID, opt => opt.MapFrom(c => c.CampaignId))
                .ForMember(c => c.CampaignLinkID, opt => opt.MapFrom(c => c.CampaignLinkId))
                .ForMember(c => c.URL, opt => opt.MapFrom(c => c.URL != null ? c.URL.URL : null));


            this.CreateMap<CampaignTracker, CampaignTrackerDb>()
                .ForMember(c => c.CampaignID, opt => opt.MapFrom(c => c.CampaignId))
                .ForMember(c => c.ActivityDate, opt => opt.MapFrom(c => c.ActivityDate))
                .ForMember(c => c.CampaignLinkID, opt => opt.MapFrom(c => c.CampaignLinkId))
                .ForMember(c => c.CampaignRecipientId, opt => opt.MapFrom(c => c.CampaignRecipientId))
                .ForMember(c => c.CampaignTrackerID, opt => opt.MapFrom(c => c.CampaignTrackerId));


            this.CreateMap<Dropdown, DropdownDb>()
                .ForMember(a => a.DropdownID, opt => opt.MapFrom(d => d.Id))
                .ForMember(a => a.DropdownName, opt => opt.MapFrom(a => a.Name))
                .ForMember(a => a.DropdownValues, opt => opt.MapFrom(a => Mapper.Map<IEnumerable<DropdownValue>, IEnumerable<DropdownValueDb>>(a.DropdownValues)));

            this.CreateMap<DropdownValue, DropdownValueDb>()
                .ForMember(d => d.DropdownValueID, opt => opt.MapFrom(d => d.Id))
                .ForMember(d => d.DropdownValue, opt => opt.MapFrom(d => d.Value));

            this.CreateMap<DA.Action, ActionsDb>()
               .ForMember(p => p.ActionID, opt => opt.MapFrom(c => c.Id))
               .ForMember(p => p.RemindbyEmail, opt => opt.MapFrom(c => c.ReminderTypes.Contains(ReminderType.Email) ? new Nullable<bool>(true) : null))
               .ForMember(p => p.RemindbyPopup, opt => opt.MapFrom(c => c.ReminderTypes.Contains(ReminderType.PopUp) ? new Nullable<bool>(true) : null))
               .ForMember(p => p.RemindbyText, opt => opt.MapFrom(c => c.ReminderTypes.Contains(ReminderType.TextMessage) ? new Nullable<bool>(true) : null))
               .ForMember(p => p.ActionDetails, opt => opt.MapFrom(c => c.Details))
               .ForMember(p => p.RemindOn, opt => opt.MapFrom(c => c.RemindOn))// != null ? new Nullable<DateTime>(c.RemindOn) : null
               .ForMember(p => p.CreatedOn, opt => opt.MapFrom(c => c.CreatedOn))
               .ForMember(p => p.Contacts, opt => opt.Ignore())
               .ForMember(p => p.Tags, opt => opt.Ignore())
               .ForMember(p => p.User, opt => opt.Ignore())
               .ForMember(p => p.ActionContacts, opt => opt.Ignore())
               .ForMember(p => p.ActionTags, opt => opt.Ignore());
            // .ForMember(p => p.CreatedBy, opt => opt.MapFrom(c => int.Parse(c.User.Id)));

            this.CreateMap<Tag, TagsDb>()
                 .IgnoreAllUnmapped()
                 .ForMember(c => c.TagID, opt => opt.MapFrom(c => c.Id))
                 .ForMember(c => c.TagName, opt => opt.MapFrom(c => c.TagName))
                 .ForMember(c => c.CreatedBy, opt => opt.MapFrom(c => c.CreatedBy))
                 .ForMember(c => c.AccountID, opt => opt.MapFrom(c => c.AccountID));

            this.CreateMap<Address, AddressesDb>()
                .ForMember(a => a.AddressTypeID, opt => opt.MapFrom(a => a.AddressTypeID))
                .ForMember(a => a.State, opt => opt.Ignore())
                .ForMember(a => a.Country, opt => opt.Ignore())
                .ForMember(a => a.StateID, opt => opt.MapFrom(a => a.State.Code == "" ? null : a.State.Code))
                .ForMember(a => a.CountryID, opt => opt.MapFrom(a => a.Country.Code == "" ? null : a.Country.Code));

            this.CreateMap<Phone, ContactPhoneNumbersDb>()
               .ForMember(a => a.PhoneNumber, opt => opt.MapFrom(a => a.Number))
               .ForMember(a => a.PhoneType, opt => opt.MapFrom(a => a.PhoneType))
               .ForMember(a => a.ContactID, opt => opt.MapFrom(a => a.ContactID))
               .ForMember(a => a.AccountID, opt => opt.MapFrom(a => a.AccountID))
               .ForMember(a => a.IsPrimary, opt => opt.MapFrom(a => a.IsPrimary))
               .ForMember(a => a.ContactPhoneNumberID, opt => opt.MapFrom(a => a.ContactPhoneNumberID));

            this.CreateMap<Email, AccountEmailsDb>()
              .ForMember(a => a.EmailID, opt => opt.MapFrom(a => a.EmailID))
              .ForMember(a => a.UserID, opt => opt.MapFrom(a => a.UserID))
              .ForMember(a => a.AccountID, opt => opt.MapFrom(a => a.AccountID))
              .ForMember(a => a.Email, opt => opt.MapFrom(a => a.EmailId))
              .ForMember(a => a.IsPrimary, opt => opt.MapFrom(a => a.IsPrimary));


            this.CreateMap<State, StatesDb>()
                .ForMember(a => a.StateName, opt => opt.MapFrom(a => a.Name))
                .ForMember(a => a.StateID, opt => opt.MapFrom(a => a.Code));

            this.CreateMap<Country, CountriesDb>()
                .ForMember(a => a.CountryID, opt => opt.MapFrom(a => a.Code))
                .ForMember(a => a.CountryName, opt => opt.MapFrom(a => a.Name));

            this.CreateMap<Address, AddressesDb>();
            this.CreateMap<BulkOperations, BulkOperationsDb>();
            this.CreateMap<RawContact, ImportContactData>();
            this.CreateMap<SmartTouch.CRM.Domain.ImportData.ImportCustomData, SmartTouch.CRM.Repository.Database.ImportCustomData>();
            this.CreateMap<SmartTouch.CRM.Domain.ImportData.ImportPhoneData, SmartTouch.CRM.Repository.Database.ImportPhoneData>();
            this.CreateMap<SmartTouch.CRM.Domain.ImportData.ImportColumnMappings, ImportColumnMappingsDb>()
                .ForMember(f => f.ImportColumnMappingID, opt => opt.MapFrom(c => c.Id));

            this.CreateMap<SmartTouch.CRM.Domain.Forms.SubmittedFormData, SmartTouch.CRM.Repository.Database.SubmittedFormDataDb>();
            this.CreateMap<SmartTouch.CRM.Domain.Forms.SubmittedFormFieldData, SmartTouch.CRM.Repository.Database.SubmittedFormFieldDataDb>();

            

            this.CreateMap<RawContact, ImportContactsEmailStatusesDb>()
                 .ForMember(c => c.MailGunVerificationID, opt => opt.Ignore())
              .ForMember(a => a.ReferenceID, opt => opt.MapFrom(a => a.ReferenceId))
              .ForMember(a => a.EmailStatus, opt => opt.MapFrom(a => a.EmailStatus));

            this.CreateMap<Attachment, AttachmentsDb>()
                .ForMember(a => a.DocumentID, opt => opt.MapFrom(a => a.Id));

            this.CreateMap<Tour, TourDb>()
               .ForMember(t => t.TourID, opt => opt.MapFrom(t => t.Id))
               .ForMember(t => t.CreatedOn, opt => opt.MapFrom(c => c.CreatedOn))
               .ForMember(p => p.RemindbyEmail, opt => opt.MapFrom(c => c.ReminderTypes.Contains(ReminderType.Email) ? new Nullable<bool>(true) : null))
               .ForMember(p => p.RemindbyPopup, opt => opt.MapFrom(c => c.ReminderTypes.Contains(ReminderType.PopUp) ? new Nullable<bool>(true) : null))
               .ForMember(p => p.RemindbyText, opt => opt.MapFrom(c => c.ReminderTypes.Contains(ReminderType.TextMessage) ? new Nullable<bool>(true) : null))
               .ForMember(t => t.Contacts, opt => opt.Ignore())
               .ForMember(t => t.User, opt => opt.Ignore())
               .ForMember(t => t.TourContacts, opt => opt.Ignore())
               .ForMember(t => t.CreatedBy, opt => opt.MapFrom(t => int.Parse(t.User.Id)));

            this.CreateMap<Person, CommunicationsDb>()
                .ForMember(c => c.FacebookUrl, opt => opt.MapFrom(c => c.FacebookUrl.URL))
                .ForMember(c => c.TwitterUrl, opt => opt.MapFrom(c => c.TwitterUrl.URL))
                .ForMember(c => c.LinkedInUrl, opt => opt.MapFrom(c => c.LinkedInUrl.URL))
                .ForMember(c => c.GooglePlusUrl, opt => opt.MapFrom(c => c.GooglePlusUrl.URL))
                .ForMember(c => c.BlogUrl, opt => opt.MapFrom(c => c.BlogUrl.URL))
                .ForMember(c => c.WebSiteUrl, opt => opt.MapFrom(c => c.WebsiteUrl.URL));


            this.CreateMap<Company, CommunicationsDb>()
                .ForMember(c => c.FacebookUrl, opt => opt.MapFrom(c => c.FacebookUrl.URL))
                .ForMember(c => c.TwitterUrl, opt => opt.MapFrom(c => c.TwitterUrl.URL))
                .ForMember(c => c.LinkedInUrl, opt => opt.MapFrom(c => c.LinkedInUrl.URL))
                .ForMember(c => c.GooglePlusUrl, opt => opt.MapFrom(c => c.GooglePlusUrl.URL))
                .ForMember(c => c.BlogUrl, opt => opt.MapFrom(c => c.BlogUrl.URL))
                .ForMember(c => c.WebSiteUrl, opt => opt.MapFrom(c => c.WebsiteUrl.URL));

            this.CreateMap<Person, Communication>()
                .ForMember(c => c.FacebookUrl, opt => opt.MapFrom(c => c.FacebookUrl.URL))
                .ForMember(c => c.TwitterUrl, opt => opt.MapFrom(c => c.TwitterUrl.URL))
                .ForMember(c => c.LinkedInUrl, opt => opt.MapFrom(c => c.LinkedInUrl.URL))
                .ForMember(c => c.GooglePlusUrl, opt => opt.MapFrom(c => c.GooglePlusUrl.URL))
                .ForMember(c => c.BlogUrl, opt => opt.MapFrom(c => c.BlogUrl.URL))
                .ForMember(c => c.WebSiteUrl, opt => opt.MapFrom(c => c.WebsiteUrl.URL))
                .IgnoreAllUnmapped();


            this.CreateMap<Company, Communication>()
                .ForMember(c => c.FacebookUrl, opt => opt.MapFrom(c => c.FacebookUrl.URL))
                .ForMember(c => c.TwitterUrl, opt => opt.MapFrom(c => c.TwitterUrl.URL))
                .ForMember(c => c.LinkedInUrl, opt => opt.MapFrom(c => c.LinkedInUrl.URL))
                .ForMember(c => c.GooglePlusUrl, opt => opt.MapFrom(c => c.GooglePlusUrl.URL))
                .ForMember(c => c.BlogUrl, opt => opt.MapFrom(c => c.BlogUrl.URL))
                .ForMember(c => c.WebSiteUrl, opt => opt.MapFrom(c => c.WebsiteUrl.URL))
                .IgnoreAllUnmapped();

            this.CreateMap<ServiceProvider, ServiceProvidersDb>()

               .ForMember(c => c.EmailType, opt => opt.MapFrom(c => c.MailType))
            .ForMember(c => c.ServiceProviderID, opt => opt.MapFrom(c => c.Id))
               .ForMember(c => c.AccountID, opt => opt.MapFrom(c => c.AccountId));

            this.CreateMap<ImageDomain, ImageDomainsDb>()
                .ForMember(n => n.ImageDomainID, opt => opt.MapFrom(n => n.Id))
                .ForMember(n => n.ImageDomain, opt => opt.MapFrom(n => n.Domain));


            this.CreateMap<CampaignRecipientsDb, CampaignRecipient>()
                .ForMember(c => c.Id, opt => opt.MapFrom(c => c.CampaignRecipientID));

            this.CreateMap<Person, ContactsDb>()
                .ForMember(c => c.ContactID, opt => opt.MapFrom(c => c.Id))
                .ForMember(c => c.Company, opt => opt.MapFrom(c => c.CompanyName))
                .ForMember(c => c.ContactType, opt => opt.MapFrom(c => ContactType.Person))
                .ForMember(c => c.ContactImageUrl, opt => opt.MapFrom(c => c.ImageUrl))
                .ForMember(c => c.ProfileImageKey, opt => opt.MapFrom(c => c.ProfileImageKey))
                .ForMember(c => c.LifecycleStage, opt => opt.MapFrom(c => c.LifecycleStage))
                .ForMember(c => c.PartnerType, opt => opt.MapFrom(c => c.PartnerType))
                .ForMember(c => c.ContactPhones, opt => opt.Ignore())//(c => Mapper.Map<IEnumerable<Phone>, IEnumerable<ContactPhoneNumbersDb>>(c.Phones)))
                .ForMember(c => c.IsDeleted, opt => opt.Ignore())
                .ForMember(c => c.Addresses, opt => opt.Ignore())
                .ForMember(c => c.ContactPhones, opt => opt.Ignore())
                .ForMember(c => c.ContactLeadSources, opt => opt.Ignore())
                .ForMember(c => c.ContactEmails, opt => opt.Ignore())
                .ForMember(c => c.Tours, opt => opt.Ignore())
                .ForMember(c => c.Actions, opt => opt.Ignore())
                .ForMember(c => c.Notes, opt => opt.Ignore())
                .ForMember(c => c.CustomFields, opt => opt.Ignore())
                .ForMember(c => c.Communities, opt => opt.Ignore())
                .ForMember(c => c.OwnerID, opt => opt.MapFrom(c => c.OwnerId != null ? c.OwnerId == 0 ? null : c.OwnerId : null))
                .ForMember(c => c.Owner, opt => opt.Ignore())
                .AfterMap((s, d) =>
                {

                    //If new addresses added in UI while database has no addresses, create a new list for database.
                    if (d.Addresses == null && s.Addresses != null && s.Addresses.Any())
                        d.Addresses = new List<AddressesDb>();
                    //If there are no new addresses in both UI and database, just return.
                    else if (d.Addresses == null && (s.Addresses == null || !s.Addresses.Any()))
                        return;

                    var addressList = d.Addresses.ToList();
                    foreach (Address address in s.Addresses)
                    {
                        var addressDb = addressList.SingleOrDefault(a => a.AddressID == address.AddressID);

                        //If database address and UI address id match, the copy the UI address values to database object.
                        if (addressDb != null)
                        {
                            addressDb = Mapper.Map<Address, AddressesDb>(address, addressDb);
                        }
                        //If this is a new address, add this new address to database.
                        else
                        {
                            d.Addresses.Add(Mapper.Map<Address, AddressesDb>(address));
                        }
                    }
                });


            //this.CreateMap<DropdownValue, DropdownValueDb>()
            //    .ForMember(c => c.DropdownValue, opt => opt.MapFrom(c => c.Value))
            //    .ForMember(c => c.DropdownID, opt => opt.MapFrom(c => c.Id))
            //    .IgnoreAllUnmapped();
            this.CreateMap<ContactRelationship, ContactRelationshipDb>()
                .ForMember(c => c.ContactRelationshipMapID, opt => opt.MapFrom(c => c.Id))
                 .ForMember(c => c.RelationshipType, opt => opt.MapFrom(c => c.RelationshipTypeID));

            this.CreateMap<ContactEmailAudit, ContactEmailAuditDb>()
           .ForMember(c => c.ContactEmailAuditID, opt => opt.MapFrom(c => c.Id));

            this.CreateMap<ContactTextMessageAudit, ContactTextMessageAuditDb>()
                 .ForMember(c => c.ContactTextMessageAuditID, opt => opt.MapFrom(c => c.Id));

            this.CreateMap<Company, ContactsDb>()
                .ForMember(c => c.ContactID, opt => opt.MapFrom(c => c.Id))
                .ForMember(c => c.Company, opt => opt.MapFrom(c => c.CompanyName))
                .ForMember(c => c.ContactType, opt => opt.MapFrom(c => ContactType.Company))
                .ForMember(c => c.ContactImageUrl, opt => opt.MapFrom(c => c.ImageUrl))
                .ForMember(c => c.ProfileImageKey, opt => opt.MapFrom(c => c.ProfileImageKey))
                .ForMember(c => c.LifecycleStage, opt => opt.MapFrom(c => c.LifecycleStage == 0 ? null : (short?)null))
                .ForMember(c => c.OwnerID, opt => opt.MapFrom(c => c.OwnerId != null ? c.OwnerId == 0 ? null : c.OwnerId : null))
                 .ForMember(c => c.Owner, opt => opt.Ignore())
                .ForMember(c => c.IsDeleted, opt => opt.Ignore())
                .ForMember(c => c.Addresses, opt => opt.Ignore())
                .ForMember(c => c.ContactPhones, opt => opt.Ignore())
                .ForMember(c => c.ContactEmails, opt => opt.Ignore())
                .ForMember(c => c.Tours, opt => opt.Ignore())
                .ForMember(c => c.Actions, opt => opt.Ignore())
                .ForMember(c => c.Notes, opt => opt.Ignore())
                .ForMember(c => c.CustomFields, opt => opt.Ignore())
                .AfterMap((s, d) =>
                {
                    if (d.Addresses == null && s.Addresses != null && s.Addresses.Any())
                        d.Addresses = new List<AddressesDb>();
                    else if (d.Addresses == null && (s.Addresses == null || !s.Addresses.Any()))
                        return;
                    var addressList = d.Addresses.ToList();
                    foreach (Address address in s.Addresses)
                    {
                        var addressDb = addressList.SingleOrDefault(a => a.AddressID == address.AddressID);
                        if (addressDb != null)
                        {
                            addressDb = Mapper.Map<Address, AddressesDb>(address, addressDb);
                        }
                        else
                        {
                            d.Addresses.Add(Mapper.Map<Address, AddressesDb>(address));
                        }
                    }
                });
            this.CreateMap<Image, ImagesDb>()
                .ForMember(p => p.ImageID, opt => opt.MapFrom(c => c.Id))
                .ForMember(p => p.CreatedDate, opt => opt.MapFrom(c => DateTime.Now.ToUniversalTime()))
                .ForMember(p => p.CreatedBy, opt => opt.MapFrom(c => 1));
            //TODO:Remove 1 for CreatedBy

            this.CreateMap<CommunicationTracker, CommunicationTrackerDb>();
            this.CreateMap<CampaignLogDetails, CampaignLogDetailsDb>();
            this.CreateMap<Note, NotesDb>()
                .ForMember(p => p.NoteID, opt => opt.MapFrom(c => c.Id))
                .ForMember(p => p.NoteDetails, opt => opt.MapFrom(c => c.Details))
                .ForMember(c => c.Contacts, opt => opt.Ignore())
                .ForMember(c => c.Tags, opt => opt.Ignore())
                .ForMember(p => p.User, opt => opt.Ignore())
                .ForMember(p => p.CreatedBy, opt => opt.MapFrom(c => int.Parse(c.User.Id)));

            this.CreateMap<User, UsersDb>()
               .ForMember(c => c.UserID, opt => opt.MapFrom(c => c.Id))
               .ForMember(c => c.HomePhone, opt => opt.MapFrom(c => c.HomePhone.Number))
               .ForMember(c => c.WorkPhone, opt => opt.MapFrom(c => c.WorkPhone.Number))
               .ForMember(c => c.MobilePhone, opt => opt.MapFrom(c => c.MobilePhone.Number))
               .ForMember(c => c.PrimaryEmail, opt => opt.MapFrom(c => c.Email.EmailId))
               .ForMember(c => c.PrimaryPhoneType, opt => opt.MapFrom(c => c.PrimaryPhoneType))
               .ForMember(c => c.HasTourCompleted, opt => opt.MapFrom(c => c.HasTourCompleted.HasValue ? c.HasTourCompleted.Value : false))
               .ForMember(c => c.Addresses, opt => opt.Ignore())
               .ForMember(c => c.Emails, opt => opt.Ignore())
               .ForMember(c => c.Role, opt => opt.Ignore())
               .ForMember(c => c.RoleID, opt => opt.MapFrom(c => c.RoleID))
               .ForMember(p => p.Account, opt => opt.Ignore())

               .AfterMap((s, d) =>
               {
                   if (d.Addresses == null && s.Addresses != null && s.Addresses.Any())
                       d.Addresses = new List<AddressesDb>();
                   else if (d.Addresses == null && (s.Addresses == null || !s.Addresses.Any()))
                       return;

                   var addressList = d.Addresses.ToList();
                   foreach (Address address in s.Addresses)
                   {
                       var addressDb = addressList.SingleOrDefault(a => a.AddressID == address.AddressID);

                       if (addressDb != null)
                       {
                           addressDb = Mapper.Map<Address, AddressesDb>(address, addressDb);
                       }
                       else
                       {
                           d.Addresses.Add(Mapper.Map<Address, AddressesDb>(address));
                       }
                   }

               }).AfterMap((s, d) =>
               {
                   if (d.Emails == null && s.Emails != null && s.Emails.Any())
                       d.Emails = new List<AccountEmailsDb>();
                   else if (d.Emails == null && (s.Emails == null || !s.Emails.Any()))
                       return;

                   var emailsList = d.Emails.ToList();
                   foreach (Email email in s.Emails)
                   {
                       var emailsDb = emailsList.SingleOrDefault(a => a.EmailID == email.EmailID);

                       if (emailsDb != null)
                       {
                           emailsDb = Mapper.Map<Email, AccountEmailsDb>(email, emailsDb);
                       }
                       else
                       {
                           d.Emails.Add(Mapper.Map<Email, AccountEmailsDb>(email));
                       }
                   }

               });

            this.CreateMap<Account, AccountsDb>()
               .ForMember(c => c.AccountID, opt => opt.MapFrom(c => c.Id))
                .ForMember(c => c.HomePhone, opt => opt.MapFrom(c => c.HomePhone.Number))
                .ForMember(c => c.WorkPhone, opt => opt.MapFrom(c => c.WorkPhone.Number))
                .ForMember(c => c.MobilePhone, opt => opt.MapFrom(c => c.MobilePhone.Number))
               .ForMember(c => c.PrimaryEmail, opt => opt.MapFrom(c => c.Email.EmailId))
               .ForMember(c => c.SubscriptionID, opt => opt.MapFrom(c => c.SubscriptionID))
               .ForMember(c => c.TimeZone, opt => opt.MapFrom(c => c.TimeZone))
               .ForMember(c => c.Addresses, opt => opt.Ignore())
               .ForMember(c => c.Subscription, opt => opt.Ignore())
               .ForMember(c => c.SubscribedModules, opt => opt.Ignore())
               .ForMember(c => c.WebAnalyticsProvider, opt => opt.Ignore())
               .AfterMap((s, d) =>
               {
                   //If new addresses added in UI while database has no addresses, create a new list for database.
                   if (d.Addresses == null && s.Addresses != null && s.Addresses.Any())
                       d.Addresses = new List<AddressesDb>();
                   //If there are no new addresses in both UI and database, just return.
                   else if (d.Addresses == null && (s.Addresses == null || !s.Addresses.Any()))
                       return;

                   var addressList = d.Addresses.ToList();
                   foreach (Address address in s.Addresses)
                   {
                       var addressDb = addressList.SingleOrDefault(a => a.AddressID == address.AddressID);

                       //If database address and UI address id match, the copy the UI address values to database object.
                       if (addressDb != null)
                       {
                           addressDb = Mapper.Map<Address, AddressesDb>(address, addressDb);
                       }
                       //If this is a new address, add this new address to database.
                       else
                       {
                           d.Addresses.Add(Mapper.Map<Address, AddressesDb>(address));
                       }
                   }
               });





            this.CreateMap<WebAnalyticsProvider, WebAnalyticsProvidersDb>()
                .ForMember(c => c.WebAnalyticsProviderID, opt => opt.MapFrom(c => c.Id))
                .ForMember(c => c.LastUpdatedOn, opt => opt.MapFrom(c => DateTime.Now.ToUniversalTime()))
                .ForMember(c => c.LastAPICallTimeStamp, opt => opt.Ignore());

            this.CreateMap<WebVisit, WebVisitsDb>()
                .ForMember(w => w.ContactWebVisitID, opt => opt.Ignore())
                .ForMember(w => w.PageVisited, opt => opt.MapFrom(w => w.PageVisited))
                .ForMember(w => w.VisitedOn, opt => opt.MapFrom(w => w.VisitedOn))
                //.ForMember(w => w.ProviderVisitID, opt => opt.Ignore())
                .ForMember(w => w.Duration, opt => opt.MapFrom(w => w.Duration))
                .ForMember(w => w.Region, opt => opt.MapFrom(w => w.State))
                .ForMember(w => w.IPAddress, opt => opt.MapFrom(w => w.IPAddress));

            this.CreateMap<Account, CommunicationsDb>()
               .ForMember(c => c.FacebookUrl, opt => opt.MapFrom(c => c.FacebookUrl.URL))
               .ForMember(c => c.TwitterUrl, opt => opt.MapFrom(c => c.TwitterUrl.URL))
               .ForMember(c => c.LinkedInUrl, opt => opt.MapFrom(c => c.LinkedInUrl.URL))
               .ForMember(c => c.GooglePlusUrl, opt => opt.MapFrom(c => c.GooglePlusUrl.URL))
               .ForMember(c => c.BlogUrl, opt => opt.MapFrom(c => c.BlogUrl.URL))
               .ForMember(c => c.WebSiteUrl, opt => opt.MapFrom(c => c.WebsiteUrl.URL))
               .ForMember(c => c.SecondaryEmails, opt => opt.MapFrom(c => c.SecondaryEmails != null ? string.Join(",", c.SecondaryEmails.Select(e => e.EmailId).ToList()) : null));

            this.CreateMap<User, CommunicationsDb>()
             .ForMember(c => c.FacebookUrl, opt => opt.MapFrom(c => c.FacebookUrl.URL))
             .ForMember(c => c.TwitterUrl, opt => opt.MapFrom(c => c.TwitterUrl.URL))
             .ForMember(c => c.LinkedInUrl, opt => opt.MapFrom(c => c.LinkedInUrl.URL))
             .ForMember(c => c.GooglePlusUrl, opt => opt.MapFrom(c => c.GooglePlusUrl.URL))
             .ForMember(c => c.BlogUrl, opt => opt.MapFrom(c => c.BlogUrl.URL))
             .ForMember(c => c.WebSiteUrl, opt => opt.MapFrom(c => c.WebsiteUrl.URL))
             .ForMember(c => c.FacebookAccessToken, opt => opt.MapFrom(c => c.FacebookAccessToken))
             .ForMember(c => c.TwitterOAuthToken, opt => opt.MapFrom(c => c.TwitterOAuthToken))
             .ForMember(c => c.TwitterOAuthTokenSecret, opt => opt.MapFrom(c => c.TwitterOAuthTokenSecret))
             .ForMember(c => c.SecondaryEmails, opt => opt.MapFrom(c => c.SecondaryEmails != null ? string.Join(",", c.SecondaryEmails.Select(e => e.EmailId).ToList()) : null));


            this.CreateMap<User, AccountEmailsDb>()
              .ForMember(c => c.Email, opt => opt.MapFrom(c => c.Email.EmailId))
              .ForMember(c => c.IsPrimary, opt => opt.MapFrom(c => c.Email.IsPrimary))
              .ForMember(c => c.UserID, opt => opt.MapFrom(c => c.Id))
              .ForMember(c => c.AccountID, opt => opt.MapFrom(c => c.AccountID))
              .ForMember(c => c.EmailSignature, opt => opt.MapFrom(c => c.Email.EmailSignature));

            this.CreateMap<LeadScoreRule, LeadScoreRulesDb>()
                .ForMember(c => c.LeadScoreRuleID, opt => opt.MapFrom(c => c.Id))
                .ForMember(p => p.User, opt => opt.Ignore())
                .ForMember(c => c.ConditionID, opt => opt.MapFrom(c => (byte)c.ConditionID))
                .ForMember(c => c.IsActive, opt => opt.MapFrom(c => true))
                .ForMember(c => c.Condition, opt => opt.Ignore())
                .ForMember(c => c.SelectedCampaignLinks, opt => opt.Ignore())
                .AfterMap((s, d) =>
                {
                    if (s.SelectedCampaignLinkID != null && s.SelectedCampaignLinkID.Length > 0)
                    {
                        d.SelectedCampaignLinks = string.Join(",", s.SelectedCampaignLinkID);
                    }
                });

            this.CreateMap<LeadScore, LeadScoreDb>()
.ForMember(c => c.LeadScoreID, opt => opt.MapFrom(c => c.Id))
              .ForMember(c => c.AddedOn, opt => opt.MapFrom(c => DateTime.Now.ToUniversalTime()));

            this.CreateMap<Condition, ConditionDb>()
                .ForMember(c => c.ConditionID, opt => opt.MapFrom(c => c.Id))
                .ForMember(c => c.ScoreCategory, opt => opt.Ignore());

            this.CreateMap<ScoreCategories, ScoreCategoriesDb>()
               .ForMember(c => c.ScoreCategoryID, opt => opt.MapFrom(c => c.Id));

            this.CreateMap<LeadScoreConditionValue, LeadScoreConditionValuesDb>()
                .ForMember(c => c.LeadScoreConditionValueID, opt => opt.MapFrom(c => c.LeadScoreConditionValueId))
                .ForMember(c => c.LeadScoreRuleID, opt => opt.MapFrom(c => c.LeadScoreRuleId))
                .ForMember(d => d.ValueType, opt => opt.Ignore())
                .AfterMap((s, d) =>
                {
                    d.ValueType = (short)s.ValueType;
                });
            

            this.CreateMap<UserSettings, UserSettingsDb>()
             .ForMember(a => a.UserSettingID, opt => opt.MapFrom(a => a.Id));

            #region AdvancedSearch
            this.CreateMap<SearchDefinition, SearchDefinitionsDb>()
             .ForMember(a => a.SearchDefinitionID, opt => opt.MapFrom(a => a.Id))
             .ForMember(a => a.SearchDefinitionName, opt => opt.MapFrom(a => a.Name))
             .ForMember(a => a.IsFavoriteSearch, opt => opt.MapFrom(a => a.IsFavoriteSearch))
             .ForMember(a => a.IsPreConfiguredSearch, opt => opt.MapFrom(a => a.IsPreConfiguredSearch))
             .ForMember(a => a.SearchFilters, opt => opt.Ignore())  //MapFrom(c => Mapper.Map<IEnumerable<SearchFilter>, IEnumerable<SearchFiltersDb>>(c.Filters))
             .ForMember(a => a.SearchPredicateTypeID, opt => opt.MapFrom(a => (short)a.PredicateType));

            this.CreateMap<SearchFilter, SearchFiltersDb>()
            .ForMember(a => a.SearchFilterID, opt => opt.MapFrom(a => a.SearchFilterId))
                //.ForMember(a => a.DropdownValueID, opt => opt.MapFrom(a => a.DropdownValueId))
            .ForMember(a => a.SearchQualifierTypeID, opt => opt.MapFrom(a => (short)a.Qualifier))
                //.ForMember(a => a.FieldID, opt => opt.MapFrom(a => a.IsDropdownField == false ? (int)a.Field : null))
            .ForMember(a => a.IsCustomField, opt => opt.MapFrom(a => a.IsCustomField))
            .ForMember(a => a.IsDropdownField, opt => opt.MapFrom(a => a.IsDropdownField))
            .ForMember(a => a.IsDateTime, opt => opt.MapFrom(a => a.IsDateTime))
            .AfterMap((s, d) =>
            {
                if (s.IsDropdownField)
                {
                    d.FieldID = null;
                    short dropdownvalue = 0;
                    short.TryParse(d.FieldID.Value.ToString(), out dropdownvalue);
                    d.DropdownValueID = dropdownvalue;
                    d.DropdownId = s.DropdownId;
                }
                else
                {
                    d.FieldID = (int)s.Field;
                }
            });

            #endregion

            this.CreateMap<Form, FormsDb>()
               .ForMember(a => a.FormID, opt => opt.MapFrom(a => a.Id))
               .ForMember(a => a.AcknowledgementType, opt => opt.MapFrom(a => a.AcknowledgementType))
               .ForMember(a => a.Status, opt => opt.MapFrom(a => (short)a.Status))
               .ForMember(a => a.FormTags, opt => opt.Ignore())
               .ForMember(a => a.LastModifiedOn, opt => opt.MapFrom(a => DateTime.Now.ToUniversalTime()))
               .ForMember(a => a.FormFields, opt => opt.Ignore())
               .ForMember(a => a.CreatedOn, opt => opt.Ignore())
               .ForMember(a => a.CreatedBy, opt => opt.Ignore())
               .ForMember(a => a.LastModifiedBy, opt => opt.MapFrom(a => a.LastModifiedBy))
               .ForMember(a => a.AccountID, opt => opt.MapFrom(a => a.AccountID))
               .ForMember(a => a.IsDeleted, opt => opt.MapFrom(a => false))
               .ForMember(a => a.Submissions, opt => opt.Ignore())
               .ForMember(a => a.LeadSource, opt => opt.MapFrom(a => a.LeadSource))
               .AfterMap((s, d) =>
               {
                   if (s.Id == 0)
                   {
                       d.CreatedOn = DateTime.Now.ToUniversalTime();
                       d.CreatedBy = s.CreatedBy;
                   }
                   if (d.FormFields != null && d.FormFields.Count != 0)
                   {
                       foreach (FormField formField in s.FormFields)
                       {
                           var formFieldDb = d.FormFields.SingleOrDefault(cfs => cfs.FormFieldID == formField.FormFieldId);
                           Mapper.Map<FormField, FormFieldsDb>(formField, formFieldDb);
                       }
                   }
                   else
                   {
                       IEnumerable<FormFieldsDb> dataBase = Mapper.Map<IEnumerable<FormField>, IEnumerable<FormFieldsDb>>(s.FormFields);
                       d.FormFields = dataBase.ToList();
                   }
               });


            this.CreateMap<FormSubmission, FormSubmissionDb>()
                .ForMember(f => f.ContactID, opt => opt.MapFrom(f => f.ContactId))
                .ForMember(f => f.FormID, opt => opt.MapFrom(f => f.FormId))
                .ForMember(f => f.FormSubmissionID, opt => opt.MapFrom(f => f.Id))
                .ForMember(f => f.StatusID, opt => opt.MapFrom(f => (short)f.StatusID))
                .ForMember(f => f.SubmittedOn, opt => opt.MapFrom(f => f.SubmittedOn));

            this.CreateMap<FormField, FormFieldsDb>()
               .ForMember(c => c.FieldID, opt => opt.MapFrom(c => c.Id))
               .ForMember(c => c.FormFieldID, opt => opt.MapFrom(c => c.FormFieldId))
               .ForMember(c => c.FormID, opt => opt.MapFrom(c => c.FormId))
               .ForMember(c => c.SortID, opt => opt.MapFrom(c => c.SortId));


            this.CreateMap<Field, FieldsDb>()
                .ForMember(a => a.FieldID, opt => opt.MapFrom(a => a.Id))
                .ForMember(a => a.FieldInputTypeID, opt => opt.MapFrom(a => a.FieldInputTypeId))
                .ForMember(a => a.StatusID, opt => opt.MapFrom(a => a.StatusId))
                .ForMember(a => a.CustomFieldValueOptions, opt => opt.MapFrom(a => a.ValueOptions))
                .ForMember(a => a.ValidationMessage, opt => opt.MapFrom(a => a.ValidationMessage))
                .IgnoreAllUnmapped();

            //this.CreateMap<CustomField, FieldsDb>()
            //    .ForMember(a => a.FieldID, opt => opt.MapFrom(a => a.Id))
            //    .ForMember(a => a.FieldInputTypeID, opt => opt.MapFrom(a => a.FieldInputTypeId))
            //    .ForMember(a => a.StatusID, opt => opt.MapFrom(a => a.StatusId))
            //    .ForMember(a => a.CustomFieldValueOptions, opt => opt.MapFrom(a => a.ValueOptions))
            //    .ForMember(a => a.CustomFieldSectionID, opt => opt.MapFrom(a => a.SectionId))
            //    .ForMember(a => a.SortID, opt => opt.MapFrom(a => a.SortId))
            //    .ForMember(a => a.ValidationMessage, opt => opt.MapFrom(a => a.ValidationMessage))
            //    .ForMember(a => a.CustomFieldSection, opt => opt.Ignore());

            this.CreateMap<CustomField, FieldsDb>()
                .ForMember(c => c.FieldID, opt => opt.MapFrom(c => c.Id))
                .ForMember(c => c.AccountID, opt => opt.MapFrom(c => c.AccountID))
                .ForMember(c => c.StatusID, opt => opt.MapFrom(c => c.StatusId))
                .ForMember(c => c.SortID, opt => opt.MapFrom(c => c.SortId))
                .ForMember(c => c.CustomFieldSectionID, opt => opt.MapFrom(c => c.SectionId))
                .ForMember(c => c.CustomFieldValueOptions, opt => opt.Ignore())
                .ForMember(c => c.FieldCode, opt => opt.MapFrom(c => ""))
                .ForMember(c => c.FieldInputTypeID, opt => opt.MapFrom(c => c.FieldInputTypeId))
                 .AfterMap((s, d) =>
                 {
                     if (d.CustomFieldValueOptions != null && d.CustomFieldValueOptions.Count > 0)
                     {
                         //Remove deleted value options

                         foreach (var fieldValue in d.CustomFieldValueOptions)
                         {
                             if (s.Id > 0)
                             {
                                 var fieldValueOption = s.ValueOptions.SingleOrDefault(cfv => cfv.Id == fieldValue.CustomFieldValueOptionID);
                                 fieldValue.IsDeleted = fieldValueOption == null ? true : false;
                             }
                         }

                         //Add new or edit existing value options
                         var newValueOptions = new List<CustomFieldValueOptionsDb>();
                         newValueOptions.AddRange(d.CustomFieldValueOptions);
                         foreach (var fieldValue in s.ValueOptions)
                         {
                             var fieldValueOptionDb = new CustomFieldValueOptionsDb();
                             if (s.Id > 0)
                             {
                                 fieldValueOptionDb = newValueOptions.SingleOrDefault(cfv => cfv.CustomFieldValueOptionID == fieldValue.Id);
                             }

                             if (fieldValueOptionDb != null)
                                 Mapper.Map(fieldValue, fieldValueOptionDb);
                             else
                                 d.CustomFieldValueOptions.Add(Mapper.Map<FieldValueOption, CustomFieldValueOptionsDb>(fieldValue));
                         }
                     }
                     else
                     {
                         ICollection<CustomFieldValueOptionsDb> dataBase =
                             Mapper.Map<IEnumerable<FieldValueOption>, ICollection<CustomFieldValueOptionsDb>>(s.ValueOptions).ToList();

                         d.CustomFieldValueOptions = dataBase.ToList();
                     }
                 });
            //this.CreateMap<AcknowledgementType,>()
            //    .ForMember(a => a.AddressTypeID, opt => opt.MapFrom(a => a.AddressType))
            //    .ForMember(a => a.State, opt => opt.Ignore())
            //    .ForMember(a => a.Country, opt => opt.Ignore())
            //    .ForMember(a => a.StateID, opt => opt.MapFrom(a => a.State.Code))
            //    .ForMember(a => a.CountryID, opt => opt.MapFrom(a => a.Country.Code));

            this.CreateMap<LeadAdapterAndAccountMap, LeadAdapterAndAccountMapDb>()
                 .ForMember(p => p.LeadAdapterAndAccountMapId, opt => opt.MapFrom(c => c.Id))
                 .ForMember(p => p.Tags, opt => opt.Ignore());

            this.CreateMap<FacebookLeadAdapter, FacebookLeadAdapterDb>()
                .ForMember(p => p.FacebookLeadAdapterID, opt => opt.MapFrom(c => c.Id));

            this.CreateMap<FacebookLeadGen, FacebookLeadGenDb>();


            this.CreateMap<LeadAdapterJobLogDetails, LeadAdapterJobLogDetailsDb>()
             .ForMember(p => p.LeadAdapterJobLogDetailID, opt => opt.MapFrom(c => c.Id));
            this.CreateMap<LeadAdapterJobLogs, LeadAdapterJobLogsDb>()
             .ForMember(p => p.LeadAdapterJobLogID, opt => opt.MapFrom(c => c.Id));

            this.CreateMap<Notification, NotificationDb>()
                .ForMember(n => n.EntityID, opt => opt.MapFrom(nt => nt.EntityId))
            .ForMember(n => n.NotificationTime, opt => opt.MapFrom(nt => nt.Time));

            this.CreateMap<CustomFieldTab, CustomFieldTabDb>()
                .ForMember(c => c.CustomFieldTabID, opt => opt.MapFrom(c => c.Id))
                .ForMember(c => c.AccountID, opt => opt.MapFrom(c => c.AccountId))
                .ForMember(c => c.StatusID, opt => opt.MapFrom(c => (short)c.StatusId))
                .ForMember(c => c.SortID, opt => opt.MapFrom(c => c.SortId))
                .ForMember(c => c.CustomFieldSections, opt => opt.Ignore())
                .AfterMap((s, d) =>
                {
                    if (d.CustomFieldSections != null && d.CustomFieldSections.Count != 0)
                    {
                        foreach (CustomFieldSection customFieldSection in s.Sections)
                        {

                            if (customFieldSection.Id > 0)
                            {
                                var customFieldSectionDb = d.CustomFieldSections.SingleOrDefault(cfs => cfs.CustomFieldSectionID == customFieldSection.Id);
                                Mapper.Map<CustomFieldSection, CustomFieldSectionDb>(customFieldSection, customFieldSectionDb);
                            }
                            else
                            {
                                d.CustomFieldSections.Add(Mapper.Map<CustomFieldSection, CustomFieldSectionDb>(customFieldSection));
                            }
                        }
                    }
                    else
                    {
                        IEnumerable<CustomFieldSectionDb> dataBase = Mapper.Map<IEnumerable<CustomFieldSection>, IEnumerable<CustomFieldSectionDb>>(s.Sections);
                        d.CustomFieldSections = dataBase.ToList();
                    }
                });

            this.CreateMap<CustomFieldSection, CustomFieldSectionDb>()
                .ForMember(s => s.CustomFieldSectionID, opt => opt.MapFrom(s => s.Id))
                .ForMember(c => c.StatusID, opt => opt.MapFrom(c => c.StatusId))
                .ForMember(c => c.SortID, opt => opt.MapFrom(c => c.SortId))
                .ForMember(c => c.TabID, opt => opt.MapFrom(c => c.TabId))
                .ForMember(c => c.CustomFields, opt => opt.Ignore())
                 .AfterMap((s, d) =>
                 {
                     if (d.CustomFields != null && d.CustomFields.Count != 0)
                     {
                        
                         foreach (CustomField field in s.CustomFields)
                         {
                             if (field.Id > 0)
                             {
                                 var customFieldDb = d.CustomFields.SingleOrDefault(cf => cf.FieldID == field.Id);
                                 Mapper.Map<CustomField, FieldsDb>(field, customFieldDb);
                             }
                             
                             else
                             {
                                 d.CustomFields.Add(Mapper.Map<CustomField, FieldsDb>(field));
                             }
                         }
                     }
                     else
                     {
                         IList<FieldsDb> fieldDbs = new List<FieldsDb>();
                         foreach (var customField in s.CustomFields)
                         {
                             fieldDbs.Add(Mapper.Map<CustomField, FieldsDb>(customField));
                         }
                         d.CustomFields = fieldDbs;
                     }
                 });

            this.CreateMap<Field, FieldsDb>()
                .ForMember(c => c.FieldID, opt => opt.MapFrom(c => c.Id))
                .ForMember(c => c.StatusID, opt => opt.MapFrom(c => c.StatusId))
                .ForMember(c => c.CustomFieldValueOptions, opt => opt.Ignore())
                .ForMember(c => c.FieldCode, opt => opt.MapFrom(c => ""))
                .ForMember(c => c.FieldInputTypeID, opt => opt.MapFrom(c => c.FieldInputTypeId))
                 .AfterMap((s, d) =>
                 {
                     if (d.CustomFieldValueOptions != null && d.CustomFieldValueOptions.Count != 0)
                     {
                         foreach (FieldValueOption valueOption in s.ValueOptions)
                         {
                             var fieldValueOptionDb = d.CustomFieldValueOptions.SingleOrDefault(cfv => cfv.CustomFieldValueOptionID == valueOption.Id);
                             if (fieldValueOptionDb != null)
                                 Mapper.Map<FieldValueOption, CustomFieldValueOptionsDb>(valueOption, fieldValueOptionDb);
                             else
                             {
                                 d.CustomFieldValueOptions.Add(Mapper.Map<FieldValueOption, CustomFieldValueOptionsDb>(valueOption));
                             }
                         }
                     }
                     else
                     {
                         IList<CustomFieldValueOptionsDb> valueOptions = new List<CustomFieldValueOptionsDb>();
                         foreach (var customField in s.ValueOptions)
                         {
                             valueOptions.Add(Mapper.Map<FieldValueOption, CustomFieldValueOptionsDb>(customField));
                         }
                         d.CustomFieldValueOptions = valueOptions;
                     }
                 });

            this.CreateMap<FieldValueOption, CustomFieldValueOptionsDb>()
                .ForMember(c => c.CustomFieldValueOptionID, opt => opt.MapFrom(c => c.Id))
                .ForMember(c => c.CustomFieldID, opt => opt.MapFrom(c => c.FieldId));

            this.CreateMap<ContactCustomField, ContactCustomFieldsDb>()
                .ForMember(c => c.ContactCustomFieldMapID, opt => opt.MapFrom(c => c.ContactCustomFieldMapId))
                .ForMember(c => c.ContactID, opt => opt.MapFrom(c => c.ContactId))
                .ForMember(c => c.CustomFieldID, opt => opt.MapFrom(c => c.CustomFieldId))
                .ForMember(c => c.FieldInputTypeId, opt => opt.MapFrom(c => c.FieldInputTypeId));


            #region Workflow to WorkflowsDb

            this.CreateMap<Workflow, WorkflowsDb>()
                  .ForMember(c => c.RemovedWorkflows, opt => opt.MapFrom(c => c.RemovefromWorkflows))
                  .ForMember(c => c.Status, opt => opt.MapFrom(c => c.StatusID))
                  .ForMember(c => c.DeactivatedOn, opt => opt.Ignore())
                  .AfterMap((s, d) =>
                  {
                      if (s.DeactivatedOn.HasValue)
                          d.DeactivatedOn = s.DeactivatedOn.Value.ToLocalTime().Date;
                  });

            this.CreateMap<BaseWorkflowAction, BaseWorkflowActionsDb>()
                .Include<WorkflowLeadScoreAction, WorkflowLeadScoreActionsDb>()
                .Include<WorkflowCampaignAction, WorkflowCampaignActionsDb>()
                .Include<WorkflowTagAction, WorkflowTagActionsDb>()
                .Include<WorkflowLifeCycleAction, WorkflowLifeCycleActionsDb>()
                .Include<WorkflowUserAssignmentAction, WorkFlowUserAssignmentActionsDb>()
                .Include<WorkflowNotifyUserAction, WorkflowNotifyUserActionsDb>()
                .Include<WorkflowContactFieldAction, WorkflowContactFieldActionsDb>()
                .Include<WorkflowTextNotificationAction, WorkFlowTextNotificationActionsDb>()
                .Include<WorkflowTimerAction, WorkflowTimerActionsDb>()
                .Include<WorkflowEmailNotificationAction, WorkflowEmailNotificationActionDb>()
                .Include<TriggerWorkflowAction, TriggerWorkflowActionsDb>();

            this.CreateMap<WorkflowAction, WorkflowActionsDb>()
                // .ForMember(p=>p.WorkflowActionTypes, opt=>opt.Ignore)
                .AfterMap((s, d) =>
                    {
                        d.Action = Mapper.Map<BaseWorkflowActionsDb>(s.Action);
                    });
            this.CreateMap<TriggerWorkflowAction, TriggerWorkflowActionsDb>();
            this.CreateMap<WorkflowCampaignAction, WorkflowCampaignActionsDb>()
                .AfterMap((s, d) => 
                {
                    d.WorkflowActionTypeID = s.WorkflowActionTypeID;
                    d.Links = Mapper.Map<IEnumerable<WorkflowCampaignActionLink>, IEnumerable<WorkflowCampaignActionLinksDb>>(s.Links).ToList();
                });
            this.CreateMap<WorkflowCampaignActionLink, WorkflowCampaignActionLinksDb>()
                .AfterMap((s, d) => 
                {
                    d.LinkActions = Mapper.Map<IEnumerable<WorkflowAction>, IEnumerable<WorkflowActionsDb>>(s.Actions);
                });
            this.CreateMap<WorkflowLeadScoreAction, WorkflowLeadScoreActionsDb>();
            this.CreateMap<WorkflowEmailNotificationAction, WorkflowEmailNotificationActionDb>();
            this.CreateMap<WorkflowTagAction, WorkflowTagActionsDb>();
            this.CreateMap<WorkflowLifeCycleAction, WorkflowLifeCycleActionsDb>();
            this.CreateMap<RoundRobinContactAssignment, RoundRobinContactAssignmentDb>();
            this.CreateMap<WorkflowUserAssignmentAction, WorkFlowUserAssignmentActionsDb>();
            this.CreateMap<WorkflowNotifyUserAction, WorkflowNotifyUserActionsDb>()
                  .AfterMap((s, d) => 
                  {
                      if (s.UserID != null && s.UserID.Any())
                      {
                          d.UserID = string.Join(",", s.UserID);
                      }
                      else
                      {
                          d.UserID = string.Empty;
                      }
                          
                  })
                  .AfterMap((s, d) =>
                  {
                      if (s.NotificationFieldID != null && s.NotificationFieldID.Any())
                      {
                          d.NotificationFields = string.Join(",", s.NotificationFieldID);
                      }
                      else
                      {
                          d.NotificationFields = string.Empty;
                      }
                          
                  });
            this.CreateMap<WorkflowContactFieldAction, WorkflowContactFieldActionsDb>()
                .ForMember(s => s.FieldID, opt => opt.Ignore())
                .AfterMap((s, d) =>
                {
                    if (s.IsDropdownField && s.FieldID != 0)
                    {
                        d.DropdownValueID = (short)s.FieldID;
                        d.FieldID = null;
                    }
                    else 
                    {
                        d.FieldID = s.FieldID;
                        d.DropdownValueID = null;
                    }
                    
                });
            this.CreateMap<WorkflowTextNotificationAction, WorkFlowTextNotificationActionsDb>();
            this.CreateMap<WorkflowTrigger, WorkflowTriggersDb>()
                  .ForMember(x => x.SelectedLinks, opt => opt.Ignore())
                  .ForMember(x => x.DurationOperator, opt => opt.MapFrom(m => m.Operator))
                  .ForMember(x => x.WebPage, opt => opt.Ignore())
                  .AfterMap((s, d) =>
                    {
                        if (s.SelectedLinks != null && s.SelectedLinks.Any())
                            d.SelectedLinks = string.Join(",", s.SelectedLinks);
                        d.WebPage = s.IsAnyWebPage ? null : s.WebPage;
                    });

            this.CreateMap<WorkflowTriggerType, WorkflowTriggerTypesDb>();
            this.CreateMap<WorkflowTimerAction, WorkflowTimerActionsDb>()
                  .AfterMap((s, d) =>
                  {
                      StringBuilder sb = new StringBuilder();

                      if (s.DaysOfWeek != null && s.DaysOfWeek.Any())
                      {
                          s.DaysOfWeek.Each(a =>
                          {
                              sb.Append((byte)a);
                              sb.Append(',');
                          });
                          sb.Remove(sb.Length - 1, 1);
                      }
                      d.DaysOfWeek = sb.ToString();
                  });
            #endregion

            #region Opportunity to OpportunityDb

            this.CreateMap<Opportunity, OpportunitiesDb>()
           .ForMember(a => a.OpportunityID, opt => opt.MapFrom(a => a.Id))
           .ForMember(a => a.Owner, opt => opt.MapFrom(a => a.OwnerId))
           .ForMember(p => p.ContactsMap, opt => opt.Ignore())
           .ForMember(p => p.OpportunityTags, opt => opt.Ignore());


            #endregion

            this.CreateMap<Subscription, SubscriptionsDb>()
                .ForMember(c => c.SubscriptionID, opt => opt.MapFrom(c => c.Id))
                .ForMember(c => c.SubscriptionName, opt => opt.MapFrom(c => c.SubscriptionName));

            this.CreateMap<WebAnalyticsProvider, WebAnalyticsProvidersDb>()
                .ForMember(w => w.WebAnalyticsProviderID, opt => opt.MapFrom(w => w.Id));
            this.CreateMap<UserSocialMediaPosts, UserSocialMediaPostsDb>();

            this.CreateMap<CRMOutlookSync, CRMOutlookSyncDb>()
                .ForMember(o => o.OutlookSyncID, opt => opt.MapFrom(o => o.OutlookSyncId))
                .ForMember(o => o.SyncStatus, opt => opt.MapFrom(o => (short)o.SyncStatus))
                .IgnoreAllPropertiesWithAnInaccessibleSetter();

            this.CreateMap<ThirdPartyClient, ThirdPartyClientsDb>()
                .ForMember(o => o.ID, opt => opt.MapFrom(o => o.ID));

            this.CreateMap<ClientRefreshToken, ClientRefreshTokensDb>();

            //this.CreateMap<ContactOutlookSync, ContactOutlookSyncDb>()
            //    .ForMember(o => o.ContactOutlookSyncID, opt => opt.MapFrom(o => o.ContactOutlookSyncId))
            //    .ForMember(o => o.SyncStatus, opt => opt.MapFrom(o => (short)o.SyncStatus));

            this.CreateMap<SeedEmail, SeedEmailDb>()
                    .ForMember(o => o.SeedID, opt => opt.MapFrom(o => o.Id))
                    .ForMember(o => o.Email, opt => opt.MapFrom(o => o.Email));
            this.CreateMap<AccountSettings, AccountSettingsDb>()
                .ForMember(o => o.AccountSettingsID, opt => opt.MapFrom(o => o.AccountSettingsID));
            this.CreateMap<LeadScoreMessage, LeadScoreMessageDb>();
            this.CreateMap<TrackMessage, LeadScoreMessageDb>();
            this.CreateMap<TrackMessage, TrackMessagesDb>();
            this.CreateMap<TrackAction, TrackActionsDb>();

            this.CreateMap<MarketingMessage, MarketingMessagesDb>();

            this.CreateMap<MarketingMessageAccountMap, MarketingMessageAccountMapDb>();

            this.CreateMap<MarketingMessageContentMap, MarketingMessageContentMapDb>();
            this.CreateMap<TemporaryRecipient, MomentaryCampaignRecipientsDb>()
                .ForMember(o => o.MomentaryRecipientID, opt => opt.MapFrom(o => o.TemporaryRecipientId));
            #region SuppressionList
            this.CreateMap<SuppressedEmail, SuppressedEmailsDb > ()
                .ForMember(f => f.SuppressedEmailID, opt => opt.MapFrom(f => f.Id));
            this.CreateMap<SuppressedDomain,SuppressedDomainsDb>()
                .ForMember(f => f.SuppressedDomainID, opt => opt.MapFrom(f => f.Id));
            #endregion
            #region SavedSearchContacts
            this.CreateMap<SmartSearchContact, SmartSearchContactsDb>();
            #endregion
            #region For Action Mail Send
            this.CreateMap<DA.ActionsMailOperation, ActionsMailOperationDb>();
            #endregion

            this.CreateMap<Communication, CommunicationsDb>()
                .ForMember(f => f.CommunicationID, opt => opt.MapFrom(m => m.Id));
        }
    }
}
