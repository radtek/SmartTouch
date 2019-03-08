using AutoMapper;
using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using Newtonsoft.Json;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.CustomFields;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmartTouch.CRM.JobProcessor
{
    public class APILeadSubmissionProcessor : CronJobProcessor
    {
        readonly IContactService contactService;
        readonly IFormService formService;
        readonly ICachingService cachingService;
        readonly ICustomFieldService customFieldService;
        readonly IFindSpamService spamService;
        readonly IContactRepository contactRepository;
        readonly IUserRepository userRepository;
        readonly IFormRepository formRepository;
        readonly IDropdownRepository dropdownRepository;
        private static bool IsAPILeadProcessing;


        public APILeadSubmissionProcessor(CronJobDb cronJob, JobService jobService, string apiLeadSubmissionProcessorCacheName)
            : base(cronJob, jobService, apiLeadSubmissionProcessorCacheName)
        {
            this.contactService = IoC.Container.GetInstance<IContactService>();
            this.formService = IoC.Container.GetInstance<IFormService>();
            this.cachingService = IoC.Container.GetInstance<ICachingService>();
            this.customFieldService = IoC.Container.GetInstance<ICustomFieldService>();
            this.contactRepository = IoC.Container.GetInstance<IContactRepository>();
            this.userRepository = IoC.Container.GetInstance<IUserRepository>();
            this.formRepository = IoC.Container.GetInstance<IFormRepository>();
            this.dropdownRepository = IoC.Container.GetInstance<IDropdownRepository>();
            this.spamService = IoC.Container.GetInstance<IFindSpamService>();
        }

        protected override void Execute()
        {
            Logger.Current.Informational("Entering into APILeadSubmission processor");
            try
            {
                if (!IsAPILeadProcessing)
                {
                    IsAPILeadProcessing = true;
                    GetAPILeadSubmissionDataResponse response = new GetAPILeadSubmissionDataResponse();
                    response = contactService.GetAPILeadSubMissionData();
                    string spamRemarks = string.Empty;
                    while (response != null && response.APILeadSubmissionViewModel != null)
                    {
                        try
                        {
                            int contactID = 0;
                            short roleId = userRepository.GettingRoleIDByUserID(response.APILeadSubmissionViewModel.OwnerID);
                            PersonViewModel viewModel = JsonConvert.DeserializeObject<PersonViewModel>(response.APILeadSubmissionViewModel.SubmittedData);
                            viewModel.AccountID = response.APILeadSubmissionViewModel.AccountID;
                            viewModel.OwnerId = response.APILeadSubmissionViewModel.OwnerID;
                            viewModel.LastUpdatedBy = response.APILeadSubmissionViewModel.OwnerID;
                            viewModel.FirstName = !string.IsNullOrEmpty(viewModel.FirstName)? viewModel.FirstName.Trim(): viewModel.FirstName;
                            viewModel.LastName = !string.IsNullOrEmpty(viewModel.LastName) ?viewModel.LastName.Trim(): viewModel.LastName;

                            if (viewModel.Phones.IsAny())
                            {
                                viewModel.Phones.Each(p =>
                                {
                                    p.Number = !string.IsNullOrEmpty(p.Number) ? p.Number.Trim() : p.Number;
                                });
                            }

                            if (viewModel.CustomFields.IsAny())
                            {
                                viewModel.CustomFields.Each(cm =>
                                {
                                    cm.Value = !string.IsNullOrEmpty(cm.Value) ? cm.Value.Trim() : cm.Value;
                                });
                            }

                            Dictionary<string, string> fields = GetContactFields(viewModel);
                            bool isSpam = spamService.SpamCheck(fields, viewModel.AccountID, response.APILeadSubmissionViewModel.IPAddress, 0,false, out spamRemarks);
                            if (isSpam && spamRemarks == "Invalid MobilePhone")
                            {
                                isSpam = false;
                                spamRemarks = "";
                            }
                            if (!isSpam)
                            {
                                #region NotSpam
                                var dropdownValues = dropdownRepository.FindAll("", 10, 1, viewModel.AccountID);
                                IEnumerable<DropdownViewModel> dropdownViewModel = Mapper.Map<IEnumerable<Dropdown>, IEnumerable<DropdownViewModel>>(dropdownValues);

                                string primaryEmail = viewModel.Emails.IsAny() ? viewModel.Emails.Where(e => e.IsPrimary).Select(s => s.EmailId).FirstOrDefault() : "";

                                if (string.IsNullOrEmpty(viewModel.FirstName) && string.IsNullOrEmpty(viewModel.LastName) && !string.IsNullOrEmpty(primaryEmail) && !IsValidEmail(primaryEmail))
                                {
                                    contactRepository.UpdateAPILeadSubmissionData(null, (byte)SubmittedFormStatus.Fail, "Bad Email", response.APILeadSubmissionViewModel.APILeadSubmissionID);
                                    return;
                                }

                                var duplicatesResponse = contactService.CheckIfDuplicate(new CheckContactDuplicateRequest() { PersonVM = viewModel });

                                var leadSourceField = viewModel.SelectedLeadSource.IsAny() ? viewModel.SelectedLeadSource.FirstOrDefault() : null;
                                var communityField = viewModel.Communities.IsAny() ? viewModel.Communities.FirstOrDefault() : null;
                                var leadSourceDropDown = dropdownViewModel.Where(c => c.DropdownID == (byte)DropdownFieldTypes.LeadSources).FirstOrDefault().DropdownValuesList;
                                var leadSourceDropDownValue = new DropdownValueViewModel();
                                if (leadSourceField != null)
                                {
                                    Logger.Current.Verbose("Attempting to fetch Lead Source drop down value submitted");
                                    leadSourceDropDownValue = leadSourceDropDown.Where(e => e.DropdownValueID == leadSourceField.DropdownValueID).FirstOrDefault();
                                }

                                if (leadSourceDropDownValue == null || leadSourceField == null)
                                {
                                    Logger.Current.Verbose("Attempting to fetch Next Gen account default Lead Source drop down value");
                                    leadSourceDropDownValue = leadSourceDropDown.Where(e => e.IsDefault).FirstOrDefault();
                                    if (leadSourceDropDownValue == null)
                                    {
                                        Logger.Current.Verbose("Attempting to fetch First Lead Source drop down value");
                                        leadSourceDropDownValue = leadSourceDropDown.FirstOrDefault();
                                        if (leadSourceDropDownValue == null)
                                            throw new UnsupportedOperationException("[|The accound do not have the specified or any lead source configured. Please contact administrator|]");
                                    }
                                }

                                DropdownValueViewModel leadSourceDropdownViewModel = new DropdownValueViewModel()
                                {
                                    AccountID = viewModel.AccountID,
                                    DropdownID = (byte)ContactFields.LeadSource,
                                    DropdownValue = leadSourceDropDownValue.DropdownValue,
                                    DropdownValueID = leadSourceDropDownValue.DropdownValueID
                                };

                                var communityDropdownViewModel = new DropdownValueViewModel();
                                if (communityField != null)
                                {
                                    IEnumerable<DropdownValueViewModel> communities = dropdownViewModel.Where(c => c.DropdownID == (byte)DropdownFieldTypes.Community).FirstOrDefault().DropdownValuesList;
                                    DropdownValueViewModel selectedcommunity = communities.Where(x => x.DropdownValueID == communityField.DropdownValueID).FirstOrDefault();
                                    if (selectedcommunity != null && selectedcommunity.DropdownValueID > 0)
                                    {
                                        communityDropdownViewModel = new DropdownValueViewModel()
                                        {
                                            AccountID = viewModel.AccountID,
                                            DropdownID = (byte)ContactFields.Community,
                                            DropdownValue = selectedcommunity.DropdownValue,
                                            DropdownValueID = selectedcommunity.DropdownValueID
                                        };
                                    }
                                        //throw new UnsupportedOperationException("[|Community field is deleted, Please contact adminstrator|]");

                                
                                }

                                if (duplicatesResponse != null && duplicatesResponse.Contacts.IsAny())
                                {
                                    Logger.Current.Informational("Entering into APILeadSubmission Updation");
                                    viewModel.ContactID = duplicatesResponse.Contacts.FirstOrDefault().Id;
                                    viewModel.ContactSource = Entities.ContactSource.API;
                                    var existingContact = duplicatesResponse.Contacts.FirstOrDefault() as Person;

                                    var leadSource = existingContact.LeadSources != null ? existingContact.LeadSources.Select(e => e.Id).ToList() : new List<short>();
                                    PersonViewModel existingContactViewModel = Mapper.Map<Person, PersonViewModel>(existingContact);
                                    if (leadSource.IndexOf(leadSourceDropdownViewModel.DropdownValueID) == -1)
                                    {
                                        existingContactViewModel.SelectedLeadSource = existingContactViewModel.SelectedLeadSource ?? new List<DropdownValueViewModel>();
                                        existingContactViewModel.SelectedLeadSource = existingContactViewModel.SelectedLeadSource.Append(leadSourceDropdownViewModel);
                                    }

                                    var community = existingContact.Communities != null ? existingContact.Communities.Select(e => e.Id).ToList() : new List<short>();
                                    if (communityDropdownViewModel != null && community.IndexOf(communityDropdownViewModel.DropdownValueID) == -1)
                                    {
                                        existingContactViewModel.Communities = existingContactViewModel.Communities ?? new List<DropdownValueViewModel>();
                                        existingContactViewModel.Communities = existingContactViewModel.Communities.Append(communityDropdownViewModel);
                                    }

                                    PersonViewModel updateViewModel = GetUpdatedPersonData(existingContactViewModel, viewModel);
                                    updateViewModel.IncludeInReports = true;
                                    UpdatePersonResponse updatePersonResult = contactService.UpdatePerson(new UpdatePersonRequest()
                                    {
                                        PersonViewModel = updateViewModel,
                                        AccountId = response.APILeadSubmissionViewModel.AccountID,
                                        RequestedBy = response.APILeadSubmissionViewModel.OwnerID,
                                        RoleId = roleId,
                                        RequestedFrom = RequestOrigin.API
                                    });

                                    if (updatePersonResult.PersonViewModel.ContactID > 0)
                                    {
                                        contactID = updatePersonResult.PersonViewModel.ContactID;
                                        contactRepository.UpdateAPILeadSubmissionData(updatePersonResult.PersonViewModel.ContactID, (byte)SubmittedFormStatus.Completed, "Completed", response.APILeadSubmissionViewModel.APILeadSubmissionID);
                                    }
                                }
                                else
                                {
                                    Logger.Current.Informational("Entering into APILeadSubmission Insertion");
                                    viewModel.FirstContactSource = Entities.ContactSource.API;
                                    viewModel.CreatedBy = response.APILeadSubmissionViewModel.OwnerID;
                                    viewModel.CreatedOn = DateTime.Now.ToUniversalTime();
                                    viewModel.SelectedLeadSource = new List<DropdownValueViewModel>();
                                    viewModel.SelectedLeadSource = viewModel.SelectedLeadSource.Append(leadSourceDropdownViewModel);
                                    viewModel.LastUpdatedOn = DateTime.Now.ToUniversalTime();
                                    if (communityField != null && communityDropdownViewModel.DropdownValueID >0)
                                    {
                                        viewModel.Communities = new List<DropdownValueViewModel>();
                                        viewModel.Communities = viewModel.Communities.Append(communityDropdownViewModel);
                                    }

                                    //if (viewModel.LifecycleStage > 0)
                                    //{
                                        IEnumerable<DropdownValueViewModel> lifecycles = dropdownViewModel.Where(c => c.DropdownID == (byte)DropdownFieldTypes.LifeCycle).FirstOrDefault().DropdownValuesList;
                                        if (!lifecycles.Where(l => l.DropdownValueID == viewModel.LifecycleStage).IsAny())
                                            viewModel.LifecycleStage = lifecycles.Where(l => l.IsDefault).Select(s => s.DropdownValueID).FirstOrDefault();
                                    //}
                                    if (viewModel.Phones.IsAny())
                                    {
                                        IEnumerable<DropdownValueViewModel> phones = dropdownViewModel.Where(c => c.DropdownID == (byte)DropdownFieldTypes.PhoneNumberType).FirstOrDefault().DropdownValuesList;
                                        viewModel.Phones.Each(p =>
                                       {
                                           if (!phones.Where(t => t.DropdownValueID == p.PhoneType).IsAny())
                                               p.PhoneType = phones.Where(t => t.IsDefault).Select(s => s.DropdownValueID).FirstOrDefault();
                                           if (!string.IsNullOrEmpty(p.Number) && (p.Number.Length < 10 || p.Number.Length > 15))
                                              p.IsDeleted = true;

                                       });

                                      viewModel.Phones = viewModel.Phones.Where(p => p.IsDeleted == false).ToList();

                                    }

                                    viewModel.IncludeInReports = true;
                                    InsertPersonResponse personResult = contactService.InsertPerson(new InsertPersonRequest()
                                    {
                                        PersonViewModel = viewModel,
                                        AccountId = response.APILeadSubmissionViewModel.AccountID,
                                        RequestedBy = response.APILeadSubmissionViewModel.OwnerID,
                                        RoleId = roleId,
                                        RequestedFrom = RequestOrigin.API
                                    });

                                    if (personResult.PersonViewModel.ContactID > 0)
                                    {
                                        contactID = personResult.PersonViewModel.ContactID;
                                        contactRepository.UpdateAPILeadSubmissionData(personResult.PersonViewModel.ContactID, (byte)SubmittedFormStatus.Completed, "Completed", response.APILeadSubmissionViewModel.APILeadSubmissionID);
                                    }
                                }
                                if (contactID > 0 && viewModel.SelectedLeadSource.IsAny())
                                {
                                    //for Indexing Contact.
                                    formRepository.ScheduleIndexing(contactID, IndexType.Contacts, true);

                                    if (response.APILeadSubmissionViewModel.FormID != 0)
                                        formService.InsertFormSubmissionEntry(contactID,
                                            new SubmittedFormViewModel() { FormId = response.APILeadSubmissionViewModel.FormID, SubmittedOn = DateTime.UtcNow, SubmittedData = response.APILeadSubmissionViewModel.SubmittedData, AccountId = response.APILeadSubmissionViewModel.AccountID },
                                            viewModel.SelectedLeadSource.FirstOrDefault().DropdownValueID);

                                }
                                #endregion
                            }
                            else
                            {
                                contactRepository.UpdateAPILeadSubmissionData(null, (byte)SubmittedFormStatus.Spam, spamRemarks, response.APILeadSubmissionViewModel.APILeadSubmissionID);
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.Data.Clear();
                            ex.Data.Add("APILeadSubmissinID", response.APILeadSubmissionViewModel.APILeadSubmissionID);
                            Logger.Current.Error("Error While Processing APILeadSubmission", ex);
                            contactRepository.UpdateAPILeadSubmissionData(null, (byte)SubmittedFormStatus.Fail, ex.Message, response.APILeadSubmissionViewModel.APILeadSubmissionID);

                        }
                        finally
                        {
                            response = contactService.GetAPILeadSubMissionData();
                        }
                    }
                    IsAPILeadProcessing = false;
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error while APILeadSubmission", ex);
                IsAPILeadProcessing = false;
            }
            finally
            {
                IsAPILeadProcessing = false;
            }
        }

        private Dictionary<string, string> GetContactFields(PersonViewModel model)
        {
            if (model != null)
            {
                Dictionary<string, string> fields = new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(model.FirstName))
                    fields.Add(((byte)ContactFields.FirstNameField).ToString(), model.FirstName);
                if (!string.IsNullOrEmpty(model.LastName))
                    fields.Add(((byte)ContactFields.LastNameField).ToString(), model.LastName);
                if (model.Emails != null && model.Emails.Where(w => w.IsPrimary && !string.IsNullOrEmpty(w.EmailId)).Any())
                    fields.Add(((byte)ContactFields.PrimaryEmail).ToString(), model.Emails.Where(w => w.IsPrimary && !string.IsNullOrEmpty(w.EmailId)).Select(s => s.EmailId).FirstOrDefault());
                if (model.CustomFields.IsAny())
                    foreach (var field in model.CustomFields)
                        fields.Add(field.CustomFieldId.ToString(), field.Value);

                return fields;
            }
            return new Dictionary<string, string>();
        }

        public bool IsValidEmail(string email)
        {
            string pattern = @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" + @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,24}))$";    //Source: http://msdn.microsoft.com/en-us/library/01escwtf(v=vs.110).aspx
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(email);
        }

        private PersonViewModel GetUpdatedPersonData(PersonViewModel existingModel, PersonViewModel newModel)
        {
            Person person = new Person();
            var dropdownValues = cachingService.GetDropdownValues(newModel.AccountID);

            existingModel.FirstName = !string.IsNullOrEmpty(newModel.FirstName) ? newModel.FirstName : existingModel.FirstName;
            existingModel.LastName = !string.IsNullOrEmpty(newModel.LastName) ? newModel.LastName : existingModel.LastName;
            existingModel.CompanyName = !string.IsNullOrEmpty(newModel.CompanyName) ? newModel.CompanyName : existingModel.CompanyName;
            existingModel.Title = !string.IsNullOrEmpty(newModel.Title) ? newModel.Title : existingModel.Title;

            List<short> phoneTypeIds = newModel.Phones.IsAny()?newModel.Phones.Select(p => p.PhoneType).ToList():new List<short>();
            List<short> dropdownValueTyeIds = new List<short>();
            if (phoneTypeIds.IsAny())
            {
                dropdownValueTyeIds = formRepository.GetDropdownValueTypeIdsByPhoneTypes(phoneTypeIds, newModel.AccountID);
            }

            var mobilePhoneNumber = dropdownValueTyeIds.Contains((short)DropdownValueTypes.MobilePhone) ? newModel.Phones.Select(m => m.Number).FirstOrDefault() : null; 
            var homePhoneNumber = dropdownValueTyeIds.Contains((short)DropdownValueTypes.Homephone) ? newModel.Phones.Select(m => m.Number).FirstOrDefault() : null;
            var workPhoneNumber = dropdownValueTyeIds.Contains((short)DropdownValueTypes.WorkPhone) ? newModel.Phones.Select(m => m.Number).FirstOrDefault() : null;

            var phoneTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.PhoneNumberType)
                 .Select(s => s.DropdownValuesList).FirstOrDefault().Where(d => d.IsActive == true);


            existingModel.Phones = new List<Phone>();

            Logger.Current.Informational("While Updating phones.");

            IEnumerable<Phone> existingPhones = formRepository.GetPhoneFields(existingModel.ContactID);

            Phone mobilePhone = existingPhones.Where(w => w.DropdownValueTypeID == (short)DropdownValueTypes.MobilePhone).FirstOrDefault();
            if (!string.IsNullOrEmpty(mobilePhoneNumber) && !(mobilePhoneNumber.Length < 10 || mobilePhoneNumber.Length > 15))
            {
                string number = mobilePhoneNumber.TrimStart(new char[] { '0', '1' });
                Phone phone = new Phone();
                phone.Number = number;
                phone.AccountID = existingModel.AccountID;
                phone.IsPrimary = mobilePhone != null ? mobilePhone.IsPrimary : (homePhoneNumber == null && workPhoneNumber == null && !existingPhones.IsAny()) ||
                    phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.MobilePhone).Select(c => c.IsDefault).FirstOrDefault();
                phone.PhoneType = phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.MobilePhone).Select(c => c.DropdownValueID).FirstOrDefault();
                phone.PhoneTypeName = phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.MobilePhone).Select(c => c.DropdownValue).FirstOrDefault();
                if (person.IsValidPhoneNumberLength(number))
                    existingModel.Phones.Add(phone);
            }
            else if (mobilePhone != null)
                existingModel.Phones.Add(mobilePhone);

            Phone homePhone = existingPhones.Where(w => w.DropdownValueTypeID == (short)DropdownValueTypes.Homephone).FirstOrDefault();
            if (!string.IsNullOrEmpty(homePhoneNumber) && !(homePhoneNumber.Length < 10 || homePhoneNumber.Length > 15))
            {
                string number = homePhoneNumber.TrimStart(new char[] { '0', '1' });
                Phone phone = new Phone();
                phone.Number = number;
                phone.AccountID = existingModel.AccountID;
                phone.IsPrimary = homePhone != null ? homePhone.IsPrimary : (mobilePhoneNumber == null && workPhoneNumber == null && !existingPhones.IsAny()) ||
                    phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.Homephone).Select(c => c.IsDefault).FirstOrDefault();
                phone.PhoneType = phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.Homephone).Select(c => c.DropdownValueID).FirstOrDefault();
                phone.PhoneTypeName = phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.Homephone).Select(c => c.DropdownValue).FirstOrDefault();
                if (person.IsValidPhoneNumberLength(number))
                    existingModel.Phones.Add(phone);
            }
            else if (homePhone != null)
                existingModel.Phones.Add(homePhone);

            Phone workPhone = existingPhones.Where(w => w.DropdownValueTypeID == (short)DropdownValueTypes.WorkPhone).FirstOrDefault();
            if (!string.IsNullOrEmpty(workPhoneNumber) && !(workPhoneNumber.Length < 10 || workPhoneNumber.Length > 15))
            {
                string number = workPhoneNumber.TrimStart(new char[] { '0', '1' });
                Phone phone = new Phone();
                phone.Number = number;
                phone.AccountID = existingModel.AccountID;
                phone.IsPrimary = workPhone != null ? workPhone.IsPrimary : (mobilePhoneNumber == null && homePhoneNumber == null && !existingPhones.IsAny()) ||
                    phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.WorkPhone).Select(c => c.IsDefault).FirstOrDefault();
                phone.PhoneType = phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.WorkPhone).Select(c => c.DropdownValueID).FirstOrDefault();
                phone.PhoneTypeName = phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.WorkPhone).Select(c => c.DropdownValue).FirstOrDefault();
                if (person.IsValidPhoneNumberLength(number))
                    existingModel.Phones.Add(phone);
            }
            else if (workPhone != null)
                existingModel.Phones.Add(workPhone);

            IEnumerable<Phone> existingNonDefaultPhones = existingPhones.Where(w => w.DropdownValueTypeID != 9 && w.DropdownValueTypeID != 10 && w.DropdownValueTypeID != 11 && !w.IsDeleted);
            if (existingNonDefaultPhones.IsAny())
                existingNonDefaultPhones.Each(e =>
                {
                    if (phoneTypeIds.IsAny())
                    {
                        if (phoneTypeIds.Contains(e.PhoneType))
                        {
                            var nonDefaultPhone = newModel.Phones.Where(p => p.PhoneType == e.PhoneType).FirstOrDefault();//workPhoneNumber.TrimStart(new char[] { '0', '1' });
                            string number = nonDefaultPhone.Number.TrimStart(new char[] { '0', '1' });
                            if (e.Number != number)
                            {
                                e.IsPrimary = false;
                                Phone phone = new Phone();
                                phone.Number = number;
                                phone.AccountID = existingModel.AccountID;
                                phone.IsPrimary = true;
                                phone.PhoneType = nonDefaultPhone.PhoneType;
                                phone.PhoneTypeName = nonDefaultPhone.PhoneTypeName;
                                if (person.IsValidPhoneNumberLength(number.TrimStart(new char[] { '0', '1' })))
                                    existingModel.Phones.Add(phone);

                            }
                                
                        }
                    }
                    existingModel.Phones.Add(e);
                });

            Logger.Current.Informational("While Updating Addresses.");
            existingModel.Addresses = new List<AddressViewModel>();
            var addressLine1 = newModel.Addresses.IsAny()? newModel.Addresses.Select(a => a.AddressLine1).FirstOrDefault():null;
            var addressLine2 = newModel.Addresses.IsAny() ? newModel.Addresses.Select(a => a.AddressLine2).FirstOrDefault():null;
            var city = newModel.Addresses.IsAny() ? newModel.Addresses.Select(a => a.City).FirstOrDefault():null;
            var state = newModel.Addresses.IsAny() ? newModel.Addresses.Select(a => a.State).FirstOrDefault(): null;
            var zip = newModel.Addresses.IsAny() ? newModel.Addresses.Select(a => a.ZipCode).FirstOrDefault() : null;
            var country = newModel.Addresses.IsAny() ? newModel.Addresses.Select(a => a.Country).FirstOrDefault() : null;

            if (addressLine1 != null || addressLine2 != null || city != null || zip != null || country != null || state != null)
            {
                existingModel.AddressTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.AddressType)
                                                .Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);

                AddressViewModel newAddress = new AddressViewModel();
                newAddress.AddressTypeID = existingModel.AddressTypes.SingleOrDefault(a => a.IsDefault).DropdownValueID;
                newAddress.AddressLine1 = addressLine1 != null && !string.IsNullOrEmpty(addressLine1) ? addressLine1 : "";
                newAddress.AddressLine2 = addressLine2 != null && !string.IsNullOrEmpty(addressLine2) ? addressLine2 : "";
                newAddress.City = city != null && !string.IsNullOrEmpty(city) ? city : "";
                if (state != null)
                    newAddress.State = new State() { Code = state.Code };
                else
                    newAddress.State = new State();
                if (country != null)
                    newAddress.Country = new Country() { Code = country.Code };
                else
                    newAddress.Country = new Country();

                var zipCode = zip != null && !string.IsNullOrEmpty(zip) ? zip : "";
                newAddress.ZipCode = zipCode;
                newAddress.IsDefault = true;

                if ((newAddress.State != null && !string.IsNullOrEmpty(newAddress.State.Code)) &&
                    (newAddress.Country == null || string.IsNullOrEmpty(newAddress.Country.Code)))
                {
                    newAddress.Country = new Country();
                    newAddress.Country.Code = newAddress.State.Code.Substring(0, 2);
                }
                existingModel.Addresses.Add(newAddress);
            }

            existingModel.ContactType = Entities.ContactType.Person.ToString();
            existingModel.SecondaryEmails = new List<dynamic>();

            Logger.Current.Informational("While Updating Life Cycle Stage.");
            existingModel.LifecycleStages = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LifeCycle)
             .Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            var defaultLifeCycleType = existingModel.LifecycleStages.SingleOrDefault(a => a.IsDefault);
            if(newModel.LifecycleStage > 0)
            {
                if (!existingModel.LifecycleStages.Where(l => l.DropdownValueID == newModel.LifecycleStage).IsAny())
                    newModel.LifecycleStage = existingModel.LifecycleStages.Where(l => l.IsDefault).Select(s => s.DropdownValueID).FirstOrDefault();
            }
            existingModel.LifecycleStage = newModel.LifecycleStage > 0 ? newModel.LifecycleStage: defaultLifeCycleType.DropdownValueID;

            existingModel.AccountID = newModel.AccountID;
            existingModel.LastUpdatedOn = DateTime.Now.ToUniversalTime();
            GetAllCustomFieldsResponse accountCustomFields = new GetAllCustomFieldsResponse();
            GetAllCustomFieldsRequest request = new GetAllCustomFieldsRequest(newModel.AccountID);
            accountCustomFields.CustomFields = customFieldService.GetAllCustomFields(request).CustomFields;
            if (newModel.CustomFields.IsAny())
            {
                Logger.Current.Informational("While Updating Custom fields.");
                foreach (ContactCustomFieldMapViewModel submittedField in newModel.CustomFields)
                {
                    try
                    {
                        var isCustomField = accountCustomFields.CustomFields.Where(c => c.FieldId == submittedField.CustomFieldId).FirstOrDefault();
                        if (isCustomField != null)
                        {
                            ContactCustomFieldMapViewModel contactCustomField = new ContactCustomFieldMapViewModel();
                            contactCustomField.CustomFieldId = submittedField.CustomFieldId;
                            contactCustomField.Value = submittedField.Value;
                            contactCustomField.FieldInputTypeId = (int)isCustomField.FieldInputTypeId;
                            contactCustomField.ContactId = existingModel.ContactID;
                            var existingCustomField = existingModel.CustomFields.Where(c => c.CustomFieldId == isCustomField.FieldId).FirstOrDefault();
                            if (existingCustomField == null)
                                existingModel.CustomFields.Add(contactCustomField);
                            else
                                existingCustomField.Value = submittedField.Value;
                        }
                    }
                    catch
                    {
                        Logger.Current.Informational("While Update: Submitted customfieldId: " + submittedField.CustomFieldId + " cannot be Updated. Value: " + submittedField.Value);
                    }
                }
            }

            return existingModel;
        }

    }
}
