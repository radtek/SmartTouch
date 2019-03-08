using AutoMapper;
using Newtonsoft.Json.Linq;
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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DA = SmartTouch.CRM.Domain.Actions;
using System.Text;
using LandmarkIT.Enterprise.Extensions;
using SmartTouch.CRM.Domain.Enterprises;
using SmartTouch.CRM.Domain.SuppressedEmails;

namespace SmartTouch.CRM.ApplicationServices.ObjectMappers
{
    public class EntityToViewModelProfile : Profile
    {
        public new string ProfileName
        {
            get
            {
                return "ViewModelToEntityProfile";
            }
        }

        protected override void Configure()
        {

            this.CreateMap<Campaign, CampaignViewModel>()
                .ForMember(c => c.Contacts, opt => opt.MapFrom(c => Mapper.Map<IList<Contact>, IList<ContactEntry>>(c.Contacts)))
                .ForMember(c => c.CampaignID, opt => opt.MapFrom(c => c.Id))
                .ForMember(c => c.CampaignTemplate, opt => opt.MapFrom(c => c.Template))
                .ForMember(c => c.SSContactsStatus, opt => opt.MapFrom(c => c.SSRecipients))
                .ForMember(c => c.ToTagStatus, opt => opt.MapFrom(c => c.TagRecipients))
                .ForMember(c => c.SearchDefinitions, opt => opt.MapFrom(c => Mapper.Map<IList<SearchDefinition>, IList<AdvancedSearchViewModel>>(c.SearchDefinitions)))
                 .ForMember(c => c.Links, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<CampaignLink>, IEnumerable<CampaignLinkViewModel>>(c.Links)));


            this.CreateMap<CampaignTheme, CampaignThemeViewModel>()
                .ForMember(t => t.CampaignThemeID, opt => opt.MapFrom(c => c.CampaignThemeID));

            this.CreateMap<CampaignLink, CampaignLinkViewModel>()
                .ForMember(c => c.CampaignId, opt => opt.MapFrom(c => c.CampaignId))
                .ForMember(c => c.URL, opt => opt.Ignore())
                .AfterMap((s, d) =>
                {
                    d.URL = new Url();
                    d.URL = s.URL;
                }); ;

            this.CreateMap<SubmittedFormData, SubmittedFormViewModel>()
            .ForMember(p => p.AccountId, opt => opt.MapFrom(c => c.AccountID))
            .ForMember(p => p.SubmittedOn, opt => opt.MapFrom(c => c.CreatedOn))
            .ForMember(p => p.FormId, opt => opt.MapFrom(c => c.FormID))
            .ForMember(p => p.IPAddress, opt => opt.MapFrom(c => c.IPAddress))
            .ForMember(p => p.LeadSourceID, opt => opt.MapFrom(c => c.LeadSourceID))
            .ForMember(p => p.Status, opt => opt.MapFrom(c => (SubmittedFormStatus)c.Status))
            .ForMember(p => p.OwnerId, opt => opt.MapFrom(c => c.OwnerID))
            .ForMember(p => p.CreatedBy, opt => opt.MapFrom(c => c.CreatedBy))
            .ForMember(p => p.STITrackingID, opt => opt.MapFrom(c => c.STITrackingID))
            .ForMember(p => p.SubmittedFormDataID, opt => opt.MapFrom(c => c.SubmittedFormDataID));


            this.CreateMap<SubmittedFormFieldData, SubmittedFormFieldViewModel>()
              .ForMember(s => s.Key, opt => opt.MapFrom(c => c.Field))
              .ForMember(s => s.Value, opt => opt.MapFrom(c => c.Value));


            this.CreateMap<CampaignStatistics, CampaignStatisticsViewModel>()
                .ForMember(c => c.CampaignId, opt => opt.MapFrom(c => c.CampaignId))
                .ForMember(c => c.Opens, opt => opt.MapFrom(c => c.Opens))
                .ForMember(c => c.SentOn, opt => opt.MapFrom(c => c.SentOn))
                .ForMember(c => c.CampaignViewModel, opt => opt.MapFrom(c => Mapper.Map<Campaign, CampaignViewModel>(c.Campaign)));
            this.CreateMap<CampaignReportData, CampaignEntryViewModel>()
               .ForMember(c => c.Id, opt => opt.MapFrom(c => c.CampaignID))
               .ForMember(c => c.ComplaintRate, opt => opt.MapFrom(c => c.TotalSends == 0 ? "0 | 0" : c.TotalCompliants + "|" + Math.Round(((float)c.TotalCompliants / (float)c.TotalSends) * 100) + "%"))
               .ForMember(c => c.OpenRate, opt => opt.MapFrom(c => c.TotalDelivered == 0 ? "0 | 0" : c.TotalOpens + "|" + Math.Round(((float)c.TotalOpens / (float)c.TotalDelivered) * 100) + "%"))
               .ForMember(c => c.ClickRate, opt => opt.MapFrom(c => c.TotalDelivered == 0 ? "0 | 0" : c.TotalClicks + "|" + Math.Round(((float)c.TotalClicks / (float)c.TotalDelivered) * 100) + "%"))
                  .ForMember(c => c.ClickCount, opt => opt.MapFrom(c => c.TotalClicks));

            this.CreateMap<Contact, ContactEntry>()
                .IgnoreAllUnmapped()
                .AfterMap((s, d) =>
                {
                    if (s.GetType().Equals(typeof(Person)))
                    {
                        Person person = s as Person;
                        if (s.Emails != null)
                        {
                            var email = s.Emails.FirstOrDefault(c => c.IsPrimary);
                            d.Email = email;
                        }
                        d.FullName = person.FirstName + " " + person.LastName;

                        if (!string.IsNullOrEmpty(person.CompanyName))
                            d.FullName = d.FullName + ": " + person.CompanyName;

                        if (string.IsNullOrEmpty(d.FullName.Trim()) && d.Email != null)
                            d.FullName = d.Email.EmailId;

                        d.ContactType = (int)ContactType.Person;
                    }
                    else
                    {
                        if (s.Emails != null)
                        {
                            var email = s.Emails.FirstOrDefault(c => c.IsPrimary);
                            d.Email = email;
                        }
                        d.FullName = s.CompanyName;

                        if (string.IsNullOrEmpty(d.FullName.Trim()) && d.Email != null)
                            d.FullName = d.Email.EmailId;
                        d.ContactType = (int)ContactType.Company;
                    }
                    d.Id = s.Id;
                });


            this.CreateMap<RawContact, ContactEntry>()
              .IgnoreAllUnmapped()
              .AfterMap((s, d) =>
              {
                  if (s.ContactType == ContactType.Person)
                  {

                      if (s.Emails != null)
                      {
                          var email = s.Emails.FirstOrDefault(c => c.IsPrimary);
                          d.Email = email;
                      }
                      d.FullName = s.FirstName + " " + s.LastName;

                      if (!string.IsNullOrEmpty(s.CompanyName))
                          d.FullName = d.FullName + ": " + s.CompanyName;

                      if (string.IsNullOrEmpty(d.FullName.Trim()) && d.Email != null)
                          d.FullName = d.Email.EmailId;

                      d.ContactType = (int)ContactType.Person;
                  }
                  else
                  {

                      if (s.Emails != null)
                      {
                          var email = s.Emails.FirstOrDefault(c => c.IsPrimary);
                          d.Email = email;
                      }
                      d.FullName = s.CompanyName;

                      if (string.IsNullOrEmpty(d.FullName.Trim()) && d.Email != null)
                          d.FullName = d.Email.EmailId;
                      d.ContactType = (int)ContactType.Company;
                  }
                  d.Id = s.ContactID;
              });


            this.CreateMap<Contact, ContactGridEntry>()

               .AfterMap((s, d) =>
               {
                   if (s.GetType().Equals(typeof(Person)))
                   {
                       Person person = s as Person;
                       if (s.Emails != null && s.Emails.Any(e => e.IsPrimary))
                       {
                           var email = s.Emails.FirstOrDefault(c => c.IsPrimary);
                           if (email != null)
                           {
                               d.PrimaryEmail = email.EmailId;
                               d.PrimaryContactEmailID = email.EmailID;
                               d.PrimaryEmailStatus = email.EmailStatusValue;
                           }
                       }
                       else
                           d.PrimaryEmail = "[|Email Not Available|]";

                       d.Name = person.FirstName + " " + person.LastName;
                       d.FirstName = person.FirstName;
                       d.LastName = person.LastName;
                       d.CompanyName = person.CompanyName ?? string.Empty;
                       d.ContactType = (int)ContactType.Person;
                       d.ContactImageUrl = person.ContactImage != null ? person.ContactImage.StorageName : null;

                       if (s.LastContacted != null)
                       {
                           d.LastContactedDate = s.LastContacted;
                           if (s.LastContactedThrough.HasValue && s.LastContactedThrough.Value > 0)
                               d.LastTouchedThrough = ((LastTouchedValues)s.LastContactedThrough).GetDisplayName();
                       }
                       else
                       {
                           d.LastTouched = "[|Not Contacted|]";
                       }
                   }
                   else
                   {
                       Company company = s as Company;
                       if (s.Emails != null && s.Emails.Any(e => e.IsPrimary))
                       {
                           var email = s.Emails.FirstOrDefault(c => c.IsPrimary);
                           if (email != null)
                           {
                               d.PrimaryEmail = email.EmailId;
                               d.PrimaryContactEmailID = email.EmailID;
                               d.PrimaryEmailStatus = email.EmailStatusValue;
                           }
                       }
                       else
                           d.PrimaryEmail = "[|Email Not Available|]";

                       d.Name = s.CompanyName ?? string.Empty;
                       d.ContactType = (int)ContactType.Company;
                       d.ContactImageUrl = company.ContactImage != null ? company.ContactImage.StorageName : null;

                   }
                   d.ContactID = s.Id;

                   var address = s.Addresses.FirstOrDefault(a => a.IsDefault);
                   d.Address = address != null ? address.ToString() : "[|No address details|]";

                   var primaryPhone = s.Phones.IsAny() ? s.Phones.FirstOrDefault(p => p.IsPrimary) : null;
                   if (primaryPhone != null)
                   {
                       d.Phone = primaryPhone.Number != null ? (primaryPhone.Number + " ," + primaryPhone.PhoneTypeName) : "(xxx) xxx - xxxx";
                       d.PrimaryContactPhoneNumberID = primaryPhone.ContactPhoneNumberID;
                       d.PhoneCountryCode = primaryPhone.CountryCode;
                       d.PhoneExtension = primaryPhone.Extension;
                   }
                   else
                       d.Phone = "(xxx) xxx - xxxx";
               });

            this.CreateMap<Contact, ContactReportEntry>()

