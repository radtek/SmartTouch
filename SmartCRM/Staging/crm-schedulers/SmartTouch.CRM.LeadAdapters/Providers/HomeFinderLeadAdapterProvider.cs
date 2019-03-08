using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ImportData;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.SearchEngine.Search;
using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using LandmarkIT.Enterprise.Utilities.Logging;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using System.Xml.Serialization;
using SmartTouch.CRM.LeadAdapters.LeadStructure;
using System.Linq;
using SmartTouch.CRM.Domain.ValueObjects;
using System.Text;
using System.Web.Script.Serialization;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.CustomFields;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.Messaging.MailGun;
using System.Collections;


namespace SmartTouch.CRM.LeadAdapters.Providers
{
    public class HomeFinderLeadAdapterProvider : BaseLeadAdapterProvider
    {
        IMailGunService mailGunService;
        IContactService contactService;
        IImportDataRepository importDataRepository;
        ISearchService<Contact> searchService;
        ILeadAdaptersRepository leadAdaptersRepository;
        IServiceProviderRepository serviceProviderRepository;
        ICustomFieldService customFieldService;
        ICachingService cacheService;

        public HomeFinderLeadAdapterProvider(int accountId, int leadAdapterAndAccountMapID, ILeadAdaptersRepository leadAdaptersRepository, IServiceProviderRepository serviceProviderRepository,
            IImportDataRepository importDataRepository, ISearchService<Contact> searchService, IUnitOfWork unitOfWork,
            ICustomFieldService customFieldService, ICachingService cacheService, ICommunicationService communicationService, IMailGunService mailGunService, IContactService contactService)
            : base(accountId, leadAdapterAndAccountMapID, LeadAdapterTypes.HomeFinder, leadAdaptersRepository, importDataRepository, searchService, unitOfWork,
            customFieldService, cacheService, serviceProviderRepository, mailGunService, contactService)
        {
            this.mailGunService = mailGunService;
            this.searchService = searchService;
            this.contactService = contactService;
            this.importDataRepository = importDataRepository;
            this.leadAdaptersRepository = leadAdaptersRepository;
            this.serviceProviderRepository = serviceProviderRepository;
            this.customFieldService = customFieldService;
            this.cacheService = cacheService;
        }