               .AfterMap((s, d) =>
               {
                   if (s.GetType().Equals(typeof(Person)))
                   {
                       Person person = s as Person;
                       if (s.Emails != null && s.Emails.Any(e => e.IsPrimary))
                       {
                           var email = s.Emails.FirstOrDefault(c => c.IsPrimary);
                           if (email != null)
                           {
                               d.PrimaryEmail = email.EmailId;
                               d.PrimaryContactEmailID = email.EmailID;
                           }
                       }
                       else
                           d.PrimaryEmail = "[|Email Not Available|]";

                       d.Name = person.FirstName + " " + person.LastName;
                       d.CompanyName = person.CompanyName ?? string.Empty;
                       d.ContactType = (int)ContactType.Person;
                       d.LeadScore = person.LeadScore;

                       if (person.LeadSources.IsAny())
                       {
                           d.PrimaryLeadSource = person.LeadSources.Min(l => l.Id);
                           d.AllLeadSources = person.LeadSources.Select(l => l.Id).ToList();
                       }
                       else
                           d.PrimaryLeadSource = new Nullable<short>();

                   }
                   else
                   {
                       if (s.Emails != null && s.Emails.Any(e => e.IsPrimary))
                       {
                           var email = s.Emails.FirstOrDefault(c => c.IsPrimary);
                           if (email != null)
                           {
                               d.PrimaryEmail = email.EmailId;
                               d.PrimaryContactEmailID = email.EmailID;
                           }
                       }
                       else
                           d.PrimaryEmail = "[|Email Not Available|]";

                       d.Name = s.CompanyName ?? string.Empty;
                       d.ContactType = (int)ContactType.Company;

                   }
                   d.ContactID = s.Id;
                   d.CreatedOn = s.CreatedOn;


                   var primaryPhone = s.Phones.IsAny() ? s.Phones.FirstOrDefault(p => p.IsPrimary) : null;
                   if (primaryPhone != null)
                   {
                       d.Phone = primaryPhone.Number != null ? primaryPhone.Number : "(xxx) xxx - xxxx";
                       d.PhoneTypeName = primaryPhone.PhoneTypeName;
                       d.PrimaryContactPhoneNumberID = primaryPhone.ContactPhoneNumberID;
                       d.CountryCode = primaryPhone.CountryCode;
                       d.Extension = primaryPhone.Extension;
                   }
                   else
                       d.Phone = "(xxx) xxx - xxxx";
               });
            this.CreateMap<ReportContactInfo, ContactReportEntry>()
             .AfterMap((s, d) =>
             {

                 d.PrimaryEmail = s.Email == null ? "[|Email Not Available|]" : s.Email;
                 d.Name = s.FullName;
                 d.Phone = s.PhoneNumber == null ? "" : s.PhoneNumber;
                 d.OwnerName = s.Owner == null ? "" : s.Owner;

             });
            this.CreateMap<Contact, ContactLeadScoreListViewModel>()
                 .ForMember(c => c.Id, opt => opt.MapFrom(c => c.Id))

                //.IgnoreAllUnmapped()
                .AfterMap((s, d) =>
                    {
                        Person person = s as Person;
                        d.FullName = person.FirstName + " " + person.LastName;
                        // d.LifecycleName = person.LifecycleStage.ToString();
                        d.LifecycleStage = person.LifecycleStage;
                        d.LeadScore = person.LeadScore;
                        if (s.Emails != null)
                        {
                            d.PrimaryEmail = person.Emails.Where(p => p.IsPrimary).IsAny() ?
                            person.Emails.Where(p => p.IsPrimary).Select(pe => pe.EmailId).SingleOrDefault() : person.Emails.Select(pe => pe.EmailId).SingleOrDefault();
                        }
                    }
                );

            this.CreateMap<Contact, ViewContactEntry>()
                .ForMember(c => c.ContactID, opt => opt.MapFrom(c => c.Id));

            this.CreateMap<Contact, ContactExportViewModel>()
            .ForMember(c => c.PhoneType, opt => opt.Ignore())
            .ForMember(c => c.LastUpdatedOn, opt => opt.MapFrom(c => c.LastUpdatedOn))
            .ForMember(c => c.LastContacted, opt => opt.MapFrom(c => c.LastContacted))
            .ForMember(c => c.LastTouchedThrough, opt => opt.MapFrom(c => c.LastTouchedThrough))
            .AfterMap((s, d) =>
            {
                if (s.GetType().Equals(typeof(Person)))
                {
                    Person person = s as Person;

                    if (s.Phones != null && s.Phones.Any(p => p.IsPrimary == true))
                    {
                        d.HomePhone = s.Phones.First(p => p.IsPrimary == true).Number;
                        d.PhoneType = s.Phones.First(p => p.IsPrimary == true).PhoneTypeName;
                    }
                    d.PrimaryEmail = person.Email;

                    d.LifecycleStage = person.LifecycleStage.ToString();
                    d.FirstName = person.FirstName;
                    d.LastName = person.LastName;
                    d.Company = person.CompanyName;
                    d.ContactType = (int)ContactType.Person;
                }

                else
                {
                    Company company = s as Company;
                    d.FirstName = "";
                    d.LastName = "";
                    d.Company = company.CompanyName;
                    d.LifecycleStage = "";
                    d.LastContacted = company.LastContacted;
                    d.ContactType = (int)ContactType.Company;
                }
                d.ContactID = s.Id;
            });

            this.CreateMap<ContactListEntry, ContactExportViewModel>()
                  .ForMember(c => c.Company, opt => opt.MapFrom(c => c.CompanyName))
                  .ForMember(c => c.HomePhone, opt => opt.MapFrom(c => c.Phone));


            this.CreateMap<Report, ReportListEntry>()
                .ForMember(c => c.ReportID, opt => opt.MapFrom(c => c.Id));

            this.CreateMap<BDXCustomLeadContactInfo, BDXLeadReportEntry>()
                .ForMember(c => c.ContactID, opt => opt.MapFrom(c => c.ContactID))
                .ForMember(c => c.LeadSource, opt => opt.MapFrom(c => c.LeadSource))
                .ForMember(c => c.CommunityName, opt => opt.MapFrom(c => c.CommunityName == null ? "" : c.CommunityName))
                .ForMember(c => c.FullName, opt => opt.MapFrom(c => c.FullName))
                .ForMember(c => c.PrimaryEmail, opt => opt.MapFrom(c => c.PrimaryEmail))
                .ForMember(c => c.LeadType, opt => opt.MapFrom(c => c.LeadType == null ? "" : c.LeadType))
                .ForMember(c => c.CommunityNumber, opt => opt.MapFrom(c => c.CommunityNumber == null ? "" : c.CommunityNumber))
                .ForMember(c => c.MarketName, opt => opt.MapFrom(c => c.MarketName == null ? "" : c.MarketName))
                .ForMember(c => c.PlanName, opt => opt.MapFrom(c => c.PlanName == null ? "" : c.PlanName))
                .ForMember(c => c.PlanNumber, opt => opt.MapFrom(c => c.PlanNumber == null ? "" : c.PlanNumber))
                .ForMember(c => c.Comments, opt => opt.MapFrom(c => c.Comments == null ? "" : c.Comments))
                .ForMember(c => c.Phone, opt => opt.MapFrom(c => c.Phone == null ? "" : c.Phone))
                .ForMember(c => c.StreetAddress, opt => opt.MapFrom(c => c.StreetAddress == null ? "" : c.StreetAddress))
                .ForMember(c => c.City, opt => opt.MapFrom(c => c.City == null ? "" : c.City))
                .ForMember(c => c.State, opt => opt.MapFrom(c => c.State == null ? "" : c.State))
                .ForMember(c => c.PostalCode, opt => opt.MapFrom(c => c.PostalCode == null ? "" : c.PostalCode))
                .ForMember(c => c.BuilderName, opt => opt.MapFrom(c => c.BuilderName == null ? "" : c.BuilderName))
                .ForMember(c => c.BuilderNumber, opt => opt.MapFrom(c => c.BuilderNumber == null ? "" : c.BuilderNumber))
                .ForMember(c => c.StateName, opt => opt.MapFrom(c => c.StateName == null ? "" : c.StateName))
                ;
            // .ForMember(c => c.CommunityNumber, opt => opt.MapFrom(c => c.CommunityNumber == null ?"": c.CommunityNumber));


            this.CreateMap<TimeLineContact, TimeLineEntryViewModel>()
                .ForMember(c => c.TimeLineID, opt => opt.MapFrom(c => c.ID))
                .AfterMap((s, d) =>
                {
                    d.Year = s.AuditDate.Year;
                    d.MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(s.AuditDate.Month);
                    d.Month = s.AuditDate.Month;
                    d.SortBy = s.AuditDate.Month > 9 ? s.AuditDate.Month.ToString() : "0" + s.AuditDate.Month;
                });

            this.CreateMap<TimeLineGroup, TimeLineGroupViewModel>()
              .ForMember(c => c.Year, opt => opt.MapFrom(c => c.Year))
              .ForMember(c => c.Months, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<TimeLineMonthGroup>, IEnumerable<TimeLineMonthGroupViewModel>>(c.Months)));

            this.CreateMap<TimeLineMonthGroup, TimeLineMonthGroupViewModel>()
           .ForMember(c => c.Month, opt => opt.MapFrom(c => c.Month));



            this.CreateMap<CampaignTemplate, CampaignTemplateViewModel>()
                .ForMember(ct => ct.TemplateId, opt => opt.MapFrom(ct => ct.Id));

            this.CreateMap<Address, AddressViewModel>()
                  .ForMember(a => a.AddressTypeID, opt => opt.MapFrom(c => c.AddressTypeID))
                  .ForMember(a => a.AddressLine1, opt => opt.MapFrom(c => c.AddressLine1))
                  .ForMember(a => a.AddressLine2, opt => opt.MapFrom(c => c.AddressLine2))
                  .ForMember(a => a.City, opt => opt.MapFrom(c => c.City))
                  .ForMember(a => a.ZipCode, opt => opt.MapFrom(c => c.ZipCode))
                  .ForMember(a => a.State, opt => opt.MapFrom(c => c.State))
                  .ForMember(a => a.Country, opt => opt.MapFrom(c => c.Country));

            this.CreateMap<ContactRelationship, RelationshipEntry>()
                .ForMember(a => a.ContactRelationshipMapID, opt => opt.MapFrom(c => c.Id))
                .ForMember(a => a.RelationshipType, opt => opt.MapFrom(c => c.RelationshipTypeID))
                .ForMember(a => a.RelationshipTypeName, opt => opt.MapFrom(c => c.RelationshipName))
                 .ForMember(a => a.RelatedContact, opt => opt.MapFrom(c => c.RelatedContact.FirstName))
                 .ForMember(a => a.ContactId, opt => opt.MapFrom(c => c.RelatedContact.Id))
                .IgnoreAllUnmapped()
                .AfterMap((s, d) =>
                {
                    d.ContactId = s.ContactId;
                    d.ContactRelationshipMapID = s.Id;
                    d.RelatedContactID = s.RelatedContactID;
                    d.RelatedUserID = s.RelatedUserID;
                    d.RelatedContact = s.RelatedContact.FirstName;
                    d.RelationshipType = s.RelationshipTypeID;
                    d.RelationshipTypeName = s.RelationshipName;
                    d.ContactName = s.ContactName;
                    d.RelatedContactType = (byte)s.RelatedContactType;
                });
            this.CreateMap<Email, string>()
                .ConvertUsing(c => c.EmailId ?? string.Empty);

            this.CreateMap<Image, ImageViewModel>()
                .ForMember(a => a.ImageID, opt => opt.MapFrom(c => c.Id));
            this.CreateMap<Dropdown, DropdownViewModel>()
                .ForMember(a => a.DropdownID, opt => opt.MapFrom(c => c.Id))
                .ForMember(a => a.Dropdownname, opt => opt.MapFrom(dpd => dpd.Name))
                .ForMember(a => a.DropdownValuesList, opt => opt.Ignore())
                .AfterMap((s, d) =>
                {
                    d.DropdownValuesList = Mapper.Map<IEnumerable<DropdownValue>, IEnumerable<DropdownValueViewModel>>(s.DropdownValues);
                    d.Dropdownname = s.Name; d.DropdownID = s.Id;
                    var sb = new StringBuilder();
                    foreach (DropdownValueViewModel dvvm in d.DropdownValuesList) sb.Append(" " + dvvm.DropdownValue + ", ");
                    d.dropdownValuesToString = sb.ToString();
                    var array = new char[] { ',', ' ' };
                    if (d.dropdownValuesToString != null)
                        d.dropdownValuesToString = d.dropdownValuesToString.TrimStart(array).TrimEnd(array);
                    else
                        d.dropdownValuesToString = "";
                });


            this.CreateMap<DropdownValue, DropdownValueViewModel>()
                .ForMember(a => a.DropdownValueID, opt => opt.MapFrom(a => a.Id))
                .ForMember(a => a.DropdownValue, opt => opt.MapFrom(a => a.Value));

            this.CreateMap<DA.Action, ActionListEntry>()
                .IgnoreAllUnmapped()
                .ForMember(a => a.ActionId, opt => opt.MapFrom(a => a.Id))
                .ForMember(a => a.ActionDetail, opt => opt.MapFrom(a => a.Details));