        private string ProcessWebRequest(string url)
        {
            string result = string.Empty;
            try
            {
                WebRequest request = HttpWebRequest.Create(url);
                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        result = reader.ReadToEnd();
                    }
                }
            }
            catch (WebException ex)
            {
                var errorResponse = (FtpWebResponse)ex.Response;
                Logger.Current.Error("WebException : An error in process web request in Homefinder", ex);
            }
            catch (Exception ex)
            {
                result = ex.ToString();
                Logger.Current.Error("An error in process web request in Homefinder", ex);
            }
            return result;
        }

        public override ImportContactsData GetContacts(string fileName,
                                                        IEnumerable<ApplicationServices.ViewModels.FieldViewModel> Fields, int jobId, IEnumerable<ApplicationServices.ViewModels.DropdownValueViewModel> DropdownFields)
        {
            ImportContactsData data = new ImportContactsData();
            var ftpManager = new FtpService();
            Developments result;
            Func<RawContact, bool> ValidateEmail = (c) =>
            {
                /*
                 * If email is not empty or null then validate email
                 * If email is empty/or null then check for legth of first name and lastname
                 */
                if ((!string.IsNullOrEmpty(c.PrimaryEmail) && c.IsValidEmail(c.PrimaryEmail)) ||
                    (string.IsNullOrEmpty(c.PrimaryEmail) && !string.IsNullOrEmpty(c.FirstName) && !string.IsNullOrEmpty(c.LastName)))
                {
                    return true;
                }
                return false;
            };
            try
            {

                var ftpcontent = ftpManager.GetService(leadAdapterAndAccountMap.RequestGuid);
                string url = ftpcontent.Host;
                var xml = this.ProcessWebRequest(url);
                var serializer = new XmlSerializer(typeof(Developments));

                //Read Lead XML File into C# Class
                using (TextReader reader = new StringReader(xml))
                {
                    result = (Developments)serializer.Deserialize(reader);
                }

                LeadAdapterRecordStatus status = LeadAdapterRecordStatus.Undefined;
                int accountID = leadAdapterAndAccountMap.AccountID;
                var communities = result.AllLeads;

                Func<string, string> GetValue = (s) =>
                    {
                        return (string.IsNullOrEmpty(s)) ? string.Empty : s;
                    };
                /*
                 * TODO: Process Leads
                 * */
                string[] laCommunities = null;//string.IsNullOrEmpty(leadAdapterAndAccountMap.CommunityNumber) ? default(string[]) : leadAdapterAndAccountMap.CommunityNumber.Split(',');
                var builderNumber = string.IsNullOrEmpty(leadAdapterAndAccountMap.BuilderNumber) ? string.Empty : leadAdapterAndAccountMap.BuilderNumber.ToLower();
                var builderNumberPass = true;
                IList<string> hashes = new List<string>();
                var personCustomFieldData = new List<ImportCustomData>();
                var persons = new List<RawContact>();

                #region Process Community Wise
                foreach (var community in communities)
                {
                    var communityName = string.IsNullOrEmpty(community.CommunityName) ? string.Empty : community.CommunityName;
                    var communityNumberPass = false;

                    if ((laCommunities != null && laCommunities.Any(c => c.ToLower().Contains(communityName.ToLower()))) || laCommunities == null)
                    {
                        communityNumberPass = true;
                    }
                    foreach (var lead in community.leads)
                    {
                        #region Process Lead
                        var oldNewValues = new Dictionary<string, dynamic>() { };
                        var guid = Guid.NewGuid();
                        var firstName = GetValue(lead.Name);
                        var lastName = string.Empty;
                        var primaryEmail = GetValue(lead.Email);
                        var company = string.Empty;
                        StringBuilder hash = new StringBuilder();
                        bool isNotValidContact = false;
                        bool isDuplicateFromFile = false;

                        Action<string> Append = (a) =>
                            {
                                hash.Append("-").Append(a);
                            };

                        IList<Email> emails = new List<Email>();
                        if (!string.IsNullOrEmpty(primaryEmail))
                        {

                            Email primaryEmailObject = new Email()
                            {
                                EmailId = primaryEmail,
                                AccountID = leadAdapterAndAccountMap.AccountID,
                                IsPrimary = true
                            };
                            emails.Add(primaryEmailObject);
                            hash.Append("-").Append(primaryEmail);
                        }
                        else
                        {
                            hash.Append("-").Append("na@na.com");
                        }

                        Person person = new Person()
                        {
                            FirstName = firstName,
                            LastName = lastName,
                            CompanyName = company,
                            Emails = emails,
                            AccountID = accountID
                        };
                        Append(firstName);
                        Append(lastName);
                        Append(company);

                        SearchResult<Contact> duplicateResult = new SearchResult<Contact>();
                        if (!isNotValidContact && builderNumberPass && communityNumberPass)
                        {
                            bool duplicatEemailCount = hashes.Where(h => h.Contains(primaryEmail)).Any();
                            if (!string.IsNullOrEmpty(primaryEmail) && duplicatEemailCount)
                                isDuplicateFromFile = true;
                            else if (!string.IsNullOrEmpty(primaryEmail) && !duplicatEemailCount)
                                isDuplicateFromFile = false;
                            else if (string.IsNullOrEmpty(primaryEmail) && hashes.Where(h => h.Contains(hash.ToString())).Any())
                                isDuplicateFromFile = true;
                            else
                                isDuplicateFromFile = false;
                        }

                        if (!isNotValidContact && builderNumberPass && isDuplicateFromFile == false && communityNumberPass)
                        {
                            SearchParameters parameters = new SearchParameters() { AccountId = accountID };
                            IEnumerable<Contact> duplicateContacts = contactService.CheckIfDuplicate(new CheckContactDuplicateRequest() { Person = person }).Contacts;
                            duplicateResult = new SearchResult<Contact>() { Results = duplicateContacts, TotalHits = duplicateContacts != null ? duplicateContacts.Count() : 0 };
                        }

                        if (!builderNumberPass)
                            status = LeadAdapterRecordStatus.BuilderNumberFailed;
                        else if (!communityNumberPass)
                            status = LeadAdapterRecordStatus.CommunityNumberFailed;
                        else if (isNotValidContact)
                            status = LeadAdapterRecordStatus.ValidationFailed;
                        else if (isDuplicateFromFile)
                            status = LeadAdapterRecordStatus.DuplicateFromFile;
                        else if (duplicateResult.TotalHits > 0)
                        {
                            status = LeadAdapterRecordStatus.Updated;
                            //guid = duplicateResult.Results.FirstOrDefault().ReferenceId;
                        }
                        else
                            status = LeadAdapterRecordStatus.Added;

                        Contact duplicateperson = new Person() { Emails = new List<Email>() { new Email() } };
                        if (status == LeadAdapterRecordStatus.Updated)
                            duplicateperson = duplicateResult.Results.FirstOrDefault();


                        RawContact contact = new RawContact();
                        IList<ImportCustomData> contactCustomData = new List<ImportCustomData>();
                        IList<ImportPhoneData> contactPhoneData = new List<ImportPhoneData>();

                        StringBuilder CustomFieldData = new StringBuilder();

                        Action<string, string, string> AddOldNewValues = (field, oldvalue, newvalue) =>
                            {
                                if (!oldNewValues.ContainsKey(field))
                                    oldNewValues.Add(field, new { OldValue = string.IsNullOrEmpty(oldvalue) ? string.Empty : oldvalue, NewValue = newvalue });
                            };
                        try
                        {
                            ImportCustomData customData = new ImportCustomData();

                            contact.FirstName = firstName;
                            AddOldNewValues("Name", ((Person)duplicateperson).FirstName, firstName);

                            contact.PrimaryEmail = primaryEmail;
                            AddOldNewValues("Email", ((Person)duplicateperson).Emails.FirstOrDefault().EmailId, primaryEmail);

                            foreach (var propertyInfo in typeof(Lead).GetProperties())
                            {
                                #region Process Customfields
                                var oldValue = string.Empty;
                                string newValue = string.Empty;
                                string propertyName = propertyInfo.Name.Trim();
                                var customField = Fields.FirstOrDefault(f => f.Title.Replace(" ", "").ToLower().Contains(propertyInfo.Name));
                                if (customField != null)
                                {
                                    newValue = ((string)propertyInfo.GetValue(lead, null));
                                    var customfielddata = duplicateperson.CustomFields == null ? null : duplicateperson.CustomFields.Where(i => i.CustomFieldId == customField.FieldId).FirstOrDefault();
                                    if (customfielddata != null)
                                        oldValue = customfielddata.Value;

                                    customData.FieldID = customField.FieldId;
                                    customData.FieldTypeID = (int?)customField.FieldInputTypeId;
                                    customData.ReferenceID = guid;

                                    if (customField.FieldInputTypeId == FieldType.date || customField.FieldInputTypeId == FieldType.datetime || customField.FieldInputTypeId == FieldType.time)
                                    {
                                        DateTime converteddate;
                                        if (DateTime.TryParse(newValue, out converteddate))
                                        {
                                            CustomFieldData.Append("~" + customField.FieldId + "##$##" + (byte)customField.FieldInputTypeId + "|" + converteddate.ToString("MM/dd/yyyy hh:mm tt"));
                                            customData.FieldValue = converteddate.ToString("MM/dd/yyyy hh:mm tt");
                                        }
                                    }
                                    else if (customField.FieldInputTypeId == FieldType.number)
                                    {
                                        double number;
                                        if (double.TryParse(newValue, out number))
                                        {
                                            CustomFieldData.Append("~" + customField.FieldId + "##$##" + (byte)customField.FieldInputTypeId + "|" + number.ToString());
                                            customData.FieldValue = number.ToString();
                                        }
                                    }
                                    else if (customField.FieldInputTypeId == FieldType.url)
                                    {
                                        if (IsValidURL(newValue))
                                        {
                                            CustomFieldData.Append("~" + customField.FieldId + "##$##" + (byte)customField.FieldInputTypeId + "|" + newValue);
                                            customData.FieldValue = newValue;
                                        }
                                    }
                                    else
                                    {
                                        CustomFieldData.Append("~" + customField.FieldId + "##$##" + (byte)customField.FieldInputTypeId + "|" + newValue);
                                        customData.FieldValue = newValue;
                                    }
                                    contactCustomData.Add(customData);

                                    AddOldNewValues(propertyInfo.Name, oldValue, newValue);
                                }
                                #endregion
                            }
                            var brokenrules = contact.GetBrokenRules();
                            if ((brokenrules != null && brokenrules.Any()) && builderNumberPass && communityNumberPass)
                            {
                                status = LeadAdapterRecordStatus.ValidationFailed;
                            }
                            contact.ReferenceId = guid;
                            contact.AccountID = accountID;
                            contact.IsBuilderNumberPass = builderNumberPass;
                            contact.IsCommunityNumberPass = communityNumberPass;
                            contact.LeadAdapterRecordStatusId = (byte)status;
                            contact.JobID = jobId;
                            contact.ContactStatusID = 1;
                            contact.ContactTypeID = 1;
                            contact.LeadAdapterRowData = lead.ToString();
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            contact.LeadAdapterSubmittedData = js.Serialize(oldNewValues);

                        }
                        catch (Exception ex)
                        {
                            Logger.Current.Error("Error while preparing contact - Homefinder", ex);
                            ex.Data.Add("Account", accountID);
                        }
                        personCustomFieldData.AddRange(contactCustomData);

                        if (CustomFieldData.Length > 0)
                            CustomFieldData.Remove(0, 1);
                        contact.CustomFieldsData = CustomFieldData.ToString();

                        contact.ValidEmail = ValidateEmail(contact);

                        RawContact duplicate_data = null;
                        if (!String.IsNullOrEmpty(Convert.ToString(contact.PrimaryEmail)))
                            duplicate_data = persons.Where(p => string.Compare(p.PrimaryEmail, Convert.ToString(contact.PrimaryEmail), true) == 0).FirstOrDefault();
                        else
                            duplicate_data = persons.Where(p => string.Compare(p.FirstName, Convert.ToString(contact.FirstName), true) == 0 && string.Compare(p.LastName, Convert.ToString(contact.LastName), true) == 0).FirstOrDefault();

                        if (duplicate_data != null)
                            contact.IsDuplicate = true;
                        persons.Add(contact);
                        #endregion
                    }
                }
                data.ContactData = persons;
                data.ContactCustomData = personCustomFieldData;
                #endregion
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error while processing GetContacts", ex);
            }
            return data;
        }

        public override void Process()
        {
            leadAdapterAndAccountMap = leadAdaptersRepository.GetLeadAdapterByID(LeadAdapterAccountMapID);

            try
            {
                if ((leadAdapterAndAccountMap.LastProcessed.HasValue && (DateTime.UtcNow - leadAdapterAndAccountMap.LastProcessed.Value).TotalDays >= 1) || !leadAdapterAndAccountMap.LastProcessed.HasValue)
                    ProcessContacts();
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured when processing NorthStar LeadAdapter, account name : " + leadAdapterAndAccountMap.AccountName, ex);
                base.SendEmail("Exception details" + ex, "Exception while processing data from " + leadAdapterAndAccountMap.LeadAdapterTypeID.ToString() + " LeadAdapter");
            }
        }

        private void ProcessContacts()
        {
            ServiceProvider ServiceProviders = serviceProviderRepository
                  .GetServiceProviders(1, CommunicationType.Mail, MailType.TransactionalEmail);

            var response = customFieldService.GetAllCustomFields(new GetAllCustomFieldsRequest(leadAdapterAndAccountMap.AccountID));
            IEnumerable<FieldViewModel> customFields = response.CustomFields;
            var dropdownfeildsresposne = cacheService.GetDropdownValues(leadAdapterAndAccountMap.AccountID);

            IEnumerable<DropdownValueViewModel> phoneFields = dropdownfeildsresposne.Where(x => x.DropdownID == (short)DropdownFieldTypes.PhoneNumberType).Select(x => x.DropdownValuesList).FirstOrDefault();

            var jobLogId = InsertJobLog(leadAdapterAndAccountMap);
            var contacts = this.GetContacts("lead_feed.xml", customFields, jobLogId, phoneFields);
            if (contacts.ContactData != null && contacts.ContactData.Any())
            {
                Logger.Current.Informational("Got contacts for inserting : " + contacts.ContactData.Count);
                /* Bulk Insert */
                Task.Factory.StartNew(() => { MailgunVerification(contacts.ContactData.ToList()); }, TaskCreationOptions.LongRunning);
                Task.Factory.StartNew(() => { ContactsBulkinsert(contacts); }, TaskCreationOptions.LongRunning);
            }
            else
                Logger.Current.Informational("No contacts for inserting, Account name : " + leadAdapterAndAccountMap.AccountName);

            leadAdaptersRepository.UpdateProcessedDate(LeadAdapterAccountMapID);
        }

        /// <summary>
        /// Saving bulk contacts from imported file to temp table
        /// </summary>
        private void ContactsBulkinsert(ImportContactsData contacts)
        {
            importDataRepository.InsertImportsData(contacts);
            //  tagService.addLeadAdapterToTopic(leadAdapterAndAccountMap.Id, contacts, leadAdapterAndAccountMap.AccountID);
        }

        /// <summary>
        /// Mailgun verification for bulk contacts
        /// </summary>
        private void MailgunVerification(List<RawContact> contacts)
        {
            var mailGunList = new List<IEnumerable<string>>();
            var interimMailList = new List<string>();
            var mailLengthList = contacts
                                .Where(c => !string.IsNullOrEmpty(c.PrimaryEmail))
                                .Select(c => new { Email = c.PrimaryEmail, Length = (c.PrimaryEmail.Length + 1) }) //adding + 1 to consider comma in final string
                                .Distinct();

            var maxSize = 4000;
            int sum = 0; int index = 0;
            foreach (var mail in mailLengthList)
            {
                index++;
                sum = sum + mail.Length;
                if (sum >= maxSize)
                {
                    mailGunList.Add(interimMailList);
                    interimMailList = new List<string>();
                    interimMailList.Add(mail.Email);
                    sum = mail.Length;
                }
                interimMailList.Add(mail.Email);
                if (index == mailLengthList.Count())
                    mailGunList.Add(interimMailList);
            }

            foreach (var mailsToCheck in mailGunList)
            {
                var emails = string.Join(",", mailsToCheck);
                GetRestResponse response = mailGunService.BulkEmailValidate(new GetRestRequest() { Email = emails });
                JavaScriptSerializer js = new JavaScriptSerializer();
                dynamic restResponse = js.Deserialize<dynamic>(response.RestResponse.Content);

                if (restResponse != null)
                {
                    string[] valid = ((IEnumerable)restResponse["parsed"]).Cast<object>().Select(x => x.ToString()).ToArray();
                    string[] notvalid = ((IEnumerable)restResponse["unparseable"]).Cast<object>().Select(x => x.ToString()).ToArray();

                    contacts = contacts.Join(valid, c => c.PrimaryEmail, v => v, (c, v) => c).ToList();
                    foreach (RawContact contact in contacts)
                    {
                        contact.EmailStatus = (byte)EmailStatus.Verified;
                    }
                    contacts = contacts.Join(notvalid, c => c.PrimaryEmail, v => v, (c, v) => c).ToList();
                    foreach (RawContact contact in contacts)
                    {
                        contact.EmailStatus = (byte)EmailStatus.HardBounce;
                    }
                }
                else
                {
                    contacts.ForEach(p => p.EmailStatus = (byte)EmailStatus.NotVerified);
                }
            }
            importDataRepository.InsertImportContactEmailStatuses(contacts);
        }

        private int InsertJobLog(LeadAdapterAndAccountMap accountMap)
        {
            Logger.Current.Informational("Inserting joblog for account id : " + accountMap.AccountID);
            var jobLog = new LeadAdapterJobLogs();
            jobLog.LeadAdapterAndAccountMapID = accountMap.Id;
            jobLog.LeadAdapterJobStatusID = LeadAdapterJobStatus.Undefined;
            jobLog.Remarks = string.Empty;
            jobLog.FileName = "lead_feed.xml";
            List<LeadAdapterJobLogDetails> details = new List<LeadAdapterJobLogDetails>();
            jobLog.LeadAdapterJobLogDetails = details;
            return importDataRepository.InsertLeadAdapterjob(jobLog, new Guid(), false, false, accountMap.AccountID, accountMap.CreatedBy, 1, accountMap.CreatedBy,true);
        }
    }
}