            this.CreateMap<DA.Action, ActionViewModel>()
                .ForMember(a => a.ActionId, opt => opt.MapFrom(a => a.Id))
                .ForMember(a => a.ActionMessage, opt => opt.MapFrom(a => a.Details))
                .ForMember(c => c.SelectedReminderTypes, opt => opt.MapFrom(a => a.ReminderTypes))
                .ForMember(a => a.RemindOn, opt => opt.MapFrom(a => a.RemindOn))
                .ForMember(a => a.TagsList, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<Tag>, IEnumerable<TagViewModel>>(c.Tags)))
                .ForMember(a => a.Contacts, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<RawContact>, IEnumerable<ContactEntry>>(c.Contacts)))
                //.ForMember(t => t.CreatedBy, opt => opt.MapFrom(t => int.Parse(t.User.Id)))
                .ForMember(t => t.CreatedOn, opt => opt.MapFrom(a => a.CreatedOn));

            this.CreateMap<Person, PersonViewModel>()
                  .ForMember(p => p.ContactID, opt => opt.MapFrom(c => c.Id))
                  .ForMember(p => p.CompanyID, opt => opt.MapFrom(c => c.CompanyID))
                  .ForMember(p => p.Addresses, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<Address>, IEnumerable<AddressViewModel>>(c.Addresses)))
                  .ForMember(p => p.Phones, opt => opt.Ignore())
                  .ForMember(p => p.Emails, opt => opt.Ignore())
                  .ForMember(p => p.Image, opt => opt.MapFrom(c => Mapper.Map<Image, ImageViewModel>(c.ContactImage)))
                  .ForMember(p => p.PartnerType, opt => opt.MapFrom(c => c.PartnerType))
                  .ForMember(p => p.LifecycleStage, opt => opt.MapFrom(c => c.LifecycleStage))
                  .ForMember(p => p.ContactImageUrl, opt => opt.MapFrom(c => c.ImageUrl))
                  .ForMember(p => p.ContactType, opt => opt.MapFrom(c => 1))
                  .ForMember(p => p.LifeCycle, opt => opt.MapFrom(c => c.LifecycleStage))
                  .ForMember(p => p.Name, opt => opt.MapFrom(c => c.FirstName + " " + c.LastName))
                //.ForMember(p => p.PrimaryEmail, opt => opt.MapFrom(c => c.Email.EmailId))
                  .ForMember(p => p.SecondaryEmails, opt => opt.Ignore())
                  .ForMember(p => p.LeadSources, opt => opt.Ignore())
                  .ForMember(p => p.Actions, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<DA.Action>, IEnumerable<ActionListEntry>>(c.Actions)))
                  .ForMember(c => c.SelectedLeadSource, opt => opt.Ignore())
                .ForMember(p => p.ContactSummary, opt => opt.MapFrom(c => c.NoteSummary ?? ""))
                               .ForMember(c => c.LastNoteDate, opt => opt.MapFrom(c => c.LastNoteDate))
                  .AfterMap((s, d) =>
                  {
                      d.SocialMediaUrls = new List<Url>();
                      if (s.FacebookUrl != null)
                          d.SocialMediaUrls.Add(new Url { MediaType = "Facebook", URL = s.FacebookUrl.URL });
                      if (s.TwitterUrl != null)
                          d.SocialMediaUrls.Add(new Url { MediaType = "Twitter", URL = s.TwitterUrl.URL });
                      if (s.GooglePlusUrl != null)
                          d.SocialMediaUrls.Add(new Url { MediaType = "Google+", URL = s.GooglePlusUrl.URL });
                      if (s.LinkedInUrl != null)
                          d.SocialMediaUrls.Add(new Url { MediaType = "LinkedIn", URL = s.LinkedInUrl.URL });
                      if (s.BlogUrl != null)
                          d.SocialMediaUrls.Add(new Url { MediaType = "Blog", URL = s.BlogUrl.URL });
                      if (s.WebsiteUrl != null)
                          d.SocialMediaUrls.Add(new Url { MediaType = "Website", URL = s.WebsiteUrl.URL });

                      var defaultAddress = Mapper.Map<Address, AddressViewModel>(s.Addresses.Where(a => a.IsDefault.Equals(true)).FirstOrDefault());
                      d.Address = defaultAddress != null ? defaultAddress.ToString() : "";

                      if (s.LeadSources != null)
                      {
                          var leadsource = new List<DropdownValueViewModel>();
                          foreach (DropdownValue value in s.LeadSources)
                          {
                              DropdownValueViewModel dropdownValues;
                              dropdownValues = new DropdownValueViewModel() { DropdownValueID = value.Id, DropdownValue = value.Value };
                              leadsource.Add(dropdownValues);
                          }
                          d.SelectedLeadSource = leadsource;
                      }

                      if (s.Communities != null)
                      {
                          var communities = new List<DropdownValueViewModel>();
                          foreach (DropdownValue value in s.Communities)
                          {
                              DropdownValueViewModel dropdownValues;
                              dropdownValues = new DropdownValueViewModel() { DropdownValueID = value.Id, DropdownValue = value.Value };
                              communities.Add(dropdownValues);
                          }
                          d.Communities = communities;
                      }
                      if (s.AllLeadSources != null)
                      {
                          var leadsource = new List<DropdownValueViewModel>();
                          foreach (DropdownValue value in s.AllLeadSources)
                          {
                              DropdownValueViewModel dropdownValues;
                              dropdownValues = new DropdownValueViewModel() { DropdownValueID = value.Id, DropdownValue = value.Value };
                              leadsource.Add(dropdownValues);
                          }
                          d.LeadSources = leadsource;
                      }

                      if (s.Phones != null)
                      {
                          var phones = new List<Phone>();
                          foreach (Phone phone in s.Phones)
                          {
                              phones.Add(phone);
                          }
                          d.Phones = phones;
                      }

                      if (s.Emails != null)
                      {
                          var emails = new List<Email>();
                          foreach (Email email in s.Emails)
                          {
                              emails.Add(email);
                          }
                          d.Emails = emails;
                      }

                  });

            this.CreateMap<Company, CompanyViewModel>()
                 .ForMember(c => c.ContactID, opt => opt.MapFrom(c => c.Id))
                 .ForMember(p => p.CompanyID, opt => opt.MapFrom(c => c.CompanyID))
                 .ForMember(c => c.CompanyName, opt => opt.MapFrom(c => c.CompanyName))
                 .ForMember(c => c.Addresses, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<Address>, IEnumerable<AddressViewModel>>(c.Addresses)))
                 .ForMember(p => p.Address, opt => opt.Ignore())
                 .ForMember(p => p.Phones, opt => opt.Ignore())
                 .ForMember(p => p.Emails, opt => opt.Ignore())
                   .ForMember(c => c.Image, opt => opt.MapFrom(c => Mapper.Map<Image, ImageViewModel>(c.ContactImage)))
                //.ForMember(p => p.PrimaryEmail, opt => opt.MapFrom(c => c.Email.EmailId))
                 .ForMember(c => c.SecondaryEmails, opt => opt.Ignore())
                 .ForMember(p => p.Actions, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<DA.Action>, IEnumerable<ActionListEntry>>(c.Actions)))
                .ForMember(p => p.ContactSummary, opt => opt.MapFrom(c => c.NoteSummary ?? ""))
                               .ForMember(c => c.LastNoteDate, opt => opt.MapFrom(c => c.LastNoteDate))
                 .AfterMap((s, d) =>
                 {

                     d.SocialMediaUrls = new List<Url>();
                     if (s.FacebookUrl != null)
                         d.SocialMediaUrls.Add(new Url { MediaType = "Facebook", URL = s.FacebookUrl.URL });
                     if (s.TwitterUrl != null)
                         d.SocialMediaUrls.Add(new Url { MediaType = "Twitter", URL = s.TwitterUrl.URL });
                     if (s.GooglePlusUrl != null)
                         d.SocialMediaUrls.Add(new Url { MediaType = "Google+", URL = s.GooglePlusUrl.URL });
                     if (s.LinkedInUrl != null)
                         d.SocialMediaUrls.Add(new Url { MediaType = "LinkedIn", URL = s.LinkedInUrl.URL });
                     if (s.BlogUrl != null)
                         d.SocialMediaUrls.Add(new Url { MediaType = "Blog", URL = s.BlogUrl.URL });
                     if (s.WebsiteUrl != null)
                         d.SocialMediaUrls.Add(new Url { MediaType = "Website", URL = s.WebsiteUrl.URL });
                     //if (s.ContactRelationships != null)
                     //{
                     //    d.RelationshipViewModel = new RelationshipViewModel();
                     //    List<RelationshipEntry> relation = new List<RelationshipEntry>();

                     //    for (int i = 0; i < s.ContactRelationships.Count; i++)
                     //    {
                     //        var contactrelation = s.ContactRelationships[i];
                     //        RelationshipEntry relationshipEntry = new RelationshipEntry();
                     //        relationshipEntry.ContactRelationshipMapID = contactrelation.Id;
                     //        relationshipEntry.ContactId = contactrelation.ContactId;
                     //        relationshipEntry.DisplayContact = s.CompanyName;
                     //        relationshipEntry.DisplayRelatedContact = contactrelation.RelatedContact.FirstName;
                     //        relationshipEntry.RelatedContactID = contactrelation.RelatedContactID;
                     //        relationshipEntry.RelationshipType = contactrelation.RelationshipTypeID;
                     //        relationshipEntry.RelationshipTypeName = relationshipEntry.RelationshipTypeName;
                     //        relation.Add(relationshipEntry);
                     //    }
                     //    d.RelationshipViewModel.Relationshipentry = relation;

                     //}
                     var defaultAddress = Mapper.Map<Address, AddressViewModel>(s.Addresses.Where(a => a.IsDefault.Equals(true)).FirstOrDefault());
                     d.Address = defaultAddress != null ? defaultAddress.ToString() : "[|No address details|]";

                     if (s.Phones != null)
                     {
                         var phones = new List<Phone>();
                         foreach (dynamic phone in s.Phones)
                         {
                             phones.Add(phone);
                         }
                         d.Phones = phones;
                     }

                     if (s.Emails != null)
                     {
                         var emails = new List<Email>();
                         foreach (dynamic email in s.Emails)
                         {
                             emails.Add(email);
                         }
                         d.Emails = emails;
                     }
                 });

            this.CreateMap<Person, ContactListEntry>()
              .ForMember(c => c.FirstName, opt => opt.MapFrom(c => c.FirstName))
              .ForMember(c => c.LastName, opt => opt.MapFrom(c => c.LastName))
              .ForMember(c => c.Name, opt => opt.MapFrom(c => c.FirstName + " " + c.LastName))
              .ForMember(c => c.CompanyName, opt => opt.MapFrom(c => !string.IsNullOrEmpty(c.CompanyName) ? c.CompanyName : string.Empty))
              .ForMember(c => c.CompanyID, opt => opt.MapFrom(c => c.CompanyID))
              .ForMember(c => c.ContactID, opt => opt.MapFrom(c => c.Id))
              .ForMember(c => c.Address, opt => opt.Ignore())
              .ForMember(c => c.ContactType, opt => opt.MapFrom(c => (int)ContactType.Person))
              .ForMember(c => c.ContactImageUrl, opt => opt.MapFrom(c => c.ContactImage.StorageName))
              .ForMember(c => c.AccountID, opt => opt.MapFrom(c => c.AccountID))
              .ForMember(c => c.ProfileImageKey, opt => opt.MapFrom(c => c.ProfileImageKey))
              .ForMember(c => c.LifecycleStage, opt => opt.MapFrom(c => c.LifecycleStage))
              .ForMember(c => c.LeadScore, opt => opt.MapFrom(c => c.LeadScore))
               .ForMember(c => c.DoNotEmail, opt => opt.MapFrom(c => c.DoNotEmail))
               .ForMember(c => c.PartnerType, opt => opt.MapFrom(c => c.PartnerType))
               .ForMember(c => c.FacebookUrl, opt => opt.MapFrom(c => c.FacebookUrl.URL))
               .ForMember(c => c.GooglePlusUrl, opt => opt.MapFrom(c => c.GooglePlusUrl.URL))
               .ForMember(c => c.TwitterUrl, opt => opt.MapFrom(c => c.TwitterUrl.URL))
               .ForMember(c => c.LinkedInUrl, opt => opt.MapFrom(c => c.LinkedInUrl.URL))
               .ForMember(c => c.BlogUrl, opt => opt.MapFrom(c => c.BlogUrl.URL))
               .ForMember(c => c.WebsiteUrl, opt => opt.MapFrom(c => c.WebsiteUrl.URL))
               .ForMember(c => c.OwnerId, opt => opt.MapFrom(c => c.OwnerId))
               .ForMember(c => c.Title, opt => opt.MapFrom(c => c.Title))
               .ForMember(p => p.LeadSources, opt => opt.Ignore())
               .ForMember(p => p.CreatedOn, opt => opt.MapFrom(c => c.CreatedOn))
               .ForMember(p => p.CreatedBy, opt => opt.MapFrom(c => c.CreatedBy))
               .ForMember(p => p.LastTouched, opt => opt.MapFrom(c => c.LastContacted))
               .ForMember(p => p.LastTouchedThrough, opt => opt.MapFrom(c => c.LastContactedThrough))
               .ForMember(c => c.LastUpdatedOn, opt => opt.MapFrom(c => c.LastUpdatedOn))
               .ForMember(c => c.CustomFields, opt => opt.MapFrom(c => c.CustomFields != null ? Mapper.Map<IEnumerable<ContactCustomField>, IEnumerable<ContactCustomFieldMapViewModel>>(c.CustomFields) : null))
               .ForMember(c => c.LeadSourceIds, opt => opt.MapFrom(c => c.LeadSources != null ? c.LeadSources.Select(s => s.Id).ToList() : null))
              .ForMember(c => c.PrimaryAddress, opt => opt.MapFrom(c => c.Addresses != null ? Mapper.Map<Address, AddressViewModel>(c.Addresses.Where(a => a.IsDefault.Equals(true)).FirstOrDefault()) : null))
               .ForMember(c => c.NoteSummary, opt => opt.MapFrom(c => c.NoteSummary ?? ""))
               .ForMember(c => c.LastNoteDate, opt => opt.MapFrom(c => c.LastNoteDate))
              .AfterMap((s, d) =>
              {
                  if (s.Addresses != null)
                  {
                      AddressViewModel defaultAddress = Mapper.Map<Address, AddressViewModel>(s.Addresses.Where(a => a.IsDefault.Equals(true)).FirstOrDefault());
                      d.Address = defaultAddress != null ? defaultAddress.ToString() : "[|No address details|]";
                      d.PrimaryAddress = defaultAddress;
                  }

                  if (s.Emails != null)
                  {
                      var primaryEmail = s.Emails.IsAny() ? s.Emails.FirstOrDefault(e => e.IsPrimary) : null;
                      if (primaryEmail != null)
                      {
                          d.PrimaryEmail = primaryEmail.EmailId;
                          d.PrimaryContactEmailID = primaryEmail.EmailID;
                          d.PrimaryEmailStatus = primaryEmail.EmailStatusValue;
                      }
                      else
                          d.PrimaryEmail = "[|Email Not Available|]";
                  }
                  if (s.LeadSources != null && s.LeadSources.Any())
                  {
                      d.LeadSourceIds = s.LeadSources.Where(w => !w.IsPrimary).Select(se => se.Id).ToList();
                      var dates = s.LeadSources.Where(w => w.IsPrimary == false).Select(se => se.LastUpdatedDate);
                      d.LeadSourceDate = dates != null && dates.IsAny() ? string.Join("| ", dates) : string.Empty;
                      d.FirstLeadSourceId = s.LeadSources.Where(w => w.IsPrimary == true).Select(se => se.Id).FirstOrDefault();
                      d.FirstLeadSourceDate = s.LeadSources.Where(w => w.IsPrimary == true).Select(se => se.LastUpdatedDate).FirstOrDefault();
                  }
                  if (s.Phones != null)
                  {
                      var primaryPhone = s.Phones.IsAny() ? s.Phones.FirstOrDefault(p => p.IsPrimary) : null;
                      if (primaryPhone != null)
                      {
                          d.Phone = primaryPhone.Number != null ? (primaryPhone.Number + " (" + primaryPhone.PhoneTypeName + ")") : "(xxx) xxx - xxxx";
                         // d.Phone = primaryPhone.Number != null ? (!string.IsNullOrEmpty(primaryPhone.CountryCode) ? "+" + primaryPhone.CountryCode + " " : "") + primaryPhone.Number + (!string.IsNullOrEmpty(primaryPhone.Extension) ? " Ext. " + primaryPhone.Extension : "") + " (" + primaryPhone.PhoneTypeName + ")" : "(xxx) xxx - xxxx";
                          d.PrimaryContactPhoneNumberID = primaryPhone.ContactPhoneNumberID;
                      }
                      else
                          d.Phone = "(xxx) xxx - xxxx";

                      var phones = new List<Phone>();
                      foreach (Phone phone in s.Phones)
                      {
                          if (phone != null)
                              phone.Number = phone.Number != null ? phone.Number : "(xxx) xxx - xxxx";
                          phones.Add(phone);
                      }
                      d.Phones = phones;
                  }

                  if (s.LastContactedThrough.HasValue && s.LastContactedThrough.Value > 0)
                  {
                      d.LastTouchedThrough = ((LastTouchedValues)s.LastContactedThrough.Value).GetDisplayName();
                  }
                  if (s.FirstContactSource.HasValue)
                      d.SourceType = ((ContactSource)s.FirstContactSource.Value).GetDisplayName();
              });

            this.CreateMap<Company, ContactListEntry>()
                .ForMember(c => c.CompanyName, opt => opt.MapFrom(c => c.CompanyName))
                .ForMember(c => c.Name, opt => opt.MapFrom(c => c.CompanyName))
                .ForMember(c => c.ContactID, opt => opt.MapFrom(c => c.Id))
                .ForMember(c => c.CompanyID, opt => opt.MapFrom(c => c.CompanyID))
                .ForMember(c => c.Address, opt => opt.Ignore())
                .ForMember(c => c.ContactImageUrl, opt => opt.MapFrom(c => c.ContactImage.StorageName))
                .ForMember(c => c.AccountID, opt => opt.MapFrom(c => c.AccountID))
                .ForMember(c => c.ProfileImageKey, opt => opt.MapFrom(c => c.ProfileImageKey))
                .ForMember(c => c.ContactType, opt => opt.MapFrom(c => (int)ContactType.Company))
                .ForMember(c => c.DoNotEmail, opt => opt.MapFrom(c => c.DoNotEmail))
                .ForMember(c => c.FacebookUrl, opt => opt.MapFrom(c => c.FacebookUrl.URL))
               .ForMember(c => c.GooglePlusUrl, opt => opt.MapFrom(c => c.GooglePlusUrl.URL))
               .ForMember(c => c.TwitterUrl, opt => opt.MapFrom(c => c.TwitterUrl.URL))
               .ForMember(c => c.LinkedInUrl, opt => opt.MapFrom(c => c.LinkedInUrl.URL))
               .ForMember(c => c.BlogUrl, opt => opt.MapFrom(c => c.BlogUrl.URL))
               .ForMember(c => c.WebsiteUrl, opt => opt.MapFrom(c => c.WebsiteUrl.URL))
               .ForMember(c => c.OwnerId, opt => opt.MapFrom(c => c.OwnerId))
               .ForMember(c => c.LastUpdatedOn, opt => opt.MapFrom(c => c.LastUpdatedOn))
               .ForMember(c => c.CreatedBy, opt => opt.MapFrom(c => c.CreatedBy))
                .ForMember(c => c.CreatedOn, opt => opt.MapFrom(c => c.CreatedOn))
               .ForMember(c => c.LastTouched, opt => opt.MapFrom(c => c.LastContacted))
               .ForMember(c => c.CustomFields, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<ContactCustomField>, IEnumerable<ContactCustomFieldMapViewModel>>(c.CustomFields)))
               .ForMember(c => c.PrimaryAddress, opt => opt.MapFrom(c => Mapper.Map<Address, AddressViewModel>(c.Addresses.Where(a => a.IsDefault.Equals(true)).FirstOrDefault())))
               .ForMember(c => c.NoteSummary, opt => opt.MapFrom(c => c.NoteSummary ?? ""))
                              .ForMember(c => c.LastNoteDate, opt => opt.MapFrom(c => c.LastNoteDate))
                .AfterMap((s, d) =>
                {
                    if (s.Addresses != null)
                    {
                        var defaultAddress = Mapper.Map<Address, AddressViewModel>(s.Addresses.Where(a => a.IsDefault.Equals(true)).FirstOrDefault());
                        d.Address = defaultAddress != null ? defaultAddress.ToString() : "[|No address details|]";
                    }

                    if (s.Emails != null)
                    {
                        var primaryEmail = s.Emails.IsAny() ? s.Emails.FirstOrDefault(e => e.IsPrimary) : null;
                        if (primaryEmail != null)
                        {
                            d.PrimaryEmail = primaryEmail.EmailId;
                            d.PrimaryContactEmailID = primaryEmail.EmailID;
                            d.PrimaryEmailStatus = primaryEmail.EmailStatusValue;
                        }
                        else
                            d.PrimaryEmail = "[|Email Not Available|]";
                    }


                    if (s.Phones != null)
                    {
                        var primaryPhone = s.Phones.IsAny() ? s.Phones.FirstOrDefault(p => p.IsPrimary) : null;
                        if (primaryPhone != null)
                        {
                            d.Phone = primaryPhone.Number + " (" + primaryPhone.PhoneTypeName + ")";
                            //d.Phone = primaryPhone.Number != null ? (!string.IsNullOrEmpty(primaryPhone.CountryCode) ? "+" + primaryPhone.CountryCode + " " : "") + primaryPhone.Number + (!string.IsNullOrEmpty(primaryPhone.Extension) ? " Ext. " + primaryPhone.Extension : "") + " (" + primaryPhone.PhoneTypeName + ")" : "(xxx) xxx - xxxx";
                            d.PrimaryContactPhoneNumberID = primaryPhone.ContactPhoneNumberID;
                        }
                        else
                            d.Phone = "(xxx) xxx - xxxx";

                        var phones = new List<Phone>();
                        foreach (Phone phone in s.Phones)
                        {
                            phones.Add(phone);
                        }
                        d.Phones = phones;
                    }

                    if (s.LastContactedThrough.HasValue && s.LastContactedThrough.Value > 0)
                    {
                        d.LastTouchedThrough = ((LastTouchedValues)s.LastContactedThrough.Value).GetDisplayName();
                    }
                    if (s.FirstContactSource.HasValue)
                        d.SourceType = ((ContactSource)s.FirstContactSource.Value).GetDisplayName();
                });

            this.CreateMap<Person, ContactAdvancedSearchEntry>()
                .ForMember(c => c.ContactID, opt => opt.MapFrom(c => c.Id))
                .ForMember(c => c.CompanyID, opt => opt.MapFrom(c => c.CompanyID))
                .ForMember(c => c.CompanyName, opt => opt.MapFrom(c => c.CompanyName))
                .ForMember(c => c.ContactType, opt => opt.MapFrom(c => (int)ContactType.Person))
                .ForMember(c => c.DoNotEmail, opt => opt.MapFrom(c => c.DoNotEmail))
                .ForMember(c => c.Name, opt => opt.MapFrom(c => c.FirstName + " " + c.LastName))
                .ForMember(c => c.FirstName, opt => opt.MapFrom(c => c.FirstName))
                .ForMember(c => c.LastName, opt => opt.MapFrom(c => c.LastName))
                .ForMember(c => c.ContactID, opt => opt.MapFrom(c => c.Id))
                .AfterMap((s, d) =>
                {
                    if (s.Emails != null)
                    {
                        var primaryEmail = s.Emails.IsAny() ? s.Emails.FirstOrDefault(e => e.IsPrimary) : null;
                        if (primaryEmail != null)
                        {
                            d.PrimaryEmail = primaryEmail.EmailId;
                            d.PrimaryContactEmailID = primaryEmail.EmailID;
                            d.PrimaryEmailStatus = primaryEmail.EmailStatusValue;
                        }
                        else
                            d.PrimaryEmail = "[|Email Not Available|]";
                    }
                });

            this.CreateMap<Company, ContactAdvancedSearchEntry>()
                .ForMember(c => c.ContactID, opt => opt.MapFrom(c => c.Id))
                .ForMember(c => c.CompanyID, opt => opt.MapFrom(c => c.CompanyID))
                .ForMember(c => c.CompanyName, opt => opt.MapFrom(c => c.CompanyName))
                .ForMember(c => c.ContactType, opt => opt.MapFrom(c => (int)ContactType.Company))
                .ForMember(c => c.DoNotEmail, opt => opt.MapFrom(c => c.DoNotEmail))
                .ForMember(c => c.ContactID, opt => opt.MapFrom(c => c.Id))
                .AfterMap((s, d) =>
                {
                    if (s.Emails != null)
                    {
                        var primaryEmail = s.Emails.IsAny() ? s.Emails.FirstOrDefault(e => e.IsPrimary) : null;
                        if (primaryEmail != null)
                        {
                            d.PrimaryEmail = primaryEmail.EmailId;
                            d.PrimaryContactEmailID = primaryEmail.EmailID;
                            d.PrimaryEmailStatus = primaryEmail.EmailStatusValue;
                        }
                        else
                            d.PrimaryEmail = "[|Email Not Available|]";
                    }
                });

            this.CreateMap<Person, ContactResultViewModel>()
                 .ForMember(c => c.FirstName, opt => opt.MapFrom(c => c.FirstName))
              .ForMember(c => c.LastName, opt => opt.MapFrom(c => c.LastName))
              .ForMember(c => c.Name, opt => opt.MapFrom(c => c.FirstName + " " + c.LastName))
              .ForMember(c => c.CompanyName, opt => opt.MapFrom(c => !string.IsNullOrEmpty(c.CompanyName) ? c.CompanyName : string.Empty))
               .ForMember(c => c.ContactType, opt => opt.MapFrom(c => "Person"))
               .ForMember(c => c.LifecycleStage, opt => opt.MapFrom(c => c.LifecycleStage))
              .ForMember(c => c.LeadScore, opt => opt.MapFrom(c => c.LeadScore))
               .ForMember(c => c.DoNotEmail, opt => opt.MapFrom(c => c.DoNotEmail))
               .ForMember(c => c.PartnerTypeID, opt => opt.MapFrom(c => c.PartnerType))
               .ForMember(c => c.FacebookUrl, opt => opt.MapFrom(c => c.FacebookUrl.URL))
               .ForMember(c => c.GooglePlusUrl, opt => opt.MapFrom(c => c.GooglePlusUrl.URL))
               .ForMember(c => c.TwitterUrl, opt => opt.MapFrom(c => c.TwitterUrl.URL))
               .ForMember(c => c.LinkedInUrl, opt => opt.MapFrom(c => c.LinkedInUrl.URL))
               .ForMember(c => c.BlogUrl, opt => opt.MapFrom(c => c.BlogUrl.URL))
               .ForMember(c => c.WebsiteUrl, opt => opt.MapFrom(c => c.WebsiteUrl.URL))
               .ForMember(c => c.OwnerId, opt => opt.MapFrom(c => c.OwnerId))
               .ForMember(c => c.Title, opt => opt.MapFrom(c => c.Title));

            this.CreateMap<Tag, TagViewModel>()
                .ForMember(t => t.TagName, opt => opt.MapFrom(t => t.TagName))
                .ForMember(t => t.TagID, opt => opt.MapFrom(t => t.Id));

            this.CreateMap<Note, NoteViewModel>()
                .ForMember(n => n.NoteId, opt => opt.MapFrom(n => n.Id))
                .ForMember(n => n.NoteDetails, opt => opt.MapFrom(n => n.Details))
                .ForMember(a => a.TagsList, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<Tag>, IEnumerable<TagViewModel>>(c.Tags)))
                .ForMember(a => a.Contacts, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<Contact>, IEnumerable<ContactEntry>>(c.Contacts)))
                .ForMember(t => t.CreatedBy, opt => opt.MapFrom(t => int.Parse(t.User.Id)))
                .ForMember(t => t.CreatedOn, opt => opt.Ignore());

            this.CreateMap<Tour, TourViewModel>()
                .ForMember(t => t.TourID, opt => opt.MapFrom(t => t.Id))
                .ForMember(t => t.CreatedBy, opt => opt.MapFrom(t => int.Parse(t.User.Id)))
                .ForMember(t => t.SelectedReminderTypes, opt => opt.MapFrom(t => t.ReminderTypes))
                .ForMember(t => t.CreatedOn, opt => opt.MapFrom(t => t.CreatedOn))
                .ForMember(t => t.Contacts, opt => opt.Ignore())
                .ForMember(t => t.Communities, opt => opt.Ignore())
                .ForMember(t => t.ReminderTypes, opt => opt.Ignore())
                .ForMember(t => t.TourTypes, opt => opt.Ignore())
                .AfterMap((s, d) =>
                {
                    if (s.Contacts != null)
                    {
                        var contacts = new List<ContactEntry>();
                        foreach (Contact contact in s.Contacts)
                        {
                            ContactEntry contactEntry;
                            contactEntry = new ContactEntry()
                            {
                                Id = contact.Id,
                                LifeCycleStage = contact.LifecycleStage,
                                OwnerId = contact.OwnerId,
                                TourType = s.TourType,
                                LeadSources = contact.LeadSources
                            };
                            contacts.Add(contactEntry);
                        }
                        d.Contacts = contacts;
                    }
                });

            this.CreateMap<DA.Action, ActionListEntry>()
                .ForMember(a => a.ActionId, opt => opt.MapFrom(c => c.Id))
                .ForMember(a => a.ActionDetail, opt => opt.MapFrom(c => c.Details))
                .ForMember(a => a.RemindOn, opt => opt.MapFrom(c => c.RemindOn));

            this.CreateMap<Attachment, AttachmentViewModel>()
              .ForMember(a => a.DocumentID, opt => opt.MapFrom(c => c.Id))
               .ForMember(p => p.DocumentType, opt => opt.MapFrom(c => c.DocumentTypeID))
              .ForMember(a => a.FileType, opt => opt.MapFrom(c => c.FileTypeID))
              .ForMember(p => p.DocumentTypeID, opt => opt.MapFrom(c => (int)c.DocumentTypeID))
              .ForMember(a => a.FileTypeID, opt => opt.MapFrom(c => (int)c.FileTypeID));

            this.CreateMap<CommunicationTracker, CommunicationTrackerViewModel>()
                .ForMember(a => a.CommunicationTrackerID, opt => opt.MapFrom(c => c.Id));

            this.CreateMap<ServiceProvider, ServiceProviderViewModel>()
                .ForMember(a => a.CommunicationLogID, opt => opt.MapFrom(c => c.Id))
                .AfterMap((s, d) =>
                {
                    if (s.Account != null)
                        d.AccountCode = s.Account.DomainURL.Replace("www.", "").Split('.').FirstOrDefault();
                    d.ImageDomain = Mapper.Map<ImageDomain, ImageDomainViewModel>(s.ImageDomain);
                });

            this.CreateMap<ImageDomain, ImageDomainViewModel>()
                .ForMember(n => n.ImageDomainId, opt => opt.MapFrom(n => n.Id));

            this.CreateMap<AccountsGridData, AccountViewModel>()
               .ForMember(p => p.AccountID, opt => opt.MapFrom(c => c.AccountID))
               .ForMember(p => p.AccountName, opt => opt.MapFrom(c => c.AccountName))
               .ForMember(p => p.Status, opt => opt.MapFrom(c => c.Status))
               .ForMember(p => p.StatusMessage, opt => opt.MapFrom(c => c.StatusMessage))
               .ForMember(p => p.ContactsCount, opt => opt.MapFrom(c => c.ContactsCount))
               .ForMember(p => p.EmailsCount, opt => opt.MapFrom(c => c.EmailsCount))
               .ForMember(p => p.DomainURL, opt => opt.MapFrom(c => c.DomainURL))
               .ForMember(p => p.SenderReputationCount, opt => opt.MapFrom(c => c.SenderReputationCount))
               .ForMember(P => P.SubscriptionName, opt => opt.MapFrom(m => m.SubscriptionName));

            this.CreateMap<Account, AccountViewModel>()
                 .ForMember(p => p.AccountID, opt => opt.MapFrom(c => c.Id))
                 .ForMember(p => p.Addresses, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<Address>, IEnumerable<AddressViewModel>>(c.Addresses)))
                 .ForMember(p => p.SubscribedModules, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<Module>, IEnumerable<ModuleViewModel>>(c.SubscribedModules)))
                 .ForMember(p => p.PrimaryEmail, opt => opt.MapFrom(c => c.Email.EmailId))
                 .ForMember(p => p.SubscriptionName, opt => opt.MapFrom(c => c.AccountName + " " + "Subscription"))
                 .ForMember(p => p.SubscriptionId, opt => opt.MapFrom(c => c.SubscriptionID))
                 .ForMember(p => p.Image, opt => opt.MapFrom(c => Mapper.Map<Image, ImageViewModel>(c.AccountLogo)))
                //.ForMember(p=>p.Status, opt=>opt)
                 .ForMember(p => p.SecondaryEmails, opt => opt.Ignore())
                 .AfterMap((s, d) =>
                 {
                     if (s.WebAnalyticsProvider != null)
                     {
                         d.WebAnalyticsProvider = Mapper.Map<WebAnalyticsProvider, WebAnalyticsProviderViewModel>(s.WebAnalyticsProvider);
                     }
                     d.Phones = new List<dynamic>();
                     if (s.HomePhone != null)
                         d.Phones.Add(new { PhoneType = "Home", PhoneNumber = s.HomePhone.Number });
                     if (s.WorkPhone != null)
                         d.Phones.Add(new { PhoneType = "Work", PhoneNumber = s.WorkPhone.Number });
                     if (s.MobilePhone != null)
                         d.Phones.Add(new { PhoneType = "Mobile", PhoneNumber = s.MobilePhone.Number });

                     d.SocialMediaUrls = new List<dynamic>();
                     if (s.FacebookUrl != null)
                         d.SocialMediaUrls.Add(new { MediaType = "Facebook", Url = s.FacebookUrl.URL });
                     if (s.TwitterUrl != null)
                         d.SocialMediaUrls.Add(new { MediaType = "Twitter", Url = s.TwitterUrl.URL });
                     if (s.GooglePlusUrl != null)
                         d.SocialMediaUrls.Add(new { MediaType = "Google+", Url = s.GooglePlusUrl.URL });
                     if (s.LinkedInUrl != null)
                         d.SocialMediaUrls.Add(new { MediaType = "LinkedIn", Url = s.LinkedInUrl.URL });
                     if (s.BlogUrl != null)
                         d.SocialMediaUrls.Add(new { MediaType = "Blog", Url = s.BlogUrl.URL });
                     if (s.WebsiteUrl != null)
                         d.SocialMediaUrls.Add(new { MediaType = "Website", Url = s.WebsiteUrl.URL });

                     if (s.SecondaryEmails != null)
                         d.SecondaryEmails = s.SecondaryEmails.Select(e => new { SecondaryEmailId = e.EmailId }).ToList<dynamic>();
                     if (!string.IsNullOrEmpty(s.ExcludedRoles))
                         d.SelectedRoles = s.ExcludedRoles.Split(',').Select(se => short.Parse(se));
                 });

            this.CreateMap<WebAnalyticsProvider, WebAnalyticsProviderViewModel>()
                .ForMember(v => v.WebAnalyticsProviderID, opt => opt.MapFrom(v => v.Id));

            this.CreateMap<User, UserViewModel>()
                  .ForMember(p => p.UserID, opt => opt.Ignore())
                  .ForMember(p => p.Addresses, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<Address>, IEnumerable<AddressViewModel>>(c.Addresses)))
                  .ForMember(p => p.Account, opt => opt.MapFrom(c => Mapper.Map<Account, AccountViewModel>(c.Account)))
                  .ForMember(p => p.Name, opt => opt.MapFrom(c => c.FirstName + " " + c.LastName))
                  .ForMember(p => p.PrimaryEmail, opt => opt.MapFrom(c => c.Email.EmailId))
                  .ForMember(p => p.RoleName, opt => opt.MapFrom(c => c.Role.RoleName))
                  .ForMember(p => p.RoleID, opt => opt.MapFrom(c => c.Role.Id))
                  .ForMember(p => p.Status, opt => opt.MapFrom(c => (byte)c.Status))
                  .AfterMap((s, d) =>
                  {
                      d.UserID = int.Parse(s.Id);
                      if (s.Account != null && s.Account.Addresses.IsAny())
                      {
                          if (s.Account.Addresses.Any(a => a.IsDefault == true))
                          {
                              d.DefaultCountry = s.Account.Addresses.SingleOrDefault(a => a.IsDefault == true).Country;
                              d.DefaultState = s.Account.Addresses.SingleOrDefault(a => a.IsDefault == true).State;
                          }
                          else
                          {
                              d.DefaultCountry = s.Account.Addresses.SingleOrDefault().Country;
                              d.DefaultState = s.Account.Addresses.SingleOrDefault().State;
                          }
                      }

                      d.Phones = new List<Phone>();
                      if (s.HomePhone != null)
                          d.Phones.Add(new Phone { PhoneType = (short)DropdownValueTypes.Homephone, Number = s.HomePhone.Number, IsPrimary = s.HomePhone.IsPrimary });
                      if (s.WorkPhone != null)
                          d.Phones.Add(new Phone { PhoneType = (short)DropdownValueTypes.WorkPhone, Number = s.WorkPhone.Number, IsPrimary = s.WorkPhone.IsPrimary });
                      if (s.MobilePhone != null)
                          d.Phones.Add(new Phone { PhoneType = (short)DropdownValueTypes.MobilePhone, Number = s.MobilePhone.Number, IsPrimary = s.MobilePhone.IsPrimary });

                      d.SocialMediaUrls = new List<dynamic>();
                      if (s.FacebookUrl != null)
                          d.SocialMediaUrls.Add(new { MediaType = "Facebook", Url = s.FacebookUrl.URL });
                      if (s.TwitterUrl != null)
                          d.SocialMediaUrls.Add(new { MediaType = "Twitter", Url = s.TwitterUrl.URL });
                      if (s.GooglePlusUrl != null)
                          d.SocialMediaUrls.Add(new { MediaType = "Google+", Url = s.GooglePlusUrl.URL });
                      if (s.LinkedInUrl != null)
                          d.SocialMediaUrls.Add(new { MediaType = "LinkedIn", Url = s.LinkedInUrl.URL });
                      if (s.BlogUrl != null)
                          d.SocialMediaUrls.Add(new { MediaType = "Blog", Url = s.BlogUrl.URL });
                      if (s.WebsiteUrl != null)
                          d.SocialMediaUrls.Add(new { MediaType = "Website", Url = s.WebsiteUrl.URL });
                  });

            this.CreateMap<UserSettings, UserSettingsViewModel>()
                .ForMember(p => p.AccountId, opt => opt.MapFrom(c => c.AccountID))
                .ForMember(p => p.UserId, opt => opt.MapFrom(c => c.UserID))
                .ForMember(p => p.CountryId, opt => opt.MapFrom(c => c.CountryID))
                .ForMember(p => p.EmailId, opt => opt.MapFrom(c => c.EmailID))
                .ForMember(p => p.ItemsPerPage, opt => opt.MapFrom(c => c.ItemsPerPage))
                .ForMember(p => p.TimeZone, opt => opt.MapFrom(c => c.TimeZone))
                .ForMember(p => p.UserSettingId, opt => opt.MapFrom(c => c.Id))
                .ForMember(c => c.IsIncludeSignature, opt => opt.MapFrom(c => c.IsIncludeSignature));

            this.CreateMap<Form, FormViewModel>()
                .ForMember(p => p.FormId, opt => opt.MapFrom(c => c.Id))
                .ForMember(p => p.FormFields, opt => opt.MapFrom(p => Mapper.Map<IList<FormField>, IList<FormFieldViewModel>>(p.FormFields)))
                .ForMember(a => a.TagsList, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<Tag>, IEnumerable<TagViewModel>>(c.Tags)));


            this.CreateMap<FormField, FormFieldViewModel>()
                .ForMember(f => f.FormFieldId, opt => opt.MapFrom(f => f.FormFieldId))
                .ForMember(f => f.FieldId, opt => opt.MapFrom(f => f.Id))
                .ForMember(f => f.FieldInputTypeId, opt => opt.MapFrom(f => f.FieldInputTypeId));

            this.CreateMap<Field, FieldViewModel>()
               .ForMember(f => f.FieldId, opt => opt.MapFrom(f => f.Id))
                //.ForMember(f => f.FieldInputTypeId, opt => opt.MapFrom(f => f.FieldInputTypeId))
                //.ForMember(f => f.IsCustomField, opt => opt.MapFrom(f => f.IsCustomField))
               .AfterMap((s, d) =>
               {
               });

            this.CreateMap<FormSubmission, FormSubmissionEntryViewModel>()
               .ForMember(f => f.FormSubmissionId, opt => opt.MapFrom(f => f.Id));

            this.CreateMap<LeadScoreRule, LeadScoreRuleViewModel>()
                    .ForMember(t => t.LeadScoreRuleID, opt => opt.MapFrom(t => t.Id))
                    .ForMember(t => t.ConditionDescription, opt => opt.MapFrom(t => t.ConditionDescription))
                    .ForMember(t => t.ConditionName, opt => opt.MapFrom(t => t.Condition == null ? "" : t.Condition.Name))
                    .ForMember(t => t.CategoryName, opt => opt.MapFrom(t => t.Category == null ? "" : t.Category.Name))
                    .ForMember(t => t.Category, opt => opt.MapFrom(t => t.Category))
                    .ForMember(t => t.Condition, opt => opt.MapFrom(t => t.Condition))
                    .ForMember(t => t.LeadScoreConditionValues, opt => opt.Ignore())
                    .AfterMap((s, d) =>
                    {
                        d.LeadScoreConditionValues = Mapper.Map<IEnumerable<LeadScoreConditionValue>, IEnumerable<LeadScoreConditionValueViewModel>>(s.LeadScoreConditionValues);
                    });

            this.CreateMap<LeadScoreConditionValue, LeadScoreConditionValueViewModel>();

            this.CreateMap<DropdownValueViewModel, CustomFieldValueOptionViewModel>()
                .ForMember(c => c.CustomFieldId, opt => opt.MapFrom(d => d.DropdownID))
                .ForMember(c => c.CustomFieldValueOptionId, opt => opt.MapFrom(d => d.DropdownValueID))
                .ForMember(c => c.IsDeleted, opt => opt.MapFrom(d => d.IsActive))
                .ForMember(c => c.Value, opt => opt.MapFrom(d => d.DropdownValue));

            this.CreateMap<Campaign, CampaignEntryViewModel>()
                   .ForMember(t => t.Id, opt => opt.MapFrom(t => t.Id))
                   .ForMember(t => t.Name, opt => opt.MapFrom(t => t.Name))
                   .ForMember(t => t.ComplaintRate, opt => opt.MapFrom(t => t.ComplaintCount));

            this.CreateMap<Form, FormEntryViewModel>()
                   .ForMember(t => t.Id, opt => opt.MapFrom(t => t.Id))
                   .ForMember(t => t.Name, opt => opt.MapFrom(t => t.Name));

            this.CreateMap<DropdownValue, DropdownValueViewModel>()
                .ForMember(f => f.DropdownValueID, opt => opt.MapFrom(t => t.Id))
                .ForMember(t => t.DropdownValue, opt => opt.MapFrom(t => t.Value));

            this.CreateMap<User, UserEntryViewModel>()
                   .ForMember(t => t.Id, opt => opt.MapFrom(t => t.Id))
                   .ForMember(t => t.Name, opt => opt.MapFrom(t => t.FirstName));

            this.CreateMap<LeadAdapterAndAccountMap, LeadAdapterViewModel>()
                    .ForMember(t => t.LeadAdapterAndAccountMapId, opt => opt.MapFrom(t => t.Id))
                    .ForMember(t => t.LeadAdapterType, opt => opt.MapFrom(c => c.LeadAdapterTypeID))
                    .ForMember(t => t.LeadAdapterName, opt => opt.MapFrom(c => c.LeadAdapterTypeID))
                    .ForMember(t => t.LastProcessed, opt => opt.MapFrom(c => c.LastProcessed))
                    .ForMember(t => t.LeadAdapterErrorStatusID, opt => opt.MapFrom(c => c.LeadAdapterErrorStatusID))
                    .ForMember(t => t.ServiceStatusMessage, opt => opt.MapFrom(c => c.ServiceStatusMessage == null ? string.Empty : c.ServiceStatusMessage))
                    .ForMember(t => t.LeadAdapterErrorName, opt => opt.MapFrom(c => c.LeadAdapterErrorName == null ? string.Empty : c.LeadAdapterErrorName))
                    .ForMember(t => t.Name, opt => opt.MapFrom(c => c.LeadAdapterType))
                    .ForMember(a => a.TagsList, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<Tag>, IEnumerable<TagViewModel>>(c.Tags)))
            .AfterMap((s, d) =>
            {
                if (s.FacebookLeadAdapter != null)
                {
                    d.PageAccessToken = s.FacebookLeadAdapter.PageAccessToken;
                    d.AddID = s.FacebookLeadAdapter.AddID;
                    d.PageID = s.FacebookLeadAdapter.PageID;
                    d.FacebookLeadAdapterID = s.FacebookLeadAdapter.Id;
                    d.FacebookLeadAdapterName = s.FacebookLeadAdapter.Name;
                }
            });

            this.CreateMap<LeadAdapterJobLogs, ViewLeadAdapterViewModel>()
                  .ForMember(t => t.LeadAdapterJobStatus, opt => opt.MapFrom(c => c.LeadAdapterJobStatusID))
                  .ForMember(t => t.LeadAdapterJobID, opt => opt.MapFrom(c => c.Id))
                  .ForMember(t => t.ImportDate, opt => opt.MapFrom(c => c.CreatedDateTime));

            this.CreateMap<LeadAdapterJobLogDetails, LeadAdapterJobLogDeailsViewModel>()
               .ForMember(t => t.LeadAdapterRecordStatus, opt => opt.MapFrom(c => c.LeadAdapterRecordStatus))
               .ForMember(t => t.Remarks, opt => opt.MapFrom(c => c.Remarks))
                .ForMember(t => t.RowData, opt => opt.MapFrom(c => c.RowData))
               .ForMember(t => t.CreatedDateTime, opt => opt.MapFrom(c => c.CreatedDateTime));

            this.CreateMap<LeadAdapterJobLogDetails, LeadAdapterSubmittedDataViewModel>()
             .ForMember(t => t.JobLogID, opt => opt.MapFrom(c => c.Id))
             .ForMember(t => t.SubmittedData, opt => opt.MapFrom(c => c.RowData));

            this.CreateMap<ImportData, ImportListViewModel>()
                   .ForMember(t => t.LeadAdapterAndAccountMapID, opt => opt.MapFrom(t => t.LeadAdapterAndAccountMapID))
                   .ForMember(t => t.LeadAdapterJobLogID, opt => opt.MapFrom(t => t.Id))
                   .ForMember(t => t.LeadAdapterJobStatus, opt => opt.MapFrom(t => ((LeadAdapterJobStatus)t.LeadAdapterJobStatusID).ToString()))
                    .ForMember(t => t.CreatedDateTime, opt => opt.MapFrom(c => c.CreatedDateTime));

            this.CreateMap<UserActivityLog, UserActivityViewModel>()
                .ForMember(c => c.EntityId, opt => opt.MapFrom(c => c.EntityID))
                .ForMember(c => c.LogDate, opt => opt.MapFrom(c => c.LogDate))
                .ForMember(c => c.ModuleID, opt => opt.MapFrom(c => c.ModuleID))
                .ForMember(c => c.ModuleName, opt => opt.MapFrom(c => c.Module.ModuleName))
                .ForMember(c => c.User, opt => opt.MapFrom(c => Mapper.Map<User, UserViewModel>(c.User)))
                .ForMember(c => c.UserActivityID, opt => opt.MapFrom(c => c.UserActivityID))
                .ForMember(c => c.UserActivityLogID, opt => opt.MapFrom(c => c.UserActivityLogID))
                .ForMember(c => c.UserActivityName, opt => opt.MapFrom(c => c.UserActivity.ActivityName))
                .ForMember(c => c.UserID, opt => opt.MapFrom(c => c.UserID));

            #region Opportunities to OpportunitiesViewModel
            this.CreateMap<Opportunity, OpportunityViewModel>()
               .ForMember(p => p.OpportunityID, opt => opt.MapFrom(c => c.Id))
               .ForMember(p => p.ExpectedCloseDate, opt => opt.MapFrom(c => c.ExpectedClosingDate))
               .ForMember(p => p.PeopleInvolved, opt => opt.Ignore())
               .ForMember(p => p.Contacts, opt => opt.Ignore())
               .ForMember(a => a.OpportunityTags, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<Tag>, IEnumerable<TagViewModel>>(c.OpportunityTags)))
               .AfterMap((s, d) =>
                    {
                        if (s.PeopleInvolved != null)
                        {
                            IList<PeopleInvolvedViewModel> entrties = new List<PeopleInvolvedViewModel>();
                            foreach (PeopleInvolved pep in s.PeopleInvolved)
                            {
                                PeopleInvolvedViewModel poeple = Mapper.Map<PeopleInvolved, PeopleInvolvedViewModel>(pep);
                                poeple.IsInEditMode = false;
                                entrties.Add(poeple);
                            }
                            d.PeopleInvolved = entrties;
                        }
                    });

            this.CreateMap<PeopleInvolved, PeopleInvolvedViewModel>()
               .ForMember(p => p.OpportunityRelationMapID, opt => opt.MapFrom(c => c.PeopleInvolvedID));


            #endregion


            #region AdvancedSearch
            this.CreateMap<SearchDefinition, AdvancedSearchViewModel>()
               .ForMember(p => p.SearchDefinitionID, opt => opt.MapFrom(c => c.Id))
               .ForMember(p => p.SearchDefinitionName, opt => opt.MapFrom(c => c.Name))
               .ForMember(p => p.IsFavoriteSearch, opt => opt.MapFrom(c => c.IsFavoriteSearch))
               .ForMember(p => p.IsPreConfiguredSearch, opt => opt.MapFrom(c => c.IsPreConfiguredSearch))
               .ForMember(p => p.SearchPredicateTypeID, opt => opt.MapFrom(c => (short)c.PredicateType))
               .ForMember(p => p.SearchFilters, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<SearchFilter>, IEnumerable<FilterViewModel>>(c.Filters)))
               .ForMember(p => p.TagsList, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<Tag>, IEnumerable<TagViewModel>>(c.TagsList)));

            this.CreateMap<SearchFilter, FilterViewModel>()
               .ForMember(p => p.SearchFilterID, opt => opt.MapFrom(c => c.SearchFilterId))
               .ForMember(p => p.SearchQualifierTypeID, opt => opt.MapFrom(c => (short)c.Qualifier))
                //.ForMember(p => p.InputTypeId, opt => opt.MapFrom(c => c.FieldOptionTypeId.HasValue ? Convert.ToByte(c.FieldOptionTypeId.Value) : default(byte)))
                //.ForMember(p => p.IsCustomField, opt => opt.MapFrom(c => c.AccountID.HasValue ? true : false));
                //.ForMember(p => p.FieldId, opt => opt.MapFrom(c => c.Field));
               .AfterMap((s, d) =>
               {
                   if (s.Field == ContactFields.DropdownField)
                   {
                       d.FieldId = s.DropdownValueId.Value;
                   }
                   else
                       d.FieldId = (int)s.Field;

                   var value = default(byte);
                   if (s.FieldOptionTypeId.HasValue)
                       value = Convert.ToByte(s.FieldOptionTypeId.Value);
                   d.InputTypeId = value;
               });

            this.CreateMap<AVColumnPreferences, AVColumnPreferenceViewModel>();

            #endregion


            #region Workflow to WorkflowDb

            this.CreateMap<Workflow, WorkFlowViewModel>()
              .ForMember(c => c.WorkflowID, opt => opt.MapFrom(c => c.Id))
              .ForMember(c => c.Status, opt => opt.MapFrom(c => c.StatusName))
              .ForMember(a => a.RemoveFromWorkflows, opt => opt.Ignore())
              .AfterMap((s, d) =>
              {
                  if (!string.IsNullOrEmpty(s.RemovefromWorkflows))
                      d.RemoveFromWorkflows = s.RemovefromWorkflows.Split(',').Select(short.Parse);

              });

            this.CreateMap<BaseWorkflowAction, BaseWorkflowActionViewModel>()
                .Include<WorkflowLeadScoreAction, WorkflowLeadScoreActionViewModel>()
                .Include<WorkflowCampaignAction, WorkflowCampaignActionViewModel>()
                .Include<WorkflowTagAction, WorkflowTagActionViewModel>()
                .Include<WorkflowLifeCycleAction, WorkflowLifeCycleActionViewModel>()
                .Include<WorkflowUserAssignmentAction, WorkflowUserAssignmentActionViewModel>()
                .Include<WorkflowNotifyUserAction, WorkflowNotifyUserActionViewModel>()
                .Include<WorkflowContactFieldAction, WorkflowContactFieldActionViewModel>()
                .Include<WorkflowTextNotificationAction, WorkflowTextNotificationActionViewModel>()
                .Include<WorkflowTimerAction, WorkflowTimerActionViewModel>()
                .Include<WorkflowEmailNotificationAction, WorkflowEmailNotifyActionViewModel>()
                .Include<TriggerWorkflowAction, TriggerWorkflowActionViewModel>();

            this.CreateMap<WorkflowAction, WorkflowActionViewModel>();

            this.CreateMap<WorkflowLeadScoreAction, WorkflowLeadScoreActionViewModel>();
            this.CreateMap<WorkflowEmailNotificationAction, WorkflowEmailNotifyActionViewModel>();
            this.CreateMap<WorkflowCampaignAction, WorkflowCampaignActionViewModel>()
                  .ForMember(a => a.CampaignLinks, opt => opt.MapFrom(c => c.Links));

            this.CreateMap<WorkflowCampaignActionLink, WorkflowCampaignActionLinkViewModel>()
                  .ForMember(c => c.CampaignLinkId, opt => opt.MapFrom(c => c.LinkID));


            this.CreateMap<WorkflowTimerAction, WorkflowTimerActionViewModel>()
                  .ForMember(s => s.RunAt, opt => opt.Ignore())
                  .AfterMap((s, d) =>
                  {
                      if (s.RunAt.HasValue)
                      {
                          if (s.TimerType == TimerType.TimeDelay)
                              d.RunAt = DateTime.Now.Date + s.RunAt.Value;
                          else if (s.TimerType == TimerType.Date)
                              d.RunAtTime = DateTime.Now.Date + s.RunAt.Value;
                      }
                  });
            this.CreateMap<TriggerWorkflowAction, TriggerWorkflowActionViewModel>();
            this.CreateMap<WorkflowTagAction, WorkflowTagActionViewModel>();
            this.CreateMap<WorkflowLifeCycleAction, WorkflowLifeCycleActionViewModel>();
            this.CreateMap<RoundRobinContactAssignment, RoundRobinContactAssignmentViewModel>()
                 .ForMember(f => f.UserID, opt => opt.Ignore())
                 .ForMember(f => f.UserIds, opt => opt.Ignore())
                 .ForMember(f => f.IsRoundRobinAssignment, opt => opt.MapFrom(m => m.IsRoundRobinAssignment ? "1" : "0"))
                 .AfterMap((s, d) =>
                 {
                     if (!string.IsNullOrEmpty(s.UserID) && s.IsRoundRobinAssignment)
                         d.UserIds = s.UserID.Split(',').Select(se => int.Parse(se));
                     else if (!string.IsNullOrEmpty(s.UserID) && !s.IsRoundRobinAssignment)
                         d.UserID = s.UserID;
                 });
            this.CreateMap<WorkflowUserAssignmentAction, WorkflowUserAssignmentActionViewModel>()
                .ForMember(f => f.UserName, opt => opt.Ignore())
                .ForMember(f => f.RoundRobinContactAssignments, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<RoundRobinContactAssignment>, IEnumerable<RoundRobinContactAssignmentViewModel>>(c.RoundRobinContactAssignments)));
            this.CreateMap<WorkflowNotifyUserAction, WorkflowNotifyUserActionViewModel>()
                  .ForMember(f => f.UserName, opt => opt.Ignore())
                  .AfterMap((s, d) =>
                  {
                      if (s.UserID.IsAny())
                          d.UserIds = s.UserID;
                      if (s.UserNames.IsAny())
                          d.UserName = string.Join(", ", s.UserNames);
                  })
                  .AfterMap((s, d) =>
                  {
                      if (s.NotificationFieldID.IsAny())
                          d.NotificationFieldIds = s.NotificationFieldID;
                  });
            this.CreateMap<WorkflowContactFieldAction, WorkflowContactFieldActionViewModel>();
            this.CreateMap<WorkflowTextNotificationAction, WorkflowTextNotificationActionViewModel>();
            this.CreateMap<WorkflowTrigger, WorkflowTriggerViewModel>();
            this.CreateMap<WorkflowTriggerType, TriggerCategoryTypeViewModel>();
            this.CreateMap<ParentWorkflow, ParentWorkflowViewModel>();
            #endregion


            this.CreateMap<CustomFieldTab, CustomFieldTabViewModel>()
                .ForMember(c => c.CustomFieldTabId, opt => opt.MapFrom(c => c.Id))
                .ForMember(c => c.Sections, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<CustomFieldSection>, IEnumerable<CustomFieldSectionViewModel>>(c.Sections)));

            this.CreateMap<CustomFieldSection, CustomFieldSectionViewModel>()
                .ForMember(c => c.CustomFieldSectionId, opt => opt.MapFrom(c => c.Id))
                .ForMember(c => c.CustomFields, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<CustomField>, IEnumerable<CustomFieldViewModel>>(c.CustomFields)));

            this.CreateMap<CustomField, CustomFieldViewModel>()
                .ForMember(c => c.FieldId, opt => opt.MapFrom(c => c.Id))
                .ForMember(c => c.ValueOptions, opt => opt.MapFrom(c => Mapper.Map<IEnumerable<FieldValueOption>, IEnumerable<CustomFieldValueOptionViewModel>>(c.ValueOptions)));

            this.CreateMap<FieldValueOption, CustomFieldValueOptionViewModel>()
                .ForMember(c => c.CustomFieldValueOptionId, opt => opt.MapFrom(c => c.Id))
                .ForMember(c => c.CustomFieldId, opt => opt.MapFrom(c => c.FieldId));

            this.CreateMap<ContactCustomField, ContactCustomFieldMapViewModel>()
                .ForMember(c => c.ContactCustomFieldMapId, opt => opt.MapFrom(c => c.ContactCustomFieldMapId));


            this.CreateMap<Role, RoleViewModel>()
                .ForMember(c => c.RoleId, opt => opt.MapFrom(c => c.Id))
                .ForMember(c => c.RoleName, opt => opt.MapFrom(c => c.RoleName));

            this.CreateMap<Module, ModuleViewModel>()
                .ForMember(c => c.ModuleId, opt => opt.MapFrom(c => c.Id))
                .ForMember(c => c.ModuleName, opt => opt.MapFrom(c => c.ModuleName))
                .ForMember(c => c.SubModules, opt => opt.MapFrom(c => new List<Module>()));
            this.CreateMap<Email, SendMailViewModel>()
                .ForMember(e => e.From, opt => opt.MapFrom(e => e.EmailId))
                .ForMember(e => e.ProviderID, opt => opt.MapFrom(e => e.ServiceProviderID))
                .ForMember(e => e.CC, opt => opt.Ignore())
                .ForMember(e => e.Body, opt => opt.Ignore())
                .ForMember(e => e.BCC, opt => opt.Ignore())
                .ForMember(e => e.Attachments, opt => opt.Ignore())
                .ForMember(e => e.Subject, opt => opt.Ignore());

            this.CreateMap<State, FieldValueOption>()
                .ForMember(f => f.Value, opt => opt.MapFrom(s => s.Name + " (" + s.Code + ")"));

            this.CreateMap<Country, FieldValueOption>()
                .ForMember(f => f.Value, opt => opt.MapFrom(s => s.Name));

            this.CreateMap<Owner, FieldValueOption>()
                .ForMember(f => f.Value, opt => opt.MapFrom(s => s.OwnerName))
                .ForMember(f => f.Id, opt => opt.MapFrom(s => s.OwnerId));

            this.CreateMap<DropdownValueViewModel, FieldValueOption>()
               .ForMember(f => f.Value, opt => opt.MapFrom(d => d.DropdownValue))
               .ForMember(f => f.Id, opt => opt.MapFrom(d => d.DropdownValueID));

            this.CreateMap<DropdownValueViewModel, FieldViewModel>()
                .ForMember(e => e.FieldId, opt => opt.MapFrom(e => e.DropdownID));

            this.CreateMap<WebAnalyticsProvider, WebAnalyticsProviderViewModel>()
                .ForMember(w => w.WebAnalyticsProviderID, opt => opt.MapFrom(w => w.Id));

            this.CreateMap<CalenderTimeSlot, CalenderTimeSlotViewModel>()
                .ForMember(c => c.title, opt => opt.MapFrom(c => c.title));
            //.ForMember(c => c.start, opt => opt.MapFrom(c => ("new Date('" + String.Format("{0:yyyy/M/d HH:mm}", c.start)) + "')"))
            //.ForMember(c => c.end, opt => opt.MapFrom(c => ("new Date('" + String.Format("{0:yyyy/M/d HH:mm}", c.end)) + "')"));
            this.CreateMap<UserSocialMediaPosts, UserSocialMediaPostsViewModel>();

            this.CreateMap<CRMOutlookSync, CRMOutlookSyncViewModel>()
                .ForMember(o => o.OutlookSyncId, opt => opt.MapFrom(o => o.OutlookSyncId));

            this.CreateMap<WebVisit, WebVisitViewModel>();

            this.CreateMap<ContactWebVisitSummary, ContactWebVisitSummaryViewModel>()
                .ForMember(c => c.Source, opt => opt.MapFrom(c => c.Source ?? ""))
                .ForMember(c => c.Location, opt => opt.MapFrom(c => c.Location ?? ""))
                .ForMember(c => c.Page1, opt => opt.MapFrom(c => c.Page1 ?? ""))
                .ForMember(c => c.Page2, opt => opt.MapFrom(c => c.Page2 ?? ""))
                .ForMember(c => c.Page3, opt => opt.MapFrom(c => c.Page3 ?? ""));


            this.CreateMap<SeedEmail, SeedEmailViewModel>()
                .ForMember(o => o.SeedID, opt => opt.MapFrom(o => o.Id));
            this.CreateMap<ThirdPartyClient, ThirdPartyClientViewModel>()
                .ForMember(o => o.ID, opt => opt.MapFrom(o => o.ID));
            this.CreateMap<Account, AccountListViewModel>()
                .IgnoreAllUnmapped()
                .ForMember(o => o.Id, opt => opt.MapFrom(o => o.Id))
                .ForMember(o => o.Name, opt => opt.MapFrom(o => o.AccountName));
            this.CreateMap<AccountSettings, AccountSettingsViewModel>()
                .ForMember(o => o.AccountID, opt => opt.MapFrom(o => o.Id));

            this.CreateMap<AccountHealthReport, ReportDataViewModel>();

            this.CreateMap<ContactAccountGroup, ContactGroup>();

            this.CreateMap<MarketingMessage, MarketingMessagesViewModel>();
            this.CreateMap<MarketingMessageAccountMap, MarketingMessageAccountMapViewModel>();
            this.CreateMap<MarketingMessageContentMap, MarketingMessageContentMapViewModel>();

            this.CreateMap<Subscription, SubscriptionViewModel>();

            this.CreateMap<ApplicationTourDetails, ApplicationTourViewModel>()
                .AfterMap((s, d) =>
                {
                    if (s.Division != null)
                        d.DivisionName = s.Division.DivisionName;
                    if (s.Section != null)
                        d.SectionName = s.Section.SectionName;
                });

            this.CreateMap<ReportedCoupons, ReportedCouponsViewModel>();

            #region SuppressionList
            this.CreateMap<SuppressedEmail, SuppressedEmailViewModel>()
                .ForMember(o => o.SuppressedEmailID, opt => opt.MapFrom(o => o.Id));
            this.CreateMap<SuppressedDomain, SuppressedDomainViewModel>()
                .ForMember(o => o.SuppressedDomainID, opt => opt.MapFrom(o => o.Id));
            #endregion

            this.CreateMap<ImportColumnMappings, ImportDataViewModel>();

            this.CreateMap<APILeadSubmission, APILeadSubmissionViewModel>();

            this.CreateMap<TourByContactReportInfo, TourByContactsViewModel>();

            this.CreateMap<NightlyStatusReport, NightlyStatusReportViewModel>();

            //this.CreateMap<DashboardPieChartDetails, DropDownValue>()
            //    .ForMember(d => d.DropdownValueName, odt => odt.MapFrom(o => o.DropdownValue))
            //    .ForMember(d => d.DropdownValue, odt => odt.MapFrom(o => o.DropdownValueID))
            //    .ForMember(d => d.Potential, odt => odt.MapFrom(o => o.Potential));
                
        }
    }
}
